using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace B1Extra
{
    public static class YLConfiguration
    {
        private static Dictionary<string, string> YLConfig;

         static YLConfiguration()
        {
            YLConfig = new Dictionary<string, string>();
        }
        public static string GetConfig(Company oCompany,string config)
        {
            if (YLConfig.ContainsKey(config))
            {
                return YLConfig[config];
            }
            else
            {
                 var configValue=new RecordsetWapper(oCompany, $"SELECT U_Value FROM [@YL_Config] WHERE Code='{config}'").GetScalarValue().ToString();
                YLConfig.Add(config,configValue);
                return configValue;
            }
        }
    }
    public class RecordsetWapper
    {
        // Fields
        private string[] args;
        private Company oCompany;
        private Recordset oRecordset;
        private string procedure;
        private string strSQL;

        // Methods
        public RecordsetWapper(Company oCompany, string strSQL)
        {
            this.strSQL = strSQL;
            this.oCompany = oCompany;
            this.oRecordset = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
        }

        public RecordsetWapper(Company oCompany, string procedure, string[] args)
        {
            this.oCompany = oCompany;
            this.procedure = procedure;
            this.args = args;
            this.oRecordset = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
        }

        public int EXECProcedureNonQuery()
        {
            this.oRecordset.Command.Name = this.procedure;
            for (int i = 0; i < this.args.Length; i++)
            {
                this.oRecordset.Command.Parameters.Item(i + 1).Value = this.args[i];
            }
            this.oRecordset.Command.Execute();
            return int.Parse(this.oRecordset.Command.Parameters.Item(0).Value.ToString());
        }

        public int EXECProcedureNonQuery1()
        {
            this.oRecordset.Command.Name = this.procedure;
            for (int i = 0; i < this.args.Length; i++)
            {
                this.oRecordset.Command.Parameters.Item(i + 1).Value = this.args[i];
            }
            DataTable table = new DataTable();
            table.Columns.Add("Direction", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            for (int j = 0; j < this.oRecordset.Command.Parameters.Count; j++)
            {
                DataRow row = table.NewRow();
                row["Direction"] = this.oRecordset.Command.Parameters.Item(j).Direction;
                row["Type"] = this.oRecordset.Command.Parameters.Item(j).Type;
                row["Name"] = this.oRecordset.Command.Parameters.Item(j).Name;
                row["Value"] = this.oRecordset.Command.Parameters.Item(j).Value;
                table.Rows.Add(row);
            }
            this.oRecordset.Command.Execute();
            DataTable table2 = new DataTable();
            table2.Columns.Add("Direction", typeof(string));
            table2.Columns.Add("Type", typeof(string));
            table2.Columns.Add("Name", typeof(string));
            table2.Columns.Add("Value", typeof(string));
            for (int k = 0; k < this.oRecordset.Command.Parameters.Count; k++)
            {
                DataRow row2 = table2.NewRow();
                row2["Direction"] = this.oRecordset.Command.Parameters.Item(k).Direction;
                row2["Type"] = this.oRecordset.Command.Parameters.Item(k).Type;
                row2["Name"] = this.oRecordset.Command.Parameters.Item(k).Name;
                row2["Value"] = this.oRecordset.Command.Parameters.Item(k).Value;
                table2.Rows.Add(row2);
            }
            if (this.oRecordset.RecordCount == 0)
            {
                int count = this.oRecordset.Fields.Count;
                for (int m = 0; m <= (count - 1); m++)
                {
                    string name = this.oRecordset.Fields.Item(m).Name;
                    table.Columns.Add(name, typeof(string));
                }
                while (!this.oRecordset.EoF)
                {
                    DataRow row3 = table.NewRow();
                    for (int n = 0; n <= (count - 1); n++)
                    {
                        row3[n] = this.oRecordset.Fields.Item(n).Value;
                    }
                    table.Rows.Add(row3);
                    this.oRecordset.MoveNext();
                }
            }
            return int.Parse(this.oRecordset.Command.Parameters.Item(0).Value.ToString());
        }

        public DataTable GetDataTable()
        {
            DataTable table = new DataTable();
            Recordset recordset = this.GetRecordset();
            int count = recordset.Fields.Count;
            for (int i = 0; i <= (count - 1); i++)
            {
                string name = recordset.Fields.Item(i).Name;
                table.Columns.Add(name, typeof(string));
            }
            while (!recordset.EoF)
            {
                DataRow row = table.NewRow();
                for (int j = 0; j <= (count - 1); j++)
                {
                    row[j] = recordset.Fields.Item(j).Value;
                }
                table.Rows.Add(row);
                recordset.MoveNext();
            }
            return table;
        }

        public Recordset GetRecordset()
        {
            this.oRecordset.DoQuery(this.strSQL);
            if (this.oRecordset.RecordCount == 0)
            {
                throw new Exception(string.Format("The Query: {0}  -- have no data", this.strSQL));
            }
            return this.oRecordset;
        }

        public object GetScalarValue()
        {
            return this.GetRecordset().Fields.Item(0).Value;
        }

        public object GetScalarValue(int index)
        {
            return this.GetRecordset().Fields.Item(index).Value;
        }
    }





}
