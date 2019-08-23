using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace B1Extra
{
    public class ApprovalDataProvider
    {
        private Dictionary<string, string> Department2ApprovalTemlatesMapping;

        // Methods
        public ApprovalDataProvider(Company oCompany, string baseKey, string cardCode)
        {
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            dictionary1.Add("D0015", "SP0001");
            this.Department2ApprovalTemlatesMapping = dictionary1;
            this.MyCompany = oCompany;
            this.BaseKey = baseKey;
            this.CardCode = cardCode;
            this.Verifly();
        }

        public bool Verifly()
        {
            RecordsetWapper wapper = new RecordsetWapper(this.MyCompany, "EXEC [YL_SP_VeryfiyApprovalData]'17','" + this.BaseKey + "','',''");
            DataTable dataTable = wapper.GetDataTable();
            string s = wapper.GetScalarValue(0).ToString();
            string message = wapper.GetScalarValue(1).ToString();
            string str3 = wapper.GetScalarValue(2).ToString();
            if (int.Parse(s) > 0)
            {
                throw new Exception(message);
            }
            this.ApprovalCode = str3;
            return true;
        }

        // Properties
        public string ApprovalCode { get; set; }

        public string BaseKey { get; set; }

        public string BaseType { get; set; }

        public string CardCode { get; set; }

        public string DepartmentCode
        {
            get
            {
                return new RecordsetWapper(this.MyCompany, "Select Department FROM aaronMDM.MDM.dbo.MDM000101 WHERE Code='" + this.SalerCode + "'").GetScalarValue().ToString();
            }
        }

        public string IsDesignated { get; set; }

        public Company MyCompany { get; set; }

        public string PostAddress
        {
            get
            {
                return new RecordsetWapper(this.MyCompany, "SELECT U_Value FROM [@YL_Config] WHERE Code='APVADD'").GetScalarValue().ToString();
            }
        }

        public string SalerCode
        {
            get
            {
                return new RecordsetWapper(this.MyCompany, "Select t11.SalerCode from OCRD t10 inner join RW0001 t11 on t10.CardCode='" + this.CardCode + "' and t10.U_Saler=t11.SalerName").GetScalarValue().ToString();
            }
        }
    }





}
