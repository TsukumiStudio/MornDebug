using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
    public static class MornDebugCore
    {
        private static int _updateFrameCount;
        private static readonly List<Entry> _entries = new();
        private static readonly MornDebugOnGUIDrawer _windowDrawer = new();
        private static readonly MornDebugOnGUIDrawer _runtimeDrawer = new();

        private sealed class Entry
        {
            public string Key;
            public Action Action;
            public CancellationToken Ct;
            public bool Disposed;
        }

        private sealed class Registration : IDisposable
        {
            private readonly Entry _entry;

            public Registration(Entry entry)
            {
                _entry = entry;
            }

            public void Dispose()
            {
                _entry.Disposed = true;
            }
        }

        static MornDebugCore()
        {
            var menus = MornDebugGlobal.I?.Menus;
            if (menus == null) return;
            foreach (var (key, action) in menus.Where(x => x != null).SelectMany(x => x.GetMenuItems()))
            {
                RegisterGUI(key, action, CancellationToken.None);
            }
        }

        private static IEnumerable<(string, Action)> GetValues()
        {
            foreach (var entry in _entries)
            {
                yield return (entry.Key, entry.Action);
            }
        }

        private static void CheckCancellation()
        {
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Disposed || _entries[i].Ct.IsCancellationRequested)
                {
                    _entries.RemoveAt(i);
                }
            }
        }

        internal static void OnUpdate()
        {
            if (Time.frameCount == _updateFrameCount)
            {
                return;
            }

            _updateFrameCount = Time.frameCount;
            var menus = MornDebugGlobal.I?.Menus;
            if (menus == null) return;
            foreach (var menu in menus)
            {
                menu?.OnUpdate();
            }
        }

        internal static void OnGUI(bool isWindow)
        {
            CheckCancellation();
            if (isWindow)
            {
                _windowDrawer.OnGUI(GetValues());
            }
            else
            {
                _runtimeDrawer.OnGUI(GetValues());
            }
        }

        /// <summary>
        /// CancellationTokenに連動して自動解除される登録。同じkeyで複数登録可能。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, CancellationToken ct)
        {
            var entry = new Entry { Key = key, Action = action, Ct = ct };
            _entries.Add(entry);
            _entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return new Registration(entry);
        }

        /// <summary>
        /// GameObjectのライフサイクルに連動して自動解除される登録。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, GameObject gameObject)
        {
            return RegisterGUI(key, action, gameObject.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// MonoBehaviourのライフサイクルに連動して自動解除される登録。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, MonoBehaviour monoBehaviour)
        {
            return RegisterGUI(key, action, monoBehaviour.destroyCancellationToken);
        }

        /// <summary>
        /// 戻り値のIDisposableをDisposeすると登録解除される。任意タイミングで解除可能。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action)
        {
            var entry = new Entry { Key = key, Action = action, Ct = CancellationToken.None };
            _entries.Add(entry);
            _entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return new Registration(entry);
        }
    }
}
