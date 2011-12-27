using System.Xml;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Com.Luxiar.Mikako.Xml
{
    /// <summary>
    /// WebService で返却される、成功を表すドキュメントクラス
    /// </summary>
    public class SuccessDocument:Xmlizabale
    {
        protected override XmlDocument Xml
        {
            get
            {
                return new XMLMaker("success");
            }
        }

        #region test
        [TestFixture]
        public class Test
        {
            [Test]
            public void 生成されるXMLの確認()
            {
                Assert.That(new SuccessDocument().Xml.OuterXml, Is.EqualTo("<success />"));
            }

            [Test]
            public void 暗黙型変換の確認()
            {
                XmlDocument doc = new SuccessDocument();
                Assert.That(doc.OuterXml, Is.EqualTo("<success />"));
            }
        }
        #endregion
    }
}