Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Text.RegularExpressions
Imports SAPbouiCOM.BoMessageTime
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.IO.Directory
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' 事件分发器
''' </summary>
''' <remarks></remarks>
Friend Class ItemDispacher
    Private Shared iothis As ItemDispacher

    Private WithEvents TI_SBO_Application As SAPbouiCOM.Application
    Public Shared TI_Company As SAPbobsCOM.Company
    Public Shared ioFormSL As Collections.SortedList = New Collections.SortedList
    Private ioZFct As Collections.SortedList = New Collections.SortedList


    Public Shared ioFormTag As SortedList = New SortedList() '存储标签
    Public Shared ioFormSon As SortedList = New SortedList() '存储子父窗体关系（存储方式为父窗体——子窗体）
    Public Shared is198FromID As String = ""
    Public Shared ioDatatable As SAPbouiCOM.DataTable = Nothing
    Public Shared iiCount As Integer = 1
    Public Shared ioListFromM As ArrayList = New ArrayList
    Public Shared isGshFormUID As String
    Public isBasePath As String = AppDomain.CurrentDomain.BaseDirectory
    Public ioTempSql As SAPbouiCOM.DataTable

    Private Shared Function GetWindowThreadProcessId(
    ByVal handle As Integer,
    <Out()> ByRef processId As Integer) As Integer
    End Function


    <STAThread()>
    Public Shared Sub Main()
        iothis = New ItemDispacher()
        System.Windows.Forms.Application.Run()
    End Sub

    ''' <summary>
    ''' 链接SBO应用程序
    ''' </summary>
    ''' <remarks></remarks>
    Private Function SetApplication() As Boolean
        Try
            Dim SboGuiApi As SAPbouiCOM.SboGuiApi
            Dim sConnectionString As String
            SboGuiApi = New SAPbouiCOM.SboGuiApi
            sConnectionString = Environment.GetCommandLineArgs.GetValue(1)
            SboGuiApi.Connect(sConnectionString)
            TI_SBO_Application = SboGuiApi.GetApplication()
            Return True
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("不能连接SBO应用程序！")
            Return False
        End Try
    End Function

    '应用程序启动
    Public Sub New()
        MyBase.New()
        '先链接SBO应用程序
        'TI_SBO_Application.StatusBar.SetText("正在连接应用程序!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
        If SetApplication() Then
            '2014-12-08 修改设置事件过滤
            '  BaseFunction.SetFilter(TI_SBO_Application)

            '链接DI
            'If BaseFunction.GFuc_DI_ContextSet(TI_SBO_Application, TI_Company) Then
            '    BaseFunction.GSub_DI_Connect(TI_SBO_Application, TI_Company)
            'Else
            '    Return
            'End If
            '添加菜单
            BaseFunction.MenuAdd(TI_SBO_Application)

            '2014-03-02在198窗体上添加DataTable
            is198FromID = ""
            Dim loForm As Form = TI_SBO_Application.Forms.GetForm("198", 1)
            If Not loForm Is Nothing Then
                is198FromID = loForm.UniqueID
                Try
                    ioDatatable = loForm.DataSources.DataTables.Add("STI_TempDt")
                Catch ex As Exception
                    ioDatatable = loForm.DataSources.DataTables.Item("STI_TempDt")
                End Try
            End If

            '取出用户ID
            Dim lsSql As String
            lsSql = "Select top 1 t10.USERID  From OUSR t10 where t10.USER_CODE='" + TI_SBO_Application.Company.UserName + "'"
            ioDatatable.ExecuteQuery(lsSql)
            BaseFunction.myUserId = ioDatatable.GetValue(0, 0)

            '初始化小数点
            Dim lsFromPath, lsToPath As String
            lsSql = " Select top 1 t10.SumDec,t10.PriceDec,t10.RateDec,t10.QtyDec,t10.PercentDec,t10.MeasureDec,t10.FreeZoneNo from OADM t10"
            ioDatatable.ExecuteQuery(lsSql)
            BaseFunction.iiSumDec = ioDatatable.GetValue("SumDec", 0)
            BaseFunction.iiPriceDec = ioDatatable.GetValue("PriceDec", 0)
            BaseFunction.iiRateDec = ioDatatable.GetValue("RateDec", 0)
            BaseFunction.iiQtyDec = ioDatatable.GetValue("QtyDec", 0)
            BaseFunction.iiPercentDec = ioDatatable.GetValue("PercentDec", 0)
            BaseFunction.iiMeasureDec = ioDatatable.GetValue("MeasureDec", 0)
            lsFromPath = ioDatatable.GetValue("FreeZoneNo", 0)
            If Not String.IsNullOrEmpty(lsFromPath) Then
                lsFromPath = lsFromPath.Trim
            End If

            'TI_SBO_Application.StatusBar.SetText("正在连接应用程序...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
            'TI_Company = TI_SBO_Application.Company.GetDICompany()

            '加载权限XML
            BaseFunction.GetQXXMLDate()
            BaseFunction.GetFromMenuList()
            TI_SBO_Application.StatusBar.SetText("连接应用程序成功!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
        End If
    End Sub

    Private Sub TI_SBO_Application_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles TI_SBO_Application.AppEvent
        Try
            Select Case EventType
                Case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged
                    System.Environment.Exit(0)
                Case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged
                    BaseFunction.MenuAdd(TI_SBO_Application)
                Case SAPbouiCOM.BoAppEventTypes.aet_ShutDown
                    System.Environment.Exit(0)
            End Select
        Catch ex As Exception

        End Try
    End Sub

    Private Sub TI_SBO_Application_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles TI_SBO_Application.FormDataEvent
        Dim loobj As FormBase
        Try
            If Not ioFormSL.ContainsKey(BusinessObjectInfo.FormUID) Then
                If BusinessObjectInfo.BeforeAction Then
                    Select Case BusinessObjectInfo.FormTypeEx
                           '如果是交货单，打开加载导出送货单的EXCEL
                        Case "140"
                            loobj = New TI_Z0001
                        Case "142"  '收货单草稿
                            loobj = New TI_Z0002
                        Case "141"  '应付发票草稿
                            loobj = New TI_Z0005
                        Case "134"  '业务伙伴
                            loobj = New TI_Z0004
                        Case "720"  '库存下发货单
                            loobj = New TI_Z0003
                        Case "180"  '销售退货单
                            loobj = New TI_Z0007
                        Case "940"  '库存转储单
                            loobj = New TI_Z0006
                        Case "139"  '销售订单
                            loobj = New TI_Z000A
                        Case "TI_Z0012"  '客户物料单位
                            loobj = New TI_Z0012
                        Case "TI_Z0081"  '货权转移界面
                            loobj = New TI_Z0081
                        Case "TI_Z0100"  'MRP计算界面
                            loobj = New TI_Z0100
                        Case "149"  'MRP计算界面
                            loobj = New TI_Z0091
                        Case "143"  '收货采购订单草稿界面
                            loobj = New TI_Z000D
                        Case Else
                            loobj = Nothing
                    End Select
                    '如果没有，添加类
                    If Not loobj Is Nothing Then
                        ioFormSL.Add(BusinessObjectInfo.FormUID, loobj)
                    End If
                Else
                    Select Case BusinessObjectInfo.FormTypeEx
                        Case "139"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "140"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "940"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "141", "60092"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "143", "142"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "180"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "134"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "720"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "139"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "TI_Z0012"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "TI_Z0081"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "TI_Z0100"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "149"
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case "143"  '收货采购订单草稿界面
                            loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                        Case Else
                            loobj = Nothing
                    End Select
                End If
            Else
                Select Case BusinessObjectInfo.FormTypeEx
                    Case "139"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "140"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "940"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "141", "60092"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "143", "142"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "180"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "134"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "720"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "139"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "TI_Z0012"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "TI_Z0081"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "TI_Z0100"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "149"
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case "143"  '收货采购订单草稿界面
                        loobj = CType(ioFormSL.Item(BusinessObjectInfo.FormUID), FormBase)
                    Case Else
                        loobj = Nothing
                End Select
            End If

            If Not loobj Is Nothing Then
                Dim Myform As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(BusinessObjectInfo.FormUID)
                loobj.MyForm = Myform
                loobj.MyApplication = TI_SBO_Application
                loobj.MyCompany = TI_Company
                loobj.ls198FromID = is198FromID
                loobj.myDataTable = ioDatatable
                If ioFormTag.ContainsKey(BusinessObjectInfo.FormUID) Then
                    loobj.Tag = ioFormTag.Item(BusinessObjectInfo.FormUID)
                End If

                If BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_UPDATE Then
                    If BusinessObjectInfo.BeforeAction Then
                        Dim lsTableName As String
                        lsTableName = loobj.MyTableName
                        If Not String.IsNullOrEmpty(lsTableName) Then
                            Try
                                Dim loDbds As DBDataSource = Myform.DataSources.DBDataSources.Item(lsTableName)
                                Dim lsDocStatus As String
                                Try
                                    lsDocStatus = loDbds.GetValue("DocStatus", 0)
                                Catch ex As Exception
                                    lsDocStatus = loDbds.GetValue("Status", 0)
                                End Try
                                If Trim(lsDocStatus) <> "C" Then
                                    Dim loEdt As EditText = Myform.Items.Item("TI_CApp").Specific
                                    loEdt.Value = loobj.MyMtext
                                End If
                            Catch ex As Exception
                                TI_SBO_Application.SetStatusBarMessage(ex.ToString)
                            End Try
                        End If
                    End If
                End If
                loobj.HandleFormDataEvent(BusinessObjectInfo, BubbleEvent)
            End If
        Catch ex As Exception
            TI_SBO_Application.SetStatusBarMessage(ex.ToString)
        End Try
    End Sub

    Private Sub TI_SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles TI_SBO_Application.ItemEvent
        Try
            '2014-10-13 添加复制功能
            Dim lsTableName, lsMText As String
            If ioListFromM.Contains(pVal.FormTypeEx) Then
                If (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD) And (Not pVal.Before_Action) Then
                    Dim loForm As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(FormUID)
                    If Not loForm Is Nothing Then
                        Dim lsObjType, lsSqlObj As String
                        Dim loMenus As SAPbouiCOM.Menus
                        loMenus = loForm.Menu
                        If Not loForm.Menu Is Nothing Then
                            'loMenus.Add("TI_Copy", "复制从Excel剪切板", SAPbouiCOM.BoMenuType.mt_STRING, 0)
                            Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
                            oCreationPackage = TI_SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
                            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                            oCreationPackage.UniqueID = "TI_Copy" + Convert.ToString(iiCount)
                            oCreationPackage.String = "复制从Excel剪切板"
                            loForm.Menu.AddEx(oCreationPackage)
                            iiCount += 1
                        End If

                    End If
                End If
            End If
            Dim loobj As FormBase = Nothing
            '之前就判断是否存在有子窗体
            If ioFormSon.ContainsKey(FormUID) Then
                BubbleEvent = False
                Dim loFormSon As Form
                loFormSon = TI_SBO_Application.Forms.Item(ioFormSon.Item(FormUID))
                If loFormSon.Visible Then
                    '焦点设置在子窗体上
                    loFormSon.Select()
                    Return
                End If
            End If

            If Not ioFormSL.ContainsKey(FormUID) Then
                If pVal.BeforeAction Then
                    '2014-08-18 修改添加仓库到物料
                    If (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_LOAD) Then
                        BaseFunction.SetMenus(TI_SBO_Application, pVal.FormUID)
                    End If
                    Select Case pVal.FormTypeEx
                        Case "TI_Z0010"
                            loobj = New TI_Z0010
                        Case "140"
                            loobj = New TI_Z0001
                        Case "142"
                            loobj = New TI_Z0002
                        Case "141"  '应付发票草稿
                            loobj = New TI_Z0005
                        Case "134"  '业务伙伴
                            loobj = New TI_Z0004
                        Case "720"  '库存下发货单
                            loobj = New TI_Z0003
                        Case "180"  '销售退货单
                            loobj = New TI_Z0007
                        Case "940"  '库存转储单
                            loobj = New TI_Z0006
                        Case "182"  '采购退货单
                            loobj = New TI_Z0008
                        Case "721"  '库存下收货单草稿
                            loobj = New TI_Z0009
                        Case "139"  '销售订单
                            loobj = New TI_Z000A
                        Case "TI_Z000B"  '销售订单
                            loobj = New TI_Z000B
                        Case "TI_Z000C"  '销售订单
                            loobj = New TI_Z000C
                        Case "42"  '批次选择界面
                            loobj = New TI_Z0011
                        Case "TI_Z0012"  '业务伙伴物料单位
                            loobj = New TI_Z0012
                        Case "TI_Z0081"  '货权转移界面
                            loobj = New TI_Z0081
                        Case "149"  '
                            loobj = New TI_Z0091
                        Case "TI_Z0150"  '
                            loobj = New TI_Z0150
                        Case "TI_Z0151"  '
                            loobj = New TI_Z0151
                        Case "TI_Z00071"  '
                            loobj = New TI_Z00071
                        Case "143"  '
                            loobj = New TI_Z000D
                        Case "TI_Z0100"  '
                            loobj = New TI_Z0100
                        Case "TI_Z0101"  '
                            loobj = New TI_Z0101
                        Case Else
                            loobj = Nothing
                    End Select
                End If

                '如果没有，添加类
                If Not loobj Is Nothing Then
                    ioFormSL.Add(FormUID, loobj)
                End If
            Else
                Select Case pVal.FormTypeEx
                    Case "140", "TI_Z0010", "142", "141", "134", "180", "940", "720", "182", "721", "139", "TI_Z000B", "TI_Z000C", "42", "TI_Z0012", "TI_Z0081", "149"， "TI_Z0151"， "TI_Z0150", "TI_Z00071", "143", "TI_Z0100", "TI_Z0101"
                        loobj = CType(ioFormSL.Item(pVal.FormUID), FormBase)
                    Case Else
                        loobj = Nothing
                End Select
            End If

            If Not loobj Is Nothing Then
                If (pVal.EventType = BoEventTypes.et_FORM_UNLOAD And Not pVal.BeforeAction) Then
                    FormRemoveGX(FormUID)
                Else
                    Dim Myform As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(FormUID)
                    loobj.MyForm = Myform
                    loobj.MyApplication = TI_SBO_Application
                    loobj.MyCompany = TI_Company
                    loobj.ls198FromID = is198FromID
                    loobj.myDataTable = ioDatatable
                    'If Not String.IsNullOrEmpty(lsTableName) Then
                    '    If String.IsNullOrEmpty(loobj.MyTableName) Then
                    '        loobj.MyTableName = lsTableName
                    '        loobj.MyMtext = lsMText
                    '    End If
                    'End If
                    If ioFormTag.ContainsKey(FormUID) Then
                        loobj.Tag = ioFormTag.Item(FormUID)
                    End If
                    loobj.HandleItemEvent(FormUID, pVal, BubbleEvent)
                End If
            End If
            '如果是交货单，点击导出交货单，触发事件
            Select Case pVal.EventType
                Case BoEventTypes.et_FORM_LOAD

                Case BoEventTypes.et_ITEM_PRESSED
                    Dim loActForm As Form
                    loActForm = TI_SBO_Application.Forms.ActiveForm()
            End Select

        Catch ex As Exception
            TI_SBO_Application.SetStatusBarMessage(ex.ToString())
            BubbleEvent = False
        End Try
    End Sub

    ''' <summary>
    ''' 移除对应关系
    ''' </summary>
    ''' <param name="FormUID"></param>
    ''' <remarks></remarks>
    Public Shared Sub FormRemoveGX(ByVal FormUID As String)
        If ioFormSL.ContainsKey(FormUID) Then
            If ioFormSL.ContainsKey(FormUID) Then
                ioFormSL.Remove(FormUID)
            End If

            If ioFormTag.ContainsKey(FormUID) Then
                ioFormTag.Remove(FormUID)
            End If

            '移除子父窗体关系（按key）
            If ioFormSon.ContainsKey(FormUID) Then
                ioFormSon.Remove(FormUID)
            End If

            '移除子父窗体关系（按value）
            If ioFormSon.ContainsValue(FormUID) Then
                Dim liIndex As Integer
                While True
                    If ioFormSon.ContainsValue(FormUID) Then
                        liIndex = ioFormSon.IndexOfValue(FormUID)
                        ioFormSon.RemoveAt(liIndex)
                    Else
                        Exit While
                    End If
                End While
            End If
        End If
    End Sub



    Private Sub ClipBoard(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean, ByVal MatrixUID As String)
        Dim lsMenuUid As String
        If pVal.MenuUID.Length > 7 Then
            lsMenuUid = Left(pVal.MenuUID, 7)
        Else
            Return
        End If
        If lsMenuUid = "TI_Copy" Then
            Dim sClipValue As String
            Dim WL_oForm As SAPbouiCOM.Form
            Dim oMatrix As SAPbouiCOM.Matrix
            Dim iCurrRow As Integer
            Dim ii As Integer
            Try
                TI_SBO_Application.SetStatusBarMessage("系统正在导入……！", bmt_Short, False)

                WL_oForm = TI_SBO_Application.Forms.ActiveForm
                'WL_oForm.Items.Item("16").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                oMatrix = WL_oForm.Items.Item(MatrixUID).Specific
                ' 获取当前网格行
                iCurrRow = oMatrix.VisualRowCount

                If Open_Clipboard(IntPtr.Zero) Then
                    ' 数据检测　
                    If CheckData(oMatrix) = False Then
                        Exit Sub
                    End If

                    Dim iClipData As IntPtr = Get_ClipboardData(13)
                    sClipValue = Marshal.PtrToStringUni(iClipData)
                    Close_Clipboard()

                    Dim sValueArray() As String = Split(sClipValue, Chr(13) + Chr(10))
                    Dim THashTable As New Collections.Hashtable

                    ' 把获取的数据放进哈希表中
                    For i As Integer = 0 To sValueArray.Length - 1
                        If sValueArray(i) <> "" Then
                            THashTable.Add(i, sValueArray(i).Split(Chr(9)))
                        End If
                    Next

                    WL_oForm.Freeze(True)
                    For i As Integer = 1 To THashTable.Count - 1


                        For ii = 0 To THashTable.Item(0).length - 1
                            Try
                                Select Case oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Type
                                    Case SAPbouiCOM.BoFormItemTypes.it_EDIT
                                        oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Cells.Item(iCurrRow).Specific.string =
                                                    THashTable.Item(i)(ii).ToString
                                    Case SAPbouiCOM.BoFormItemTypes.it_LINKED_BUTTON
                                        oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Cells.Item(iCurrRow).Specific.string =
                                                   THashTable.Item(i)(ii).ToString
                                    Case SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX
                                        oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Cells.Item(iCurrRow).Specific.Select(THashTable.Item(i)(ii).ToString)
                                End Select
                            Catch ex As Exception
                                ' 在物料编号出错的时候，在原行操作
                                If THashTable.Item(0)(ii).ToString = "1" Then
                                    iCurrRow = iCurrRow - 1
                                ElseIf THashTable.Item(0)(ii).ToString = "2" Then
                                    iCurrRow = iCurrRow - 1
                                    ii = 100
                                End If
                                ' 记录日志
                                'TI_SBO_Application.SetStatusBarMessage(ex.ToString)
                                'WL_oForm.Freeze(False)
                            End Try
                        Next
                        iCurrRow = iCurrRow + 1
                    Next
                    WL_oForm.Freeze(False)
                    WL_oForm.Update()

                    BubbleEvent = False
                    TI_SBO_Application.SetStatusBarMessage("导入完毕……！", bmt_Short, False)
                    Exit Sub
                End If
            Catch ex As Exception
                WL_oForm.Freeze(False)
                TI_SBO_Application.SetStatusBarMessage(ex.Message)
            End Try
        End If
    End Sub


    Private Function CheckData(ByVal oMatrix As SAPbouiCOM.Matrix) As Boolean
        CheckData = True
    End Function

    Private Sub ClipBoardForPosting(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean, ByVal MatrixUID As String)
        Dim lsMenuUid As String
        If pVal.MenuUID.Length > 7 Then
            lsMenuUid = Left(pVal.MenuUID, 7)
        Else
            Return
        End If
        If lsMenuUid = "TI_Copy" Then
            Dim sClipValue As String
            Dim WL_oForm As SAPbouiCOM.Form
            Dim oMatrix As SAPbouiCOM.Matrix
            Dim iCurrRow As Integer
            Dim ii As Integer

            Try

                TI_SBO_Application.SetStatusBarMessage("系统正在导入……！", bmt_Short, False)

                WL_oForm = TI_SBO_Application.Forms.ActiveForm
                'WL_oForm.Items.Item("16").Click(SAPbouiCOM.BoCellClickType.ct_Regular)

                oMatrix = WL_oForm.Items.Item(MatrixUID).Specific
                ' 获取当前网格行
                iCurrRow = oMatrix.VisualRowCount

                If Open_Clipboard(IntPtr.Zero) Then
                    ' 数据检测　
                    'If CheckData(oMatrix) = False Then
                    '    Exit Sub
                    'End If

                    Dim iClipData As IntPtr = Get_ClipboardData(13)
                    sClipValue = Marshal.PtrToStringUni(iClipData)
                    Close_Clipboard()

                    Dim sValueArray() As String = Split(sClipValue, Chr(13) + Chr(10))
                    Dim THashTable As New Collections.Hashtable

                    ' 把获取的数据放进哈希表中
                    For i As Integer = 0 To sValueArray.Length - 1
                        If sValueArray(i) <> "" Then
                            THashTable.Add(i, sValueArray(i).Split(Chr(9)))
                        End If
                    Next

                    WL_oForm.Freeze(True)
                    For i As Integer = 1 To THashTable.Count - 1
                        ' If THashTable.Item(i)(4).ToString = "Y" Then


                        For ii = 0 To THashTable.Item(0).length - 1
                            Try

                                Select Case oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Type
                                    Case SAPbouiCOM.BoFormItemTypes.it_CHECK_BOX
                                        ' oMatrix.Columns.Item("4").Cells.Item(CInt(THashTable.Item(i)(0).ToString)).Specific.checked = True
                                    Case SAPbouiCOM.BoFormItemTypes.it_LINKED_BUTTON
                                        oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Cells.Item(CInt(THashTable.Item(i)(0).ToString)).Specific.string =
                                                   THashTable.Item(i)(ii).ToString
                                    Case SAPbouiCOM.BoFormItemTypes.it_EDIT
                                        oMatrix.Columns.Item(THashTable.Item(0)(ii).ToString).Cells.Item(CInt(THashTable.Item(i)(0).ToString)).Specific.string =
                                                   THashTable.Item(i)(ii).ToString
                                End Select
                            Catch ex As Exception
                                'TI_SBO_Application.SetStatusBarMessage(ex.ToString)
                                'WL_oForm.Freeze(False)
                            End Try
                        Next
                        ' End If

                    Next
                    BubbleEvent = False
                    WL_oForm.Freeze(False)
                    WL_oForm.Update()
                    TI_SBO_Application.SetStatusBarMessage("导入完毕……！", BoMessageTime.bmt_Short, False)
                    Exit Sub
                End If
                WL_oForm.Freeze(False)

            Catch ex As Exception
                WL_oForm.Freeze(False)
                TI_SBO_Application.SetStatusBarMessage(ex.Message)
            End Try
        End If
    End Sub

    Private Sub TI_SBO_Application_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles TI_SBO_Application.MenuEvent
        Try
            '主菜单，就不处理
            If pVal.MenuUID = "1030" Then
                Return
            End If
            Dim loForm As Form
            Dim lsMenuUid As String

            loForm = TI_SBO_Application.Forms.ActiveForm()
            If Not loForm Is Nothing Then
                If ioListFromM.Contains(loForm.TypeEx) Then
                    If pVal.MenuUID.Length > 7 Then
                        lsMenuUid = Left(pVal.MenuUID, 7)
                        Select Case lsMenuUid
                            Case "TI_Copy"
                                If pVal.BeforeAction = True Then
                                    '销售模块：销售订单、销售报价单、销售交货、销售退货、预收款请求、预收款发票、应收发票、
                                    '         应收发票+请求、应收贷项凭证、应收预留发票
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 139 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 149 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 140 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 180 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 65308 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 65300 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 133 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 60090 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 179 _
                                            Or TI_SBO_Application.Forms.ActiveForm.Type = 60091 Then
                                        Call ClipBoard(pVal, BubbleEvent, "38")      '物料类型
                                        Call ClipBoard(pVal, BubbleEvent, "39")      '服务类型
                                    End If

                                    '采购模块：采购订单、采购收货、采购退货、预付款请求、预付款发票、应付发票、
                                    '         应付贷项凭证、应付预留发票
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 142 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 143 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 182 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 65309 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 65301 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 141 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 181 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 60092 Then
                                        Call ClipBoard(pVal, BubbleEvent, "38")
                                        Call ClipBoard(pVal, BubbleEvent, "39")
                                    End If


                                    '库存转储
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 940 Or TI_SBO_Application.Forms.ActiveForm.Type = 393 Then
                                        Call ClipBoard(pVal, BubbleEvent, "23")
                                    End If

                                    '日记帐分录
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 392 Or TI_SBO_Application.Forms.ActiveForm.Type = 393 Then
                                        Call ClipBoard(pVal, BubbleEvent, "76")
                                    End If

                                    '库存重估
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 70001 Then
                                        Call ClipBoard(pVal, BubbleEvent, "41")
                                    End If


                                    '业务伙伴目录编号
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 993 Then
                                        Call ClipBoard(pVal, BubbleEvent, "17")
                                    End If

                                    '库存盘点
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 840 Then   '库存跟踪
                                        Call ClipBoardForPosting(pVal, BubbleEvent, "3")
                                    ElseIf TI_SBO_Application.Forms.ActiveForm.Type = 906 Then   '库存初始数量
                                        Call ClipBoardForPosting(pVal, BubbleEvent, "9")
                                    ElseIf TI_SBO_Application.Forms.ActiveForm.Type = 157 Then    '价格清单
                                        Call ClipBoardForPosting(pVal, BubbleEvent, "3")
                                    End If

                                    'YOYO于2008-08-19添加以下功能：
                                    'MRP预测
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 65201 Then
                                        Call ClipBoard(pVal, BubbleEvent, "11")
                                    End If

                                    'BOM复制
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 672 Then
                                        Call ClipBoard(pVal, BubbleEvent, "3")
                                    End If

                                    '库存收、发货，生产收、发货
                                    If TI_SBO_Application.Forms.ActiveForm.Type = 721 Or TI_SBO_Application.Forms.ActiveForm.Type = 720 _
                                        Or TI_SBO_Application.Forms.ActiveForm.Type = 65214 Or TI_SBO_Application.Forms.ActiveForm.Type = 65213 Then
                                        Call ClipBoard(pVal, BubbleEvent, "13")
                                    End If

                                End If
                        End Select
                    End If
                End If
            End If
            Dim loobj As FormBase
            If pVal.BeforeAction Then
                Select Case pVal.MenuUID
                    Case "TI_T004" '基础设置
                        loForm = BaseFunction.londFromXml("TI_Z0060", TI_SBO_Application)
                    Case "TI_T005" '审批阶段
                        loForm = BaseFunction.londFromXml("TI_Z0040", TI_SBO_Application)
                    Case "TI_T010" '打印模板配置
                        loForm = BaseFunction.londFromXml("TI_Z0010", TI_SBO_Application)
                    Case "TI_T011" '货权转移界面
                        loForm = BaseFunction.londFromXml("TI_Z0081", TI_SBO_Application)
                    Case "TI_T058" '销售交货向导
                        If (TI_Company Is Nothing) Then
                            TI_SBO_Application.StatusBar.SetText("正在连接SAPDIAPI程序...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
                            TI_Company = TI_SBO_Application.Company.GetDICompany()
                            TI_SBO_Application.StatusBar.SetText("连接SAPDIAPI程序成功!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                        End If
                        loForm = BaseFunction.londFromXml("TI_Z0151", TI_SBO_Application)
                    Case "TI_T059" '销售价格调整界面
                        loForm = BaseFunction.londFromXml("TI_Z0100", TI_SBO_Application)
                    Case Else
                        If Not TI_SBO_Application.Forms.ActiveForm() Is Nothing Then
                            loForm = TI_SBO_Application.Forms.ActiveForm()
                        Else
                            Return
                        End If

                End Select
            Else
                If Not TI_SBO_Application.Forms.ActiveForm() Is Nothing Then
                    loForm = TI_SBO_Application.Forms.ActiveForm()
                Else
                    Return
                End If
                Select Case pVal.MenuUID
                    Case Else
                        If Not TI_SBO_Application.Forms.ActiveForm() Is Nothing Then
                            loForm = TI_SBO_Application.Forms.ActiveForm()
                        Else
                            Return
                        End If

                End Select
            End If

            If Not TI_SBO_Application.Forms.ActiveForm() Is Nothing Then
                loForm = TI_SBO_Application.Forms.ActiveForm()
            Else
                Return
            End If

            If loForm Is Nothing Then
                Return
            End If

            Dim FormUID As String
            FormUID = loForm.UniqueID
            If Not ioFormSL.ContainsKey(FormUID) Then
                If pVal.BeforeAction Then
                    Select Case loForm.TypeEx
                        Case "TI_Z0010"
                            loobj = New TI_Z0010
                        Case "TI_Z0081"
                            loobj = New TI_Z0081
                        Case "134"
                            loobj = New TI_Z0004
                        Case "TI_Z0100"
                            loobj = New TI_Z0100
                        Case "TI_Z0150"
                            loobj = New TI_Z0150
                        Case "TI_Z0151"
                            loobj = New TI_Z0151
                        Case Else
                            loobj = Nothing
                    End Select
                End If
                '如果没有，添加类
                If Not loobj Is Nothing Then
                    ioFormSL.Add(FormUID, loobj)
                End If
            Else
                Select Case loForm.TypeEx
                    Case "TI_Z0010"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "134"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0081"
                        loobj = New TI_Z0081
                    Case "TI_Z0100"
                        loobj = New TI_Z0100
                    Case "TI_Z0150"
                        loobj = New TI_Z0150
                    Case "TI_Z0151"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case Else
                        loobj = Nothing
                End Select
            End If

            If Not loobj Is Nothing Then
                Dim Myform As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(FormUID)
                loobj.MyForm = Myform
                loobj.MyApplication = TI_SBO_Application
                loobj.MyCompany = TI_Company
                loobj.ls198FromID = is198FromID
                loobj.myDataTable = ioDatatable
                If ioFormTag.ContainsKey(FormUID) Then
                    loobj.Tag = ioFormTag.Item(FormUID)
                End If
                loobj.HandleMenuEvent(pVal, BubbleEvent)
            End If

        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show(ex.ToString())
        End Try
    End Sub

    Private Sub TI_SBO_Application_PrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean) Handles TI_SBO_Application.PrintEvent
        Try
            If Not BubbleEvent Then
                Return
            End If
            Dim loobj As FormBase
            Dim loForm As Form
            loForm = TI_SBO_Application.Forms.Item(eventInfo.FormUID)
            Dim FormUID As String
            FormUID = loForm.UniqueID
            If Not ioFormSL.ContainsKey(FormUID) Then
                If eventInfo.BeforeAction Then
                    Select Case loForm.TypeEx
                        Case "TI_Z0120"
                            loobj = New TI_Z0010
                        Case "TI_Z0140"
                            loobj = New TI_Z0140
                        Case Else
                            loobj = Nothing
                    End Select
                End If
                '如果没有，添加类
                If Not loobj Is Nothing Then
                    ioFormSL.Add(FormUID, loobj)
                End If
            Else
                Select Case loForm.TypeEx
                    Case "TI_Z0010"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0020"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "41"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "42"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "140"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "940"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "141", "60092"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "143"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "181"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "720"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "182"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "721"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "139"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0100"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case Else
                        loobj = Nothing
                End Select
            End If

            If Not loobj Is Nothing Then
                Dim Myform As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(FormUID)
                loobj.MyForm = Myform
                loobj.MyApplication = TI_SBO_Application
                loobj.MyCompany = TI_Company
                loobj.ls198FromID = is198FromID
                loobj.myDataTable = ioDatatable
                If ioFormTag.ContainsKey(FormUID) Then
                    loobj.Tag = ioFormTag.Item(FormUID)
                End If
                loobj.HandlePrintEvent(eventInfo, BubbleEvent)
            End If

        Catch ex As Exception
            TI_SBO_Application.MessageBox(ex.ToString())
        End Try
    End Sub

    Private Sub TI_SBO_Application_RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean) Handles TI_SBO_Application.RightClickEvent
        Try
            Dim loobj As FormBase
            Dim loForm As Form
            loForm = TI_SBO_Application.Forms.Item(eventInfo.FormUID)
            Dim FormUID As String
            FormUID = loForm.UniqueID
            If Not ioFormSL.ContainsKey(FormUID) Then
                If eventInfo.BeforeAction Then
                    Select Case loForm.TypeEx
                        Case "TI_Z0010"
                            loobj = New TI_Z0010
                        Case "TI_Z0140"
                            loobj = New TI_Z0140
                        Case "134"
                            loobj = New TI_Z0004
                        Case "TI_Z0151"
                            loobj = New TI_Z0151
                        Case "TI_Z0150"
                            loobj = New TI_Z0150
                        Case Else
                            loobj = Nothing
                    End Select
                End If
                '如果没有，添加类
                If Not loobj Is Nothing Then
                    ioFormSL.Add(FormUID, loobj)
                End If
            Else
                Select Case loForm.TypeEx
                    Case "TI_Z0010"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0020"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "41"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "42"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "139"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "140"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "940"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "141", "60092"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "143", "142"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "720"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "182"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "721"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "139"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "134"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "149"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0150"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0151"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case "TI_Z0400"
                        loobj = CType(ioFormSL.Item(FormUID), FormBase)
                    Case Else
                        loobj = Nothing
                End Select
            End If

            If Not loobj Is Nothing Then
                Dim Myform As SAPbouiCOM.Form = TI_SBO_Application.Forms.Item(FormUID)
                loobj.MyForm = Myform
                loobj.MyApplication = TI_SBO_Application
                loobj.MyCompany = TI_Company
                loobj.ls198FromID = is198FromID
                loobj.myDataTable = ioDatatable
                If ioFormTag.ContainsKey(FormUID) Then
                    loobj.Tag = ioFormTag.Item(FormUID)
                End If
                loobj.HandleRightClickEvent(eventInfo, BubbleEvent)
            End If

        Catch ex As Exception
            TI_SBO_Application.MessageBox(ex.ToString())
        End Try
    End Sub
End Class
