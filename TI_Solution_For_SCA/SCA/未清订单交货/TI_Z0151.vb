Option Strict Off
Option Explicit On
Imports SAPbouiCOM
Imports System.Runtime.InteropServices
Imports System.IO

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