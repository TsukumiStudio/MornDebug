#if UNITY_EDITOR
using UnityEditor;

namespace MornDebug
{
    internal sealed class MornDebugWindow : EditorWindow
    {
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
            MornDebugCore.OnUpdate();
        }

        private void OnGUI()
        {
            MornDebugCore.OnGUI(true);
        }
    }
}
#endif