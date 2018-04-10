Public Class MDM007606Request
    ''' <summary>
    ''' 审批代码
    ''' </summary>
    ''' <returns></returns>
    Public Property Code As String

    ''' <summary>
    ''' 是否指定审批信息（Y;查看审批人和抄送人 N:不查看审批人和抄送人）
    ''' </summary>
    ''' <returns></returns>
    Public Property IsDesignated As String

    ''' <summary>
    ''' 传入参数JSON
    ''' </summary>
    ''' <returns></returns>
    Public Property InputJson As String

    ''' <summary>
    ''' 审批对象（单据类型）
    ''' </summary>
    ''' <returns></returns>
    Public Property BaseType As String

    ''' <summary>
    ''' 对象主键（单据号）
    ''' </summary>
    ''' <returns></returns>
    Public Property BaseKey As String

    ''' <summary>
    ''' 发起人工号（必须）
    ''' </summary>
    ''' <returns></returns>
    Public Property UserCode As String
End Class
