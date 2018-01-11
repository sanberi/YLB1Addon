Option Strict Off
Option Explicit On
Imports SAPbouiCOM

Public NotInheritable Class TI_Z0010
    Inherits FormBase
    Public ioMtx_10 As Matrix
    Public ioMtxItem As Item
    Public ioDtTempSql As SAPbouiCOM.DataTable
    Private ioFld As Folder
    Public ioDbds_TI_Z0010, ioDbds_TI_Z0011 As DBDataSource
    Public ibCheck As Boolean = False

    Private Sub TI_Z0040_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_ADD
                If BusinessObjectInfo.BeforeAction Then
                    Check(BubbleEvent)
                Else
                    If BusinessObjectInfo.ActionSuccess Then
                        AddMtxRow()
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_UPDATE
                If BusinessObjectInfo.BeforeAction Then
                    Check(BubbleEvent)
                Else
                    If BusinessObjectInfo.ActionSuccess Then
                        AddMtxRow()
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_LOAD
                If Not BusinessObjectInfo.BeforeAction Then
                    AddMtxRow()
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 检查数据
    ''' </summary>
    ''' <param name="BubbleEvent"></param>
    ''' <remarks></remarks>
    Private Sub Check(ByRef BubbleEvent As Boolean)
        '校验主表
        Dim lsCardCode As String
        lsCardCode = ioDbds_TI_Z0010.GetValue("Code", 0)
        If Not String.IsNullOrEmpty(lsCardCode) Then
            lsCardCode = lsCardCode.Trim
        End If
        If String.IsNullOrEmpty(lsCardCode) Then
            MyApplication.MessageBox("代码不能为空！")
            BubbleEvent = False
            Return
        End If
        '必须是1位
        'If lsCardCode.Length > 1 Then
        '    MyApplication.MessageBox("解决方案代码不能大于1位！")
        '    BubbleEvent = False
        '    Return
        'End If

        'Dim lsSql As String
        'lsSql = "Declare @Code as Nvarchar(1) Set @Code='2' Select 'A' where  @Code like '[A-Z]'"
        'ioDtTempSql.ExecuteQuery(lsSql)
        'If Not ioDtTempSql.IsEmpty Then
        '    MyApplication.MessageBox("解决方案代码必须在A-Z之间！")
        '    BubbleEvent = False
        '    Return
        'End If

        Dim lsName As String
        lsName = ioDbds_TI_Z0010.GetValue("Name", 0)
        If Not String.IsNullOrEmpty(lsName) Then
            lsName = lsName.Trim
        End If
        If String.IsNullOrEmpty(lsName) Then
            MyApplication.MessageBox("名称不能为空！")
            BubbleEvent = False
            Return
        End If


        Dim lsU_Code, lsFromType, lsLeftCode As String
        ioMtx_10.FlushToDataSource()
        Dim loHtRY As Hashtable = New Hashtable

        'For i As Integer = 0 To ioDbds_TI_Z0011.Size - 1
        '    If i >= ioDbds_TI_Z0011.Size Then
        '        Exit For
        '    End If
        '    lsU_Code = ioDbds_TI_Z0011.GetValue("U_Code", i)
        '    If Not String.IsNullOrEmpty(lsU_Code) Then
        '        lsU_Code = lsU_Code.Trim
        '    End If
        '    If String.IsNullOrEmpty(lsU_Code) Then
        '        ioDbds_TI_Z0011.RemoveRecord(i)
        '        i = i - 1
        '        Continue For
        '    End If

        '    If Not loHtRY.ContainsKey(lsU_Code) Then
        '        loHtRY.Add(lsU_Code, lsU_Code)
        '    Else
        '        MyApplication.MessageBox("行代码不能重复，行：" + lsU_Code + "！")
        '        BubbleEvent = False
        '        Return
        '    End If
        '    '检查窗体代码不能为空
        '    lsFromType = ioDbds_TI_Z0011.GetValue("U_FromType", i)
        '    If Not String.IsNullOrEmpty(lsFromType) Then
        '        lsFromType = lsFromType.Trim
        '    End If
        '    If String.IsNullOrEmpty(lsFromType) Then
        '        MyApplication.MessageBox("行中的窗体类型不能为空，行：" + lsU_Code + "！")
        '        BubbleEvent = False
        '        Return
        '    End If

        '    '第一个字母必须相同
        '    lsLeftCode = Left(lsU_Code, 1)
        '    If lsLeftCode <> lsCardCode Then
        '        MyApplication.MessageBox("行中的打印格式代码的首位和解决方案相同，行：" + lsU_Code + "！")
        '        BubbleEvent = False
        '        Return
        '    End If

        '    If lsU_Code.Length <> 4 Then
        '        MyApplication.MessageBox("行中的打印格式代码的字符长度必须为4，行：" + lsU_Code + "！")
        '        BubbleEvent = False
        '        Return
        '    End If
        'Next i
    End Sub


    Private Sub TI_Z0080_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_FORM_RESIZE
                If Not pVal.BeforeAction Then
                    If ibCheck Then
                        FormRize()
                    End If
                End If
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "Fld_10"
                            If MyForm.PaneLevel <> 1 Then
                                MyForm.PaneLevel = 1
                            End If
                    End Select
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
                If Not pVal.BeforeAction And pVal.ItemChanged And pVal.ColUID = "TempName" Then
                    AddMtxRow()
                End If
        End Select
    End Sub


    Public Sub FormRize()
        Dim loItem As Item
        loItem = MyForm.Items.Item("15")
        If Not loItem Is Nothing Then
            loItem.Width = ioMtxItem.Width + 15
            loItem.Height = ioMtxItem.Height + 13 '
        End If
    End Sub


    Private Sub TI_Z0010_MenuEvent(ByVal pVal As SAPbouiCOM.IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent
        Select Case pVal.MenuUID
            Case "TI_T010"
                If pVal.BeforeAction Then
                    MyForm.Freeze(True)
                    Try
                        ioFld = MyForm.Items.Item("Fld_10").Specific
                        ioFld.Select()

                        ioDbds_TI_Z0010 = MyForm.DataSources.DBDataSources.Item("@TI_Z0010")
                        ioDbds_TI_Z0011 = MyForm.DataSources.DBDataSources.Item("@TI_Z0011")

                        ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSQL")

                        ioMtx_10 = MyForm.Items.Item("Mtx_10").Specific
                        ioMtxItem = MyForm.Items.Item("Mtx_10")

                        FormRize()
                        '  AddMtxRow()

                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.ToString())
                    Finally
                        MyForm.Freeze(False)
                    End Try

                    ibCheck = False
                Else
                    ibCheck = True
                End If
            Case "1282"
                '添加
                If Not pVal.BeforeAction Then
                    SetValues()
                End If

        End Select
    End Sub

    Private Sub SetValues()
        '初始化明细表数据
        ioDbds_TI_Z0011.InsertRecord(ioDbds_TI_Z0011.Size)
        ioDbds_TI_Z0011.RemoveRecord(ioDbds_TI_Z0011.Size - 1)
        ioMtx_10.LoadFromDataSource()
    End Sub


    Private Sub AddMtxRow()
        ioMtxItem.AffectsFormMode = False
        Try
            Dim lsEmpId As String
            lsEmpId = ioMtx_10.Columns.Item("TempName").Cells.Item(ioMtx_10.VisualRowCount).Specific.Value
            If Not String.IsNullOrEmpty(lsEmpId) Then
                lsEmpId = lsEmpId.Trim
            End If
            If Not String.IsNullOrEmpty(lsEmpId) Then
                ioDbds_TI_Z0011.InsertRecord(ioDbds_TI_Z0011.Size)
                ioDbds_TI_Z0011.Offset = ioDbds_TI_Z0011.Size - 1
                ioMtx_10.AddRow(1, ioMtx_10.VisualRowCount)
            End If
            ioMtx_10.FlushToDataSource()
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
        Finally
            ioMtxItem.AffectsFormMode = True
        End Try
    End Sub
End Class