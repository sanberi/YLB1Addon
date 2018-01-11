Imports System.Collections.Generic
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Public Class CmpFileDialog
    'ファイルダイアログスレッドの生成
    Public m_objThread As Thread
    'パスの初期状態
    Public m_strInit As String
    '説明
    Public m_strDescription As String
    '選択結果
    Public m_strSelected As String
    'フィルター
    Public m_strFilter As String
    '
    Public m_strDialogType As String = "File"
    Public Sub New()
        m_objThread = New Thread(AddressOf Me.RunBrowser)
        m_objThread.SetApartmentState(ApartmentState.STA)
    End Sub
    Public Function ShowFolderDialog() As String
        m_strDialogType = "Folder"

        m_objThread.Start()

        '終了を待つ
        m_objThread.Join()
        Return m_strSelected
    End Function
    ''---------------------------------------------------------------------------
    '' ファイルダイアログを表示する
    '' <処理> 
    '' <引数> なし
    '' <戻値> 選択された文字列
    ''---------------------------------------------------------------------------
    Public Function ShowFileDialog() As String
        'm_strDialogType = "File"

        m_objThread.Start()

        ''終了を待つ
        m_objThread.Join()

        Return m_strSelected
    End Function
    ''Thread.runメソッド
    Private Sub RunBrowser()
        Dim objWindow As WindowWrapper = GetWindowWrapper()
        If Me.m_strDialogType = "Folder" Then
            ''フォルダダイアログ
            Dim objFolderDialog As New FolderBrowserDialog()
            ''ルートフォルダはデフォルト（ディスクトップ）とする。
            objFolderDialog.SelectedPath = "C:\"
            objFolderDialog.Description = m_strDescription
            objFolderDialog.ShowDialog(objWindow)
            Me.m_strSelected = objFolderDialog.SelectedPath
        ElseIf Me.m_strDialogType = "File" Then
            ''ファイルダイアログ
            Dim objFileDialog As New OpenFileDialog()
            objFileDialog.Filter = m_strFilter
            objFileDialog.DefaultExt = "xls"
            objFileDialog.Title = "请选择要导入的excel文件"
            objFileDialog.ShowDialog(objWindow)
            Me.m_strSelected = objFileDialog.FileName
        End If
    End Sub
    Private Function GetWindowWrapper() As WindowWrapper
        Dim MyProcs As System.Diagnostics.Process()
        Dim appDomain__1 As System.AppDomain = AppDomain.CurrentDomain

        MyProcs = System.Diagnostics.Process.GetProcessesByName("SAP Business One")

        ''SBOのプロセスを特定するが、複数起動している場合は一番最初のプロセスについて
        ''処理をする。そのため挙動が不審となる。
        If MyProcs.Length <> 0 Then
            For s As Integer = 0 To MyProcs.Length - 1
                Dim MyWindow As New WindowWrapper(MyProcs(s).MainWindowHandle)
                Return MyWindow
            Next
        End If

        Return Nothing
    End Function
    Private Class WindowWrapper
        Implements System.Windows.Forms.IWin32Window
        'Implements System.Windows.Forms.IWin32Window

        Private _hwnd As IntPtr

        Public Sub New(ByVal handle As IntPtr)
            _hwnd = handle
        End Sub

        Public ReadOnly Property Handle() As System.IntPtr Implements IWin32Window.Handle
            Get
                Return _hwnd
            End Get
        End Property
    End Class
End Class
