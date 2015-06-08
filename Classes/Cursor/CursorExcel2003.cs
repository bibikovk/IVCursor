using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes;
using ExcelLibrary.SpreadSheet;
using System.IO;
using SXShared.Classes.SXLexems;
using IVCursor.Classes.Mappings;

namespace IVCursor.Classes.Cursor
{
    public class SXCursorExcel2003 : SXCursorExcel
    {
        #region Variables
        protected Worksheet worksheet = null;
        #endregion

        #region Properties
        public override SXSchema.SXMappingType MappingType
        { get { return SXSchema.SXMappingType.Excel2003; } }
        #endregion

        #region Constructor
        public SXCursorExcel2003(Worksheet worksheet)
        {  this.worksheet = worksheet; }

        public SXCursorExcel2003(MemoryStream memory, SXSchema mapping)
        {
            try
            {
                Workbook wb = Workbook.Load(memory);
                if (wb == null || wb.Worksheets.Count <= 0)
                {
                    this.worksheet = null;
                    this.Schema = null;
                }

                this.worksheet = wb.Worksheets[0];
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

                if (row < 0 || col < 0) return "";

                Cell cell = this.worksheet.Cells[row, col];

                if (cell == null || cell.Value == null || cell.IsEmpty)
                    return "";

                CellFormatType cell_type = cell.Format.FormatType;

                string result = "";

                try { result = cell.StringValue; }
                catch { }

                try
                {
                    if (value_type.Trim().ToLower() == "date" || value_type.Trim().ToLower() == "datetime" || cell_type == CellFormatType.DateTime || cell_type == CellFormatType.Date)
                        result = cell.DateTimeValue.ToString("dd.MM.yyyy HH:mm:ss");
                }
                catch { }

                return result;
            }
            catch { return ""; }
        }
        #endregion
    }
}
