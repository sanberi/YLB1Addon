Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices


Public NotInheritable Class TI_Z0003
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function


    Private Sub TI_Z0003_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    '添加打印导出EXCEL的按钮
                    Dim loItem, loItemChoose As Item
                    loItem = MyForm.Items.Add("Export", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1 As Item
                    Dim loBtn_Export As SAPbouiCOM.Button
                    loBtn_Create1 = MyForm.Items.Item("1")
                    'loItem3 = MyForm.Items.Item("10000329")

                    loItem.Left = loBtn_Create1.Left + 300
                    loItem.Width = 100
                    loItem.Top = loBtn_Create1.Top
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "1"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "打印"

                    '添加打印选项
                    loItemChoose = MyForm.Items.Add("ChooseList", BoFormItemTypes.it_COMBO_BOX)
                    Dim loCmb_Chooselist As SAPbouiCOM.ComboBox
                    loItemChoose.Left = loItem.Left + 100 + 5
                    loItemChoose.Width = 100
                    loItemChoose.Top = loItem.Top
                    loItemChoose.Height = loItem.Height
                    loItemChoose.AffectsFormMode = False
                    loItemChoose.LinkTo = "Export"
                    loCmb_Chooselist = loItemChoose.Specific
                    Dim lsTempName As String
                    Dim lsSQL As String = "select T10.U_TempPath,T11.U_Template,T11.U_PrintName,T11.U_PageSize,T11.U_TempName " &
                                            "   from [@ti_z0010] T10 inner join [@ti_z0011] T11 On t10.Code=t11.code  " &
                                              " where T10.code ='OIGE' and isnull(T11.U_TempName,'') <>'' "
                    ioTempSql.ExecuteQuery(lsSQL)
                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                        lsTempName = ioTempSql.GetValue("U_TempName", i)
                        loCmb_Chooselist.ValidValues.Add(lsTempName, lsTempName)
                    Next
                    loCmb_Chooselist.Select("库存发货单", BoSearchKey.psk_ByValue)
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
                                                " where T10.code ='OIGE' and T11.U_tempname='" & lsTempName & "'"
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
                    Dim liDocEntry As Integer = MyForm.Items.Item("7").Specific.value

                    Dim lsTargetFile As String = lsServicePath + lsTempEXCEL
                    Dim lsU_Printer As String = lsPrintName  '打印机名称
                    Dim lsU_PsizeID As String = lsPageSize   '打印机纸张
                    Dim lsSourcePath As String  '原始表格路径
                    lsSourcePath = lsPageSize

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
                    doc.DefaultPageSettings.PrinterSettings.Copies = 1
                    doc.PrinterSettings.Copies = 1
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
        End Select
    End Sub
End Class
