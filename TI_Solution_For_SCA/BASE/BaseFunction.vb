Option Strict Off
Option Explicit On
Imports System.IO
Imports System.Net
Imports System.Text
Imports SAPbouiCOM

Public Class BaseFunction
    Public Shared FromCount As Integer
    Public Shared myUserId As Integer
    Public Shared ioQXZxml As Xml.XmlDocument = New Xml.XmlDocument
    Public Shared ioFromPermission As Hashtable = New Hashtable
    Public Shared ioFromQXID As Hashtable = New Hashtable
    Public Shared ioKZDQXID As Hashtable = New Hashtable
    Public Shared liMenusCount As Integer = 1
    Public Shared ioConfigXML As Xml.XmlDocument = New Xml.XmlDocument
    Public Shared ioVVSXML As Xml.XmlDocument = New Xml.XmlDocument
    Public Shared iiSumDec, iiPriceDec, iiRateDec, iiQtyDec, iiPercentDec, iiMeasureDec As Integer
    Public Shared isPoFromType As String
    Public Shared isURL As String = "http://api.mdm.ylscm.com"

    '   Public Shared isOINVFromType As String
    'UI初始化
    Public Shared Function GSub_UI_AppConnect(ByRef PObj_UI_App As SAPbouiCOM.Application) As Boolean
        Dim LObj_UI_GuiApi As New SAPbouiCOM.SboGuiApi
        Dim LStr_UI_Identifier As String
        '连接当前打开的B1的
        LStr_UI_Identifier = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056"
        Try
            LObj_UI_GuiApi.Connect(LStr_UI_Identifier)
            '获取UI应用程序对象
            PObj_UI_App = LObj_UI_GuiApi.GetApplication()
            If PObj_UI_App Is Nothing Then
                MsgBox("Cann't get any User Interface ojbect.", MsgBoxStyle.Exclamation, "Pay attention!")
                End
            End If
            PObj_UI_App.StatusBar.SetText("Finished User Iterface initiate.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
            Return True
        Catch ex As Exception
            MsgBox("Cann't find Business One client,User Iterface fail to connect and Addon will quit.", MsgBoxStyle.Exclamation, "Pay attention!")
            Return False
            End
        End Try
    End Function

    '添加物料到仓库
    Public Shared Sub AddtoWhs(ByVal lsItemCode As String, ByVal lsWhsCode As String)
        Try
            Dim lsSql As String
            lsSql = "Select top 1 'A' From [OITW] t10 where t10.ItemCode='" + lsItemCode + "' and t10.WhsCode='" + lsWhsCode + "'"
            Dim loR As SAPbobsCOM.Recordset = ItemDispacher.TI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            loR.DoQuery(lsSql)
            If loR.RecordCount = 0 Then
                Dim loItem As SAPbobsCOM.Items
                loItem = ItemDispacher.TI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)

                loItem.GetByKey(lsItemCode.Trim)
                loItem.WhsInfo.Add()
                loItem.WhsInfo.WarehouseCode = lsWhsCode.Trim
                loItem.Update()
            End If
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString())
        End Try
    End Sub


    ''' <summary>
    ''' 添加菜单
    ''' </summary>
    ''' <param name="PObj_UI_App"></param>
    ''' <param name="lsFromUID"></param>
    ''' <remarks></remarks>
    Public Shared Sub SetMenus(ByRef PObj_UI_App As SAPbouiCOM.Application, ByVal lsFromUID As String)
        Try
            Dim loFrom As SAPbouiCOM.Form = PObj_UI_App.Forms.Item(lsFromUID)
            Dim lsFromType As String
            lsFromType = loFrom.TypeEx
            Dim lsXmlPath, lsXmlString As String
            Dim lsFileName As String
            lsFileName = "TI_Solution_For_SCA.TI_TBWEB.XML"
            Dim FileIO As System.IO.Stream
            FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            lsXmlString = sr.ReadToEnd
            Dim loXmlDoc As Xml.XmlDocument = New Xml.XmlDocument
            loXmlDoc.LoadXml(lsXmlString)
            If Not loXmlDoc Is Nothing Then
                lsXmlPath = "/forms/form[@FormType='" + lsFromType + "']"
                Dim loNetSelect As Xml.XmlNode
                loNetSelect = loXmlDoc.SelectSingleNode(lsXmlPath)
                If Not loNetSelect Is Nothing Then
                    Dim loMenus As SAPbouiCOM.Menus
                    loMenus = loFrom.Menu
                    'loMenus.Add("TI_Copy", "复制从Excel剪切板", SAPbouiCOM.BoMenuType.mt_STRING, 0)
                    Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
                    oCreationPackage = PObj_UI_App.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
                    oCreationPackage.UniqueID = "TI_AddWhs" + Convert.ToString(liMenusCount)
                    oCreationPackage.String = "添加仓库到物料"
                    loFrom.Menu.AddEx(oCreationPackage)
                    liMenusCount += 1
                End If
            End If
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString())
        End Try
    End Sub


    ''' <summary>
    ''' 加载权限树,下拉菜单树
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub GetQXXMLDate()
        Try
            Dim lsFileName As String
            lsFileName = "TI_Solution_For_SCA.QX.XML"
            Dim FileIO As System.IO.Stream
            FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            Dim XmlText As String
            XmlText = sr.ReadToEnd
            ioQXZxml.LoadXml(XmlText)


            lsFileName = "TI_Solution_For_SCA.ConfigVVS.XML"
            FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
            sr = New IO.StreamReader(FileIO)
            XmlText = sr.ReadToEnd
            ioConfigXML.LoadXml(XmlText)

            lsFileName = "TI_Solution_For_SCA.VVS.XML"
            FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
            sr = New IO.StreamReader(FileIO)
            XmlText = sr.ReadToEnd
            ioVVSXML.LoadXml(XmlText)
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString())
        End Try
    End Sub

    ''' <summary>
    ''' 获取窗体列表
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub GetFromMenuList()
        Try
            Dim loXmlDoc As Xml.XmlDocument = New Xml.XmlDocument
            Dim lsFileName As String
            lsFileName = "TI_Solution_For_SCA.TI_AddMenu.XML"
            Dim FileIO As System.IO.Stream
            FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            Dim XmlText As String
            XmlText = sr.ReadToEnd
            loXmlDoc.LoadXml(XmlText)

            Dim lsXmlPath As String
            lsXmlPath = "/forms/form"
            Dim loXmlNodeList As Xml.XmlNodeList
            loXmlNodeList = loXmlDoc.SelectNodes(lsXmlPath)
            Dim lsFromType As String
            ItemDispacher.ioListFromM.Clear()

            For Each loXmlNode As Xml.XmlNode In loXmlNodeList
                lsFromType = CType(loXmlNode, Xml.XmlElement).GetAttribute("FormType")
                If Not String.IsNullOrEmpty(lsFromType) Then
                    lsFromType = lsFromType.Trim
                End If
                If Not String.IsNullOrEmpty(lsFromType) Then
                    ItemDispacher.ioListFromM.Add(lsFromType)
                End If
            Next
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString())
        End Try
    End Sub


    ''' <summary>
    ''' 获取权限
    ''' </summary>
    ''' <param name="lsFromType"></param>
    ''' <param name="lsQXType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetQXID(ByVal lsFromType As String, ByVal lsQXType As String) As String
        GetQXID = ""
        Try
            Dim lsXMLPath As String
            lsXMLPath = "QXList/QX[@FromType='" + lsFromType + "' and @QXType='" + lsQXType + "']"
            Select Case lsQXType
                Case "1"
                    '窗体
                    If Not ioFromQXID.ContainsKey(lsFromType) Then
                        Dim loNode As Xml.XmlNode
                        loNode = ioQXZxml.SelectSingleNode(lsXMLPath)
                        If Not loNode Is Nothing Then
                            Dim loXmlE As Xml.XmlElement = CType(loNode, Xml.XmlElement)
                            GetQXID = loXmlE.GetAttribute("QXID")
                            ioFromQXID.Add(lsFromType, GetQXID)
                        End If
                    Else
                        GetQXID = ioFromQXID.Item(lsFromType)
                    End If
                Case "2"
                    '控制点
                    If Not ioKZDQXID.ContainsKey(lsFromType) Then
                        Dim loNode As Xml.XmlNode
                        loNode = ioQXZxml.SelectSingleNode(lsXMLPath)
                        If Not loNode Is Nothing Then
                            Dim loXmlE As Xml.XmlElement = CType(loNode, Xml.XmlElement)
                            GetQXID = loXmlE.GetAttribute("QXID")
                            ioKZDQXID.Add(lsFromType, GetQXID)
                        End If
                    Else
                        GetQXID = ioKZDQXID.Item(lsFromType)
                    End If
            End Select
        Catch ex As Exception

        End Try
    End Function


    ''' <summary>
    ''' 时间转化
    ''' </summary>
    ''' <param name="Str"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FDate(ByVal Str As String) As Date
        Dim lsYear, lsMouth, lsDay As String
        Dim ldDate As Date
        If Not Date.TryParse(Str, ldDate) And Str.Length = 8 Then
            lsYear = Str.Substring(0, 4)
            lsMouth = Str.Substring(4, 2)
            lsDay = Str.Substring(6, 2)
            Str = lsYear + "-" + lsMouth + "-" + lsDay
            ldDate = Date.Parse(Str)
        Else

            ldDate = Date.Parse(Str)
        End If
        Return ldDate
    End Function

    ''' <summary>
    ''' 菜单提交
    ''' </summary>
    ''' <param name="App"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MenuAdd(ByVal App As SAPbouiCOM.Application) As Boolean
        Dim oMenus As SAPbouiCOM.Menus
        Dim oMenuItem As SAPbouiCOM.MenuItem
        Dim oFrom As SAPbouiCOM.Form
        oFrom = App.Forms.GetFormByTypeAndCount(169, 1) '获取主菜单From
        '冻结窗口
        oFrom.Freeze(True)
        Dim lbcheck As Boolean = False
        Try
            '2014-08-27 修改把设置的信息单独
            oMenuItem = App.Menus.Item("8448")
            oMenus = oMenuItem.SubMenus()
            If Not (App.Menus.Exists("TI_T099")) Then '如果菜单不存在，就添加菜单
                oMenuItem = oMenus.Add("TI_T099", "Addon基础设置", SAPbouiCOM.BoMenuType.mt_POPUP, 15) '添加一个菜单
                '再添加子菜单
                oMenus = oMenuItem.SubMenus
                '  oMenus.Add("TI_T004", "基础设置", SAPbouiCOM.BoMenuType.mt_STRING, 4001)
                oMenus.Add("TI_T010", "窗体打印设置", SAPbouiCOM.BoMenuType.mt_STRING, 4002)
                '   oMenus.Add("TI_T030", "快递单打印快递公司维护", SAPbouiCOM.BoMenuType.mt_STRING, 4001)

                lbcheck = True
            End If
            '添加货权转移界面
            If Not (App.Menus.Exists("TI_T011")) Then '如果菜单不存在，就添加菜单
                oMenuItem = App.Menus.Item("2304")
                oMenus = oMenuItem.SubMenus()
                oMenuItem = oMenus.Add("TI_T011", "货权转移", SAPbouiCOM.BoMenuType.mt_STRING, 6) '添加一个菜单
                lbcheck = True
            End If

            '添加货权转移界面
            If Not (App.Menus.Exists("TI_T058")) Then '如果菜单不存在，就添加菜单
                oMenuItem = App.Menus.Item("2048")
                oMenus = oMenuItem.SubMenus()
                oMenuItem = oMenus.Add("TI_T058", "销售交货向导", SAPbouiCOM.BoMenuType.mt_STRING, 6) '添加一个菜单
                lbcheck = True
            End If

            '添加MRP计算界面
            ' oMenuItem = App.Menus.Item("2304")
            ' oMenus = oMenuItem.SubMenus()
            ' If Not (App.Menus.Exists("TI_T088")) Then '如果菜单不存在，就添加菜单
            '    oMenuItem = oMenus.Add("TI_T088", "MRP计算", SAPbouiCOM.BoMenuType.mt_POPUP, 1) '添加一个菜单
            '再添加子菜单
            '   oMenus = oMenuItem.SubMenus
            '  oMenus.Add("TI_T004", "基础设置", SAPbouiCOM.BoMenuType.mt_STRING, 4001)
            '  oMenus.Add("TI_T012", "MRP计算", SAPbouiCOM.BoMenuType.mt_STRING, 4002)
            '   oMenus.Add("TI_T030", "快递单打印快递公司维护", SAPbouiCOM.BoMenuType.mt_STRING, 4001)

            ' lbcheck = True
            ' End If

            '‘’‘
            '添加采购负需求菜单
            'If Not (App.Menus.Exists("TI_T400")) Then '如果菜单不存在，就添加菜单
            '    oMenuItem = App.Menus.Item("43534")
            '    oMenus = oMenuItem.SubMenus()
            '    oMenus.Add("TI_T400", "库存状态-ITEM负需求", SAPbouiCOM.BoMenuType.mt_STRING, 4001)
            '    lbcheck = True
            'End If
            ''添加仓库的负需求
            'If Not (App.Menus.Exists("TI_T43001")) Then '如果菜单不存在，就添加菜单
            '    oMenuItem = App.Menus.Item("4352")
            '    oMenus = oMenuItem.SubMenus()
            '    oMenus.Add("TI_T43001", "库存状态-ITEM负需求", SAPbouiCOM.BoMenuType.mt_STRING, 4001)
            '    lbcheck = True
            'End If


            Return True
        Catch ex As Exception
            App.MessageBox("ADDON 读入窗体菜单时出错:" & ex.ToString())
            Return False
        Finally
            oFrom.Freeze(False)
            '释放窗口
            If lbcheck Then
                oFrom.Update()
            End If
            oMenus = Nothing
            oMenuItem = Nothing
        End Try

    End Function


    ''' <summary>
    ''' 从XML文件中读取窗体
    ''' 2014-04-29 修改程序添加权限
    ''' 2014-09-16 修改处理窗体信息
    ''' 2014-09-16 修改处理下拉框设置
    ''' 2014-09-28 修改修改
    ''' </summary>
    ''' <param name="FileName">文件名</param>
    ''' <param name="App"></param>
    ''' <remarks></remarks>
    Public Shared Function londFromXml(ByVal FileName As String, ByVal App As SAPbouiCOM.Application) As SAPbouiCOM.Form
        londFromXml = Nothing
        Try
            FileName = "TI_Solution_For_SCA." + FileName + ".XML"
            Dim FileIO As System.IO.Stream
            FileIO = GetEmbeddedResource(FileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            Dim XmlText As String
            XmlText = sr.ReadToEnd
            Dim XmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
            '读取XML内容
            XmlDoc.LoadXml(XmlText)
            Dim XmlNode As Xml.XmlNode = XmlDoc.SelectSingleNode("/Application/forms/action/form")
            '获取节点
            Dim XMLE As Xml.XmlElement = XmlNode
            '读取XML文件中的uid属性
            Dim FormUid As String = XMLE.GetAttribute("uid")
            Dim lsFromType As String
            lsFromType = XMLE.GetAttribute("FormType")
            If Not String.IsNullOrEmpty(lsFromType) Then
                lsFromType = lsFromType.Trim
            End If
            If Not String.IsNullOrEmpty(lsFromType) Then
                Dim lsFromUid As String
                lsFromUid = lsFromType + Convert.ToString(BaseFunction.FromCount)
                Dim lsQXID, lsPermission As String
                lsPermission = "F"
                lsQXID = GetQXID(lsFromType, "1")
                If Not String.IsNullOrEmpty(lsQXID) Then
                    lsQXID = lsQXID.Trim
                End If
                If Not String.IsNullOrEmpty(lsQXID) Then
                    lsPermission = GetQxStatus(lsQXID, App.Company.UserName)
                End If

                Dim lileft, litop, liwidth, liheight, liclient_width, liclient_height As Integer
                Dim lsMatrixUID As String
                Dim lbCheckR As Boolean = False
                If lsPermission = "N" Then
                    App.SetStatusBarMessage("该窗体没有权限，请先在用户权限表中设置权限！")
                    Return Nothing
                Else
                    '设置XML文件中的uid属性
                    XMLE.SetAttribute("uid", lsFromUid)

                    '处理下拉框
                    Dim lsXmlPath As String
                    lsXmlPath = "/FormList/Form[@Type='" + lsFromType + "']/ValidValue"
                    Dim loXmlNodeList As Xml.XmlNodeList = ioConfigXML.SelectNodes(lsXmlPath)
                    Dim loXmlEm As Xml.XmlElement
                    If Not loXmlNodeList Is Nothing Then
                        If loXmlNodeList.Count > 0 Then
                            Dim lsSourceType, lsSQLCode, lsItemUID, lsColID As String
                            '处理VVS
                            For Each loXmlNode1 As Xml.XmlNode In loXmlNodeList
                                loXmlEm = CType(loXmlNode1, Xml.XmlElement)
                                lsSourceType = loXmlEm.GetAttribute("SourceType")
                                lsSQLCode = loXmlEm.GetAttribute("SQL")
                                lsItemUID = loXmlEm.GetAttribute("ItemUID")
                                lsColID = loXmlEm.GetAttribute("ColID")
                                Select Case lsSourceType
                                    Case "1"
                                        'SQL
                                        AddComboBoxForSQL(XmlDoc, lsSQLCode, lsItemUID, lsColID)
                                    Case "2"
                                        'XML
                                        AddComboBoxForXmlCode(XmlDoc, lsSQLCode, lsItemUID, lsColID)
                                End Select
                            Next
                        End If
                    End If

                    App.LoadBatchActions(XmlDoc.InnerXml)
                    Dim oForm As SAPbouiCOM.Form
                    oForm = App.Forms.Item(lsFromUid)

                    If lbCheckR Then
                        oForm.Resize(liwidth, liheight)
                    End If


                    FromCount = FromCount + 1
                    ioFromPermission.Add(oForm.UniqueID, lsPermission)
                    If lsPermission = "R" Then
                        oForm.Mode = BoFormMode.fm_VIEW_MODE
                        'oForm.EnableMenu("1982", False)
                        'oForm.EnableMenu("1981", False)
                    End If
                    Return oForm
                End If
            End If
        Catch ex As Exception
            App.MessageBox("出现错误：" + ex.ToString())
            FromCount = FromCount + 1
            Return Nothing
        End Try
    End Function



    ''' <summary>
    ''' 获取用户权限
    ''' </summary>
    ''' <param name="lsQXID"></param>
    ''' <param name="lsUserCode"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetQxStatus(ByVal lsQXID As String, ByVal lsUserCode As String) As String
        GetQxStatus = "N"
        Try
            Dim lsSql As String
            lsSql = "Select (CASE when t10.SUPERUSER='Y' then 'F' else ISNULL(t11.Permission,'N') end) Permission From OUSR t10" + vbNewLine + _
                    "left join USR3 t11 on t10.USERID=t11.UserLink and t11.PermId ='" + lsQXID + "'" + vbNewLine + _
                    "where t10.USER_CODE='" + lsUserCode + "'"
            ItemDispacher.ioDatatable.ExecuteQuery(lsSql)
            If Not ItemDispacher.ioDatatable.IsEmpty Then
                GetQxStatus = ItemDispacher.ioDatatable.GetValue("Permission", 0)
            Else
                GetQxStatus = "N"
            End If
        Catch ex As Exception
            GetQxStatus = "N"
        End Try
    End Function



    ''' <summary>
    ''' 从xmlString中加载窗体
    ''' 2014-04-29 修改程序添加权限
    ''' </summary>
    ''' <param name="XmlStr"></param>
    ''' <param name="App"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function londFromXmlString(ByVal XmlStr As String, ByVal App As SAPbouiCOM.Application) As SAPbouiCOM.Form
        londFromXmlString = Nothing
        Try
            Dim XmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
            '读取XML内容
            XmlDoc.LoadXml(XmlStr)
            Dim XmlNode As Xml.XmlNode = XmlDoc.SelectSingleNode("/Application/forms/action/form")
            '获取节点
            Dim XMLE As Xml.XmlElement = XmlNode
            '读取XML文件中的uid属性
            Dim FormUid As String = XMLE.GetAttribute("uid")
            Dim lsFromType As String
            lsFromType = XMLE.GetAttribute("FormType")
            If Not String.IsNullOrEmpty(lsFromType) Then
                lsFromType = lsFromType.Trim
            End If
            If Not String.IsNullOrEmpty(lsFromType) Then
                Dim lsFromUid As String
                lsFromUid = lsFromType + Convert.ToString(BaseFunction.FromCount)

                '设置XML文件中的uid属性
                XMLE.SetAttribute("uid", lsFromUid)

                '处理下拉框
                Dim lsXmlPath As String
                lsXmlPath = "/FormList/Form[@Type='" + lsFromType + "']/ValidValue"
                Dim loXmlNodeList As Xml.XmlNodeList = ioConfigXML.SelectNodes(lsXmlPath)
                Dim loXmlEm As Xml.XmlElement
                If Not loXmlNodeList Is Nothing Then
                    If loXmlNodeList.Count > 0 Then
                        Dim lsSourceType, lsSQLCode, lsItemUID, lsColID As String
                        '处理VVS
                        For Each loXmlNode1 As Xml.XmlNode In loXmlNodeList
                            loXmlEm = CType(loXmlNode1, Xml.XmlElement)
                            lsSourceType = loXmlEm.GetAttribute("SourceType")
                            lsSQLCode = loXmlEm.GetAttribute("SQL")
                            lsItemUID = loXmlEm.GetAttribute("ItemUID")
                            lsColID = loXmlEm.GetAttribute("ColID")
                            Select Case lsSourceType
                                Case "1"
                                    'SQL
                                    AddComboBoxForSQL(XmlDoc, lsSQLCode, lsItemUID, lsColID)
                                Case "2"
                                    'XML
                                    AddComboBoxForXmlCode(XmlDoc, lsSQLCode, lsItemUID, lsColID)
                            End Select
                        Next
                    End If
                End If

                App.LoadBatchActions(XmlDoc.InnerXml)
                Dim oForm As SAPbouiCOM.Form
                oForm = App.Forms.Item(lsFromUid)

                'If lbCheckR Then
                '    oForm.Resize(liwidth, liheight)
                'End If

                'FromCount = FromCount + 1
                'ioFromPermission.Add(oForm.UniqueID, lsPermission)
                'If lsPermission = "R" Then
                '    oForm.Mode = BoFormMode.fm_VIEW_MODE
                '    'oForm.EnableMenu("1982", False)
                '    'oForm.EnableMenu("1981", False)
                'End If
                Return oForm

                'Dim lsQXID, lsPermission As String
                'lsPermission = "F"
                'lsQXID = GetQXID(lsFromType, "1")
                'If Not String.IsNullOrEmpty(lsQXID) Then
                '    lsQXID = lsQXID.Trim
                'End If
                'If Not String.IsNullOrEmpty(lsQXID) Then
                '    lsPermission = GetQxStatus(lsQXID, App.Company.UserName)
                'End If

                'Dim lileft, litop, liwidth, liheight, liclient_width, liclient_height As Integer
                'Dim lsMatrixUID As String
                'Dim lbCheckR As Boolean = False

                'If lsPermission = "N" Then
                '    App.SetStatusBarMessage("该窗体没有权限，请先在用户权限表中设置权限！")
                '    Return Nothing
                'Else
                '    '设置XML文件中的uid属性
                '    XMLE.SetAttribute("uid", lsFromUid)




                '    '处理窗体信息
                '    'Dim lsSql As String
                '    'lsSql = "Select * From [@TI_Z0450] t10 where t10.U_FormType='" + lsFromType + "' and t10.U_UserID='" + Convert.ToString(myUserId) + "'"
                '    'ItemDispacher.ioDatatable.ExecuteQuery(lsSql)
                '    'If Not ItemDispacher.ioDatatable.IsEmpty Then

                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_left", 0), lileft)
                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_top", 0), litop)
                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_width", 0), liwidth)
                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_height", 0), liheight)

                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_cwidth", 0), liclient_width)
                '    '    Integer.TryParse(ItemDispacher.ioDatatable.GetValue("U_cheight", 0), liclient_height)

                '    '    XMLE.SetAttribute("left", Convert.ToString(lileft))
                '    '    XMLE.SetAttribute("top", Convert.ToString(litop))
                '    '    'XMLE.SetAttribute("width", Convert.ToString(liwidth))
                '    '    'XMLE.SetAttribute("height", Convert.ToString(liheight))
                '    '    XMLE.SetAttribute("client_width", Convert.ToString(liclient_width))
                '    '    XMLE.SetAttribute("client_height", Convert.ToString(liclient_height))

                '    '    lsMatrixUID = ItemDispacher.ioDatatable.GetValue("U_MUID", 0)
                '    '    If Not String.IsNullOrEmpty(lsMatrixUID) Then
                '    '        lsMatrixUID = lsMatrixUID.Trim
                '    '    End If
                '    '    If Not String.IsNullOrEmpty(lsMatrixUID) Then
                '    '        XmlNode = XmlNode.SelectSingleNode("Settings")
                '    '        If Not XmlNode Is Nothing Then
                '    '            XMLE = CType(XmlNode, Xml.XmlElement)
                '    '            XMLE.SetAttribute("MatrixUID", lsMatrixUID)
                '    '        End If
                '    '    End If
                '    '    lbCheckR = True
                '    'End If

                '    ' App.LoadBatchActions(XmlDoc.InnerXml)

                '    Dim oForm As SAPbouiCOM.Form
                '    oForm = App.Forms.Item(lsFromUid)

                '    If lbCheckR Then
                '        oForm.Resize(liwidth, liheight)
                '    End If

                '    FromCount = FromCount + 1
                '    ioFromPermission.Add(oForm.UniqueID, lsPermission)
                '    If lsPermission = "R" Then
                '        oForm.Mode = BoFormMode.fm_VIEW_MODE
                '        'oForm.EnableMenu("1982", False)
                '        'oForm.EnableMenu("1981", False)
                '    End If
                '    Return oForm
                'End If
            End If
        Catch ex As Exception
            App.MessageBox("出现错误：" + ex.ToString())
            FromCount = FromCount + 1
            Return Nothing
        End Try
    End Function


    ''' <summary>
    ''' 获取报表
    ''' </summary>
    ''' <param name="loForm"></param>
    ''' <param name="ioDataTable"></param>
    ''' <remarks></remarks>
    Public Shared Sub GetReportType(ByVal loForm As Form, ByVal ioDataTable As SAPbouiCOM.DataTable)
        Dim lsSql, lsFromType, lsCode As String
        lsFromType = loForm.TypeEx
        lsSql = "Select top 1 U_Code From [@TI_Z0121] where U_FromType='" + lsFromType + "'"
        ioDataTable.ExecuteQuery(lsSql)
        If Not ioDataTable.IsEmpty Then
            lsCode = ioDataTable.GetValue("U_Code", 0)
            loForm.ReportType = lsCode
            loForm.EnableMenu("5895", True)
        Else
            loForm.EnableMenu("5895", False)
        End If
    End Sub


    ''' <summary>
    ''' 添加下拉框
    ''' </summary>
    ''' <param name="loXmlDoc"></param>
    ''' <param name="lsSql"></param>
    ''' <param name="lsItemUID"></param>
    ''' <param name="lsColID"></param>
    ''' <remarks></remarks>
    Public Shared Sub AddComboBoxForSQL(ByVal loXmlDoc As Xml.XmlDocument, ByVal lsSql As String, ByVal lsItemUID As String, ByVal lsColID As String)
        Try
            Dim loXmlNode As Xml.XmlNode
            Dim lsXmlPath As String
            Dim lsCode, lsName As String
            Dim lsXmlString, lsXmlStringLine As String
            lsXmlString = ""
            If lsColID = "" Then
                '是Item
                lsXmlPath = "/Application/forms/action/form/items/action/item[@uid='" + lsItemUID + "']/specific/ValidValues/action"
                loXmlNode = loXmlDoc.SelectSingleNode(lsXmlPath)
                If Not loXmlNode Is Nothing Then
                    ItemDispacher.ioDatatable.ExecuteQuery(lsSql)
                    If Not ItemDispacher.ioDatatable.IsEmpty Then
                        For i As Integer = 0 To ItemDispacher.ioDatatable.Rows.Count - 1
                            lsCode = ItemDispacher.ioDatatable.GetValue(0, i)
                            lsName = ItemDispacher.ioDatatable.GetValue(1, i)
                            lsXmlStringLine = "<ValidValue value=""" + lsCode + """ description=""" + lsName.Replace("&", "") + """/>" + vbNewLine

                            lsXmlString = lsXmlString + lsXmlStringLine

                        Next i

                        loXmlNode.InnerXml = lsXmlString
                    End If
                End If
            Else
                '是matrix
                lsXmlPath = "/Application/forms/action/form/items/action/item[@uid='" + lsItemUID + "']/specific/columns/action/column[@uid='" + lsColID + "']/ValidValues/action"
                loXmlNode = loXmlDoc.SelectSingleNode(lsXmlPath)
                If Not loXmlNode Is Nothing Then
                    ItemDispacher.ioDatatable.ExecuteQuery(lsSql)
                    If Not ItemDispacher.ioDatatable.IsEmpty Then
                        For i As Integer = 0 To ItemDispacher.ioDatatable.Rows.Count - 1
                            lsCode = ItemDispacher.ioDatatable.GetValue(0, i)
                            lsName = ItemDispacher.ioDatatable.GetValue(1, i)
                            lsXmlStringLine = "<ValidValue value=""" + lsCode + """ description=""" + lsName.Replace("&", "") + """/>" + vbNewLine

                            lsXmlString = lsXmlString + lsXmlStringLine

                        Next i

                        loXmlNode.InnerXml = lsXmlString
                    End If
                End If
            End If
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 添加下拉框
    ''' </summary>
    ''' <param name="loXmlDoc"></param>
    ''' <param name="lsCode"></param>
    ''' <param name="lsItemUID"></param>
    ''' <param name="lsColID"></param>
    ''' <remarks></remarks>
    Public Shared Sub AddComboBoxForXmlCode(ByVal loXmlDoc As Xml.XmlDocument, ByVal lsSql As String, ByVal lsItemUID As String, ByVal lsColID As String)
        Try
            Dim loXmlNode As Xml.XmlNode
            Dim lsXmlPath As String
            Dim lsCode, lsName As String
            Dim lsXmlString, lsXmlStringLine As String
            lsXmlString = ""
            If lsColID = "" Then
                '是Item
                lsXmlPath = "/Application/forms/action/form/items/action/item[@uid='" + lsItemUID + "']/specific/ValidValues/action"
                loXmlNode = loXmlDoc.SelectSingleNode(lsXmlPath)
                If Not loXmlNode Is Nothing Then
                    lsXmlPath = "/VVSList/VVS[@Code='" + lsSql + "']/ValidValue"
                    Dim loXmlNodeList As System.Xml.XmlNodeList = ioVVSXML.SelectNodes(lsXmlPath)
                    If Not loXmlNodeList Is Nothing Then
                        For Each loXmlNode22 As Xml.XmlNode In loXmlNodeList
                            lsCode = Convert.ToString(CType(loXmlNode22, Xml.XmlElement).GetAttribute("value"))
                            lsName = Convert.ToString(CType(loXmlNode22, Xml.XmlElement).GetAttribute("description"))

                            lsXmlStringLine = "<ValidValue value=""" + lsCode + """ description=""" + lsName + """/>" + vbNewLine

                            lsXmlString = lsXmlString + lsXmlStringLine
                        Next
                        loXmlNode.InnerXml = lsXmlString
                    End If
                End If
            Else
                '是matrix
                lsXmlPath = "/Application/forms/action/form/items/action/item[@uid='" + lsItemUID + "']/specific/columns/action/column[@uid='" + lsColID + "']/ValidValues/action"
                loXmlNode = loXmlDoc.SelectSingleNode(lsXmlPath)
                If Not loXmlNode Is Nothing Then
                    '处理XML文件
                    lsXmlPath = "/VVSList/VVS[@Code='" + lsSql + "']/ValidValue"
                    Dim loXmlNodeList As System.Xml.XmlNodeList = ioVVSXML.SelectNodes(lsXmlPath)
                    If Not loXmlNodeList Is Nothing Then
                        For Each loXmlNode22 As Xml.XmlNode In loXmlNodeList
                            lsCode = Convert.ToString(CType(loXmlNode22, Xml.XmlElement).GetAttribute("value"))
                            lsName = Convert.ToString(CType(loXmlNode22, Xml.XmlElement).GetAttribute("description"))

                            lsXmlStringLine = "<ValidValue value=""" + lsCode + """ description=""" + lsName + """/>" + vbNewLine

                            lsXmlString = lsXmlString + lsXmlStringLine
                        Next
                        loXmlNode.InnerXml = lsXmlString
                    End If
                End If
            End If
        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString)
        End Try
    End Sub


    ''' <summary>
    ''' 初始化下拉框
    ''' </summary>
    ''' <param name="lsSourceType">数据源类型（1:SQL 2:XML配置文件）</param>
    ''' <param name="MyForm"></param>
    ''' <param name="myDataTable"></param>
    ''' <param name="Sql"></param>
    ''' <param name="ItemUID"></param>
    ''' <remarks></remarks>
    Public Shared Sub AddComboBox(ByVal lsSourceType As String, ByVal MyForm As Form, ByVal myDataTable As SAPbouiCOM.DataTable, ByVal lsSql As String, ByVal lsItemUID As String)
        Dim loCbx As ComboBox
        loCbx = MyForm.Items.Item(lsItemUID).Specific
        Dim lsCode, lsName As String
        Select Case lsSourceType
            Case "1"
                'SQL 
                myDataTable.ExecuteQuery(lsSql)
                If Not myDataTable.IsEmpty Then
                    For i As Integer = 0 To myDataTable.Rows.Count - 1
                        lsCode = myDataTable.GetValue(0, i)
                        lsName = myDataTable.GetValue(1, i)
                        loCbx.ValidValues.Add(lsCode, lsName)
                    Next i
                End If
            Case "2"
                'XML配置文件
                '读取XML配置文件
                Dim lsFileName As String
                lsFileName = "TI_Solution_For_SCA.VVS.XML"
                Dim FileIO As System.IO.Stream
                FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
                Dim sr As New IO.StreamReader(FileIO)
                Dim lsXmlText As String
                lsXmlText = sr.ReadToEnd

                '处理XML文件
                Dim loXmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
                loXmlDoc.LoadXml(lsXmlText)
                Dim lsXMLPath As String
                lsXMLPath = "/VVSList/VVS[@Code='" + lsSql + "']/ValidValue"
                Dim loXmlNodeList As System.Xml.XmlNodeList = loXmlDoc.SelectNodes(lsXMLPath)
                If Not loXmlNodeList Is Nothing Then
                    For Each loXmlNode As Xml.XmlNode In loXmlNodeList
                        lsCode = Convert.ToString(CType(loXmlNode, Xml.XmlElement).GetAttribute("value"))
                        lsName = Convert.ToString(CType(loXmlNode, Xml.XmlElement).GetAttribute("description"))
                        loCbx.ValidValues.Add(lsCode, lsName)
                    Next
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 初始化下拉框(表格)
    ''' </summary>
    ''' <param name="lsSourceType">数据源类型（1:SQL 2:XML配置文件）</param>
    ''' <param name="MyForm"></param>
    ''' <param name="myDataTable"></param>
    ''' <param name="Sql"></param>
    ''' <param name="ItemUID"></param>
    ''' <param name="ColID"></param>
    ''' <remarks></remarks>
    Public Shared Sub AddComboBox(ByVal lsSourceType As String, ByVal MyForm As Form, ByVal myDataTable As SAPbouiCOM.DataTable, ByVal lsSql As String, ByVal lsItemUID As String, ByVal lsColID As String)
        Dim lsCode, lsName As String
        Dim loMtx As Matrix
        loMtx = MyForm.Items.Item(lsItemUID).Specific
        Dim loCol As Column = loMtx.Columns.Item(lsColID)
        Select Case lsSourceType
            Case "1"
                myDataTable.ExecuteQuery(lsSql)
                'SQL
                If Not myDataTable.IsEmpty Then
                    For i As Integer = 0 To myDataTable.Rows.Count - 1
                        lsCode = myDataTable.GetValue(0, i)
                        lsName = myDataTable.GetValue(1, i)
                        loCol.ValidValues.Add(lsCode, lsName)
                    Next
                End If
            Case "2"
                'XML配置文件
                '读取XML配置文件
                Dim lsFileName As String
                lsFileName = "TI_Solution_For_SCA.VVS.XML"
                Dim FileIO As System.IO.Stream
                FileIO = GetEmbeddedResource(lsFileName) '读取资源文件
                Dim sr As New IO.StreamReader(FileIO)
                Dim lsXmlText As String
                lsXmlText = sr.ReadToEnd

                '处理XML文件
                Dim loXmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
                loXmlDoc.LoadXml(lsXmlText)
                Dim lsXMLPath As String
                lsXMLPath = "/VVSList/VVS[@Code='" + lsSql + "']/ValidValue"
                Dim loXmlNodeList As System.Xml.XmlNodeList = loXmlDoc.SelectNodes(lsXMLPath)
                If Not loXmlNodeList Is Nothing Then
                    For Each loXmlNode As Xml.XmlNode In loXmlNodeList
                        lsCode = Convert.ToString(CType(loXmlNode, Xml.XmlElement).GetAttribute("value"))
                        lsName = Convert.ToString(CType(loXmlNode, Xml.XmlElement).GetAttribute("description"))
                        loCol.ValidValues.Add(lsCode, lsName)
                    Next
                End If
        End Select
    End Sub


    '资源文件处理
    Public Shared Function GetEmbeddedResource(ByVal strname As String) As System.IO.Stream
        Return System.Reflection.Assembly.GetExecutingAssembly.GetManifestResourceStream(strname)
    End Function

    ''设置DI的连接上下文
    'Public Shared Function GFuc_DI_ContextSet(ByVal PUI_App_Obj As SAPbouiCOM.Application, ByRef PDI_Cmp_Obj As SAPbobsCOM.Company) As Boolean
    '    Dim LStr_DI_ConnectContext As String
    '    '
    '    Try
    '        PDI_Cmp_Obj = New SAPbobsCOM.Company
    '        LStr_DI_ConnectContext = PUI_App_Obj.Company.GetConnectionContext(PDI_Cmp_Obj.GetContextCookie)
    '        '判断是否已经连接到DI
    '        If PDI_Cmp_Obj.Connected Then
    '            PDI_Cmp_Obj.Disconnect()
    '        End If
    '        '返回
    '        PDI_Cmp_Obj.SetSboLoginContext(LStr_DI_ConnectContext)
    '        Return True
    '    Catch ex As Exception
    '        PUI_App_Obj.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        Return False
    '    End Try
    'End Function

    ''连接DI
    'Public Shared Sub GSub_DI_Connect(ByVal PUI_App_Obj As SAPbouiCOM.Application, ByRef PDI_Cmp_Obj As SAPbobsCOM.Company)
    '    Try
    '        If Not PDI_Cmp_Obj.Connected Then
    '            If Not PDI_Cmp_Obj.Connect = 0 Then
    '                End 'Addon 终止
    '            Else
    '                Return
    '            End If
    '        End If
    '    Catch ex As Exception
    '        End
    '    End Try
    'End Sub


    Public Shared Function GetFilePath(ByVal pTitle As String, ByVal Filter As String) As String
        Dim WINfrm As New System.Windows.Forms.Form
        WINfrm.TopMost = True
        WINfrm.Height = 0
        WINfrm.Width = 0
        WINfrm.WindowState = FormWindowState.Minimized
        WINfrm.Visible = True
        Dim OpenFileDialog1 As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()

        '创建一个OpenFileDialog实例
        With OpenFileDialog1
            .Filter = Filter
            '设定文件类型过滤条件为：文本类型和全部文件
            .FilterIndex = 1
            '设定打开文件对话框缺省的文件过滤条件
            '设定打开文件对话框缺省的目录
            .Title = pTitle
            '设定打开文件对话框的标题
            .Multiselect = False
            '设定可以选择多个文件
            .ReadOnlyChecked = False
            '设定选中"只读"复选框
            .ShowReadOnly = False
            '设定显示"只读"复选框
        End With
        '设定打开文件对话框的式样和功
        System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False
        Try
            If OpenFileDialog1.ShowDialog(WINfrm) = DialogResult.OK Then
                ' 显示打开文件对话框，并判断单击对话框中的"确定"按钮
                GetFilePath = OpenFileDialog1.FileName
                WINfrm.Close()
            Else
                GetFilePath = ""
                WINfrm.Close()
            End If
        Catch ex As Exception

        End Try
        System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = True
        GC.Collect()
    End Function

    '************日志文件****************************
    Public Shared Sub LogFile(ByVal ErrString As String)
        Try
            '定义路径，在程序目录下
            Dim Path As String
            Path = IO.Directory.GetParent(System.Windows.Forms.Application.StartupPath).ToString() + "\Log.txt"
            Dim Fs As IO.FileStream
            '检测是否有这个文件
            '如果存在日志文件,写入TXT文件
            If IO.File.Exists(Path) Then
                '先读取TXT文本
                Fs = New IO.FileStream(Path, IO.FileMode.Open)
                Dim Sr As IO.StreamReader = New IO.StreamReader(Fs, System.Text.Encoding.Default)
                Dim IoEX, IoEx2 As String
                IoEX = Sr.ReadToEnd().ToString()
                '再写入TXT文本
                IoEx2 = Chr(13) + Chr(10) & ErrString & "    错误发生时间：" & Now.ToString() + Chr(13) + Chr(10)
                Dim Sw As IO.StreamWriter = New IO.StreamWriter(Fs, System.Text.Encoding.Default)
                Sw.Write(IoEx2)
                Sw.Flush()
                Sw.Close()

            Else
                '如果不存在，创建文件
                Fs = IO.File.Create(Path)
                '写入内容
                Dim Sw As IO.StreamWriter = New IO.StreamWriter(Fs, System.Text.Encoding.Default)
                Dim IoEX As String
                IoEX = ErrString & "     错误发生时间：" & Now.ToString() + Chr(13) + Chr(10)
                Sw.Write(IoEX)
                '清理战场
                Sw.Flush()
                Sw.Close()
            End If

        Catch ex As Exception
            Windows.Forms.MessageBox.Show(ex.ToString)
        End Try
    End Sub
    ''' <summary>
    ''' 将日期字符串转换为日期
    ''' </summary>
    ''' <param name="lsDateStr">日期字符串</param>
    ''' <returns>日期</returns>
    Public Shared Function GetStandardDate(ByRef lsDateStr As String) As Date
        Dim lsYear, lsMouth, lsDay As String
        Dim ldDate As Date
        If Not Date.TryParse(lsDateStr, ldDate) And lsDateStr.Length = 8 Then
            lsYear = lsDateStr.Substring(0, 4)
            lsMouth = lsDateStr.Substring(4, 2)
            lsDay = lsDateStr.Substring(6, 2)
            lsDateStr = lsYear + "-" + lsMouth + "-" + lsDay
            ldDate = Date.Parse(lsDateStr)
        Else
            ldDate = Date.Parse(lsDateStr)
        End If
        Return ldDate
    End Function


    Public Shared Function PostMoths(ByVal url As String, ByVal param As String) As String
        Try
            Dim strURL As String = url
            Dim request As System.Net.HttpWebRequest
            request = CType(WebRequest.Create(strURL), System.Net.HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/json;charset=UTF-8"
            Dim paraUrlCoded As String = param
            Dim payload() As Byte
            payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded)
            request.ContentLength = payload.Length
            Dim writer As Stream = request.GetRequestStream()
            writer.Write(payload, 0, payload.Length)
            writer.Close()
            Dim response As System.Net.HttpWebResponse
            response = CType(request.GetResponse(), System.Net.HttpWebResponse)
            Dim s As System.IO.Stream
            s = response.GetResponseStream()
            Dim StrDate As String = ""
            Dim strValue As String = ""
            Dim Reader As StreamReader = New StreamReader(s, Encoding.UTF8)
            StrDate = Reader.ReadLine()
            While Not String.IsNullOrEmpty(StrDate)
                strValue += StrDate + vbNewLine
                StrDate = Reader.ReadLine()
            End While
            Return strValue
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function
End Class
