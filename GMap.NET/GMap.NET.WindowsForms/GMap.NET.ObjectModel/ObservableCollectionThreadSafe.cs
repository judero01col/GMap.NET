using System;

namespace GMap.NET.ObjectModel
{
    public class ObservableCollectionThreadSafe<T> : ObservableCollection<T>
    {
        NotifyCollectionChangedEventHandler _collectionChanged;

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                _collectionChanged += value;
            }
            remove
            {
                _collectionChanged -= value;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (BlockReentrancy())
            {
                if (_collectionChanged != null)
                {
                    Delegate[] delegates = _collectionChanged.GetInvocationList();

                    // Walk thru invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        var dispatcherObject = handler.Target as System.Windows.Forms.Control;

                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && dispatcherObject.InvokeRequired)
                        {
                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Invoke(handler, this, e);
                        }
                        else // Execute handler as is 
                        {
                            _collectionChanged(this, e);
                        }
                    }
                }
            }
        }
    }
}
