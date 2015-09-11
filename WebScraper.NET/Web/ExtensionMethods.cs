using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebScraper.NET.Web
{
    public abstract class ExtensionMethods
    {
        public bool IsNull(object obj)
        {
            return obj == null;
        }
    }
}
