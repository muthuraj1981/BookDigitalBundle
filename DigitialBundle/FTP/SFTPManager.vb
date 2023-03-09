Imports System.ComponentModel
Imports Renci.SshNet
Imports System.Threading
Imports System.IO

Public Class UploadStatusArgs
    Inherits EventArgs

    Public Property Percentage As Double = 0
    Public Property Index As Integer = 0

End Class


Public Class DownloadStatusArgs
    Inherits EventArgs

    Public Property Percentage As Double = 0
    Public Property Index As Integer = 0
    Public Property Worker As BackgroundWorker

End Class

Public Class DownloadUploadArgs

    Public Property FtpStatus As FtpStatusType = FtpStatusType.NONE
    Public Property LocalPath As String = String.Empty
    Public Property UrlPath As String = String.Empty
    Public Property Extension As String = String.Empty
    Public Property Index As Integer = 0
    Public Property NeededFileList As New List(Of String)
    Public Property IsDownloadLocalResource As Boolean = False
End Class

Public Class ReportProgressArgs

    Public Property Index As Integer = 0
    Public Property Percentage As Integer = 0

End Class