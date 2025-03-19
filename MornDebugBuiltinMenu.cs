using System;
using System.Collections.Generic;
using MornEditor;
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
        private class SceneTree : MornEditorTreeBase<EditorBuildSettingsScene>
        {
            public SceneTree(string prefix) : base(prefix)
            {
            }

            protected override string NodeToPath(EditorBuildSettingsScene node)
            {
                return node.path;
            }

            protected override void NodeOnGUI(EditorBuildSettingsScene node)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(node.path);
                }
            }
        }

        [SerializeField] private string _scenePathPrefix;
        private SceneTree _tree;

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
            _tree = new SceneTree(_scenePathPrefix);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                _tree.Add(scene);
            }

            yield return ("シーン一覧", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                _tree.OnGUI();
                GUI.enabled = cachedEnabled;
            });
        }
    }
}