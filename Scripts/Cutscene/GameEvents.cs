using System;
using System.Collections.Generic;

namespace Game.State
{
    /// <summary>
    /// Lightweight named-event bus. Any script can raise a named event,
    /// any ConditionMonitor set to CustomEvent can react.
    /// </summary>
    public static class GameEvents
    {
        private static readonly Dictionary<string, Action> _listeners = new();

        public static void Subscribe(string eventName, Action callback)
        {
            if (string.IsNullOrWhiteSpace(eventName)) return;
            if (!_listeners.ContainsKey(eventName))
                _listeners[eventName] = null;
            _listeners[eventName] += callback;
        }

        public static void Unsubscribe(string eventName, Action callback)
        {
            if (string.IsNullOrWhiteSpace(eventName)) return;
            if (_listeners.ContainsKey(eventName))
                _listeners[eventName] -= callback;
        }

        public static void Raise(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName)) return;
            if (_listeners.TryGetValue(eventName, out var action))
                action?.Invoke();
        }

        public static void Clear()
        {
            _listeners.Clear();
        }
    }
}