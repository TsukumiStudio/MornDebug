using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugSaveMenu), menuName = "Morn/Debug/" + nameof(MornDebugSaveMenu))]
    public sealed class MornDebugSaveMenu : MornDebugMenuBase
    {
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
        }
    }
}
