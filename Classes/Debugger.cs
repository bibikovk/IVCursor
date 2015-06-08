using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICCursor.Classes
{
    public class SXImportDebugger
    {
        public delegate void ValueDefineLog(string address, string value);

        static public event ValueDefineLog OnValueDefine;

        static public void ExValueDefineLog(string address, string value)
        {
            if (SXImportDebugger.OnValueDefine != null)
                SXImportDebugger.OnValueDefine(((address == null) ? "NULL" : address), ((value == null) ? "NULL" : value));
        }
    }
}
