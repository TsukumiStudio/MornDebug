using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornLib
{
    public abstract class MornDebugMenuBase : ScriptableObject
    {
        public abstract IEnumerable<(string key, Action action)> GetMenuItems();

        public virtual void OnUpdate()
        {
        }
    }
}