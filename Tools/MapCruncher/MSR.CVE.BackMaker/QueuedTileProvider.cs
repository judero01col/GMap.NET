using System;
using System.Collections.Generic;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

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
                int num = -queuedInterest.CompareTo(queueRequestCell.queuedInterest);
                if (num != 0)
                {
                    return num;
                }

                return uniqueID.CompareTo(queueRequestCell.uniqueID);
            }

            public override string ToString()
            {
                return string.Format("#{0} {1}", queuedInterest, qr);
            }
        }

        private class PriQueue
        {
            private Dictionary<QueueRequestIfc, QueueRequestCell> cellMap =
                new Dictionary<QueueRequestIfc, QueueRequestCell>();

            private BiSortedDictionary<QueueRequestCell, QueueRequestIfc> queue =
                new BiSortedDictionary<QueueRequestCell, QueueRequestIfc>();

            private ListUIIfc listUI;

            public PriQueue()
            {
                listUI = DiagnosticUI.theDiagnostics;
            }

            public void Clear()
            {
                TruncateQueue(0);
            }

            public void TruncateQueue(int targetCount)
            {
                List<QueueRequestCell> list = new List<QueueRequestCell>();
                Monitor.Enter(this);
                try
                {
                    while (queue.Count > targetCount)
                    {
                        QueueRequestCell lastKey = queue.LastKey;
                        cellMap.Remove(lastKey.qr);
                        queue.Remove(lastKey);
                        list.Add(lastKey);
                    }

                    UpdateDebugList();
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
                    cellMap.Add(qr, queueRequestCell);
                    queue.Add(queueRequestCell, qr);
                    UpdateDebugList();
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
                    QueueRequestCell firstKey = queue.FirstKey;
                    if (firstKey == null)
                    {
                        result = null;
                    }
                    else
                    {
                        queue.Remove(firstKey);
                        cellMap.Remove(firstKey.qr);
                        UpdateDebugList();
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
                    if (cellMap.ContainsKey(qr))
                    {
                        QueueRequestCell key = cellMap[qr];
                        queue.Remove(key);
                        cellMap.Remove(qr);
                        Enqueue(qr);
                        UpdateDebugList();
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
                    D.Assert(cellMap.Count == queue.Count);
                    count = queue.Count;
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
                    foreach (QueueRequestCell current in queue.Keys)
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
                    foreach (QueueRequestCell current in queue)
                    {
                        D.Sayf(0, "interest {0} {1}", new object[] {current.queuedInterest, current.qr});
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
                foreach (QueueRequestCell current in queue)
                {
                    list.Add(current);
                    if (list.Count >= num)
                    {
                        break;
                    }
                }

                listUI.listChanged(list);
            }
        }

        private static int MAX_QLEN = 2000;
        private static int QueueSizeReportPeriod = 10;
        private int numWorkerThreads;
        private string debugName;
        private PriQueue priQueue = new PriQueue();

        private EventWaitHandle queueNonemptyEvent = new CountedEventWaitHandle(false,
            EventResetMode.ManualReset,
            "QueuedTileProvider.queueNonemptyEvent");

        private int previousBucket = -1;

        public QueuedTileProvider(int numWorkerThreads, string debugName)
        {
            this.numWorkerThreads = numWorkerThreads;
            this.debugName = debugName;
            for (int i = 0; i < numWorkerThreads; i++)
            {
                DebugThreadInterrupter.theInstance.AddThread(string.Format("QueuedTileProvider {0}-{1}", debugName, i),
                    workerThread,
                    ThreadPriority.BelowNormal);
            }
        }

        public void Clear()
        {
            priQueue.Clear();
        }

        public void ChangePriority(QueueRequestIfc qr)
        {
            priQueue.ChangePriority(qr);
        }

        public void Dispose()
        {
            D.Say(1, "QTP Disposal in progress");
            PriQueue obj;
            Monitor.Enter(obj = priQueue);
            try
            {
                priQueue.Clear();
            }
            finally
            {
                Monitor.Exit(obj);
            }

            for (int i = 0; i < numWorkerThreads; i++)
            {
                enqueueTileRequest(new QueueSuicideRequest());
            }
        }

        public void cancelOutstandingRequests()
        {
            priQueue.Clear();
        }

        public void enqueueTileRequests(QueueRequestIfc[] qrlist)
        {
            PriQueue obj;
            Monitor.Enter(obj = priQueue);
            try
            {
                for (int i = 0; i < qrlist.Length; i++)
                {
                    QueueRequestIfc queueRequestIfc = qrlist[i];
                    D.Say(10, string.Format("Queueing req {0}", queueRequestIfc.ToString()));
                    priQueue.Enqueue(queueRequestIfc);
                }

                priQueue.TruncateQueue(MAX_QLEN);
                queueNonemptyEvent.Set();
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void enqueueTileRequest(QueueRequestIfc qr)
        {
            enqueueTileRequests(new[] {qr});
        }

        private QueueRequestIfc Dequeue()
        {
            PriQueue obj;
            Monitor.Enter(obj = priQueue);
            QueueRequestIfc result;
            try
            {
                QueueRequestIfc queueRequestIfc = priQueue.Dequeue();
                if (queueRequestIfc == null)
                {
                    queueNonemptyEvent.Reset();
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
                D.Say(10, string.Format("worker going to sleep; queue count {0}", priQueue.Count()));
                queueNonemptyEvent.WaitOne();
                D.Say(10, "worker waking up");
                QueueRequestIfc queueRequestIfc = Dequeue();
                if (queueRequestIfc == null)
                {
                    D.Sayf(10,
                        "Worker thread continuing after apparent queue cancellation; queuelen {0}",
                        new object[] {priQueue.Count()});
                }
                else
                {
                    if (queueRequestIfc is QueueSuicideRequest)
                    {
                        break;
                    }

                    D.Sayf(10, "Processing {0}", new object[] {queueRequestIfc});
                    queueRequestIfc.DoWork();
                    D.Sayf(10, "done with {0}", new object[] {queueRequestIfc});
                }
            }

            D.Say(3, "worker thread exiting");
        }

        public override string ToString()
        {
            return priQueue.ToString();
        }

        private void QueueLenStatus(string eventName)
        {
            int num = priQueue.Count() / QueueSizeReportPeriod;
            if (num != previousBucket)
            {
                previousBucket = num;
                D.Sayf(0, "QTP({0}).{1}: count={2}", new object[] {debugName, eventName, priQueue.Count()});
            }
        }
    }
}
