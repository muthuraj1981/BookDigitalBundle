Imports System.Web
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.IO
Imports System.Reflection

Public Class GBL

    Public Shared ReleaseMethod As ToolReleaseType = ToolReleaseType.NONE
    Public Shared Property ToolType As DigitialToolType = Server_DigitalBundle.DigitialToolType.NONE
    Public Shared LogFilePath As String = String.Empty
    Public Shared Property HostName As String = String.Empty
    Public Shared Property UserName As String = String.Empty
    Public Shared Property Password As String = String.Empty
    Public Shared Property DBDataList As New List(Of DigitalBundleData)
    Public Shared Property Worker As New BackgroundWorker
    Public Shared Property LocalCopyList As New List(Of LocalResourceData)

    Public Shared ReadOnly Property LocalResourcePath As String
        Get
            Return ""
        End Get
    End Property

    Public Shared ReadOnly Property GetIndianTime As DateTime
        Get
            Dim INDIAN_ZONE As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
            Dim indianTime As DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE)
            Return indianTime
        End Get
    End Property

    Public Shared ReadOnly Property CurrentWorkingPath As String
        Get
#If CONFIG = "Debug" Or CONFIG = "TEST" Or CONFIG = "FinalXML" Then
            Return "D:\InDesignEngine"
#Else
            Return "E:\InDesignEngine"
