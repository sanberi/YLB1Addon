Option Strict Off
Option Explicit On 

Module MessageAPIs

    '//  SAP MANAGE UI API 6.5 SDK Sample
    '//****************************************************************************
    '//
    '//  File:      MessageAPIs.bas
    '//
    '//  Copyright (c) SAP MANAGE
    '//
    '// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
    '// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
    '// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    '// PARTICULAR PURPOSE.
    '//
    '//****************************************************************************

    '//****************************************************************
    '// API Declarations
    '// Enable us to process a message loop in Sub Main()
    '//
    '// A developer should copy this module 'as is' and create
    '// an object of your class in Sub Main()
    '//****************************************************************

    '// Part of the MSG structure - receives the location of the mouse
#Region "Win32 API"
    '// Coredll,kernel32
    <Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="OpenClipboard", _
    SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
    Public Function Open_Clipboard(ByVal hWnd As IntPtr) As Boolean
    End Function

    '<Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="EmptyClipboard", _
    'SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
    '    Public Shared Function Empty_Clipboard() As Boolean
    'End Function

    <Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="SetClipboardData", _
    SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
    Public Function SetClipboard_Data(ByVal uFormat As Integer, ByVal hWnd As IntPtr) As IntPtr
    End Function

    <Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="CloseClipboard", _
    SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
        Public Function Close_Clipboard() As Boolean
    End Function

    <Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="GetClipboardData", _
    SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
        Public Function Get_ClipboardData(ByVal uFormat As Integer) As IntPtr
    End Function

    '<Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="IsClipboardFormatAvailable", _
    'SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
    'Public Shared Function IsClipboardFormatAvailable(ByVal uFormat As Integer) As Boolean
    'End Function

    '<Runtime.InteropServices.DllImport("USER32.dll", EntryPoint:="LocalAlloc", _
    'SetLastError:=True, CallingConvention:=System.Runtime.InteropServices.CallingConvention.Winapi)> _
    'Public Shared Function LocalAlloc(ByVal uFlags As Integer, ByVal uBytes As Integer) As IntPtr
    'End Function


#End Region
    Public Structure POINTAPI
        Dim X As Integer
        Dim Y As Integer
    End Structure

    '// The message structure
    Public Structure Msg
        Dim hwnd As Integer
        Dim message As Integer
        Dim wParam As Integer
        Dim lParam As Integer
        Dim Time As Integer
        Dim pt As POINTAPI
    End Structure

    '// Will get us out of our message loop
    Public Declare Sub PostQuitMessage Lib "user32" (ByVal nExitCode As Integer)

    '// Retrieves messages sent to the calling thread's message queue
    'UPGRADE_WARNING: 结构 Msg 可能要求封送处理属性作为此声明语句中的参数传递。 单击以获得更多信息:“ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1050"”
    Public Declare Function GetMessage Lib "user32" Alias "GetMessageA" (ByRef lpMsg As Msg, ByVal hwnd As Integer, ByVal wMsgFilterMin As Integer, ByVal wMsgFilterMax As Integer) As Integer


    '// Translates virtual-key messages into character messages
    'UPGRADE_WARNING: 结构 Msg 可能要求封送处理属性作为此声明语句中的参数传递。 单击以获得更多信息:“ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1050"”
    Public Declare Function TranslateMessage Lib "user32" (ByRef lpMsg As Msg) As Integer



    '// Forwards the message on to the window represented by the
    '// hWnd member of the Msg structure
    'UPGRADE_WARNING: 结构 Msg 可能要求封送处理属性作为此声明语句中的参数传递。 单击以获得更多信息:“ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1050"”
    Public Declare Function DispatchMessage Lib "user32" Alias "DispatchMessageA" (ByRef lpMsg As Msg) As Integer

    'UPGRADE_NOTE: Msg 已升级到 Msg_Renamed。 单击以获得更多信息:“ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1061"”
    Public Msg_Renamed As Msg

    '//定义报表类型
    Public Enum ReportTypes
        ReportZC = -1
        ReportSY = -3
    End Enum
End Module