using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MornLib
{
    internal abstract class MornDebugTreeBase<T> where T : new()
    {
        private readonly MornDebugTreeNode _rootNode;

        private sealed class MornDebugTreeNode
        {
            private readonly MornDebugTreeBase<T> _tree;
            private readonly MornDebugTreeNode _parentNode;
            private readonly string _originalPath;
            private readonly string _folderName;
            private readonly Dictionary<string, MornDebugTreeNode> _childNodes = new();
            private readonly List<T> _childList = new();
            private bool IsRoot => _parentNode == null;
            private bool _isFoldout;
            private int Depth => IsRoot ? -1 : _parentNode.Depth + 1;

            public MornDebugTreeNode(MornDebugTreeBase<T> tree, MornDebugTreeNode parent, string originalPath)
            {
                _tree = tree;
                _parentNode = parent;
                _originalPath = originalPath;
                _folderName = originalPath.Contains('/') ? originalPath.Substring(0, originalPath.Length - 1).Split('/').Last() : "";
            }

            public void Clear()
            {
                _childNodes.Clear();
                _childList.Clear();
            }

            public void Add(T target)
            {
                var path = _tree.NodeToPath(target);
                if (!string.IsNullOrEmpty(_originalPath) && !path.StartsWith(_originalPath))
                {
                    return;
                }

                var pathFromPrefix = path.Substring(_originalPath.Length);
                if (pathFromPrefix.Contains("/"))
                {
                    var childPath = pathFromPrefix.Substring(0, pathFromPrefix.IndexOf('/'));
                    if (!_childNodes.TryGetValue(childPath, out var childNode))
                    {
                        childNode = new MornDebugTreeNode(_tree, this, _originalPath + childPath + "/");
                        _childNodes[childPath] = childNode;
                    }

                    childNode.Add(target);
                }
                else
                {
                    _childList.Add(target);
                }
            }

            public void OnGUI()
            {
                if (!IsRoot)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20 * Depth);
                        _isFoldout = FoldOutButton(_isFoldout, _folderName);
                    }
                }

                if (!_isFoldout && !IsRoot) return;

                foreach (var sceneNode in _childNodes.Values)
                {
                    sceneNode.OnGUI();
                }

                foreach (var child in _childList)
                {
                    var path = _tree.NodeToPath(child);
                    var pathFromPrefix = path.Substring(_originalPath.Length);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20 * (Depth + 1));
                        if (GUILayout.Button(pathFromPrefix))
                        {
                            _tree.NodeClicked(child);
                        }
                    }
                }
            }
        }

        protected MornDebugTreeBase(string originalPath)
        {
            _rootNode = new MornDebugTreeNode(this, null, originalPath);
        }

        public void Add(T node)
        {
            _rootNode.Add(node);
        }

        public void OnGUI()
        {
            _rootNode.OnGUI();
        }

        protected abstract string NodeToPath(T node);
        protected abstract void NodeClicked(T node);

        private static bool FoldOutButton(bool isFoldout, string text)
        {
            var label = (isFoldout ? "▼" : "▶") + text;
            var labelWidth = GUI.skin.label.CalcSize(new GUIContent(label)).x;
            if (GUILayout.Button(label, GUILayout.Width(labelWidth + 40)))
            {
                isFoldout = !isFoldout;
            }

            return isFoldout;
        }
    }
}
