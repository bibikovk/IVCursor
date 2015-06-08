using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IVCursor.Classes.Cursor;
using IVCursor.Classes.Mappings;
using IVInterface.Classes;
using IVInterface.Classes.Duty;

namespace IVCursor.Classes.Import
{
    public class SXImporter
    {
        #region Variables
        protected List<SXImportDuty> duties = new List<SXImportDuty>();
        #endregion

        #region Properties
        public virtual List<SXImportDuty> Duties
        { get { return this.duties; } }

        public virtual SXSerialDutyList SerialDuties
        {
            get
            {
                SXSerialDutyList list = new SXSerialDutyList();
                list.Array.AddRange(this.Duties);
                return list;
            }
        }

        #endregion

        #region Constructors
        public SXImporter() { }
        #endregion

        #region Empties
        protected virtual void Error(string type, string identifier, string comment)
        {
        }

        protected virtual string DefineContract(string input)
        { return ""; }

        protected virtual bool IsDuplicate(SXImportDuty duty)
        { return false; }

        /// <summary>
        /// bkn
        /// </summary>
        public virtual void AgregateDuty()
        { }

        public virtual void InsertDuty()
        { }
        #endregion

        #region Virtuals
        protected void Error(Exception ex)
        { this.Error("Exception", "", ex.Message + Environment.NewLine + ex.StackTrace); }

        protected virtual SXImportDuty FindDuty(SXImportDuty duty)
        {
            try
            {
                if (duty == null || duty.Number == null || duty.Type == null || duty.Contract == null) return null;

                string search_contract = duty.Contract.Code.Trim().ToLower();
                string search_type = duty.Type.Name.Trim().ToLower();
                string search_number = duty.Number.Number.Trim().ToLower();
                string search_date = ((duty.Number.DateDuty == null) ? "null" : duty.Number.DateDuty.ToString("yyyyMMdd"));

                foreach (SXImportDuty d in this.Duties)
                {
                    try
                    {
                        if (d.Contract == null || d.Type == null || d.Number == null || d.Number.DateDuty == null || !d.Number.DateDuty.HasValue)
                            continue;

                        if (d.Number.Number != search_number || d.Number.DateDuty.ToString("yyyyMMdd") != search_date)
                            continue;

                        if (d.Type.Name.Trim().ToLower() != search_type || d.Contract.Code.Trim().ToLower() != search_contract)
                            continue;

                        return d;
                    }
                    catch { }
                }

                return null;
            }
            catch { return null; }
        }

