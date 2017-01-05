using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{
    public sealed class SpiderAgent
    {
        public static void Execute(ExtractTaskConfig task)
        {
            ExtractTaskDocument taskDoc = ExtractTaskDocument.FromExtractRule(task.InvokeArguments);
            Document rootDoc = taskDoc.GetStartupDocument();

        fetchPageData:
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {
                ExtractDocumentReport report = taskDoc.ExtractWith(rootDoc);
                if (!report.IsSuccess())
                {
                    throw report.ExtractExcetpion;
                }
                else
                {
                    if (task.DataReceiver is ISimpleObjectReceiver)
                    {
                        #region 简单对象
                        ISimpleObjectReceiver receriver = task.DataReceiver as ISimpleObjectReceiver;
                        receriver.Accept(report.CurrentExtractResult);
                        #endregion
                    }
                    else if (task.DataReceiver is IObjectListReceiver)
                    {
                        #region 仅集合对象
                        IObjectListReceiver rev2 = task.DataReceiver as IObjectListReceiver;
                        var allKeys = report.CurrentExtractResult.Keys;
                        foreach (string item in allKeys)
                        {
                            if (report.CurrentExtractResult[item] is List<Dictionary<string, object>>)
                            {
                                List<Dictionary<string, object>> allResultList = (List<Dictionary<string, object>>)report.CurrentExtractResult[item];
                                rev2.RecordCount = allResultList.Count;
                                allResultList.ForEach(ed =>
                                {
                                    rev2.Send(ed);
                                });
                            }
                        }
                        #endregion
                    }
                    else if (task.DataReceiver is IMixedDataReceiver)
                    {
                        #region 符合对象
                        IMixedDataReceiver rev3 = task.DataReceiver as IMixedDataReceiver;
                        MixedExtractData data = new MixedExtractData();
                        Dictionary<string, object> temSummary = new Dictionary<string, object>();
                        Dictionary<string, List<Dictionary<string, object>>> nameDict = new Dictionary<string, List<Dictionary<string, object>>>();
                        var allKeys = report.CurrentExtractResult.Keys;
                        foreach (string item in allKeys)
                        {
                            if (report.CurrentExtractResult[item] is List<Dictionary<string, object>>)
                            {
                                List<Dictionary<string, object>> allResultList = (List<Dictionary<string, object>>)report.CurrentExtractResult[item];
                                nameDict.Add(item, allResultList);
                            }
                            else
                            {
                                temSummary.Add(item, report.CurrentExtractResult[item]);
                            }
                        }

                        data.Summary = temSummary;
                        data.NamedListObject = nameDict;
                        rev3.Accept(data);
                        #endregion
                    }

                    #region 持续抽取（重复）
                    ExtractPagerNode node = taskDoc.GetPagerNode();
                    if (node != null)
                    {
                        List<string> nextUrls = node.GetPageUrlList();
                        if (node.PageListType == PagerType.ByNext)
                        {
                            if (nextUrls.Any())
                            {
                                taskDoc.DocumentUrl = nextUrls[0];
                                rootDoc = taskDoc.GetDocumentByUrl(taskDoc.DocumentUrl);
                                goto fetchPageData;
                            }
                        }
                        else
                        {
                            string currentDocUrl = taskDoc.EntryUrl.GetUrl();
                        }
                    }
                    #endregion
                }

            }

        }
    }

}
