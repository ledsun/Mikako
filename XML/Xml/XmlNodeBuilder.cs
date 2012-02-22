using System;
using System.Collections.Generic;
using System.Xml;

namespace Ledsun.Xml
{
    /// <summary>
    /// 属性つきのXmlNodeを生成するヘルパ
    /// </summary>
    public class XmlNodeBuilder
    {
        private readonly XmlNode _node;
        private readonly XmlDocument _doc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nodeName">生成するノードの名前</param>
        /// <param name="doc">操作対象のDocumentオブジェクト</param>
        public XmlNodeBuilder(string nodeName, XmlDocument doc)
        {
            _doc = doc;
            _node = doc.CreateElement(nodeName);
        }

        /// <summary>
        /// 現在生成中のノードに属性を追加する
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="value">属性値</param>
        public void AddAttribute(string name, string value)
        {
            if (String.IsNullOrEmpty(value)) return;

            XmlAttribute attribute = _doc.CreateAttribute(name);
            attribute.Value = value;
            _node.Attributes.Append(attribute);
        }

        public void AddAttributes(Dictionary<string, string> attrs)
        {
            foreach (KeyValuePair<string, string> i in attrs)
                AddAttribute(i.Key, i.Value);
        }

        public static implicit operator XmlNode(XmlNodeBuilder value)
        {
            return value._node;
        }
    }
}