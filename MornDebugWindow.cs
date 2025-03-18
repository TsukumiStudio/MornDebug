#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MornDebug
{
    internal sealed class MornDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

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
                var anyItem = false;
                foreach (var (key, action, _) in MornDebugCore.GetValues())
                {
                    anyItem = true;
                    EditorGUILayout.LabelField(key, EditorStyles.boldLabel);
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        action?.Invoke();
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