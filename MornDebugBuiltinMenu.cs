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
                if (GUILayout.Button("PlayerPrefsをリセット"))
                {
                    PlayerPrefs.DeleteAll();
                }
            });
#if UNITY_EDITOR
            yield return ("リロード", () =>
            {
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
            });
#endif
            yield return ("シーン一覧", () =>
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var sceneName = scene.path.Split('/').Last();
                    if (GUILayout.Button(sceneName))
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }
            });
        }
    }
}