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
        private readonly Action complete;
        private readonly T maximum;
        private bool isCompleted;

        public CProgress(Action<T> handler, Action complete, T maximum)
            : base(handler)
        {
            this.complete = complete;
            this.maximum = maximum;

            ProgressChanged += CheckCompletion;
        }

        protected override void OnReport(T value)
        {
            if (isCompleted)
                return;
            base.OnReport(value);
        }

        private void CheckCompletion(object sender, T e)
        {
            if (e.Equals(maximum) && !isCompleted)
            {
                isCompleted = true;
                complete?.Invoke();
            }
        }
        public void Reset()
        {
            isCompleted = false;
        }
        public void Report(T value)
        {
            
            this.OnReport(value);
        }
    }
}
