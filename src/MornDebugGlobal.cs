using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("MornDebug.Editor")]

namespace MornLib
{
    [CreateAssetMenu(fileName = nameof(MornDebugGlobal), menuName = "Morn/Debug/" + nameof(MornDebugGlobal))]
    internal sealed class MornDebugGlobal : MornGlobalBase<MornDebugGlobal>
    {
        [SerializeField] private List<MornDebugMenuBase> _menus;
        public List<MornDebugMenuBase> Menus => _menus;
        protected override string ModuleName => "MornDebug";
    }
}