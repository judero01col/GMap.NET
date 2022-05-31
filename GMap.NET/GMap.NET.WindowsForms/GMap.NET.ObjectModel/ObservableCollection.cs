using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GMap.NET.ObjectModel
{
    public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);

    public interface INotifyCollectionChanged
    {
        // Events
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    public interface INotifyPropertyChanged
    {
        // Events
        event PropertyChangedEventHandler PropertyChanged;
    }

    public enum NotifyCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Move,
        Reset
    }

    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        // Fields
        private NotifyCollectionChangedAction _action;
        private IList _newItems;
        private int _newStartingIndex;
        private IList _oldItems;
        private int _oldStartingIndex;

        // Methods
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            InitializeAdd(action, null, -1);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
            }

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                }

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }

                InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
            }

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                }

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new[] { changedItem }, -1);
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }

            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }

            InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems,
            int startingIndex)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
            }

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                }

                if (startingIndex != -1)
                {
                    throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");
                }

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }

                if (startingIndex < -1)
                {
                    throw new ArgumentException("IndexCannotBeNegative", "startingIndex");
                }

                InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
            }

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem", "action");
                }

                if (index != -1)
                {
                    throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");
                }

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new[] { changedItem }, index);
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            InitializeMoveOrReplace(action, new[] { newItem }, new[] { oldItem }, -1, -1);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems,
            int startingIndex)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }

            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }

            InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index,
            int oldIndex)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Move)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            if (index < 0)
            {
                throw new ArgumentException("IndexCannotBeNegative", "index");
            }

            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index,
            int oldIndex)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Move)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            if (index < 0)
            {
                throw new ArgumentException("IndexCannotBeNegative", "index");
            }

            object[] newItems = new[] { changedItem };
            InitializeMoveOrReplace(action, newItems, newItems, index, oldIndex);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem,
            int index)
        {
            _newStartingIndex = -1;
            _oldStartingIndex = -1;
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor", "action");
            }

            InitializeMoveOrReplace(action, new[] { newItem }, new[] { oldItem }, index, index);
        }

        private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            _action = action;
            _newItems = newItems == null ? null : ArrayList.ReadOnly(newItems);
            _newStartingIndex = newStartingIndex;
        }

        private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                InitializeAdd(action, changedItems, startingIndex);
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                InitializeRemove(action, changedItems, startingIndex);
            }
            else
            {
                throw new ArgumentException(string.Format("InvariantFailure, Unsupported action: {0}",
                    action.ToString()));
            }
        }

        private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems,
            int startingIndex, int oldStartingIndex)
        {
            InitializeAdd(action, newItems, startingIndex);
            InitializeRemove(action, oldItems, oldStartingIndex);
        }

        private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            _action = action;
            _oldItems = oldItems == null ? null : ArrayList.ReadOnly(oldItems);
            _oldStartingIndex = oldStartingIndex;
        }

        // Properties
        public NotifyCollectionChangedAction Action
        {
            get
            {
                return _action;
            }
        }

        public IList NewItems
        {
            get
            {
                return _newItems;
            }
        }

        public int NewStartingIndex
        {
            get
            {
                return _newStartingIndex;
            }
        }

        public IList OldItems
        {
            get
            {
                return _oldItems;
            }
        }

        public int OldStartingIndex
        {
            get
            {
                return _oldStartingIndex;
            }
        }
    }

    [Serializable]
    public class ObservableCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        // Fields
        protected Collection<T> _inner;

        protected object _lock = new object();

        private SimpleMonitor _monitor;
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        // Events
        [field: NonSerialized] public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        [field: NonSerialized] protected event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                    return _inner.Count;
            }
        }

        public bool IsReadOnly => false;


        // Methods
        public ObservableCollection()
        {
            _monitor = new SimpleMonitor();
            _inner = new Collection<T>();
        }

        public ObservableCollection(IEnumerable<T> collection)
        {
            _monitor = new SimpleMonitor();
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            _inner = new Collection<T>();

            CopyFrom(collection);
        }

        public ObservableCollection(List<T> list)
        {
            _monitor = new SimpleMonitor();
            _inner = new Collection<T>(list != null ? new List<T>(list.Count) : list);

            CopyFrom(list);
        }

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void CheckReentrancy()
        {
            if (_monitor.Busy && CollectionChanged != null &&
                CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                    return _inner[index];
            }
            set
            {
                SetItem(index, value);
            }
        }

        protected void ClearItems()
        {
            lock (_lock)
            {
                CheckReentrancy();
                _inner.Clear();
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionReset();
            }
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            if (collection != null)
            {
                lock (_lock)
                {
                    using (var enumerator = collection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            AddItem(enumerator.Current);
                        }
                    }
                }
            }
        }

        protected void AddItem(T item)
        {
            lock (_lock)
            {
                CheckReentrancy();
                _inner.Add(item);
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _inner.Count - 1);
            }
        }
        protected void InsertItem(int index, T item)
        {
            lock (_lock)
            {
                CheckReentrancy();
                _inner.Insert(index, item);
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            lock (_lock)
            {
                CheckReentrancy();
                var item = _inner[oldIndex];
                _inner.RemoveAt(oldIndex);
                _inner.Insert(newIndex, item);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
            }
        }
        protected void RemoveItem(int index)
        {
            lock (_lock)
            {
                CheckReentrancy();
                var item = _inner[index];
                _inner.RemoveAt(index);
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
        }

        protected void SetItem(int index, T item)
        {
            lock (_lock)
            {
                CheckReentrancy();
                var oldItem = _inner[index];
                _inner[index] = item;
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem,
            int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


        public void Add(T item)
        {
            AddItem(item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(T item)
        {
            lock (_lock)
                return _inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
                _inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            RemoveItem(_inner.IndexOf(item));
            return true;
        }

        public void RemoveAt(int index) => RemoveItem(index);

        public void Insert(int index, T item)
        {
            InsertItem(index, item);
        }

        public int IndexOf(T item)
        {
            lock (_lock)
                return _inner.IndexOf(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // instead of returning an usafe enumerator,
            // we wrap it into our thread-safe class
            lock (_lock)
                return new ThreadSafeEnumerator<T>(_inner.GetEnumerator(), _lock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {                         
            return GetEnumerator();
        }

        // Nested Types
        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            // Fields
            private int _busyCount;

            // Methods
            public void Dispose()
            {
                _busyCount--;
            }

            public void Enter()
            {
                _busyCount++;
            }

            // Properties
            public bool Busy
            {
                get
                {
                    return _busyCount > 0;
                }
            }
        }
    }
}
