using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    [DebuggerDisplay("Deepth={ScopeDeepth,nq}, Id={ScopeId,nq}, ContainerId={ContainerId}")]
    public class ExtractScope
    {
        public ExtractScope()
        {

        }

        public int ScopeDeepth { get; set; }

        public string ScopeId { get; set; }

        public string ContainerId { get; set; }


        public void Set(string key, object value)
        {
            ExecutionContext current = GetCodeEnvContext();

            string cmpScopeKey = getContainerKey(ScopeDeepth, ScopeId);
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
            else
            {
                existDict[key] = value;
            }

        }

        string getContainerKey(int scopeLen, string containerId)
        {
            return string.Format("#{0}#{1}", scopeLen, containerId ?? "^");
        }

        string getPopKey()
        {
            return "ExtractScope#" + ScopeDeepth.ToString();
        }

        protected void ResetScopeObject(Dictionary<string, object> dict = null)
        {
            ExecutionContext current = GetCodeEnvContext();
            string cmpScopeKey = getContainerKey(ScopeDeepth, ScopeId);
            current.SetValue(cmpScopeKey, dict ?? new Dictionary<string, object>());
        }

        public Dictionary<string, object> GetScopeObject()
        {
            ExecutionContext current = GetCodeEnvContext();
            string cmpScopeKey = getContainerKey(ScopeDeepth, ScopeId);
            return current.GetValue<Dictionary<string, object>>(cmpScopeKey);
        }

        public Dictionary<string, object> GetPopObjectIteration()
        {
            ExecutionContext current = GetCodeEnvContext();
            string popKey = getPopKey();
            Dictionary<string, object> ret = current.GetValue<Dictionary<string, object>>(popKey);
            current.SetValue(popKey, new Dictionary<string, object>());
            return ret;
        }

        internal static ExecutionContext GetCodeEnvContext()
        {
            ExecutionContext current = ExecutionContext.Current;
            if (current == null)
                throw new InvalidOperationException("代码需运行在ExecutionContextScope环境中！");

            return current;
        }

        public int PopUp(bool overWrite = false)
        {
            ExecutionContext current = GetCodeEnvContext();
            string cmpScopeKey = getContainerKey(ScopeDeepth, ScopeId);
            Dictionary<string, object> scopeObj = current.GetValue<Dictionary<string, object>>(cmpScopeKey);
            if (scopeObj == null)
            {
                Dictionary<string, object> thisPopObject = current.GetValue<Dictionary<string, object>>(getPopKey());
                if (thisPopObject == null)
                    return ScopeDeepth;
                else
                    scopeObj = thisPopObject;
            }

            int rightScopeDeepth = ScopeDeepth - 1;
            string popKey = string.Format("ExtractScope#{0}", rightScopeDeepth);
            Dictionary<string, object> popDict = current.GetValue<Dictionary<string, object>>(popKey);
            if (popDict == null)
            {
                current.SetValue(popKey, scopeObj);
            }
            else
            {
                MergingScopeObjectWith(scopeObj, popDict, overWrite);
            }

            return rightScopeDeepth;
        }

        internal static void MergingScopeObjectWith(IDictionary<string, object> toMergin, IDictionary<string, object> target, bool overWrite = false)
        {
            if (toMergin == null) return;
            List<string> scopeKeys = toMergin.Keys.ToList();
            foreach (var key in scopeKeys)
            {
                if (target.ContainsKey(key))
                {
                    if (overWrite == true)
                        target[key] = toMergin[key];
                }
                else
                {
                    target.Add(key, toMergin[key]);
                }
            }

        }


        public static Dictionary<string, object> MergingAllScopeObject()
        {
            ExecutionContext current = GetCodeEnvContext();
            Dictionary<string, object> rootObject = new Dictionary<string, object>();
            foreach (var item in current.Items.Keys)
            {
                var crtDict = current.Items[item] as IDictionary<string, object>;
                if (crtDict != null)
                    MergingScopeObjectWith(crtDict, rootObject, true);
            }
            return rootObject;
        }

    }

}
