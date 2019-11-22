using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class DiskCache
    {
        private class DeferredWriteRecord : IDisposable
        {
            public Present result;
            public string freshPath;
            public string debugOriginInfo;

            public DeferredWriteRecord(Present result, string freshPath, string debugOriginInfo)
            {
                this.result = result.Duplicate("DiskCache.DeferredWriteRecord");
                this.freshPath = freshPath;
                this.debugOriginInfo = debugOriginInfo;
            }

            public void Dispose()
            {
                result.Dispose();
            }
        }

        private const long freshCountMax = 524288000L;
        private const long stableFreshCountAccuracy = 10485760L;
        private const string stableFreshCountFilename = "FreshCount.txt";
        public const string CacheControlExtension = ".cc";
        private const string FreshSide = "fresh.";
        private const string StaleSide = "stale.";
        private string cacheDir;
        private string stableFreshCountPathname;
        private bool disposed;
        private bool demoting;
        private PresentDiskDispatcher presentDiskDispatcher = new PresentDiskDispatcher();
        private long freshCount = -1L;
        private long lastStableFreshCount;

        private EventWaitHandle plowCacheEvent =
            new CountedEventWaitHandle(false, EventResetMode.AutoReset, "DiskCache.plowCacheEvent");

        private long delayedIncrementFreshCount;
        private Queue<DeferredWriteRecord> deferredWriteQueue = new Queue<DeferredWriteRecord>();

        private EventWaitHandle writeQueueNonEmptyEvent =
            new CountedEventWaitHandle(false, EventResetMode.AutoReset, "DiskCache.WriteQueueNonemptyEvent");

        private ResourceCounter resourceCounter;

        public DiskCache()
        {
            cacheDir = Path.Combine(Environment.GetEnvironmentVariable("TMP"), "mapcache\\");
            stableFreshCountPathname = Path.Combine(cacheDir, "FreshCount.txt");
            CreateCacheDirIfNeeded();
            DebugThreadInterrupter.theInstance.AddThread("DiskCache.DeferredWriteThread",
                DeferredWriteThread,
                ThreadPriority.Normal);
            DebugThreadInterrupter.theInstance.AddThread("DiskCache.EvictThread",
                EvictThread,
                ThreadPriority.Normal);
            resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("DiskCache", -1);
        }

        public void Dispose()
        {
            Monitor.Enter(this);
            try
            {
                disposed = true;
                if (plowCacheEvent != null)
                {
                    plowCacheEvent.Set();
                }

                if (writeQueueNonEmptyEvent != null)
                {
                    writeQueueNonEmptyEvent.Set();
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public Present Get(IFuture future, string refCredit)
        {
            string text = makeCachePathname(future, "fresh.");
            string text2 = makeCachePathname(future, "stale.");
            Monitor.Enter(this);
            try
            {
                long num;
                Present present = Fetch(text, out num);
                if (present != null)
                {
                    D.Sayf(10, "fresh hit! {0}", new object[] {"fresh."});
                    Present result = present;
                    return result;
                }

                present = Fetch(text2, out num);
                if (present != null)
                {
                    File.Move(text2, text);
                    IncrementFreshCount(num);
                    D.Sayf(10, "stale hit! {0} {1}", new object[] {"stale.", num});
                    Present result = present;
                    return result;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            Present result2 = future.Realize(refCredit);
            ScheduleDeferredWrite(result2, text, future.ToString());
            D.Sayf(10, "miss", new object[0]);
            return result2;
        }

        private void IncrementFreshCount(long increment)
        {
            if (demoting)
            {
                delayedIncrementFreshCount += increment;
                return;
            }

            freshCount += increment;
            if (freshCount - lastStableFreshCount > 10485760L)
            {
                UpdateStableFreshCount();
            }

            if (freshCount > 524288000L)
            {
                plowCacheEvent.Set();
            }

            resourceCounter.crement((int)increment);
        }

        private void UpdateStableFreshCount()
        {
            try
            {
                FileStream fileStream = File.Open(stableFreshCountPathname, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.Write(freshCount.ToString());
                streamWriter.Close();
                fileStream.Close();
                lastStableFreshCount = freshCount;
            }
            catch (IOException)
            {
            }
        }

        private string makeCachePathname(IFuture future, string cacheSide)
        {
            string path = cacheSide + RobustHashTools.Hash(future).ToString() + ".cc";
            return Path.Combine(cacheDir, path);
        }

        private Present Fetch(string path, out long length)
        {
            if (File.Exists(path))
            {
                try
                {
                    return presentDiskDispatcher.ReadObject(path, out length);
                }
                catch (Exception arg)
                {
                    File.Delete(path);
                    D.Say(0, string.Format("Removing corrupt file at {0}; ex {1}", path, arg));
                }
            }

            length = -1L;
            return null;
        }

        private void CreateCacheDirIfNeeded()
        {
            try
            {
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                    freshCount = 0L;
                }
                else
                {
                    try
                    {
                        FileStream fileStream = File.Open(stableFreshCountPathname, FileMode.Open);
                        StreamReader streamReader = new StreamReader(fileStream);
                        string s = streamReader.ReadToEnd();
                        fileStream.Close();
                        freshCount = long.Parse(s);
                    }
                    catch (Exception)
                    {
                        freshCount = 0L;
                    }
                }

                lastStableFreshCount = freshCount;
                freshCount += 10485760L;
            }
            catch (Exception innerException)
            {
                throw new ConfigurationException(
                    string.Format("TileCache cannot create or access cache directory {0}", cacheDir),
                    innerException);
            }
        }

        private void ScheduleDeferredWrite(Present result, string freshPath, string debugOriginInfo)
        {
            Monitor.Enter(this);
            try
            {
                DeferredWriteRecord item = new DeferredWriteRecord(result, freshPath, debugOriginInfo);
                deferredWriteQueue.Enqueue(item);
                writeQueueNonEmptyEvent.Set();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void WriteRecord(DeferredWriteRecord record)
        {
            try
            {
                long increment;
                presentDiskDispatcher.WriteObject(record.result, record.freshPath, out increment);
                Monitor.Enter(this);
                try
                {
                    IncrementFreshCount(increment);
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            catch (WriteObjectFailedException ex)
            {
                D.Sayf(1, "disk cache write failed; ignoring: {0}", new object[] {ex});
            }

            record.Dispose();
        }

        private void DeferredWriteThread()
        {
            while (!disposed)
            {
                writeQueueNonEmptyEvent.WaitOne();
                int num = 0;
                while (!disposed)
                {
                    DeferredWriteRecord deferredWriteRecord = null;
                    Monitor.Enter(this);
                    try
                    {
                        if (deferredWriteQueue.Count > 0)
                        {
                            deferredWriteRecord = deferredWriteQueue.Dequeue();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }

                    if (deferredWriteRecord == null)
                    {
                        break;
                    }

                    WriteRecord(deferredWriteRecord);
                    num++;
                }
            }

            Monitor.Enter(this);
            try
            {
                writeQueueNonEmptyEvent.Close();
                writeQueueNonEmptyEvent = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void EvictThread()
        {
            Label_0000:
            try
            {
                plowCacheEvent.WaitOne();
                lock (this)
                {
                    if (disposed)
                    {
                        lock (this)
                        {
                            plowCacheEvent.Close();
                            plowCacheEvent = null;
                        }

                        return;
                    }

                    if (freshCount <= 0x1f400000L)
                    {
                        goto Label_0000;
                    }
                }

                D.Sayf(1, "Before evict: freshCount {0} kB", new object[] {freshCount >> 10});
                EvictStaleFiles();
                EvictDemoteFreshFiles();
                D.Sayf(1, "After evict: freshCount {0} kB", new object[] {freshCount >> 10});
            }
            catch (Exception exception)
            {
                D.Sayf(1, "DiskCache.EvictThread ignores ex {0}", new object[] {exception});
            }

            goto Label_0000;
        }

        private void EvictStaleFiles()
        {
            D.Say(0, "DiskCache.EvictStaleFiles");
            Monitor.Enter(this);
            string[] files;
            try
            {
                files = Directory.GetFiles(cacheDir, "stale.*.cc");
            }
            finally
            {
                Monitor.Exit(this);
            }

            int num = 0;
            string[] array = files;
            for (int i = 0; i < array.Length; i++)
            {
                string cacheControlFilePath = array[i];
                Monitor.Enter(this);
                try
                {
                    if (disposed)
                    {
                        break;
                    }

                    try
                    {
                        try
                        {
                            presentDiskDispatcher.DeleteCacheFile(cacheControlFilePath);
                            num++;
                        }
                        catch (IOException)
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        D.Sayf(1, "DiskCache.EvictStaleFiles ignores exception {0}", new object[] {ex});
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            D.Sayf(1, "EvictStaleFiles: Examined {0} files, removed {1} files", new object[] {files.Length, num});
        }

        private void EvictDemoteFreshFiles()
        {
            D.Say(0, "DiskCache.EvictDemoteFreshFiles");
            Monitor.Enter(this);
            try
            {
                demoting = true;
            }
            finally
            {
                Monitor.Exit(this);
            }

            string[] files = Directory.GetFiles(cacheDir, "fresh.*.cc");
            int num = 0;
            string[] array = files;
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                Monitor.Enter(this);
                try
                {
                    if (disposed)
                    {
                        break;
                    }

                    try
                    {
                        string fileName = Path.GetFileName(text);
                        if (!fileName.StartsWith("fresh."))
                        {
                            D.Sayf(1, "Certainly didn't expect wildcard to return filename {0}", new object[] {text});
                        }
                        else
                        {
                            string path = fileName.Replace("fresh.", "stale.");
                            string text2 = Path.Combine(Path.GetDirectoryName(text), path);
                            File.Move(text, text2);
                            long num2 = presentDiskDispatcher.CacheFileLength(text2);
                            freshCount -= num2;
                            num++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is IOException) || !ex.ToString()
                                .StartsWith("System.IO.IOException: The process cannot access the file"))
                        {
                            D.Sayf(1, "DiskCache.EvictDemoteFreshFiles ignores exception {0}", new object[] {ex});
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            D.Sayf(1, "EvictDemoteFreshFiles: Examined {0} files, demoted {1} files", new object[] {files.Length, num});
            Monitor.Enter(this);
            try
            {
                D.Sayf(1,
                    "EvictDemoteFreshFiles: At end of pass, {0} bytes unaccounted for. Writing off.",
                    new object[] {freshCount});
                freshCount = 0L;
                demoting = false;
                IncrementFreshCount(delayedIncrementFreshCount);
                delayedIncrementFreshCount = 0L;
                UpdateStableFreshCount();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
