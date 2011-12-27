using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Com.Luxiar.Mikako.Xml
{
    public class XMLMaker
    {
        //XMLの元となるデータを保持するフィールド
        private readonly string _rootId;
        private readonly string _rootName;
        private readonly Dictionary<string, XmlMaterial> _materials = new Dictionary<string, XmlMaterial>();
        public readonly Dictionary<string, string> RootAttrs = new Dictionary<string, string>();

        public XMLMaker(string rootName) : this(rootName, "") { }
        public XMLMaker(string rootName, string rootId)
        {
            _rootName = rootName;
            _rootId = rootId;
        }

        public XMLMaker.XmlMaterial this[string key]
        {
            get
            {
                return _materials[key];
            }
            set
            {
                if (key == _rootId) throw new ArgumentException("ルートのIDを指定したノードの追加はできません。key: " + key);
                _materials[key] = value;
            }
        }

        public string Add(XmlMaterial child)
        {
            Random rnd = new Random();
            string key;

            while (true)
            {
                key = rnd.Next(100000).ToString();
                if (!_materials.ContainsKey(key))
                    break;
            }

            _materials[key] = child;
            return key;
        }

        public static implicit operator XmlDocument(XMLMaker value)
        {
            return value.Xml;
        }

        private XmlDocument Xml
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                XmlNodeBuilder root = MakeRoot(doc);
                MakeChildrenOfParent(root, _rootId, doc);

                doc.AppendChild(root);
                return doc;
            }
        }

        private XmlNodeBuilder MakeRoot(XmlDocument workXml)
        {
            XmlNodeBuilder root = new XmlNodeBuilder(_rootName, workXml);
            root.AddAttributes(RootAttrs);
            return root;
        }

        private void MakeChildrenOfParent(XmlNode parentNode, string parentId, XmlDocument doc)
        {
            foreach (KeyValuePair<string, XmlMaterial> i in _materials)
            {
                if (i.Value.ParentKey == parentId)
                {
                    XmlNode child = i.Value.ToXml(doc);
                    parentNode.AppendChild(child);
                    MakeChildrenOfParent(child, i.Key, doc);
                }
            }
        }

        public class XmlMaterial
        {
            public readonly string ParentKey;
            public readonly string Name;
            public readonly Dictionary<string, string> Attrs = new Dictionary<string, string>();

            public XmlMaterial(string name) : this(name, "") { }
            public XmlMaterial(string name, string parentKey)
            {
                if (name == null) throw new ArgumentNullException("name");
                ParentKey = parentKey;
                Name = name;
            }

            public XmlNode ToXml(XmlDocument doc)
            {
                XmlNodeBuilder node = new XmlNodeBuilder(Name, doc);
                node.AddAttributes(Attrs);
                return node;
            }
        }

        #region test
        [TestFixture]
        public class Test
        {
            const string SIMPLE_XML = "<root><child1 /></root>";
            const string ATTRIBUTE_XML = "<root><child1 hogehoge=\"100\" /></root>";
            const string GRANDCHILD_XML = "<root><child1 hogehoge=\"100\"><child2 /></child1><child3 hogehoge=\"100\" /></root>";

            [Test]
            public void Dictionaryぽく使えます()
            {
                XMLMaker maker = new XMLMaker("root");
                maker["1"] = new XmlMaterial("child1");
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(SIMPLE_XML));

                maker["1"].Attrs["hogehoge"] = "100";
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(ATTRIBUTE_XML));

                maker["2"] = new XmlMaterial("child2", "1");
                maker["3"] = new XmlMaterial("child3");
                maker["3"].Attrs["hogehoge"] = "100";
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(GRANDCHILD_XML));
            }

            [Test]
            public void Addもできます()
            {
                XMLMaker maker = new XMLMaker("root");
                string key = maker.Add(new XmlMaterial("child1"));
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(SIMPLE_XML));

                maker[key].Attrs["hogehoge"] = "100";
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(ATTRIBUTE_XML));

                maker.Add(new XmlMaterial("child2", key));
                key = maker.Add(new XmlMaterial("child3"));
                maker[key].Attrs["hogehoge"] = "100";
                Assert.That(maker.Xml.InnerXml, Is.EqualTo(GRANDCHILD_XML));
            }


            [Test]
            public void Rootの属性を設定できます()
            {
                XMLMaker maker = new XMLMaker("root");
                maker.RootAttrs["name"] = "ルート";
                maker.RootAttrs["code"] = "コード";
                Assert.That(maker.Xml.InnerXml, Is.EqualTo("<root name=\"ルート\" code=\"コード\" />"));
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "ルートのIDを指定したノードの追加はできません。key: root")]
            public void 無限ループするのでRootと同じIDは設定できません()
            {
                XMLMaker maker = new XMLMaker("root", "root");
                maker["root"] = new XmlMaterial("child", "root");
                XmlDocument a = maker.Xml;
            }
        }
        #endregion //test
    }
}
