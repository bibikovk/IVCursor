using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaVariable
    {
        #region Variables
        protected string name = "";
        protected SXSchemaAddress address = new SXSchemaAddress();
        #endregion

        #region Properties
        public string Name
        { get { return this.name; } set { this.name = value; } }

        public SXSchemaAddress Address
        { get { return this.address; } set { this.address = value; } }
        #endregion

        #region Constructor
        public SXSchemaVariable() { }

        public SXSchemaVariable(string name, string position)
        {
            this.Name = name;
            this.Address = new SXSchemaAddress(position);
        }

        public SXSchemaVariable(SXNode node)
        {
            if (node == null)return;

            this.Name = node.GetAttribute("Name");

            if (node.GetNode("Address") != null)
                this.Address = new SXSchemaAddress(node.GetNode("Address"));
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            result.SetAttribute("Name", this.Name);
            if (this.Address != null)
                result.Nodes.Add(this.Address.GetNode(result, "Address"));
            return result;
        }
        #endregion
    }

    public class SXSchemaVariableList : List<SXSchemaVariable>
    {
        #region Constructor
        public SXSchemaVariableList() : base() { }
        public SXSchemaVariableList(List<SXSchemaVariable> values) : base(values) { }

        public SXSchemaVariableList(SXNode node)
        {
            if (node == null) return;

            foreach (SXNode n in node.Nodes)
                this.Add(new SXSchemaVariable(n));
        }
        #endregion

        #region Functions
        public void Add(string name, string position)
        { this.Add(new SXSchemaVariable(name, position)); }

        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            foreach (SXSchemaVariable v in this)
                result.Nodes.Add(v.GetNode(result, "Variable"));
            return result;
        }
        #endregion
    }

}
