Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO
Imports SAPbobsCOM

Public NotInheritable Class TI_Z0150
    Inherits FormBase
    Public ioMtx_10 As Matrix

    Private ioDtDoc, ioDtTempSql As SAPbouiCOM.DataTable
    Private ibCheckLoad As Boolean = False
    Private ioListDoc As SortedList = New SortedList


    Private Sub TI_Z0055_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Create"
                            CreateODLN()
                    End Select
                End If

            Case BoEventTypes.et_CHOOSE_FROM_LIST
                If Not pVal.BeforeAction Then
                    Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
                    Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
                    If Not lodt Is Nothing Then
                        Select Case pVal.ItemUID
                            Case "Mtx_10"
                                If pVal.ColUID = "WhsCode" Then
                                    Dim lsWhsCode, lsItemCode As String
                                    lsWhsCode = lodt.GetValue("WhsCode", 0)
                                    If Not String.IsNullOrEmpty(lsWhsCode) Then
                                        lsWhsCode = lsWhsCode.Trim
                                    End If
                                    If Not String.IsNullOrEmpty(lsWhsCode) Then
                                        ioDtDoc.Rows.Offset = pVal.Row - 1
                                        ioMtx_10.GetLineData(pVal.Row)
                                        lsItemCode = ioDtDoc.GetValue("ItemCode", ioDtDoc.Rows.Offset)
                                        ioDtDoc.SetValue("WhsCode", ioDtDoc.Rows.Offset, lsWhsCode)

                                        Dim lsSql As String
                                        lsSql = "Select t10.OnHand from OITW t10 where t10.ItemCode='" + lsItemCode + "' and t10.WhsCode='" + lsWhsCode + "'"
                                        ioDtTempSql.ExecuteQuery(lsSql)
                                        Dim ldOnHand As Double
                                        Double.TryParse(ioDtTempSql.GetValue("OnHand", 0), ldOnHand)
                                        ioDtDoc.SetValue("OnHand", ioDtDoc.Rows.Offset, ldOnHand)
                                        ioMtx_10.SetLineData(pVal.Row)
                                        If MyForm.Mode = BoFormMode.fm_OK_MODE Then
                                            MyForm.Mode = BoFormMode.fm_UPDATE_MODE
                                        End If
                                    End If
                                End If
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
                        End Select
                    End If
                End If
            Case BoEventTypes.et_CLICK
                If pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Mtx_10"
                            If pVal.Row <= 0 Then
                                BubbleEvent = False
                                Return
                            End If
                            ioMtx_10.SelectRow(pVal.Row, True, False)
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
    ''' 创建交货单
    ''' </summary>
    Public Sub CreateODLN()
        ioMtx_10.FlushToDataSource()
        Dim lsSelect, lsCardCode As String
        Dim loList As SortedList = New SortedList()
        Dim loArrayList As ArrayList = New ArrayList()
        Dim loListItem As SortedList = New SortedList()
        Dim lsItemCode, lsWhsCode, lsItemCodeStr As String
        Dim liRowCount, liRowCountCardCode As Integer
        liRowCount = 0
        For i As Integer = 0 To ioDtDoc.Rows.Count - 1
            lsSelect = ioDtDoc.GetValue("U_Select", i)
            If Not String.IsNullOrEmpty(lsSelect) Then
                lsSelect = lsSelect.Trim()
            End If
            If (lsSelect = "Y") Then
                lsCardCode = ioDtDoc.GetValue("CardCode", i)
                lsItemCode = ioDtDoc.GetValue("ItemCode", i)
                lsWhsCode = ioDtDoc.GetValue("WhsCode", i)
                If Not loList.ContainsKey(lsCardCode) Then
                    loArrayList = New ArrayList()
                    loArrayList.Add(i)
                    loList.Add(lsCardCode, loArrayList)
                    loListItem.Add(lsCardCode, "('" + lsItemCode + "','" + lsWhsCode + "'),")
                Else
                    loArrayList = loList.Item(lsCardCode)
                    loArrayList.Add(i)
                    loList.Item(lsCardCode) = loArrayList
                    lsItemCodeStr = loListItem.Item(lsCardCode)
                    lsItemCodeStr = lsItemCodeStr + "('" + lsItemCode + "','" + lsWhsCode + "'),"
                    loListItem.Item(lsCardCode) = lsItemCodeStr
                End If

                liRowCount = liRowCount + 1
            End If
        Next i
        If liRowCount <= 0 Then
            Return
        End If
        liRowCountCardCode = loList.Count
        If MyApplication.MessageBox("有" + Convert.ToString(liRowCount） + "行数据即将进行交货，交货客户数:" + Convert.ToString(liRowCountCardCode), 1, "是", "否") <> 1 Then
            Return
        End If

        Dim lodt As System.Data.DataTable = New Data.DataTable
        lodt.Columns.Add("ItemCode", GetType(String))
        lodt.Columns.Add("WhsCode", GetType(String))
        lodt.Columns.Add("BatchNum", GetType(String))
        lodt.Columns.Add("InDate", GetType(Date))
        lodt.Columns.Add("OpenQty", GetType(Decimal))
        Dim loNewRow As DataRow
        Dim loDoc15 As SAPbobsCOM.Documents
        Dim lsBatchSql, lsBatchNum, lsSelectString As String
        Dim ldInDate As Date
        Dim ldOpenQty, ldDoQty As Double
        Dim loSelectRow As DataRow()
        Dim ldBOpenQty, ldBatchLQty As Double
        Dim liAddRow, liBaseEntry, liBaseLine， liBatchLine As Integer
        Dim lsErrMsgDel As String
        Dim liErrCodeDel, liODLNEntry As Integer
        Dim lsReceiWare As String
        Dim loCreateSOList As ArrayList = New ArrayList()
        Dim lsSbString As String = ""
        For i As Integer = 0 To loList.Count - 1
            lsCardCode = loList.GetKey(i)
            loArrayList = loList.Item(lsCardCode)
            loDoc15 = MyCompany.GetBusinessObject(BoObjectTypes.oDeliveryNotes) '销售交货
            If Not loDoc15 Is Nothing Then
                lsBatchSql = "Declare  @TempWhs table(ItemCode Nvarchar(30),WhsCode Nvarchar(30)) "
                lsBatchSql = lsBatchSql + " Insert into @TempWhs(ItemCode,WhsCode) Values "
                lsItemCodeStr = loListItem.Item(lsCardCode)
                lsItemCodeStr = Left(lsItemCodeStr, lsItemCodeStr.Length - 1)
                lsBatchSql = lsBatchSql + lsItemCodeStr
                lsBatchSql = lsBatchSql + " select T12.ItemCode,t12.WhsCode,t12.BatchNum,t12.InDate,(T12.Quantity-isnull(t12.IsCommited,0)) as OpenQty "
                lsBatchSql = lsBatchSql + " from OIBT T12  inner join @TempWhs t13 on t12.ItemCode=t13.ItemCode and t12.WhsCode=t13.WhsCode and t12.Status='0' "
                lsBatchSql = lsBatchSql + " where (T12.Quantity-isnull(t12.IsCommited,0)) >0 "
                ioDtTempSql.ExecuteQuery(lsBatchSql)
                For j As Integer = 0 To ioDtTempSql.Rows.Count - 1
                    lsItemCode = ioDtTempSql.GetValue("ItemCode", j)
                    lsWhsCode = ioDtTempSql.GetValue("WhsCode", j)
                    lsBatchNum = ioDtTempSql.GetValue("BatchNum", j)
                    ldInDate = ioDtTempSql.GetValue("InDate", j)
                    ldOpenQty = ioDtTempSql.GetValue("OpenQty", j)
                    loNewRow = lodt.NewRow()
                    loNewRow("ItemCode") = lsItemCode
                    loNewRow("WhsCode") = lsWhsCode
                    loNewRow("BatchNum") = lsBatchNum
                    loNewRow("InDate") = ldInDate
                    loNewRow("OpenQty") = ldOpenQty
                    lodt.Rows.Add(loNewRow)
                Next j

                loDoc15.CardCode = lsCardCode
                loDoc15.UserFields.Fields.Item("U_ReturnReason").Value = "向导创建"
                liAddRow = 0
                loArrayList.Sort()
                For Each index1 As Integer In loArrayList
                    ldDoQty = ioDtDoc.GetValue("DOQty", index1)
                    ldBOpenQty = ldDoQty
                    If ldDoQty > 0 Then
                        lsItemCode = ioDtDoc.GetValue("ItemCode", index1)
                        lsWhsCode = ioDtDoc.GetValue("WhsCode", index1)
                        liBaseEntry = ioDtDoc.GetValue("DocEntry", index1)
                        liBaseLine = ioDtDoc.GetValue("LineNum", index1)
                        lsReceiWare = ioDtDoc.GetValue("ReceiWare", index1)
                        If liAddRow > 0 Then
                            loDoc15.Lines.Add()
                        End If
                        liAddRow = liAddRow + 1
                        liBatchLine = 0
                        loDoc15.Lines.SetCurrentLine(loDoc15.Lines.Count - 1)
                        loDoc15.Lines.BaseType = 17
                        loDoc15.Lines.BaseEntry = liBaseEntry
                        loDoc15.Lines.BaseLine = liBaseLine
                        loDoc15.Lines.Quantity = ldDoQty
                        loDoc15.Lines.WarehouseCode = lsWhsCode
                        loDoc15.Lines.UserFields.Fields.Item("U_ReceiWare").Value = lsReceiWare
                        lsSelectString = "ItemCode='" + lsItemCode + "' and WhsCode='" + lsWhsCode + "' and OpenQty>0 "
                        loSelectRow = lodt.Select(lsSelectString, "InDate ASC")
                        If (loSelectRow.Length > 0) Then
                            For Each lorows1 As DataRow In loSelectRow
                                ldOpenQty = lorows1.Item("OpenQty")
                                If ldBOpenQty >= ldOpenQty Then
                                    ldBatchLQty = ldOpenQty
                                Else
                                    ldBatchLQty = ldBOpenQty
                                End If
                                ldBOpenQty = ldBOpenQty - ldOpenQty
                                ldOpenQty = ldOpenQty - ldBatchLQty
                                lorows1.BeginEdit()
                                lorows1.Item("OpenQty") = ldOpenQty
                                lorows1.EndEdit()

                                If ldBatchLQty > 0 Then
                                    lsBatchNum = lorows1.Item("BatchNum")
                                    If liBatchLine > 0 Then
                                        loDoc15.Lines.BatchNumbers.Add()
                                    End If
                                    loDoc15.Lines.BatchNumbers.SetCurrentLine(loDoc15.Lines.BatchNumbers.Count - 1)
                                    loDoc15.Lines.BatchNumbers.BatchNumber = lsBatchNum
                                    loDoc15.Lines.BatchNumbers.Quantity = ldBatchLQty
                                    liBatchLine = liBatchLine + 1
                                End If
                                If ldBOpenQty <= 0 Then
                                    Exit For
                                End If
                            Next
                        End If

                        If ldBOpenQty > 0 Then
                            Continue For
                        End If
                    End If
                Next


                loDoc15.Add()
                MyCompany.GetLastError(liErrCodeDel, lsErrMsgDel)
                If liErrCodeDel = 0 Then
                    liODLNEntry = MyCompany.GetNewObjectKey()
                    loCreateSOList.Add(liODLNEntry)

                    For Each index1 As Integer In loArrayList
                        ioDtDoc.Rows.Offset = index1
                        ioMtx_10.GetLineData(index1 + 1)
                        ioDtDoc.SetValue("U_Select", index1, "N")
                        ioMtx_10.SetLineData(index1 + 1)
                    Next
                Else
                    lsSbString = lsSbString + ",客户：" + lsCardCode + ",错误信息：" + lsErrMsgDel + ","
                End If
            End If
        Next

        If Not String.IsNullOrEmpty(lsSbString) Then
            MyApplication.MessageBox("创建单据出现错误，错误信息：" + lsSbString, 1, "是", "否")
        Else
            MyForm.Close()
        End If
        If loCreateSOList.Count > 0 Then
            Dim loForm As Form
            For Each lidocEntry1 As Integer In loCreateSOList
                MyApplication.ActivateMenuItem("2051")
                loForm = MyApplication.Forms.ActiveForm()
                If Not loForm Is Nothing Then
                    loForm.Mode = BoFormMode.fm_FIND_MODE
                    loForm.Items.Item("8").Specific.Value = lidocEntry1
                    loForm.Items.Item("1").Click(BoCellClickType.ct_Regular)
                End If
            Next
        End If


    End Sub

    Public Sub DYBL()
        ioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
        ioDtDoc = MyForm.DataSources.DataTables.Item("DOC")
        ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSql")
    End Sub
End Class