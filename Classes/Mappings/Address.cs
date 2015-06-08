using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaAddress
    {
        #region Variables
        protected string uri = "";
        #endregion

        #region Properties
        public string Uri
        {
            get { return this.uri; }
            set { this.uri = value; }
        }
        #endregion

        #region Constructors
        public SXSchemaAddress() { }

        public SXSchemaAddress(string uri)
        { this.uri = uri; }

        public SXSchemaAddress(SXNode node)
        {
            if (node == null) return;

            if (node.GetNode("Uri") != null)
                this.Uri = node.GetNode("Uri").Value;
            else if (node.GetNode("Position") != null)
                this.Uri = node.GetNode("Position").Value;
            else
                this.Uri = node.Value;
        }
        #endregion

        #region Functions
        public override string ToString()
        { return this.Uri; }

        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            result.AddNode("Uri", this.Uri);
            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Address"); }
        #endregion
    }

}
