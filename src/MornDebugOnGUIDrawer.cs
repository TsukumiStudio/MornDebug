using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornDebugOnGUIDrawer
    {
        private GUIStyle _headerStyle;
        private Vector2 _scrollPosition;
        private string _currentPath;
        private readonly List<string> _groups = new();
        private readonly HashSet<string> _folderHash = new();
        private readonly List<MornDebugEntry> _endpoints = new();

        public void OnGUI(List<MornDebugEntry> entries)
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                };
            }

            DrawPath();
            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                if (_currentPath == null)
                {
                    _currentPath = string.Empty;
                }

                DrawTree(entries);
            }
        }

        private void DrawPath()
        {
            using (new GUILayout.HorizontalScope())
            {
                var canBack = !string.IsNullOrEmpty(_currentPath);
                using (new MornDebugGUILayout.EnableScope(canBack))
                {
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
                }

                using (new MornDebugGUILayout.EnableScope(false))
                {
                    _currentPath = GUILayout.TextField(_currentPath);
                }
            }
        }

        private void DrawTree(List<MornDebugEntry> entries)
        {
            _groups.Clear();
            _folderHash.Clear();
            _endpoints.Clear();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(_currentPath) && !entry.Key.StartsWith(_currentPath)) continue;
                var relativePath = entry.Key.Substring(_currentPath.Length);
                if (relativePath.Contains('/'))
                {
                    _groups.Add(relativePath);
                }
                else
                {
                    _endpoints.Add(entry);
                }
            }

            if (_groups.Count == 0 && _endpoints.Count == 0)
            {
                GUILayout.Label("項目がありません。");
                return;
            }

            foreach (var relativePath in _groups)
            {
                var index = relativePath.IndexOf('/');
                var folderName = relativePath.Substring(0, index);
                if (_folderHash.Add(folderName))
                {
                    if (GUILayout.Button($"[ {folderName} ]"))
                    {
                        _currentPath += folderName + "/";
                    }
                }
            }

            foreach (var entry in _endpoints)
            {
                var label = entry.Key.Substring(_currentPath.Length);
                GUILayout.Label(label, _headerStyle);
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    entry.Invoke();
                }
            }
        }
    }
}
