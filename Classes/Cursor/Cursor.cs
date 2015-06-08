using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;
using SXShared.Classes.SXLexems;
using System.IO;
using IVCursor.Classes.Mappings;

namespace IVCursor.Classes.Cursor
{
    public class SXCursor
    {
        #region Variables
        protected SXEnvironment environment = new SXEnvironment();
        protected SXSchema schema = null;
        #endregion

        #region Properties
        public SXEnvironment Environment
        { get { return this.environment; } }

        public SXSchema Schema
        {
            get { return this.schema; }
            set { this.SetMapping(value); }
        }

        public bool IsSkip
        {
            get
            {
                if (this.Schema == null || this.Schema.Rules == null) return true;

                foreach (SXSchemaRule r in this.Schema.Rules)
                    if (r.Name.Trim().ToLower() == "skip")
                    {
                        if (this.Schema.Fields[r.Param] != null)
                        {
                            if (this.GetFieldString(r.Param).Trim().ToLower().Contains(r.Value.Trim().ToLower()))
                                return true;
                        }
                        else
                        {
                            if (this.GetValue(new SXSchemaAddress(r.Param)).Trim().ToLower() == r.Value.Trim().ToLower())
                                return true;
                        }
                    }

                return false;
            }
        }
        #endregion

        #region Virtual
        public virtual SXSchema.SXMappingType MappingType
        { get { return SXSchema.SXMappingType.None; } }

        protected virtual void SetMapping(SXSchema mapping)
        { this.schema = mapping; }

        protected virtual void ReloadEnvironment()
        {
            if (this.Environment == null)
                this.environment = new SXEnvironment();

            if (this.Schema != null)
                foreach (SXSchemaVariable v in this.Schema.Variables)
                    this.Environment.SetVariable(v.Name, this.GetValue(v.Address));

            this.Environment.FunctionExecute = this.OnLexemFunctionExecute;
        }

        protected virtual SXLexemVariable OnLexemFunctionExecute(SXLexemFunction func)
        { return null; }

        public virtual bool CheckCondition()
        {
            if (this.Schema == null || this.Schema.Conditions == null) return true;

            try
            {
                foreach (SXSchemaCondition cond in this.Schema.Conditions)
                    if (this.GetValue(cond.Address).Trim().ToLower() != cond.Value.Trim().ToLower())
                        return false;

                return true;
            }
            catch { return false; }
        }

        public virtual bool MoveNext()
        { return false; }

        protected virtual string GetCalc(SXExpression expression)
        {
            try
            {
                if (expression == null || !expression.Correct)
                    return "";

                SXLexemVariable result = expression.Calculate(this.Environment);

                if (result != null && result.Value != null && result.Value.Data != null)
                    return result.Value.Data.ToString();

                return "";
            }
            catch { return ""; }
        }

        protected virtual string GetValue(string address)
        {
            try
            {
                if (address == null || address.Trim() == "") return "";

                if (address.Trim().StartsWith("="))
                    return this.GetCalc(new SXExpression(address.Trim().Substring(1)));

                return "";
            }
            catch { return ""; }
        }

        protected virtual string GetValue(SXSchemaAddress address)
        {
            if (address == null || address.Uri == null || address.Uri.Trim() == "")
                return "";

            return this.GetValue(address.Uri.Trim());
        }
        
        public virtual string GetFieldString(string field_name)
        {
            SXSchemaField field = ((this.Schema == null) ? null : this.Schema.Fields[field_name]);

            if (field == null) 
                return "";

            if (field.Address == null || field.Address.Uri.Trim() == "")
                return field.Default;

            return this.GetValue(field.Address);
        }

        public virtual DateTime? GetFieldDate(string field_name)
        {
            return IVInterface.Classes.SXMethods.ParseDateTime(this.GetFieldString(field_name));
        }

        public virtual decimal? GetFieldNumber(string field_name)
        {
            return IVInterface.Classes.SXMethods.ParseDecimal(this.GetFieldString(field_name));
        }

        public virtual SXImportDuty CreateDuty()
        { return new SXImportDuty(this); }
        #endregion

        #region Statics
        static public SXCursor DefineCursor(MemoryStream memory, SXSchemaList schema_list)
        {
            if (memory == null || schema_list == null) return null;

            memory.Position = 0;

            SXCursor cursor = null;
            byte[] data = memory.ToArray();

            #region Excel2003
            if (cursor == null)
            {
                try //пытаемся установить курсор в формате Excel2003
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        ExcelLibrary.SpreadSheet.Workbook wb = ExcelLibrary.SpreadSheet.Workbook.Load(ms);
                        if (wb.Worksheets != null && wb.Worksheets.Count > 0)
                            cursor = new SXCursorExcel2003(wb.Worksheets[0]);
                    }
                }
                catch { cursor = null; }
            }
            #endregion

            #region Excel2007
            if (cursor == null)
            {
                try //пытаемся установить курсор в формате Excel2007
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        OfficeOpenXml.ExcelWorkbook wb = (new OfficeOpenXml.ExcelPackage(ms)).Workbook;
                        if (wb != null && wb.Worksheets != null && wb.Worksheets.Count > 0)
                            cursor = new SXCursorExcel2007(wb.Worksheets.First());
                    }
                }
                catch { cursor = null; }
            }
            #endregion

            #region XML
            if (cursor == null)
            {
                try //пытаемся установить курсор в формате XML
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        SXCursorXML xml_cursor = new SXCursorXML(ms);
                        if (xml_cursor != null && xml_cursor.Document != null)
                            cursor = xml_cursor;
                    }
                }
                catch { cursor = null; }
            }
            #endregion

            if (cursor == null || cursor.MappingType == SXSchema.SXMappingType.None)
                return null;

            #region Define Mapping from list for current Cursor and CursorType
            foreach (SXSchema schema in schema_list)
            {
                if (schema == null || schema.MappingType != cursor.MappingType)
                    continue;

                cursor.Schema = schema;

                if (cursor.CheckCondition())
                    return cursor;
                else
                    cursor.Schema = null;
            }
            #endregion

            return null;
        }
        #endregion
    }
}
