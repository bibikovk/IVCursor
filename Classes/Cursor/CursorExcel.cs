using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SXShared.Classes.SXLexems;

namespace IVCursor.Classes.Cursor
{
    public class SXCursorExcel : SXCursor
    {
        #region Variables
        protected int row = -1;
        #endregion

        #region Properties
        public int Row
        { get { return this.row; } }
        #endregion

        #region Constructor
        public SXCursorExcel() { }
        #endregion

        #region Functions
        protected void SetRow(int row_index)
        {
            this.row = row_index;
            this.ReloadEnvironment();
        }

        protected virtual string GetValue(int row, int col, string value_type)
        { return ""; }

        protected override string GetValue(string uri)
        {
            try
            {
                if (uri == null || uri.Trim() == "") return "";

                string adr = uri.Trim();

                #region Expression
                if (adr.StartsWith("=") && adr.Length > 1)
                    return this.GetCalc(new SXExpression(adr.Substring(1)));
                #endregion

                #region Multiple Concatination
                if (adr.Contains('+'))
                {
                    string result = "";

                    string[] parts = adr.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string part in parts)
                        result += this.GetValue(part.Trim());

                    return result;
                }
                #endregion

                #region Constant String
                if (adr.StartsWith("'") && adr.EndsWith("'"))
                    return ((adr.Length <= 2) ? "" : adr.Substring(1, adr.Length - 2));
                #endregion

                string value_type = "";
                int value_row = this.Row;
                int value_col = -1;

                #region Define Value Type
                //6:Date;  R2C1:String
                if (adr.Contains(':'))
                {
                    string[] adr_parts = adr.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (adr_parts != null && adr_parts.Length == 2)
                    {
                        value_type = adr_parts[1].Trim();
                        adr = adr_parts[0].Trim();
                    }
                }
                #endregion

                #region Column Index
                if (Int32.TryParse(adr, out value_col))
                    return this.GetValue(value_row, value_col, value_type);
                #endregion

                #region Excel Corrds R3C9
                if (adr.ToLower().StartsWith("r") || adr.ToLower().StartsWith("c"))
                {
                    string[] coords = adr.Split(new char[] { 'r', 'c', 'R', 'C' });
                    if (coords != null && coords.Length == 3)
                    {
                        //define row and column address
                        string row_adr = ((adr.ToLower().StartsWith("r")) ? coords[1] : coords[2]);
                        string col_adr = ((adr.ToLower().StartsWith("c")) ? coords[1] : coords[2]);

                        //define row index
                        if (row_adr.Trim() == "" || !Int32.TryParse(row_adr, out value_row))
                            value_row = this.Row;

                        //define column index
                        if (!Int32.TryParse(col_adr, out value_col))
                            value_col = -1;
                    }
                }
                #endregion

                return this.GetValue(value_row, value_col, value_type);
            }
            catch { return ""; }
        }

        public override bool MoveNext()
        {
            if (this.Schema == null || this.Schema.Range == null || this.Schema.Range.Condition == null)
                return false;

            int start_position = ((this.Schema.Range.StartPosition.Trim() == "") ? 0 : Convert.ToInt32(this.Schema.Range.StartPosition));

            //move next row
            this.SetRow(((this.Row <= -1) ? start_position : (this.Row + 1)));

            return (this.GetValue(this.Schema.Range.Condition.Address).Trim().ToLower() != this.Schema.Range.Condition.Value.Trim().ToLower());
        }

        protected override SXLexemVariable OnLexemFunctionExecute(SXLexemFunction func)
        {
            string result = "";
            int save_row = this.Row;

            if (func == null) return null;

            try
            {
                if (func.Name.Trim().ToLower() == "lastnoempty" && func.Arguments.Count == 1)
                {
                    #region
                    for (int r = save_row; r >= 0; r--)
                    {
                        this.SetRow(r);

                        result = this.GetCalc(func.Arguments[0]);
                        if (result.Trim() != "") break;
                    }
                    #endregion
                }

                if (func.Name.Trim().ToLower() == "previous" && func.Arguments.Count == 1)
                {
                    #region
                    if (this.Row >= 1)
                    {
                        this.SetRow(this.Row - 1);
                        result = this.GetCalc(func.Arguments[0]);
                    }
                    #endregion
                }

                if (func.Name.Trim().ToLower() == "next" && func.Arguments.Count == 1)
                {
                    #region
                    this.SetRow(this.Row + 1);           
                    result = this.GetCalc(func.Arguments[0]);
                    #endregion
                }
            }
            catch { result = ""; }

            this.SetRow(save_row);

            return new SXLexemVariable("res", result);
        }
        #endregion
    }
}
