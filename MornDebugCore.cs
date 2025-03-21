using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MornDebug
{
    public static class MornDebugCore
    {
        private static readonly List<string> _menuKeys = new();
        private static readonly Dictionary<string, (Action, CancellationToken)> _menuItems = new();

        static MornDebugCore()
        {
            foreach (var (key, action) in MornDebugGlobal.I.Menus.SelectMany(menu => menu.GetMenuItems()))
            {
                RegisterGUI(key, action);
            }
        }

        internal static IEnumerable<(string, Action, CancellationToken)> GetValues()
        {
            foreach (var key in _menuKeys)
            {
                var pair = _menuItems[key];
                yield return (key, pair.Item1, pair.Item2);
            }
        }

        internal static void CheckCancellation()
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