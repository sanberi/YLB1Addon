Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Public NotInheritable Class TI_Z0004
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function
    Public Shared iiMemuCount_134 As Integer = 1

    Private Sub TI_Z0004_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    '添加打印导出EXCEL的按钮
                    Dim loItem, loItemChoose As Item
                    loItem = MyForm.Items.Add("Export", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1 As Item
                    Dim loBtn_Export As SAPbouiCOM.Button
                    loBtn_Create1 = MyForm.Items.Item("271")
                    'loItem3 = MyForm.Items.Item("10000329")

                    loItem.Left = loBtn_Create1.Left - 100 - 5
                    loItem.Width = 100
                    loItem.Top = loBtn_Create1.Top
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "271"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "打印"

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
                                              " where T10.code ='OCRD' and isnull(T11.U_TempName,'') <>''  "
                    ioTempSql.ExecuteQuery(lsSQL)
                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                        lsTempName = ioTempSql.GetValue("U_TempName", i)
                        loCmb_Chooselist.ValidValues.Add(lsTempName, lsTempName)
                    Next
                    If ioTempSql.Rows.Count > 0 Then
                        loCmb_Chooselist.Select("付款申请单", BoSearchKey.psk_ByValue)
                    End If
                    '右键添加业务伙伴物料单位
                    Dim loMenus As SAPbouiCOM.Menus
                    loMenus = MyForm.Menu
                    If Not loMenus Is Nothing Then
                        Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
                        oCreationPackage = MyApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
                        oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                        oCreationPackage.UniqueID = "TI_OCRDUni_134" + Convert.ToString(iiMemuCount_134)
                        oCreationPackage.String = "业务伙伴物料单位"
                        oCreationPackage.Enabled = True
                        MyForm.Menu.AddEx(oCreationPackage)
                        iiMemuCount_134 += 1

                    End If
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
                                                " where T10.code ='OCRD' and T11.U_tempname='" & lsTempName & "'"
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
                    Dim lsdocentry As String = MyForm.Items.Item("5").Specific.value

                    Dim lsTargetFile As String = lsServicePath + lsTempEXCEL
                    Dim lsU_Printer As String = lsPrintName  '打印机名称
                    Dim lsU_PsizeID As String = lsPageSize   '
                    Dim lsSourcePath As String  '原始表格路径
                    lsSourcePath = lsU_PsizeID

                    '  If lsTempName <> "条码" Then
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

                    '  File.Copy(lsSourcePath, lsTargetFile, True)

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
                        oExcelApp.Run("GetDataString", lsdocentry)
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
                    '   End If


                End If
        End Select
    End Sub

    Private Sub TI_Z0004_MenuEvent(pVal As IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent
        If MyForm.Mode <> BoFormMode.fm_FIND_MODE Then
            If pVal.BeforeAction Then
                If pVal.MenuUID.Length >= 15 Then
                    Dim lsMenu As String
                    lsMenu = Left(pVal.MenuUID, 14)
                    Select lsMenu
                        Case "TI_OCRDUni_134"
                            LoadFromTI_Z0800()
                    End Select
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' 跳出窗体(申请单加载)
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub LoadFromTI_Z0800()
        Try
            Dim loForm As Form
            Dim FileName As String
            FileName = "TI_Solution_For_SCA.TI_Z0012.XML"
            Dim FileIO As System.IO.Stream
            FileIO = BaseFunction.GetEmbeddedResource(FileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            Dim XmlText As String
            XmlText = sr.ReadToEnd
            Dim loXmlDoc As Xml.XmlDocument = New Xml.XmlDocument
            loXmlDoc.LoadXml(XmlText)
            XmlText = loXmlDoc.InnerXml
            loForm = BaseFunction.londFromXmlString(XmlText, MyApplication)

            'SQL 
            '获取当前界面上的客户代码和客户名称
            Dim lsCardCode As String = MyForm.Items.Item("5").Specific.value
            Dim lsCardName As String = MyForm.Items.Item("7").Specific.value
            If Not loForm Is Nothing Then

                Dim Tag(0) As Object
                Tag(0) = MyForm.UniqueID
                If Not ItemDispacher.ioFormTag.ContainsKey(loForm.UniqueID) Then
                    ItemDispacher.ioFormTag.Add(loForm.UniqueID, Tag)
                Else
                    ItemDispacher.ioFormTag.Item(loForm.UniqueID) = Tag
                End If
                '确认子父窗体关系
                If Not ItemDispacher.ioFormSon.ContainsKey(MyForm.UniqueID) Then
                    ItemDispacher.ioFormSon.Add(MyForm.UniqueID, loForm.UniqueID)
                End If

                Dim loobj As TI_Z0012
                loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)
                loobj.ioMatrix = loForm.Items.Item("7").Specific
                loobj.ioDtDocSub = loForm.DataSources.DataTables.Add("DOC")
                loobj.ioDbds_TI_Z0800 = loForm.DataSources.DBDataSources.Add("@TI_Z0800")
                loobj.ioDbds_TI_Z0801 = loForm.DataSources.DBDataSources.Add("@TI_Z0801")
                loobj.isCardCode = lsCardCode.Trim

                '如果TI_Z0800表里没有
                Dim lsTemp As String
                Dim lsSQLTI_Z0800 As String = "select 'A' as 'Test' from [@TI_Z0800] where Code='" + lsCardCode.Trim + "'"
                ioTempSql.ExecuteQuery(lsSQLTI_Z0800)
                If ioTempSql.Rows.Count > 0 Then
                    lsTemp = ioTempSql.GetValue("Test", 0)
                    If Not String.IsNullOrEmpty(lsTemp) Then
                        '存在，直接加载该界面
                        Dim loConditions As SAPbouiCOM.Conditions
                        Dim loCondition As SAPbouiCOM.Condition
                        loConditions = MyApplication.CreateObject(BoCreatableObjectType.cot_Conditions)
                        loCondition = loConditions.Add
                        loCondition.Alias = "Code"
                        loCondition.Operation = BoConditionOperation.co_EQUAL
                        loCondition.CondVal = Convert.ToString(lsCardCode.Trim)
                        loobj.ioDbds_TI_Z0800.Query(loConditions)
                        loobj.ioDbds_TI_Z0801.Query(loConditions)
                        loForm.Items.Item("1").Click(BoCellClickType.ct_Regular)
                    Else
                        '不存在，将客户代码和名称放在弹出的界面

                        loForm.Mode = BoFormMode.fm_ADD_MODE
                        loobj.ioDbds_TI_Z0800.SetValue("Code", 0, lsCardCode.Trim)
                        loobj.ioDbds_TI_Z0800.SetValue("U_CardName", 0, lsCardName.Trim)
                        '添加一个空行
                        loForm.Items.Item("7").AffectsFormMode = False
                        loobj.ioDbds_TI_Z0801.InsertRecord(loobj.ioDbds_TI_Z0801.Size)
                        loobj.ioDbds_TI_Z0801.Offset = loobj.ioDbds_TI_Z0801.Size - 1
                        loobj.ioMatrix.AddRow(1, loobj.ioMatrix.VisualRowCount)
                        loForm.Items.Item("7").AffectsFormMode = True

                    End If

                End If

            End If
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString)
        End Try
    End Sub
End Class
