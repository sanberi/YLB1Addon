Imports System.Collections.Generic

Public Class MDM007606Response
    ''' <summary>
    ''' 审批人
    ''' </summary>
    ''' <returns></returns>
    Public Property Approvers As List(Of String)

    ''' <summary>
    ''' 抄送人
    ''' </summary>
    ''' <returns></returns>
    Public Property CcList As List(Of String)

    ''' <summary>
    ''' 请求id
    ''' </summary>
    ''' <returns></returns>
    Public Property RequestId As String

    ''' <summary>
    ''' 单号
    ''' </summary>
    ''' <returns></returns>
    Public Property DocEntry As Long
End Class
