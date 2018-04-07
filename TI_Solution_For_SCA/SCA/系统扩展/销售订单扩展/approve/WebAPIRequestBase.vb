'/ <summary>
'/ 请求基础类
'/ 2017.9.22 添加审批信息
'/ </summary>
Public Class WebAPIRequestBase

    '/ <summary>
    '/ 用户身份
    '/ </summary>
    Public Property Tonken As String


    '/ <summary>
    '/ 浏览器平台
    '/ </summary>
    Public Property BrowserID As String


    '/ <summary>
    '/ 请求来源(1.PC 2.微官网 3.APP 4.手工录入 5.未知)
    '/ </summary>
    Public Property Source As String


    '/ <summary>
    '/ 执行方法（Q（查询），A（添加），U（更新），D（删除），C（取消），L（关闭））
    '/ </summary>
    Public Property Method As String


    '/ <summary>
    '/ 用户代码/用户ID
    '/ </summary>
    Public Property UserCode As String


    '/ <summary>
    '/ 用户手机号
    '/ </summary>
    Public Property Phone As String


    '/ <summary>
    '/ 需要返回内容（1.返回 2.不返回）
    '/ </summary>
    Public Property IsReturnContent As String

    '/ <summary>
    '/ 当前页（Jqgrid）
    '/ </summary>
    Public Property page As Integer

    '/ <summary>
    '/ 页面大小（Jqgrid）
    '/ </summary>
    Public Property rows As Integer

    '/ <summary>
    '/ 排序字段（Jqgrid）
    '/ </summary>
    Public Property sidx As String


    '/ <summary>
    '/ 排序方式（Jqgrid）
    '/ </summary>
    Public Property sord As String

    '/ <summary>
    '/ 客户代码
    '/ </summary>
    Public Property CardCode As String

    '/ <summary>
    '/ 工号
    '/ </summary>
    Public Property EmployeeNo As String

    '/ <summary>
    '/ 是否触发审批模板
    '/ </summary>
    Public Property IsApprove As Boolean

End Class
