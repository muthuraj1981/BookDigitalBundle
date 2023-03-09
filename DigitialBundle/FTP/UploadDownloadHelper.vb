Imports System.ComponentModel
Imports Renci.SshNet
Imports System.Threading
Imports System.IO
Public Class UploadDownloadHelper

    Dim SftpClient As SftpClient
    Dim DownUploadArgs As New DownloadUploadArgs
    Public IsLocalCopy As Boolean = False
    Public IsSuccess As Boolean = False

    Public Event ProgressChanged(ByVal ProVal As Int16)

    Public Sub New()
        Me.DownUploadArgs = Nothing
        Me.IsLocalCopy = False
    End Sub
    Public Sub New(ByVal Args As DownloadUploadArgs, Optional ByVal IsLocal As Boolean = False)
        Me.DownUploadArgs = Args

        Me.IsLocalCopy = IsLocal

        If (Me.IsLocalCopy) Then
            Me.DownUploadArgs.UrlPath = Me.DownUploadArgs.UrlPath.Replace("/", "\")
            Me.DownUploadArgs.LocalPath = Me.DownUploadArgs.LocalPath.Replace("/", "\")
        End If

        If (Me.DownUploadArgs.FtpStatus = FtpStatusType.DOWNLOAD) Then
            'Me.Text = "Ftp Downloading Status"
        ElseIf (Me.DownUploadArgs.FtpStatus = FtpStatusType.UPLOAD) Then
            'Me.Text = "Ftp Uploading Status"
        End If

    End Sub
    Public Sub DoUploadDownload()
        Dim Attempt As Integer = 0
        If (Not Me.IsLocalCopy) Then
            GBL.DeantaBallon($"UserName: {GBL.UserName} - IP: {GBL.HostName} - {Me.DownUploadArgs.UrlPath}", MessageType.MSGINFO)
            Do
                If (Attempt <> 0) Then Threading.Thread.Sleep(5000)
                If (Attempt = 50) Then Exit Do
                Dim method As New PasswordAuthenticationMethod(GBL.UserName, GBL.Password)
                GBL.DeantaBallon($"SFTP Connection Attempt {Attempt}", MessageType.MSGERROR)
                Dim conInfo As New ConnectionInfo(GBL.HostName, 22, GBL.UserName, method)
                conInfo.Timeout = TimeSpan.FromMinutes(2)
                Try
                    SftpClient = New SftpClient(conInfo)
                    SftpClient.Connect()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                Attempt = Attempt + 1
            Loop While (Not SftpClient.IsConnected)
        End If
        Worker_Dowork()
    End Sub

    Private Sub Worker_Dowork()
        Dim FtpArgs As DownloadUploadArgs = Me.DownUploadArgs
        Dim FileCnt As Integer = 1
        Select Case FtpArgs.FtpStatus
            Case FtpStatusType.DOWNLOAD
                Dim Percentage As Double = 0D
                If (Me.IsLocalCopy) Then
                    If (IsFileOrDirectoryExists(Me.DownUploadArgs.UrlPath)) Then
                        If ((File.GetAttributes(FtpArgs.UrlPath) And FileAttribute.Directory) = FileAttribute.Directory) Then
                            Dim NewDir As String = Path.Combine(FtpArgs.LocalPath, Path.GetFileName(FtpArgs.UrlPath))
                            If (Not Directory.Exists(NewDir)) Then
                                Directory.CreateDirectory(NewDir)
                            End If
                            Dim TotalCount As Integer = 0
                            TotalCount = Directory.GetFiles(FtpArgs.UrlPath).Count - 1
                            For Each InnerFile As String In Directory.GetFiles(FtpArgs.UrlPath)
                                If ((String.Compare(Path.GetFileName(InnerFile), "..", True) <> 0) And (String.Compare(Path.GetFileName(InnerFile), ".", True) <> 0)) Then
                                    LocalDownloadFile(New DownloadUploadArgs With {.UrlPath = InnerFile, .LocalPath = NewDir, .Index = FtpArgs.Index, .FtpStatus = 0})
                                    'FtpDownloadFile(New DownloadUploadArgs With {.UrlPath = InnerFile, .LocalPath = NewDir, .Index = FtpArgs.Index, .FtpStatus = 0})
                                    FileCnt += 1
                                End If
                            Next
                        Else
                            LocalDownloadFile(FtpArgs)
                        End If
                    End If
                Else
                    If (SftpClient.Exists(Me.DownUploadArgs.UrlPath)) Then
                        If (SftpClient.GetAttributes(FtpArgs.UrlPath).IsDirectory) Then
                            Dim NewDir As String = Path.Combine(FtpArgs.LocalPath, Path.GetFileName(FtpArgs.UrlPath))
                            If (Not Directory.Exists(NewDir)) Then
                                Directory.CreateDirectory(NewDir)
                            End If
                            Dim TotalCount As Integer = 0
                            TotalCount = SftpClient.ListDirectory(FtpArgs.UrlPath).Count
                            TotalCount = IIf(TotalCount > 3, TotalCount - 2, TotalCount)
                            For Each InnerFile As Sftp.SftpFile In SftpClient.ListDirectory(FtpArgs.UrlPath)
                                If ((String.Compare(Path.GetFileName(InnerFile.FullName), "..", True) <> 0) And (String.Compare(Path.GetFileName(InnerFile.FullName), ".", True) <> 0)) Then
                                    If (Not IsNeededFile(InnerFile.Name)) Then Continue For
                                    If (FtpArgs.IsDownloadLocalResource) Then
                                        CopyLocalResourceFile(New DownloadUploadArgs With {.UrlPath = InnerFile.FullName, .LocalPath = NewDir, .Index = FtpArgs.Index, .FtpStatus = 0})
                                    Else
                                        FtpDownloadFile(New DownloadUploadArgs With {.UrlPath = InnerFile.FullName, .LocalPath = NewDir, .Index = FtpArgs.Index, .FtpStatus = 0})
                                    End If
                                    FileCnt += 1
                                End If
                            Next
                        Else
                            FtpDownloadFile(FtpArgs)
                        End If
                    Else
                        GBL.DeantaBallon("File not exists:" & FtpArgs.UrlPath, MessageType.MSGERROR)
                    End If
                End If
            Case FtpStatusType.UPLOAD
                If (Me.IsLocalCopy) Then
                    LocalUploadFile(New DownloadUploadArgs With {.UrlPath = FtpArgs.UrlPath, .LocalPath = FtpArgs.LocalPath, .Index = FtpArgs.Index, .FtpStatus = 0})
                Else
                    UploadFile(FtpArgs.UrlPath, FtpArgs.LocalPath)
                End If
            Case FtpStatusType.UPLOADALLFILES
                If (SftpClient.Exists(Me.DownUploadArgs.UrlPath)) Then
                    Dim UploadFileList As New List(Of String)
                    UploadFileList.AddRange(Directory.GetFiles(FtpArgs.LocalPath, "*.*", SearchOption.TopDirectoryOnly))
                    For f As Integer = 0 To UploadFileList.Count - 1
                        Dim UpFile As String = UploadFileList(f)
                        If (From n In FtpArgs.NeededFileList Where (String.Compare(Path.GetFileName(n), Path.GetFileName(UpFile), True) = 0) Select n).Any Then
                            UploadFile(FtpArgs.UrlPath, UpFile)
                        End If
                    Next
                End If
        End Select
    End Sub

    Private Sub DisconnectSftp()
        If ((SftpClient IsNot Nothing) AndAlso (SftpClient.IsConnected)) Then
            SftpClient.Disconnect()
        End If
    End Sub

    Private Function CopyLocalResourceFile(FtpArgs As DownloadUploadArgs) As Boolean
        Dim DownloadFile As String = String.Empty
        Dim LandData As DigitalBundleData = GBL.DBDataList(FtpArgs.Index)
        Dim DocumentResourcePath As String = String.Empty
        DownloadFile = Path.Combine(FtpArgs.LocalPath, Path.GetFileName(FtpArgs.UrlPath))
        Dim ResourceName As String = Path.GetFileName(FtpArgs.UrlPath)
        'DocumentResourcePath = MySqlHelper.ExecuteScalar("select document_path from tb_documents where document_name = '" & ResourceName & "' and project_id=" & LanstadData.ProjectID & " and task_id=" & LanstadData.TaskID & "")
        DocumentResourcePath = MySqlHelper.ExecuteScalar("select document_path from tb_documents where document_name = '" & ResourceName & "' and project_id=" & LandData.ProjectID & " ")
        Dim TmpResource As String = String.Empty
        If (String.IsNullOrEmpty(DocumentResourcePath)) Then
            GBL.DeantaBallon("Could not able to find the resource name in tb_documents:" & ResourceName, MessageType.MSGERROR)
            Return False
        End If
        TmpResource = Path.Combine("\\192.168.1.51\Lanstad\" & DocumentResourcePath.Replace("resources/", ""))
        If (Not File.Exists(TmpResource)) Then
            GBL.DeantaBallon("File not found in the local resource path :" & TmpResource, MessageType.MSGERROR)
            Return False
        End If
        Try
            File.Copy(TmpResource, DownloadFile, True)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function IsNeededFile(ByVal CurrentFile As String) As Boolean
        Dim IsValid As Boolean = False
        If (String.Compare(Path.GetExtension(CurrentFile), ".jpg", True) = 0) Then Return False
        If ((Me.DownUploadArgs.NeededFileList Is Nothing) OrElse (Me.DownUploadArgs.NeededFileList.Count = 0)) Then Return False
        IsValid = (From n In Me.DownUploadArgs.NeededFileList Where (String.Compare(n, Path.GetFileNameWithoutExtension(CurrentFile), True) = 0) Select n).Any
        Return IsValid
    End Function

    Private Function IsFileOrDirectoryExists(ByVal DirPath As String) As Boolean
        Dim IsExists As Boolean = False
        If ((File.GetAttributes(DirPath) And FileAttribute.Directory) = FileAttribute.Directory) Then
            If (Directory.Exists(DirPath)) Then
                IsExists = True
            Else
                IsExists = False
            End If
        ElseIf ((File.GetAttributes(DirPath) And FileAttribute.Directory) <> FileAttribute.Directory) Then
            If (File.Exists(DirPath)) Then
                IsExists = True
            Else
                IsExists = False
            End If
        End If
        Return IsExists
    End Function

    Private Sub LocalDownloadFile(FtpArgs As DownloadUploadArgs)
        Dim brReader As System.IO.BinaryReader = Nothing
        Dim brWriter As System.IO.BinaryWriter = Nothing
        Dim input As New FileStream(FtpArgs.UrlPath, FileMode.Open, FileAccess.Read)
        Dim output As New FileStream(Path.Combine(FtpArgs.LocalPath, Path.GetFileName(FtpArgs.UrlPath)), FileMode.Create, FileAccess.Write)
        Try
            brReader = New System.IO.BinaryReader(input)
            brWriter = New System.IO.BinaryWriter(output)
            Dim FileLen As Long = My.Computer.FileSystem.GetFileInfo(FtpArgs.UrlPath).Length
            Dim count As Integer = 100 * 1048576 ' this is buffer size
            Dim buffer(count) As Byte
            Dim bytesRead As Integer
            While FileLen > 0
                bytesRead = brReader.Read(buffer, 0, count)
                If bytesRead = 0 Then ' 0 means end of file reached
                    Exit While
                End If
                brWriter.Write(buffer, 0, bytesRead)
                FileLen = FileLen - bytesRead
            End While
        Catch ex As Exception
        Finally
            brReader.Close()
            brWriter.Close()
            input.Close()
            output.Close()
        End Try
    End Sub

    Private Sub LocalUploadFile(FtpArgs As DownloadUploadArgs)
        Dim brReader As System.IO.BinaryReader = Nothing
        Dim brWriter As System.IO.BinaryWriter = Nothing
        Dim input As New FileStream(FtpArgs.LocalPath, FileMode.Open, FileAccess.Read)
        Dim output As New FileStream(Path.Combine(FtpArgs.UrlPath, Path.GetFileName(FtpArgs.LocalPath)), FileMode.Create, FileAccess.Write)
        Try
            brReader = New System.IO.BinaryReader(input)
            brWriter = New System.IO.BinaryWriter(output)
            Dim FileLen As Long = My.Computer.FileSystem.GetFileInfo(FtpArgs.LocalPath).Length
            Dim count As Integer = 100 * 1048576 ' this is buffer size
            Dim buffer(count) As Byte
            Dim bytesRead As Integer
            While FileLen > 0
                bytesRead = brReader.Read(buffer, 0, count)
                If bytesRead = 0 Then ' 0 means end of file reached
                    Exit While
                End If
                brWriter.Write(buffer, 0, bytesRead)
                FileLen = FileLen - bytesRead
            End While
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        Finally
            brReader.Close()
            brWriter.Close()
            input.Close()
            output.Close()
        End Try
    End Sub

    Private Sub FtpDownloadFile(FtpArgs As DownloadUploadArgs)
        Try
            Dim WaitHandleList As New List(Of WaitHandle)
            Dim Percentage As Double = 0
            Dim DownloadFile As String = String.Empty
            DownloadFile = Path.Combine(FtpArgs.LocalPath, Path.GetFileName(FtpArgs.UrlPath))
            If (Not SftpClient.IsConnected) Then
                SftpClient.Connect()
            End If
            Dim DownloadStream As New FileStream(DownloadFile, FileMode.Create, FileAccess.Write)
            Dim DownloadAsyc As Sftp.SftpDownloadAsyncResult = SftpClient.BeginDownloadFile(FtpArgs.UrlPath, DownloadStream)
            WaitHandleList.Add(DownloadAsyc.AsyncWaitHandle)
            Dim IsDownloadCompleted As Boolean = False
            Dim FileSizeAttrib As Sftp.SftpFileAttributes = SftpClient.GetAttributes(FtpArgs.UrlPath)
            Dim FileSize As Double = FileSizeAttrib.Size
            While (Not IsDownloadCompleted)
                IsDownloadCompleted = True
                If (Not DownloadAsyc.IsCompleted) Then
                    IsDownloadCompleted = False
                End If
                Percentage = ((DownloadAsyc.DownloadedBytes / FileSize) * 100)
                If (Percentage >= 100) Then
                    IsDownloadCompleted = True
                End If
                RaiseEvent ProgressChanged(Percentage)
            End While
            DownloadStream.Flush()
            DownloadStream.Close()
            IsSuccess = True
        Catch ex As Exception
            GBL.DeantaBallon("FtpDownload: " & ex.Message, MessageType.MSGERROR)
        End Try
    End Sub

    Public Sub UploadFile(UploadUrl As String, LocalFile As String)
        Try
            Dim Percentage As Double = 0D
            Dim UploadFile As String = String.Empty
            UploadFile = String.Format("{0}/{1}", UploadUrl, Path.GetFileName(LocalFile).TrimStart("_"))
            Dim UploadStream As New FileStream(LocalFile, FileMode.Open, FileAccess.Read)
            Dim WaitHandleList As New List(Of WaitHandle)
            SftpClient.ChangeDirectory(UploadUrl)
            SftpClient.BufferSize = 4 * 1024
            Dim UploadAsyc As Sftp.SftpUploadAsyncResult = SftpClient.BeginUploadFile(UploadStream, UploadFile)
            WaitHandleList.Add(UploadAsyc.AsyncWaitHandle)
            Dim IsUploadCompleted As Boolean = False
            Dim FileSize As Double = UploadStream.Length
            While (Not IsUploadCompleted)
                IsUploadCompleted = True
                If (Not UploadAsyc.IsCompleted) Then
                    IsUploadCompleted = False
                End If
                Percentage = ((UploadAsyc.UploadedBytes / FileSize) * 100)
                Thread.Sleep(500)
            End While
            UploadStream.Flush()
            UploadStream.Close()
            IsSuccess = True
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
    End Sub

End Class
