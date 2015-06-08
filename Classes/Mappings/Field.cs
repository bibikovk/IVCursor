using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaField
    {
        #region Variables
        protected string name = "";
        protected string title = "";
        protected string type = "";
        protected string default_value = "";

        protected SXSchemaAddress address = new SXSchemaAddress();
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public string Default
        {
            get { return this.default_value; }
            set { this.default_value = value; }
        }

        public SXSchemaAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }
        #endregion

        #region Constructor
        public SXSchemaField() { }

        public SXSchemaField(string name, string uri)
        {
            this.name = name;
            this.address = new SXSchemaAddress(uri);
        }

        public SXSchemaField(SXNode node)
        {
            if (node == null) return;

            this.Name = node.GetAttribute("Name");
            this.Title = node.GetAttribute("Title");
            this.Type = node.GetAttribute("Type");
            this.Default = node.GetAttribute("Default");

            this.Address = new SXSchemaAddress(node.GetNode("Address"));
        }

        public SXSchemaField(SXSettings settings)
        {
            this.Name = settings.Name;
            this.Title = settings.Title;
            this.Type = settings.Type;
            this.Default = settings["default"].ToString();


            string uri = "";
            foreach (SXSettingsValue sv in settings.SettingsValues)
                if (sv.Name.Trim().ToLower() == "column" || sv.Name.Trim().ToLower() == "col")
                {
                    //this.Address.Add(new SXSchemaAddress() { Uri = sv["columnindex"].ToString(), Prefix = sv["columnprefix"].ToString() });

                    string cur_prefix = ((sv["columnprefix"].ToString() == "") ? "" : ("'" + sv["columnprefix"].ToString() + "'"));
                    string cur_column = sv["columnindex"].ToString();

                    string cur_uri = ((cur_prefix == "") ? cur_column : ((cur_column == "") ? cur_prefix : (cur_prefix + " + " + cur_column)));

                    uri = ((uri == "") ? cur_uri : ((cur_uri == "") ? uri : (uri + " + " + cur_uri)));
                }

            int local_column_index = 0;
            if (uri == "" && settings["columnindex"].ToString().Trim() != "" && Int32.TryParse(settings["columnindex"].ToString(), out local_column_index))
            {
                //this.Address.Add(new SXSchemaAddress() { Uri = local_column_index.ToString(), Prefix = settings["columnprefix"].ToString() });

                uri = ((settings["columnprefix"].ToString() == "") ? "" : ("'" + settings["columnprefix"].ToString() + "' + ")) + local_column_index.ToString();
            }

            this.Address = new SXSchemaAddress(uri);
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");

            result.SetAttribute("Name", this.Name);
            result.SetAttribute("Title", this.Title);
            result.SetAttribute("Type", this.Type);
            result.SetAttribute("Default", this.Default);

            if (this.Address != null)
                result.Nodes.Add(this.Address.GetNode(result, "Address"));

            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Field"); }
        #endregion
    }

    public class SXSchemaFieldList : List<SXSchemaField>
    {
        #region Properties
        public SXSchemaField this[string name]
        {
            get
            {
                foreach (SXSchemaField f in this)
                    if (f.Name.Trim().ToLower() == name.Trim().ToLower())
                        return f;
                return null;
            }
        }
        #endregion

        #region Constructor
        public SXSchemaFieldList() : base() { }
        public SXSchemaFieldList(List<SXSchemaField> fields) : base(fields) { }

        public SXSchemaFieldList(SXNode node)
        {
            if (node == null) return;

            foreach (SXNode n in node.Nodes)
                this.Add(new SXSchemaField(n));
        }

        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            foreach (SXSchemaField f in this)
                result.Nodes.Add(f.GetNode(result, "Field"));
            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Fields"); }
        #endregion
    }
}
