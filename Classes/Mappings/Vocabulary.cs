using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;
using SXShared.Classes.SXVocabularies;

namespace IVCursor.Classes.Mappings
{
    public class SXSchemaVocabulary
    {
        #region Variables
        protected string name = "";
        protected List<SXType> types = new List<SXType>();
        #endregion

        #region Properties
        public SXType this[string title]
        {
            get
            {
                foreach (SXType t in this.Types)
                    if (t.Title.Trim().ToLower() == title.Trim().ToLower())
                        return t;
                return null;
            }
        }

        public string Name
        {get{return this.name;}
            set{this.name = value;}
        }

        public List<SXType> Types
        {
            get { return this.types; }
            set { this.types = value; }
        }
        #endregion

        #region Constructor
        public SXSchemaVocabulary() { }

        public SXSchemaVocabulary(string name)
        { this.name = name; }

        public SXSchemaVocabulary(SXNode node)
        {
            if (node == null) return;

            this.Name = node.GetAttribute("Name");

            for (int i = 0; i < node.Nodes.Count; i++)
                if (node.Nodes[i].Name.Trim().ToLower() == "type")
                    this.Types.Add(new SXType(i + 1, node.Nodes[i].GetAttribute("name"), node.Nodes[i].GetAttribute("title")));
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            result.SetAttribute("Name", this.Name);

            foreach (SXType t in this.Types)
            {
                SXNode n = new SXNode(result, "type", "");
                n.SetAttribute("name", t.Identifier);
                n.SetAttribute("title", t.Title);
                result.Nodes.Add(n);
            }

            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Vocabulary"); }
        #endregion
    }

    public class SXSchemaVocabularyList : List<SXSchemaVocabulary>
    {
        #region Properties
        public SXSchemaVocabulary this[string name]
        {
            get
            {
                foreach (SXSchemaVocabulary v in this)
                    if (v.Name.Trim().ToLower() == name.Trim().ToLower())
                        return v;
                return null;
            }
        }
        #endregion

        #region Constructor
        public SXSchemaVocabularyList() : base() { }
        public SXSchemaVocabularyList(List<SXSchemaVocabulary> vocs) : base(vocs) { }

        public SXSchemaVocabularyList(SXNode node)
        {
            if (node == null) return;

            foreach (SXNode n in node.Nodes)
                this.Add(new SXSchemaVocabulary(n));
        }
        #endregion

        #region Functions
        public SXNode GetNode(SXNode parent, string name)
        {
            SXNode result = new SXNode(parent, name, "");
            foreach (SXSchemaVocabulary v in this)
                result.Nodes.Add(v.GetNode(result, "Vocabulary"));
            return result;
        }

        public SXNode GetNode(SXNode parent)
        { return this.GetNode(parent, "Vocabularies"); }
        #endregion
    }
}
