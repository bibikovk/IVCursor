using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaCondition
    {
        #region Variables
        protected SXSchemaAddress address = new SXSchemaAddress();
        protected string value = "";
        #endregion

        #region Properties
        public SXSchemaAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region Constructors
        public SXSchemaCondition() { }

        public SXSchemaCondition(string position, string value)
        {
            this.Address = new SXSchemaAddress(position);
            this.Value = value;
        }

        public SXSchemaCondition(SXNode node)
        {
            if (node == null)return;

            if (node.GetNode("Row") != null && node.GetNode("Field") != null)
                this.Address = new SXSchemaAddress() { Uri = "R" + node.GetNode("Row").Value + "C" + node.GetNode("Field").Value };
            else if (node.GetNode("Row") != null && node.GetNode("Column") != null)
                this.Address = new SXSchemaAddress() { Uri = "R" + node.GetNode("Row").Value + "C" + node.GetNode("Column").Value };
            else if (node.GetNode("Address") != null)
                this.Address = new SXSchemaAddress(node.GetNode("Address"));

            this.Value = ((node.GetNode("Value") == null) ? "" : node.GetNode("Value").Value);
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            if (this.Address != null)
                result.Nodes.Add(this.Address.GetNode(result, "Address"));
            result.AddNode("Value", this.Value);
            return result;
        }
        #endregion
    }

    public class SXSchemaConditionList : List<SXSchemaCondition>
    {
        #region Constructor
        public SXSchemaConditionList() : base() { }
        public SXSchemaConditionList(List<SXSchemaCondition> values) : base(values) { }

        public SXSchemaConditionList(SXNode node)
        {
            if (node == null) return;

            foreach (SXNode n in node.Nodes)
                this.Add(new SXSchemaCondition(n));
        }
        #endregion

        #region Functions
        public void Add(string position, string value)
        { this.Add(new SXSchemaCondition(position, value)); }

        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            foreach (SXSchemaCondition v in this)
                result.Nodes.Add(v.GetNode(result, "Condition"));
            return result;
        }
        #endregion
    }
}
