using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpider
{
    public abstract class WorkInScopeObject
    {
        protected ExtractScope _runScope = new ExtractScope();

        public ExtractScope Scope
        {
            get { return _runScope; }
            set { _runScope = value; }
        }

    }

    public class ExtractScope
    {
        public void Combine(int scopeLen, string key, object value)
        {
            ExecutionContext current = ExecutionContext.Current;
            if (current == null)
                throw new InvalidOperationException("代码需运行在ExecutionContextScope环境中！");

            string cmpScopeKey = getScopeKey(scopeLen);

            Dictionary<string, object> existDict = current.GetValue<Dictionary<string, object>>(cmpScopeKey);
            if (existDict == null)
            {
                existDict = new Dictionary<string, object>();
                current.SetValue(cmpScopeKey, existDict);
            }

            if (!existDict.ContainsKey(key))
            {
                existDict.Add(key, value);
            }

        }

        string getScopeKey(int scopeLen)
        {
            return string.Format("#ExtractScope#{0}", scopeLen);
        }

        public void ResetScopeObject(int scopeLen, Dictionary<string, object> dict = null)
        {
            ExecutionContext current = ExecutionContext.Current;
            if (current == null)
                throw new InvalidOperationException("代码需运行在ExecutionContextScope环境中！");

            string cmpScopeKey = getScopeKey(scopeLen);
            current.SetValue(cmpScopeKey, dict ?? new Dictionary<string, object>());
        }

        public Dictionary<string, object> GetScopeObject(int scopeLen)
        {
            ExecutionContext current = ExecutionContext.Current;
            if (current == null)
                throw new InvalidOperationException("代码需运行在ExecutionContextScope环境中！");

            string cmpScopeKey = getScopeKey(scopeLen);
            return current.GetValue<Dictionary<string, object>>(cmpScopeKey);
        }

    }

}
