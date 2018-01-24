Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO
Imports SAPbobsCOM

Public NotInheritable Class TI_Z00071
    Inherits FormBase
    Public ioMtx_10 As Matrix

    Private ioDtDoc, ioDtTempSql As SAPbouiCOM.DataTable
    Private ibCheckLoad As Boolean = False
    Private ioListDoc As SortedList = New SortedList
    Public isCardCode, isFromUID As String
    Private ioUds_SKU, ioUds_ItemName, ioUds_DocDateF, ioUds_DocDateT As UserDataSource
    Public ioNetdt As System.Data.DataTable = New Data.DataTable


    Private Sub TI_Z0055_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Create"
                            Copy()
                        Case "8"
                            Btn_Select()
                    End Select
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
        End Select
    End Sub

    Public Sub Copy()
        ioMtx_10.FlushToDataSource()
        Dim lsSelect As String
        Dim loForm As Form
        loForm = MyApplication.Forms.Item(isFromUID)
        If Not loForm Is Nothing Then

            ioNetdt.Rows.Clear()
            Dim loMtx As Matrix = loForm.Items.Item("38").Specific
            Dim loNetNewRow As Data.DataRow
            Dim lsSKU As String
            Dim liDocEntry, liLineNum As Integer
            Dim ldQty As Double
            For i As Integer = 0 To ioDtDoc.Rows.Count - 1
                lsSelect = ioDtDoc.GetValue("U_Select", i)
                If Not String.IsNullOrEmpty(lsSelect) Then
                    lsSelect = lsSelect.Trim
                End If
                If lsSelect = "Y" Then
                    lsSKU = ioDtDoc.GetValue("ItemCode", i)
                    liDocEntry = ioDtDoc.GetValue("DocEntry", i)
                    liLineNum = ioDtDoc.GetValue("LineNum", i)
                    ldQty = ioDtDoc.GetValue("Qty", i)

                    loNetNewRow = ioNetdt.NewRow
                    loNetNewRow("ItemCode") = lsSKU
                    loNetNewRow("DocEntry") = liDocEntry
                    loNetNewRow("LineNum") = liLineNum
                    loNetNewRow("Qty") = ldQty
                    'loMtx.Columns.Item("1").Cells.Item(liMtxRowcount).Specific.Value = lsSKU

                    ioNetdt.Rows.Add(loNetNewRow)

                End If
            Next i
            MyForm.Close()
            If ioNetdt.Rows.Count > 0 Then
                Dim liMtxRowcount As Integer = loMtx.VisualRowCount
                For Each loRow1 As DataRow In ioNetdt.Rows
                    lsSKU = loRow1("ItemCode")
                    liDocEntry = loRow1("DocEntry")
                    liLineNum = loRow1("LineNum")
                    ldQty = loRow1("Qty")

                    loMtx.Columns.Item("1").Cells.Item(liMtxRowcount).Specific.Value = lsSKU
                    loMtx.Columns.Item("11").Cells.Item(liMtxRowcount).Specific.Value = ldQty
                    loMtx.Columns.Item("U_DNEntry").Cells.Item(liMtxRowcount).Specific.Value = Convert.ToString(liDocEntry)
                    loMtx.Columns.Item("U_DNLine").Cells.Item(liMtxRowcount).Specific.Value = Convert.ToString(liLineNum)

                    liMtxRowcount = liMtxRowcount + 1
                Next
            End If

        End If

    End Sub


    Public Sub Btn_Select()
        Dim lsSql As String
        Dim lsSKU, lsItemName, lsDocDateF, lsDocDateT As String
        lsSKU = ioUds_SKU.ValueEx
        If Not String.IsNullOrEmpty(lsSKU) Then
            lsSKU = lsSKU.Trim
        End If
        lsItemName = ioUds_ItemName.ValueEx
        If Not String.IsNullOrEmpty(lsItemName) Then
            lsItemName = lsItemName.Trim
        End If
        lsDocDateF = ioUds_DocDateF.ValueEx
        If Not String.IsNullOrEmpty(lsDocDateF) Then
            lsDocDateF = lsDocDateF.Trim
        End If
        lsDocDateT = ioUds_DocDateT.ValueEx
        If Not String.IsNullOrEmpty(lsDocDateT) Then
            lsDocDateT = lsDocDateT.Trim
        End If

        lsSql = "exec ORDNNotBaseODLN '" + isCardCode + "','" + lsSKU + "','" + lsItemName + "','" + lsDocDateF + "','" + lsDocDateT + "'"

        ioDtTempSql.ExecuteQuery(lsSql)
        ioDtDoc.Rows.Clear()
        Dim lsValue As String
        Dim liValue As Integer
        Dim ldValue As Double
        Dim lddValue As Date
        For i As Integer = 0 To ioDtTempSql.Rows.Count - 1
            liValue = ioDtTempSql.GetValue("DocEntry", i)
            If liValue > 0 Then
                ioDtDoc.Rows.Add(1)
                ioDtDoc.Rows.Offset = ioDtDoc.Rows.Count - 1
                ioDtDoc.SetValue("DocEntry", ioDtDoc.Rows.Offset, liValue)
                liValue = ioDtTempSql.GetValue("LineId", i)
                ioDtDoc.SetValue("LineId", ioDtDoc.Rows.Offset, liValue)
                liValue = ioDtTempSql.GetValue("LineNum", i)
                ioDtDoc.SetValue("LineNum", ioDtDoc.Rows.Offset, liValue)

                lsValue = ioDtTempSql.GetValue("ItemCode", i)
                ioDtDoc.SetValue("ItemCode", ioDtDoc.Rows.Offset, lsValue)

                lsValue = ioDtTempSql.GetValue("ItemName", i)
                ioDtDoc.SetValue("ItemName", ioDtDoc.Rows.Offset, lsValue)

                ldValue = ioDtTempSql.GetValue("U_OReQty", i)
                ioDtDoc.SetValue("U_OReQty", ioDtDoc.Rows.Offset, ldValue)
                ioDtDoc.SetValue("Qty", ioDtDoc.Rows.Offset, ldValue)

                ldValue = ioDtTempSql.GetValue("PriceAfVAT", i)
                ioDtDoc.SetValue("PriceAfVAT", ioDtDoc.Rows.Offset, ldValue)

                ldValue = ioDtTempSql.GetValue("LineTotal", i)
                ioDtDoc.SetValue("LineTotal", ioDtDoc.Rows.Offset, ldValue)

                lsValue = ioDtTempSql.GetValue("Comments", i)
                ioDtDoc.SetValue("Comments", ioDtDoc.Rows.Offset, lsValue)

                lddValue = ioDtTempSql.GetValue("DocDate", i)
                ioDtDoc.SetValue("DocDate", ioDtDoc.Rows.Offset, lddValue)
            End If
        Next i

        ioMtx_10.LoadFromDataSource()
    End Sub

    Public Sub DYBL()
        ioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
        ioDtDoc = MyForm.DataSources.DataTables.Item("DOC")
        ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSql")

        ioUds_SKU = MyForm.DataSources.UserDataSources.Item("SKU")
        ioUds_ItemName = MyForm.DataSources.UserDataSources.Item("ItemName")
        ioUds_DocDateF = MyForm.DataSources.UserDataSources.Item("DocDateF")
        ioUds_DocDateT = MyForm.DataSources.UserDataSources.Item("DocDateT")
        ioUds_DocDateF.ValueEx = Today.AddMonths(-3).ToString("yyyyMMdd")
        ioUds_DocDateT.ValueEx = Today.ToString("yyyyMMdd")

        ioNetdt.Columns.Add("DocEntry", GetType(Integer))
        ioNetdt.Columns.Add("LineNum", GetType(Integer))

        ioNetdt.Columns.Add("ItemCode", GetType(String))
        ioNetdt.Columns.Add("Qty", GetType(Double))
    End Sub
End Class