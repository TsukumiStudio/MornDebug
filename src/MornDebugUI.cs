using UnityEngine;

namespace MornLib
{
    public sealed class MornDebugUI : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            MornDebugCore.OnUpdate();
        }

        private void OnGUI()
        {
            if (!gameObject.activeSelf) return;

            const int padding = 40;
            GUILayout.BeginArea(
                new Rect(padding, padding, Screen.width - padding * 2, Screen.height - padding * 2),
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
        }
    }
}
