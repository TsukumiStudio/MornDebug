using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornDebug
{
    internal sealed class MornDebugOnGUIDrawer
    {
        private GUIStyle _boldStyle;
        private Vector2 _scrollPosition;
        private string _currentPath;
        private readonly List<string> _groups = new();
        private readonly HashSet<string> _folderHash = new();
        private readonly List<(string, Action)> _endpoints = new();

        public void OnGUI(IEnumerable<(string, Action)> pairs)
        {
            if (_boldStyle == null)
            {
                _boldStyle = new GUIStyle(GUI.skin.label)
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

                DrawTree(pairs);
            }
        }

        private void DrawPath()
        {
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
        }

        private void DrawTree(IEnumerable<(string, Action)> pairs)
        {
            _groups.Clear();
            _folderHash.Clear();
            _endpoints.Clear();
            foreach (var (key, action) in pairs)
            {
                if (string.IsNullOrEmpty(_currentPath) || key.StartsWith(_currentPath))
                {
                    var relativePath = key.Substring(_currentPath.Length);
                    if (relativePath.Contains('/'))
                    {
                        _groups.Add(relativePath);
                    }
                    else
                    {
                        _endpoints.Add((relativePath, action));
                    }
                }
            }

            if (_groups.Count == 0 && _endpoints.Count == 0)
            {
                GUILayout.Label("No items in this path.");
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

            foreach (var (relativePath, action) in _endpoints)
            {
                GUILayout.Label(relativePath, _boldStyle);
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    action?.Invoke();
                }
            }
        }
    }
}