using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MornLib
{
    public static class MornDebugCore
    {
        private static int _updateFrameCount;
        private static readonly List<MornDebugEntry> _entries = new();
        private static readonly MornDebugOnGUIDrawer _windowDrawer = new();
        private static readonly MornDebugOnGUIDrawer _runtimeDrawer = new();

        static MornDebugCore()
        {
            var menus = MornDebugGlobal.I?.Menus;
            if (menus == null) return;
            foreach (var (key, action) in menus.Where(x => x != null).SelectMany(x => x.GetMenuItems()))
            {
                RegisterGUI(key, action, CancellationToken.None);
            }
        }

        private static void Cleanup()
        {
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].IsInvalid)
                {
                    _entries.RemoveAt(i);
                }
            }
        }

        internal static void OnUpdate()
        {
            if (Time.frameCount == _updateFrameCount) return;
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
            Cleanup();
            var drawer = isWindow ? _windowDrawer : _runtimeDrawer;
            drawer.OnGUI(_entries);
        }

        /// <summary>
        /// CancellationTokenに連動して自動解除される登録。同じkeyで複数登録可能。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, CancellationToken ct)
        {
            var entry = new MornDebugEntry(key, action, ct);
            _entries.Add(entry);
            _entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            return entry;
        }

        /// <summary>
        /// GameObjectのライフサイクルに連動して自動解除される登録。
        /// </summary>
        public static IDisposable RegisterGUI(string key, Action action, GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<MornDebugDestroyTrigger>(out var trigger))
            {
                trigger = gameObject.AddComponent<MornDebugDestroyTrigger>();
            }

            return RegisterGUI(key, action, trigger.CancellationToken);
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
            return RegisterGUI(key, action, CancellationToken.None);
        }
    }
}
