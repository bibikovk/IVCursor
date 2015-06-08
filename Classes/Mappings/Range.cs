using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaRange
    {
        #region Variables
        protected string start_position = "";

        protected SXSchemaCondition condition = new SXSchemaCondition();
        //protected string check_field = "";
        //protected string check_value = "";
        #endregion

        #region Properties
        public string StartPosition
        {
            get { return this.start_position; }
            set { this.start_position = value; }
        }

        public SXSchemaCondition Condition
        { get { return this.condition; } set { this.condition = value; } }

        //public string CheckField
        //{
        //    get { return this.check_field; }
        //    set { this.check_field = value; }
        //}

        //public string CheckValue
        //{
        //    get { return this.check_value; }
        //    set { this.check_value = value; }
        //}
        #endregion

        #region Constructors
        public SXSchemaRange() { }

        public SXSchemaRange(SXNode node)
        {
            if (node == null) return;

            this.StartPosition = ((node.GetNode("StartPosition") == null) ? "" : node.GetNode("StartPosition").Value);
            if (node.GetNode("Condition") != null)
                this.Condition = new SXSchemaCondition(node.GetNode("Condition"));
            else
            {
                this.Condition = new SXSchemaCondition();
                condition.Address = new SXSchemaAddress(((node.GetNode("CheckField") == null) ? "" : node.GetNode("CheckField").Value));
                condition.Value = ((node.GetNode("CheckValue") == null) ? "" : node.GetNode("CheckValue").Value);
                //this.CheckField = ((node.GetNode("CheckField") == null) ? "" : node.GetNode("CheckField").Value);
                //this.CheckValue = ((node.GetNode("CheckValue") == null) ? "" : node.GetNode("CheckValue").Value);
            }
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            result.AddNode("StartPosition", this.StartPosition);
            result.Nodes.Add(this.Condition.GetNode(result, "Condition"));
            //result.AddNode("CheckField", this.CheckField);
            //result.AddNode("CheckValue", this.CheckValue);
            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Range"); }
        #endregion
    }
}
