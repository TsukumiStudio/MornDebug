using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("MornDebug.Editor")]

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugGlobal), menuName = "Morn/Debug/" + nameof(MornDebugGlobal))]
    internal sealed class MornDebugGlobal : MornGlobalBase<MornDebugGlobal>
    {
        [Header("ランタイムUI")]
        [SerializeField] private float _guiScale = 2f;
        [SerializeField] private int _padding = 20;

        [Header("メニュー")]
        [SerializeField] private List<MornDebugMenuBase> _menus;

        public List<MornDebugMenuBase> Menus => _menus;
        public float GUIScale => _guiScale;
        public int Padding => _padding;
        protected override string ModuleName => "MornDebug";
    }
}