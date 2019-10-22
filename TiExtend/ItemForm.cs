using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SAPbouiCOM;
using TI_Solution_For_SCA;

namespace TiExtend
{
    public class ItemForm:FormBase
    {
        public ItemForm()
        {
            this.ItemEvent += MyItemEvent;
        }

        private void MyItemEvent(string FormUID, ref ItemEvent pVal, ref bool BubbleEvent)
        {
            if (pVal.EventType==BoEventTypes.et_FORM_DATA_LOAD)
            {
                var ds = MyForm.DataSources.DBDataSources.Item("OITM");
            }
        }
        //private void MyItemEvent(string formUID, ItemEvent pVal, bool bubbleEvent)
        //{

        //}

    }
}
