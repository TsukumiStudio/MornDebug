using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MornDebug
{
    public static class MornDebugCore
    {
        private static Vector2 _scrollPosition;
        private static string _currentPath;
        private static readonly List<string> _menuKeys = new();
        private static readonly Dictionary<string, (Action, CancellationToken)> _menuItems = new();

        static MornDebugCore()
        {
            foreach (var (key, action) in MornDebugGlobal.I.Menus.SelectMany(menu => menu.GetMenuItems()))
            {
                RegisterGUI(key, action);
            }
        }

        private static IEnumerable<(string, Action, CancellationToken)> GetValues()
        {
            foreach (var key in _menuKeys)
            {
                var pair = _menuItems[key];
                yield return (key, pair.Item1, pair.Item2);
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
            foreach (var menu in MornDebugGlobal.I.Menus)
            {
                menu.OnUpdate();
            }
        }

        public static void OnGUI()
        {
            CheckCancellation();
            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                using (new GUILayout.HorizontalScope())
                {
                    var canBack = !string.IsNullOrEmpty(_currentPath) && _currentPath.Length > 0;
                    var cachedEnabled = GUI.enabled;
                    GUI.enabled = canBack;
                    if (GUILayout.Button("Root", GUILayout.Width(50)))
                    {
                        _currentPath = string.Empty;
                    }

                    if (GUILayout.Button("Back", GUILayout.Width(50)))
                    {
                        var index = _currentPath.LastIndexOf('/');
                        if (index > 0)
                        {
                            var nextIndex = _currentPath.LastIndexOf('/', index - 1);
                            _currentPath = nextIndex > 0 ? _currentPath[..nextIndex] : string.Empty;
                        }
                        else
                        {
                            _currentPath = string.Empty;
                        }
                    }

                    GUI.enabled = false;
                    _currentPath = GUILayout.TextField(_currentPath);
                    GUI.enabled = cachedEnabled;
                }

                if (_currentPath == null)
                {
                    _currentPath = string.Empty;
                }

                var anyItem = false;
                foreach (var (key, action, _) in MornDebugCore.GetValues())
                {
                    if (string.IsNullOrEmpty(_currentPath) || key.StartsWith(_currentPath))
                    {
                        anyItem = true;
                        var relativePath = key.Substring(_currentPath.Length);
                        if (relativePath.Contains('/'))
                        {
                            var index = relativePath.IndexOf('/');
                            var folderName = relativePath.Substring(0, index);
                            if (GUILayout.Button($"▶ {folderName}"))
                            {
                                _currentPath += folderName + "/";
                            }
                        }
                        else
                        {
                            GUILayout.Label(relativePath, GUI.skin.label);
                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                action?.Invoke();
                            }
                        }
                    }
                }

                if (!anyItem)
                {
                    GUILayout.Label("No menu items.");
                }
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