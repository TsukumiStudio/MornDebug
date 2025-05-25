using UnityEngine;

namespace MornDebug
{
    public sealed class MornDebugUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

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
            if (_rectTransform == null)
            {
                return;
            }

            const int padding = 40;
            GUILayout.BeginArea(
                new Rect(padding, padding, Screen.width - padding * 2, Screen.height - padding * 2),
                style: "box");

            // 右詰めで閉じるボタン
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("閉じる"))
                {
                    Hide();
                }
            }
            
            // デバッグ情報の表示
            MornDebugCore.OnGUI(false);

            GUILayout.EndArea();
        }
    }
}