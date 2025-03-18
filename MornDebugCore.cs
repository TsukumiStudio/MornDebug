using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MornDebug
{
    public static class MornDebugCore
    {
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
            foreach (var pair in _menuItems)
            {
                yield return (pair.Key, pair.Value.Item1, pair.Value.Item2);
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
            _menuItems[key] = (action, ct);
        }

        public static void UnregisterGUI(string key)
        {
            _menuItems.Remove(key);
        }
    }
}