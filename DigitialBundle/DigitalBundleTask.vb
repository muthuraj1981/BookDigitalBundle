Imports System.IO

Public Enum DigitalBundleTask
    NONE = 0
    EXPORTXML = 1
    BOOKPDF = 2
    COMBINEDRTF = 3
    POD = 4
    PACKAGING = 5
    EPUB = 6
    MOBI = 7
    ZIP = 9
    PRC = 10
    TFXML = 11
    COVER = 12
    WEBPDF = 13
    PREVIERPDF = 14
    APPLICATION = 15
    IMAGES = 16
    XMLIMAGE = 17
    XMLPDF = 18
    LSPDF = 19
    PRINTPDF = 20
    MATHTYPE = 21
    GSPREIVEW = 22
End Enum

Public Enum NodeMoveOption
    NONE = 0
    MOVEAFTER = 1
    MOVEBEFORE = 2
    EMPTYTEXT = 3
    FIRSTCHILD = 4
    LASTCHILD = 5
    TEXTCONTAINS = 6
    ENDSWITH = 7
End Enum

Public Class DBFolderStructure

    Public Property WorkingPath As String = String.Empty
    Public Property HardbackNum As String = String.Empty
    Public Property PaperbackNum As String = String.Empty
    Public Property TaskID As String = String.Empty
    Public Property DigitalID As String = String.Empty

    Public Sub New(ByVal TaskID As String, DigitalID As String)
        Me.TaskID = TaskID
        Me.DigitalID = DigitalID
    End Sub

    Public ReadOnly Property BookPDFPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "BookPDF")
        End Get
    End Property

    Public ReadOnly Property PackagePath As String
        Get
            Return Path.Combine(Me.WorkingPath, "Package")
        End Get
    End Property

    Public ReadOnly Property PBPackagePath As String
        Get
            Return Path.Combine(Me.WorkingPath, "PB_Package")
        End Get
    End Property

    Public ReadOnly Property PODPdfPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "PODPdf")
        End Get
    End Property

    Public ReadOnly Property RTFPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "RTF")
        End Get
    End Property

    Public ReadOnly Property ClientXMLPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "ClientXML")
        End Get
    End Property

    Public ReadOnly Property FinalXMLPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "FinalXML")
        End Get
    End Property

    Public ReadOnly Property ExportXMLPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "ExportXML")
        End Get
    End Property

    Public ReadOnly Property GSPreviewXMLPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "GSPreivewXML")
        End Get
    End Property

    Public ReadOnly Property GSPreviewInDesign As String
        Get
            Return Path.Combine(Me.WorkingPath, "GSPreivewInDesign")
        End Get
    End Property

    Public ReadOnly Property LXEXMLPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "LXEXML")
        End Get
    End Property

    Public ReadOnly Property FinalIndividualPDF As String
        Get
            Return Path.Combine(Me.WorkingPath, "IndividualPDF")
        End Get
    End Property

    Public ReadOnly Property ApplicationPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "Application")
        End Get
    End Property

    Public ReadOnly Property WEBPDFPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "WEBPDF")
        End Get
    End Property

    Public ReadOnly Property MOBIPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "MOBI")
        End Get
    End Property

    Public ReadOnly Property LSPDFPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "LSPDF")
        End Get
    End Property

    Public ReadOnly Property BookHardbackPDFName As String
        Get
            Return Path.Combine(Me.WEBPDFPath, Me.HardbackNum & ".PDF")
        End Get
    End Property

    Public ReadOnly Property BookPaperbackPDFName As String
        Get
            Return Path.Combine(Me.WEBPDFPath, Me.PaperbackNum & ".PDF")
        End Get
    End Property

    Public Property WEBPDFName As String = String.Empty 

    Public ReadOnly Property EPubPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "EPub")
        End Get
    End Property

    Public ReadOnly Property EPubName As String
        Get
            Return Path.Combine(Me.EPubPath, Me.HardbackNum & ".epub")
        End Get
    End Property

    Public ReadOnly Property DeliveryPath As String
        Get
            Return Path.Combine(Me.WorkingPath, "Delivery")
        End Get
    End Property

    Public ReadOnly Property DeliveryWEBPDF As String
        Get
            Return Path.Combine(Me.DeliveryPath, "Adobe")
        End Get
    End Property

    Public ReadOnly Property DeliveryCover As String
        Get
            Return Path.Combine(Me.DeliveryPath, "Covers")
        End Get
    End Property

    Public ReadOnly Property DeliveryePub As String
        Get
            Return Path.Combine(Me.DeliveryPath, "ePub")
        End Get
    End Property

    Public ReadOnly Property DeliveryPreview As String
        Get
            Return Path.Combine(Me.DeliveryPath, "Preview")
        End Get
    End Property

    Public ReadOnly Property DeliveryPalm As String
        Get
            Return Path.Combine(Me.DeliveryPath, "Palm")
        End Get
    End Property

    Public Function CreateRequiredFolder() As Boolean
        Dim TmpInddPath As String = String.Empty
        If (String.IsNullOrEmpty(TaskID)) Then
            GBL.DeantaBallon("TaskID should not be empty. Pleae check", MessageType.MSGERROR)
            Return False
        End If
        TmpInddPath = Path.Combine(GBL.CurrentWorkingPath, TaskID)
        'TmpInddPath = Path.Combine(TmpInddPath, Path.GetFileName(Path.GetTempFileName))
        TmpInddPath = Path.Combine(TmpInddPath, $"{TaskID}{DateTime.Now.ToString("HHmmssff")}")
        If (Not Directory.Exists(TmpInddPath)) Then
            Directory.CreateDirectory(TmpInddPath)
        End If
        Me.WorkingPath = TmpInddPath

        If (Not Directory.Exists(Me.ExportXMLPath)) Then
            Directory.CreateDirectory(Me.ExportXMLPath)
        End If

        If (Not Directory.Exists(Me.LXEXMLPath)) Then
            Directory.CreateDirectory(Me.LXEXMLPath)
        End If

        If (Not Directory.Exists(Me.WEBPDFPath)) Then
            Directory.CreateDirectory(Me.WEBPDFPath)
        End If

        If (Not Directory.Exists(Me.MOBIPath)) Then
            Directory.CreateDirectory(Me.MOBIPath)
        End If

        If (Not Directory.Exists(Me.LSPDFPath)) Then
            Directory.CreateDirectory(Me.LSPDFPath)
        End If

        If (Not Directory.Exists(Me.FinalIndividualPDF)) Then
            Directory.CreateDirectory(Me.FinalIndividualPDF)
        End If

        If (Not Directory.Exists(Me.PackagePath)) Then
            Directory.CreateDirectory(Me.PackagePath)
        End If

        If (Not Directory.Exists(Me.PBPackagePath)) Then
            Directory.CreateDirectory(Me.PBPackagePath)
        End If

        If (Not Directory.Exists(Me.RTFPath)) Then
            Directory.CreateDirectory(Me.RTFPath)
        End If

        If (Not Directory.Exists(Me.EPubPath)) Then
            Directory.CreateDirectory(Me.EPubPath)
        End If

        If (Not Directory.Exists(Me.PODPdfPath)) Then
            Directory.CreateDirectory(Me.PODPdfPath)
        End If

        If (Not Directory.Exists(Me.BookPDFPath)) Then
            Directory.CreateDirectory(Me.BookPDFPath)
        End If

        If (Not Directory.Exists(Me.FinalXMLPath)) Then
            Directory.CreateDirectory(Me.FinalXMLPath)
        End If

        If (Not Directory.Exists(Me.ClientXMLPath)) Then
            Directory.CreateDirectory(Me.ClientXMLPath)
        End If
        If (Not Directory.Exists(Me.ApplicationPath)) Then
            Directory.CreateDirectory(ApplicationPath)
        End If
        If (Not Directory.Exists(DeliveryPath)) Then
            Directory.CreateDirectory(Me.DeliveryPath)
        End If

        If (Not Directory.Exists(DeliveryCover)) Then
            Directory.CreateDirectory(Me.DeliveryCover)
        End If

        If (Not Directory.Exists(DeliveryWEBPDF)) Then
            Directory.CreateDirectory(Me.DeliveryWEBPDF)
        End If

        If (Not Directory.Exists(DeliveryePub)) Then
            Directory.CreateDirectory(Me.DeliveryePub)
        End If

        If (Not Directory.Exists(DeliveryPreview)) Then
            Directory.CreateDirectory(Me.DeliveryPreview)
        End If

        If (Not Directory.Exists(DeliveryPalm)) Then
            Directory.CreateDirectory(Me.DeliveryPalm)
        End If

        If (Not Directory.Exists(GSPreviewInDesign)) Then
            Directory.CreateDirectory(GSPreviewInDesign)
        End If

        If (Not Directory.Exists(GSPreviewXMLPath)) Then
            Directory.CreateDirectory(GSPreviewXMLPath)
        End If

        Return True
    End Function

