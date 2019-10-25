Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO
Imports TIModule

Public NotInheritable Class TI_Z0151
    Inherits FormBase

    Private ioUds_DateF, ioUds_DateT, ioUds_CardName, ioUds_XSY, ioUds_KF, ioUds_ShowHand As UserDataSource
    Private ibCheckLoad As Boolean = False
    Private iiDocEntry As Integer
    Dim isBsEntry As Object


    Private Sub TI_Z0055_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles Me.ItemEvent
        Select Case pVal.EventType
            Case BoEventTypes.et_ITEM_PRESSED
                If Not pVal.BeforeAction Then
                    Select Case pVal.ItemUID
                        Case "CX"
                            Btn_Select()
                    End Select
                End If
            Case BoEventTypes.et_VALIDATE
                If Not pVal.BeforeAction And pVal.ItemChanged Then
                    Select Case pVal.ItemUID
                        Case "Mtx_10"
                            Select Case pVal.ColUID
                                Case "KDDoc"

                            End Select
                    End Select
                End If
        End Select
    End Sub


    Public Sub Btn_Select()
        Try
            Dim lsSql As String
            lsSql = "Select Cast(ROW_NUMBER() OVER(ORDER BY t10.CardCode) as Int) as LineId,'N' as U_Select " +
                " ,t10.CardCode,t10.CardName,t11.ItemCode,t11.Dscription as ItemName,t11.OpenCreQty as Qty,t11.PriceAfVAT as Price " +
               ",ROUND(t11.OpenCreQty*t11.PriceAfVAT,2) as LineTotal,t11.ShipDate,t11.U_ReceiWare as ReceiWare,t11.WhsCode,t10.Comments,t11.FreeTxt,t12.OnHand,t13.OnHand WhsOnHand,t12.U_Price ,t10.DocEntry,t11.LineNum,t11.OpenCreQty as DOQty " +
               " from ORDR t10 inner join RDR1 t11 on t10.DocEntry=t11.DocEntry  inner join OITM t12 on t11.ItemCode=t12.ItemCode " +
               " inner join OCRD t15 on t10.CardCode=t15.CardCode left join OITW t13 on t11.ItemCode=t13.ItemCode and t11.WhsCode=t13.WhsCode " +
               " where  t10.CANCELED='N' and t10.DocStatus='O' and t11.LineStatus='O' "
            Dim lsCardName, lsXSY, lsKF, lsShowHand As String
            Dim lsDocDateF, lsDocDateT As String
            lsCardName = ioUds_CardName.ValueEx
            lsXSY = ioUds_XSY.ValueEx
            lsKF = ioUds_KF.ValueEx
            lsShowHand = ioUds_ShowHand.ValueEx
            lsDocDateF = ioUds_DateF.ValueEx
            lsDocDateT = ioUds_DateT.ValueEx
            If Not String.IsNullOrEmpty(lsCardName) Then
                lsCardName = lsCardName.Trim()
            End If
            If Not String.IsNullOrEmpty(lsXSY) Then
                lsXSY = lsXSY.Trim()
            End If
            If Not String.IsNullOrEmpty(lsKF) Then
                lsKF = lsKF.Trim()
            End If
            If Not String.IsNullOrEmpty(lsShowHand) Then
                lsShowHand = lsShowHand.Trim()
            End If
            If Not String.IsNullOrEmpty(lsCardName) Then
                lsSql = lsSql + " and t10.CardName like '%" + lsCardName + "%'"
            End If

            If lsShowHand = "Y" Then
                lsSql = lsSql + " and (t12.OnHand >0 or t13.OnHand >0) "
            End If

            If Not String.IsNullOrEmpty(lsKF) Then
                lsSql = lsSql + " and t15.U_CS like '%" + lsKF + "%' "
            End If

            If Not String.IsNullOrEmpty(lsXSY) Then
                lsSql = lsSql + " and t10.U_Saler like '%" + lsXSY + "%' "
            End If

            If Not String.IsNullOrEmpty(lsDocDateF) Then
                lsSql = lsSql + " and t11.ShipDate>='" + lsDocDateF + "' "
            End If

            If Not String.IsNullOrEmpty(lsDocDateT) Then
                lsSql = lsSql + " and t11.ShipDate<='" + lsDocDateT + "' "
            End If

            Dim loForm As Form
            Dim FileName As String
            FileName = "TI_Solution_For_SCA.TI_Z0150.XML"
            Dim FileIO As System.IO.Stream
            FileIO = BaseFunction.GetEmbeddedResource(FileName) '读取资源文件
            Dim sr As New IO.StreamReader(FileIO)
            Dim XmlText As String
            XmlText = sr.ReadToEnd
            Dim XmlDoc As System.Xml.XmlDocument = New Xml.XmlDocument
            XmlDoc.LoadXml(XmlText)
            Dim XmlNode As Xml.XmlNode = XmlDoc.SelectSingleNode("/Application/forms/action/form/datasources/DataTables/DataTable[@Uid='DOC']/Query")
            '处理SQL
            XmlNode.InnerText = lsSql

            loForm = BaseFunction.londFromXmlString(XmlDoc.InnerXml, MyApplication)
            Dim loDesktop As Desktop = MyApplication.Desktop
            Dim liH, liW As Integer
            liH = loDesktop.Height - 130
            liW = loDesktop.Width - 80
            loForm.Resize(liW, liH)
            If Not loForm Is Nothing Then
                Dim Tag(0) As Object
                Tag(0) = MyForm.UniqueID
                If Not ItemDispacher.ioFormTag.ContainsKey(loForm.UniqueID) Then
                    ItemDispacher.ioFormTag.Add(loForm.UniqueID, Tag)
                Else
                    ItemDispacher.ioFormTag.Item(loForm.UniqueID) = Tag
                End If
                '确认子父窗体关系
                If Not ItemDispacher.ioFormSon.ContainsKey(MyForm.UniqueID) Then
                    ItemDispacher.ioFormSon.Add(MyForm.UniqueID, loForm.UniqueID)
                End If

                Dim loobj As TI_Z0150
                loobj = ItemDispacher.ioFormSL.Item(loForm.UniqueID)

                loobj.DYBL()
                'loobj.SetVVS()
                loobj.ioMtx_10.LoadFromDataSource()

            End If
        Catch ex As Exception
            MyApplication.SetStatusBarMessage(ex.Message.ToString(), BoMessageTime.bmt_Short, True)
        End Try
    End Sub


    Private Sub TI_Z0040_MenuEvent(ByVal pVal As SAPbouiCOM.IMenuEvent, ByRef BubbleEvent As Boolean) Handles Me.MenuEvent
        Select Case pVal.MenuUID
            Case "TI_T058"
                If pVal.BeforeAction Then
                    MyForm.Freeze(True)
                    Try
                        ioUds_DateF = MyForm.DataSources.UserDataSources.Item("DateF")
                        ioUds_DateT = MyForm.DataSources.UserDataSources.Item("DateT")
                        ioUds_CardName = MyForm.DataSources.UserDataSources.Item("CardName")
                        ioUds_XSY = MyForm.DataSources.UserDataSources.Item("XSY")
                        ioUds_KF = MyForm.DataSources.UserDataSources.Item("KF")
                        ioUds_ShowHand = MyForm.DataSources.UserDataSources.Item("ShowHand")
                        ioUds_ShowHand.ValueEx = "Y"
                    Catch ex As Exception
                        MyApplication.SetStatusBarMessage(ex.ToString())
                    Finally
                        MyForm.Freeze(False)
                    End Try
                End If
        End Select
    End Sub
End Class