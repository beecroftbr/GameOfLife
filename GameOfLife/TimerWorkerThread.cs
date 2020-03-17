using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace GameOfLife
{
    public class TimerWorkerThread : IHostedService, IDisposable
    {
        public static IHubContext<Hubs.DrawHub> HubContext;
        public static CancellationTokenSource cts;
        public static int _timerDelayInMilliseconds;

        public TimerWorkerThread()
        {
            cts = new CancellationTokenSource();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>ExecuteAsync(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await new Hubs.DrawHub().FinalizeDraw(Hubs.DrawHub.lastArray, Hubs.DrawHub._lastTotalTileDepth, 1);
                await Task.Delay(_timerDelayInMilliseconds, stoppingToken);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TimerWorkerThread()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
