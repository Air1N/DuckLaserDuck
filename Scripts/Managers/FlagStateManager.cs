using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.State
{
    /// <summary>
    /// Central flag storage shared by dialogue, cutscenes, and gameplay triggers.
    /// Persists to PlayerPrefs. Fires events on every change so listeners can react
    /// without polling.
    /// </summary>
    public static class FlagStateManager
    {
        // Same key DialogueState used — existing saves load automatically.
        private const string PrefsKey = "Game_CompletedDialogueSequences";
        private const char Delimiter = '|';

        private static HashSet<string> _flags;
        private static HashSet<string> Flags
        {
            get
            {
                if (_flags == null) Load();
                return _flags;
            }
        }

        /* ── Events ─────────────────────────────────────────── */

        /// <summary>Fired after a flag is newly set (not fired if already set).</summary>
        public static event Action<string> OnFlagSet;

        /// <summary>Fired after a flag is removed (not fired if wasn't set).</summary>
        public static event Action<string> OnFlagCleared;

        /// <summary>Fired after <see cref="Reset"/> wipes everything.</summary>
        public static event Action OnAllCleared;

        /* ── Core API ───────────────────────────────────────── */

        public static void Set(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (Flags.Add(id))
            {
                Save();
                OnFlagSet?.Invoke(id);
            }
        }

        public static void Clear(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (Flags.Remove(id))
            {
                Save();
                OnFlagCleared?.Invoke(id);
            }
        }

        public static bool IsSet(string id)
            => !string.IsNullOrWhiteSpace(id) && Flags.Contains(id);

        public static bool AllSet(params string[] ids)
        {
            if (ids == null || ids.Length == 0) return true;
            foreach (var id in ids)
                if (!string.IsNullOrWhiteSpace(id) && !Flags.Contains(id))
                    return false;
            return true;
        }

        public static bool AnySet(params string[] ids)
        {
            if (ids == null || ids.Length == 0) return false;
            foreach (var id in ids)
                if (!string.IsNullOrWhiteSpace(id) && Flags.Contains(id))
                    return true;
            return false;
        }

        public static void Reset()
        {
            Flags.Clear();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
            OnAllCleared?.Invoke();
        }

        public static IReadOnlyCollection<string> GetAll() => Flags;

        /* ── Persistence ────────────────────────────────────── */

        private static void Save()
        {
            PlayerPrefs.SetString(PrefsKey, string.Join(Delimiter.ToString(), Flags));
            PlayerPrefs.Save();
        }

        private static void Load()
        {
            _flags = new HashSet<string>();
            string raw = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (string.IsNullOrEmpty(raw)) return;
            foreach (string id in raw.Split(Delimiter))
                if (!string.IsNullOrWhiteSpace(id))
                    _flags.Add(id);
        }
    }
}