using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Infrastructure
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization
    /// </summary>
    public class AsyncLazy<T>
    {
        private readonly Lazy<Task<T>> _instance;

        public AsyncLazy(Func<T> valueFactory)
        {
            _instance = new Lazy<Task<T>>(() => Task.Run(valueFactory));
        }

        public AsyncLazy(Func<Task<T>> taskFactory)
        {
            _instance = new Lazy<Task<T>>(() => Task.Run(taskFactory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return _instance.Value.GetAwaiter();
        }

        public Task<T> Value => _instance.Value;
    }
}
