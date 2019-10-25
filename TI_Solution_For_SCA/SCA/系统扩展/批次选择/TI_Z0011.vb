Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices
Imports TIModule

Public NotInheritable Class TI_Z0011
    Inherits FormBase
    Private ioMtx_10, ioMtx_30 As Matrix
    Private ioBtn_OK As Item
    Private ioDtTempSql As SAPbouiCOM.DataTable
    Private ioSList As Hashtable = New Hashtable
    Private ioItemAutoSelectButton, ioItemAuto As Item


    Private Sub TI_Z0011_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_LOAD
                If pVal.BeforeAction Then
                    Dim loItem As Item
                    loItem = MyForm.Items.Add("Create", BoFormItemTypes.it_BUTTON)
                    Dim loBtn_Create1 As Item
                    Dim ioBtn_Create As SAPbouiCOM.Button
                    loBtn_Create1 = MyForm.Items.Item("16")
                    loItem.Left = loBtn_Create1.Left - 200
                    loItem.Width = loBtn_Create1.Width + 30
                    loItem.Top = loBtn_Create1.Top
                    loItem.Height = loBtn_Create1.Height
                    loItem.LinkTo = "16"
                    ioBtn_Create = loItem.Specific
                    ioBtn_Create.Caption = "自动选择（无原厂批次）"

                    ioItemAuto = loItem
                    ioItemAutoSelectButton = loBtn_Create1

                    ioMtx_10 = MyForm.Items.Item("3").Specific
                    ioBtn_OK = MyForm.Items.Item("1")
                    ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSql")

                End If
            Case BoEventTypes.et_ITEM_PRESSED
                '自动选择所有物料批次
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Create"
                            '批次自动选择
                            MyForm.Freeze(True)
                            ioItemAuto.Visible = False
                            Try
                                Dim lsItemCode, lsPCGL As String
                                Dim liRCount As Integer = ioMtx_10.VisualRowCount
                                Dim liSelectRow As Integer
                                liSelectRow = ioMtx_10.GetNextSelectedRow(0, BoOrderType.ot_RowOrder)
                                For i As Integer = 1 To ioMtx_10.VisualRowCount
                                    ioMtx_10.Columns.Item("0").Cells.Item(i).Click(BoCellClickType.ct_Regular, BoModifiersEnum.mt_None)
                                    Try
                                        MyApplication.SetStatusBarMessage("正在自动选择批次，总计" + Convert.ToString(liRCount) + "行,当前" + Convert.ToString(i) + "行！", BoMessageTime.bmt_Short, False)
                                        lsItemCode = ioMtx_10.Columns.Item("1").Cells.Item(i).Specific.Value
                                        lsPCGL = GetItemPCGLFlag(lsItemCode)
                                        If lsPCGL = "N" Then
                                            If ioItemAutoSelectButton.Enabled Then
                                                ioItemAutoSelectButton.Click(BoCellClickType.ct_Regular)
                                            End If
                                            If MyForm.Mode = BoFormMode.fm_UPDATE_MODE Then
                                                ioBtn_OK.Click(BoCellClickType.ct_Regular)
                                            End If
                                        End If
                                    Catch ex As Exception
                                        MyApplication.SetStatusBarMessage(ex.ToString())
                                    End Try
                                Next i
                                MyApplication.StatusBar.SetText("自动分配批次成功!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)

                                If liSelectRow > 0 Then
                                    ioMtx_10.Columns.Item("0").Cells.Item(liSelectRow).Click(BoCellClickType.ct_Regular, BoModifiersEnum.mt_None)
                                End If
                            Catch ex As Exception
                                MyApplication.SetStatusBarMessage(ex.ToString(), BoMessageTime.bmt_Short, False)
                            Finally
                                ioItemAuto.Visible = True
                                MyForm.Freeze(False)
                            End Try
                    End Select
                End If
        End Select
    End Sub

    Private Function GetItemPCGLFlag(ByVal lsItemCode As String) As String
        Dim lsFlag As String
        lsFlag = "N"
        lsItemCode = lsItemCode.Replace("'", "''")
        ' If Not ioSList.ContainsKey(lsItemCode) Then
        Dim lsSql As String
        lsSql = "Select isnull(t10.U_BatchMan,'N') PCGL From OITM t10 where t10.ItemCode='" + lsItemCode + "'"
        ioDtTempSql.ExecuteQuery(lsSql)
        lsFlag = ioDtTempSql.GetValue(0, 0)
        '  ioSList.Add(lsItemCode, lsFlag)
        '  Else
        '  lsFlag = ioSList.Item(lsItemCode)
        '  End If
        Return lsFlag
    End Function
End Class
