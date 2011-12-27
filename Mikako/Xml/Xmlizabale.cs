using System.Xml;

namespace Com.Luxiar.Mikako.Xml
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
