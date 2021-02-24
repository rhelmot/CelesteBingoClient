using System;
using System.Threading;

namespace Celeste.Mod.BingoClient {
    public class LockHolder : IDisposable {
        private SemaphoreSlim Semaphore;

        public LockHolder(SemaphoreSlim sem, CancellationToken token) {
            this.Semaphore = sem;
            this.Semaphore.Wait(token);
        }

        public void Dispose() {
            this.Semaphore.Release();
        }
    }

    public static class LockHolderExt {
        public static LockHolder Use(this SemaphoreSlim self, CancellationToken token) {
            return new LockHolder(self, token);
        }
    }
}
