Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices
Imports B1Extra
Imports Microsoft.VisualBasic.CompilerServices
Imports Newtonsoft.Json

Public NotInheritable Class TI_Z0100
    Inherits FormBase
    Private ioDbds_TI_Z0100, ioDbds_TI_Z0101 As DBDataSource
    Private ioTempSql As SAPbouiCOM.DataTable
    Private ioMtx_10 As Matrix
    Private ioBtn_Copy, ioBtn_ChangePri, ioBtn_Approve As SAPbouiCOM.Button


    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function


    Private Sub TI_Z0081_FormDataEvent(ByRef BusinessObjectInfo As BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_LOAD
                If Not BusinessObjectInfo.BeforeAction Then
                    Dim myStr As String = MyForm.GetAsXML
                    '判断如果单据是已清，单据不可再编辑，如果未清，可以编辑
                    Dim lsDocstatus As String
                    lsDocstatus = ioDbds_TI_Z0100.GetValue("Status", 0)
                    If lsDocstatus.Trim = "O" Then
                        MyForm.Mode = BoFormMode.fm_OK_MODE
                    Else
                        MyForm.Mode = BoFormMode.fm_VIEW_MODE
                    End If

                End If
            Case BoEventTypes.et_FORM_DATA_ADD
                If ioDbds_TI_Z0101.Size = 1 Then
                    'MyApplication.SetStatusBarMessage("无有效数据！", BoMessageTime.bmt_Short, True)
                    MyApplication.MessageBox("无有效数据！")
                    BubbleEvent = False
                End If
        End Select

    End Sub


    Private Sub TI_Z0081_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent

        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If Not pVal.Before_Action Then
                    ioTempSql = MyForm.DataSources.DataTables.Add("TempSql")
                    ioDbds_TI_Z0100 = MyForm.DataSources.DBDataSources.Item("@TI_Z0100")
                    ioDbds_TI_Z0101 = MyForm.DataSources.DBDataSources.Item("@TI_Z0101")

                End If
            Case BoEventTypes.et_CHOOSE_FROM_LIST
                If Not pVal.BeforeAction Then
                    Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
                    Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
                    If Not lodt Is Nothing Then
                        Select Case pVal.ItemUID
                            Case "CardCode", "CardName"
                                Dim lsCardCode As String
                                Dim lsCardName As String
                                lsCardCode = lodt.GetValue("CardCode", 0)
                                lsCardName = lodt.GetValue("CardName", 0)
                                ioDbds_TI_Z0100.SetValue("U_CardCode", 0, lsCardCode.Trim)
                                ioDbds_TI_Z0100.SetValue("U_CardName", 0, lsCardName.Trim)
                        End Select
                    End If
                End If
            Case BoEventTypes.et_ITEM_PRESSED
                'Find
                If Not pVal.Before_Action And MyForm.Mode = BoFormMode.fm_FIND_MODE And pVal.ItemUID = "1" Then
                    Dim myConditions As Conditions = New Conditions()
                    Dim oCondition As Condition = myConditions.Add
                    'Dim curDocNum As String = ioDbds_TI_Z0100.GetValue("DocNum", 0)
                    Dim curDocNum As String = MyForm.Items.Item("DocNum").Specific.Value
                    If String.IsNullOrEmpty(curDocNum) Then
                        MyApplication.SetStatusBarMessage("请输入单号！")
                        Return
                    End If
                    oCondition.Alias = "DocEntry"
                    oCondition.Operation = BoConditionOperation.co_EQUAL
                    oCondition.CondVal = curDocNum
                    ioDbds_TI_Z0100.Query(myConditions)
                    ioDbds_TI_Z0101.Query(myConditions)
                    Dim myMatrix As Matrix = MyForm.Items.Item("Mtx_10").Specific
                    myMatrix.LoadFromDataSource()
                End If
                '复制从没有做过发票和退货的交货
                If Not pVal.Before_Action And pVal.ItemUID = "CopyFODLN" Then
                    Dim lsCardCode As String
                    lsCardCode = ioDbds_TI_Z0100.GetValue("U_CardCode", 0)
                    If Not String.IsNullOrEmpty(lsCardCode) Then
                        lsCardCode = lsCardCode.Trim()
                    End If
                    If String.IsNullOrEmpty(lsCardCode) Then
                        MyApplication.SetStatusBarMessage("请输入客户代码！")
                        Return
                    End If
                    Dim loForm As Form
                    Dim FileName As String
                    FileName = "TI_Solution_For_SCA.TI_Z0101.XML"
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

                        Dim loobj As TI_Z0101
                        loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)
                        loobj.isCardCode = lsCardCode
                        loobj.isFromUID = pVal.FormUID
                        loobj.DYBL()
                    End If
                End If
                '更改价格
                If Not pVal.Before_Action And pVal.ItemUID = "ChangePri" Then
                    If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
                        MyApplication.MessageBox("只有在确定模式下才能更改价格！")
                    End If

                    If (MyApplication.MessageBox("是否确认修改价格？", 1, "确认", "取消")) <> 1 Then
                        BubbleEvent = False
                        Return
                    End If
                    '判断如果审批没通过，不能接着审批

                    Try
                        Dim lsDocentry As String = ioDbds_TI_Z0100.GetValue("DocEntry", 0)
                        Dim lsCardCode As String = ioDbds_TI_Z0100.GetValue("U_CardCode", 0)

                        Dim provider As ApprovalDataProvider = New ApprovalDataProvider(MyApplication.Company.GetDICompany, "TI_Z0100", Conversions.ToString(lsDocentry), lsCardCode, False)

                        'Dim lsItemCode, lsAppEntry As String
                        ''判断是否存在低于成本的销售价，如果低于，一定要先提交审批
                        'Dim lsSQL1 As String = "select T10.U_ItemCode from [@TI_Z0101]  T10 inner join [@TI_Z0100] t11 ON T10.DOCENTRY=T11.DOCENTRY
                        '              inner join OITW T12 on T10.U_ItemCode =T12.ItemCode and T10.U_WhsCode =t12.WhsCode 
                        '               where Round(T10.U_Price,4) <Round(T12.AvgPrice ,4)  and T10.docentry= '" + lsDocentry.Trim + "'"
                        'ioTempSql.ExecuteQuery(lsSQL1)
                        'If ioTempSql.Rows.Count > 0 Then
                        '    lsItemCode = ioTempSql.GetValue("U_ItemCode", 0)
                        '    If Not String.IsNullOrEmpty(lsItemCode) Then
                        '        '查看负毛利审批单是否有，如果没有提示

                        '        Dim lsSQL3 As String = "select AppEntry from MDM0076_Approve T120 where t120.BaseType='OMS0001' and t120.APPCode='SP0004'  and t120.Canceled='N' and t120.Basekey ='" + lsDocentry.Trim + "'"
                        '        ioTempSql.ExecuteQuery(lsSQL3)
                        '        If ioTempSql.Rows.Count > 0 And ioTempSql.GetValue("AppEntry", 0) = "0" Then
                        '            MyApplication.SetStatusBarMessage("负毛利审批单不存在，请先进行审批,物料 " + lsItemCode.Trim + "!", BoMessageTime.bmt_Medium, True)
                        '            Return
                        '        End If
                        '        If ioTempSql.Rows.Count <= 0 Then
                        '            MyApplication.SetStatusBarMessage("负毛利审批单不存在，请先进行审批,物料 " + lsItemCode.Trim + "!", BoMessageTime.bmt_Medium, True)
                        '            Return
                        '        End If


                        '        Dim lsSQL2 As String = "select AppEntry from MDM0076_Approve T120 where t120.BaseType='OMS0001' and t120.APPCode='SP0004'  and t120.AppStatus in('O','N')  and t120.Canceled='N' and t120.Basekey ='" + lsDocentry.Trim + "'"
                        '        ioTempSql.ExecuteQuery(lsSQL2)
                        '        If ioTempSql.Rows.Count > 0 Then
                        '            lsAppEntry = ioTempSql.GetValue("AppEntry", 0)
                        '            If Not String.IsNullOrEmpty(lsAppEntry) And lsAppEntry <> "0" Then
                        '                MyApplication.SetStatusBarMessage("审批没有通过，请先确认审批通过,审批单号" + lsAppEntry.Trim + "！", BoMessageTime.bmt_Medium, True)
                        '                Return
                        '            End If
                        '        End If
                        '    End If
                        'End If
                        'Dim lsSQL, lsCardCode As String
                        'lsCardCode = ioDbds_TI_Z0100.GetValue("U_CardCode", 0)
                        Dim lsSQL As String
                        lsSQL = "exec YL_ChangeODLNPrice '" + lsDocentry + "','" + lsCardCode.Trim() + "'"
                        ioTempSql.ExecuteQuery(lsSQL)
                        MyApplication.SetStatusBarMessage("交货单价格已更新成功！", BoMessageTime.bmt_Medium, False)
                        '将单据设置为结束
                        lsSQL = "update T10 set T10.Status='C' from [@TI_Z0100] T10 where docentry='" + lsDocentry + "'"
                        ioTempSql.ExecuteQuery(lsSQL)

                        MyForm.Mode = BoFormMode.fm_VIEW_MODE
                        'ioMtx_10.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
                        'ioBtn_Copy.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
                        'ioBtn_ChangePri.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
                        'ioBtn_Approve.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)

                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.Message)
                    End Try

                End If
                If Not pVal.Before_Action And pVal.ItemUID = "Approve" Then
                    Dim liDocEntry As Integer
                    Integer.TryParse(ioDbds_TI_Z0100.GetValue("DocEntry", 0), liDocEntry)
                    'liDocEntry = MyForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0)
                    If liDocEntry > 0 Then
                        myapprove(liDocEntry)
                    End If
                End If
            Case BoEventTypes.et_VALIDATE
                If Not pVal.Before_Action And pVal.ColUID = "PriceAfVAT" Then

                    ioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
                    Dim ldPriceAfVat, ldPrice, ldquantity, ldLineTotal, ldLineTotalAfVat, ldVatSum, ldVatPrcnt As Double
                    Decimal.TryParse(ioMtx_10.Columns.Item("PriceAfVAT").Cells.Item(pVal.Row).Specific.value, ldPriceAfVat)
                    Decimal.TryParse(ioMtx_10.Columns.Item("Quantity").Cells.Item(pVal.Row).Specific.value, ldquantity)
                    Decimal.TryParse(ioMtx_10.Columns.Item("VatPrcnt").Cells.Item(pVal.Row).Specific.value, ldVatPrcnt)

                    ldPrice = Math.Round(ldPriceAfVat / (1 + ldVatPrcnt * 0.01), 6)
                    ldLineTotal = Math.Round(ldPrice * ldquantity, 2)
                    ldVatSum = Math.Round(ldPriceAfVat * ldquantity - ldPrice * ldquantity, 2)
                    ldLineTotalAfVat = ldLineTotal + ldVatSum

                    ioDbds_TI_Z0101.Offset = pVal.Row - 1
                    ioMtx_10.GetLineData(pVal.Row)
                    ioDbds_TI_Z0101.SetValue("U_Price", ioDbds_TI_Z0101.Offset, ldPrice.ToString) '不含税总计
                    ioDbds_TI_Z0101.SetValue("U_LineTotalAfVat", ioDbds_TI_Z0101.Offset, ldLineTotalAfVat.ToString) '税额总计
                    ioDbds_TI_Z0101.SetValue("U_LineTotal", ioDbds_TI_Z0101.Offset, ldLineTotal.ToString) '含税总计
                    ioDbds_TI_Z0101.SetValue("U_VatSum", ioDbds_TI_Z0101.Offset, ldVatSum.ToString) '含税总计
                    ioMtx_10.SetLineData(pVal.Row)
                    '重新计算单据总计
                    Dim ldDocTotal, ldDocVatSum, ldDocTotalAfVat As Double
                    For i As Integer = 0 To ioDbds_TI_Z0101.Size - 1
                        Decimal.TryParse(ioDbds_TI_Z0101.GetValue("U_LineTotal", i), ldLineTotal)
                        Decimal.TryParse(ioDbds_TI_Z0101.GetValue("U_VatSum", i), ldVatSum)
                        Decimal.TryParse(ioDbds_TI_Z0101.GetValue("U_LineTotalAfVat", i), ldLineTotalAfVat)
                        ldDocTotal = ldDocTotal + ldLineTotal
                        ldDocVatSum = ldDocVatSum + ldVatSum
                        ldDocTotalAfVat = ldDocTotalAfVat + ldLineTotalAfVat
                    Next
                    ioDbds_TI_Z0100.SetValue("U_DocTotal", 0, ldDocTotal.ToString)
                    ioDbds_TI_Z0100.SetValue("U_VatSum", 0, ldDocVatSum.ToString)
                    ioDbds_TI_Z0100.SetValue("U_DocTotalAfVAT", 0, ldDocTotalAfVat.ToString)
                End If

        End Select
    End Sub

    ''' <summary>
    ''' 触发审批
    ''' </summary>
    ''' <param name="liDocEntry"></param>
    Private Sub approve(liBaseKey As Integer)
        If liBaseKey > 0 Then
            Dim lsCardCode As String = ioDbds_TI_Z0100.GetValue("U_CardCode", 0)
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
                    loWebAPIRequest.Content.Code = "SP0004"
                    loWebAPIRequest.Content.IsDesignated = "N"
                    loWebAPIRequest.Content.InputJson = ""
                    loWebAPIRequest.Content.BaseType = "OMS0001"
                    loWebAPIRequest.Content.BaseKey = liBaseKey
                    loWebAPIRequest.Content.UserCode = lsSalerCode
                    loWebAPIRequest.UserCode = lsSalerCode
                    'loWebAPIRequest.Content.UserCode = "P0013"
                    'loWebAPIRequest.UserCode = "P0013"
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
                                    lsSql = "Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select " + liAppEntry.ToString() + ",'OMS0001'," + liBaseKey.ToString() + ",GETDATE(),'N','O','SP0004'"
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
    End Sub
    Private Sub myapprove(liBaseKey As Integer)
        If liBaseKey > 0 Then
            Dim lsCardCode As String = ioDbds_TI_Z0100.GetValue("U_CardCode", 0)
            If Not String.IsNullOrEmpty(lsCardCode) Then
                lsCardCode = lsCardCode.Trim
            End If
            Try
                Dim provider As ApprovalDataProvider = New ApprovalDataProvider(MyApplication.Company.GetDICompany, "TI_Z0100", Conversions.ToString(liBaseKey), lsCardCode, True)
                Dim request As WebAPIRequest(Of MDM007606Request) = New WebAPIRequest(Of MDM007606Request)
                request.Content.Code = provider.ApprovalCode
                request.Content.IsDesignated = provider.IsDesignated
                request.Content.InputJson = ""
                request.Content.BaseType = provider.BaseType
                request.Content.BaseKey = provider.DocEntry
                request.Content.UserCode = provider.SalerCode
                request.UserCode = provider.SalerCode
                Dim param As String = JsonConvert.SerializeObject(request)
                Dim response As WebAPIResponse(Of MDM007606Response) = JsonConvert.DeserializeObject(Of WebAPIResponse(Of MDM007606Response))(BaseFunction.PostMoths(provider.PostAddress, param))
                If (response.Status <> 200) Then
                    MyBase.MyApplication.SetStatusBarMessage(("审批触发异常(远程),错误信息:" & response.Message), BoMessageTime.bmt_Medium, True)
                Else
                    Dim docEntry As Long = response.Content.DocEntry
                    If (docEntry > 0) Then
                        MyBase.MyApplication.StatusBar.SetText(("审批触发成功,审批单号:" & docEntry.ToString), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                        Try
                            Dim textArray1 As String() = New String() {"Insert into MDM0076_Approve(AppEntry,BaseType,Basekey,CreateDate,Canceled,AppStatus,APPCode) Select ", docEntry.ToString, ",'OMS0001',", liBaseKey.ToString, ",GETDATE(),'N','O','SP0001'"}
                            Dim query As String = String.Concat(textArray1)
                            Me.ioTempSql.ExecuteQuery(query)
                            'Dim textArray2 As String() = New String() {"UPDATE T0 SET U_APPNo='", docEntry.ToString, "' FROM ORDR T0 WHERE DocEntry='", liBaseKey.ToString, "'"}
                            'query = String.Concat(textArray2)
                            'Me.ioTempSql.ExecuteQuery(query)
                        Catch exception1 As Exception
                            Dim exception As Exception = exception1
                            MyBase.MyApplication.SetStatusBarMessage(("插入审批表异常，请联系IT部,错误信息:" & exception.Message.ToString), BoMessageTime.bmt_Medium, True)
                        End Try
                    End If
                End If
            Catch ex As Exception
                MyApplication.SetStatusBarMessage("审批触发异常(本地),错误信息:" + ex.Message.ToString())
            End Try

        End If
    End Sub

End Class
