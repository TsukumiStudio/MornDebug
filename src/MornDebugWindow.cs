#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornDebugWindow : EditorWindow
    {
        private const string GitUrl = "https://github.com/TsukumiStudio/MornDebug";
        private static Texture2D _icon;
        private string _version;

        [MenuItem("Tools/MornDebug")]
        private static void Open()
        {
            var window = GetWindow<MornDebugWindow>();
            window.SetTitleWithIcon();
        }

        private void OnEnable()
        {
            LoadIcon();
            SetTitleWithIcon();
            LoadPackageVersion();
            EditorApplication.update += UpdateLoop;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateLoop;
        }

        private void LoadIcon()
        {
            if (_icon != null) return;
            var guids = AssetDatabase.FindAssets("MornDebug_Icon t:texture2d");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("MornDebug_Icon.png"))
                {
                    _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    break;
                }
            }
        }

        private void SetTitleWithIcon()
        {
            titleContent = _icon != null
                ? new GUIContent(ToInvisibleUniqueText(nameof(MornDebugWindow)), _icon)
                : new GUIContent("MornDebug");
        }

        /// <summary>
        /// 文字列を不可視Unicode文字列にエンコードする。
        /// 空文字だとUnityが同一ウィンドウと誤認するため、ユニークかつ不可視な文字列が必要。
        /// </summary>
        private static string ToInvisibleUniqueText(string name)
        {
            const string k = "\u200B\u2060\uFEFF\u200E\u200F\u2061\u2062\u2063";
            return string.Concat(name.Select(c => $"{k[c >> 3 & 7]}{k[c & 7]}"));
        }

        private void UpdateLoop()
        {
            Repaint();
            MornDebugCore.OnUpdate();
        }

        private void LoadPackageVersion()
        {
            _version = "unknown";
            var guids = AssetDatabase.FindAssets("package t:textasset");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("MornDebug") && path.EndsWith("package.json"))
                {
                    var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (json != null)
                    {
                        var packageInfo = JsonUtility.FromJson<PackageInfo>(json.text);
                        _version = packageInfo.version;
                    }

                    break;
                }
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label($"MornDebug v{_version}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("GitHub", EditorStyles.linkLabel))
                {
                    Application.OpenURL(GitUrl);
                }
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space();
            MornDebugCore.OnGUI(true);
        }

        [System.Serializable]
        private struct PackageInfo
        {
            public string version;
        }
    }
}
#endif
