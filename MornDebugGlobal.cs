using System.Collections.Generic;
using MornGlobal;
using UnityEngine;

namespace MornDebug
{
    [CreateAssetMenu(fileName = nameof(MornDebugGlobal), menuName = "Morn/" + nameof(MornDebugGlobal))]
    internal sealed class MornDebugGlobal : MornGlobalBase<MornDebugGlobal>
    {
        [SerializeField] private List<MornDebugMenuBase> _menus;
        public List<MornDebugMenuBase> Menus => _menus;
        protected override string ModuleName => nameof(MornDebug);

        public static void Log(string message)
        {
            I.LogInternal(message);
        }

        public static void LogWarning(string message)
        {
            I.LogWarningInternal(message);
        }

        public static void LogError(string message)
        {
            I.LogErrorInternal(message);
        }
    }
}