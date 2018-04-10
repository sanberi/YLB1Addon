Public Class WebAPIResponseBase
    '''/ <summary>
    '''/ 返回状态
    '''/ </summary>
    Public Property Status As Integer

    ''' <summary>
    '''  返回的错误信息
    ''' </summary>
    ''' <returns></returns>
    Public Property Message As String

    ''' <summary>
    ''' 用户身份
    ''' </summary>
    ''' <returns></returns>
    Public Property Tonken As String

    ''' <summary>
    ''' 执行方法
    ''' </summary>
    ''' <returns></returns>
    Public Property Method As String

    ''' <summary>
    ''' 用户代码/用户ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UserCode As String

    ''' <summary>
    ''' 需要返回内容
    ''' </summary>
    ''' <returns></returns>
    Public Property IbReturnContent As Boolean
End Class
