using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MornDebug
{
    [CreateAssetMenu(fileName = nameof(MornDebugBuiltinMenu), menuName = "Morn/" + nameof(MornDebugBuiltinMenu))]
    public sealed class MornDebugBuiltinMenu : MornDebugInitialMenuBase
    {
        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return ("セーブマネージャ", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                if (GUILayout.Button("PlayerPrefsをリセット"))
                {
                    PlayerPrefs.DeleteAll();
                }

                GUI.enabled = cachedEnabled;
            });
#if UNITY_EDITOR
            yield return ("リロード", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reload Domain"))
                    {
                        EditorUtility.RequestScriptReload();
                    }

                    if (GUILayout.Button("Reload Scene"))
                    {
                        var scene = SceneManager.GetActiveScene();
                        var opts = new LoadSceneParameters();
                        EditorSceneManager.LoadSceneInPlayMode(scene.path, opts);
                    }
                }

                GUI.enabled = cachedEnabled;
            });
#endif
            yield return ("シーン一覧", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var sceneName = scene.path.Split('/').Last();
                    if (GUILayout.Button(sceneName))
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }

                GUI.enabled = cachedEnabled;
            });
        }
    }
}