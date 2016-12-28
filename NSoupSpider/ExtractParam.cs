using System;
using System.Xml.Serialization;

namespace NSoupSpider
{
    [Serializable]
    [XmlRoot(ElementName = "param")]
    public class ExtractParam
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "scope")]
        public ParamScope Scope { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [Serializable]
    public enum ParamScope : int
    {
        arguments = 0,
        workScope = 1
    }
}
