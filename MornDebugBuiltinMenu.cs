using System;
using System.Collections.Generic;
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
                using (new MornGUILayout.EnableScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("PlayerPrefsをリセット"))
                    {
                        PlayerPrefs.DeleteAll();
                    }
                }
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
                using (new MornGUILayout.EnableScope(Application.isPlaying))
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
                using (new GUILayout.VerticalScope())
                {
                    using (new MornGUILayout.EnableScope(Application.isPlaying))
                    {
                        if (GUILayout.Button("現在のシーンを読み込み直す"))
                        {
                            var scene = SceneManager.GetActiveScene();
                            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
                        }
                    }
#if UNITY_EDITOR
                    using (new MornGUILayout.EnableScope(!Application.isPlaying))
                    {
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
                    }
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
                using (new MornGUILayout.EnableScope(!Application.isPlaying))
                {
                    _sceneAssetTree.OnGUI();
                }
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