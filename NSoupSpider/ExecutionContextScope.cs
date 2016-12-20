using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSoupSpider
{
    /// <summary>
    /// 统一执行上下问事务区间（单线程）
    /// </summary>
    /// <remarks>http://www.cnblogs.com/artech/archive/2013/04/14/execution-context.html</remarks>
    public class ExecutionContextScope : IDisposable
    {
        private ExecutionContext originalContext = ExecutionContext.Current;

        public ExecutionContextScope(ExecutionContextOption contextOption = ExecutionContextOption.Required)
        {
            switch (contextOption)
            {
                case ExecutionContextOption.RequiresNew:
                    {
                        ExecutionContext.Current = new ExecutionContext();
                        break;
                    }
                case ExecutionContextOption.Required:
                    {
                        ExecutionContext.Current = originalContext ?? new ExecutionContext();
                        break;
                    }
                case ExecutionContextOption.Suppress:
                    {
                        ExecutionContext.Current = null;
                        break;
                    }
            }
        }

        public ExecutionContextScope(DependentContext dependentContext)
        {
            if (dependentContext.OriginalThread == Thread.CurrentThread)
            {
                throw new InvalidOperationException("The DependentContextScope cannot be created in the thread in which the DependentContext is created.");
            }
            ExecutionContext.Current = dependentContext;
        }

        public void Dispose()
        {
            ExecutionContext.Current = originalContext;
        }
    }

    public enum ExecutionContextOption
    {
        /// <summary>
        /// 继承
        /// </summary>
        Required,
        /// <summary>
        /// 新建
        /// </summary>
        RequiresNew,
        /// <summary>
        /// 排它
        /// </summary>
        Suppress
    }

    [Serializable]
    public class ExecutionContext
    {
        [ThreadStatic]
        private static ExecutionContext current;

        public IDictionary<string, object> Items { get; internal set; }

        internal ExecutionContext()
        {
            this.Items = new Dictionary<string, object>();
        }

        public T GetValue<T>(string name, T defaultValue = default(T))
        {
            object value;
            if (this.Items.TryGetValue(name, out value))
            {
                return (T)value;
            }
            return defaultValue;
        }

        public void SetValue(string name, object value)
        {
            this.Items[name] = value;
        }

        /// <summary>
        /// 当前执行上下文
        /// </summary>
        public static ExecutionContext Current
        {
            get { return current; }
            internal set { current = value; }
        }

        public DependentContext DepedentClone()
        {
            return new DependentContext(this);
        }
    }

    [Serializable]
    public class DependentContext : ExecutionContext
    {
        public Thread OriginalThread { get; private set; }

        public DependentContext(ExecutionContext context)
        {
            OriginalThread = Thread.CurrentThread;
            this.Items = new Dictionary<string, object>(context.Items);
        }
    }

}
