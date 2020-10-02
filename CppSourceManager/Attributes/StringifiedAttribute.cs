using System;

namespace CppSourceManager.Attributes
{
    /// <summary>
    /// This is the base class for stringified attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StringifiedAttribute : Attribute
    {
        public string StringifedValue { get; private set; }

        public StringifiedAttribute(string strValue)
        {
            StringifedValue = strValue;
        }
    }
}
