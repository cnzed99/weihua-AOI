using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.CancelableThreadTask
{
    /// <summary>
    /// 包装方法 该方法内部需有锁阻塞如Monitor.Wait,Mutex.WaitOne,SemaphoreSlim.Wait,EventWaitHandle.WaitOne等 使其可异步取消
    /// </summary>
    public class CancelableThreadTask
    {
        private Thread _thread;
        private readonly Action _action;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;
        private TaskCompletionSource<object> _tcs;

        private int _isRuning = 0;

        public CancelableThreadTask(
            Action action,
            Action<Exception> onError = null,
            Action onCompleted = null
        )
        {
            _action = action;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public Task RunAsync(CancellationToken token)
        {
            if (Interlocked.CompareExchange(ref _isRuning, 1, 0) == 1)
                throw new InvalidOperationException("Task is already runing");
            _tcs = new TaskCompletionSource<object>();
            _thread = new Thread(() =>
            {
                try
                {
                    _action();
                    _tcs.SetResult(null);
                    _onCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadInterruptedException)
                        _tcs.TrySetCanceled(token);
                    else
                        _tcs.TrySetException(ex);
                    _onError?.Invoke(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref _isRuning, 0);
                }
            });
            token.Register(() =>
            {
                if (Interlocked.CompareExchange(ref _isRuning, 0, 1) == 1)
                {
                    _thread.Interrupt();
                    _thread.Join();
                    _tcs.TrySetCanceled(token);
                }
            });
            _thread.Start();
            return _tcs.Task;
        }
    }
}
