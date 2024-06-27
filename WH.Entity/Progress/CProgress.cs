using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Progress
{
    /// <summary>
    /// 用于异步汇报进度
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CProgress<T> : Progress<T> where T : notnull
    {
        private readonly Action _complete;
        private readonly T _maximum;
        private bool _isCompleted;

        public CProgress(Action<T> handler, Action complete, T maximum)
            : base(handler)
        {
            _complete = complete;
            _maximum = maximum;

            ProgressChanged += CheckCompletion;
        }

        protected override void OnReport(T value)
        {
            if (_isCompleted)
                return;
            base.OnReport(value);
        }

        private void CheckCompletion(object sender, T e)
        {
            if (e.Equals(_maximum) && !_isCompleted)
            {
                _isCompleted = true;
                _complete?.Invoke();
            }
        }
        public void Reset()
        {
            _isCompleted = false;
        }
        public void Report(T value)
        {
            
            this.OnReport(value);
        }
    }
}
