using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugSoundMenu), menuName = "Morn/Debug/" + nameof(MornDebugSoundMenu))]
    public sealed class MornDebugSoundMenu : MornDebugMenuBase
    {
        [SerializeField] private string _menuKey = "サウンド";
        [SerializeField] private AudioMixer _audioMixer;
        private string[] _exposedParams;
        private AudioMixer _cachedMixer;

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return (_menuKey, () =>
            {
                if (_audioMixer == null)
                {
                    GUILayout.Label("AudioMixerが未設定です。");
                    return;
                }

                GUILayout.Label($"Mixer: {_audioMixer.name}");
                if (_exposedParams == null || _cachedMixer != _audioMixer)
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
                    _audioMixer.GetFloat(param, out var value);
                    var overrideKey = $"{nameof(MornDebugSoundMenu)}_Override_{param}";
                    var valueKey = $"{nameof(MornDebugSoundMenu)}_Value_{param}";
                    var isOverride = PlayerPrefs.GetInt(overrideKey, 0) == 1;

                    using (new GUILayout.HorizontalScope())
                    {
                        var newOverride = GUILayout.Toggle(isOverride, "", GUILayout.Width(20));
                        if (newOverride != isOverride)
                        {
                            PlayerPrefs.SetInt(overrideKey, newOverride ? 1 : 0);
                            PlayerPrefs.Save();
                            isOverride = newOverride;
                        }

                        if (isOverride)
                        {
                            var overrideValue = PlayerPrefs.GetFloat(valueKey, value);
                            GUILayout.Label(param, GUILayout.Width(100));
                            var sliderValue = GUILayout.HorizontalSlider(overrideValue, -80, 20);
                            var inputText = GUILayout.TextField($"{overrideValue:F1}", GUILayout.Width(50));
                            GUILayout.Label("dB", GUILayout.Width(20));
                            var newValue = overrideValue;
                            if (!Mathf.Approximately(overrideValue, sliderValue))
                            {
                                newValue = sliderValue;
                            }
                            else if (float.TryParse(inputText, out var parsed) && !Mathf.Approximately(overrideValue, parsed))
                            {
                                newValue = Mathf.Clamp(parsed, -80, 20);
                            }

                            if (!Mathf.Approximately(overrideValue, newValue))
                            {
                                PlayerPrefs.SetFloat(valueKey, newValue);
                                PlayerPrefs.Save();
                            }
                        }
                        else
                        {
                            using (new MornDebugGUILayout.EnableScope(false))
                            {
                                GUILayout.Label(param, GUILayout.Width(100));
                                GUILayout.HorizontalSlider(value, -80, 20);
                                GUILayout.Label($"{value:F1} dB", GUILayout.Width(70));
                            }
                        }
                    }
                }
            });
        }

        private void CacheExposedParams()
        {
            _cachedMixer = _audioMixer;
            var paramList = new List<string>();
#if UNITY_EDITOR
            if (_audioMixer != null)
            {
                var so = new SerializedObject(_audioMixer);
                var prop = so.FindProperty("m_ExposedParameters");
                if (prop != null && prop.isArray)
                {
                    for (var i = 0; i < prop.arraySize; i++)
                    {
                        var name = prop.GetArrayElementAtIndex(i).FindPropertyRelative("name")?.stringValue;
                        if (!string.IsNullOrEmpty(name))
                        {
                            paramList.Add(name);
                        }
                    }
                }
            }
#endif
            _exposedParams = paramList.ToArray();
        }

        public override void OnUpdate()
        {
            if (_audioMixer == null || _exposedParams == null) return;
            if (!Application.isPlaying) return;
            foreach (var param in _exposedParams)
            {
                var overrideKey = $"{nameof(MornDebugSoundMenu)}_Override_{param}";
                if (PlayerPrefs.GetInt(overrideKey, 0) != 1) continue;
                var valueKey = $"{nameof(MornDebugSoundMenu)}_Value_{param}";
                var val = PlayerPrefs.GetFloat(valueKey, 0);
                _audioMixer.SetFloat(param, val);
            }
        }
    }
}
