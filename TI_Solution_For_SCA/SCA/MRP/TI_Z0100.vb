Option Strict Off
Option Explicit On
Imports SAPbouiCOM

Public NotInheritable Class TI_Z0100
    Inherits FormBase
    Public ioMtx_10 As Matrix
    Public ioMtxItem As Item
    Public ioDtTempSql As SAPbouiCOM.DataTable
    Private ioFld1, ioFld2 As Folder
    Public ioDbds_TI_Z0100, ioDbds_TI_Z0101 As DBDataSource
    Public ibCheck As Boolean = False
    Public ioUds_CardCode, ioUds_CardName As UserDataSource

    Private Sub TI_Z0100_FormDataEvent(ByRef BusinessObjectInfo As BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles Me.FormDataEvent
        Select Case BusinessObjectInfo.EventType
            Case BoEventTypes.et_FORM_DATA_ADD
                If BusinessObjectInfo.BeforeAction Then
                    '   Check(BubbleEvent)
                Else
                    If BusinessObjectInfo.ActionSuccess Then
                        '  AddMtxRow()
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_UPDATE
                If BusinessObjectInfo.BeforeAction Then
                    '   Check(BubbleEvent)
                Else
                    If BusinessObjectInfo.ActionSuccess Then
                        '  AddMtxRow()
                    End If
                End If
            Case BoEventTypes.et_FORM_DATA_LOAD
                If Not BusinessObjectInfo.BeforeAction Then
                    '  AddMtxRow()
                End If
        End Select
    End Sub

    Private Sub TI_Z0100_ItemEvent(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
           Select pVal.EventType
            Case BoEventTypes.et_CLICK
                If Not pVal.BeforeAction And pVal.ItemUID = "3" Then
                    MyForm.PaneLevel = 1
                End If
                If Not pVal.BeforeAction And pVal.ItemUID = "4" Then
                    MyForm.PaneLevel = 2
                End If
                '查询，查询出所有的销售订单，公司安全库存，客户备货等信息
                If Not pVal.Before_Action And pVal.ItemUID = "Search" Then
                    Dim lsCountDate, lsIsOrder, lsIsCusBack, lsIsSafeStock, lsPurchaserMain As String
                    ioDbds_TI_Z0100 = MyForm.DataSources.DBDataSources.Item("@TI_Z0100")
                    ioDbds_TI_Z0101 = MyForm.DataSources.DBDataSources.Item("@TI_Z0101")
                    ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSQL1")
                    ioMtx_10 = MyForm.Items.Item("13").Specific

                    lsCountDate = ioDbds_TI_Z0100.GetValue("U_CountDate", 0)
                    lsIsOrder = ioDbds_TI_Z0100.GetValue("U_IsOrder", 0)
                    lsIsCusBack = ioDbds_TI_Z0100.GetValue("U_IsCusBack", 0)
                    lsIsSafeStock = ioDbds_TI_Z0100.GetValue("U_IsSafeStock", 0)
                    lsPurchaserMain = ioDbds_TI_Z0100.GetValue("U_Purchaser", 0)
                    If String.IsNullOrEmpty(lsPurchaserMain) Then
                        MyApplication.MessageBox("请先选择采购员!")
                        BubbleEvent = False
                        Return
                    End If
                    '清除DBDatasource
                    ioDbds_TI_Z0101.Clear()
                    Dim lsSQL As String = "EXEC [YL_ViceDemand] '" + lsCountDate + "'"
                    ioDtTempSql.ExecuteQuery(lsSQL)
                    '将数据塞在销售订单明细中
                    Dim lsBaseEntry, lsBaseType, lsBaseLine, lsCardCode, lsCardName, lsItemCode, lsItemName, lsPurchaser, lsBrand, lsVendors, lsSaler As String
                    Dim ldQuantity As Decimal
                    Dim liPurCircle, liExcDays As Integer
                    Dim ldDeliDate, ldPurDate As DateTime
                    If ioDtTempSql.Rows.Count > 0 Then
                        For i As Integer = 0 To ioDtTempSql.Rows.Count - 1
                            lsBaseType = ioDtTempSql.GetValue("BaseType", i)
                            lsBaseEntry = ioDtTempSql.GetValue("BaseEntry", i)
                            lsBaseLine = ioDtTempSql.GetValue("BaseLine", i)
                            lsCardCode = ioDtTempSql.GetValue("CardCode", i)
                            lsCardName = ioDtTempSql.GetValue("CardName", i)
                            lsItemCode = ioDtTempSql.GetValue("ItemCode", i)
                            lsItemName = ioDtTempSql.GetValue("ItemName", i)
                            lsPurchaser = ioDtTempSql.GetValue("Purchaser", i)
                            lsBrand = ioDtTempSql.GetValue("Brand", i)
                            lsVendors = ioDtTempSql.GetValue("Vendors", i)
                            lsSaler = ioDtTempSql.GetValue("Saler", i)
                            Decimal.TryParse(ioDtTempSql.GetValue("Quantity", i), ldQuantity)
                            '  lsDeliDate = ioDtTempSql.GetValue("DeliDate", i)
                            Integer.TryParse(ioDtTempSql.GetValue("PurCircle", i), liPurCircle)
                            Decimal.TryParse(ioDtTempSql.GetValue("ExcDays", i), liExcDays)
                            ' lsPurDate = ioDtTempSql.GetValue("PurDate", i)
                            DateTime.TryParse(ioDtTempSql.GetValue("DeliDate", i), ldDeliDate)
                            DateTime.TryParse(ioDtTempSql.GetValue("PurDate", i), ldPurDate)

                            ioDbds_TI_Z0101.InsertRecord(ioDbds_TI_Z0101.Size)
                            ioDbds_TI_Z0101.Offset = ioDbds_TI_Z0101.Size - 1
                            ioDbds_TI_Z0101.SetValue("LineId", ioDbds_TI_Z0101.Offset, (i + 1).ToString)
                            'ioDbds_TI_Z0101.SetValue("U_Selected", ioDbds_TI_Z0101.Offset, "Y")
                            ioDbds_TI_Z0101.SetValue("U_BaseType", ioDbds_TI_Z0101.Offset, lsBaseType.Trim)
                            ioDbds_TI_Z0101.SetValue("U_BaseEntry", ioDbds_TI_Z0101.Offset, lsBaseEntry.Trim)
                            ioDbds_TI_Z0101.SetValue("U_BaseLine", ioDbds_TI_Z0101.Offset, lsBaseLine.Trim)
                            ioDbds_TI_Z0101.SetValue("U_CardCode", ioDbds_TI_Z0101.Offset, lsCardCode.Trim)
                            ioDbds_TI_Z0101.SetValue("U_CardName", ioDbds_TI_Z0101.Offset, lsCardName.Trim)
                            ioDbds_TI_Z0101.SetValue("U_Saler", ioDbds_TI_Z0101.Offset, lsSaler.Trim)
                            ioDbds_TI_Z0101.SetValue("U_ItemCode", ioDbds_TI_Z0101.Offset, lsItemCode.Trim)
                            ioDbds_TI_Z0101.SetValue("U_ItemName", ioDbds_TI_Z0101.Offset, lsItemName.Trim)
                            ioDbds_TI_Z0101.SetValue("U_Quantity", ioDbds_TI_Z0101.Offset, ldQuantity)
                            ioDbds_TI_Z0101.SetValue("U_OrderQty", ioDbds_TI_Z0101.Offset, ldQuantity)
                            ioDbds_TI_Z0101.SetValue("U_Vendors", ioDbds_TI_Z0101.Offset, lsVendors.Trim)
                            ioDbds_TI_Z0101.SetValue("U_Purchaser", ioDbds_TI_Z0101.Offset, lsPurchaser.Trim)
                            ioDbds_TI_Z0101.SetValue("U_Brand", ioDbds_TI_Z0101.Offset, lsBrand.Trim)
                            ioDbds_TI_Z0101.SetValue("U_DeliDate", ioDbds_TI_Z0101.Offset, ldDeliDate)
                            ioDbds_TI_Z0101.SetValue("U_PurCircle", ioDbds_TI_Z0101.Offset, liPurCircle)
                            ioDbds_TI_Z0101.SetValue("U_PurDate", ioDbds_TI_Z0101.Offset, ldPurDate)
                            ioDbds_TI_Z0101.SetValue("U_ExcDays", ioDbds_TI_Z0101.Offset, liExcDays)
                        Next
                    End If
                    ioMtx_10.LoadFromDataSource()
                End If
                If Not pVal.BeforeAction And pVal.ItemUID = "CreatePO" Then
                    '创建采购订单
                    Dim lsCardCode As String
                    lsCardCode = MyForm.DataSources.UserDataSources.Item("CardCode").Value
                    If String.IsNullOrEmpty(lsCardCode) Then
                        MyApplication.MessageBox("请先选择供应商代码!")
                        BubbleEvent = False
                        Return
                    End If

                    ioDbds_TI_Z0100 = MyForm.DataSources.DBDataSources.Item("@TI_Z0100")
                    ioDbds_TI_Z0101 = MyForm.DataSources.DBDataSources.Item("@TI_Z0101")
                    Try
                        Dim loActForm As Form
                        MyApplication.Menus.Item("2305").Activate()
                        Dim loMatrix As Matrix   '采购订单的matrix
                        loActForm = MyApplication.Forms.ActiveForm()
                        loMatrix = loActForm.Items.Item("38").Specific
                        loActForm.Items.Item("4").Specific.value = lsCardCode    '供应商代码
                        Dim lsSelected As String
                        Dim loSortlist As SortedList = New Collections.SortedList

                        Dim lsItemCode As String
                        Dim ldQuantity As Double
                        Dim ldAllQty As Double
                        For i As Integer = 0 To ioDbds_TI_Z0101.Size - 1
                            lsSelected = ioDbds_TI_Z0101.GetValue("U_Selected", i)
                            lsItemCode = ioDbds_TI_Z0101.GetValue("U_ItemCode", i)
                            Double.TryParse(ioDbds_TI_Z0101.GetValue("U_OrderQty", i), ldQuantity)
                            lsSelected = lsSelected.Trim
                            lsItemCode = lsItemCode.Trim
                            If lsSelected.Trim = "Y" Then
                                If loSortlist.ContainsKey(lsItemCode.Trim) Then
                                    ldAllQty = loSortlist.Item(lsItemCode)
                                    loSortlist.Item(lsItemCode) = ldAllQty + ldQuantity
                                Else
                                    loSortlist.Add(lsItemCode, ldQuantity)
                                End If
                            End If
                        Next
                        '将物料信息填充在采购订单上
                        Dim lsItemCodePO As String
                        Dim ldQuantityPO As Double
                        Dim PoRow As Integer
                        PoRow = 1
                        For i As Integer = 0 To loSortlist.Count - 1
                            lsItemCodePO = loSortlist.GetKey(i)
                            ldQuantityPO = loSortlist.Item(lsItemCodePO)
                            loMatrix.Columns.Item("1").Cells.Item(PoRow).Specific.Value = lsItemCodePO '描述
                            loMatrix.Columns.Item("11").Cells.Item(PoRow).Specific.Value = Convert.ToString(ldQuantityPO)  '数量                     
                            PoRow = PoRow + 1
                        Next
                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.ToString())
                    End Try                    
                End If
            Case BoEventTypes.et_FORM_RESIZE
                If Not pVal.Before_Action Then
                    '  FormRize()
                End If
            Case BoEventTypes.et_CHOOSE_FROM_LIST
                If Not pVal.Before_Action Then
                    Dim loCflE As SAPbouiCOM.ChooseFromListEvent = pVal
                    Dim lodt As SAPbouiCOM.DataTable = loCflE.SelectedObjects
                    If MyForm.Mode <> BoFormMode.fm_FIND_MODE Then
                        If Not lodt Is Nothing Then
                        Select pVal.ItemUID
                                Case "CardCode", "CardName"
                                    Dim lsCardCode, lsCardName As String
                                    lsCardCode = lodt.GetValue("CardCode", 0)
                                    lsCardName = lodt.GetValue("CardName", 0)
                                    MyForm.DataSources.UserDataSources.Item("CardCode").ValueEx = lsCardCode
                                    MyForm.DataSources.UserDataSources.Item("CardName").ValueEx = lsCardName
                                Case "12"
                                    Dim lsPurchaser As String
                                    lsPurchaser = lodt.GetValue("U_NAME", 0)
                                    '  MyForm.DataSources.UserDataSources.Item("U_NAME").ValueEx = lsPurchaser
                                    MyForm.DataSources.DBDataSources.Item("@TI_Z0100").SetValue("U_Purchaser", 0, lsPurchaser)
                            End Select
                        End If
                    End If
                End If

        End Select
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
                ioDbds_TI_Z0101.InsertRecord(ioDbds_TI_Z0101.Size)
                ioDbds_TI_Z0101.Offset = ioDbds_TI_Z0101.Size - 1
                ioMtx_10.AddRow(1, ioMtx_10.VisualRowCount)
            End If
            ioMtx_10.FlushToDataSource()
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.ToString())
        Finally
            ioMtxItem.AffectsFormMode = True
        End Try
    End Sub

    Private Sub TI_Z0100_MenuEvent(pVal As IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent
        Select Case pVal.MenuUID
            Case "TI_T012"
                If pVal.BeforeAction Then
                    MyForm.Freeze(True)
                    Try
                        ioFld1 = MyForm.Items.Item("3").Specific
                        ioFld2 = MyForm.Items.Item("4").Specific
                        ioFld1.Select()
                        ioDbds_TI_Z0100 = MyForm.DataSources.DBDataSources.Item("@TI_Z0100")
                        ioDbds_TI_Z0101 = MyForm.DataSources.DBDataSources.Item("@TI_Z0101")
                        ioDtTempSql = MyForm.DataSources.DataTables.Add("TempSQL")
                        ioMtx_10 = MyForm.Items.Item("13").Specific
                        ioMtxItem = MyForm.Items.Item("13")
                        ioUds_CardCode = MyForm.DataSources.UserDataSources.Item("CardCode")
                        ioUds_CardName = MyForm.DataSources.UserDataSources.Item("CardName")

                        '   FormRize()
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
                    '  SetValues()
                End If
        End Select
    End Sub

    Private Sub SetValues()
        '初始化明细表数据
        ioDbds_TI_Z0101.InsertRecord(ioDbds_TI_Z0101.Size)
        ioDbds_TI_Z0101.RemoveRecord(ioDbds_TI_Z0101.Size - 1)
        ioMtx_10.LoadFromDataSource()
    End Sub

    '界面的大小调整
    Public Sub FormRize()
        Dim loItem As Item
        loItem = MyForm.Items.Item("5")
        If Not loItem Is Nothing Then
            loItem.Width = ioMtxItem.Width + 15
            loItem.Height = ioMtxItem.Height + 13 '
        End If
    End Sub
End Class
