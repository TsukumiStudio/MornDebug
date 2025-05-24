using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MornDebug
{
    [CreateAssetMenu(fileName = nameof(MornDebugBuiltinMenu), menuName = "Morn/" + nameof(MornDebugBuiltinMenu))]
    public sealed class MornDebugBuiltinMenu : MornDebugMenuBase
    {
#if UNITY_EDITOR
        private sealed class SceneAssetTree : MornEditorTreeBase<EditorBuildSettingsScene>
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

        [SerializeField] private string _scenePathPrefix;
        private SceneAssetTree _sceneAssetTree;
#endif
        [SerializeField] private string _mixerVolumeKey;
        [SerializeField] private AudioMixer _debugMixer;
        private const string MixerVolumeKey = nameof(MornDebugBuiltinMenu) + "_MixerVolume";

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return ("セーブマネージャ/データ削除", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                if (GUILayout.Button("PlayerPrefsをリセット"))
                {
                    PlayerPrefs.DeleteAll();
                }

                GUI.enabled = cachedEnabled;
            });
            yield return ("サウンド", () =>
            {
                var volume = PlayerPrefs.GetFloat(MixerVolumeKey, 0);
                GUILayout.Label($"音量 : {volume} dB");
                var newVolume = GUILayout.HorizontalSlider(volume, -100, 0, GUILayout.Height(10));
                if (!Mathf.Approximately(volume, newVolume))
                {
                    PlayerPrefs.SetFloat(MixerVolumeKey, newVolume);
                    PlayerPrefs.Save();
                }
            });
            yield return ("リロード", () =>
            {
                var cachedEnabled = GUI.enabled;
                using (new GUILayout.VerticalScope())
                {
                    GUI.enabled = Application.isPlaying;
                    if (GUILayout.Button("現在のシーンを読み込み直す"))
                    {
                        var scene = SceneManager.GetActiveScene();
                        SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
                    }
#if UNITY_EDITOR
                    GUI.enabled = !Application.isPlaying;
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Reload Domain"))
                        {
                            ReloadDomain();
                        }

                        if (GUILayout.Button("Reload Scene"))
                        {
                            ReloadScene();
                        }
                    }

                    GUI.enabled = cachedEnabled;
#endif
                }
            });
#if UNITY_EDITOR
            _sceneAssetTree = new SceneAssetTree(_scenePathPrefix);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                _sceneAssetTree.Add(scene);
            }

            yield return ("シーン一覧", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                _sceneAssetTree.OnGUI();
                GUI.enabled = cachedEnabled;
            });
            yield return ("git/便利系", () =>
            {
                var cachedEnabled = GUI.enabled;
                GUI.enabled = !Application.isPlaying;
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Submodule更新"))
                    {
                        UpdateSubmoduleAsync().Forget();
                    }

                    if (GUILayout.Button("差分全消し"))
                    {
                        DeleteDiffAsync().Forget();
                    }
                }

                GUI.enabled = cachedEnabled;
            });
#endif
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Application.isPlaying)
            {
                if (_debugMixer)
                {
                    var volume = PlayerPrefs.GetFloat(MixerVolumeKey, 0);
                    _debugMixer.SetFloat(_mixerVolumeKey, volume);
                }
            }
        }

#if UNITY_EDITOR
        private async static UniTask UpdateSubmoduleAsync(CancellationToken ct = default)
        {
            var process = MornProcess.MornProcess.CreateAtAssets("git");
            var stashName = $"{nameof(MornDebug)}による退避 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            await process.ExecuteAsync($"stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync($"submodule foreach --recursive git stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync("submodule deinit -f --all", ct);
            await process.ExecuteAsync("submodule update --init --recursive", ct);
            process.Dispose();
            MornDebugGlobal.Log("submodule更新完了");
        }

        private async static UniTask DeleteDiffAsync(CancellationToken ct = default)
        {
            var process = MornProcess.MornProcess.CreateAtAssets("git");
            var stashName = $"{nameof(MornDebug)}による退避 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            await process.ExecuteAsync($"stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync($"submodule foreach --recursive git stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync("reset --hard HEAD", ct);
            await process.ExecuteAsync("clean -fd", ct);
            process.Dispose();
            MornDebugGlobal.Log("差分全消し完了");
        }

        [MenuItem("Tools/Submoduleなおすボタン")]
        private static void ReloadSubmodule()
        {
            UpdateSubmoduleAsync().Forget();
        }

        [MenuItem("Tools/差分全消しボタン")]
        private static void DeleteDiff()
        {
            DeleteDiffAsync().Forget();
        }

        [MenuItem("Tools/Reload Domain")]
        private static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }

        [MenuItem("Tools/Reload Scene")]
        private static void ReloadScene()
        {
            var scene = SceneManager.GetActiveScene();
            var opts = new LoadSceneParameters();
            EditorSceneManager.LoadSceneInPlayMode(scene.path, opts);
        }
#endif
    }
}