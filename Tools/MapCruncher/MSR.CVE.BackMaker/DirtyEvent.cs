using System;

namespace MSR.CVE.BackMaker
{
    public class DirtyEvent
    {
        private DirtyListener eventList;

        public DirtyEvent()
        {
        }

        public DirtyEvent(DirtyEvent parentEvent)
        {
            Add(parentEvent);
        }

        public void SetDirty()
        {
            if (eventList != null)
            {
                eventList();
            }
        }

        public void Add(DirtyListener listener)
        {
            eventList = (DirtyListener)Delegate.Combine(eventList, listener);
        }

        public void Add(DirtyEvent listener)
        {
            Add(listener.SetDirty);
        }

        public void Remove(DirtyListener listener)
        {
            eventList = (DirtyListener)Delegate.Remove(eventList, listener);
        }

        public void Remove(DirtyEvent listener)
        {
            Remove(listener.SetDirty);
        }
    }
}
