using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using IVCursor.Classes.Mappings;
using SXShared.Classes;

namespace IVCursor.Classes.Cursor
{
    public class SXCursorXML : SXCursor
    {
        #region Variables
        protected XPathDocument document = null;
        protected XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
        protected XPathNodeIterator iterator = null;
        #endregion

        #region Properties
        public XPathDocument Document
        {get{return this.document;}}

        protected XmlNamespaceManager Manager
        {
            get { return this.manager; }
            set { this.manager = value; }
        }

        protected XPathNodeIterator Iterator
        {
            get { return this.iterator; }
            set { this.iterator = value; }
        }

        public override SXSchema.SXMappingType MappingType
        { get { return SXSchema.SXMappingType.XML; } }
        #endregion

        #region Constructor
        public SXCursorXML(MemoryStream memory)
        {
            try
            {
                this.document = new XPathDocument(memory);
            }
            catch
            {
                this.document = null;
                this.Manager = null;
                this.Iterator = null;
            }
        }
        #endregion

        #region Functions
        protected XPathNodeIterator CreateIterator(string xpath)
        { return ((this.Document == null) ? null : this.CreateIterator(xpath, this.Document.CreateNavigator())); }

        protected XPathNodeIterator CreateIterator(string xpath, XPathNavigator navigator)
        {
            try
            {
                if (this.Document == null || xpath == null || xpath.Trim() == "") return null;

                XPathNavigator nav = ((navigator == null) ? this.Document.CreateNavigator() : navigator);
                
                XPathExpression expression = nav.Compile(xpath.Trim());
                if (this.Manager != null) expression.SetContext(this.Manager);

                return nav.Select(expression);
            }
            catch { return null; }
        }

        protected override string GetValue(string uri)
        {
            try
            {
                if (uri == null || uri.Trim() == "")
                    return "";

                string query = uri.Trim();

                if (this.Iterator == null || this.Iterator.Current == null)
                    return "";

                if (query.StartsWith("'") && query.EndsWith("'"))
                    return ((query.Length <= 2) ? "" : query.Substring(1, query.Length - 2));

                XPathNodeIterator cur = this.CreateIterator(query, this.Iterator.Current);
                if (cur == null || !cur.MoveNext())
                    return "";

                return ((cur == null || cur.Current == null) ? "" : cur.Current.Value);
            }
            catch { return ""; }
        }

        public override bool MoveNext()
        {
            if (this.Schema == null || this.Schema.Range == null)
                return false;

            if (this.Iterator == null)
                return false;

            return this.Iterator.MoveNext();
        }

        protected override void SetMapping(SXSchema mapping)
        {
            base.SetMapping(mapping);

            this.Iterator = null;
            this.Manager = new XmlNamespaceManager(new NameTable());

            if (this.Schema == null) return;

            this.Manager = new XmlNamespaceManager(new NameTable());
            foreach (SXSchemaRule rule in mapping.Rules)
                if (rule.Name.Trim().ToLower() == "xmlns")
                    this.Manager.AddNamespace(rule.Param, rule.Value);

            if (this.Schema.Range != null)
                this.Iterator = this.CreateIterator(this.Schema.Range.StartPosition.Trim());
        }

        public override bool CheckCondition()
        {
            if (this.Schema == null || this.Schema.Conditions == null) return true;

            try
            {
                foreach (SXSchemaCondition cond in this.Schema.Conditions)
                {
                    XPathNodeIterator cond_iter = this.CreateIterator(cond.Address.Uri);
                    if (cond_iter == null || !cond_iter.MoveNext() || cond_iter.Current == null || cond_iter.Current.Value.Trim().ToLower() != cond.Value.Trim().ToLower())
                        return false;
                }

                return true;
            }
            catch { return false; }
        }
        #endregion
    }
}
