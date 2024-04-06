using System;

namespace pryLogger.src.Db.ConnectionManager
{
    public interface IObservable
    {
        void ClearListeners();

        void Notify(params object[] keys);

        void RemoveListener(params object[] keys);

        void AddListener(object key, Action action);
    }
}
