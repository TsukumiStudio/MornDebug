#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugSceneListMenu), menuName = "Morn/Debug/" + nameof(MornDebugSceneListMenu))]
    public sealed class MornDebugSceneListMenu : MornDebugMenuBase
    {
        private sealed class SceneAssetTree : MornDebugTreeBase<EditorBuildSettingsScene>
        {
            public SceneAssetTree(string prefix) : base(prefix)
            {
            }

            protected override string NodeToPath(EditorBuildSettingsScene node)
            {
                return node.path;
            }

            protected override void NodeClicked(EditorBuildSettingsScene node)
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<SceneAsset>(node.path));
            }
        }

        [SerializeField] private DefaultAsset _sceneFolder;
        private SceneAssetTree _sceneAssetTree;

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            var prefix = _sceneFolder != null ? AssetDatabase.GetAssetPath(_sceneFolder) + "/" : "";
            _sceneAssetTree = new SceneAssetTree(prefix);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                _sceneAssetTree.Add(scene);
            }

            yield return ("シーン一覧", () =>
            {
                using (new MornDebugGUILayout.EnableScope(!Application.isPlaying))
                {
                    _sceneAssetTree.OnGUI();
                }
            });
        }
    }
}
#endif
