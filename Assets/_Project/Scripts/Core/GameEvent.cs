using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restless.Core
{
    [CreateAssetMenu(menuName = "Restless/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<Action> _listeners = new();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke();
        }

        public void Subscribe(Action listener) => _listeners.Add(listener);
        public void Unsubscribe(Action listener) => _listeners.Remove(listener);
    }

    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<Action<T>> _listeners = new();

        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke(value);
        }

        public void Subscribe(Action<T> listener) => _listeners.Add(listener);
        public void Unsubscribe(Action<T> listener) => _listeners.Remove(listener);
    }
}
