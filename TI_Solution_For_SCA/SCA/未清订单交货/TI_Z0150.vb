Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO

Public NotInheritable Class TI_Z0150
    Inherits FormBase
    Public ioMtx_10 As Matrix

    Private ioDtDoc, ioDtTempSql As SAPbouiCOM.DataTable
    Private ibCheckLoad As Boolean = False
    Private ioListDoc As SortedList = New SortedList
    Private iiDocEntry As Integer
    Dim isBsEntry As Object


    Private Sub TI_Z0055_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "CX"

                    End Select
                End If

            Case BoEventTypes.et_CHOOSE_FROM_LIST

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

End Class