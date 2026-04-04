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

        [SerializeField] private string _scenePathPrefix;
        private SceneAssetTree _sceneAssetTree;
#endif
        [SerializeField] private AudioMixer _debugMixer;
        private string[] _exposedParams;

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return ("セーブ/データ削除", () =>
            {
                using (new MornDebugGUILayout.EnableScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("PlayerPrefsをリセット"))
                    {
                        PlayerPrefs.DeleteAll();
                    }
                }
            });
            yield return ("サウンド", () =>
            {
                if (_debugMixer == null)
                {
                    GUILayout.Label("AudioMixerが未設定です。");
                    return;
                }

                GUILayout.Label($"Mixer: {_debugMixer.name}");
                if (_exposedParams == null)
                {
                    CacheExposedParams();
                }

                if (_exposedParams.Length == 0)
                {
                    GUILayout.Label("公開パラメータがありません。");
                    return;
                }

                foreach (var param in _exposedParams)
                {
                    if (!_debugMixer.GetFloat(param, out var value)) continue;
                    GUILayout.Label($"{param}: {value:F1} dB");
                    var newValue = GUILayout.HorizontalSlider(value, -80, 20, GUILayout.Height(10));
                    GUILayout.Space(5);
                    if (!Mathf.Approximately(value, newValue))
                    {
                        _debugMixer.SetFloat(param, newValue);
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("全てミュート"))
                    {
                        foreach (var param in _exposedParams)
                        {
                            _debugMixer.SetFloat(param, -80);
                        }
                    }

                    if (GUILayout.Button("全てリセット(0dB)"))
                    {
                        foreach (var param in _exposedParams)
                        {
                            _debugMixer.SetFloat(param, 0);
                        }
                    }
                }
            });
            yield return ("チート/時間操作", () =>
            {
                using (new MornDebugGUILayout.EnableScope(Application.isPlaying))
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
                using (new MornDebugGUILayout.EnableScope(!Application.isPlaying))
                {
                    _sceneAssetTree.OnGUI();
                }
            });
#endif
        }

        private void CacheExposedParams()
        {
            var paramList = new List<string>();
#if UNITY_EDITOR
            var so = new SerializedObject(_debugMixer);
            var exposedParams = so.FindProperty("m_ExposedParameters");
            if (exposedParams != null)
            {
                for (var i = 0; i < exposedParams.arraySize; i++)
                {
                    var param = exposedParams.GetArrayElementAtIndex(i);
                    var name = param.FindPropertyRelative("name")?.stringValue;
                    if (!string.IsNullOrEmpty(name))
                    {
                        paramList.Add(name);
                    }
                }
            }
#endif
            _exposedParams = paramList.ToArray();
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