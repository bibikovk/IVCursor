using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaRule
    {
        #region Variables
        protected string name = "";
        protected string param = "";
        protected string value = "";
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Param
        {
            get { return this.param; }
            set { this.param = value; }
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region Constructors
        public SXSchemaRule() { }

        public SXSchemaRule(SXNode node)
        {
            if (node == null) return;

            this.Name = node.GetAttribute("Name");

            this.Param = ((node.GetNode("Param") == null) ? "" : node.GetNode("Param").Value);
            this.Value = ((node.GetNode("Value") == null) ? "" : node.GetNode("Value").Value);
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");

            result.SetAttribute("Name", this.Name);

            if (this.Param != null && this.Param != "")
                result.AddNode("Param", this.Param);

            if (this.Value != null && this.Value != "")
                result.AddNode("Value", this.Value);

            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Rule"); }
        #endregion
    }

    public class SXSchemaRuleList : List<SXSchemaRule>
    {
        public string this[string name]
        {
            get
            {
                foreach (SXSchemaRule r in this)
                    if (r.Name.Trim().ToLower() == name.Trim().ToLower())
                        return r.Value;
                return "";
            }
        }

        #region Constructor
        public SXSchemaRuleList() : base() { }
        public SXSchemaRuleList(List<SXSchemaRule> rules) : base(rules) { }

        public SXSchemaRuleList(SXNode node)
        {
            if (node == null) return;

            foreach (SXNode n in node.Nodes)
                    this.Add(new SXSchemaRule(n));
        }
        #endregion

        #region Functions
        public void Add(string name, string value)
        { this.Add(new SXSchemaRule() { Name = name, Value = value }); }

        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            foreach (SXSchemaRule r in this)
                result.Nodes.Add(r.GetNode(result, "Rule"));
            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Rules"); }
        #endregion
    }
}
