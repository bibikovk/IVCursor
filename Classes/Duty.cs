using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVCursor.Classes.Cursor;
using IVCursor.Classes.Mappings;
using IVInterface.Classes;
using IVInterface.Classes.Duty;
using SXShared.Classes;

namespace IVCursor.Classes
{
    public class SXImportDuty : SXSerialDuty
    {
        #region Variables
        protected SXSchema schema = null;
        #endregion

        #region Properties
        public SXSchema Schema { get { return this.schema; } set { this.schema = value; } }
        #endregion

        #region Constructors
        public SXImportDuty() { }

        public SXImportDuty(SXCursor cursor)
        {
            this.Schema = ((cursor == null) ? null : cursor.Schema);

            if (cursor == null || this.Schema == null) return;

            this.DefineContract(cursor.GetFieldString("contract").Trim());
            this.DefineType(cursor.GetFieldString("dutytype"));
            this.DefineSumm(cursor.GetFieldNumber("summ"));

            this.NumberAddon = cursor.GetFieldString("numadd");

            this.Number = new SXSerialDutyNumber()
            {
                Number = cursor.GetFieldString("number").Trim().ToLower(),
                DateDuty = cursor.GetFieldDate("dateduty"),
                DatePayment = cursor.GetFieldDate("datepay"),

                //NumDoc = cursor.GetFieldString("numdoc").Trim(),
                //NumAdd = cursor.GetFieldString("numadd").Trim(),
                //DateDoc = cursor.GetFieldDate("datedoc"),

                RefNum = cursor.GetFieldString("refnum")
            };

            if (cursor.Schema.Rules["Extras"].Trim().ToLower() == "true" || cursor.Schema.Rules["ExtraValues"].Trim().ToLower() == "true")
            {
                this.ExtraValues = new SXSerialParamList();
                foreach (SXSchemaField field in cursor.Schema.Fields)
                    if (field.Name.Trim().ToLower().StartsWith("extra"))
                        this.ExtraValues.Add(field.Name, cursor.GetFieldString(field.Name));
            }

        }
        #endregion

        #region Functions
        protected string DefineContract(string value)
        {
            string result = value;

            if (this.Schema != null)
            {
                #region Replace with ContractReplacement Vocabulary
                SXSchemaVocabulary voc_contract_replacement = this.Schema.Vocabularies["ContractReplacement"];
                if (voc_contract_replacement != null)
                    foreach (SXType t in voc_contract_replacement.Types)
                        result = result.Replace(t.Identifier, t.Title);
                #endregion
            }

            this.Contract = new SXSerialReference(0, result, result);

            return result;
        }

        protected string DefineType(string value)
        {
            string result = value;

            if (this.Schema != null)
            {
                #region Define From DutyType Vocabulary
                SXSchemaVocabulary voc_duty_type = this.Schema.Vocabularies["DutyType"];
                if (voc_duty_type != null)
                {
                    SXType voc_duty_type_item = voc_duty_type[value];
                    if (voc_duty_type_item != null)
                        result = voc_duty_type_item.Identifier;
                }
                #endregion
            }

            this.Type = new SXSerialType(0, result, value);

            return result;
        }

        protected decimal DefineSumm(decimal? value)
        {
            this.Summ = this.Rest = 0;

            if (value == null || !value.HasValue)
                return 0;

            decimal result = value.Value;

            if (this.Schema != null)
            {
                #region Применение общих правил (нормализации значения задолженности заданного типа)
                //пытаемся преобразовать сумму к нормальной форме
                //кто-то грузит свою задолженность с +, кто-то с -
                string rule_summ_value = this.Schema.Rules["Summ.Value"].Trim().ToLower();

                if (rule_summ_value == "negative")
                    result *= -1;
                #endregion

                #region задолженность с отрицательным знаком (словарь загрузчика)
                //задолженность с отрицательным знаком = нужно смотреть на правило
                if (result < 0)
                {
                    string rule_negative = this.Schema.Rules["IF.DutyType.Value-" + this.Type.Title + ".Negative"].Trim();
                    if (rule_negative.Trim() != "")
                    {
                        result *= -1;
                        this.DefineType(rule_negative);
                    }
                }
                #endregion

                #region задолженность с отрицательным знаком (словарь медии)
                //задолженность с отрицательным знаком = нужно смотреть на правило
                if (result < 0)
                {
                    string rule_negative = this.Schema.Rules["IF.DutyType.Type-" + this.Type.Name + ".Negative"].Trim();
                    if (rule_negative.Trim() != "")
                    {
                        result *= -1;
                        this.DefineType(rule_negative);
                    }
                }
                #endregion
            }

            this.Summ = this.Rest = result;

            return result;
        }
        #endregion
    }
}
