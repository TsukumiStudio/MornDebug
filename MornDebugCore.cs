using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MornDebug
{
    public static class MornDebugCore
    {
        private static int _updateFrameCount;
        private static readonly List<string> _menuKeys = new();
        private static readonly Dictionary<string, (Action, CancellationToken)> _menuItems = new();
        private static readonly MornDebugOnGUIDrawer _windowDrawer = new();
        private static readonly MornDebugOnGUIDrawer _runtimeDrawer = new();

        static MornDebugCore()
        {
            foreach (var (key, action) in MornDebugGlobal.I.Menus.SelectMany(menu => menu.GetMenuItems()))
            {
                RegisterGUI(key, action);
            }
        }

        private static IEnumerable<(string, Action)> GetValues()
        {
            foreach (var key in _menuKeys)
            {
                var pair = _menuItems[key];
                yield return (key, pair.Item1);
            }
        }

        private static void CheckCancellation()
        {
            var cancelList = new List<string>();
            foreach (var item in _menuItems)
            {
                var ct = item.Value.Item2;
                if (ct.IsCancellationRequested)
                {
                    cancelList.Add(item.Key);
                }
            }

            foreach (var key in cancelList)
            {
                UnregisterGUI(key);
            }
        }

        internal static void OnUpdate()
        {
            if (Time.frameCount == _updateFrameCount)
            {
                return; // 既に更新済み
            }

            _updateFrameCount = Time.frameCount;
            foreach (var menu in MornDebugGlobal.I.Menus)
            {
                menu.OnUpdate();
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

        public static void RegisterGUI(string key, Action action, CancellationToken ct = default)
        {
            if (_menuItems.ContainsKey(key))
            {
                MornDebugGlobal.LogWarning($"キーが重複しているので登録処理をスキップします:{key}");
                return;
            }

            _menuItems[key] = (action, ct);
            _menuKeys.Add(key);
            _menuKeys.Sort();
        }

        public static void UnregisterGUI(string key)
        {
            if (!_menuItems.ContainsKey(key))
            {
                MornDebugGlobal.LogWarning($"キーが見つからないので削除処理をスキップします:{key}");
                return;
            }

            _menuItems.Remove(key);
            _menuKeys.Remove(key);
        }
    }
}