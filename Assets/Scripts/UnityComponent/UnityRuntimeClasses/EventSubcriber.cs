using System;

namespace RiichiReign.UnityComponent
{
    public class EventSubscription : IDisposable
    {
        private Action _handler;
        private Action<Action> _unsubscribe;

        public EventSubscription(Action handler, Action<Action> unsubscribe)
        {
            _handler = handler;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke(_handler);
            _unsubscribe = null;
            _handler = null;
        }

        public Action Handler => _handler;
    }

    public class EventSubscription<T> : IDisposable
    {
        private Action<T> _handler;
        private Action<Action<T>> _unsubscribe;

        public EventSubscription(Action<T> handler, Action<Action<T>> unsubscribe)
        {
            _handler = handler;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke(_handler);
            _unsubscribe = null;
            _handler = null;
        }

        public Action<T> Handler => _handler;
    }

    public class EventSubscription<T1, T2> : IDisposable
    {
        private Action<T1, T2> _handler;
        private Action<Action<T1, T2>> _unsubscribe;

        public EventSubscription(Action<T1, T2> handler, Action<Action<T1, T2>> unsubscribe)
        {
            _handler = handler;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke(_handler);
            _unsubscribe = null;
            _handler = null;
        }

        public Action<T1, T2> Handler => _handler;
    }
}
