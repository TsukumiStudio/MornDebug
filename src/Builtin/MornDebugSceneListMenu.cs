using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugSceneListMenu), menuName = "Morn/Debug/" + nameof(MornDebugSceneListMenu))]
    public sealed class MornDebugSceneListMenu : MornDebugMenuBase
    {
        // フィールドは Editor / ランタイム で同一レイアウトにして、ビルド時のシリアライズレイアウト差分警告を避ける。
        // _rootSceneFolder は Editor では DefaultAsset をドロップする想定。型は UnityEngine.Object として保持する。
        [SerializeField] private string _menuKey = "シーン一覧";
        [SerializeField] private UnityEngine.Object _rootSceneFolder;

#if UNITY_EDITOR
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

        private SceneAssetTree _sceneAssetTree;
#endif

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
#if UNITY_EDITOR
            var folderPath = _rootSceneFolder != null ? AssetDatabase.GetAssetPath(_rootSceneFolder) : "";
            var prefix = string.IsNullOrEmpty(folderPath) ? "" : folderPath + "/";
            _sceneAssetTree = new SceneAssetTree(prefix);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                _sceneAssetTree.Add(scene);
            }

            yield return (_menuKey, () =>
            {
                using (new MornDebugGUILayout.EnableScope(!Application.isPlaying))
                {
                    _sceneAssetTree.OnGUI();
                }
            });
#else
            yield break;
#endif
        }
    }
}
