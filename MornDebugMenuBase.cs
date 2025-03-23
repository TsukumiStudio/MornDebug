using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornDebug
{
    public abstract class MornDebugMenuBase : ScriptableObject
    {
        public abstract IEnumerable<(string key, Action action)> GetMenuItems();
    }
}