using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

namespace Com.Luxiar.Mikako.Util
{
    /// <summary>
    /// WebService�Ńf�[�^����M����ƁAVisualStudio�����������N���X�ɒu���������Ă��܂��܂��B
    /// ���X�̃N���X�����L���C�u�����ŎQ�Ƃł���ꍇ�͌��N���X�̃C���X�^���X�ɖ߂������B
    /// XMLSerializer���g���Č��N���X�̃C���X�^���X�ɖ߂��܂��B
    /// </summary>
    public class WebObjectReverter<LibraryClass, WebserviceClass>
    {
        public LibraryClass Revert(WebserviceClass wc)
        {
            return RevertToLibraryClass(wc);
        }

        public List<LibraryClass> Revert(WebserviceClass[] wcs)
        {
            List<LibraryClass> lcs = new List<LibraryClass>();
            foreach (WebserviceClass wc in wcs)
            {
                lcs.Add(RevertToLibraryClass(wc));
            }
            return lcs;
        }

        private LibraryClass RevertToLibraryClass(WebserviceClass wc)
        {
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb))
                new XmlSerializer(typeof(WebserviceClass)).Serialize(writer, wc);

            using (MemoryStream mm = new MemoryStream(Encoding.Unicode.GetBytes(sb.Replace(" xmlns=\"http://tempuri.org/\"", "").ToString())))
                return (LibraryClass)new XmlSerializer(typeof(LibraryClass)).Deserialize(mm);
        }
    }

    #region test
    //Generic�N���X�ɂ�InnerTest�p�^�[���͎g���Ȃ��B
    [TestFixture]
    public class WebObjectReverter_TEST
    {
        [Test]
        public void �P��N���X�̕���()
        {
            WebClass.SampleClass wc = new WebClass.SampleClass();
            wc.Code = "AAAA";
            LibraryClass.SampleClass lc = new WebObjectReverter<LibraryClass.SampleClass, WebClass.SampleClass>().Revert(wc);
            Assert.That(lc.Code, Is.EqualTo("AAAA"));
        }

        [Test]
        public void �N���X�z���List�ɕ���()
        {
            WebClass.SampleClass[] array = new WebClass.SampleClass[3];
            array[0] = new WebClass.SampleClass();
            array[0].Code = "AAAA";
            array[1] = new WebClass.SampleClass();
            array[1].Code = "BBBB";
            array[2] = new WebClass.SampleClass();
            array[2].Code = "CCCC";

            List<LibraryClass.SampleClass> list = new WebObjectReverter<LibraryClass.SampleClass, WebClass.SampleClass>().Revert(array);
            Assert.That(list[0].Code, Is.EqualTo("AAAA"));
            Assert.That(list[1].Code, Is.EqualTo("BBBB"));
            Assert.That(list[2].Code, Is.EqualTo("CCCC"));
        }
    }
    #endregion
}

#region test�p�̃N���X��`
namespace LibraryClass
{
    public class SampleClass
    {
        public string Code;
    }
}

namespace WebClass
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://tempuri.org/")]
    public class SampleClass
    {
        private string codeField;

        /// <remarks/>
        public string Code
        {
            get { return this.codeField; }
            set { this.codeField = value; }
        }
    }
}
#endregion