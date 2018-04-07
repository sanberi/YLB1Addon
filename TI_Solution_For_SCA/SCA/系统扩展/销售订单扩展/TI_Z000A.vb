Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Public NotInheritable Class TI_Z000A
    Inherits FormBase
    Private ioDbds_DLN1, ioDbds_ODLN As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix
    '  Private isLadPath As String

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function


    Public Shared iiMemuCount_ASN As Integer = 1

    Private Sub TI_Z0001_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    ioDbds_ODLN = MyForm.DataSources.DBDataSources.Item("ORDR")
                    '添加打印导出EXCEL的按钮
                    Dim loItem, loItemChoose As Item
                    loItem = MyForm.Items.Add("Export", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1 As Item
                    Dim loBtn_Export As SAPbouiCOM.Button
                    loBtn_Create1 = MyForm.Items.Item("10000330")
                    'loItem3 = MyForm.Items.Item("10000329")

                    loItem.Left = loBtn_Create1.Left - 100 - 5 - 60
                    loItem.Width = 60
                    loItem.Top = loBtn_Create1.Top
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "10000330"
                    loBtn_Export = loItem.Specific
                    loBtn_Export.Caption = "打印"

                    '添加打印选项
                    loItemChoose = MyForm.Items.Add("ChooseList", BoFormItemTypes.it_COMBO_BOX)
                    Dim loCmb_Chooselist As SAPbouiCOM.ComboBox
                    loItemChoose.Left = loItem.Left - 80 - 5
                    loItemChoose.Width = 80
                    loItemChoose.Top = loItem.Top
                    loItemChoose.Height = loItem.Height
                    loItemChoose.AffectsFormMode = False
                    loItemChoose.LinkTo = "Export"
                    loCmb_Chooselist = loItemChoose.Specific
                    Dim lsTempName As String
                    Dim lsSQL As String = "select T10.U_TempPath,T11.U_Template,T11.U_PrintName,T11.U_PageSize,T11.U_TempName " &
                                            "   from [@ti_z0010] T10 inner join [@ti_z0011] T11 On t10.Code=t11.code  " &
                                              " where T10.code ='ORDR' and isnull(T11.U_TempName,'') <>''  "
                    ioTempSql.ExecuteQuery(lsSQL)
                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                        lsTempName = ioTempSql.GetValue("U_TempName", i)
                        loCmb_Chooselist.ValidValues.Add(lsTempName, lsTempName)
                    Next
                    loCmb_Chooselist.Select("销售合同", BoSearchKey.psk_ByValue)

                    loBtn_Create1 = MyForm.Items.Item("10000329")
                    loItemChoose = MyForm.Items.Add("approve", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_approve As SAPbouiCOM.Button
                    loItemChoose.Left = loBtn_Create1.Left
                    loItemChoose.Width = loBtn_Create1.Width
                    loItemChoose.Top = loBtn_Create1.Top - loBtn_Create1.Height - 5
                    loItemChoose.Height = loBtn_Create1.Height
                    loItemChoose.AffectsFormMode = False
                    loItemChoose.LinkTo = "10000329"
                    loBtn_approve = loItemChoose.Specific
                    loBtn_approve.Caption = "触发审批"
                End If
            Case BoEventTypes.et_ITEM_PRESSED
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
                    If lsTempName = "" Then
                        Return
                        BubbleEvent = False
                    End If
                    Dim lsSQL As String = "select T10.U_TempPath,T11.U_Template,T11.U_PrintName,T11.U_PageSize " &
                                              "   from [@ti_z0010] T10 inner join [@ti_z0011] T11 On t10.Code=t11.code  " &
                                                " where T10.code ='ORDR' and T11.U_tempname='" & lsTempName & "'"
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
                    Dim lsU_PsizeID As String = lsPageSize   '
                    Dim lsSourcePath As String  '原始表格路径
                    lsSourcePath = lsU_PsizeID

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
                ElseIf Not pVal.Before_Action And pVal.ItemUID = "approve" Then
                    '触发审批
                    If MyForm.Mode = BoFormMode.fm_OK_MODE Then
                        Dim liBaseKey As Integer
                        liBaseKey = ioDbds_ODLN.GetValue("DocEntry", 0)
                        If liBaseKey > 0 Then
                            Dim lsCardCode As String = ioDbds_ODLN.GetValue("CardCode", 0)
                            If Not String.IsNullOrEmpty(lsCardCode) Then
                                lsCardCode = lsCardCode.Trim
                            End If
                            If Not String.IsNullOrEmpty(lsCardCode) Then
                                Dim lsSql As String
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
                                    loWebAPIRequest.Content.Code = "SP0001"
                                    loWebAPIRequest.Content.IsDesignated = "N"
                                    loWebAPIRequest.Content.InputJson = ""
                                    loWebAPIRequest.Content.BaseType = "OMS0001"
                                    loWebAPIRequest.Content.BaseKey = liBaseKey
                                    loWebAPIRequest.Content.UserCode = lsSalerCode
                                    lsstring = Newtonsoft.Json.JsonConvert.SerializeObject(loWebAPIRequest)
                                    Try
                                        Dim lsRString As String = BaseFunction.PostMoths(BaseFunction.isURL + "/MDM0076/MDM007606", lsstring)
                                        Dim loWebAPIResponse As WebAPIResponse(Of MDM007606Response) = Newtonsoft.Json.JsonConvert.DeserializeObject(Of WebAPIResponse(Of MDM007606Response))(lsRString)
                                        If (loWebAPIResponse.Status <> 200) Then
                                            MyApplication.SetStatusBarMessage("审批触发异常(远程),错误信息:" + loWebAPIResponse.Message)
                                        Else
                                            Dim liAppEntry As Long
                                            liAppEntry = loWebAPIResponse.Content.DocEntry
                                            If liAppEntry > 0 Then
                                                MyApplication.StatusBar.SetText("审批触发成功,审批单号:" + liAppEntry.ToString(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                                                '通过单号请求审批信息
                                                Try
                                                    lsSql = "Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select " + liAppEntry.ToString() + ",'OMS0001'," + liBaseKey.ToString() + ",GETDATE(),'N','O','SP0001'"
                                                    ioTempSql.ExecuteQuery(lsSql)
                                                Catch ex As Exception
                                                    MyApplication.SetStatusBarMessage("插入审批表异常，请联系IT部,错误信息:" + ex.Message.ToString())
                                                End Try
                                            End If
                                        End If
                                    Catch ex As Exception
                                        MyApplication.SetStatusBarMessage("审批触发异常(本地),错误信息:" + ex.Message.ToString())
                                    End Try
                                End If
                            End If
                        End If
                    End If
                End If
            Case BoEventTypes.et_DOUBLE_CLICK
                If Not pVal.Before_Action And pVal.ItemUID = "38" Then
                    If pVal.ColUID = "1" Then
                        '复制从采购收货单
                        Dim lsCardCode, lsItemCode As String
                        lsCardCode = MyForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0)
                        Dim loMatrix As Matrix
                        loMatrix = MyForm.Items.Item("38").Specific
                        If Not pVal.Before_Action Then
                            lsItemCode = loMatrix.Columns.Item("1").Cells.Item(pVal.Row).Specific.Value
                            ' Dim lsCardCode As String = ioDbs_TI_Z0340.GetValue("U_CardCode", 0)
                            Dim lsSQLDetail As String = "ODLNRecord '" + lsCardCode.Trim + "','" + lsItemCode.Trim + "'"
                            Dim loForm As SAPbouiCOM.Form
                            Dim FileName As String
                            FileName = "TI_Solution_For_SCA.TI_Z000B.XML"
                            Dim FileIO As System.IO.Stream
                            FileIO = BaseFunction.GetEmbeddedResource(FileName) '读取资源文件
                            Dim sr As New IO.StreamReader(FileIO)
                            Dim XmlText As String
                            XmlText = sr.ReadToEnd
                            loForm = BaseFunction.londFromXmlString(XmlText, MyApplication)
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
                                Dim loobj As TI_Z000B
                                loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)
                                loForm.Freeze(True)
                                Try
                                    Dim loSubioMtx_10 As Matrix
                                    loSubioMtx_10 = loForm.Items.Item("Mtx_10").Specific
                                    ioTempSql = loForm.DataSources.DataTables.Add("TempDt")
                                    Dim ioDtDoc As SAPbouiCOM.DataTable = loForm.DataSources.DataTables.Item("DOC")
                                    ioTempSql.ExecuteQuery(lsSQLDetail)
                                    ioDtDoc.Rows.Clear()
                                    '加载数据
                                    Dim lsLineId, lsItemCodeSub, lsItemNameSub, lsbaseentry, lsbaseline, lscardcodesub, lscardnamesub, lsCreatedate, lsAliasName As String
                                    Dim ldQty, ldPrice As Decimal
                                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                                        '    loSubioMtx_10.GetLineData(i + 1)
                                        ioDtDoc.Rows.Add()
                                        ioDtDoc.Rows.Offset = ioDtDoc.Rows.Count - 1
                                        lsLineId = ioTempSql.GetValue("LineId", i)
                                        lsItemCodeSub = ioTempSql.GetValue("ItemCode", i)
                                        lsItemNameSub = ioTempSql.GetValue("itemname", i)
                                        lsbaseentry = ioTempSql.GetValue("docentry", i)
                                        lsbaseline = ioTempSql.GetValue("LineNum", i)
                                        lscardcodesub = ioTempSql.GetValue("CardCode", i)
                                        lscardnamesub = ioTempSql.GetValue("CardName", i)
                                        lsCreatedate = ioTempSql.GetValue("CreateDate", i)
                                        ldQty = ioTempSql.GetValue("Quantity", i)
                                        ldPrice = ioTempSql.GetValue("PriceAfVAT", i)
                                        lsAliasName = ioTempSql.GetValue("u_itemalias", i)
                                        '  ioDtDoc.SetValue("LineId", i, lsLineId)
                                        ioDtDoc.SetValue("ItemCode", i, lsItemCodeSub)
                                        ioDtDoc.SetValue("itemname", i, lsItemNameSub)
                                        ioDtDoc.SetValue("docentry", i, lsbaseentry)
                                        ioDtDoc.SetValue("LineNum", i, lsbaseline)
                                        ioDtDoc.SetValue("CardCode", i, lscardcodesub)
                                        ioDtDoc.SetValue("CardName", i, lscardnamesub)
                                        ioDtDoc.SetValue("CreateDate", i, lsCreatedate)
                                        ioDtDoc.SetValue("Quantity", i, ldQty.ToString())
                                        ioDtDoc.SetValue("PriceAfVAT", i, ldPrice.ToString())
                                        ioDtDoc.SetValue("u_itemalias", i, lsAliasName)
                                        '   loSubioMtx_10.GetLineData(i + 1)
                                    Next
                                    loSubioMtx_10.LoadFromDataSource()
                                Catch ex As Exception
                                    MyApplication.SetStatusBarMessage(ex.ToString())
                                    BubbleEvent = False
                                Finally
                                    loForm.Freeze(False)
                                End Try
                            End If
                        End If
                    End If

                    '双击物料名称，查找所有历史购买过的物料的记录
                    If pVal.ColUID = "3" Then
                        '复制从采购收货单
                        Dim lsCardCode, lsItemCode, lsItemName As String
                        lsCardCode = MyForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0)
                        Dim loMatrix As Matrix
                        loMatrix = MyForm.Items.Item("38").Specific
                        If Not pVal.Before_Action Then
                            lsItemName = loMatrix.Columns.Item("3").Cells.Item(pVal.Row).Specific.Value
                            ' Dim lsCardCode As String = ioDbs_TI_Z0340.GetValue("U_CardCode", 0)
                            lsItemName = Replace(lsItemName, "*", "")
                            Dim lsSQLDetail As String = "ODLNRecordByName '" + lsCardCode.Trim + "','" + lsItemName.Trim + "'"
                            Dim loForm As SAPbouiCOM.Form
                            Dim FileName As String
                            FileName = "TI_Solution_For_SCA.TI_Z000C.XML"
                            Dim FileIO As System.IO.Stream
                            FileIO = BaseFunction.GetEmbeddedResource(FileName) '读取资源文件
                            Dim sr As New IO.StreamReader(FileIO)
                            Dim XmlText As String
                            XmlText = sr.ReadToEnd
                            loForm = BaseFunction.londFromXmlString(XmlText, MyApplication)
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
                                Dim loobj As TI_Z000C
                                loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)
                                loForm.Freeze(True)
                                Try
                                    Dim loSubioMtx_10 As Matrix
                                    loSubioMtx_10 = loForm.Items.Item("Mtx_10").Specific
                                    ioTempSql = loForm.DataSources.DataTables.Add("TempDt")
                                    Dim ioDtDoc As SAPbouiCOM.DataTable = loForm.DataSources.DataTables.Item("DOC")
                                    ioTempSql.ExecuteQuery(lsSQLDetail)
                                    ioDtDoc.Rows.Clear()
                                    '加载数据
                                    Dim lsLineId, lsItemCodeSub, lsItemNameSub, lsbaseentry, lsbaseline, lscardcodesub, lscardnamesub, lsCreatedate, lsAliasName As String
                                    Dim ldQty, ldPrice As Decimal
                                    For i As Integer = 0 To ioTempSql.Rows.Count - 1
                                        '    loSubioMtx_10.GetLineData(i + 1)
                                        ioDtDoc.Rows.Add()
                                        ioDtDoc.Rows.Offset = ioDtDoc.Rows.Count - 1
                                        lsItemCodeSub = ioTempSql.GetValue("ItemCode", i)
                                        lsItemNameSub = ioTempSql.GetValue("itemname", i)
                                        lsbaseentry = ioTempSql.GetValue("docentry", i)
                                        lsbaseline = ioTempSql.GetValue("LineNum", i)
                                        lscardcodesub = ioTempSql.GetValue("CardCode", i)
                                        lscardnamesub = ioTempSql.GetValue("CardName", i)
                                        lsCreatedate = ioTempSql.GetValue("CreateDate", i)
                                        lsAliasName = ioTempSql.GetValue("u_itemalias", i)
                                        lsLineId = ioTempSql.GetValue("LineId", i)
                                        ldQty = ioTempSql.GetValue("Quantity", i)
                                        ldPrice = ioTempSql.GetValue("PriceAfVAT", i)
                                        ' ioDtDoc.SetValue("LineId", i, lsLineId)
                                        ioDtDoc.SetValue("ItemCode", i, lsItemCodeSub)
                                        ioDtDoc.SetValue("itemname", i, lsItemNameSub)
                                        ioDtDoc.SetValue("docentry", i, lsbaseentry)
                                        ioDtDoc.SetValue("LineNum", i, lsbaseline)
                                        ioDtDoc.SetValue("CardCode", i, lscardcodesub)
                                        ioDtDoc.SetValue("CardName", i, lscardnamesub)
                                        ioDtDoc.SetValue("CreateDate", i, lsCreatedate)
                                        ioDtDoc.SetValue("Quantity", i, ldQty.ToString())
                                        ioDtDoc.SetValue("PriceAfVAT", i, ldPrice.ToString())
                                        ioDtDoc.SetValue("u_itemalias", i, lsAliasName)
                                        '   loSubioMtx_10.GetLineData(i + 1)
                                    Next
                                    loSubioMtx_10.LoadFromDataSource()
                                Catch ex As Exception
                                    MyApplication.SetStatusBarMessage(ex.ToString())
                                    BubbleEvent = False
                                Finally
                                    loForm.Freeze(False)
                                End Try
                            End If
                        End If
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

    Private Sub TI_Z0001_MenuEvent(pVal As IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent

    End Sub
End Class