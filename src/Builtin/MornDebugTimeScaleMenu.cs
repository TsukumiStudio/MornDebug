using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugTimeScaleMenu), menuName = "Morn/Debug/" + nameof(MornDebugTimeScaleMenu))]
    public sealed class MornDebugTimeScaleMenu : MornDebugMenuBase
    {
        [SerializeField] private string _menuKey = "チート/時間操作";

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return (_menuKey, () =>
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
                                Time.timeScale = Mathf.Max(timeScale - 1f, 0f);
                            }

                            if (GUILayout.Button("-0.1"))
                            {
                                Time.timeScale = Mathf.Max(timeScale - 0.1f, 0f);
                            }

                            if (GUILayout.Button("=1"))
                            {
                                Time.timeScale = 1f;
                            }

                            if (GUILayout.Button("+0.1"))
                            {
                                Time.timeScale = Mathf.Min(timeScale + 0.1f, 10f);
                            }

                            if (GUILayout.Button("+1"))
                            {
                                Time.timeScale = Mathf.Min(timeScale + 1f, 10f);
                            }
                        }
                    }
                }
            });
        }
    }
}