        protected virtual bool CheckDuty(SXImportDuty duty)
        {
            try
            {
                //проверяем, что задолженность не пуста
                if (duty == null)
                    return false;

                //должен существовать номер задолженности
                if (duty.Number == null)
                {
                    this.Error("Error", "ImportProcess.Number.Null", "");
                    return false;
                }

                //должен быть задан номер задолженности
                if (duty.Number.Number == null || duty.Number.Number.Trim() == "")
                {
                    this.Error("Error", "ImportProcess.Number.Empty", duty.Title);
                    return false;
                }

                //должна быть задана дата задолженности
                if (duty.Number.DateDuty == null || !duty.Number.DateDuty.HasValue || duty.Number.DateDuty.DateTime == null || !duty.Number.DateDuty.DateTime.HasValue)
                {
                    this.Error("Error", "ImportProcess.DateDuty.Empty", duty.Title);
                    return false;
                }

                //должен быть задан тип задолженности
                if (duty.Type == null || duty.Type.Name == null || duty.Type.Name.Trim() == "")
                {
                    this.Error("Error", "ImportProcess.Type.NotFound", duty.Title + " (" + ((duty.Type == null) ? "NULL" : duty.Type.Title) + ")");
                    return false;
                }

                //должен быть задана контракт
                if (duty.Contract == null || duty.Contract.Title == null || duty.Contract.Title == "")
                {
                    this.Error("Error", "ImportProcess.Contract.Empty", duty.Title);
                    return false;
                }

                //контракт должен найтись в базе
                string contract = this.DefineContract(duty.Contract.Title);
                if (contract == "")
                {
                    this.Error("Error", "ImportProcess.Contract.NotFound", duty.Title + " (" + duty.Contract.Title + ")");
                    return false;
                }
                else
                    duty.Contract.Code = contract;

                //задолженность должна быть уникальной и не должна дублироваться...
                bool is_duplicate = this.IsDuplicate(duty);
                if (is_duplicate)
                {
                    this.Error("Error", "ImportProcess.Duplicate", duty.Title);
                    return false;
                }

                //если эта задолженность встречалась в этой загрузке, то ее нужно сложить
                SXImportDuty exist = this.FindDuty(duty);
                if (exist != null)
                {
                    exist.Summ += duty.Summ;
                    exist.Rest += duty.Rest;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Error(ex);
                return false;
            }
        }

        protected virtual void AddDuty(SXImportDuty duty)
        {
            try
            {
                if (duty != null)
                    this.Duties.Add(duty);
            }
            catch (Exception ex) { this.Error(ex); }
        }
        #endregion

        #region Functions
        public void Import(SXSchemaList schemas, byte[] data, bool insert_duty)
        {
            try
            {              
                this.duties = new List<SXImportDuty>();

                #region Define Cursor
                if (schemas == null || schemas.Count <= 0)
                {
                    this.Error("Error", "ImportProcess.Schema.Empty", "");
                    return;
                }

                if (data == null || data.Length <= 0)
                {
                    this.Error("Error", "ImportProcess.Data.Empty", "");
                    return;
                }

                SXCursor cursor = null;
                
                try
                {
                    using (MemoryStream ms = new MemoryStream(data))
                        cursor = SXCursor.DefineCursor(ms, schemas);
                }
                catch { cursor = null; }

                if (cursor == null || cursor.Schema == null)
                {
                    this.Error("Error", "ImportProcess.Schema.NotFound", "");
                    return;
                }
                #endregion

                #region Import Duty
                while (cursor.MoveNext())
                    if (!cursor.IsSkip)
                    {
                        SXImportDuty duty = cursor.CreateDuty();

                        if (this.CheckDuty(duty))
                            this.AddDuty(duty);
                    }
                #endregion

                this.AgregateDuty();

                if (insert_duty)
                    this.InsertDuty();
            }
            catch (Exception ex) { this.Error(ex); }
        }

        public SXResult ConfirmationParse(SXSchemaList schemas, byte[] data)
        {
            try
            {
                this.duties = new List<SXImportDuty>();

                #region Define Cursor
                if (schemas == null || schemas.Count <= 0)
                {
                    this.Error("Error", "ImportProcess.Schema.Empty", "");
                    return null;
                }

                if (data == null || data.Length <= 0)
                {
                    this.Error("Error", "ImportProcess.Data.Empty", "");
                    return null;
                }

                SXCursor cursor = null;

                try
                {
                    using (MemoryStream ms = new MemoryStream(data))
                        cursor = SXCursor.DefineCursor(ms, schemas);
                }
                catch { cursor = null; }

                if (cursor == null || cursor.Schema == null)
                {
                    this.Error("Error", "ImportProcess.Schema.NotFound", "");
                    return null;
                }
                #endregion

                #region Import Duty
                while (cursor.MoveNext())
                    if (!cursor.IsSkip)
                    {
                        SXImportDuty duty = cursor.CreateDuty();

                        if (this.CheckDuty(duty))
                            this.AddDuty(duty);
                    }
                #endregion

                SXResult result = new SXResult(this.Duties.Count, "", cursor);
                result.AddSubItem(this.SerialDuties);
                return result;
            }
            catch (Exception ex) { return SXResult.Exception(ex, ""); }
        }

        #endregion
    }
}
