using System;
using System.Linq;
using System.Collections.Generic;

namespace pryLogger.src.Db.ConnectionManager
{
    /// <summary>
    /// Interface for an object that can be observed by listeners.
    /// </summary>
    public interface IObservable
    {
        /// <summary>
        /// Clears all registered listeners.
        /// </summary>
        void ClearListeners();

        /// <summary>
        /// Notifies listeners associated with specified keys.
        /// </summary>
        /// <param name="keys">The keys associated with the listeners to notify.</param>
        void Notify(params object[] keys);

        /// <summary>
        /// Removes listeners associated with specified keys.
        /// </summary>
        /// <param name="keys">The keys associated with the listeners to remove.</param>
        void RemoveListener(params object[] keys);

        /// <summary>
        /// Adds a listener associated with a key.
        /// </summary>
        /// <param name="key">The key associated with the listener.</param>
        /// <param name="action">The action to be invoked when the listener is notified.</param>
        void AddListener(object key, Action action);
    }

    /// <summary>
    /// A generic list that implements the IObservable interface for observer pattern support.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class ObservableList<T> : List<T>, IObservable
    {
        private readonly Dictionary<object, Action> Listeners = new Dictionary<object, Action>();

        /// <summary>
        /// Initializes a new instance of the ObservableList class.
        /// </summary>
        public ObservableList() : base() { }

        /// <summary>
        /// Initializes a new instance of the ObservableList class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        public ObservableList(int capacity) : base(capacity) { }

        /// <summary>
        /// Initializes a new instance of the ObservableList class with elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public ObservableList(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Clears all registered listeners.
        /// </summary>
        public void ClearListeners() => Listeners.Clear();

        /// <summary>
        /// Adds a listener associated with a key.
        /// </summary>
        /// <param name="key">The key associated with the listener.</param>
        /// <param name="action">The action to be invoked when the listener is notified.</param>
        public void AddListener(object key, Action action)
        {
            if (!Listeners.ContainsKey(key))
            {
                Listeners.Add(key, action);
            }
        }

        /// <summary>
        /// Removes listeners associated with specified keys.
        /// </summary>
        /// <param name="keys">The keys associated with the listeners to remove.</param>
        public void RemoveListener(params object[] keys)
        {
            foreach (var key in keys.Where(key => Listeners.ContainsKey(key)))
            {
                Listeners.Remove(key);
            }
        }

        /// <summary>
        /// Notifies listeners associated with specified keys or all registered listeners if no keys are specified.
        /// </summary>
        /// <param name="keys">The keys associated with the listeners to notify.</param>
        public void Notify(params object[] keys)
        {
            if (keys.Length == 0)
            {
                foreach (object key in Listeners.Keys)
                {
                    Listeners[key]();
                }
            }

            foreach (var key in keys.Where(key => Listeners.ContainsKey(key)))
            {
                Listeners[key]();
            }
        }
    }
}
