﻿using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace B1Extra
{
    public class TI_Z0100ApprovalDataProvider : ApprovalDataProvider
    {
        public TI_Z0100ApprovalDataProvider(Company oCompany, string objType, string docEntry, string cardCode) : base(
            oCompany, objType, docEntry, cardCode)
        {

        }
        public override bool Verifly()
        {
            return true;
        }
    }
    public class ApprovalDataProvider
    {

        // Methods
        public ApprovalDataProvider(Company oCompany, string objType,string docEntry, string cardCode,bool isTriggerApproval=false)
        {
            this.MyCompany = oCompany;
            this.ObjType = objType;
            this.DocEntry = docEntry;
            this.CardCode = cardCode;
            this.IsTriggerApproval = isTriggerApproval;
            this.Verifly();
        }

        public virtual bool Verifly()
        {
            var isTriggerApproval = this.IsTriggerApproval ? "Y" : "N";
            RecordsetWapper wapper = new RecordsetWapper(this.MyCompany, $"EXEC [YL_SP_VeryfiyApprovalData],'{this.ObjType}' ,'{this.DocEntry}','{isTriggerApproval}','','' ");
            DataTable dataTable = wapper.GetDataTable();
            string errorCode = wapper.GetScalarValue(0).ToString();
            string errorMessage = wapper.GetScalarValue(1).ToString();
            string apvCode = wapper.GetScalarValue(2).ToString();
            if (int.Parse(errorCode) > 0)
            {
                throw new Exception(errorMessage);
            }
            this.ApprovalCode = apvCode;
            return true;
        }

        // Properties
        public bool IsTriggerApproval { get; set; } 
        public string ObjType { get; set; }
        public string ApprovalCode { get; set; }

        public string DocEntry { get; set; }

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
