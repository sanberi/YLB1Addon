Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices
Imports TIModule

Public NotInheritable Class TI_Z0012
    Inherits FormBase
    Public ioDbds_TI_Z0800, ioDbds_TI_Z0801 As DBDataSource
    Public ioMatrix As Matrix
    Public ioDtDocSub As SAPbouiCOM.DataTable
    Public isCardCode As String
    Public iiDocEntry As Integer
    Public isCheckAddorUpdate As String
    Private Sub TI_Z0800_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_ADD
                If BusinessObjectInfo.BeforeAction Then
                    Check(BubbleEvent)
                    isCheckAddorUpdate = "A"
                Else
                End If
            Case BoEventTypes.et_FORM_DATA_UPDATE
                If BusinessObjectInfo.BeforeAction Then
                    Check(BubbleEvent)
                    isCheckAddorUpdate = "U"
                End If

            Case BoEventTypes.et_FORM_DATA_LOAD
                If Not BusinessObjectInfo.BeforeAction Then
                    MyForm.Items.Item("7").AffectsFormMode = False
                    'MyForm.Items.Item("19").AffectsFormMode = False
                    Try
                        ' Integer.TryParse(ioDbds_TI_Z0800.GetValue("DocEntry", 0), iiDocEntry)
                        AddMtxRow()
                    Catch ex As Exception

                    Finally
                        MyForm.Items.Item("7").AffectsFormMode = True
                        'MyForm.Items.Item("19").AffectsFormMode = True
                    End Try
                End If
        End Select
    End Sub

    Public Sub Check(ByRef BubbleEvent As Boolean)
        Try
            ioMatrix.FlushToDataSource()
            Dim lsBaseEntry, lsItemCode As String
            lsBaseEntry = ioDbds_TI_Z0801.GetValue("U_ItemCode", 0)
            If String.IsNullOrEmpty(lsBaseEntry) Then
                MyApplication.SetStatusBarMessage("空单据不能添加！")
                BubbleEvent = False
            End If
            If ioDbds_TI_Z0801.Size > 0 Then
                For i As Integer = 0 To ioDbds_TI_Z0801.Size - 1
                    If i >= ioDbds_TI_Z0801.Size Then
                        Exit For
                    End If
                    lsItemCode = ioDbds_TI_Z0801.GetValue("U_ItemCode", i)
                    If Not String.IsNullOrEmpty(lsItemCode) Then
                        lsItemCode = lsItemCode.Trim
                    End If
                    If String.IsNullOrEmpty(lsItemCode) Then
                        ioDbds_TI_Z0801.RemoveRecord(i)
                        i = i - 1
                        Continue For
                    End If
                Next

            End If
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
            BubbleEvent = False
            Return
        End Try
    End Sub

    Private Sub AddMtxRow()
        ' ioMtx_10.AffectsFormMode = False
        MyForm.Items.Item("7").AffectsFormMode = False
        Try

            If ioMatrix.VisualRowCount = 0 Then
                ioDbds_TI_Z0801.InsertRecord(ioDbds_TI_Z0801.Size)
                ioDbds_TI_Z0801.Offset = ioDbds_TI_Z0801.Size - 1
                ioMatrix.AddRow(1, ioMatrix.VisualRowCount)
            Else
                Dim lsEmpId As String
                lsEmpId = ioMatrix.Columns.Item("ItemCode").Cells.Item(ioMatrix.VisualRowCount).Specific.Value
                If Not String.IsNullOrEmpty(lsEmpId) Then
                    lsEmpId = lsEmpId.Trim
                End If
                If Not String.IsNullOrEmpty(lsEmpId) Then
                    ioDbds_TI_Z0801.InsertRecord(ioDbds_TI_Z0801.Size)
                    ioDbds_TI_Z0801.Offset = ioDbds_TI_Z0801.Size - 1
                    ioMatrix.AddRow(1, ioMatrix.VisualRowCount)
                End If
                ' ioMatrix.FlushToDataSource()
            End If
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
        Finally
            MyForm.Items.Item("7").AffectsFormMode = True
        End Try
    End Sub

    Private Sub TI_Z0800_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        If pVal.EventType = BoEventTypes.et_CHOOSE_FROM_LIST Then
            Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
            Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
            If Not lodt Is Nothing Then
                '物料代码和物料名称
                If pVal.ItemUID = "7" And Not pVal.Before_Action Then
                    If pVal.ColUID = "ItemCode" Or pVal.ColUID = "ItemName" Then

                        Dim liRowCount As Integer = (pVal.Row + lodt.Rows.Count) - 1
                        Dim liMaxMtxId As Integer
                        ioMatrix.FlushToDataSource()
                        liMaxMtxId = ioMatrix.VisualRowCount
                        Dim liLineId, liMaxId As Integer
                        Dim lsItemCode, lsItemName As String
                        Dim liDtIndex As Integer = 0
                        For i As Integer = pVal.Row To liRowCount
                            If i > liMaxMtxId Then
                                '插入DBds
                                ioDbds_TI_Z0801.InsertRecord(ioDbds_TI_Z0801.Size)
                                ioDbds_TI_Z0801.Offset = ioDbds_TI_Z0801.Size - 1
                            Else
                                ioDbds_TI_Z0801.Offset = (i - 1)
                            End If
                            Integer.TryParse(ioDbds_TI_Z0801.GetValue("LineId", ioDbds_TI_Z0801.Offset), liLineId)
                            If liLineId <= 0 Then
                                ' If Not lbCheckMaxId Then
                                liMaxId = GetMaxLine()
                                ' lbCheckMaxId = True
                                'End If
                                '   liLineId = liMaxId
                                ioDbds_TI_Z0801.SetValue("LineId", ioDbds_TI_Z0801.Offset, Convert.ToString(liMaxId))
                                '  liMaxId = liMaxId + 1
                            End If

                            lsItemCode = lodt.GetValue("ItemCode", liDtIndex)
                            lsItemName = lodt.GetValue("ItemName", liDtIndex)

                            ioDbds_TI_Z0801.SetValue("U_ItemCode", ioDbds_TI_Z0801.Offset, lsItemCode)
                            ioDbds_TI_Z0801.SetValue("U_ItemName", ioDbds_TI_Z0801.Offset, lsItemName)
                            liDtIndex = liDtIndex + 1
                        Next i

                        ioMatrix.LoadFromDataSource()
                        Dim lsItemCodeTemp As String
                        lsItemCodeTemp = ioMatrix.Columns.Item("ItemCode").Cells.Item(ioMatrix.VisualRowCount).Specific.Value
                        If Not String.IsNullOrEmpty(lsItemCodeTemp) Then
                            lsItemCodeTemp = lsItemCodeTemp.Trim
                        End If
                        If Not String.IsNullOrEmpty(lsItemCodeTemp) Then
                            ioDbds_TI_Z0801.InsertRecord(ioDbds_TI_Z0801.Size)
                            ioDbds_TI_Z0801.Offset = ioDbds_TI_Z0801.Size - 1
                            ' ioMatrix.LoadFromDataSource()
                            ioMatrix.AddRow(1, ioMatrix.VisualRowCount)
                        End If

                        If MyForm.Mode = BoFormMode.fm_OK_MODE Then
                            MyForm.Mode = BoFormMode.fm_UPDATE_MODE
                        End If
                    End If
                End If

                '客户代码和客户名称
                If pVal.ItemUID = "4" Or pVal.ItemUID = "6" Then
                    Dim lsCardCode, lsCardName As String
                    lsCardCode = lodt.GetValue("CardCode", 0)
                    lsCardName = lodt.GetValue("CardName", 0)
                    ioDbds_TI_Z0800.SetValue("Code", 0, lsCardCode)
                    ioDbds_TI_Z0800.SetValue("Name", 0, lsCardCode)
                    ioDbds_TI_Z0800.SetValue("U_CardName", 0, lsCardName)
                    If MyForm.Mode = BoFormMode.fm_OK_MODE Then
                        MyForm.Mode = BoFormMode.fm_UPDATE_MODE
                    End If
                End If

            End If

        End If
        If pVal.ItemUID = "1" And Not pVal.BeforeAction Then
            If pVal.ActionSuccess Then
                If MyForm.Mode <> BoFormMode.fm_FIND_MODE Then
                    If isCheckAddorUpdate = "A" Or isCheckAddorUpdate = "U" Then
                        MyForm.Freeze(True)
                        Try
                            If isCheckAddorUpdate = "A" Then
                                MyForm.Mode = BoFormMode.fm_OK_MODE
                            End If

                            Dim lsTemp As String
                            Dim lsSQLTI_Z0800 As String = "select 'A' as 'Test' from [@TI_Z0800] where Code='" + isCardCode.Trim + "'"
                            ioDtDocSub.ExecuteQuery(lsSQLTI_Z0800)
                            If ioDtDocSub.Rows.Count > 0 Then
                                lsTemp = ioDtDocSub.GetValue("Test", 0)
                                If Not String.IsNullOrEmpty(lsTemp) Then
                                    '存在，直接加载该界面
                                    Dim loConditions As SAPbouiCOM.Conditions
                                    Dim loCondition As SAPbouiCOM.Condition
                                    loConditions = MyApplication.CreateObject(BoCreatableObjectType.cot_Conditions)
                                    loCondition = loConditions.Add
                                    loCondition.Alias = "Code"
                                    loCondition.Operation = BoConditionOperation.co_EQUAL
                                    loCondition.CondVal = Convert.ToString(isCardCode.Trim)
                                    ioDbds_TI_Z0800.Query(loConditions)
                                    ioDbds_TI_Z0801.Query(loConditions)
                                    ioMatrix.LoadFromDataSource()
                                End If
                            End If
                            AddMtxRow()

                            isCheckAddorUpdate = ""

                        Catch ex As Exception
                            MyApplication.SetStatusBarMessage(ex.Message, BoMessageTime.bmt_Short, True)
                            BubbleEvent = False

                        Finally
                            MyForm.Freeze(False)
                        End Try

                    End If

                End If


                'AddMtxRow()
                '

            End If
        End If
    End Sub



    ''' <summary>
    ''' 获取最大行号
    ''' </summary>
    ''' <remarks></remarks>
    Private Function GetMaxLine()
        Dim liMaxLine, liLine As Integer
        liMaxLine = 0
        Dim lsItemCode As String
        '  ioMatrix.FlushToDataSource()
        For i As Integer = 0 To ioDbds_TI_Z0801.Size - 1
            lsItemCode = ioDbds_TI_Z0801.GetValue("U_ItemCode", i)
            If String.IsNullOrEmpty(lsItemCode) Then
                Continue For
            End If
            Integer.TryParse(ioDbds_TI_Z0801.GetValue("LineId", i), liLine)
            If liLine > liMaxLine Then
                liMaxLine = liLine
            End If
        Next
        liMaxLine = liMaxLine + 1
        Return liMaxLine
    End Function
End Class
