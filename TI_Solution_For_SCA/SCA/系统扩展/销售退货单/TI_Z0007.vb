Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Public NotInheritable Class TI_Z0007
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix

    Public Sub New()

    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function
    Private Sub TI_Z0007_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    ioDbds_ODLN = MyForm.DataSources.DBDataSources.Item("ORDN")
                    '添加打印导出EXCEL的按钮
                    Dim loItem, loItemChoose As Item
                    loItem = MyForm.Items.Add("Export", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1, loBtn_W003approve As Item
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
                                              " where T10.code ='ORDN' and isnull(T11.U_TempName,'') <>''  "
                    ioTempSql.ExecuteQuery(lsSQL)
                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                        lsTempName = ioTempSql.GetValue("U_TempName", i)
                        loCmb_Chooselist.ValidValues.Add(lsTempName, lsTempName)
                    Next
                    If ioTempSql.Rows.Count > 0 Then
                        loCmb_Chooselist.Select("销售退货申请单-仓库", BoSearchKey.psk_ByValue)
                    End If

                    '添加按钮
                    loBtn_Create1 = MyForm.Items.Item("10000329")
                    loItem = MyForm.Items.Add("Copy", BoFormItemTypes.it_BUTTON)
                    loItem.Left = loBtn_Create1.Left
                    loItem.Width = loBtn_Create1.Width
                    loItem.Top = loBtn_Create1.Top - loBtn_Create1.Height - 2
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "10000329"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "复制从已清交货单"

                    loBtn_Create1 = MyForm.Items.Item("10000330")
                    loItem = MyForm.Items.Add("approve", BoFormItemTypes.it_BUTTON)
                    loItem.Left = loBtn_Create1.Left
                    loItem.Width = loBtn_Create1.Width
                    loItem.Top = loBtn_Create1.Top - loBtn_Create1.Height - 2
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "10000330"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "非标品审批"

                    loBtn_Create1 = MyForm.Items.Item("Export")
                    loItem = MyForm.Items.Add("W003app", BoFormItemTypes.it_BUTTON)
                    loItem.Left = loBtn_Create1.Left
                    loItem.Width = loBtn_Create1.Width
                    loItem.Top = loBtn_Create1.Top - loBtn_Create1.Height - 2
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "Export"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "问题仓审批"

                    loBtn_W003approve = MyForm.Items.Item("W003app")
                    loItem = MyForm.Items.Add("W002app", BoFormItemTypes.it_BUTTON)
                    loItem.Left = loBtn_W003approve.Left
                    loItem.Width = loBtn_W003approve.Width
                    loItem.Top = loBtn_W003approve.Top - loBtn_W003approve.Height - 2
                    loItem.Height = loBtn_W003approve.Height
                    loItem.LinkTo = "W003app"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "虚拟仓审批"

                End If
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.Before_Action And pVal.ItemUID = "approve" Then
                    Dim liDocEntry As Integer
                    Integer.TryParse(ioDbds_ODLN.GetValue("DocEntry", 0), liDocEntry)
                    'liDocEntry = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
                    If liDocEntry > 0 Then
                        approve(liDocEntry)
                    End If
                End If
                If Not pVal.Before_Action And pVal.ItemUID = "Copy" Then
                    Dim lsCardCode As String
                    lsCardCode = ioDbds_ODLN.GetValue("CardCode", 0)
                    If Not String.IsNullOrEmpty(lsCardCode) Then
                        lsCardCode = lsCardCode.Trim()
                    End If
                    If String.IsNullOrEmpty(lsCardCode) Then
                        MyApplication.SetStatusBarMessage("请输入客户代码！")
                        Return
                    End If
                    Dim loForm As Form
                    Dim FileName As String
                    FileName = "TI_Solution_For_SCA.TI_Z00071.XML"
                    Dim FileIO As System.IO.Stream
                    FileIO = BaseFunction.GetEmbeddedResource(FileName) '读取资源文件
                    Dim sr As New IO.StreamReader(FileIO)
                    Dim XmlText As String
                    XmlText = sr.ReadToEnd
                    Dim XmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
                    XmlDoc.LoadXml(XmlText)

                    loForm = BaseFunction.londFromXmlString(XmlDoc.InnerXml, MyApplication)

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

                        Dim loobj As TI_Z00071
                        loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)
                        loobj.isCardCode = lsCardCode
                        loobj.isFromUID = pVal.FormUID
                        loobj.DYBL()
                    End If
                End If

                '按导出EXCEL 时将交货单的数据导出到EXCEL
                If Not pVal.Before_Action And pVal.ItemUID = "Export" Then
                    'If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
                    '    MyApplication.MessageBox("只有在确认模式下才能打印!", "确定", "取消")
                    '    Return
                    'End If

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
                                                " where T10.code ='ORDN' and T11.U_tempname='" & lsTempName & "'"
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
                    Dim lsDocEntry As Integer = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)

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
                    doc.PrinterSettings.Copies = 3
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
                        ' oExcelApp.printsettints.copies = 3;
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
                '如果货物退到问题仓，需要发送审批
                If Not pVal.Before_Action And pVal.ItemUID = "W003app" Then
                    Dim liDocEntry As Integer
                    Integer.TryParse(ioDbds_ODLN.GetValue("DocEntry", 0), liDocEntry)
                    'liDocEntry = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
                    If liDocEntry > 0 Then
                        W003approve(liDocEntry)
                    End If
                End If
                '如果正常仓的货物退到虚拟仓，需要发送审批
                If Not pVal.Before_Action And pVal.ItemUID = "W002app" Then
                    Dim liDocEntry As Integer
                    Integer.TryParse(ioDbds_ODLN.GetValue("DocEntry", 0), liDocEntry)
                    'liDocEntry = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
                    If liDocEntry > 0 Then
                        W002approve(liDocEntry)
                    End If
                End If

        End Select
    End Sub

    ''' <summary>
    ''' 问题仓触发审批
    ''' </summary>
    ''' <param name="liDocEntry"></param>
    Private Sub W003approve(liDocEntry As Integer)
        Dim lsSql As String
        lsSql = "Select t10.CardCode from ODRF t10 where t10.DocEntry=" + Convert.ToString(liDocEntry) + " and ObjType='16'"
        ioTempSql.ExecuteQuery(lsSql)
        Dim lsCardCode As String = ioTempSql.GetValue("CardCode", 0)
        'Dim lsCardCode As String = MyForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0)
        'Dim liDocEntry As Integer = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
        If Not String.IsNullOrEmpty(lsCardCode) Then
            lsCardCode = lsCardCode.Trim
        End If
        If Not String.IsNullOrEmpty(lsCardCode) Then
            '是否需要审批
            lsSql = "select 'A' ItemCode from ODRF t1  inner join DRF1 t2 on t2.DocEntry=t1.DocEntry inner join OITM t3 on t2.ItemCode=t3.ItemCode where t1.objtype='16'  and DocStatus='O' and t2.whscode='W003' and t1.DocEntry='" + Convert.ToString(liDocEntry) + "'"
            ioTempSql.ExecuteQuery(lsSql)
            Dim lsItemCode As String
            lsItemCode = ioTempSql.GetValue("ItemCode", 0)
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsItemCode = lsItemCode.Trim
            End If
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsSql = "Select t11.SalerCode from OCRD t10 inner join RW0001 t11 on t10.CardCode='" + lsCardCode + "' and t10.U_Saler=t11.SalerName"
                ioTempSql.ExecuteQuery(lsSql)
                Dim lsSalerCode As String
                lsSalerCode = ioTempSql.GetValue(0, 0)
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    lsSalerCode = lsSalerCode.Trim
                End If
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    Dim lsstring As String
                    Dim loWebAPIRequest As WebAPIRequest(Of MDM007606Request) = New WebAPIRequest(Of MDM007606Request)
                    loWebAPIRequest.Content = New MDM007606Request()
                    loWebAPIRequest.Content.Code = "SP0005"
                    loWebAPIRequest.Content.IsDesignated = "N"
                    loWebAPIRequest.Content.InputJson = ""
                    loWebAPIRequest.Content.BaseType = "OMS0023"
                    loWebAPIRequest.Content.BaseKey = liDocEntry
                    loWebAPIRequest.Content.UserCode = lsSalerCode
                    loWebAPIRequest.UserCode = lsSalerCode
                    'loWebAPIRequest.Content.UserCode = "P0013"
                    'loWebAPIRequest.UserCode = "P0013"
                    'loWebAPIRequest.Content.UserCode = "P0014"
                    'loWebAPIRequest.UserCode = "P0014"
                    lsstring = Newtonsoft.Json.JsonConvert.SerializeObject(loWebAPIRequest)
                    Try
                        Dim lsRString As String = BaseFunction.PostMoths(BaseFunction.isURL + "/MDM0076/MDM007606", lsstring)
                        Dim loWebAPIResponse As WebAPIResponse(Of MDM007606Response) = Newtonsoft.Json.JsonConvert.DeserializeObject(Of WebAPIResponse(Of MDM007606Response))(lsRString)
                        If (loWebAPIResponse.Status <> 200) Then
                            MyApplication.SetStatusBarMessage("审批触发异常(远程),请手动触发审批,错误信息:" + loWebAPIResponse.Message)
                        Else
                            Dim liAppEntry As Long
                            liAppEntry = loWebAPIResponse.Content.DocEntry
                            If liAppEntry > 0 Then
                                MyApplication.StatusBar.SetText("审批触发成功,审批单号:" + liAppEntry.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                                '通过单号请求审批信息
                                Try
                                    lsSql = "Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select " + liAppEntry.ToString() + ",'OMS0023'," + liDocEntry.ToString() + ",GETDATE(),'N','O','SP0005'"
                                    ioTempSql.ExecuteQuery(lsSql)
                                Catch ex As Exception
                                    MyApplication.SetStatusBarMessage("插入审批表异常，请联系IT部,错误信息:" + ex.Message.ToString())
                                End Try
                            End If
                        End If
                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage("审批触发异常(本地),错误信息:" + ex.Message.ToString() + "请联系IT部")
                    End Try
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' 问题仓触发审批
    ''' </summary>
    ''' <param name="liDocEntry"></param>
    Private Sub W002approve(liDocEntry As Integer)
        Dim lsSql As String
        lsSql = "Select t10.CardCode from ODRF t10 where t10.DocEntry=" + Convert.ToString(liDocEntry) + " and ObjType='16'"
        ioTempSql.ExecuteQuery(lsSql)
        Dim lsCardCode As String = ioTempSql.GetValue("CardCode", 0)
        'Dim lsCardCode As String = MyForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0)
        'Dim liDocEntry As Integer = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
        If Not String.IsNullOrEmpty(lsCardCode) Then
            lsCardCode = lsCardCode.Trim
        End If
        If Not String.IsNullOrEmpty(lsCardCode) Then
            '是否需要审批
            lsSql = "exec  YL_W002Approve " + Convert.ToString(liDocEntry)
            ioTempSql.ExecuteQuery(lsSql)
            Dim lsItemCode As String
            lsItemCode = ioTempSql.GetValue("ItemCode", 0)
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsItemCode = lsItemCode.Trim
            End If
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsSql = "Select t11.SalerCode from OCRD t10 inner join RW0001 t11 on t10.CardCode='" + lsCardCode + "' and t10.U_Saler=t11.SalerName"
                ioTempSql.ExecuteQuery(lsSql)
                Dim lsSalerCode As String
                lsSalerCode = ioTempSql.GetValue(0, 0)
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    lsSalerCode = lsSalerCode.Trim
                End If
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    Dim lsstring As String
                    Dim loWebAPIRequest As WebAPIRequest(Of MDM007606Request) = New WebAPIRequest(Of MDM007606Request)
                    loWebAPIRequest.Content = New MDM007606Request()
                    loWebAPIRequest.Content.Code = "SP0007"
                    loWebAPIRequest.Content.IsDesignated = "N"
                    loWebAPIRequest.Content.InputJson = ""
                    loWebAPIRequest.Content.BaseType = "OMS0023"
                    loWebAPIRequest.Content.BaseKey = liDocEntry
                    '  loWebAPIRequest.Content.UserCode = lsSalerCode
                    '  loWebAPIRequest.UserCode = lsSalerCode
                    loWebAPIRequest.Content.UserCode = "P0013"
                    loWebAPIRequest.UserCode = "P0013"
                    'loWebAPIRequest.Content.UserCode = "P0014"
                    'loWebAPIRequest.UserCode = "P0014"
                    lsstring = Newtonsoft.Json.JsonConvert.SerializeObject(loWebAPIRequest)
                    Try
                        Dim lsRString As String = BaseFunction.PostMoths(BaseFunction.isURL + "/MDM0076/MDM007606", lsstring)
                        Dim loWebAPIResponse As WebAPIResponse(Of MDM007606Response) = Newtonsoft.Json.JsonConvert.DeserializeObject(Of WebAPIResponse(Of MDM007606Response))(lsRString)
                        If (loWebAPIResponse.Status <> 200) Then
                            MyApplication.SetStatusBarMessage("审批触发异常(远程),请手动触发审批,错误信息:" + loWebAPIResponse.Message)
                        Else
                            Dim liAppEntry As Long
                            liAppEntry = loWebAPIResponse.Content.DocEntry
                            If liAppEntry > 0 Then
                                MyApplication.StatusBar.SetText("审批触发成功,审批单号:" + liAppEntry.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                                '通过单号请求审批信息
                                Try
                                    lsSql = "Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select " + liAppEntry.ToString() + ",'OMS0023'," + liDocEntry.ToString() + ",GETDATE(),'N','O','SP0007'"
                                    ioTempSql.ExecuteQuery(lsSql)
                                Catch ex As Exception
                                    MyApplication.SetStatusBarMessage("插入审批表异常，请联系IT部,错误信息:" + ex.Message.ToString())
                                End Try
                            End If
                        End If
                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage("审批触发异常(本地),错误信息:" + ex.Message.ToString() + "请联系IT部")
                    End Try
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' 触发审批
    ''' </summary>
    ''' <param name="liDocEntry"></param>
    Private Sub approve(liDocEntry As Integer)
        Dim lsSql As String
        lsSql = "Select t10.CardCode from ODRF t10 where t10.DocEntry=" + Convert.ToString(liDocEntry) + " and ObjType='16'"
        ioTempSql.ExecuteQuery(lsSql)
        Dim lsCardCode As String = ioTempSql.GetValue("CardCode", 0)
        'Dim lsCardCode As String = MyForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0)
        '   Dim liDocEntry As Integer = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
        If Not String.IsNullOrEmpty(lsCardCode) Then
            lsCardCode = lsCardCode.Trim
        End If
        If Not String.IsNullOrEmpty(lsCardCode) Then
            '是否需要审批
            lsSql = "select 'A' ItemCode from ODRF t1  inner join DRF1 t2 on t2.DocEntry=t1.DocEntry inner join OITM t3 on t2.ItemCode=t3.ItemCode where t1.objtype='16'  and DocStatus='O' and isnull(t3.u_ItemLevel,'标准件')<>'标准件' and t1.DocEntry='" + Convert.ToString(liDocEntry) + "'"
            ioTempSql.ExecuteQuery(lsSql)
            Dim lsItemCode As String
            lsItemCode = ioTempSql.GetValue("ItemCode", 0)
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsItemCode = lsItemCode.Trim
            End If
            If Not String.IsNullOrEmpty(lsItemCode) Then
                lsSql = "Select t11.SalerCode from OCRD t10 inner join RW0001 t11 on t10.CardCode='" + lsCardCode + "' and t10.U_Saler=t11.SalerName"
                ioTempSql.ExecuteQuery(lsSql)
                Dim lsSalerCode As String
                lsSalerCode = ioTempSql.GetValue(0, 0)
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    lsSalerCode = lsSalerCode.Trim
                End If
                If Not String.IsNullOrEmpty(lsSalerCode) Then
                    Dim lsstring As String
                    Dim loWebAPIRequest As WebAPIRequest(Of MDM007606Request) = New WebAPIRequest(Of MDM007606Request)
                    loWebAPIRequest.Content = New MDM007606Request()
                    loWebAPIRequest.Content.Code = "SP0002"
                    loWebAPIRequest.Content.IsDesignated = "N"
                    loWebAPIRequest.Content.InputJson = ""
                    loWebAPIRequest.Content.BaseType = "OMS0023"
                    loWebAPIRequest.Content.BaseKey = liDocEntry
                    loWebAPIRequest.Content.UserCode = lsSalerCode
                    loWebAPIRequest.UserCode = lsSalerCode
                    'loWebAPIRequest.Content.UserCode = "P0014"
                    'loWebAPIRequest.UserCode = "P0014"
                    lsstring = Newtonsoft.Json.JsonConvert.SerializeObject(loWebAPIRequest)
                    Try
                        Dim lsRString As String = BaseFunction.PostMoths(BaseFunction.isURL + "/MDM0076/MDM007606", lsstring)
                        Dim loWebAPIResponse As WebAPIResponse(Of MDM007606Response) = Newtonsoft.Json.JsonConvert.DeserializeObject(Of WebAPIResponse(Of MDM007606Response))(lsRString)
                        If (loWebAPIResponse.Status <> 200) Then
                            MyApplication.SetStatusBarMessage("审批触发异常(远程),请手动触发审批,错误信息:" + loWebAPIResponse.Message)
                        Else
                            Dim liAppEntry As Long
                            liAppEntry = loWebAPIResponse.Content.DocEntry
                            If liAppEntry > 0 Then
                                MyApplication.StatusBar.SetText("审批触发成功,审批单号:" + liAppEntry.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                                '通过单号请求审批信息
                                Try
                                    lsSql = "Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select " + liAppEntry.ToString() + ",'OMS0023'," + liDocEntry.ToString() + ",GETDATE(),'N','O','SP0002'"
                                    ioTempSql.ExecuteQuery(lsSql)
                                Catch ex As Exception
                                    MyApplication.SetStatusBarMessage("插入审批表异常，请联系IT部,错误信息:" + ex.Message.ToString())
                                End Try
                            End If
                        End If
                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage("审批触发异常(本地),错误信息:" + ex.Message.ToString() + "请联系IT部")
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub TI_Z0007_FormDataEvent(ByRef BusinessObjectInfo As BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_ADD
                If Not BusinessObjectInfo.BeforeAction Then
                    Dim lsObjType As String = BusinessObjectInfo.Type
                    If lsObjType = "112" Then
                        Dim liDocEntry As Integer
                        'Integer.TryParse(BusinessObjectInfo.ObjectKey, liDocEntry)
                        Dim XmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
                        XmlDoc.LoadXml(BusinessObjectInfo.ObjectKey)
                        Dim XmlNode As Xml.XmlNode = XmlDoc.SelectSingleNode("/DocumentParams/DocEntry")

                        Integer.TryParse(XmlNode.FirstChild.Value, liDocEntry)
                        '    Integer.TryParse(MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0), liDocEntry)
                        If liDocEntry > 0 Then
                            approve(liDocEntry)
                            W003approve(liDocEntry)
                            W002approve(liDocEntry)
                        End If
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_UPDATE

        End Select
    End Sub
End Class
