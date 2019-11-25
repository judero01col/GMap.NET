using System.Collections.Generic;
using System.Threading;

namespace MSR.CVE.BackMaker
{
    public class DebugThreadInterrupter
    {
        private class ThreadRec
        {
            public Thread thread
            {
                get;
            }

            public ThreadRec(Thread thread)
            {
                this.thread = thread;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", thread.ManagedThreadId, thread.Name);
            }
        }

        public class ThreadWrapper
        {
            private ThreadStart threadStart;
            private DebugThreadInterrupter debugThreadInterrupter;

            public ThreadWrapper(ThreadStart threadStart, DebugThreadInterrupter debugThreadInterrupter)
            {
                this.threadStart = threadStart;
                this.debugThreadInterrupter = debugThreadInterrupter;
            }

            public void DoWork()
            {
                threadStart();
                debugThreadInterrupter.UnregisterThread(Thread.CurrentThread);
            }
        }

        private SortedDictionary<int, ThreadRec> threadDict = new SortedDictionary<int, ThreadRec>();
        private bool quitFlag;

        private EventWaitHandle quitEvent =
            new EventWaitHandle(false, EventResetMode.AutoReset, "DebugThreadInterrupter");

        public static DebugThreadInterrupter theInstance = new DebugThreadInterrupter();

        public DebugThreadInterrupter()
        {
            AddThread("DebugThreadInterrupter", DoWork, ThreadPriority.Normal);
        }

        public void AddThread(string name, ThreadStart start, ThreadPriority priority)
        {
            ThreadWrapper @object = new ThreadWrapper(start, this);
            Thread thread = new Thread(@object.DoWork);
            thread.Priority = priority;
            thread.Name = name;
            RegisterThread(thread);
            thread.Start();
            D.Sayf(1, "Started thread {0}", new object[] {name});
        }

        public void Quit()
        {
            quitFlag = true;
            quitEvent.Set();
        }

        private void DoWork()
        {
            while (true)
            {
                int num = -1;
                quitEvent.WaitOne(1000, false);
                if (quitFlag)
                {
                    break;
                }

                ThreadRec threadRec = null;
                Monitor.Enter(this);
                try
                {
                    if (num >= 0)
                    {
                        threadRec = threadDict[num];
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }

                if (threadRec != null)
                {
                    threadRec.thread.Interrupt();
                }
            }
        }

        internal void RegisterThread(Thread thread)
        {
            Monitor.Enter(this);
            try
            {
                threadDict[thread.ManagedThreadId] = new ThreadRec(thread);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal void UnregisterThread(Thread thread)
        {
            Monitor.Enter(this);
            ThreadRec threadRec;
            try
            {
                threadRec = threadDict[thread.ManagedThreadId];
                threadDict.Remove(thread.ManagedThreadId);
            }
            finally
            {
                Monitor.Exit(this);
            }

            D.Sayf(1, "Exiting thread {0}", new object[] {threadRec});
        }
    }
}
