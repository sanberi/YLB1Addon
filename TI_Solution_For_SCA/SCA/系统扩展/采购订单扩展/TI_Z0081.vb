Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Public NotInheritable Class TI_Z0081
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function


    Private Sub TI_Z0081_FormDataEvent(ByRef BusinessObjectInfo As BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        'If Not BusinessObjectInfo.BeforeAction Then
        '    ioTempSql = MyForm.DataSources.DataTables.Add("TempDt")
        'End If
    End Sub


    Private Sub TI_Z0081_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent

        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If Not pVal.Before_Action Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    '打开界面的时候默认当前的用户
                    Dim lsName As String
                    lsName = MyApplication.Company.UserName
                    MyForm.DataSources.UserDataSources.Item("Purchaser")
                    If lsName.StartsWith("PO") Then
                        MyForm.DataSources.UserDataSources.Item("Purchaser").Value = lsName.Trim
                    End If
                End If
            Case BoEventTypes.et_CHOOSE_FROM_LIST
                If Not pVal.BeforeAction Then
                    Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
                    Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
                    If Not lodt Is Nothing Then
                        Select Case pVal.ItemUID
                            Case "Purchaser"
                                Dim lsName As String
                                lsName = lodt.GetValue("U_NAME", 0)
                                MyForm.DataSources.UserDataSources.Item("Purchaser").Value = lsName.Trim
                        End Select
                    End If
                End If
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.Before_Action And pVal.ItemUID = "CX" Then
                    GetMtx_10Data(BubbleEvent)
                End If
                If Not pVal.Before_Action And pVal.ItemUID = "CreatePO" Then
                    '创建采购订单
                    CreatePO(BubbleEvent)
                End If
        End Select
    End Sub
    Public Sub GetMtx_10Data(ByVal lbRef As Boolean)
        Try
            Dim lsPurchaser, lsBrand, lsSKU, lsDesc, lsSQL As String
            lsPurchaser = MyForm.DataSources.UserDataSources.Item("Purchaser").ValueEx
            If Not String.IsNullOrEmpty(lsPurchaser) Then
                lsPurchaser = lsPurchaser.Trim
            End If
            If String.IsNullOrEmpty(lsPurchaser) Then
                MyApplication.SetStatusBarMessage("请先选择采购员！", BoMessageTime.bmt_Short, True)
                lbRef = False
                Return
            End If
            lsBrand = MyForm.DataSources.UserDataSources.Item("Brand").ValueEx
            If Not String.IsNullOrEmpty(lsBrand) Then
                lsBrand = lsBrand.Trim
            End If
            lsDesc = MyForm.DataSources.UserDataSources.Item("Desc").ValueEx
            If Not String.IsNullOrEmpty(lsDesc) Then
                lsDesc = lsDesc.Trim
            End If
            lsSKU = MyForm.DataSources.UserDataSources.Item("SKU").ValueEx
            If Not String.IsNullOrEmpty(lsSKU) Then
                lsSKU = lsSKU.Trim
            End If
            lsSQL = "Exec [YL_GetAgentItems] '" + lsPurchaser + "','" + lsBrand + "','" + lsDesc + "','" + lsSKU + "'"
            Dim loSubioMtx_10 As Matrix
            loSubioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
            Dim liRowCount As Integer = ioTempSql.Rows.Count
            ioTempSql.ExecuteQuery(lsSQL)
            Dim liRowCount1 As Integer = ioTempSql.Rows.Count

            Dim ioDtDoc As SAPbouiCOM.DataTable = MyForm.DataSources.DataTables.Item("DOC")
            ioDtDoc.Rows.Clear()
            '加载数据
            Dim lsDocEntry, lsLineId, lsDocDate, lsItemCode, lsitemname, lsBatchNum, lsAgentName, lsLocation As String
            Dim ldQty, ldPrice, ldOrderPrice As Decimal
            For i As Integer = 0 To ioTempSql.Rows.Count - 1
                '    loSubioMtx_10.GetLineData(i + 1)
                ioDtDoc.Rows.Add()
                ioDtDoc.Rows.Offset = ioDtDoc.Rows.Count - 1
                lsDocEntry = ioTempSql.GetValue("DocEntry", i)
                lsLineId = ioTempSql.GetValue("LineNum", i)
                lsDocDate = ioTempSql.GetValue("DocDate", i)
                lsItemCode = ioTempSql.GetValue("ItemCode", i)
                lsitemname = ioTempSql.GetValue("ItemName", i)
                ldOrderPrice = ioTempSql.GetValue("U_OrderPrice", i)
                ldQty = ioTempSql.GetValue("Quantity", i)
                ldPrice = ioTempSql.GetValue("U_Price", i)
                lsBatchNum = ioTempSql.GetValue("BatchNum", i)
                lsAgentName = ioTempSql.GetValue("U_AgentName", i)
                lsLocation = ioTempSql.GetValue("Location", i)

                ioDtDoc.SetValue("LineId", i, (i + 1).ToString())
                ioDtDoc.SetValue("DocEntry", i, lsDocEntry)
                ioDtDoc.SetValue("LineNum", i, lsLineId)
                ioDtDoc.SetValue("DocDate", i, CType(lsDocDate, Date).ToString("yyyyMMdd"))
                ioDtDoc.SetValue("ItemCode", i, lsItemCode)
                ioDtDoc.SetValue("ItemName", i, lsitemname)
                ioDtDoc.SetValue("U_OrderPrice", i, ldOrderPrice.ToString())
                ioDtDoc.SetValue("Quantity", i, ldQty.ToString())
                ioDtDoc.SetValue("TransQty", i, ldQty.ToString())
                ioDtDoc.SetValue("U_Price", i, ldPrice.ToString())
                ioDtDoc.SetValue("BatchNum", i, lsBatchNum)
                ioDtDoc.SetValue("U_AgentName", i, lsAgentName)
                ioDtDoc.SetValue("Location", i, lsLocation)
                '   loSubioMtx_10.GetLineData(i + 1)
            Next
            loSubioMtx_10.LoadFromDataSource()
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
            lbRef = False
        Finally
            ' MyForm.Freeze(False)
        End Try
        ' End If
        '  End If
    End Sub
    Public Sub CreatePO(ByVal lbRef As Boolean)
        Try
            '根据选中的行打开采购订单
            Dim loNetDt As System.Data.DataTable = New System.Data.DataTable
            loNetDt.Columns.Add("ItemCode", GetType(String))   '物料代码
            loNetDt.Columns.Add("OrderPrice", GetType(Decimal)) '下单价格
            loNetDt.Columns.Add("Quantity", GetType(Decimal))  '数量
            loNetDt.Columns.Add("AgentName", GetType(String))  '代销名称
            loNetDt.Columns.Add("BatchNum", GetType(String))  '批次号
            loNetDt.Columns.Add("Location", GetType(String))  '库位
            loNetDt.Columns.Add("OIGNEntry", GetType(String))  '库存收货单号
            loNetDt.Columns.Add("OIGNLine", GetType(String))  '库存收货单行号
            Dim loNetRow As System.Data.DataRow
            Dim loMtx_10 As Matrix
            loMtx_10 = MyForm.Items.Item("Mtx_10").Specific
            Dim lsItemCode, lsAgentName, lsBatchNum, lsLocation, lsOIGNEntry, lsOIGNLine As String
            Dim ldPrice, ldQty As Double
            Dim loCheck As SAPbouiCOM.CheckBox
            lsAgentName = ""
            For i As Integer = 1 To loMtx_10.RowCount
                'lsSelected = loMtx_10.GetCellSpecific("U_Select", i).Value
                loCheck = loMtx_10.Columns.Item("U_Select").Cells.Item(i).Specific
                If loCheck.Checked Then
                    lsItemCode = loMtx_10.Columns.Item("ItemCode").Cells.Item(i).Specific.Value
                    lsAgentName = loMtx_10.Columns.Item("AgentName").Cells.Item(i).Specific.Value
                    lsBatchNum = loMtx_10.Columns.Item("BatchNum").Cells.Item(i).Specific.Value
                    lsLocation = loMtx_10.Columns.Item("Location").Cells.Item(i).Specific.Value
                    lsOIGNEntry = loMtx_10.Columns.Item("DocEntry").Cells.Item(i).Specific.Value
                    lsOIGNLine = loMtx_10.Columns.Item("LineNum").Cells.Item(i).Specific.Value
                    Decimal.TryParse(loMtx_10.Columns.Item("OrderPrice").Cells.Item(i).Specific.Value, ldPrice)
                    Decimal.TryParse(loMtx_10.Columns.Item("TransQty").Cells.Item(i).Specific.Value, ldQty)
                    loNetRow = loNetDt.NewRow
                    loNetRow("ItemCode") = lsItemCode
                    loNetRow("OrderPrice") = ldPrice
                    loNetRow("Quantity") = ldQty
                    loNetRow("AgentName") = lsAgentName
                    loNetRow("BatchNum") = lsBatchNum
                    loNetRow("Location") = lsLocation
                    loNetRow("OIGNEntry") = lsOIGNEntry
                    loNetRow("OIGNLine") = lsOIGNLine
                    loNetDt.Rows.Add(loNetRow)
                End If
            Next
            '打开采购订单页面
            Dim loActForm As Form
            MyApplication.Menus.Item("2305").Activate()
            Dim loMatrix As Matrix   '采购订单的matrix
            loActForm = MyApplication.Forms.ActiveForm()
            loMatrix = loActForm.Items.Item("38").Specific           
            loActForm.Items.Item("54").Specific.value = lsAgentName   '供应商名称
            ' MyForm.DataSources.DBDataSources.Item(0).SetValue("U_POType", 0, "3")
            Dim PoRow As Integer
            PoRow = 1
            For Each loNetRow1 As System.Data.DataRow In loNetDt.Rows
                lsItemCode = loNetRow1("ItemCode")
                ldPrice = loNetRow1("OrderPrice")
                ldQty = loNetRow1("Quantity")
                lsBatchNum = loNetRow1("BatchNum")
                lsLocation = loNetRow1("Location")
                lsOIGNEntry = loNetRow1("OIGNEntry")
                lsOIGNLine = loNetRow1("OIGNLine")
                loMatrix.Columns.Item("1").Cells.Item(PoRow).Specific.Value = lsItemCode '描述
                loMatrix.Columns.Item("11").Cells.Item(PoRow).Specific.Value = Convert.ToString(ldQty)  '数量
                loMatrix.Columns.Item("20").Cells.Item(PoRow).Specific.Value = Convert.ToString(ldPrice)  '含税价
                loMatrix.Columns.Item("U_BatchNum").Cells.Item(PoRow).Specific.Value = lsBatchNum  '批次
                loMatrix.Columns.Item("U_Location").Cells.Item(PoRow).Specific.Value = lsLocation   '库位
                loMatrix.Columns.Item("U_AgentBaseEntry").Cells.Item(PoRow).Specific.Value = lsOIGNEntry     '库存收货单号
                loMatrix.Columns.Item("U_AgentBaseLine").Cells.Item(PoRow).Specific.Value = lsOIGNLine    '库存收货单行号
                PoRow = PoRow + 1
            Next
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
            lbRef = False
        End Try
    End Sub
End Class
