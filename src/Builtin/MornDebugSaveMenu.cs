using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugSaveMenu), menuName = "Morn/Debug/" + nameof(MornDebugSaveMenu))]
    public sealed class MornDebugSaveMenu : MornDebugMenuBase
    {
        [SerializeField] private string _menuKey = "セーブ/データ削除";

        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield return (_menuKey, () =>
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
