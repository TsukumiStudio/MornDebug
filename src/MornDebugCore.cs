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
        private static int _nextId;
        private static readonly List<Entry> _entries = new();
        private static readonly MornDebugOnGUIDrawer _windowDrawer = new();
        private static readonly MornDebugOnGUIDrawer _runtimeDrawer = new();

        private sealed class Entry
        {
            public int Id;
            public string Key;
            public Action Action;
            public CancellationToken Ct;
        }

        private sealed class Registration : IDisposable
        {
            private readonly int _id;
            private bool _disposed;

            public Registration(int id)
            {
                _id = id;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Unregister(_id);
            }
        }

        static MornDebugCore()
        {
            var menus = MornDebugGlobal.I?.Menus;
            if (menus == null) return;
            foreach (var (key, action) in menus.Where(x => x != null).SelectMany(x => x.GetMenuItems()))
            {
                RegisterGUI(key, action);
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
                if (_entries[i].Ct.IsCancellationRequested)
                {
                    _entries.RemoveAt(i);
                }
            }
        }

        private static void Unregister(int id)
        {
            _entries.RemoveAll(e => e.Id == id);
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
        /// デバッグメニューにGUI描画コールバックを登録する。
        /// 同じkeyで複数登録可能。戻り値のIDisposableをDisposeすると登録解除される。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action)
        {
            var id = _nextId++;
            _entries.Add(new Entry { Id = id, Key = key, Action = action, Ct = default });
            _entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return new Registration(id);
        }

        /// <summary>
        /// CancellationTokenに連動して自動解除される登録。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, CancellationToken ct)
        {
            var id = _nextId++;
            _entries.Add(new Entry { Id = id, Key = key, Action = action, Ct = ct });
            _entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return new Registration(id);
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
    }
}
