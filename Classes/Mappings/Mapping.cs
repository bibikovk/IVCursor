using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SXShared.Classes;
using SXShared.Classes.SXVocabularies;

namespace IVCursor.Classes.Mappings
{
    public class SXSchema
    {
        public enum SXMappingType { None, Excel2003, Excel2007, XML };

        #region Variables
        protected string name = "";
        protected string type = "";

        protected SXSchemaRange range = new SXSchemaRange();

        protected SXSchemaFieldList fields = new SXSchemaFieldList();

        protected SXSchemaRuleList rules = new SXSchemaRuleList();

        protected SXSchemaConditionList conditions = new SXSchemaConditionList();

        protected SXSchemaVocabularyList vocabularies = new SXSchemaVocabularyList();

        protected SXSchemaVariableList variables = new SXSchemaVariableList();
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public SXMappingType MappingType
        { get { return SXSchema.GetMappingType(this.Type); } }

        public SXSchemaRange Range
        {
            get { return this.range; }
            set { this.range = value; }
        }

        public SXSchemaFieldList Fields
        {
            get { return this.fields; }
            set { this.fields = value; }
        }

        public SXSchemaRuleList Rules
        {
            get { return this.rules; }
            set { this.rules = value; }
        }

        public SXSchemaConditionList Conditions
        {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        public SXSchemaVocabularyList Vocabularies
        {
            get { return this.vocabularies; }
            set { this.vocabularies = value; }
        }

        public SXSchemaVariableList Variables
        {
            get { return this.variables; }
            set { this.variables = value; }
        }
        #endregion

        #region Constructor
        public SXSchema(){}

        public SXSchema(string name, string type)
        {
            this.Name = name;
            this.type = type;
        }

        public SXSchema(SXNode node)
        {
            if (node == null) return;

            this.Name = node.GetAttribute("Name");
            this.Type = node.GetAttribute("Type");

            this.Range = new SXSchemaRange(node.GetNode("Range"));
            this.Fields = new SXSchemaFieldList(node.GetNode("Fields"));
            this.Rules = new SXSchemaRuleList(node.GetNode("Rules"));
            this.Conditions = new SXSchemaConditionList(node.GetNode("Conditions"));
            this.Vocabularies = new SXSchemaVocabularyList(node.GetNode("Vocabularies"));
            this.Variables = new SXSchemaVariableList(node.GetNode("Variables"));
        }


        public SXSchema(SXSettings settings)
        {
            if (settings == null) return;

            this.Name = settings.Name;
            this.Type = settings.Type;

            this.Range.StartPosition = settings["IndexStart"].ToString();
            this.Range.Condition = new SXSchemaCondition(settings["IndexLook"].ToString(), "");

            #region Conditions
            SXSettings node_conditions = settings.GetNode("Condition");
            if (node_conditions != null)
                foreach (SXSettingsValue sv in node_conditions.SettingsValues)
                    this.Conditions.Add("R" + sv["row"].ToString() + "C" + sv["col"].ToString(), sv.Value);
            #endregion

            #region Rules
            SXSettings node_rules = settings.GetNode("Rules");
            if (node_rules != null)
                foreach (SXSettingsValue sv in node_rules.SettingsValues)
                {
                    string param = "";
                    foreach (SXAttribute att in sv.Attributes)
                        if (att.Name.Trim().ToLower() != "name" && att.Value.Trim() != "")
                            param = att.Value;

                    this.Rules.Add(new SXSchemaRule() { Name = sv.Name, Value = sv.Value, Param = param });
                }
            #endregion

            #region Fields
            SXSettings node_fields = settings.GetNode("FieldsMapping");
            if (node_fields != null)
                foreach (SXSettings field_node in node_fields.Nodes)
                    this.Fields.Add(new SXSchemaField(field_node));
            #endregion

            #region Dictionary
            SXSettings node_vocabularies= settings.GetNode("Dictionary");
            if (node_vocabularies != null)
                foreach (SXSettings nv in node_vocabularies.Nodes)
                {
                    SXSchemaVocabulary voc = new SXSchemaVocabulary(nv.Name);
                    foreach (SXSettingsValue sv in nv.SettingsValues)
                        voc.Types.Add(new SXType(sv.ID, sv.Name, sv["Title"].ToString()));
                    this.Vocabularies.Add(voc);
                }
            #endregion
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            result.SetAttribute("Name", this.Name);
            result.SetAttribute("Type", this.Type);

            result.Nodes.Add(this.Range.GetNode(result, "Range"));
            result.Nodes.Add(this.Fields.GetNode(result, "Fields"));
            result.Nodes.Add(this.Rules.GetNode(result, "Rules"));
            result.Nodes.Add(this.Conditions.GetNode(result, "Conditions"));
            result.Nodes.Add(this.Vocabularies.GetNode(result, "Vocabularies"));
            result.Nodes.Add(this.Variables.GetNode(result, "Variables"));

            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Mapping"); }
        #endregion

        #region Statics
        static public SXMappingType GetMappingType(string type)
        {
            string cur_type = type.Trim().ToLower();
            if (cur_type == "excel" || cur_type == "xls" || cur_type == "excel2003")
                return SXMappingType.Excel2003;
            if (cur_type == "xlsx" || cur_type == "excel2007")
                return SXMappingType.Excel2007;
            if (cur_type == "xml")
                return SXMappingType.XML;
            return SXMappingType.None;
        }
        #endregion
    }

    public class SXSchemaList : List<SXSchema>
    {
        #region Variables
        protected SXSchemaRuleList rules = new SXSchemaRuleList();
        #endregion

        #region Properties
        public SXSchemaRuleList Rules
        {
            get { return this.rules; }
            set { this.rules = value; }
        }

        public SXSchema this[string name]
        {
            get
            {
                foreach (SXSchema m in this)
                    if (m.Name.Trim().ToLower() == name.Trim().ToLower())
                        return m;
                return null;
            }
        }
        #endregion

        #region Constructors
        public SXSchemaList() : base() { }
        public SXSchemaList(List<SXSchema> list) : base(list) { }

        public SXSchemaList(SXNode node)
        {
            if (node == null) return;

            SXSettings settings = new SXSettings(node);
            if (settings != null && settings.Nodes.Count > 0)
            {
                foreach (SXSettings item in settings.Nodes)
                    this.Add(new SXSchema(item));
            }
            else
            {
                this.Rules = new SXSchemaRuleList(node.GetNode("Rules"));

                foreach (SXNode item in node.Nodes)
                    if (item.Name.Trim().ToLower() == "mapping" || item.Name.Trim().ToLower() == "schema")
                        this.Add(new SXSchema(item));
            }
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");

            if (this.Rules != null)
                result.Nodes.Add(this.Rules.GetNode(result));

            foreach (SXSchema sch in this)
                result.Nodes.Add(sch.GetNode(result));

            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "MappingList"); }
        #endregion
    }
}