#End If

        End Get
    End Property

    Public Shared ReadOnly Property InDesignServerSoap As String
        Get
            Return Path.Combine(AppPath, "SampleClient.exe")
        End Get
    End Property

    Public Shared ReadOnly Property BookTemplate As String
        Get
            Return "E:\ServerEngine\Templates\Book"
        End Get
    End Property

    Public Shared ReadOnly Property JournalTemplate As String
        Get
            Return "E:\ServerEngine\Templates\Journal"
        End Get
    End Property


    Public Shared Sub UpdateGridStatus(ByVal index As Integer, ByVal Status As String)
        If (index <> -1) Then
            GBL.DBDataList(index).CurrentStatus = Status
        End If
        GBL.Worker.ReportProgress(1)
    End Sub

    Public Shared ReadOnly Property GetPdfTime() As String
        Get
            'Dim INDIAN_ZONE As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
            'Dim indianTime As DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE)
            'Return indianTime.ToString("yyyy-MM-dd HH:mm:ss")
            Return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        End Get
    End Property

    Public Shared ReadOnly Property BloomsburyLogo As String
        Get
            Return "E:\ServerEngine\extra\Logos\Blooms\Bloomsbury.tif"
        End Get
    End Property

    Public Shared ReadOnly Property FTPResourcePath As String
        Get
            'Return "/var/www/epublishing/epublishing/resources"
            Return "/data/storage/epublishing/resources"
        End Get
    End Property

    Public Shared ReadOnly Property FTPBookPDFPath As String
        Get
            Return "/var/www/html/epublishing/wordplugins/bookpdf"
        End Get
    End Property

    Public Shared ReadOnly Property TNBitConversionXSL As String
        Get
            Return Path.Combine(AppPath, "TFB-2-BITS.xsl")
        End Get
    End Property

    Public Shared Property OutputDataList As New List(Of DigitalOutputData)

    Public Shared ReadOnly Property TNBitXSLPath As String
        Get
            Return Path.Combine(AppPath, "xsl")
        End Get
    End Property
    Public Shared ReadOnly Property UploadLogFile As String
        Get
            Return Path.Combine(AppPath, "DB_Upload_Log.csv")
        End Get
    End Property

    Public Shared Sub DbUploadLog(ByVal FileName As String, ByVal FileSize As Long, ByVal StartTime As String, ByVal EndTime As String, ByVal Diff As String)
        File.AppendAllText(UploadLogFile, $"""{FileName}"",""{FileSize}"",""{StartTime}"",""{EndTime}"",""{Diff}""{vbCrLf}")
    End Sub

    Public Shared ReadOnly Property AppPath As String
        Get
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Or CONFIG = "TEST" Then
            Return "\\FSDEANTA\TechRelease\Accounts\Common\InDesignServerEngine\extra\"
#ElseIf CONFIG = "TESTING" Then
            Return "\\FSDEANTA\TechRelease\Accounts\Common\InDesignServerEngine\extra\"
#ElseIf CONFIG = "Release" Then
#If (ISLOCAL) Then
            Return "D:\DeantaTools\Lanstad\extra"
#Else
            'Return "\\FSDEANTA\TechRelease\Accounts\Common\InDesignServerEngine\extra\"
            Return "E:\ServerEngine\extra\DigitalBundle"
#End If
#End If
        End Get
    End Property

    Public Shared ReadOnly Property JobOptionPath As String
        Get
            Return Path.Combine(Path.GetDirectoryName(AppPath), "joboptions/")
        End Get
    End Property

    Public Shared ReadOnly Property PrintPresetPath As String
        Get
            Return Path.Combine(Path.GetDirectoryName(AppPath), "PrintPresets/")
        End Get
    End Property

    Public Shared ReadOnly Property ConnectionString As String
        Get
            If (ReleaseMethod = ToolReleaseType.DARRANLAPTOP) Then
                Return "Data Source=localhost;port=3306; Initial Catalog=journals; User ID=root;"
            ElseIf (ReleaseMethod = ToolReleaseType.LANSTANDLIVE) Then
                'Return "Data Source=78.137.168.30; Initial Catalog= journals_live; User ID=root; password=XKv4rdKRAGnWR3Ut;"
                Return "Data Source=78.137.168.31; Initial Catalog= journals_live;Allow Zero Datetime=true;User ID=appservdb; password=b5YlZxKFN1@gPxVMg;"
            ElseIf (ReleaseMethod = ToolReleaseType.LANSTANDSTG) Then
                Return "Data Source=92.51.243.215; Initial Catalog=journals; User ID=root; Password=admin123;"
            ElseIf (ReleaseMethod = ToolReleaseType.AZUREDB) Then
                'Return "Data Source=52.164.222.186; Initial Catalog=journals_live; User ID=devopslive;Connect Timeout=60;Allow Zero Datetime=true;Password=B2=l9rlrisPuW+qapL=w;"
                Return "Data Source='production-lanstad-database.mysql.database.azure.com';port=3306;Allow Zero Datetime=true;Initial Catalog='journals_live';User ID='indesign_prod'; password='.)<G^c;dR4';"
            ElseIf (ReleaseMethod = ToolReleaseType.AZURESTGDB) Then
                Return "Data Source=52.164.222.186; Initial Catalog=staging_lanstad; User ID=devstaging;Connect Timeout=60;Allow Zero Datetime=true;Password=DGd4t4b4s4St4g1ngUs3r;"
            Else
                Return "Data Source=92.51.243.215; Initial Catalog=journals; User ID=root; Password=admin123;"
            End If
        End Get
    End Property

    Public Shared ReadOnly Property AppTitle() As String
        Get
            Dim Assm As System.Reflection.Assembly = Assembly.GetExecutingAssembly()
            Return String.Format("::: {0}_{1}.{2} :::", Assm.GetName().Name, Assm.GetName.Version.Major, Assm.GetName.Version.Minor)
        End Get
    End Property

    Public Shared ReadOnly Property MSSQLConnectionString As String
        Get
            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property InDesignServerScript As String
        Get
            Return Path.Combine(AppPath, "BookExportWebPDF.jsx")
        End Get
    End Property

    Public Shared ReadOnly Property GSPreviewScript As String
        Get
            Return Path.Combine(AppPath, "BookGSPreviewPDF.jsx")
        End Get
    End Property

    Public Shared ReadOnly Property TNFMetadataXML As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TNF_Metadata.xml")
        End Get
    End Property

    Public Shared ReadOnly Property PsWatchPath As String
        Get
            Return "E:\PSWatcher"
        End Get
    End Property
    Public Shared ReadOnly Property ExportXMLScript As String
        Get
            Return Path.Combine(AppPath, "ExportXML.jsx")
        End Get
    End Property

    Public Shared ReadOnly Property SupportMailID() As String
        Get
            Return "techsupport@deantaglobal.com"
        End Get
    End Property

    Public Shared Function HtmlEncode(text As String) As String
        Dim chars As Char() = HttpUtility.HtmlEncode(text).ToCharArray()
        Dim result As New StringBuilder(text.Length + CInt(text.Length * 0.1))
        Dim Input As String = String.Empty
        For Each c As Char In chars
            Dim value As Integer = Convert.ToInt32(c)
            If value > 127 Then
                result.AppendFormat("&#x{0};", Hex(value).PadLeft(4, "0"))
            Else
                result.Append(c)
            End If
        Next
        Input = result.ToString().Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", """")
        For Each Dit As System.Text.RegularExpressions.Match In Regex.Matches(result.ToString(), "(&#)([0-9]{1,3})(;)")
            If (Dit.Success) Then
                Input = Input.Replace(Dit.Value, String.Format("{0}x{1}{2}", Dit.Groups(1).Value, Hex(Dit.Groups(2).Value).PadLeft(4, "0000"), Dit.Groups(3).Value))
            End If
        Next
        'Input = Regex.Replace(result.ToString(), "(?<front>&#)(?<num>[0-9]{1,3};)", "${front}x0${num}")
        Return Input
    End Function

    Public Shared Sub DeantaBallon(Msg As String, MsgType As MessageType)
        File.AppendAllText(LogFilePath, $"{vbNewLine}{DateTime.Now} - {Msg.Replace("<table>", "table")}")
    End Sub

    Public Shared Function GetFileFromFolder(ByVal Folder As String, ByVal Pattern As String) As List(Of String)
        Dim FileList As New List(Of String)
        If (Not Directory.Exists(Folder)) Then
            Return Nothing
        End If
        FileList.AddRange(Directory.GetFiles(Folder, Pattern, SearchOption.TopDirectoryOnly))
        If ((FileList IsNot Nothing) AndAlso (FileList.Count > 0)) Then
            Return FileList
        End If
        Return Nothing
    End Function

End Class

