using System.Threading;
using UnityEngine;

namespace MornLib
{
    internal sealed class MornDebugDestroyTrigger : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        public CancellationToken CancellationToken
        {
            get
            {
                _cts ??= new CancellationTokenSource();
                return _cts.Token;
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
