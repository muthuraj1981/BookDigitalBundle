Imports System.Data
Imports System.Windows.Forms
Imports System.IO
Imports MySql.Data.MySqlClient

Public Enum LanstadPathOptions
    NONE = 0
    XMLPATH = 1
    IMAGE = 2
    ASSETPATH = 3
End Enum

Public Class AzureHelper

    Shared AzureSasList As New List(Of AzureCopyData)
    Shared AzureCopyFile As String = "E:\ServerEngine\extra\AzcopyLog.csv"
    Public Sub New()

    End Sub

    Public Shared Sub AzureLog(ByVal Choice As String, ByVal FileName As String, ByVal FileSize As String, ByVal StartTime As String, ByVal EndTime As String, ByVal Diff As String)
        File.AppendAllText(AzureCopyFile, $"""{GBL.AppTitle}"",""{Choice}"",""{FileName}"",""{FileSize}"",""{StartTime}"",""{EndTime}"",""{Diff}""{vbCrLf}")
    End Sub
    Private Shared Function GetAzureData() As Boolean
        Dim Dta As New DataTable
        Dim SqlCon As New MySqlConnection
        Dim SqlAda As New MySqlDataAdapter
        Dim SqlCmd As New MySqlCommand
        AzureSasList.Clear()
        Try
            If (String.IsNullOrEmpty(SqlCon.ConnectionString)) Then
                SqlCon.ConnectionString = GBL.ConnectionString '"Data Source=52.164.222.186; Initial Catalog= engines;Allow Zero Datetime=true;Connect Timeout=120;User ID=engines01; password=JI3E5AbuM1lukO68TA4AVesE8e35So;"
            End If
            If (SqlCon.State = Data.ConnectionState.Closed) Then
                SqlCon.Open()
            End If
            Dim cmd As MySqlCommand = New MySqlCommand("select * from token", SqlCon)
            Dim ada As MySqlDataAdapter = New MySqlDataAdapter(cmd)
            ada.Fill(Dta)
            If ((Dta IsNot Nothing) AndAlso (Dta.Rows IsNot Nothing) AndAlso (Dta.Rows.Count > 0)) Then
                For d As Integer = 0 To Dta.Rows.Count - 1
                    Dim PathID As LanstadPathOptions = DirectCast([Enum].Parse(GetType(LanstadPathOptions), Dta.Rows(d).Item("pathid").ToString()), LanstadPathOptions)
                    AzureSasList.Add(New AzureCopyData With {.SASKey = Dta.Rows(d).Item("sas").ToString(),
                                                            .LanstadPath = Dta.Rows(d).Item("path").ToString(),
                                                            .PathChoice = PathID})
                Next
            End If
            GBL.DeantaBallon($"Azure tokens : {AzureSasList.Count}", MessageType.MSGINFO)
        Catch ex As MySqlException
            GBL.DeantaBallon($"My SQL Error: {ex.Message} - {ex.InnerException.Message} - {ex.Source}", MessageType.MSGERROR)
            Return False
        Finally
            SqlCon.Close()
        End Try
        Return True
    End Function
    Public Shared Function DownloadFile(ByVal AssetFile As String, ByVal LocalPath As String, ByVal PathOptions As LanstadPathOptions, ByVal VXEXMLFile As String) As Boolean
        Dim ServerPath As String = String.Empty
        Dim SaxjanProcessInfo As New ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azcopy"))
        SaxjanProcessInfo.WorkingDirectory = LocalPath
        GBL.DeantaBallon($"Azure Download : {AssetFile} -- {LocalPath}", MessageType.MSGINFO)
        Try
            If (Not GetAzureData()) Then
                GBL.DeantaBallon($"Could not able to collect the data. GetAzureData", MessageType.MSGERROR)
                Return False
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim AzureData As AzureCopyData = (From n In AzureSasList Where n.PathChoice = PathOptions Select n).FirstOrDefault
        If (AzureData Is Nothing) Then
            GBL.DeantaBallon($"Could not able to find the Azure data for Path : {PathOptions}", MessageType.MSGERROR)
            Return False
        End If
        Select Case PathOptions
            Case LanstadPathOptions.XMLPATH
                ServerPath = $"{AzureData.LanstadPath}{Path.GetDirectoryName(VXEXMLFile).Replace("\var\www\html\epublishing\lanstaddoc\", "").Replace("\", "/")}"
            Case LanstadPathOptions.IMAGE
                ServerPath = $"{AzureData.LanstadPath}{Path.GetDirectoryName(VXEXMLFile).Replace("\var\www\html\webdav-server\", "").Replace("\", "/")}"
            Case LanstadPathOptions.ASSETPATH
                ServerPath = $"{AzureData.LanstadPath}resources"
        End Select
        GBL.DeantaBallon($"Azure Download1 : $cp ""{ServerPath}/{AssetFile}{AzureData.SASKey}"" {LocalPath}", MessageType.MSGINFO)
        Try
            Dim StartTime As String = DateTime.Now.ToString()
            SaxjanProcessInfo.Arguments = $"cp ""{ServerPath}/{AssetFile}{AzureData.SASKey}"" {LocalPath}"
            SaxjanProcessInfo.RedirectStandardError = True
            SaxjanProcessInfo.RedirectStandardOutput = True
            SaxjanProcessInfo.CreateNoWindow = True
            SaxjanProcessInfo.WindowStyle = ProcessWindowStyle.Hidden
            SaxjanProcessInfo.UseShellExecute = False
            Dim SaxjanProcess As Process = Process.Start(SaxjanProcessInfo)
            SaxjanProcess.WaitForExit(50000)
            Dim output As String = SaxjanProcess.StandardOutput.ReadToEnd()
            Dim errors As String = SaxjanProcess.StandardError.ReadToEnd()
            Dim EndTime As String = DateTime.Now.ToString()
            GBL.DeantaBallon($"{output} -- Error: {errors}", MessageType.MSGINFO)
            If (File.Exists(Path.Combine(LocalPath, AssetFile))) Then
                AzureLog("Download", AssetFile, (((New FileInfo(Path.Combine(LocalPath, AssetFile)).Length) / 1024) / 1024).ToString(), StartTime, EndTime, DateDiff(DateInterval.Second, Date.Parse(StartTime), Date.Parse(EndTime)).ToString())
                Return True
            End If
        Catch ex As Exception
            GBL.DeantaBallon($"{DownloadFile} - {ex.Message} - {ex.StackTrace.ToString()}", MessageType.MSGERROR)
        End Try
        Return False
    End Function

    Public Shared Function UploadFile(ByVal LocalFile As String, ByVal PathOptions As LanstadPathOptions, ByVal VXEXMLFile As String) As Boolean
        Dim ServerPath As String = String.Empty
        Dim SaxjanProcessInfo As New ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azcopy"))
        SaxjanProcessInfo.WorkingDirectory = Path.GetDirectoryName(LocalFile)
        GBL.DeantaBallon($"Azure Upload : {LocalFile} -- {PathOptions.ToString()}", MessageType.MSGINFO)
        Try
            If (Not GetAzureData()) Then
                GBL.DeantaBallon($"Could not able to collect the data. GetAzureData", MessageType.MSGERROR)
                Return False
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Dim AzureData As AzureCopyData = (From n In AzureSasList Where n.PathChoice = PathOptions Select n).FirstOrDefault
        If (AzureData Is Nothing) Then
            GBL.DeantaBallon($"Could not able to find the Azure data for Path : {PathOptions}", MessageType.MSGERROR)
            Return False
        End If
        Select Case PathOptions
            Case LanstadPathOptions.XMLPATH
                ServerPath = $"{AzureData.LanstadPath}{Path.GetDirectoryName(VXEXMLFile).Replace("\var\www\html\epublishing\lanstaddoc\", "").Replace("\", "/")}"
            Case LanstadPathOptions.IMAGE
                ServerPath = $"{AzureData.LanstadPath}{Path.GetDirectoryName(VXEXMLFile).Replace("\var\www\html\webdav-server\", "").Replace("\", "/")}"
            Case LanstadPathOptions.ASSETPATH
                ServerPath = $"{AzureData.LanstadPath}resources/"
        End Select
        Try
            Dim StartTime As String = DateTime.Now.ToString()
            SaxjanProcessInfo.Arguments = $"cp {LocalFile} {ServerPath}{AzureData.SASKey}"
            SaxjanProcessInfo.RedirectStandardError = True
            SaxjanProcessInfo.RedirectStandardOutput = True
            SaxjanProcessInfo.CreateNoWindow = True
            SaxjanProcessInfo.WindowStyle = ProcessWindowStyle.Hidden
            SaxjanProcessInfo.UseShellExecute = False
            Dim SaxjanProcess As Process = Process.Start(SaxjanProcessInfo)
            SaxjanProcess.WaitForExit(50000)
            Dim output As String = SaxjanProcess.StandardOutput.ReadToEnd()
            Dim errors As String = SaxjanProcess.StandardError.ReadToEnd()
            Dim EndTime As String = DateTime.Now.ToString()
            GBL.DeantaBallon($"Azure Log: {output}", MessageType.MSGERROR)
            AzureLog("Upload", Path.GetFileName(LocalFile), (((New FileInfo(LocalFile).Length) / 1024) / 1024).ToString(), StartTime, EndTime, DateDiff(DateInterval.Second, Date.Parse(StartTime), Date.Parse(EndTime)).ToString())
        Catch ex As Exception
            GBL.DeantaBallon($"UploadFile - {ex.Message} -- {ex.StackTrace.ToString()}", MessageType.MSGERROR)
        End Try
        Return True
    End Function

End Class

Public Class AzureCopyData
    Public Property SASKey As String = String.Empty
    Public Property LanstadPath As String = String.Empty
    Public Property PathChoice As LanstadPathOptions = LanstadPathOptions.NONE

End Class