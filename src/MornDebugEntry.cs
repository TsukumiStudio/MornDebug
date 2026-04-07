using System;
using System.Threading;

namespace MornLib
{
    internal sealed class MornDebugEntry : IDisposable
    {
        public readonly string Key;
        private readonly Action _action;
        private readonly CancellationToken _ct;
        private bool _disposed;

        public bool IsInvalid => _disposed || _ct.IsCancellationRequested;

        public MornDebugEntry(string key, Action action, CancellationToken ct)
        {
            Key = key;
            _action = action;
            _ct = ct;
        }

        public void Invoke()
        {
            try
            {
                _action?.Invoke();
            }
            catch (Exception e)
            {
                MornDebugGlobal.Logger.LogError($"{Key} でエラーが発生したため自動破棄します: {e}");
                _disposed = true;
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
