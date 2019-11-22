using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker
{
    public class QueuedTileProvider
    {
        private class QueueRequestCell : IComparable
        {
            private static int uniqueIDallocator;
            public QueueRequestIfc qr;
            public int queuedInterest;
            private int uniqueID = uniqueIDallocator++;
            public int CompareTo(object o)
            {
                QueueRequestCell queueRequestCell = (QueueRequestCell)o;
                int num = -this.queuedInterest.CompareTo(queueRequestCell.queuedInterest);
                if (num != 0)
                {
                    return num;
                }
                return this.uniqueID.CompareTo(queueRequestCell.uniqueID);
            }
            public override string ToString()
            {
                return string.Format("#{0} {1}", this.queuedInterest, this.qr);
            }
        }
        private class PriQueue
        {
            private Dictionary<QueueRequestIfc, QueueRequestCell> cellMap = new Dictionary<QueueRequestIfc, QueueRequestCell>();
            private BiSortedDictionary<QueueRequestCell, QueueRequestIfc> queue = new BiSortedDictionary<QueueRequestCell, QueueRequestIfc>();
            private ListUIIfc listUI;
            public PriQueue()
            {
                this.listUI = DiagnosticUI.theDiagnostics;
            }
            public void Clear()
            {
                this.TruncateQueue(0);
            }
            public void TruncateQueue(int targetCount)
            {
                List<QueueRequestCell> list = new List<QueueRequestCell>();
                Monitor.Enter(this);
                try
                {
                    while (this.queue.Count > targetCount)
                    {
                        QueueRequestCell lastKey = this.queue.LastKey;
                        this.cellMap.Remove(lastKey.qr);
                        this.queue.Remove(lastKey);
                        list.Add(lastKey);
                    }
                    this.UpdateDebugList();
                }
                finally
                {
                    Monitor.Exit(this);
                }
                foreach (QueueRequestCell current in list)
                {
                    current.qr.DeQueued();
                }
            }
            public void Enqueue(QueueRequestIfc qr)
            {
                Monitor.Enter(this);
                try
                {
                    QueueRequestCell queueRequestCell = new QueueRequestCell();
                    queueRequestCell.qr = qr;
                    queueRequestCell.queuedInterest = qr.GetInterest();
                    this.cellMap.Add(qr, queueRequestCell);
                    this.queue.Add(queueRequestCell, qr);
                    this.UpdateDebugList();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            public QueueRequestIfc Dequeue()
            {
                Monitor.Enter(this);
                QueueRequestIfc result;
                try
                {
                    QueueRequestCell firstKey = this.queue.FirstKey;
                    if (firstKey == null)
                    {
                        result = null;
                    }
                    else
                    {
                        this.queue.Remove(firstKey);
                        this.cellMap.Remove(firstKey.qr);
                        this.UpdateDebugList();
                        result = firstKey.qr;
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
                return result;
            }
            public void ChangePriority(QueueRequestIfc qr)
            {
                Monitor.Enter(this);
                try
                {
                    if (this.cellMap.ContainsKey(qr))
                    {
                        QueueRequestCell key = this.cellMap[qr];
                        this.queue.Remove(key);
                        this.cellMap.Remove(qr);
                        this.Enqueue(qr);
                        this.UpdateDebugList();
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            public int Count()
            {
                Monitor.Enter(this);
                int count;
                try
                {
                    D.Assert(this.cellMap.Count == this.queue.Count);
                    count = this.queue.Count;
                }
                finally
                {
                    Monitor.Exit(this);
                }
                return count;
            }
            public override string ToString()
            {
                Monitor.Enter(this);
                string result;
                try
                {
                    int num = 10;
                    string text = "";
                    foreach (QueueRequestCell current in this.queue.Keys)
                    {
                        if (num == 0)
                        {
                            text += "...\n";
                            break;
                        }
                        text = text + current.ToString() + "\n";
                        num--;
                    }
                    result = text;
                }
                finally
                {
                    Monitor.Exit(this);
                }
                return result;
            }
            public void DebugPrintQueue()
            {
                Monitor.Enter(this);
                try
                {
                    D.Sayf(0, "Queue status:", new object[0]);
                    foreach (QueueRequestCell current in this.queue)
                    {
                        D.Sayf(0, "interest {0} {1}", new object[]
                        {
                            current.queuedInterest,
                            current.qr
                        });
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            private void UpdateDebugList()
            {
                int num = 10;
                List<object> list = new List<object>();
                foreach (QueueRequestCell current in this.queue)
                {
                    list.Add(current);
                    if (list.Count >= num)
                    {
                        break;
                    }
                }
                this.listUI.listChanged(list);
            }
        }
        private static int MAX_QLEN = 2000;
        private static int QueueSizeReportPeriod = 10;
        private int numWorkerThreads;
        private string debugName;
        private PriQueue priQueue = new PriQueue();
        private EventWaitHandle queueNonemptyEvent = new CountedEventWaitHandle(false, EventResetMode.ManualReset, "QueuedTileProvider.queueNonemptyEvent");
        private int previousBucket = -1;
        public QueuedTileProvider(int numWorkerThreads, string debugName)
        {
            this.numWorkerThreads = numWorkerThreads;
            this.debugName = debugName;
            for (int i = 0; i < numWorkerThreads; i++)
            {
                DebugThreadInterrupter.theInstance.AddThread(string.Format("QueuedTileProvider {0}-{1}", debugName, i), new ThreadStart(this.workerThread), ThreadPriority.BelowNormal);
            }
        }
        public void Clear()
        {
            this.priQueue.Clear();
        }
        public void ChangePriority(QueueRequestIfc qr)
        {
            this.priQueue.ChangePriority(qr);
        }
        public void Dispose()
        {
            D.Say(1, "QTP Disposal in progress");
            PriQueue obj;
            Monitor.Enter(obj = this.priQueue);
            try
            {
                this.priQueue.Clear();
            }
            finally
            {
                Monitor.Exit(obj);
            }
            for (int i = 0; i < this.numWorkerThreads; i++)
            {
                this.enqueueTileRequest(new QueueSuicideRequest());
            }
        }
        public void cancelOutstandingRequests()
        {
            this.priQueue.Clear();
        }
        public void enqueueTileRequests(QueueRequestIfc[] qrlist)
        {
            PriQueue obj;
            Monitor.Enter(obj = this.priQueue);
            try
            {
                for (int i = 0; i < qrlist.Length; i++)
                {
                    QueueRequestIfc queueRequestIfc = qrlist[i];
                    D.Say(10, string.Format("Queueing req {0}", queueRequestIfc.ToString()));
                    this.priQueue.Enqueue(queueRequestIfc);
                }
                this.priQueue.TruncateQueue(MAX_QLEN);
                this.queueNonemptyEvent.Set();
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }
        public void enqueueTileRequest(QueueRequestIfc qr)
        {
            this.enqueueTileRequests(new QueueRequestIfc[]
            {
                qr
            });
        }
        private QueueRequestIfc Dequeue()
        {
            PriQueue obj;
            Monitor.Enter(obj = this.priQueue);
            QueueRequestIfc result;
            try
            {
                QueueRequestIfc queueRequestIfc = this.priQueue.Dequeue();
                if (queueRequestIfc == null)
                {
                    this.queueNonemptyEvent.Reset();
                }
                result = queueRequestIfc;
            }
            finally
            {
                Monitor.Exit(obj);
            }
            return result;
        }
        private void workerThread()
        {
            D.Say(10, "starting working thread");
            while (true)
            {
                D.Say(10, string.Format("worker going to sleep; queue count {0}", this.priQueue.Count()));
                this.queueNonemptyEvent.WaitOne();
                D.Say(10, "worker waking up");
                QueueRequestIfc queueRequestIfc = this.Dequeue();
                if (queueRequestIfc == null)
                {
                    D.Sayf(10, "Worker thread continuing after apparent queue cancellation; queuelen {0}", new object[]
                    {
                        this.priQueue.Count()
                    });
                }
                else
                {
                    if (queueRequestIfc is QueueSuicideRequest)
                    {
                        break;
                    }
                    D.Sayf(10, "Processing {0}", new object[]
                    {
                        queueRequestIfc
                    });
                    queueRequestIfc.DoWork();
                    D.Sayf(10, "done with {0}", new object[]
                    {
                        queueRequestIfc
                    });
                }
            }
            D.Say(3, "worker thread exiting");
        }
        public override string ToString()
        {
            return this.priQueue.ToString();
        }
        private void QueueLenStatus(string eventName)
        {
            int num = this.priQueue.Count() / QueueSizeReportPeriod;
            if (num != this.previousBucket)
            {
                this.previousBucket = num;
                D.Sayf(0, "QTP({0}).{1}: count={2}", new object[]
                {
                    this.debugName,
                    eventName,
                    this.priQueue.Count()
                });
            }
        }
    }
}
