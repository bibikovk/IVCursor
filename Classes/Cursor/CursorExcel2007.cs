using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using SXShared.Classes;
using System.IO;
using SXShared.Classes.SXLexems;
using IVCursor.Classes.Mappings;

namespace IVCursor.Classes.Cursor
{
    public class SXCursorExcel2007 : SXCursorExcel
    {
        #region Variables
        protected ExcelWorksheet worksheet = null;
        #endregion

        #region Properties
        public override SXSchema.SXMappingType MappingType
        { get { return SXSchema.SXMappingType.Excel2007; } }
        #endregion

        #region Constructor
        public SXCursorExcel2007(ExcelWorksheet worksheet)
        { this.worksheet = worksheet; }

        public SXCursorExcel2007(MemoryStream memory, SXSchema mapping)
        {
            try
            {
                ExcelWorkbook wb = (new ExcelPackage(memory)).Workbook;
                if (wb == null || wb.Worksheets == null || wb.Worksheets.Count <= 0)
                {
                    this.worksheet = null;
                    this.Schema = null;
                }

                this.worksheet = wb.Worksheets.First();
                this.Schema = mapping;
            }
            catch
            {
                this.worksheet = null;
                this.Schema = null;
            }
        }
        #endregion

        #region Functions
        protected override string GetValue(int row, int col, string value_type)
        {
            try
            {
                if (this.worksheet == null) return "";
                ExcelRange range = this.worksheet.Cells[row + 1, col + 1];

                if (range == null || range.Value == null) 
                    return "";

                string result = range.Value.ToString();

                try
                {
                    if (value_type.Trim().ToLower() == "date" || value_type.Trim().ToLower() == "datetime" || range.Value is DateTime)
                        result = Convert.ToDateTime(range.Value).ToString("dd.MM.yyyy HH:mm:ss");
                }
                catch { }

                return result;
            }
            catch { return ""; }
        }
        #endregion
    }
}
