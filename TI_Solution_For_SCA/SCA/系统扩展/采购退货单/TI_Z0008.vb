Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Public NotInheritable Class TI_Z0008
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function
    Public Shared iiMemuCount_ASN As Integer = 1

    Private Sub TI_Z0008_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    '添加打印导出EXCEL的按钮
                    Dim loItem, loItemChoose As Item
                    loItem = MyForm.Items.Add("Export", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1 As Item
                    Dim loBtn_Export As SAPbouiCOM.Button
                    loBtn_Create1 = MyForm.Items.Item("10000330")
                    'loItem3 = MyForm.Items.Item("10000329")

                    loItem.Left = loBtn_Create1.Left - 100 - 5
                    loItem.Width = 100
                    loItem.Top = loBtn_Create1.Top
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "10000330"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "打印"

                    Dim loItemTrans As Item
                    Dim loBtn_Generate As SAPbouiCOM.Button
                    loItemTrans = MyForm.Items.Add("Return", BoFormItemTypes.it_BUTTON)
                    loItemTrans.Left = loBtn_Create1.Left
                    loItemTrans.Top = loBtn_Create1.Top - 25
                    ' loItemTrans.Width = 100
                    '  loItemTrans.Top = loBtn_Create1.Top
                    loItemTrans.Height = loBtn_Create1.Height
                    loItemTrans.Width = loBtn_Create1.Width
                    loItemTrans.LinkTo = "10000330"
                    loBtn_Generate = loItemTrans.Specific
                    loBtn_Generate.Caption = "寄售退回W006"

                    '添加打印选项
                    loItemChoose = MyForm.Items.Add("ChooseList", BoFormItemTypes.it_COMBO_BOX)
                    Dim loCmb_Chooselist As SAPbouiCOM.ComboBox
                    loItemChoose.Left = loItem.Left - 100 - 5
                    loItemChoose.Width = 100
                    loItemChoose.Top = loItem.Top
                    loItemChoose.Height = loItem.Height
                    loItemChoose.AffectsFormMode = False
                    loItemChoose.LinkTo = "Export"
                    loCmb_Chooselist = loItemChoose.Specific
                    Dim lsTempName As String
                    Dim lsSQL As String = "select T10.U_TempPath,T11.U_Template,T11.U_PrintName,T11.U_PageSize,T11.U_TempName " &
                                            "   from [@ti_z0010] T10 inner join [@ti_z0011] T11 On t10.Code=t11.code  " &
                                              " where T10.code ='ORPD' and isnull(T11.U_TempName,'') <>'' "
                    ioTempSql.ExecuteQuery(lsSQL)
                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                        lsTempName = ioTempSql.GetValue("U_TempName", i)
                        loCmb_Chooselist.ValidValues.Add(lsTempName, lsTempName)
                    Next
                    loCmb_Chooselist.Select("采购退货单", BoSearchKey.psk_ByValue)
                End If
            Case BoEventTypes.et_ITEM_PRESSED
                '按导出EXCEL 时将交货单的数据导出到EXCEL
                If Not pVal.Before_Action And pVal.ItemUID = "Export" Then
                    If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
                        MyApplication.MessageBox("只有在确认模式下才能打印!", "确定", "取消")
                        Return
                    End If

                    Dim loCmb_Choose As SAPbouiCOM.ComboBox
                    Dim lsServicePath, lsTempEXCEL, lsPrintName, lsPageSize As String
                    Dim loItem = MyForm.Items.Item("ChooseList")
                    loCmb_Choose = loItem.Specific
                    Dim lsTempName As String = loCmb_Choose.Selected.Value   '打印模板名称
                    '  Dim lsTempName As String = MyForm.Items.Item("ChooseList").Specific
                    If lsTempName = "" Then
                        Return
                        BubbleEvent = False
                    End If
                    Dim lsSQL As String = "select T10.U_TempPath,T11.U_Template,T11.U_PrintName,T11.U_PageSize " &
                                              "   from [@ti_z0010] T10 inner join [@ti_z0011] T11 On t10.Code=t11.code  " &
                                                " where T10.code ='ORPD' and T11.U_tempname='" & lsTempName & "'"
                    ioTempSql.ExecuteQuery(lsSQL)
                    If ioTempSql.Rows.Count > 0 Then
                        lsServicePath = ioTempSql.GetValue("U_TempPath", 0)
                        lsTempEXCEL = ioTempSql.GetValue("U_Template", 0)
                        lsPrintName = ioTempSql.GetValue("U_PrintName", 0)
                        lsPageSize = ioTempSql.GetValue("U_PageSize", 0)
                    End If
                    Dim lsXML As String = MyForm.GetAsXML
                    '如果是交货草稿则取另外的SQL
                    ' Dim lsSourcePath As String = GetPaht()
                    Dim liDocEntry As Integer = MyForm.Items.Item("8").Specific.value

                    Dim lsTargetFile As String = lsServicePath + lsTempEXCEL
                    Dim lsU_Printer As String = lsPrintName  '打印机名称
                    Dim lsU_PsizeID As String = lsPageSize   '打印机纸张
                    Dim lsSourcePath As String  '原始表格路径
                    lsSourcePath = lsPageSize
                    If lsTempName <> "条码" Then
                        '打开Excel
                        Dim oExcelApp As Microsoft.Office.Interop.Excel.Application
                        oExcelApp = New Microsoft.Office.Interop.Excel.Application
                        Dim hwnd As Integer = CInt(oExcelApp.Hwnd)
                        Dim processid As Integer
                        GetWindowThreadProcessId(hwnd, processid)

                        Dim m_objBooks As Microsoft.Office.Interop.Excel.Workbooks
                        Dim m_objBook As Microsoft.Office.Interop.Excel.Workbook
                        Dim m_objSheets As Microsoft.Office.Interop.Excel.Sheets
                        Dim m_objSheet As Microsoft.Office.Interop.Excel.Worksheet

                        ' File.Copy(lsSourcePath, lsTargetFile, True)

                        oExcelApp.Visible = True
                        oExcelApp.DisplayAlerts = False
                        m_objBooks = oExcelApp.Workbooks
                        m_objBook = m_objBooks.Open(lsTargetFile)

                        m_objSheets = m_objBook.Worksheets
                        m_objSheet = m_objSheets.Item(1) '定位第一张表
                        m_objSheet.Activate()

                        '设置打印机，纸张
                        '检查打印机是否正确
                        Dim doc As System.Drawing.Printing.PrintDocument = New System.Drawing.Printing.PrintDocument()
                        doc.PrinterSettings.PrinterName = lsU_Printer
                        doc.DefaultPageSettings.PrinterSettings.Copies = 2
                        doc.PrinterSettings.Copies = 2
                        Dim lsFlag As String = "2"
                        If Not doc.PrinterSettings.IsValid Then
                            lsFlag = "1"
                        End If
                        doc = Nothing
                        GC.Collect()

                        '打印
                        Try
                            oExcelApp.Run("Sheet1.FindPrinter", lsU_Printer, lsU_PsizeID, lsFlag)
                            oExcelApp.ScreenUpdating = False
                            oExcelApp.Run("GetDataString", Convert.ToString(liDocEntry))
                            oExcelApp.ScreenUpdating = True

                            m_objSheet.PrintOutEx()

                        Catch ex As Exception
                            MyApplication.SetStatusBarMessage(ex.ToString())
                            BubbleEvent = False
                            '   File.Delete(lsTargetFile)
                            ' Return
                        Finally
                            m_objBook.Close()
                            Dim deadProcess As Process = Process.GetProcessById(processid)  '获取该进程
                            oExcelApp.Quit()
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp)
                            oExcelApp = Nothing
                            GC.Collect()
                            deadProcess.Kill()  '杀死进程
                        End Try
                    End If
                    If lsTempName = "条码" Then
                        '打印
                        Try
                            '打开Excel
                            Dim lsSQLInsert As String = "EXEC GenerateCodeBars '" + liDocEntry.ToString() + "'"
                            ioTempSql.ExecuteQuery(lsSQLInsert)
                            Dim BarTenderApp As New BarTender.Application
                            BarTenderApp.Formats.Open(lsTargetFile)
                            BarTenderApp.Formats.Item(0).PrintOut(True)
                            BarTenderApp.Quit(BarTender.BtSaveOptions.btDoNotSaveChanges)

                        Catch ex As Exception
                            MyApplication.SetStatusBarMessage(ex.ToString())
                            BubbleEvent = False
                            '   File.Delete(lsTargetFile)
                            ' Return
                        Finally

                        End Try
                    End If
                End If
                '销售退回仓库
                If Not pVal.Before_Action And pVal.ItemUID = "Return" Then
                    '货权转移，先做库存下发货，再创建收货单,只有整个订单式代销订单才能做货权转移
                    If MyForm.DataSources.DBDataSources.Item(0).GetValue("U_ReturnType", 0).Trim() <> "6" Then
                        MyApplication.MessageBox("只有代销退货才能点此按钮!", 1, "确定")
                        BubbleEvent = False
                        Return
                    Else
                        CreateTransferDoc(BubbleEvent)
                    End If
                End If
        End Select
    End Sub

    '获取模板位置
    Private Function GetPaht() As String
        Dim lsSql As String
        lsSql = "Select top 1 U_DelTem  From [@TI_Z0060] t10 where t10.Code='TI_001' "
        Dim isLadPath As String = ""
        ioTempSql.ExecuteQuery(lsSql)
        isLadPath = ioTempSql.GetValue("U_DelTem", 0)
        Return isLadPath
    End Function

    Public Sub CreateTransferDoc(ByVal lbRef As Boolean)
        If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
            MyApplication.MessageBox("只有在确认模式下才能打印!", 1, "确定")
            lbRef = False
            Return
        End If
        Dim loMtx_10 As Matrix
        loMtx_10 = MyForm.Items.Item("38").Specific
        Dim loNetRow As System.Data.DataRow
        Dim lsItemCode, lsBatchNum, lsLocation, lsBaseEntry, lsBaseLine, lsCardName As String
        Dim ldQty, ldPrice As Double
        lsBaseEntry = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
        '控制已经转移过不能再次点击
        Dim lsSql As String
        lsSql = "Select isnull(U_OIGEEntry,'') as 'U_OIGEEntry'  From ORPD t10 where t10.docentry='" + lsBaseEntry + "' "
        Dim lsOIGEEntry As String = ""
        ioTempSql.ExecuteQuery(lsSql)
        lsOIGEEntry = ioTempSql.GetValue("U_OIGEEntry", 0)
        If lsOIGEEntry <> "" Then
            MyApplication.MessageBox("该订单已经退货到W006，不能添加多次，请重新加载订单查看单号!", 1, "确定")
            lbRef = False
            Return
        End If

        Dim loNetDt As System.Data.DataTable = New System.Data.DataTable
        loNetDt.Columns.Add("ItemCode", GetType(String))   '物料代码
        loNetDt.Columns.Add("Price", GetType(Decimal)) '下单价格
        loNetDt.Columns.Add("Quantity", GetType(Decimal))  '数量
        loNetDt.Columns.Add("BatchNum", GetType(String))  '批次号
        loNetDt.Columns.Add("Location", GetType(String))  '库位
        loNetDt.Columns.Add("BaseEntry", GetType(String))  '退货订单号
        loNetDt.Columns.Add("BaseLine", GetType(String))  '库位
        loNetDt.Columns.Add("CardName", GetType(String))  '供应商名称

        '查询出来当前退货单的所有批次，数量
        Dim lsSQLorpd As String = "select T10.DocEntry ,T11.LineNum ,T10.DocDate,T11.PriceAfVAT as 'Price' ,T21.BatchNum ,T21.Quantity  ," + _
                            " T21.ItemCode ,T12.ItemName ,T10.CardName ,T22.SuppSerial as 'Location'  " + _
                           "  from ORPD T10 inner join RPD1 t11 ON T10.DocEntry =t11.DocEntry " + _
                           "  inner join OITM t12 ON t11.ItemCode=T12.ItemCode " + _
                            "  inner join IBT1 T21 ON t12.ItemCode =T21.ItemCode and t11.LineNum  =T21.BaseLinNum  and  T11.DocEntry =T21.BaseEntry and  t11.WhsCode =T21.WhsCode  and T21.BaseType='21' " + _
                            " inner join OIBT T22 on T21.ItemCode =t22.ItemCode and T21.BatchNum =t22.BatchNum and T21.WhsCode =T22.WhsCode " + _
                                " where T10.DocEntry ='" + lsBaseEntry + "'"
        ioTempSql.ExecuteQuery(lsSQLorpd)

        '搜索当前退货单的批次，库位信息
        If ioTempSql.Rows.Count > 0 Then
            For k As Integer = 0 To ioTempSql.Rows.Count - 1
                lsItemCode = ioTempSql.GetValue("ItemCode", k)  '物料代码
                If String.IsNullOrEmpty(lsItemCode) Then
                    Exit For
                End If
                lsBaseEntry = ioTempSql.GetValue("DocEntry", k)  '退货单号
                lsBaseLine = ioTempSql.GetValue("LineNum", k)  '退货行号
                Decimal.TryParse(ioTempSql.GetValue("Quantity", k), ldQty)
                Decimal.TryParse(ioTempSql.GetValue("Price", k), ldPrice)  '价格
                lsBatchNum = ioTempSql.GetValue("BatchNum", k)  '批次
                lsCardName = ioTempSql.GetValue("CardName", k) '供应商名称
                lsLocation = ioTempSql.GetValue("Location", k) '库位
                loNetRow = loNetDt.NewRow
                loNetRow("ItemCode") = lsItemCode.Trim
                loNetRow("Quantity") = ldQty
                loNetRow("Price") = ldPrice
                loNetRow("BatchNum") = lsBatchNum.Trim
                loNetRow("Location") = lsLocation.Trim
                loNetRow("BaseEntry") = lsBaseEntry.Trim
                loNetRow("BaseLine") = lsBaseLine.Trim.Trim
                loNetRow("CardName") = lsCardName.Trim.Trim
                loNetDt.Rows.Add(loNetRow)
            Next
        End If
        
        '创建库存发货单
        MyApplication.StatusBar.SetText("正在创建库存收货单，请稍后...", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning)
        Dim MyCompany = MyApplication.Company.GetDICompany()  '获取当前公司
        '启动事物，如果不成功，所有单据回滚
        Dim loDoc As SAPbobsCOM.Documents = MyCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry)
        Try
            lsCardName = ""
            Dim i As Integer = 1
            loDoc.DocDate = Today
            loDoc.UserFields.Fields.Item("U_ReceiType").Value = "04"
            Dim liOIGELine As Integer = 0
            For Each loNetRow1 As System.Data.DataRow In loNetDt.Rows
                lsItemCode = loNetRow1("ItemCode")
                ldPrice = loNetRow1("Price")
                ldQty = loNetRow1("Quantity")
                lsBatchNum = loNetRow1("BatchNum")
                lsLocation = loNetRow1("Location")
                lsBaseEntry = loNetRow1("BaseEntry")
                lsBaseLine = loNetRow1("BaseLine")
                lsCardName = loNetRow1("CardName")
                If liOIGELine = 0 Then
                    loDoc.Lines.SetCurrentLine(0)
                Else
                    loDoc.Lines.Add()
                    loDoc.Lines.SetCurrentLine(loDoc.Lines.Count - 1)
                End If
                loDoc.Lines.ItemCode = lsItemCode.Trim
                loDoc.Lines.WarehouseCode = "W006"
                loDoc.Lines.Quantity = ldQty
                loDoc.Lines.AccountCode = "141011"

                loDoc.Lines.LineTotal = 0
                loDoc.Lines.Price = 0
                loDoc.Lines.UnitPrice = 0

                ' loDoc.Lines.StockInmPrice = 0
                loDoc.Lines.UserFields.Fields.Item("U_OrderPrice").Value = ldPrice
                loDoc.Lines.UserFields.Fields.Item("U_AgentBaseEntry").Value = lsBaseEntry
                loDoc.Lines.UserFields.Fields.Item("U_AgentBaseLine").Value = lsBaseLine
                '添加批次
                loDoc.Lines.BatchNumbers.SetCurrentLine(0)
                loDoc.Lines.BatchNumbers.BatchNumber = lsBatchNum.Trim
                loDoc.Lines.BatchNumbers.Quantity = ldQty
                loDoc.Lines.BatchNumbers.Location = lsLocation.Trim
                liOIGELine = liOIGELine + 1
            Next
            '  Dim price As Decimal = loDoc.Lines.Price
            loDoc.UserFields.Fields.Item("U_AgentName").Value = lsCardName.Trim
            loDoc.Reference2 = "代销退回"
            Dim lsErrorOIGE As String
            Dim liErrorCodeOIGE, liOIGEDocEntry As Integer
            lsErrorOIGE = ""
            liOIGEDocEntry = 0
            liErrorCodeOIGE = loDoc.Add()
            MyCompany.GetLastError(liErrorCodeOIGE, lsErrorOIGE)
            '    MyCompany.GetNewObjectCode(liOIGEDocEntry)
            If liErrorCodeOIGE <> 0 Then
                MyApplication.SetStatusBarMessage(lsErrorOIGE, BoMessageTime.bmt_Medium, True)
                lbRef = False
                Return
            End If
            MyApplication.StatusBar.SetText("已创建库存收货单，货物已经成功退回到W006。", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success)

        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
            lbRef = False
        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(loDoc)
        End Try
    End Sub
End Class
