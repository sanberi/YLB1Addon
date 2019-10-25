using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SAPbouiCOM;
using TIModule;

namespace TiExtend
{
    public class SYS998Form:FormBase
    {
        public SYS998Form()
        {
            this.ItemEvent += MyItemEvent;
        }

        private void MyItemEvent(string FormUID, ref ItemEvent pVal, ref bool BubbleEvent)
        {
            //if (pVal.EventType==BoEventTypes.et_FORM_DATA_LOAD)
            //{
            //    var ds = MyForm.DataSources.DBDataSources.Item("OITM");
            //}
            //仅特权用户可以更改特定字段显示方式
            if (pVal.EventType==BoEventTypes.et_ITEM_PRESSED & pVal.BeforeAction==true)
            {
                try
                {
                    var myMatrix = (Matrix)MyForm.Items.Item("11").Specific;
                    //var curCol = myMatrix.Columns.Item(pVal.ColUID);
                    var namedField = (SAPbouiCOM.EditText)myMatrix.Columns.Item(1).Cells.Item(pVal.Row).Specific;
                    var powerUserSetting = B1Extra.YLConfiguration.GetConfig((SAPbobsCOM.Company)MyApplication.Company.GetDICompany(), "Doc_PUsr");
                    var powerUsers= powerUserSetting.Split(',').Select(fld => fld.Trim().ToLower());
                    if (!powerUsers.Contains(MyApplication.Company.UserName.ToLower()))
                    {
                        var forbidenFiledsSetting = B1Extra.YLConfiguration.GetConfig((SAPbobsCOM.Company)MyApplication.Company.GetDICompany(), "Doc_FFld");
                        var forbidenFileds = forbidenFiledsSetting.Split(',').Select(fld => fld.Trim());
                        foreach (var forbidenFiled in forbidenFileds)
                        {
                            if (namedField.Value.Contains(forbidenFiled))
                            {
                                var curCell = (SAPbouiCOM.CheckBox)myMatrix.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row).Specific;
                                if (curCell.Checked)
                                {
                                    curCell.Checked = false;
                                    MyApplication.SetStatusBarMessage("You cann't Change this");
                                    BubbleEvent = false;
                                    return;
                                }
                            }

                        }

                    }

                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    //throw;
                }

            }
        }
        //private void MyItemEvent(string formUID, ItemEvent pVal, bool bubbleEvent)
        //{

        //}

    }
}
