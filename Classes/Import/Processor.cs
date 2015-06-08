using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICCursor.Classes.Import
{
    public class SXImportProcessor
    {
        #region Delegates
        public delegate void ErrorLog(string type, string identifier, string comment);
        public delegate string DefineContract(string input);
        public delegate bool DefineDublicates(string contract, string type, string number, DateTime date);
        #endregion

        #region Events
        public event ErrorLog OnErrorLog;
        public event DefineContract OnDefineContract;
        public event DefineDublicates OnDefineDublicates;
        #endregion

        #region Constructor
        public SXImportProcessor() { }
        #endregion

        #region Functions
        public void ExErrorLog(string type, string identifier, string comment)
        {
            if (this.OnErrorLog != null)
                this.OnErrorLog(type, identifier, comment);
        }

        public string ExDefineContract(string input)
        {
            if (this.OnDefineContract!= null)
                return this.OnDefineContract(input);
            return "";
        }

        public bool ExDefineDublicates(string contract, string type, string number, DateTime date)
        {
            if (this.OnDefineDublicates != null)
                return this.OnDefineDublicates(contract, type, number, date);
            return false;
        }
        #endregion
    }
}
