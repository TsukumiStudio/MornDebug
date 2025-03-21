#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MornDebug
{
    internal sealed class MornDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _currentPath;

        [MenuItem("Tools/何でもデバッグ(MornDebugWindow)")]
        private static void Open()
        {
            GetWindow<MornDebugWindow>("何デバ");
        }

        private void OnEnable()
        {
            EditorApplication.update += UpdateLoop;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateLoop;
        }

        private void UpdateLoop()
        {
            Repaint();
        }

        private void OnGUI()
        {
            MornDebugCore.CheckCancellation();
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                using (new GUILayout.HorizontalScope())
                {
                    var canBack = _currentPath.Length > 0;
                    var cachedEnabled = GUI.enabled;
                    GUI.enabled = canBack;
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
                    _currentPath = EditorGUILayout.TextField(GUIContent.none, _currentPath);
                    GUI.enabled = cachedEnabled;
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
                            EditorGUILayout.LabelField(relativePath, EditorStyles.boldLabel);
                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                action?.Invoke();
                            }
                        }
                    }
                }

                if (!anyItem)
                {
                    EditorGUILayout.LabelField("No menu items.");
                }
            }
        }
    }
}
#endif