using UnityEngine;

namespace MornLib
{
    public sealed class MornDebugUI : MonoBehaviour
    {
        private static MornDebugUI _instance;

        public static bool IsVisible => _instance != null && _instance.gameObject.activeSelf;

        public static void Show()
        {
            if (_instance == null)
            {
                var obj = new GameObject(nameof(MornDebugUI));
                _instance = obj.AddComponent<MornDebugUI>();
                DontDestroyOnLoad(obj);
            }

            _instance.gameObject.SetActive(true);
        }

        public static void Hide()
        {
            if (_instance != null)
            {
                _instance.gameObject.SetActive(false);
            }
        }

        public static void Toggle()
        {
            if (_instance != null && _instance.gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Update()
        {
            MornDebugCore.OnUpdate();
        }

        private void OnGUI()
        {
            var global = MornDebugGlobal.I;
            var guiScale = global != null ? global.GUIScale : 2f;
            var padding = global != null ? global.Padding : 20;
            var cachedMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * guiScale);
            var scaledWidth = Screen.width / guiScale;
            var scaledHeight = Screen.height / guiScale;
            GUILayout.BeginArea(
                new Rect(padding, padding, scaledWidth - padding * 2, scaledHeight - padding * 2),
                style: "box");

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("閉じる"))
                {
                    Hide();
                }
            }

            MornDebugCore.OnGUI(false);

            GUILayout.EndArea();
            GUI.matrix = cachedMatrix;
        }
    }
}