End Class


Public Class DigitalBundleData

    Public Property DigitalID As String = String.Empty
    Public Property ProjectID As String = String.Empty
    Public Property TaskID As String = String.Empty
    Public Property TaskName As String = String.Empty
    Public Property UploadTaskID As String = String.Empty
    Public Property ChapterID As String = String.Empty
    Public Property MilestoneID As String = String.Empty
    Public Property MilestoneName As String = String.Empty
    Public Property DocumentID As String = String.Empty
    Public Property UserID As String = String.Empty
        Public Property UserName As String = String.Empty
    Public Property ProjectName As String = String.Empty
    Public Property BookCode As String = String.Empty
    Public Property ApplicationISBN As String = String.Empty
    Public Property HardbackISBN As String = String.Empty
    Public Property PaperbackISBN As String = String.Empty
    Public Property CoverISBN As String = String.Empty
    Public Property WebPDFISBN As String = String.Empty
    Public Property Keywords As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property GSPreviewCombinedXML As String = String.Empty
    Public Property ePubISBN As String = String.Empty
    Public Property ProjectAbb As String = String.Empty
    Public Property HardbackNum As String = String.Empty
    Public Property PaperbackNum As String = String.Empty
    Public Property XmlURL As String = String.Empty
    Public Property WorkPath As String = String.Empty
    Public Property ImagePath As String = String.Empty
    Public Property IsXMLGenerated As Boolean = False
    Public Property IsPODGenerated As Boolean = False
    Public Property IsWEBPDFGeneratd As Boolean = False
    Public Property IsEpubGenerated As Boolean = False
    Public Property IsPackageGenerated As Boolean = False
    Public Property IsRTFGenerated As Boolean = False
    Public Property IsCoverGenerated As Boolean = False
    Public Property IsBookPDFGenerated As Boolean = False
    Public Property IsMOBIGenerated As Boolean = False
    Public Property Status As Boolean = False
    Public Property CurrentStatus As String = String.Empty
    Public Property Folder As DBFolderStructure
    Public Property DocType As DocumentType = DocumentType.NONE
    Public Property OrgDocType As DocumentType = DocumentType.NONE
    Public Property MainXML As String = String.Empty
    Public Property PageSectionXML As String = String.Empty
    Public Property ClientXML As String = String.Empty
    Public Property ClientePubXML As String = String.Empty
    Public Property ClientOutXML As String = String.Empty
    Public Property ClientCleanXML As String = String.Empty
    Public Property FileOrderList As New List(Of String)
    Public Property InDesignFileList As New List(Of String)
    Public Property GSPreviewFileList As New List(Of String)
    Public Property IsProcessCompleted As Boolean = False
    Public Property TemplateFullName As String = String.Empty
    Public Property CoverImageFullName As String = String.Empty
    Public Property ClientAbbrevation As String = String.Empty
    Public Property TaskList As New List(Of String)
    Public Property IsLocalSetup As Boolean = False
    Public Property FinalAssets As New List(Of FinalResourceData)
    Public Property ResourceAssets As New List(Of FinalResourceData)
    Public Property Stage As DBStage = DBStage.NONE
    Public Property LogFilePath As String = String.Empty
    Public Property AbstractXML As String = String.Empty
    Public Sub DeantaBallon(Msg As String, MsgType As MessageType)
        File.AppendAllText(LogFilePath, $"{vbNewLine}{DateTime.Now} - {Msg.Replace("<table>", "table")}")
    End Sub

    Public Sub ErrorDigitalBundle(ByVal Message As String)
        MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "', remarks='" & File.ReadAllText(LogFilePath).Replace("'", "''").Replace("<", "").Replace(">", "") & "' where digitalbundle_id=" & Me.DigitalID)
        Me.DeantaBallon("Completed time :" & DateTime.Now.ToString("yyyyMMdd"), MessageType.MSGINFO)
        Me.IsProcessCompleted = True
        Me.DeantaBallon(Message, MessageType.MSGERROR)
    End Sub

    Public Function GetToeMailID() As String
        Dim eMailto As String = String.Empty
        Try
            If ((Me.DocType = DocumentType.TANDF) Or (Me.DocType = DocumentType.CRITICALPUB)) Then
                If ((String.Compare(Me.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(Me.TaskName, "first pages typesetting", True) = 0)) Then
                    eMailto = "layoutaudit@deantaglobal.com"
                Else
                    eMailto = "production3@deantaglobal.com"
                End If
            ElseIf (Me.DocType = DocumentType.TANDFUK) Then
                If ((String.Compare(Me.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(Me.TaskName, "first pages typesetting", True) = 0)) Then
                    eMailto = "layoutaudit@deantaglobal.com"
                Else
                    eMailto = "production3@deantaglobal.com"
                End If
            ElseIf ((Me.DocType = DocumentType.BLOOMSBURY) Or (Me.DocType = DocumentType.TRD) Or (Me.DocType = DocumentType.ANTHEM) Or (Me.DocType = DocumentType.MUP)) Then
                eMailto = "production2@deantaglobal.com"
            ElseIf ((Me.DocType = DocumentType.RL) Or (Me.DocType = DocumentType.SEQUOIA) Or (Me.DocType = DocumentType.UWIP) Or (Me.DocType = DocumentType.PELAGIC) Or (Me.DocType = DocumentType.EDWARDELGAR) Or (Me.DocType = DocumentType.UEPress)) Then
                eMailto = "production2@deantaglobal.com"
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        If String.IsNullOrEmpty(eMailto) Then
            eMailto = GBL.SupportMailID
        End If
        Return eMailto
    End Function

End Class

Public Enum FtpStatusType
    NONE = 0
    DOWNLOAD = 1
    UPLOAD = 2
    UPLOADALLFILES = 3
    DOWNLOADLIST = 4
    DOWNLOADWEBPDF = 5
End Enum

Public Enum DBStage
    NONE = 0
    XMLTRIGGER = 1
    WEBTRIGGER = 2
    FRISTPROFFXMLTRIGGER = 3
End Enum

Public Enum InDesignFloatType
    NONE = 0
    TABLE = 1
    FIGURE = 2
    SIDEBAR = 3
End Enum

Public Enum FloatFitColumnType
    NONE = 0
    SINGLECOLUMN = 1
    DOUBLECOLUMN = 2
    TWOTHIRDCOLUMN = 3
End Enum

Public Enum FloatSideType
    NONE = 0
    LEFTSIDE = 1
    RIGHTSIDE = 2
End Enum

Public Enum ExportMathType
    NONE = 0
    MATHML = 1
    EPS = 2
End Enum

Public Enum LanstadClientType
    NONE = 0
    BOOK = 1
    JOURNAL = 2
    BLOOMSBURY = 5
    BIOSCIENTIFICA = 20
    RANDL = 12
    TANDF = 11
    ANS = 43
    DOVE = 29
    CM = 31
    TANDFJOURNAL = 36
    CVP = 13
    TANDFUK = 18
    BDS = 19
    MANNING = 27
    GTDT = 42
    ISTE = 30
    INFORMALAW = 10
    MUP = 35
    CRITICALPUB = 70
    UWIP = 73
    PELAGIC = 75
    ANTHEM = 80
    EDWARDELGAR = 81
    SUNY = 82
    UEPress = 85
End Enum

Public Class NamedEntityData
    Public Property EntityName As String = String.Empty
    Public Property HexCode As String = String.Empty
End Class

Public Enum DigitialToolType
    NONE = 0
    SERVER = 1
    LOCAL = 2
End Enum


Public Enum ToolReleaseType
    NONE = 0
    LANSTANDLIVE = 1
    LANSTANDSTG = 2
    DARRANLAPTOP = 3
    AZUREDB = 4
    AZURESTGDB = 5
End Enum

Public Enum MessageType
    NONE = 0
    MSGERROR = 1
    MSGINFO = 2
    MSGPROGRESS = 3
    MSGDIALOG = 4
End Enum

Public Class XMLFileData

    Public Property SeqenceNum As Integer = 0
    Public Property InDesignFile As String = String.Empty

End Class

Public Class DigitalOutputData

    Public Property OutputFile As String = String.Empty
    Public Property ResourceID As String = String.Empty
    Public Property TaskName As DigitalBundleTask = DigitalBundleTask.NONE

End Class


Public Enum DocumentType
    NONE = 0
    RL = 12
    CM = 10
    JOURNAL = 1
    BOOK = 2
    TANDFUK = 18
    TANDF = 11
    CVP = 13
    BDS = 6
    BLOOMSBURY = 5
    ANS = 8
    BIO = 9
    DEMO = 3
    INFORMALAW = 13
    MUP = 35
    CRITICALPUB = 70
    SEQUOIA = 71
    TRD = 77
    UWIP = 73
    PELAGIC = 75
    ANTHEM = 80
    EDWARDELGAR = 81
    SUNY = 82
    UEPress = 85
End Enum


Public Class LocalResourceData

    Public Property docType As DocumentType = DocumentType.NONE
    Public Property ProjectID As String = String.Empty
    Public Property LocalPath As String = String.Empty
    Public Property ServerPath As String = String.Empty
    Public Property TaskType As DigitalBundleTask = DigitalBundleTask.NONE

End Class

Public Class FinalResourceData
    Public Property DBProcessName As String = String.Empty
    Public Property DBOrderID As Integer = 0
    Public Property DBTaskType As DigitalBundleTask = DigitalBundleTask.NONE
    Public Property FinalFileName As String = String.Empty
    Public Property FinalFilePath As String = String.Empty
End Class

Public Class TNFMetaDataCollector
    Public Property ElementName As String = String.Empty
    Public Property ElementType As TNFMetaDataType = TNFMetaDataType.NONE
    Public Property ElementSourceXPath As String = String.Empty
    Public Property ElementDestXPath As String = String.Empty
    Public Property MetaSource As MetadataType = MetadataType.NONE
    Public Property DBColumnName As String = String.Empty

End Class

Public Enum MetadataType
    NONE = 0
    XML = 1
    DB = 2
End Enum


Public Enum TNFMetaDataType
    NONE = 0
    eBookMasterISBN = 1
    Edition = 2
    Impression = 3
    PublisherName = 4
    DOI = 5
End Enum