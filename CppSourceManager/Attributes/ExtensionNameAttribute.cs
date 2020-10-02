using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSourceManager.Attributes
{
    public class ExtensionNameAttribute : StringifiedAttribute
    {
        public ExtensionNameAttribute(string strValue) : base(strValue)
        {
        }
    }
}
