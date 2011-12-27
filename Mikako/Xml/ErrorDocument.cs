using System;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Com.Luxiar.Mikako.Xml
{
    /// <summary>
    /// WebService で返却される、エラーを表すドキュメントクラス
    /// </summary>
    public class ErrorDocument :Xmlizabale
    {
        private readonly Exception _error;

        /**
         * 例外オブジェクトで初期化するコンストラクタ
         */
        public ErrorDocument(Exception e)
        {
            if (e == null) throw new ArgumentNullException();
            _error = e;
        }

        public ErrorDocument(string message) : this(new Exception(message)) { }

        /**
         * エラーをXML形式にして返す
         */
        protected override XmlDocument Xml
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(BuildNode(doc));
                return doc;
            }
        }

        private XmlNode BuildNode(XmlDocument doc)
        {
            XmlNodeBuilder builder = new XmlNodeBuilder("error", doc);
            builder.AddAttribute("message", _error.Message);
            builder.AddAttribute("detail", _error.TargetSite == null ? _error.HelpLink : _error.HelpLink + "\n" + _error.TargetSite);
            builder.AddAttribute("source", _error.Source);
            builder.AddAttribute("stackTrace", _error.StackTrace);

            if (null != _error.InnerException)
            {
                ErrorDocument innerDoc = new ErrorDocument(_error.InnerException);
                ((XmlNode)builder).AppendChild(innerDoc.BuildNode(doc));
            }

            return builder;
        }

        #region test
        [TestFixture]
        public class Test
        {
            const string xmlString = "<error message=\"種類 'System.Exception' の例外がスローされました。\" />";
            private Exception e;

            [Test]
            public void コンストラクト()
            {
                Assert.That(new ErrorDocument(new Exception()), Is.Not.Null);

                try
                {
                    e = null;
                    Assert.That(new ErrorDocument(e), Is.Not.Null);
                    Assert.Fail("nullを入れたらだめです。");
                }
                catch (ArgumentNullException e)
                {
                    Assert.That(e.Message, Is.EqualTo("値を Null にすることはできません。"));
                }
            }

            [Test]
            public void ExcepitionクラスのXML化()
            {
                e = new Exception();
                Assert.That(new ErrorDocument(e).Xml.OuterXml, Is.EqualTo(xmlString));

                //メッセージにnullを指定
                e = new Exception(null);
                Assert.That(new ErrorDocument(e).Xml.OuterXml, Is.EqualTo(xmlString));

                e = new Exception(null, null);
                Assert.That(new ErrorDocument(e).Xml.OuterXml, Is.EqualTo(xmlString));

                e = new Exception("", new Exception());
                Assert.That(new ErrorDocument(e).Xml.OuterXml, Is.EqualTo("<error>" + xmlString + "</error>"));
            }

            [Test]
            public void 暗黙型変換()
            {
                e = new Exception();
                XmlDocument doc = new ErrorDocument(e);
                Assert.That(doc.OuterXml, Is.EqualTo(xmlString));
            }
        }
        #endregion
    }
}