using System.Xml;

namespace Ledsun.Xml
{
    abstract public class Xmlizabale
    {
        public static implicit operator XmlDocument(Xmlizabale value)
        {
            return value.Xml;
        }

        abstract protected XmlDocument Xml { get; }
    }
}
