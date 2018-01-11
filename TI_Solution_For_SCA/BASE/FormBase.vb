Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports SAPbobsCOM
Imports System.Runtime.InteropServices

Public Class FormBase
    Protected Friend MyForm As SAPbouiCOM.Form
    Protected Friend MyApplication As SAPbouiCOM.Application
    Protected Friend MyCompany As SAPbobsCOM.Company
    Protected Friend Tag As Object()
    Protected Friend ls198FromID As String
    Protected Friend myDataTable As SAPbouiCOM.DataTable
    Protected Friend isPermission As String
    Protected Friend MyTableName As String
    Protected Friend MyMtext As String

    Public Event ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
    Public Event MenuEvent(ByVal pVal As SAPbouiCOM.IMenuEvent, ByRef BubbleEvent As Boolean)
    Public Event FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
    Public Event RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean)
    Public Event PrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)

    Protected Friend Sub HandleItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        RaiseEvent ItemEvent(FormUID, pVal, BubbleEvent)
    End Sub

    Protected Friend Sub HandleMenuEvent(ByVal pVal As SAPbouiCOM.IMenuEvent, ByRef BubbleEvent As Boolean)
        RaiseEvent MenuEvent(pVal, BubbleEvent)
    End Sub

    Protected Friend Sub HandleFormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        RaiseEvent FormDataEvent(BusinessObjectInfo, BubbleEvent)
    End Sub

    Protected Friend Sub HandleRightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean)
        RaiseEvent RightClickEvent(eventInfo, BubbleEvent)
    End Sub


    Protected Friend Sub HandlePrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)
        RaiseEvent PrintEvent(eventInfo, BubbleEvent)
    End Sub

    ''' <summary>
    ''' 清除字符转中的空格
    ''' </summary>
    ''' <param name="loObj">需要清除的字符串</param>
    ''' <returns>已清除的字符串</returns>
    Protected Friend Function TrimStr(ByVal loObj As Object) As String
        Dim lsTrimStr As String
        If loObj Is Nothing Then
            lsTrimStr = String.Empty
        Else
            If TypeOf (loObj) Is Date Then
                Dim ldDate As Date = loObj
                lsTrimStr = ldDate.ToString("yyyyMMdd")
            ElseIf TypeOf (loObj) Is String Then
                Dim lsStr As String = loObj
                If Not String.IsNullOrEmpty(lsStr) Then
                    lsTrimStr = lsStr.Trim()
                Else
                    lsTrimStr = lsStr
                End If
            Else
                lsTrimStr = loObj.ToString()
            End If
        End If

        Return lsTrimStr
    End Function

    ''' <summary>
    ''' 根据Item UniqueId获取Item对象
    ''' </summary>
    ''' <param name="lsItemUid">Item UniqueId</param>
    ''' <returns>Item 对象</returns>
    Protected Friend Function GetItemSpecific(ByVal lsItemUid As String) As Object
        Return GetItemSpecific(lsItemUid, MyForm)
    End Function

    ''' <summary>
    ''' 根据Item UniqueId获取Item对象
    ''' </summary>
    ''' <param name="lsItemUid">Item UniqueId</param>
    ''' <param name="loForm">Item所在Form</param>
    ''' <returns>Item 对象</returns>
    Protected Friend Function GetItemSpecific(ByVal lsItemUid As String, ByVal loForm As Form) As Object
        Return loForm.Items.Item(lsItemUid).Specific
    End Function

    ''' <summary>
    ''' 根据DbDataSource UniqueId获取DbDataSource 对象
    ''' </summary>
    ''' <param name="lsDbDataSourceUid">DbDataSource UniqueId</param>
    ''' <param name="loForm">DbDataSource 所在的窗口</param>
    ''' <returns>DbDataSource 对象</returns>
    Protected Friend Function GetDbDataSource(ByVal lsDbDataSourceUid As String, ByVal loForm As Form) As DBDataSource
        Dim loDbDataSource As DBDataSource
        Try
            loDbDataSource = loForm.DataSources.DBDataSources.Item(lsDbDataSourceUid)
        Catch ex As Exception
            loDbDataSource = loForm.DataSources.DBDataSources.Add(lsDbDataSourceUid)
        End Try
        Return loDbDataSource
    End Function

    ''' <summary>
    ''' 根据DbDataSource UniqueId获取DbDataSource 对象
    ''' </summary>
    ''' <param name="lsDbDataSourceUid">DbDataSource UniqueId</param>
    ''' <returns>DbDataSource 对象</returns>
    Protected Friend Function GetDbDataSource(ByVal lsDbDataSourceUid As String) As DBDataSource
        Return GetDbDataSource(lsDbDataSourceUid, MyForm)
    End Function

    ''' <summary>
    ''' 根据UserDataSource UniqueId获取UserDataSource 对象
    ''' </summary>
    ''' <param name="lsUserDataSourceUid">UserDataSource UniqueId</param>
    ''' <param name="loForm">UserDataSource所在的窗口</param>
    ''' <returns>DbDataSource 对象</returns>
    Protected Friend Function GetUserDataSource(ByVal lsUserDataSourceUid As String, ByVal loForm As Form) As UserDataSource
        Dim loUserDataSource As UserDataSource
        loUserDataSource = loForm.DataSources.UserDataSources.Item(lsUserDataSourceUid)
        Return loUserDataSource
    End Function

    ''' <summary>
    ''' 根据UserDataSource UniqueId获取UserDataSource 对象
    ''' </summary>
    ''' <param name="lsUserDataSourceUid">UserDataSource UniqueId</param>
    ''' <returns>DbDataSource 对象</returns>
    Protected Friend Function GetUserDataSource(ByVal lsUserDataSourceUid As String) As UserDataSource
        Return GetUserDataSource(lsUserDataSourceUid, MyForm)
    End Function

    ''' <summary>
    ''' 根据DataTable UniqueId获取DataTable 对象
    ''' </summary>
    ''' <param name="lsDtUid">DataTable UniqueId</param>
    ''' <param name="loForm">DataTable所在的窗口</param>
    ''' <returns>DataTable 对象</returns>
    Protected Friend Function GetDataTable(ByVal lsDtUid As String, ByVal loForm As Form) As DataTable
        Dim loDt As DataTable = Nothing
        Try
            loDt = loForm.DataSources.DataTables.Add(lsDtUid)
        Catch ex As Exception
            loDt = loForm.DataSources.DataTables.Item(lsDtUid)
        End Try
        Return loDt
    End Function

    ''' <summary>
    ''' 根据DataTable UniqueId获取DataTable 对象
    ''' </summary>
    ''' <param name="lsDtUid">DataTable UniqueId</param>
    ''' <returns>DataTable 对象</returns>
    Protected Friend Function GetDataTable(ByVal lsDtUid As String) As DataTable
        Return GetDataTable(lsDtUid, MyForm)
    End Function


    ''' <summary>
    ''' 获取默认Series
    ''' </summary>
    ''' <param name="lsFormType">Form Type</param>
    ''' <returns>Default Series</returns>
    Protected Friend Function GetDefaultSeries(ByVal lsFormType As String) As String
        Dim lsDefaultSeries As String = String.Empty
        Dim lsSql As String = "select t10.DfltSeries from ONNM t10 where t10.ObjectCode = '" + lsFormType + "'"
        Dim loDt_Sql As DataTable = GetDataTable("TI_ZKH_Sql")
        loDt_Sql.ExecuteQuery(lsSql)
        If Not loDt_Sql.IsEmpty Then
            lsDefaultSeries = TrimStr(loDt_Sql.GetValue(0, 0))
        End If
        Return lsDefaultSeries
    End Function

    ''' <summary>
    ''' 获取当前窗口的默认Series
    ''' </summary>
    ''' <returns>Default Series</returns>
    Protected Friend Function GetDefaultSeries() As String
        Return GetDefaultSeries(MyForm.BusinessObject.Type)
    End Function
End Class
