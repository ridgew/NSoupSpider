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

        /// <summary>
        /// 执行领域
        /// </summary>
        public ScopeMode Mode { get; set; }

        protected ExtractScope _ContainerScope = null;
        /// <summary>
        /// 容器区间
        /// </summary>
        public ExtractScope ContainerScope
        {
            get { return _ContainerScope; }
            set { _ContainerScope = value; }
        }

        public int ScopeDeepth { get; set; }

        public string ScopeId { get; set; }

        public string ContainerId { get; set; }


        public void Set(string key, object value)
        {
            ExecutionContext current = GetCodeEnvContext();
            string cmpScopeKey = GetWorkScopeKey();
            IDictionary<string, object> existDict = cmpScopeKey == null ? current.Items : current.GetValue<Dictionary<string, object>>(cmpScopeKey);
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

        public T Get<T>(string itemName)
        {
            ExecutionContext current = GetCodeEnvContext();

            object retVal = null;
            string cmpScopeKey = GetWorkScopeKey();
            IDictionary<string, object> scopeResult = cmpScopeKey == null ? current.Items : current.GetValue<Dictionary<string, object>>(cmpScopeKey);

            if (scopeResult != null && scopeResult.ContainsKey(itemName))
                retVal = scopeResult[itemName];

            if (retVal != null)
                return (T)Convert.ChangeType(retVal, typeof(T));

            return default(T);
        }

        public string GetWorkScopeKey()
        {
            string targetKey = null;
            switch (Mode)
            {
                case ScopeMode.Inherit:
                    if (_ContainerScope != null)
                        targetKey = _ContainerScope.GetWorkScopeKey();
                    break;
                case ScopeMode.CreateNew:
                    targetKey = GetCurrentScopeKey();
                    break;
                case ScopeMode.Top:
                default:
                    break;
            }
            return targetKey;
        }

        protected string GetCurrentScopeKey()
        {
            return "ExtractScope#" + ScopeId;
        }

        public Dictionary<string, object> GetContainerObjectIteration()
        {
            ExecutionContext current = GetCodeEnvContext();
            string popKey = GetWorkScopeKey();
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
            if (Mode != ScopeMode.CreateNew)
                return ScopeDeepth - 1;

            ExecutionContext current = GetCodeEnvContext();
            string cmpScopeKey = GetWorkScopeKey();
            Dictionary<string, object> scopeObj = current.GetValue<Dictionary<string, object>>(cmpScopeKey);
            if (scopeObj == null)
            {
                Dictionary<string, object> thisPopObject = current.GetValue<Dictionary<string, object>>(GetCurrentScopeKey());
                if (thisPopObject == null)
                    return ScopeDeepth;
                else
                    scopeObj = thisPopObject;
            }

            int rightScopeDeepth = ScopeDeepth - 1;
            string popKey = (_ContainerScope != null) ? _ContainerScope.GetWorkScopeKey() : string.Format("ExtractScope#{0}", rightScopeDeepth);
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

        public static string NotNullStringOf(IDictionary<string, object> resultDict, string itemName)
        {
            if (resultDict.ContainsKey(itemName) && resultDict[itemName] != null)
            {
                return resultDict[itemName].ToString();
            }
            return string.Empty;
        }

        public static void MergingScopeObjectWith(IDictionary<string, object> toMergin, IDictionary<string, object> target, bool overWrite = false)
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
                if (current.Items[item] is IDictionary<string, object>)
                {
                    var crtDict = current.Items[item] as IDictionary<string, object>;
                    if (crtDict != null)
                        MergingScopeObjectWith(crtDict, rootObject, true);
                }
                else
                {
                    if (!rootObject.ContainsKey(item))
                        rootObject[item] = current.Items[item];
                }
            }
            return rootObject;
        }

    }

    public enum ScopeMode : int
    {
        /// <summary>
        /// 继承
        /// </summary>
        Inherit = 0,
        /// <summary>
        /// 创建新的运行域
        /// </summary>
        CreateNew = 1,
        /// <summary>
        /// 顶级运行域（0级）
        /// </summary>
        Top = 2
    }

}
