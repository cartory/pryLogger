using System;
using System.Linq;
using System.Collections.Generic;

namespace pryLogger.src.Db.ConnectionManager
{
    public interface IObservable
    {
        void ClearListeners();
        void Notify(params object[] keys);
        void RemoveListener(params object[] keys);
        void AddListener(object key, Action action);
    }

    public class ObservableList<T> : List<T>, IObservable
    {
        private readonly Dictionary<object, Action> Listeners = new Dictionary<object, Action>();

        public ObservableList() : base() { }
        public ObservableList(int capacity) : base(capacity) { }
        public ObservableList(IEnumerable<T> collection) : base(collection) { }

        public void ClearListeners() => Listeners.Clear();

        public void AddListener(object key, Action action)
        {
            if (!Listeners.ContainsKey(key))
            {
                Listeners.Add(key, action);
            }
        }

        public void RemoveListener(params object[] keys)
        {
            foreach (var key in keys.Where(key => Listeners.ContainsKey(key)))
            {
                Listeners.Remove(key);
            }
        }

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
