Public Class MDM007607Request
    ''' <summary>
    ''' Redis Key
    ''' </summary>
    ''' <returns></returns>
    Public Property RequestId As String

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

End Class
