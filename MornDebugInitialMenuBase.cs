using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornDebug
{
    public abstract class MornDebugInitialMenuBase : ScriptableObject
    {
        public abstract IEnumerable<(string key, Action action)> GetMenuItems();
    }
}