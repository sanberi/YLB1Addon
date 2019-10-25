Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO
Imports TIModule

Public NotInheritable Class TI_Z0140
    Inherits FormBase
    Public ioMtx_10, ioMtx_20 As Matrix
    Private ioDbs_TI_Z0140, ioDbs_TI_Z0141 As DBDataSource

    Private ioDtDoc, ioDtTempSql As SAPbouiCOM.DataTable
    Private ioUds_DateF, ioUds_DateT, ioUds_PPKD, ioUds_ShowC As UserDataSource
    Private ibCheckLoad As Boolean = False
    Private ioListDoc As SortedList = New SortedList
    Private iiDocEntry As Integer
    Dim isBsEntry As Object

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function

    Private Sub TI_Z0140_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_ADD
                If BusinessObjectInfo.BeforeAction Then
                    '添加时，把查询号的数据插入到Dbds
                    InsertDbds(BubbleEvent)
                    If ioDbs_TI_Z0141.Size = 0 Then
                        MyApplication.MessageBox("必须存在快递打印行！")
                        BubbleEvent = False
                        Return
                    End If
                Else
                    Integer.TryParse(ioDbs_TI_Z0140.GetValue("DocEntry", 0), iiDocEntry)
                End If
            Case BoEventTypes.et_FORM_DATA_UPDATE
                If BusinessObjectInfo.BeforeAction Then
                    Check(BubbleEvent)
                    If ioDbs_TI_Z0141.Size = 0 Then
                        BubbleEvent = False
                        Return
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_LOAD
                If Not BusinessObjectInfo.BeforeAction Then
                    If MyForm.PaneLevel <> 2 Then
                        MyForm.PaneLevel = 2
                    End If
                    setItemE()
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 检查数据
    ''' </summary>
    ''' <param name="BubbleEvent"></param>
    ''' <remarks></remarks>
    Private Sub Check(ByRef BubbleEvent As Boolean)
        Dim lsU_Code As String
        ioMtx_20.FlushToDataSource()
        '行校验逻辑
        For i As Integer = 0 To ioDbs_TI_Z0141.Size - 1
            If i >= ioDbs_TI_Z0141.Size Then
                Exit For
            End If
            lsU_Code = ioDbs_TI_Z0141.GetValue("U_CardCode", i)
            If Not String.IsNullOrEmpty(lsU_Code) Then
                lsU_Code = lsU_Code.Trim
            End If
            If String.IsNullOrEmpty(lsU_Code) Then
                ioDbs_TI_Z0141.RemoveRecord(i)
                i = i - 1
                Continue For
            End If
        Next i
    End Sub

    Private Sub TI_Z0055_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "CX"
                            GetMtx_10Data(False)
                        Case "SP"
                            ibCheckLoad = False
                            SetSp()
                        Case "Create"
                            '生成快递单号
                            SetCreate()
                        Case "1"
                            If pVal.ActionSuccess Then
                                If MyForm.Mode <> BoFormMode.fm_FIND_MODE Then
                                    ReGetDbds()
                                    setItemE()
                                    MyForm.PaneLevel = 2
                                End If
                            End If
                    End Select
                End If

            Case BoEventTypes.et_CHOOSE_FROM_LIST
                If Not pVal.BeforeAction Then
                    Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
                    Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
                    If Not lodt Is Nothing Then
                        Select Case pVal.ItemUID
                            Case "CardF", "CardCF"
                                Dim lsCardCode, lsCardName As String
                                lsCardCode = lodt.GetValue("CardCode", 0)
                                lsCardName = lodt.GetValue("CardName", 0)
                                MyForm.DataSources.UserDataSources.Item("CardCF").ValueEx = lsCardCode
                                MyForm.DataSources.UserDataSources.Item("CardNF").ValueEx = lsCardName
                            Case "CardT", "CardCT"
                                Dim lsCardCode, lsCardName As String
                                lsCardCode = lodt.GetValue("CardCode", 0)
                                lsCardName = lodt.GetValue("CardName", 0)
                                MyForm.DataSources.UserDataSources.Item("CardCT").ValueEx = lsCardCode
                                MyForm.DataSources.UserDataSources.Item("CardNT").ValueEx = lsCardName
                        End Select
                    End If
                End If
            Case BoEventTypes.et_KEY_DOWN
                If pVal.BeforeAction Then
                    If pVal.CharPressed = 40 Then
                        Select Case pVal.ItemUID
                            Case "Mtx_10"
                                If pVal.Row <= 0 Then
                                    BubbleEvent = False
                                    Return
                                End If
                                If ioMtx_10.VisualRowCount >= pVal.Row + 1 Then
                                    ioMtx_10.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row + 1).Click(BoCellClickType.ct_Regular)
                                End If
                            Case "Mtx_20"
                                If pVal.Row <= 0 Then
                                    BubbleEvent = False
                                    Return
                                End If
                                If ioMtx_20.VisualRowCount >= pVal.Row + 1 Then
                                    ioMtx_20.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row + 1).Click(BoCellClickType.ct_Regular)
                                End If
                        End Select
                    ElseIf pVal.CharPressed = 38 Then
                        Select Case pVal.ItemUID
                            Case "Mtx_10"
                                If pVal.Row <= 0 Then
                                    BubbleEvent = False
                                    Return
                                End If
                                If pVal.Row >= 2 Then
                                    ioMtx_10.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row - 1).Click(BoCellClickType.ct_Regular)
                                End If
                            Case "Mtx_20"
                                If pVal.Row <= 0 Then
                                    BubbleEvent = False
                                    Return
                                End If
                                If pVal.Row >= 2 Then
                                    ioMtx_20.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row - 1).Click(BoCellClickType.ct_Regular)
                                End If
                        End Select
                    End If
                End If
                If Not pVal.Before_Action And pVal.CharPressed = 13 And pVal.ItemUID = "Express" Then
                    Try
                        '输入起始单号之后将最后的几位截取显示出来
                        Dim litruncatenumber As Integer
                        Dim lsSplitString As String
                        Dim lsQZ As String
                        Integer.TryParse(ioDbs_TI_Z0140.GetValue("U_DocWS", 0), litruncatenumber)   '需要截取多少位
                        Dim lsExpNumber As String = MyForm.Items.Item("Express").Specific.value   '起始快递单号
                        lsSplitString = lsExpNumber.Substring(lsExpNumber.Length - litruncatenumber, litruncatenumber)
                        lsQZ = lsExpNumber.Substring(0, lsExpNumber.Length - litruncatenumber)

                        ioDbs_TI_Z0140.SetValue("U_Express", 0, lsExpNumber)
                        ioDbs_TI_Z0140.SetValue("U_SatDoc", 0, lsSplitString)
                        ioDbs_TI_Z0140.SetValue("U_QZ", 0, lsQZ)

                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.Message)
                        Return
                    End Try

                End If
            Case BoEventTypes.et_CLICK
                If pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Mtx_20"
                            If pVal.Row <= 0 Then
                                BubbleEvent = False
                                Return
                            End If
                            ioMtx_20.SelectRow(pVal.Row, True, False)
                    End Select
                End If
            Case BoEventTypes.et_COMBO_SELECT
                If Not pVal.BeforeAction And pVal.ItemChanged Then
                    Select Case pVal.ItemUID
                        Case "KDGS"
                            Dim lsSql, lsCode As String
                            lsCode = ioDbs_TI_Z0140.GetValue("U_KDGS", 0)
                            If Not String.IsNullOrEmpty(lsCode) Then
                                lsCode = lsCode.Trim
                            End If
                            lsSql = "Select t10.Name,t10.U_QZ,t10.U_DocWS  From [@TI_Z0150] t10 where t10.Code ='" + lsCode + "'"
                            ioDtTempSql.ExecuteQuery(lsSql)
                            Dim lsQZ, lsKDName As String
                            Dim liDocWS As Integer
                            lsQZ = ioDtTempSql.GetValue("U_QZ", 0)
                            lsKDName = ioDtTempSql.GetValue("Name", 0)
                            Integer.TryParse(ioDtTempSql.GetValue("U_DocWS", 0), liDocWS)

                            ioDbs_TI_Z0140.SetValue("U_QZ", 0, lsQZ)
                            ioDbs_TI_Z0140.SetValue("U_DocWS", 0, Convert.ToString(liDocWS))

                            '选择之后将光标定位到起始单号
                            MyForm.Items.Item("QZ").Click(BoCellClickType.ct_Regular)
                    End Select
                End If
            Case BoEventTypes.et_VALIDATE
                If Not pVal.BeforeAction And pVal.ItemChanged Then
                    Select Case pVal.ItemUID
                        Case "Mtx_10"
                            Select Case pVal.ColUID
                                Case "KDDoc"

                            End Select
                    End Select
                End If


        End Select
    End Sub


    ''' <summary>
    ''' 获取查询数据
    ''' </summary>
    ''' <param name="lbRef"></param>
    ''' <remarks></remarks>
    Public Sub GetMtx_10Data(ByVal lbRef As Boolean)
        If MyForm.Mode <> BoFormMode.fm_ADD_MODE Then
            MyApplication.SetStatusBarMessage("只能在添加状态下才能生成查询！")
            Return
        End If

        Dim lsKDGS As String
        lsKDGS = ioDbs_TI_Z0140.GetValue("U_KDGS", 0)
        If String.IsNullOrEmpty(lsKDGS) Then
            MyApplication.SetStatusBarMessage("快递公司不能为空！")
            Return
        End If

        Dim lsSql As String
        Dim lsCardNF, lsCardNT As String
        Dim lsCardCF, lsCardCT As String
        Dim lsDateF, lsDateT As String
        Dim liPoF, liPoT As Integer
        Dim lsSQF, lsSQT As String
        lsCardCF = MyForm.DataSources.UserDataSources.Item("CardCF").ValueEx
        If Not String.IsNullOrEmpty(lsCardCF) Then
            lsCardCF = lsCardCF.Trim
        End If
        lsCardCT = MyForm.DataSources.UserDataSources.Item("CardCT").ValueEx
        If Not String.IsNullOrEmpty(lsCardCT) Then
            lsCardCT = lsCardCT.Trim
        End If
        lsCardNF = MyForm.DataSources.UserDataSources.Item("CardNF").ValueEx
        If Not String.IsNullOrEmpty(lsCardNF) Then
            lsCardNF = lsCardNF.Trim
        End If
        lsCardNT = MyForm.DataSources.UserDataSources.Item("CardNT").ValueEx
        If Not String.IsNullOrEmpty(lsCardNT) Then
            lsCardNT = lsCardNT.Trim
        End If
        If String.IsNullOrEmpty(lsCardNF) Then
            lsCardCF = "null"
        Else
            lsCardCF = "'" + lsCardCF + "'"
        End If
        If String.IsNullOrEmpty(lsCardNT) Then
            lsCardCT = "null"
        Else
            lsCardCT = "'" + lsCardCT + "'"
        End If
        lsDateF = MyForm.DataSources.UserDataSources.Item("DateF").ValueEx
        If Not String.IsNullOrEmpty(lsDateF) Then
            lsDateF = lsDateF.Trim
        End If
        If String.IsNullOrEmpty(lsDateF) Then
            MyApplication.SetStatusBarMessage("开始日期不能为空！")
            Return
        Else
            lsDateF = "'" + lsDateF + "'"
        End If
        lsDateT = MyForm.DataSources.UserDataSources.Item("DateT").ValueEx
        If Not String.IsNullOrEmpty(lsDateT) Then
            lsDateT = lsDateT.Trim
        End If
        If String.IsNullOrEmpty(lsDateT) Then
            MyApplication.SetStatusBarMessage("结束日期不能为空！")
            Return
        Else
            lsDateT = "'" + lsDateT + "'"
        End If
        Integer.TryParse(MyForm.DataSources.UserDataSources.Item("PoF").ValueEx, liPoF)
        Integer.TryParse(MyForm.DataSources.UserDataSources.Item("PoT").ValueEx, liPoT)

        lsSQF = MyForm.DataSources.UserDataSources.Item("SQF").ValueEx
        If Not String.IsNullOrEmpty(lsSQF) Then
            lsSQF = lsSQF.Trim
        End If
        If String.IsNullOrEmpty(lsSQF) Then
            lsSQF = "''"
        End If
        lsSQT = MyForm.DataSources.UserDataSources.Item("SQT").ValueEx
        If Not String.IsNullOrEmpty(lsSQT) Then
            lsSQT = lsSQT.Trim
        End If
        If String.IsNullOrEmpty(lsSQT) Then
            lsSQT = "''"
        End If

        Dim lsKfName As String
        lsKfName = ioDbs_TI_Z0140.GetValue("U_KfName", 0)
        If Not String.IsNullOrEmpty(lsKfName) Then
            lsKfName = lsKfName.Trim
        End If

        Dim lsShowC, lsPPKD As String
        lsPPKD = ioUds_PPKD.Value
        If Not String.IsNullOrEmpty(lsPPKD) Then
            lsPPKD = lsPPKD.Trim
        End If
        If String.IsNullOrEmpty(lsPPKD) Then
            lsPPKD = "N"
        End If
        lsShowC = ioUds_ShowC.Value
        If Not String.IsNullOrEmpty(lsShowC) Then
            lsShowC = lsShowC.Trim
        End If
        If String.IsNullOrEmpty(lsShowC) Then
            lsShowC = "N"
        End If

        lsSql = "if object_id('tempdb..#TempWhere') is not null" + vbNewLine +
                "Begin drop table #TempWhere End" + vbNewLine +
                "Select " + lsCardCF + " CardNF," + lsCardCT + " CardNT," + lsDateF + " DateF," + lsDateT + "" + vbNewLine +
                " DateT," + Convert.ToString(liPoT) + " PoF, " + Convert.ToString(liPoT) + " PoT," + vbNewLine +
                lsSQF + " SQF, " + lsSQT + " SQT,'" + lsKfName + "' KfName,'" + lsShowC + "' ShowC,'" + lsPPKD + "' PPKD,'" + lsKDGS + "' KDName into #TempWhere" + vbNewLine +
                "Exec TI_GetKDDY"
        ioDtTempSql.ExecuteQuery(lsSql)
        ioDtDoc.Rows.Clear()
        'If Not ioDtTempSql.IsEmpty Then
        '    Dim lsXmlString As String
        '    lsXmlString = ioDtTempSql.SerializeAsXML(BoDataTableXmlSelect.dxs_DataOnly)
        '    ioDtDoc.LoadSerializedXML(BoDataTableXmlSelect.dxs_DataOnly, lsXmlString)
        'End If
        ioMtx_10.LoadFromDataSource()
    End Sub

    Private Sub setItemE()
        Dim loItem33, loItem34 As Item
        loItem33 = MyForm.Items.Item("SP")
        loItem34 = MyForm.Items.Item("Create")
        Dim lsPrtDate As String
        lsPrtDate = ioDbs_TI_Z0140.GetValue("U_PrtDate", 0)
        If Not String.IsNullOrEmpty(lsPrtDate) Then
            lsPrtDate = lsPrtDate.Trim
        End If
        If MyForm.Mode = BoFormMode.fm_ADD_MODE Then
            loItem33.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
            loItem34.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
        Else
            If String.IsNullOrEmpty(lsPrtDate) Then
                loItem33.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_True)
                loItem34.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_True)
            Else
                loItem33.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
                loItem34.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, BoAutoFormMode.afm_All, BoModeVisualBehavior.mvb_False)
            End If
        End If
    End Sub

    ''' <summary>
    ''' 提交数据
    ''' </summary>
    ''' <param name="BubbleEvent"></param>
    ''' <remarks></remarks>
    Public Sub InsertDbds(ByRef BubbleEvent As Boolean)
        Try
            Dim lsU_CardCode, lsU_CardName, lsU_BsEntry, lsU_NAtCard, lsU_DftKDGS, lsU_KDDoc, lsU_JHDoc As String
            Dim lsU_Memo, lsU_InvSDate, lsU_InvSMode, lsU_InvPDate, lsU_Regon, lsU_Taker, lsU_Iaddress, lsDeliverData As String
            Dim lsSwitchNo, lsPickRmrk, lsShKf, lsZipCode As String
            Dim liU_Scount As Integer
            Dim lsSelect As String
            Dim ldInvSDate, ldInvPDate As Date
            Dim ldDocTotal As Decimal
            ioDbs_TI_Z0141.Clear()
            ioMtx_10.FlushToDataSource()
            Dim liLineId As Integer = 1
            For i As Integer = 0 To ioDtDoc.Rows.Count - 1
                lsSelect = ioDtDoc.GetValue("U_Select", i)
                If Not String.IsNullOrEmpty(lsSelect) Then
                    lsSelect = lsSelect.Trim
                End If
                If lsSelect = "Y" Then
                    ioDbs_TI_Z0141.InsertRecord(ioDbs_TI_Z0141.Size)
                    ioDbs_TI_Z0141.Offset = ioDbs_TI_Z0141.Size - 1
                    'U_CardCode
                    lsU_CardCode = ioDtDoc.GetValue("U_CardCode", i)
                    ioDbs_TI_Z0141.SetValue("U_CardCode", ioDbs_TI_Z0141.Offset, lsU_CardCode)
                    'U_CardName
                    lsU_CardName = ioDtDoc.GetValue("U_CardName", i)
                    ioDbs_TI_Z0141.SetValue("U_CardName", ioDbs_TI_Z0141.Offset, lsU_CardName)
                    'U_BsEntry
                    lsU_BsEntry = ioDtDoc.GetValue("U_BsEntry", i)
                    ioDbs_TI_Z0141.SetValue("U_BsEntry", ioDbs_TI_Z0141.Offset, lsU_BsEntry)
                    'U_JHDoc
                    lsU_JHDoc = ioDtDoc.GetValue("U_JHDoc", i)
                    ioDbs_TI_Z0141.SetValue("U_JHDoc", ioDbs_TI_Z0141.Offset, lsU_JHDoc)
                    'U_NAtCard
                    lsU_NAtCard = ioDtDoc.GetValue("U_NAtCard", i)
                    ioDbs_TI_Z0141.SetValue("U_NAtCard", ioDbs_TI_Z0141.Offset, lsU_NAtCard)
                    'U_DftKDGS
                    lsU_DftKDGS = ioDtDoc.GetValue("U_DftKDGS", i)
                    ioDbs_TI_Z0141.SetValue("U_DftKDGS", ioDbs_TI_Z0141.Offset, lsU_DftKDGS)
                    'U_KDDoc
                    lsU_KDDoc = ioDtDoc.GetValue("U_KDDoc", i)
                    ioDbs_TI_Z0141.SetValue("U_KDDoc", ioDbs_TI_Z0141.Offset, lsU_KDDoc)

                    lsDeliverData = ioDtDoc.GetValue("U_DeliverData", i)
                    ioDbs_TI_Z0141.SetValue("U_DeliverData", ioDbs_TI_Z0141.Offset, lsDeliverData)
                    'U_Memo
                    lsU_Memo = ioDtDoc.GetValue("U_Memo", i)
                    ioDbs_TI_Z0141.SetValue("U_Memo", ioDbs_TI_Z0141.Offset, lsU_Memo)
                    'U_DocTtl
                    Decimal.TryParse(ioDtDoc.GetValue("U_DocTtl", i), ldDocTotal)
                    ioDbs_TI_Z0141.SetValue("U_DocTtl", ioDbs_TI_Z0141.Offset, Convert.ToString(ldDocTotal))
                    'U_InvSDate
                    lsU_InvSDate = ioDtDoc.GetValue("U_InvSDate", i)
                    If Not String.IsNullOrEmpty(lsU_InvSDate) Then
                        ldInvSDate = ioDtDoc.GetValue("U_InvSDate", i)
                        lsU_InvSDate = ldInvSDate.ToString("yyyyMMdd")
                        ioDbs_TI_Z0141.SetValue("U_InvSDate", ioDbs_TI_Z0141.Offset, lsU_InvSDate)
                    End If
                    'U_InvSMode
                    lsU_InvSMode = ioDtDoc.GetValue("U_InvSMode", i)
                    ioDbs_TI_Z0141.SetValue("U_InvSMode", ioDbs_TI_Z0141.Offset, lsU_InvSMode)
                    'U_InvPDate
                    lsU_InvPDate = ioDtDoc.GetValue("U_InvPDate", i)
                    If Not String.IsNullOrEmpty(lsU_InvPDate) Then
                        ldInvPDate = ioDtDoc.GetValue("U_InvPDate", i)
                        lsU_InvPDate = ldInvPDate.ToString("yyyyMMdd")
                        ioDbs_TI_Z0141.SetValue("U_InvPDate", ioDbs_TI_Z0141.Offset, lsU_InvPDate)
                    End If
                    'U_Regon
                    lsU_Regon = ioDtDoc.GetValue("U_Regon", i)
                    ioDbs_TI_Z0141.SetValue("U_Regon", ioDbs_TI_Z0141.Offset, lsU_Regon)
                    'U_Scount
                    Integer.TryParse(ioDtDoc.GetValue("U_Scount", i), liU_Scount)
                    ioDbs_TI_Z0141.SetValue("U_Scount", ioDbs_TI_Z0141.Offset, Convert.ToString(liU_Scount))
                    'U_Taker
                    lsU_Taker = ioDtDoc.GetValue("U_Taker", i)
                    ioDbs_TI_Z0141.SetValue("U_Taker", ioDbs_TI_Z0141.Offset, lsU_Taker)
                    'U_Iaddress
                    lsU_Iaddress = ioDtDoc.GetValue("U_Iaddress", i)
                    ioDbs_TI_Z0141.SetValue("U_Iaddress", ioDbs_TI_Z0141.Offset, lsU_Iaddress)
                    'U_Scount
                    lsSwitchNo = ioDtDoc.GetValue("U_SwitchNo", i)
                    ioDbs_TI_Z0141.SetValue("U_SwitchNo", ioDbs_TI_Z0141.Offset, lsSwitchNo)
                    'U_Taker
                    lsPickRmrk = ioDtDoc.GetValue("U_PickRmrk", i)
                    ioDbs_TI_Z0141.SetValue("U_PickRmrk", ioDbs_TI_Z0141.Offset, lsPickRmrk)
                    'U_Iaddress
                    lsShKf = ioDtDoc.GetValue("U_ShKf", i)
                    ioDbs_TI_Z0141.SetValue("U_ShKf", ioDbs_TI_Z0141.Offset, lsShKf)
                    'U_Regon
                    lsZipCode = ioDtDoc.GetValue("U_ZipCode", i)
                    ioDbs_TI_Z0141.SetValue("U_ZipCode", ioDbs_TI_Z0141.Offset, lsZipCode)

                    ioDbs_TI_Z0141.SetValue("LineId", ioDbs_TI_Z0141.Offset, Convert.ToString(liLineId))
                    liLineId = liLineId + 1
                End If
            Next i

            ioMtx_20.LoadFromDataSource()
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
            BubbleEvent = False
        End Try
    End Sub


    ''' <summary>
    ''' 生成快递单号
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetCreate()
        '检查快递公司，前缀，起始单号，快递单位数
        If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
            MyApplication.SetStatusBarMessage("只能在确认状态下才能生成快递单号！")
            Return
        End If
        Dim lsKDGS, lsQZ As String
        Dim liSatDoc, liDocWS As Integer
        lsKDGS = ioDbs_TI_Z0140.GetValue("U_KDGS", 0)
        lsQZ = ioDbs_TI_Z0140.GetValue("U_QZ", 0)
        Integer.TryParse(ioDbs_TI_Z0140.GetValue("U_SatDoc", 0), liSatDoc)
        Integer.TryParse(ioDbs_TI_Z0140.GetValue("U_DocWS", 0), liDocWS)
        If Not String.IsNullOrEmpty(lsKDGS) Then
            lsKDGS = lsKDGS.Trim
        End If
        If Not String.IsNullOrEmpty(lsQZ) Then
            lsQZ = lsQZ.Trim
        End If
        If String.IsNullOrEmpty(lsKDGS) Then
            MyApplication.SetStatusBarMessage("快递公司不能为空！")
            Return
        End If
        If String.IsNullOrEmpty(lsQZ) Then
            MyApplication.SetStatusBarMessage("前缀不能为空！")
            Return
        End If
        If liSatDoc <= 0 Then
            MyApplication.SetStatusBarMessage("起始快递号不能为空！")
            Return
        End If
        If liDocWS <= 0 Then
            MyApplication.SetStatusBarMessage("单号位数不能为空！")
            Return
        End If
        Dim liDocEntry As Integer
        Integer.TryParse(ioDbs_TI_Z0140.GetValue("DocEntry", 0), liDocEntry)
        If liDocEntry > 0 Then
            If MyApplication.MessageBox("确认对选择行生成快递单号吗?", 1, "是", "否") = 1 Then
                Try
                    'ioMtx_10.FlushToDataSource()
                    Dim lsSQL As String
                    MyApplication.StatusBar.SetText("正在生成快递单号，请稍后....", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
                    lsSQL = "Declare @error  int,@error_message nvarchar (200) " + vbNewLine +
                            "Exec [TI_CreateKDDoc] " + Convert.ToString(liDocEntry) + ",@error output,@error_message output" + vbNewLine +
                            "Select @error,@error_message"
                    ioDtTempSql.ExecuteQuery(lsSQL)
                    Dim liError As Integer
                    Dim lsEmage As String
                    Integer.TryParse(ioDtTempSql.GetValue(0, 0), liError)
                    lsEmage = ioDtTempSql.GetValue(1, 0)
                    If liError <> 0 Then
                        MyApplication.StatusBar.SetText(lsEmage, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Else
                        '重新加在
                        Integer.TryParse(ioDbs_TI_Z0140.GetValue("DocEntry", 0), iiDocEntry)
                        ReGetDbds()
                        MyApplication.StatusBar.SetText("快递单号生成成功！", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                    End If
                Catch ex As Exception
                    MyApplication.SetStatusBarMessage(ex.ToString())
                Finally
                End Try
            End If
        End If
    End Sub

    ''' <summary>
    ''' 刷新数据
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReGetDbds()
        MyForm.Freeze(True)
        Try
            If iiDocEntry > 0 Then
                '取出最好一张单据数据
                MyForm.Mode = BoFormMode.fm_OK_MODE
                Dim loConditions As SAPbouiCOM.Conditions
                Dim loCondition As SAPbouiCOM.Condition
                loConditions = MyApplication.CreateObject(BoCreatableObjectType.cot_Conditions)
                loCondition = loConditions.Add

                loCondition.Alias = "DocEntry"
                loCondition.Operation = BoConditionOperation.co_EQUAL
                loCondition.CondVal = Convert.ToString(iiDocEntry)
                ioDbs_TI_Z0140.Query(loConditions)
                ioDbs_TI_Z0141.Query(loConditions)


                ioMtx_10.LoadFromDataSource()
                ioMtx_20.LoadFromDataSource()
            End If
        Catch ex As Exception
        Finally
            MyForm.Freeze(False)
        End Try
    End Sub

    ''' <summary>
    ''' 批量打印
    ''' 2014-06-08 修改使用Excel打印
    ''' Excel 设置打印机 的格式为 查看
    '''本示例将网络 HP LaserJet IIISi 打印机设置为活动打印机。
    '''Application.ActivePrinter = "HP LaserJet IIISi on \\printers\laser"
    '''本示例将 LPT1 端口的本地 HP LaserJet 4 打印机设置为活动打印机。
    ''' Application.ActivePrinter = "HP LaserJet 4 local on LPT1:"
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetSp()
        '必须确认状态下才能打印
        If MyForm.Mode <> BoFormMode.fm_OK_MODE Then
            MyApplication.SetStatusBarMessage("只能在确认状态下才能打印！")
            Return
        End If
        '询问 打印过了是否继续打印
        Dim lsPrtDate As String
        lsPrtDate = ioDbs_TI_Z0140.GetValue("U_PrtDate", 0)
        If Not String.IsNullOrEmpty(lsPrtDate) Then
            lsPrtDate = lsPrtDate.Trim
        End If
        If Not String.IsNullOrEmpty(lsPrtDate) Then
            If MyApplication.MessageBox("该单据已经打印，是否再次打印?", 1, "是", "否") <> 1 Then
                Return
            End If
        End If
        '先检查打印机环境是否正确
        '1.Excecl 文件路径是否正确
        Dim lsKDGS As String
        lsKDGS = ioDbs_TI_Z0140.GetValue("U_KDGS", 0)
        If Not String.IsNullOrEmpty(lsKDGS) Then
            lsKDGS = lsKDGS.Trim
        End If
        Integer.TryParse(ioDbs_TI_Z0140.GetValue("DocEntry", 0), iiDocEntry)

        Dim lsExcelSercerPath, lsExcelMarchPath, lsSelectSql As String
        lsSelectSql = "Select t10.U_ExtPath,t10.U_LadPath From [@TI_Z0060] t10 where t10.Code='TI_001'"
        ioDtTempSql.ExecuteQuery(lsSelectSql)
        If Not ioDtTempSql.IsEmpty Then
            lsExcelSercerPath = ioDtTempSql.GetValue("U_ExtPath", 0)
            lsExcelMarchPath = ioDtTempSql.GetValue("U_LadPath", 0)
        Else
            lsExcelSercerPath = ""
            lsExcelMarchPath = ""
        End If
        If Not String.IsNullOrEmpty(lsExcelSercerPath) Then
            lsExcelSercerPath = lsExcelSercerPath.Trim
        End If
        If Not String.IsNullOrEmpty(lsExcelMarchPath) Then
            lsExcelMarchPath = lsExcelMarchPath.Trim
        End If
        If String.IsNullOrEmpty(lsExcelSercerPath) Then
            MyApplication.SetStatusBarMessage("Excel模块服务器地址为空！")
            Return
        End If
        If String.IsNullOrEmpty(lsExcelMarchPath) Then
            MyApplication.SetStatusBarMessage("Excel模块本地地址为空！")
            Return
        End If
        '2 Excel文件是否正确
        If (Not Directory.Exists(lsExcelSercerPath)) Then
            MyApplication.SetStatusBarMessage("Excel模块服务器地址不正确！")
            Return
        End If

        Dim lsCurrPcName, lsU_Fpath, lsU_Esub, lsU_Esubp, lsU_DPrinter, lsU_Printer, lsU_PsizeID As String
        lsCurrPcName = System.Environment.MachineName
        lsSelectSql = "Select t10.U_Fpath,t10.U_Esub,t10.U_Esubp,t10.U_Printer U_DPrinter,t11.U_Printer,t11.U_PsizeID From [@TI_Z0150] t10" + vbNewLine +
                      "left join [@TI_Z0151] t11 on t10.Code=t11.Code and t11.U_PcName='" + lsCurrPcName + "' where t10.Code ='" + lsKDGS + "'"
        ioDtTempSql.ExecuteQuery(lsSelectSql)
        If Not ioDtTempSql.IsEmpty Then
            lsU_Fpath = ioDtTempSql.GetValue("U_Fpath", 0)
            lsU_Esub = ioDtTempSql.GetValue("U_Esub", 0)
            lsU_Esubp = ioDtTempSql.GetValue("U_Esubp", 0)
            lsU_DPrinter = ioDtTempSql.GetValue("U_DPrinter", 0)
            lsU_Printer = ioDtTempSql.GetValue("U_Printer", 0)
            lsU_PsizeID = ioDtTempSql.GetValue("U_PsizeID", 0)
        Else
            MyApplication.SetStatusBarMessage("没有找到相应的快递公司！")
            Return
        End If
        If Not String.IsNullOrEmpty(lsU_Fpath) Then
            lsU_Fpath = lsU_Fpath.Trim
        End If
        If String.IsNullOrEmpty(lsU_Fpath) Then
            MyApplication.SetStatusBarMessage("快递公司文件不能为空！")
            Return
        End If
        If Not String.IsNullOrEmpty(lsU_Esub) Then
            lsU_Esub = lsU_Esub.Trim
        End If
        If String.IsNullOrEmpty(lsU_Esub) Then
            MyApplication.SetStatusBarMessage("快递公司Excel宏方法不能为空！")
            Return
        End If
        If Not String.IsNullOrEmpty(lsU_Esubp) Then
            lsU_Esubp = lsU_Esubp.Trim
        End If
        If String.IsNullOrEmpty(lsU_Esubp) Then
            MyApplication.SetStatusBarMessage("快递公司Excel宏方法参数不能为空！")
            Return
        End If
        If Not String.IsNullOrEmpty(lsU_DPrinter) Then
            lsU_DPrinter = lsU_DPrinter.Trim
        End If
        If Not String.IsNullOrEmpty(lsU_Printer) Then
            lsU_Printer = lsU_Printer.Trim
        End If
        If Not String.IsNullOrEmpty(lsU_PsizeID) Then
            lsU_PsizeID = lsU_PsizeID.Trim
        End If
        If String.IsNullOrEmpty(lsU_Printer) Then
            lsU_Printer = lsU_DPrinter
            lsU_PsizeID = ""
        Else
            If String.IsNullOrEmpty(lsU_PsizeID) Then
                MyApplication.SetStatusBarMessage("快递公司对应的纸张不能为空！")
                Return
            End If
        End If
        '检查文件是否正确
        Dim lsFileWJ, lsToFile As String
        lsFileWJ = lsExcelSercerPath + "\" + lsU_Fpath
        If (Not File.Exists(lsFileWJ)) Then
            MyApplication.SetStatusBarMessage("服务器端EXCEL文件没有找到！")
            Return
        End If
        '3 是否可以正常下载到临时文件夹,如果没有就创建
        If (Not Directory.Exists(lsExcelMarchPath)) Then
            Directory.CreateDirectory(lsExcelMarchPath)
        End If
        lsToFile = lsExcelMarchPath + "\" + lsU_Fpath

        '由一个存储过程检查
        lsSelectSql = "Declare @error  int,@error_message nvarchar (200) " + vbNewLine +
                             "Exec [TI_CheckPrintKDDOc] " + Convert.ToString(iiDocEntry) + ",@error output,@error_message output" + vbNewLine +
                             "Select @error,@error_message"
        ioDtTempSql.ExecuteQuery(lsSelectSql)
        Dim liError As Integer
        Dim lsEmage As String
        Integer.TryParse(ioDtTempSql.GetValue(0, 0), liError)
        lsEmage = ioDtTempSql.GetValue(1, 0)
        If liError <> 0 Then
            MyApplication.StatusBar.SetText(lsEmage, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return
        End If
        Dim liRowCount As Integer = ioDbs_TI_Z0141.Size
        '先把数据插入到数据库
        If liRowCount > 0 Then
            If MyApplication.MessageBox("确认对选择行进行批量打印吗?", 1, "是", "否") = 1 Then
                MyApplication.StatusBar.SetText("打印准备!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
                '复制文件
                File.Copy(lsFileWJ, lsToFile, True)
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

                oExcelApp.Visible = True
                oExcelApp.DisplayAlerts = False
                m_objBooks = oExcelApp.Workbooks
                m_objBook = m_objBooks.Open(lsToFile)

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
                Try
                    '初始化打印机
                    Dim liLineId As Integer
                    oExcelApp.Run("Sheet1.FindPrinter", lsU_Printer, lsU_PsizeID, lsFlag)
                    For i As Integer = 0 To ioDbs_TI_Z0141.Size - 1
                        Integer.TryParse(ioDbs_TI_Z0141.GetValue("LineId", i), liLineId)
                        If liLineId > 0 Then
                            oExcelApp.ScreenUpdating = False
                            oExcelApp.Run(lsU_Esub, Convert.ToString(iiDocEntry), Convert.ToString(liLineId))
                            oExcelApp.ScreenUpdating = True
                            m_objSheet.PrintOutEx()
                        End If

                        MyApplication.StatusBar.SetText("正在打印快递单！", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                    Next i
                    Dim lsSql As String
                    lsSql = "Update t10 set t10.U_PrtDate=(CONVERT(varchar(100), GETDATE(), 121))" + vbNewLine + _
                            "from [@TI_Z0140] t10 where t10.DocEntry ='" + Convert.ToString(iiDocEntry) + "'" + vbNewLine + _
                            "Update t10 set t10.U_PrtDate=(CONVERT(varchar(100), GETDATE(), 121))" + vbNewLine + _
                            "from [@TI_Z0141] t10 where t10.DocEntry ='" + Convert.ToString(iiDocEntry) + "'" + vbNewLine + _
                            "Exec SBO_SP_TransactionNotification 'TI_Z0140','U',1,'DocEntry','" + Convert.ToString(iiDocEntry) + "'"
                    ioDtTempSql.ExecuteQuery(lsSql)
                    ReGetDbds()
                    MyApplication.StatusBar.SetText("批量打印完成，总计:" + Convert.ToString(liRowCount), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                    setItemE()
                Catch ex As Exception
                    MyApplication.SetStatusBarMessage(ex.ToString())
                Finally
                    '关闭Excel进程
                    m_objBook.Close()
                    Dim deadProcess As Process = Process.GetProcessById(processid)  '获取该进程
                    oExcelApp.Quit()
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp)
                    oExcelApp = Nothing
                    GC.Collect()
                    deadProcess.Kill()  '杀死进程

                    '删除文件
                    If (File.Exists(lsToFile)) Then
                        File.Delete(lsToFile)
                    End If
                End Try
            End If
        End If
    End Sub


    Private Sub TI_Z0040_MenuEvent(ByVal pVal As SAPbouiCOM.IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent
        Select Case pVal.MenuUID
            Case "TI_T014"
                If pVal.BeforeAction Then
                    MyForm.Freeze(True)
                    Try
                        ioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
                        ioMtx_20 = MyForm.Items.Item("Mtx_20").Specific

                        ioDtDoc = MyForm.DataSources.DataTables.Item("DOC")
                        ioDtTempSql = MyForm.DataSources.DataTables.Add("TempDt")

                        ioDbs_TI_Z0140 = MyForm.DataSources.DBDataSources.Item("@TI_Z0140")
                        ioDbs_TI_Z0141 = MyForm.DataSources.DBDataSources.Item("@TI_Z0141")

                        ioUds_DateF = MyForm.DataSources.UserDataSources.Item("DateF")
                        ioUds_DateT = MyForm.DataSources.UserDataSources.Item("DateT")
                        ioUds_PPKD = MyForm.DataSources.UserDataSources.Item("PPKD")
                        ioUds_ShowC = MyForm.DataSources.UserDataSources.Item("ShowC")

                        ioUds_ShowC.ValueEx = "N"
                        ioUds_PPKD.ValueEx = "Y"
                        ioUds_DateF.ValueEx = Today.ToString("yyyyMMdd")
                        ioUds_DateT.ValueEx = Today.ToString("yyyyMMdd")

                        BaseFunction.AddComboBox("1", MyForm, myDataTable, "Select t10.Code,t10.Name  From [@TI_Z0150] t10", "KDGS")
                        BaseFunction.AddComboBox("1", MyForm, myDataTable, "Select t10.Code,t10.Name  From [@TI_Z0150] t10", "Mtx_20", "DftKDGS")
                        BaseFunction.AddComboBox("1", MyForm, myDataTable, "Select t10.Code,t10.Name  From [@TI_Z0150] t10", "Mtx_10", "DftKDGS")
                        'BaseFunction.GetReportType(MyForm, myDataTable)

                        If MyForm.PaneLevel <> 1 Then
                            MyForm.PaneLevel = 1
                        End If

                        setItemE()

                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.ToString())
                    Finally
                        MyForm.Freeze(False)
                    End Try
                Else
                End If
            Case "1282"
                If Not pVal.BeforeAction Then
                    If MyForm.Mode = BoFormMode.fm_ADD_MODE Then
                        If MyForm.PaneLevel <> 1 Then
                            MyForm.PaneLevel = 1
                        End If

                        setItemE()
                    End If
                End If
        End Select
    End Sub
End Class