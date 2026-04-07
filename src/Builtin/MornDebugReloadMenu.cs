using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugReloadMenu), menuName = "Morn/Debug/" + nameof(MornDebugReloadMenu))]
    public sealed class MornDebugReloadMenu : MornDebugMenuBase
    {
        [SerializeField] private string _menuKey = "リロード";

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return (_menuKey, () =>
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new MornDebugGUILayout.EnableScope(Application.isPlaying))
                    {
                        if (GUILayout.Button("現在のシーンを読み込み直す"))
                        {
                            var scene = SceneManager.GetActiveScene();
                            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
                        }
                    }
#if UNITY_EDITOR
                    using (new MornDebugGUILayout.EnableScope(!Application.isPlaying))
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
                    }
#endif
                }
            });
        }
    }
}
