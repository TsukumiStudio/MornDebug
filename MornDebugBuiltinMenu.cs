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

namespace MornLib
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
            yield return ("チート/時間操作", () =>
            {
                if (Application.isPlaying)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        var timeScale = Time.timeScale;
                        GUILayout.Label($"Time.timeScale : {timeScale}");
                        var newTimeScale = GUILayout.HorizontalSlider(timeScale, 0, 10);
                        GUILayout.Space(15);
                        if (!Mathf.Approximately(timeScale, newTimeScale))
                        {
                            Time.timeScale = newTimeScale;
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("-1"))
                            {
                                var decrementedScale = Mathf.Max(timeScale - 1f, 0f);
                                Time.timeScale = decrementedScale;
                            }

                            if (GUILayout.Button("-0.1"))
                            {
                                var decrementedScale = Mathf.Max(timeScale - 0.1f, 0f);
                                Time.timeScale = decrementedScale;
                            }

                            if (GUILayout.Button("=1"))
                            {
                                Time.timeScale = 1f;
                            }

                            if (GUILayout.Button("+0.1"))
                            {
                                var incrementedScale = Mathf.Min(timeScale + 0.1f, 10f);
                                Time.timeScale = incrementedScale;
                            }

                            if (GUILayout.Button("+1"))
                            {
                                var incrementedScale = Mathf.Min(timeScale + 1f, 10f);
                                Time.timeScale = incrementedScale;
                            }
                        }
                    }
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
                        if (EditorUtility.DisplayDialog("確認", "本当にSubmoduleを更新しますか？\n現在の変更はstashに退避されます。", "実行", "キャンセル"))
                        {
                            UpdateSubmoduleAsync().Forget();
                        }
                    }

                    if (GUILayout.Button("差分全消し"))
                    {
                        if (EditorUtility.DisplayDialog("警告", "本当に差分を全て削除しますか？\nこの操作は取り消せません。\n現在の変更はstashに退避されます。", "実行", "キャンセル"))
                        {
                            DeleteDiffAsync().Forget();
                        }
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
            var process = MornProcess.CreateAtAssets("git");
            var stashName = $"{MornDebugGlobal.I.ModuleName}による退避 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            await process.ExecuteAsync($"stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync($"submodule foreach --recursive git stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync("submodule deinit -f --all", ct);
            await process.ExecuteAsync("submodule update --init --recursive", ct);
            process.Dispose();
            MornDebugGlobal.Log("submodule更新完了");
        }

        private async static UniTask DeleteDiffAsync(CancellationToken ct = default)
        {
            var process = MornProcess.CreateAtAssets("git");
            var stashName = $"{MornDebugGlobal.I.ModuleName}による退避 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            await process.ExecuteAsync($"stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync($"submodule foreach --recursive git stash push -m \"{stashName}\"", ct);
            await process.ExecuteAsync("reset --hard HEAD", ct);
            await process.ExecuteAsync("clean -fd", ct);
            await process.ExecuteAsync("submodule update --init --recursive", ct);
            process.Dispose();
            MornDebugGlobal.Log("差分全消し完了");
        }

        [MenuItem("Tools/MornDebug/Git Submodule再取得")]
        private static void ReloadSubmodule()
        {
            if (EditorUtility.DisplayDialog("確認", "本当にSubmoduleを再取得しますか？\n現在の変更はstashに退避されます。", "実行", "キャンセル"))
            {
                UpdateSubmoduleAsync().Forget();
            }
        }

        [MenuItem("Tools/MornDebug/Git 差分全消しボタン")]
        private static void DeleteDiff()
        {
            if (EditorUtility.DisplayDialog("警告", "本当に差分を全て削除しますか？\nこの操作は取り消せません。\n現在の変更はstashに退避されます。", "実行", "キャンセル"))
            {
                DeleteDiffAsync().Forget();
            }
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