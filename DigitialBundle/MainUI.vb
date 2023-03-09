Imports System.Web
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Text
Imports System.ComponentModel
Imports System.IO
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports System.Xml.Xsl
Imports Saxon.Api
Imports System.Globalization
Imports System.Threading
Imports DocumentFormat.OpenXml.Wordprocessing
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml
Imports iTextSharp.text.pdf

''' <summary>
''' 
''' </summary>
Public Class MainUI
    'C:\InDesignEngine\176219\tmp7617.tmp

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.Text = String.Format("::: InDesign Digitial Bundle Engine_v{0}- :::", Application.ProductVersion)

        GBL.ReleaseMethod = ToolReleaseType.AZUREDB

        GBL.LogFilePath = Path.Combine(GBL.CurrentWorkingPath, DateTime.Now.ToString("yyyyMMdd") & "_Digital.txt")

        If (GBL.ReleaseMethod = ToolReleaseType.LANSTANDLIVE) Then
            GBL.UserName = "appservd"
            'GBL.UserName = "root"

#If CONFIG = "Debug" Or CONFIG = "TEST" Then
            GBL.HostName = "78.137.168.31"
#Else
            GBL.HostName = "192.168.12.39"
#End If
            GBL.Password = "Tf16tWfs_AobzMPTI"
        ElseIf (GBL.ReleaseMethod = ToolReleaseType.LANSTANDSTG) Then
            GBL.UserName = "root"
            GBL.HostName = "92.51.243.215"
            GBL.Password = "Gj#mL$61pD"
        ElseIf (GBL.ReleaseMethod = ToolReleaseType.DARRANLAPTOP) Then
            GBL.UserName = "root"
            GBL.HostName = "92.51.243.215"
            GBL.Password = "Gj#mL$61pD"
            Me.Opacity = 0.2
        ElseIf (GBL.ReleaseMethod = ToolReleaseType.AZUREDB) Then
            GBL.UserName = "dev_sftp_01"
#If CONFIG <> "Release" Then
            GBL.HostName = "13.79.228.224"
#Else
            GBL.HostName = "172.17.0.6"
#End If
            GBL.Password = "dl&RAcowr8R+YEsT#4W&"
        ElseIf (GBL.ReleaseMethod = ToolReleaseType.AZURESTGDB) Then
            GBL.UserName = "dev_sftp_01"
            GBL.HostName = "13.79.228.224"
            GBL.Password = "dl&RAcowr8R+YEsT#4W&"
        Else
            GBL.UserName = "root"
            GBL.HostName = "92.51.243.215"
            GBL.Password = "Gj#mL$61pD"
        End If

        GBL.Worker.WorkerReportsProgress = True
        GBL.Worker.WorkerSupportsCancellation = True

        AddHandler GBL.Worker.DoWork, AddressOf Worker_Dowork
        AddHandler GBL.Worker.ProgressChanged, AddressOf Worker_ProgressChanged
        AddHandler GBL.Worker.RunWorkerCompleted, AddressOf Worker_RunWorkerCompleted

    End Sub

    Private Sub MainUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'BloomsburyPaparbackPackage("D:\DDrive\Documents\Sprint_70\RL_Discussion_2\TransferNow-Technology points\Technology points\02_DB Fixes\FSM\01\Package",
        '                           "D:\DDrive\Documents\Sprint_70\RL_Discussion_2\TransferNow-Technology points\Technology points\02_DB Fixes\FSM\01\PP_Package",
        '                           "9781350150010", "9781350150003")
        'Try
        '    AddMetadata("D:\DDrive\Support\07-02-2020\Metadata\9780367431709_txt.pdf", "D:\DDrive\Support\07-02-2020\Metadata\9780367431709_txt_output.pdf")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        InitializeLoclCopyResouce()

        'ConvertHtmltoDocx("D:\DDrive\Documents\Sprint_59\IFAC_word\A11 IPSAS_05_RTF.rtf", "D:\DDrive\Documents\Sprint_59\IFAC_word\IFAC_Template.dotx")

        If (Not GBL.Worker.IsBusy) Then
            GBL.Worker.RunWorkerAsync()
        End If
    End Sub



#Region "Private Methods"

    Private Sub Worker_Dowork(ByVal Sender As Object, e As DoWorkEventArgs)
        Dim TblIndesignPdf As DataTable = Nothing
        Dim IsError As Boolean = False
        Dim IsExists As Boolean = False
        Dim TmpInddPath As String = String.Empty
        Dim DbData As DigitalBundleData = Nothing
        GBL.LogFilePath = Path.Combine(GBL.CurrentWorkingPath, DateTime.Now.ToString("yyyyMMdd") & "_Digital.txt")
        If (e.Cancel) Then
            GBL.Worker.CancelAsync()
        Else
            While (1)
                Try
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
                    TblIndesignPdf = MySqlHelper.ReadSqlData("select * from tbl_digitalbundle_pdf where digitalbundle_id=5104")
#Else
                    TblIndesignPdf = MySqlHelper.ReadSqlData("select * from tbl_digitalbundle_pdf where status=0 order by digitalbundle_id")
#End If
                    If ((TblIndesignPdf IsNot Nothing) AndAlso (TblIndesignPdf.Rows IsNot Nothing) AndAlso (TblIndesignPdf.Rows.Count > 0)) Then
                        For Each DtRow As DataRow In TblIndesignPdf.Rows
                            IsExists = IsLandstadProjectExists(DtRow)
                            If (Not IsExists) Then
                                GBL.DBDataList.Add(New DigitalBundleData With {.CurrentStatus = "Added",
                                                                               .DigitalID = Convert.ToString(DtRow.Item("digitalbundle_id")),
                                                                               .ImagePath = Convert.ToString(DtRow.Item("imagepath")),
                                                                               .MilestoneID = Convert.ToString(DtRow.Item("milestone_id")),
                                                                               .DocumentID = Convert.ToString(DtRow.Item("document_id")),
                                                                               .ChapterID = Convert.ToString(DtRow.Item("chapter_id")),
                                                                               .ProjectID = Convert.ToString(DtRow.Item("project_id")),
                                                                               .TaskID = Convert.ToString(DtRow.Item("task_id")),
                                                                               .UserID = Convert.ToString(DtRow.Item("user_id")),
                                                                               .WorkPath = String.Empty,
                                                                               .IsProcessCompleted = False,
                                                                               .IsBookPDFGenerated = Convert.ToString(DtRow.Item("bookpdf")),
                                                                               .IsPODGenerated = Convert.ToString(DtRow.Item("POD")),
                                                                               .IsXMLGenerated = Convert.ToString(DtRow.Item("XML")),
                                                                               .Stage = DirectCast([Enum].Parse(GetType(DBStage), Convert.ToString(DtRow.Item("stage"))), Integer),
                                                                               .IsWEBPDFGeneratd = Convert.ToString(DtRow.Item("webpdf")),
                                                                               .IsPackageGenerated = Convert.ToString(DtRow.Item("package")),
                                                                               .IsEpubGenerated = Convert.ToString(DtRow.Item("epub")),
                                                                               .IsRTFGenerated = Convert.ToString(DtRow.Item("RTF")),
                                                                               .IsCoverGenerated = Convert.ToString(DtRow.Item("cover")),
                                                                               .XmlURL = Convert.ToString(DtRow.Item("xmlpath"))})

                            End If
                        Next
                    End If

                    'GBL.DBDataList.Add(New DigitalBundleData With {.CurrentStatus = "Added",
                    '                                                          .DigitalID = 10,
                    '                                                          .MilestoneID = "44152",
                    '                                                          .ImagePath = "/opt/apache-tomcat-7.0.72/webapps/oxygen-webapp-19.0.0.0/xmlfiles/docbook/3209/finaloutput/images",
                    '                                                          .ChapterID = "",
                    '                                                          .ProjectID = "3209",
                    '                                                          .TaskID = "179525",
                    '                                                          .UserID = "997",
                    '                                                          .WorkPath = String.Empty,
                    '                                                          .IsProcessCompleted = False,
                    '                                                          .XmlURL = "/opt/apache-tomcat-7.0.72/webapps/oxygen-webapp-19.0.0.0/xmlfiles/docbook/3209/finaloutput/Main.xml"})

                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & ex.StackTrace, MessageType.MSGERROR)
                    Threading.Thread.Sleep(2000)
                End Try

                'GBL.DeantaBallon("Count :" & GBL.DBDataList.Count, MessageType.MSGINFO)
                Try
                    DoGenerateDigitalBundleProcess()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & ex.StackTrace, MessageType.MSGERROR)
                End Try
                Threading.Thread.Sleep(1000)
            End While
        End If
    End Sub

    Private Function DoGenerateDigitalBundleProcess() As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            Return False
        End If
        Dim isLocal As Boolean = False
        For dbID As Integer = 0 To GBL.DBDataList.Count - 1
            DBdata = GBL.DBDataList(dbID)
            GBL.DBDataList(dbID).TaskList.Clear()
            isLocal = IsCheckLocalProject(DBdata.ProjectID)
            If (Not isLocal) Then
                Try
                    DoGenerateDigitalBundle(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & ex.StackTrace, MessageType.MSGERROR)
                    Continue For
                End Try
            Else
                GBL.DBDataList(dbID).IsLocalSetup = True
                Try
                    DownloadLocalFilesUpload(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & ex.StackTrace, MessageType.MSGERROR)
                    Continue For
                End Try
            End If
            Threading.Thread.Sleep(2000)
        Next
        Return True
    End Function

    Private Function IsCheckLocalProject(ByVal ProjectID As String) As Boolean
        Dim LocalProjectList As New List(Of String)
        Dim IsLocal As Boolean = False
        LocalProjectList.AddRange(New String() {"3383", "3335", "3209", "4802", "6342", "6343"})
        IsLocal = (From n In LocalProjectList Where (String.Compare(ProjectID, n, True) = 0) Select n).Any
        Return IsLocal
    End Function

    Private Function IsLandstadProjectExists(DtRow As DataRow) As Boolean
        Dim IsFound As Boolean = GBL.DBDataList.Exists(Function(pro As DigitalBundleData)
                                                           If (pro.DigitalID = Convert.ToString(DtRow.Item("digitalbundle_id"))) Then
                                                               Return True
                                                           End If
                                                           Return False
                                                       End Function)
        Return IsFound
    End Function

    Private Sub Worker_ProgressChanged(ByVal Sender As Object, e As ProgressChangedEventArgs)
        Try
            Dim BdSource As New BindingSource
            Dim Result As Object = Nothing
            Result = e.UserState
            If (Result IsNot Nothing) Then
                GBL.DBDataList(Result.Index).CurrentStatus = Result.Status
            End If
            BdSource.DataSource = GBL.DBDataList
            dgvDigitialBun.DataSource = BdSource
        Catch ex As Exception
        End Try
    End Sub

    Private Function InitializeLoclCopyResouce() As Boolean
        GBL.LocalCopyList.Clear()
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\epub\9781315393483.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\xml", .TaskType = DigitalBundleTask.TFXML})
        'GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\xml\artwork", .TaskType = DigitalBundleTask.XMLIMAGE})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\webpdf\9781315393490.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\cover\9781315393506.jpg", .TaskType = DigitalBundleTask.COVER})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\mobi\9781315393476.prc", .TaskType = DigitalBundleTask.MOBI})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\webpdf\9781315393490.pdf", .TaskType = DigitalBundleTask.PREVIERPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\app", .TaskType = DigitalBundleTask.APPLICATION})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3209", .docType = DocumentType.TANDF, .LocalPath = "C:\ServerEngine\DigitialBundle\3209\images", .TaskType = DigitalBundleTask.IMAGES})

        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\epub\9781000033328_epub.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\xml", .TaskType = DigitalBundleTask.TFXML})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\webpdf\9781000033281_webpdf.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\cover\9780429280368.jpg", .TaskType = DigitalBundleTask.COVER})
        'GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6332", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6332\mobi\9781000033304_mobi.prc", .TaskType = DigitalBundleTask.MOBI})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\preview\9780429280368_GSpreview.pdf", .TaskType = DigitalBundleTask.PREVIERPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\app", .TaskType = DigitalBundleTask.APPLICATION})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6343", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6343\images", .TaskType = DigitalBundleTask.IMAGES})

        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\epub\9781000033328_epub.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\xml", .TaskType = DigitalBundleTask.TFXML})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\webpdf\9781000033281_webpdf.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\cover\9780429280368.jpg", .TaskType = DigitalBundleTask.COVER})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\preview\9780429280368_GSpreview.pdf", .TaskType = DigitalBundleTask.PREVIERPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\app", .TaskType = DigitalBundleTask.APPLICATION})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "6342", .docType = DocumentType.TANDFUK, .LocalPath = "C:\ServerEngine\DigitialBundle\6342\images", .TaskType = DigitalBundleTask.IMAGES})


        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\epub\9781501329005_epub.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\mobi\9781501329005_mobi.mobi", .TaskType = DigitalBundleTask.MOBI})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\xml\9781501329012_txt_xml.xml", .TaskType = DigitalBundleTask.TFXML})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\xml\images", .TaskType = DigitalBundleTask.XMLIMAGE})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\xml\images", .TaskType = DigitalBundleTask.XMLPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\webpdf\9781501328992_web.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\webpdf\9781501329029_preview.pdf", .TaskType = DigitalBundleTask.PREVIERPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\webpdf\9781501329029_txt_LS.pdf", .TaskType = DigitalBundleTask.LSPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\webpdf\9781501329029_txt_print.pdf", .TaskType = DigitalBundleTask.PRINTPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\rtf\9781501329029_txt_txt.doc", .TaskType = DigitalBundleTask.COMBINEDRTF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\images\9781501329029_txt_images", .TaskType = DigitalBundleTask.IMAGES})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3335", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\3335\app", .TaskType = DigitalBundleTask.APPLICATION})

        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\epub\9781350036932_epub.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\mobi\9781350036932_mobi.mobi", .TaskType = DigitalBundleTask.MOBI})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\xml", .TaskType = DigitalBundleTask.TFXML})
        'GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BB, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\xml\images", .TaskType = DigitalBundleTask.XMLIMAGE})
        'GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BB, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\xml\images", .TaskType = DigitalBundleTask.XMLPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\webpdf\9781350036949_web.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\webpdf\9781350036925_preview.pdf", .TaskType = DigitalBundleTask.PREVIERPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\webpdf\9781350036925_txt_LS.pdf", .TaskType = DigitalBundleTask.LSPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\webpdf\9781350036925_txt_print.pdf", .TaskType = DigitalBundleTask.PRINTPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\rtf\9781350036925_txt_txt.doc", .TaskType = DigitalBundleTask.COMBINEDRTF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\images\9781350036925_txt_images", .TaskType = DigitalBundleTask.IMAGES})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "4802", .docType = DocumentType.BLOOMSBURY, .LocalPath = "C:\ServerEngine\DigitialBundle\4802\app", .TaskType = DigitalBundleTask.PACKAGING})

        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3383", .docType = DocumentType.RL, .LocalPath = "C:\ServerEngine\DigitialBundle\3383\webpdf\9781475822700_web.pdf", .TaskType = DigitalBundleTask.WEBPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3383", .docType = DocumentType.RL, .LocalPath = "C:\ServerEngine\DigitialBundle\3383\epub\9781475822700_epub.epub", .TaskType = DigitalBundleTask.EPUB})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3383", .docType = DocumentType.RL, .LocalPath = "C:\ServerEngine\DigitialBundle\3383\print\9781475822687_print.pdf", .TaskType = DigitalBundleTask.PRINTPDF})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3383", .docType = DocumentType.RL, .LocalPath = "C:\ServerEngine\DigitialBundle\3383\app", .TaskType = DigitalBundleTask.APPLICATION})
        GBL.LocalCopyList.Add(New LocalResourceData With {.ProjectID = "3383", .docType = DocumentType.RL, .LocalPath = "C:\ServerEngine\DigitialBundle\3383\xml", .TaskType = DigitalBundleTask.TFXML})

        Return True
    End Function

    Private Sub Worker_RunWorkerCompleted(ByVal Sender As Object, e As RunWorkerCompletedEventArgs)
        If (e.Cancelled) Then
            GBL.Worker.CancelAsync()
        ElseIf (e.Error IsNot Nothing) Then
        Else
        End If
    End Sub

    Private Function DownloadLocalFilesUpload(ByVal dbID As Integer) As Boolean
        Dim Stdw As New Stopwatch
        Dim DBdata As DigitalBundleData = Nothing
        'GBL.DeantaBallon("Count :" & GBL.DBDataList.Count, MessageType.MSGINFO)
        DBdata = GBL.DBDataList(dbID)
        GBL.DBDataList(dbID).TaskList.Clear()

        If (GBL.DBDataList(dbID).IsProcessCompleted) Then
            Return False
        End If

        Stdw.Start()

        GBL.UpdateGridStatus(dbID, "create folder strcuture")
        GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)

        If (Not (GBL.DBDataList(dbID).Folder.CreateRequiredFolder())) Then
            GBL.DeantaBallon("Error occured while creating the folder strucuture for DB", MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='Error occured while creating the folder strucuture for DB' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End If

        GBL.DBDataList(dbID).WorkPath = DBdata.Folder.WorkingPath
        GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))

        Try
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set temp_path='" & GBL.DBDataList(dbID).WorkPath.Replace("/", "//") & "' where digitalbundle_id=" & DBdata.DigitalID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        GBL.DeantaBallon("index : " & dbID, MessageType.MSGINFO)
        Try
            UpdateClientandDocumentType(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("Update client doctype." & DBdata.DocType, MessageType.MSGINFO)

        GBL.DeantaBallon("create task list.", MessageType.MSGINFO)

        Try
            CreateTaskList(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("identify book elements.", MessageType.MSGINFO)

        Try
            'IdentifiyBookElement("C:\InDesignEngine\1522\tmp3544.tmp\LXEXML", index)
            IdentifiyBookElement(DBdata.Folder.LXEXMLPath, dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
        GBL.DeantaBallon("download lxe xml.", MessageType.MSGINFO)

        Dim LocalResourceList As List(Of LocalResourceData) = GetLocalPathFile(GBL.DBDataList(dbID))
        If ((LocalResourceList Is Nothing) OrElse (LocalResourceList.Count = 0)) Then
            GBL.DeantaBallon("Local resource could not found for the Project ID : " & DBdata.ProjectID & " docType :" & DBdata.DocType, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & "Local resource could not found for the Project ID : " & DBdata.ProjectID & " docType :" & DBdata.DocType & "' where digitalbundle_id=" & DBdata.DigitalID)
            Return False
        End If
        Stdw.Stop()

        GBL.DeantaBallon("Local file copy started." & Stdw.Elapsed.Seconds, MessageType.MSGINFO)
        Stdw.Restart()

        Try
            For Each Res As LocalResourceData In LocalResourceList
                '''Threading.Thread.Sleep(9000)
                GBL.DeantaBallon("Local asset copying.", MessageType.MSGINFO)
                Select Case Res.TaskType
                    Case DigitalBundleTask.WEBPDF
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.WEBPDFPath, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.WEBPDFPath, Path.GetFileName(Res.LocalPath)))
                        GBL.DBDataList(dbID).Folder.WEBPDFName = Path.GetFileName(Res.LocalPath).Replace(".pdf", ".indb").Replace("_web", "_txt")
                        ff.LastWriteTime = GBL.GetIndianTime

                        If ((Res.docType = DocumentType.TANDF) Or (Res.docType = DocumentType.TANDFUK)) Then
                            File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.DeliveryWEBPDF, Path.GetFileName(Res.LocalPath)), True)
                            Dim ff1 As New FileInfo(Path.Combine(DBdata.Folder.DeliveryWEBPDF, Path.GetFileName(Res.LocalPath)))
                            ff1.LastWriteTime = GBL.GetIndianTime
                        End If

                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.WEBPDFPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.COVER
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.DeliveryCover, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.DeliveryCover, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.DeliveryCover, MessageType.MSGINFO)
                    Case DigitalBundleTask.EPUB
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.DeliveryePub, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.DeliveryePub, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.DeliveryePub, MessageType.MSGINFO)
                    Case DigitalBundleTask.COMBINEDRTF
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.RTFPath, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.RTFPath, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.RTFPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.MOBI
                        If ((Res.docType = DocumentType.TANDF) Or (Res.docType = DocumentType.TANDFUK)) Then
                            File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.DeliveryPalm, Path.GetFileName(Res.LocalPath)), True)
                            Dim ff As New FileInfo(Path.Combine(DBdata.Folder.DeliveryPalm, Path.GetFileName(Res.LocalPath)))
                            ff.LastWriteTime = GBL.GetIndianTime
                            GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.MOBIPath, MessageType.MSGINFO)
                        Else
                            File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.MOBIPath, Path.GetFileName(Res.LocalPath)), True)
                            Dim ff As New FileInfo(Path.Combine(DBdata.Folder.MOBIPath, Path.GetFileName(Res.LocalPath)))
                            ff.LastWriteTime = GBL.GetIndianTime
                            GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.MOBIPath, MessageType.MSGINFO)
                        End If


                    Case DigitalBundleTask.APPLICATION
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.ApplicationPath, MessageType.MSGINFO)
                        Dim AppPath As String = String.Empty
                        AppPath = DBdata.Folder.ApplicationPath
                        If (Not Directory.Exists(AppPath)) Then
                            Directory.CreateDirectory(AppPath)
                        End If
                        For Each InddFile As String In Directory.GetFiles(Res.LocalPath, "*.*", SearchOption.TopDirectoryOnly)
                            Try
                                File.Copy(InddFile, Path.Combine(AppPath, Path.GetFileName(InddFile)), True)
                                Dim ff As New FileInfo(Path.Combine(AppPath, Path.GetFileName(InddFile)))
                                ff.LastWriteTime = GBL.GetIndianTime
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        Next
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.ApplicationPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.LSPDF
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.LSPDFPath, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.LSPDFPath, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.LSPDFPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.PRINTPDF
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.BookPDFPath, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.BookPDFPath, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.BookPDFPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.PREVIERPDF
                        File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.DeliveryPreview, Path.GetFileName(Res.LocalPath)), True)
                        Dim ff As New FileInfo(Path.Combine(DBdata.Folder.DeliveryPreview, Path.GetFileName(Res.LocalPath)))
                        ff.LastWriteTime = GBL.GetIndianTime
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.DeliveryPreview, MessageType.MSGINFO)
                    Case DigitalBundleTask.TFXML
                        'File.Copy(Res.LocalPath, Path.Combine(DBdata.Folder.ClientXMLPath, Path.GetFileName(Res.LocalPath)), True)
                        'Dim ff As New FileInfo(Path.Combine(DBdata.Folder.ClientXMLPath, Path.GetFileName(Res.LocalPath)))
                        'ff.LastWriteTime = GBL.GetIndianTime
                        'GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.ClientXMLPath, MessageType.MSGINFO)
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.ClientXMLPath, MessageType.MSGINFO)
                        CopyDirectory(Res.LocalPath, DBdata.Folder.ClientXMLPath)
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.ClientXMLPath, MessageType.MSGINFO)
                    Case DigitalBundleTask.PACKAGING
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.PackagePath, MessageType.MSGINFO)
                        CopyDirectory(Res.LocalPath, DBdata.Folder.PackagePath)
                        GBL.DeantaBallon(Res.LocalPath & " - " & DBdata.Folder.PackagePath, MessageType.MSGINFO)
                    Case DigitalBundleTask.IMAGES
                        GBL.DeantaBallon(Res.LocalPath & " - " & Path.Combine(DBdata.Folder.WEBPDFPath, "images"), MessageType.MSGINFO)
                        Dim ImagePath As String = String.Empty
                        If ((Res.docType = DocumentType.TANDF) Or (Res.docType = DocumentType.TANDFUK)) Then
                            ImagePath = Path.Combine(DBdata.Folder.WEBPDFPath, "images")
                        ElseIf ((Res.docType = DocumentType.BLOOMSBURY) Or (Res.docType = DocumentType.TRD)) Then
                            ImagePath = Path.Combine(DBdata.Folder.WEBPDFPath, "images_print")
                        End If
                        If (Not Directory.Exists(ImagePath)) Then
                            Directory.CreateDirectory(ImagePath)
                        End If
                        For Each InddFile As String In Directory.GetFiles(Res.LocalPath, "*.*", SearchOption.TopDirectoryOnly)
                            Try
                                File.Copy(InddFile, Path.Combine(ImagePath, Path.GetFileName(InddFile)), True)
                                Dim ff As New FileInfo(Path.Combine(ImagePath, Path.GetFileName(InddFile)))
                                ff.LastWriteTime = GBL.GetIndianTime
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        Next
                        GBL.DeantaBallon(Res.LocalPath & " - " & Path.Combine(DBdata.Folder.WEBPDFPath, "images"), MessageType.MSGINFO)
                    Case DigitalBundleTask.XMLIMAGE
                        GBL.DeantaBallon(Res.LocalPath & " - " & Path.Combine(DBdata.Folder.ClientXMLPath, "artwork"), MessageType.MSGINFO)
                        Dim ImagePath As String = String.Empty
                        If ((Res.docType = DocumentType.TANDF) Or (Res.docType = DocumentType.TANDFUK)) Then
                            ImagePath = Path.Combine(DBdata.Folder.ClientXMLPath, "artwork")
                        Else
                            ImagePath = DBdata.Folder.ClientXMLPath
                        End If
                        If (Not Directory.Exists(ImagePath)) Then
                            Directory.CreateDirectory(ImagePath)
                        End If
                        For Each InddFile As String In Directory.GetFiles(Res.LocalPath, "*.*", SearchOption.TopDirectoryOnly)
                            Try
                                File.Copy(InddFile, Path.Combine(ImagePath, Path.GetFileName(InddFile)), True)
                                Dim ff As New FileInfo(Path.Combine(ImagePath, Path.GetFileName(InddFile)))
                                ff.LastWriteTime = GBL.GetIndianTime
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        Next
                        GBL.DeantaBallon(Res.LocalPath & " - " & Path.Combine(DBdata.Folder.ClientXMLPath, "images"), MessageType.MSGINFO)
                End Select

            Next
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            Return False
        End Try

        Stdw.Restart()
        GBL.DeantaBallon("Local file copy completed." & Stdw.Elapsed.Seconds, MessageType.MSGINFO)

        Try
            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK)) Then
                If (Not UploadRequiredFilesForTANDF(dbID, True)) Then
                    GBL.DeantaBallon("Error while uploading Tandf files.", MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks=remarks+'Error while uploading Tandf files' where digitalbundle_id=" & DBdata.DigitalID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End If
            ElseIf ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD) Or (DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then
                If (Not UploadRequiredFilesForBloomsbury(dbID, True)) Then
                    GBL.DeantaBallon("Error while uploading bloomsbury files.", MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks=remarks+'Error while uploading bloomsbury files' where digitalbundle_id=" & DBdata.DigitalID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End If
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)
        Try
            Dim NotifyMgr As New NotificationManager
            If (Not NotifyMgr.AddNotification(dbID)) Then
                GBL.DeantaBallon("Error while generating notification table", MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='Error while generating notification table' where digitalbundle_id=" & DBdata.DigitalID)
                'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
        GBL.DeantaBallon("Notification completed.", MessageType.MSGINFO)
        MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=1, end_date='" & GBL.GetPdfTime & "',remarks='" & File.ReadAllText(GBL.LogFilePath).Replace("'", "''") & "' where digitalbundle_id=" & DBdata.DigitalID)
        'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
        GBL.DBDataList(dbID).IsProcessCompleted = True
        GBL.UpdateGridStatus(dbID, "Completed.")

        Return True
    End Function

    Public Sub CopyDirectory(ByVal sourcePath As String, ByVal destinationPath As String)
        Dim sourceDirectoryInfo As New System.IO.DirectoryInfo(sourcePath)
        If Not System.IO.Directory.Exists(destinationPath) Then
            System.IO.Directory.CreateDirectory(destinationPath)
            Dim ff As New DirectoryInfo(destinationPath)
            ff.LastWriteTime = GBL.GetIndianTime
        End If

        Dim fileSystemInfo As System.IO.FileSystemInfo
        For Each fileSystemInfo In sourceDirectoryInfo.GetFileSystemInfos
            Dim destinationFileName As String =
                System.IO.Path.Combine(destinationPath, fileSystemInfo.Name)

            If TypeOf fileSystemInfo Is System.IO.FileInfo Then
                System.IO.File.Copy(fileSystemInfo.FullName, destinationFileName, True)
                Dim ff As New DirectoryInfo(destinationFileName)
                ff.LastWriteTime = GBL.GetIndianTime
            Else
                CopyDirectory(fileSystemInfo.FullName, destinationFileName)
            End If
        Next
    End Sub

    Private Function DownloadArtworkForXML(ByVal index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim StatusUI As UploadDownloadHelper

        Dim ArtworkFile As String = MySqlHelper.ExecuteScalar("select document_path from tb_documents where task_id=" & DBData.TaskID & " and document_name like '%artwork%' and document_type like '%zip%'")
        ArtworkFile = Path.Combine(GBL.FTPResourcePath.Replace("resources", ""), ArtworkFile).Replace("\", "/")
        Try
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBData.Folder.ClientXMLPath, .UrlPath = ArtworkFile, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(ArtworkFile), DBData.Folder.ClientXMLPath, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        GBL.DeantaBallon("artwork downloaded into XML path." & ArtworkFile, MessageType.MSGINFO)

        If (File.Exists(ArtworkFile)) Then
            Dim ArtworkPath As String = Path.Combine(DBData.Folder.ClientXMLPath, "artwork")
            If (Not Directory.Exists(ArtworkPath)) Then
                Directory.CreateDirectory(ArtworkPath)
            Else
                Array.ForEach(Directory.GetFiles("*.*", ArtworkPath), Sub(fd As String)
                                                                          Try
                                                                              File.Delete(fd)
                                                                          Catch ex As Exception
                                                                          End Try
                                                                      End Sub)
            End If
            Try
                ExtractZipFile(ArtworkFile, ArtworkPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        Else
            GBL.DeantaBallon("Could not able to find the zip file " & ArtworkFile, MessageType.MSGERROR)
            Return False
        End If
        Return True
    End Function

    Private Function DownloadCoverImageFromAsset(ByVal index As Integer) As Boolean
        Dim DBdata As New DigitalBundleData
        Dim TblCover As New DataTable
        Dim CoverImagename As String = String.Empty
        Dim StatusUI As UploadDownloadHelper
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            Return False
        End If

        DBdata = GBL.DBDataList(index)
        'TblCover = MySqlHelper.ReadSqlData("select * from tb_documents where project_id=" & DBdata.ProjectID & " and document_name like '%" & String.Format("_cover.jpg", DBdata.ApplicationISBN) & "' order by document_id desc")

        TblCover = MySqlHelper.ReadSqlData("select * from tb_documents where project_id=" & DBdata.ProjectID & " and document_name like '%_cover.jpg' order by document_id desc")

        If ((TblCover Is Nothing) OrElse (TblCover.Rows Is Nothing) OrElse (TblCover.Rows.Count = 0)) Then
            GBL.DeantaBallon("Could not able to find the cover image.", MessageType.MSGERROR)
            Return False
        End If

        CoverImagename = Convert.ToString(TblCover.Rows(0).Item("document_name"))

        If (String.IsNullOrEmpty(CoverImagename)) Then
            Return False
        End If

        GBL.DBDataList(index).CoverImageFullName = Path.Combine(DBdata.Folder.WEBPDFPath, CoverImagename)

        CoverImagename = Path.Combine(GBL.FTPResourcePath, Convert.ToString(TblCover.Rows(0).Item("document_path")).Replace("resources/", "")).Replace("\", "/")

        GBL.DeantaBallon("cover image name " & CoverImagename, MessageType.MSGINFO)

        Try
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.WEBPDFPath, .UrlPath = CoverImagename, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(CoverImagename), DBdata.Folder.WEBPDFPath, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If (File.Exists(Path.Combine(DBdata.Folder.WEBPDFPath, Path.GetFileName(CoverImagename)))) Then
            File.Move(Path.Combine(DBdata.Folder.WEBPDFPath, Path.GetFileName(CoverImagename)), GBL.DBDataList(index).CoverImageFullName)
        Else
            GBL.DeantaBallon("Could not able to download the cover image", MessageType.MSGERROR)
            Return False
        End If

        If (File.Exists(GBL.DBDataList(index).CoverImageFullName)) Then
            File.Copy(GBL.DBDataList(index).CoverImageFullName, Path.Combine(GBL.DBDataList(index).Folder.DeliveryCover, String.Format("{0}{1}", DBdata.WebPDFISBN, Path.GetExtension(GBL.DBDataList(index).CoverImageFullName))), True)
            'File.Copy(GBL.DBDataList(index).CoverImageFullName, Path.Combine(GBL.DBDataList(index).Folder.DeliveryCover, String.Format("{0}{1}", DBdata.ApplicationISBN, Path.GetFileName(GBL.DBDataList(index).CoverImageFullName))), True)
        End If

        Return True
    End Function

    Private Function DownloadCoverImage(ByVal index As Integer) As Boolean
        Dim DBdata As New DigitalBundleData
        Dim StatusUI As UploadDownloadHelper
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            Return False
        End If
        DBdata = GBL.DBDataList(index)
        Dim CoverImagename As String = MySqlHelper.ExecuteScalar("select project_image from tb_projects where project_id=" & DBdata.ProjectID)
        If (String.IsNullOrEmpty(CoverImagename)) Then
            Return False
        End If

        GBL.DBDataList(index).CoverImageFullName = Path.Combine(DBdata.Folder.WEBPDFPath, CoverImagename)

        CoverImagename = Path.Combine("/var/www/epublishing/epublishing/coverimage/", CoverImagename).Replace("\", "/")

        GBL.DeantaBallon("cover image name " & CoverImagename, MessageType.MSGINFO)

        Try
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.WEBPDFPath, .UrlPath = CoverImagename, .Index = index})
            AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If (File.Exists(GBL.DBDataList(index).CoverImageFullName)) Then
            File.Copy(GBL.DBDataList(index).CoverImageFullName, Path.Combine(GBL.DBDataList(index).Folder.DeliveryCover, String.Format("{0}{1}", DBdata.CoverISBN, Path.GetFileName(GBL.DBDataList(index).CoverImageFullName))), True)
        End If

        Return True
    End Function

    Private Function DoGenerateDigitalBundle(ByVal dbID As Integer) As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        Dim eMailto As String = String.Empty
        Dim XmlStage As String = String.Empty
        Dim UserName As String = String.Empty

        GBL.DBDataList(dbID).LogFilePath = Path.Combine(GBL.CurrentWorkingPath, DateTime.Now.ToString("yyyyMMdd") & "_Digital.txt")
        GBL.DBDataList(dbID).TaskList.Clear()
        GBL.DBDataList(dbID).FinalAssets.Clear()
        GBL.OutputDataList.Clear()
        DBdata = GBL.DBDataList(dbID)
        If (GBL.DBDataList(dbID).IsProcessCompleted) Then
            Return False
        End If

#If CONFIG = "FinalXML" Then
        Try
            FinalXML(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        End
#End If

        GBL.UpdateGridStatus(dbID, "create folder strcuture")
        GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)

        If (Not (GBL.DBDataList(dbID).Folder.CreateRequiredFolder())) Then
            DBdata.ErrorDigitalBundle("Error occured while creating the folder strucuture for DB")
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End If

        GBL.DBDataList(dbID).WorkPath = DBdata.Folder.WorkingPath
        GBL.DBDataList(dbID).LogFilePath = Path.Combine(DBdata.Folder.WorkingPath, $"{DBdata.DigitalID}.txt")
        GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))

        Try
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set start_date='" & GBL.GetPdfTime & "',bookpdfpath='" & GBL.LogFilePath.Replace("\", "\\") & "' where digitalbundle_id=" & DBdata.DigitalID)
        Catch ex As Exception
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("index : " & dbID, MessageType.MSGINFO)
        GBL.UpdateGridStatus(dbID, "identifiy required files")
        GBL.DeantaBallon("Update client doctype.", MessageType.MSGINFO)

        Try
            UpdateClientandDocumentType(dbID)
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message & ex.StackTrace)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("communication started.", MessageType.MSGINFO)
        Dim CommMgr As New LanstadCommunication(dbID)

#If CONFIG <> "Debug" Then

        Try
            CommMgr.StartLanstadTask()
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message & ex.StackTrace)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
        GBL.DeantaBallon("Task created.", MessageType.MSGINFO)
#End If

        Try
            CreateTaskList(dbID)
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message & ex.StackTrace)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("Download Zip Asset.", MessageType.MSGINFO)

        Try
            If (Not DownloadAssetZip(dbID)) Then
                DBdata.ErrorDigitalBundle("could not able to download db_pacage.zip file.")
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End If
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message & ex.StackTrace)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        If ((DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then
            Try
                DownloadPrintandCopyRightPDforRL(dbID, $"%_Print.pdf")
            Catch ex As Exception
                DBdata.DeantaBallon("$DownloadPrintandCopyRightPDforRL-{ex.Message & ex.StackTrace}", MessageType.MSGERROR)
            End Try
            Try
                DownloadPrintandCopyRightPDforRL(dbID, $"%_copyright%.pdf")
            Catch ex As Exception
                DBdata.DeantaBallon("$DownloadPrintandCopyRightPDforRL-{ex.Message & ex.StackTrace}", MessageType.MSGERROR)
            End Try
        End If

        GBL.DeantaBallon("Update Page Number.", MessageType.MSGINFO)
        GBL.UpdateGridStatus(dbID, "Update page number")

        If (DBdata.IsXMLGenerated) Then
#If CONFIG <> "Debug" Then
            If (Not GenereateExportXMLFromInDesign(dbID)) Then
                DBdata.ErrorDigitalBundle("error while exporting xml")
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End If
#End If

            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.OrgDocType = DocumentType.CRITICALPUB) Or (DBdata.OrgDocType = DocumentType.BLOOMSBURY)) Then
                Thread.Sleep(10000)
                'GBL.UpdateGridStatus(dbID, "GS Preivew started")
                'Try
                '    CombinedGSPreivewFile(dbID)
                'Catch ex As Exception
                '    DBdata.ErrorDigitalBundle(ex.Message & " - GS Preivew")
                '    GBL.UpdateGridStatus(dbID, "Error")
                'End Try
                'GBL.UpdateGridStatus(dbID, "GS Preivew completed")

                GBL.UpdateGridStatus(dbID, "Down Preivew started")

                Try
                    DownloadAbstractXML(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon($"{ex.Message}", MessageType.MSGERROR)
                End Try
            End If

            Try
                IdentifiyBookElement(DBdata.Folder.ExportXMLPath, dbID)
            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            Try

                eMailto = String.Empty
                XmlStage = String.Empty
                UserName = String.Empty
                eMailto = DBdata.GetToeMailID
                If ((String.Compare(DBdata.TaskName, "first pages to pm And for xml validation", True) = 0) Or (String.Compare(DBdata.TaskName, "first pages typesetting", True) = 0)) Then
                    XmlStage = "First Proof Revert XML"
                    UserName = "FP Team"
                Else
                    XmlStage = "Final Revert XML"
                    UserName = "Production Team"
                End If

                GBL.DeantaBallon("InDesign cleanup conversion started", MessageType.MSGINFO)
                If (DBdata.DocType = DocumentType.CRITICALPUB) Then
                    doInDesignCleanupandConversion(dbID, DBdata.Folder.ExportXMLPath, DBdata.Folder.FinalXMLPath, DocumentType.CRITICALPUB)
                    'GBL.DeantaBallon("InDesign cleanup fail notification", MessageType.MSGINFO)
                    'MailHelper.SendMail($"[InDesign Export XML Fail]::  [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", eMailto, "edelivery@deantaglobal.com", $"Hi {UserName},{vbCrLf}The InDesign export XML was not properly converted as respective client XML. Please check with technology team.{vbCrLf}Regards,{vbCrLf}Digital Bundle Engine.")

                Else
                    doInDesignCleanupandConversion(dbID, DBdata.Folder.ExportXMLPath, DBdata.Folder.FinalXMLPath, DBdata.DocType)
                    'GBL.DeantaBallon("InDesign cleanup fail notification", MessageType.MSGINFO)
                    'MailHelper.SendMail($"[InDesign Export XML Fail]::  [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", eMailto, "edelivery@deantaglobal.com", $"Hi {UserName},{vbCrLf}The InDesign export XML was not properly converted as respective client XML. Please check with technology team.{vbCrLf}Regards,{vbCrLf}Digital Bundle Engine.")
                End If

            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            GBL.DeantaBallon("Final XML conversion started", MessageType.MSGINFO)

            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                eMailto = String.Empty
                XmlStage = String.Empty
                UserName = String.Empty
                eMailto = DBdata.GetToeMailID
                If ((String.Compare(DBdata.TaskName, "first pages to pm And for xml validation", True) = 0) Or (String.Compare(DBdata.TaskName, "first pages typesetting", True) = 0)) Then
                    XmlStage = "First Proof Revert XML"
                    UserName = "FP Team"
                Else
                    XmlStage = "Final Revert XML"
                    UserName = "Production Team"
                End If
                Try
                    If (Not TFClientXMLConversion(dbID, DBdata.Folder.FinalXMLPath)) Then
                        MailHelper.SendMail($"[{XmlStage} Fail]::  [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", eMailto, "edelivery@deantaglobal.com", $"Hi {UserName},{vbCrLf}The Final XML is not converted properly. Please check with technology team.{vbCrLf}Regards,{vbCrLf}Digital Bundle Engine.")
                    End If
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try
            ElseIf ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD) Or (DBdata.OrgDocType = DocumentType.ANTHEM) Or (DBdata.OrgDocType = DocumentType.MUP)) Then
                Try
                    BloomsburyClientXMLConversion(dbID, DBdata.Folder.FinalXMLPath)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

                GBL.DeantaBallon("ePub XML conversion started", MessageType.MSGINFO)

                Try
                    TFClientEPubConversion(dbID, DBdata.Folder.EPubPath)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("ePub XML conversion completed", MessageType.MSGINFO)

            ElseIf ((DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then

                GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))
                GBL.DeantaBallon("RL XML cleanup started.", MessageType.MSGINFO)
                Try
                    RLClientXMLConversion(dbID, DBdata.Folder.FinalXMLPath)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("RL XML cleanup completed", MessageType.MSGINFO)
                GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

                GBL.DeantaBallon("RL ePUb cleanup start", MessageType.MSGINFO)

                Try
                    TFClientEPubConversion(dbID, DBdata.Folder.EPubPath)
                    'RLClientEPubConversion(dbID, DBdata.Folder.EPubPath)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                Try
                    CopyPrintImageFile(DBdata.Folder.EPubPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))
                GBL.DeantaBallon("RL ePUb cleanup completed", MessageType.MSGINFO)
            End If

            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                If ((DBdata.IsWEBPDFGeneratd) Or (String.Compare(DBdata.TaskName, "export xml to eproduct team", True) = 0)) Then
                    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

                    GBL.DeantaBallon("ePub XML conversion started", MessageType.MSGINFO)
                    eMailto = String.Empty
                    XmlStage = String.Empty
                    UserName = String.Empty
                    eMailto = DBdata.GetToeMailID
                    If ((String.Compare(DBdata.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(DBdata.TaskName, "first pages typesetting", True) = 0)) Then
                        XmlStage = "First Proof Revert XML"
                        UserName = "FP Team"
                    Else
                        XmlStage = "Final Revert XML"
                        UserName = "Production Team"
                    End If

                    Try
                        If (Not TFClientEPubConversion(dbID, DBdata.Folder.EPubPath)) Then
                            MailHelper.SendMail($"[Final ePub XML Fail]:: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", eMailto, "techsupport@deantaglobal.com", $"Hi {UserName},{vbCrLf}The ePub XML is not converted properly. Please check with technology team.{vbCrLf}Regards,{vbCrLf}Digital Bundle Engine.")
                        End If
                    Catch ex As Exception
                        DBdata.ErrorDigitalBundle(ex.Message)
                        GBL.UpdateGridStatus(dbID, "Error")
                        Return False
                    End Try

                End If

                GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

                GBL.DeantaBallon("ePub Conversion started", MessageType.MSGINFO)

                Dim ePubConv As New ePubConversion(GBL.DBDataList(dbID).ClientePubXML)
                Try
                    'ePubConv.DoePubConversion()
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("ePub Conversion completed", MessageType.MSGINFO)

            End If

            Try
                If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                    GBL.DeantaBallon("Tandf Upload started.", MessageType.MSGINFO)
                    UploadRequiredFilesForTANDF(dbID, True, DBStage.XMLTRIGGER)
                    GBL.DeantaBallon("Tandf Upload completed.", MessageType.MSGINFO)
                Else

                    If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD)) Then
                        If (Not DBdata.IsWEBPDFGeneratd) Then
                            GBL.DeantaBallon("blloms Upload started.", MessageType.MSGINFO)
                            UploadRequiredFilesForBloomsbury(dbID, True, DBStage.XMLTRIGGER)
                            GBL.DeantaBallon("blloms Upload completed.", MessageType.MSGINFO)
                        End If
                    Else
                        GBL.DeantaBallon("blloms Upload started.", MessageType.MSGINFO)
                        UploadRequiredFilesForBloomsbury(dbID, True, DBStage.XMLTRIGGER)
                        GBL.DeantaBallon("blloms Upload completed.", MessageType.MSGINFO)
                    End If

                End If

            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try


            GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)
            Try
                Dim NotifyMgr As New NotificationManager
                If (Not NotifyMgr.AddNotification(dbID)) Then
                    DBdata.ErrorDigitalBundle("Error while generating notification table")
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End If
            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try
            GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)

            eMailto = String.Empty
            eMailto = DBdata.GetToeMailID
            XmlStage = String.Empty
            UserName = String.Empty
            If ((String.Compare(DBdata.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(DBdata.TaskName, "first pages typesetting", True) = 0)) Then
                XmlStage = "First Proof Revert XML"
                UserName = "FP Team"
            Else
                If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                    XmlStage = "Final Revert XML"
                Else
                    XmlStage = "Revert XML"
                End If

                UserName = "Production Team"
            End If
            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                Try
                    MailHelper.SendMail($"[{XmlStage} for processing]:: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", "edelivery@deantaglobal.com", eMailto, $"Hi Team,{vbCrLf}The Final XML has converted. Please start processing the Final XML and upload to S3.{vbCrLf}Regards,{vbCrLf}{UserName}.", False)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Else
                Dim PManager As String = MySqlHelper.ExecuteScalar($"select user_email from tb_users where user_id in (select project_manager from tb_projects where project_id = {DBdata.ProjectID})")
                If (Not DBdata.IsWEBPDFGeneratd) Then
                    If ((DBdata.OrgDocType = DocumentType.UWIP) Or (DBdata.OrgDocType = DocumentType.PELAGIC)) Then
                        Try
                            MailHelper.SendMail($"[{DBdata.OrgDocType.ToString()} :: {XmlStage} for processing] :: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", $"edelivery@deantaglobal.com,artwork@deantaglobal.com", $"{PManager},{eMailto}", $"Hi Ramasamy,{vbCrLf}Please download the revert XML, exported ePub and Individual PDFs for the subjected title.{vbCrLf}<b>@Art team,</b> Could you please update/confirm about figures for our DB process.{vbCrLf}Regards,{vbCrLf}{DBdata.UserName}.", False)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                    ElseIf (DBdata.OrgDocType = DocumentType.TRD) Then
                        Try
                            MailHelper.SendMail($"[{DBdata.OrgDocType.ToString()} :: {XmlStage} for processing] :: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", $"edelivery@deantaglobal.com,artwork@deantaglobal.com", $"{PManager},{eMailto}", $"Hi Ramasamy,{vbCrLf}Please download the revert XML, exported ePub and Individual PDFs for the subjected title.{vbCrLf}Regards,{vbCrLf}{DBdata.UserName}.", False)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                    Else
                        If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.OrgDocType = DocumentType.ANTHEM) Or (DBdata.OrgDocType = DocumentType.MUP)) Then
                            Try
                                MailHelper.SendMail($"[{DBdata.OrgDocType.ToString()} :: {XmlStage} for processing] :: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", $"edelivery@deantaglobal.com,artwork@deantaglobal.com", $"{PManager},{eMailto}", $"Hi Ramasamy,{vbCrLf}Please download the revert XML, exported ePub and Individual PDFs for the subjected title.{vbCrLf}<b>@Art team,</b> Could you please update/confirm about figures for our DB process.{vbCrLf}Regards,{vbCrLf}{DBdata.UserName}.", False)
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        End If
                    End If
                End If
                If ((DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then 'Mail subject karthik
                    Try
                        MailHelper.SendMail($"[{DBdata.OrgDocType.ToString()} :: {XmlStage} for processing] :: [{GBL.DBDataList(dbID).BookCode}]-{GBL.DBDataList(dbID).ProjectName}", $"edelivery@deantaglobal.com", $"{PManager},{eMailto}", $"Hi Karthik,{vbCrLf}Please download the revert XML and ePub XML for the subjected title.{vbCrLf}Regards,{vbCrLf}{DBdata.UserName}.", False)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                End If

            End If
        End If

        GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

        If (DBdata.IsWEBPDFGeneratd) Then
            GBL.DeantaBallon("Download cover image", MessageType.MSGINFO)
            Try
                DownloadCoverImageFromAsset(dbID)
            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            GBL.DeantaBallon("Download cover image completed", MessageType.MSGINFO)

            GBL.DeantaBallon("WEB PDF creation started", MessageType.MSGINFO)
        End If

        If (DBdata.IsWEBPDFGeneratd) Then
            Try
                CreateBookBasedonMainXML(dbID)
            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            GBL.DeantaBallon("WEB PDF creation completed", MessageType.MSGINFO)

            If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then

                'GBL.DeantaBallon("GS Preview PDF creation started", MessageType.MSGINFO)
                'If ((Not String.IsNullOrEmpty(GBL.DBDataList(dbID).GSPreviewCombinedXML)) AndAlso (File.Exists(GBL.DBDataList(dbID).GSPreviewCombinedXML))) Then
                '    Try
                '        CreateGSPreviewPDFGeneration(dbID)
                '    Catch ex As Exception
                '        DBdata.ErrorDigitalBundle(ex.Message)
                '        GBL.UpdateGridStatus(dbID, "Error")
                '    End Try
                'End If
                'GBL.DeantaBallon("GS Preview PDF creation completed", MessageType.MSGINFO)
            End If

            If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD) Or (DBdata.IsPODGenerated)) Then
                GBL.DeantaBallon("POD PDF generation", MessageType.MSGINFO)
                Try
                    PsToPDFGeneration(dbID)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("POD PDF generation completed.", MessageType.MSGINFO)
            End If

            If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD) Or (DBdata.OrgDocType = DocumentType.MUP)) Then
                GBL.DeantaBallon("Format package folder.", MessageType.MSGINFO)
                Try
                    FormatPackageFolderforBloomsbury(dbID)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try
                GBL.DeantaBallon("Format package folder completed.", MessageType.MSGINFO)
            ElseIf ((DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then
                GBL.DeantaBallon("Format package folder.", MessageType.MSGINFO)
                Try
                    FormatPackageFolderforRandL(dbID)
                Catch ex As Exception
                    DBdata.ErrorDigitalBundle(ex.Message)
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try
                GBL.DeantaBallon("Format package folder completed.", MessageType.MSGINFO)
            End If

            GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))

            Try
                If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD) Or (DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA) Or (DBdata.OrgDocType = DocumentType.MUP)) Then
                    GBL.DeantaBallon("Bloomsbury Upload started", MessageType.MSGINFO)
                    UploadRequiredFilesForBloomsbury(dbID, True, DBStage.WEBTRIGGER)
                    GBL.DeantaBallon("Bloomsbury Upload completed", MessageType.MSGINFO)
                ElseIf ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK) Or (DBdata.DocType = DocumentType.CRITICALPUB)) Then
                    GBL.DeantaBallon("Tandf Upload started.", MessageType.MSGINFO)
                    UploadRequiredFilesForTANDF(dbID, True, DBStage.WEBTRIGGER)
                    GBL.DeantaBallon("Tandf Upload completed.", MessageType.MSGINFO)
                End If

            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)
            Try
                Dim NotifyMgr As New NotificationManager
                If (Not NotifyMgr.AddNotification(dbID)) Then
                    DBdata.ErrorDigitalBundle("Error while generating notification table")
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End If
            Catch ex As Exception
                DBdata.ErrorDigitalBundle(ex.Message)
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try
            GBL.DeantaBallon("Notification completed.", MessageType.MSGINFO)

        End If

        MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=1, end_date='" & GBL.GetPdfTime & "',remarks='" & File.ReadAllText(GBL.LogFilePath).Replace("'", "''") & "' where digitalbundle_id=" & DBdata.DigitalID)
        GBL.DBDataList(dbID).IsProcessCompleted = True
        GBL.UpdateGridStatus(dbID, "Completed.")

#If CONFIG <> "Debug" Then
        Try
            CommMgr.EndLanstadTask()
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
#End If

        Try
            If ((GBL.DBDataList(dbID).FinalAssets IsNot Nothing) AndAlso (GBL.DBDataList(dbID).FinalAssets.Count > 0)) Then
                If (From n In GBL.DBDataList(dbID).FinalAssets Where n.DBTaskType = DigitalBundleTask.MATHTYPE Select n.FinalFileName).Any Then
                    CommMgr.MathTypeEmailToArtwork()
                End If
            End If
        Catch ex As Exception
            DBdata.ErrorDigitalBundle(ex.Message)
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
        Return True
    End Function

    Private Sub ProgressChanged(ByVal ProVal As Int16)
        TssLabel.Text = $"{ProVal} %"
        TssPgBar.Value = ProVal
    End Sub

    Private Function CopyPrintImageFile(ByVal ePubFolder As String) As Boolean
        Dim PrintFolder As String = String.Empty
        Dim XMLImgFolder As String = String.Empty
        XMLImgFolder = Path.Combine(ePubFolder, "images")
        PrintFolder = Path.Combine(Path.GetDirectoryName(ePubFolder), "WEBPDF")
        PrintFolder = Path.Combine(PrintFolder, "images_web")
        If (Not Directory.Exists(PrintFolder)) Then
            GBL.DeantaBallon($"Could not able to find the print images folder.- {PrintFolder}", MessageType.MSGERROR)
            Return False
        End If
        If (Not Directory.Exists(XMLImgFolder)) Then
            Directory.CreateDirectory(XMLImgFolder)
        End If
        Try
            For Each imgFile As String In Directory.GetFiles(PrintFolder, "*.*", SearchOption.TopDirectoryOnly)
                File.Copy(imgFile, Path.Combine(XMLImgFolder, Path.GetFileName(imgFile)))
            Next
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    ''' <summary>
    ''' This function is used to debug the final XML and ePub for all clients
    ''' </summary>
    ''' <param name="dbID">index of the list in the Digital bundle process</param>
    ''' <returns></returns>
    Private Function FinalXML(dbID As Integer) As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        DBdata = GBL.DBDataList(dbID)
        GBL.DBDataList(dbID).TaskList.Clear()
        GBL.LogFilePath = "D:\DDrive\Support\20-06-2022\IDOV\FinalXML\error.txt"

        'IdentifiyBookElement("D:\DDrive\Samples\WEBPDF_Lasntad\TNF\367290\ExportXML", 0)

        Try
            doInDesignCleanupandConversion(0, "D:\DDrive\Support\13-02-2023\LOAU\ExportXML", "D:\DDrive\Support\13-02-2023\LOAU\FinalXML", LanstadClientType.TANDF)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        'Try
        '    GBL.DBDataList(dbID).ProjectID = "243842"
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\08-04-2021\TMMC\" '"D:\DDrive\Samples\WEBPDF_Lasntad\TNF\K14510"
        '    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))
        '    CombinedGSPreivewFile(dbID)
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

#Region "TNFFinalXMLandePub"

        Try
            GBL.DBDataList(dbID).ProjectID = "250274"
            GBL.DBDataList(dbID).MainXML = ""
            'GBL.DBDataList(dbID).MainXML = "D:\DDrive\Support\29-09-2022\PRMI\main.xml"
            GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
            GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER
            GBL.DBDataList(dbID).AbstractXML = ""
            GBL.DBDataList(dbID).DocType = DocumentType.TANDF
            GBL.DBDataList(dbID).ApplicationISBN = "9781032309507"
            GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\24-02-2023\SECL\9780367769154_xml"
            Try
                DownloadAbstractXML(dbID)
            Catch ex As Exception
                GBL.DeantaBallon($"{ex.Message}", MessageType.MSGERROR)
            End Try
            TFClientXMLConversion(dbID, "D:\DDrive\Support\24-02-2023\SECL\9780367769154_xml")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        'Dim FolderList As New List(Of String)
        'FolderList.AddRange(New String() {"D:\DDrive\Support\24-02-2023\Inline_Images\ePub"})
        'For Each SFolder As String In FolderList
        '    Try
        '        GBL.DBDataList(dbID).MainXML = ""
        '        GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER
        '        GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '        GBL.DBDataList(dbID).DocType = DocumentType.RL
        '        GBL.DBDataList(dbID).OrgDocType = DocumentType.BLOOMSBURY
        '        GBL.DBDataList(dbID).Folder.WorkingPath = SFolder
        '        TFClientEPubConversion(dbID, SFolder)
        '    Catch ex As Exception
        '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    End Try
        'Next

#End Region

#Region "Bloomsbury_FinalXML"
        Dim BloomsMgr As New BloomsburyCleanupManager
        'Try
        '    BloomsMgr.UpdateIbidinFootnote("D:\DDrive\Support\24-02-2023\Ibid_TNF_Sample\9781003095651.xml")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Try
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\24-02-2023\Inline_Images\ExportXML"
        '    GBL.DBDataList(dbID).MainXML = ""
        '    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).Folder.WorkingPath, String.Format("{0}.txt", DBdata.DigitalID))
        '    GBL.DBDataList(dbID).AbstractXML = ""
        '    GBL.DBDataList(dbID).ApplicationISBN = "1234567891411"
        '    BloomsburyClientXMLConversion(dbID, "D:\DDrive\Support\24-02-2023\Inline_Images\ExportXML")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Try
        '    GBL.DBDataList(dbID).MainXML = "D:\DDrive\Support\20-10-2021\RETU\ExportXML\main.xml"
        '    GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).DocType = DocumentType.BLOOMSBURY
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\20-10-2021\RETU\EPub"
        '    TFClientEPubConversion(dbID, "D:\DDrive\Support\20-10-2021\RETU\EPub")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

#End Region


#Region "RL_FinalXMLePub"

        'Try
        '    GBL.DBDataList(dbID).MainXML = ""
        '    GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER
        '    GBL.DBDataList(dbID).ApplicationISBN = "9781666928044"
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\19-01-2023\MAJO\FinalXML"
        '    GBL.DBDataList(dbID).ProjectName = "Leonard Bonapartism_9781666928044"
        '    RLClientXMLConversion(dbID, "D:\DDrive\Support\19-01-2023\MAJO\FinalXML")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Dim FolderList As New List(Of String)
        'FolderList.AddRange(New String() {"D:\DDrive\Support\07-02-2022\INDI\9781000573466_ePub"})

        'For Each SFolder As String In FolderList
        '    Try
        '        GBL.DBDataList(dbID).MainXML = ""
        '        GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER
        '        GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '        GBL.DBDataList(dbID).DocType = DocumentType.RL
        '        GBL.DBDataList(dbID).OrgDocType = DocumentType.RL
        '        GBL.DBDataList(dbID).Folder.WorkingPath = SFolder
        '        TFClientEPubConversion(dbID, SFolder)
        '    Catch ex As Exception
        '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    End Try
        'Next

#End Region


#Region "Edward Elyar Excelsheet generation"
        'Try
        '    GBL.DBDataList(dbID).MainXML = "D:\DDrive\Support\16-07-2022\EEP\Excel-Sheet\ExportXML\main.xml"
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "D:\DDrive\Support\16-07-2022\EEP\Excel-Sheet"
        '    DoGenerateMetadataExcelReport(dbID)
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

#End Region
        Return True
    End Function

    Private Function DoGenerateMetadataExcelReport(ByVal dbID As Integer) As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        DBdata = GBL.DBDataList(dbID)
        MsgBox(DBdata.Folder.FinalXMLPath)

        MsgBox(DBdata.Folder.WorkingPath)
        Return True
    End Function
    Private Function OLD_DoGenerateDigitalBundle(ByVal dbID As Integer) As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        DBdata = GBL.DBDataList(dbID)
        GBL.DBDataList(dbID).TaskList.Clear()

        If (GBL.DBDataList(dbID).IsProcessCompleted) Then
            Return False
        End If

#If CONFIG = "FinalXML" Then

        'Try
        '    doInDesignCleanupandConversion(0, "D:\DDrive\Support\21-12-2020\SOPI\ExportXML", "D:\DDrive\Support\21-12-2020\SOPI\FinalXML", LanstadClientType.TANDFUK)
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Try
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "C:\InDesignEngine\473332\47333211185690\FinalXML"
        '    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))
        '    GBL.DBDataList(dbID).ApplicationISBN = "1234567891411"
        '    TFClientXMLConversion(dbID, "C:\InDesignEngine\473332\47333211185690\FinalXML")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Try
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "C:\InDesignEngine\339929\33992914363661\ExportXML"
        '    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))
        '    GBL.DBDataList(dbID).ApplicationISBN = "1234567891411"
        '    BloomsburyClientXMLConversion(dbID, "C:\InDesignEngine\339929\33992914363661\FinalXML")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        'Try
        '    GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)
        '    GBL.DBDataList(dbID).Folder.WorkingPath = "C:\InDesignEngine\473332\47333211185690\EPub\"
        '    GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))
        '    TFClientEPubConversion(dbID, "C:\InDesignEngine\473332\47333211185690\EPub\")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try
        Exit Function
#End If

        GBL.UpdateGridStatus(dbID, "create folder strcuture")
        GBL.DBDataList(dbID).Folder = New DBFolderStructure(DBdata.TaskID, DBdata.DigitalID)

        If (Not (GBL.DBDataList(dbID).Folder.CreateRequiredFolder())) Then
            GBL.DeantaBallon("Error occured while creating the folder strucuture for DB", MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='Error occured while creating the folder strucuture for DB' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End If

        GBL.DBDataList(dbID).WorkPath = DBdata.Folder.WorkingPath
        GBL.LogFilePath = Path.Combine(GBL.DBDataList(dbID).WorkPath, String.Format("{0}.txt", DBdata.DigitalID))

        Try
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set bookpdfpath='" & GBL.LogFilePath.Replace("\", "\\") & "' where digitalbundle_id=" & DBdata.DigitalID)
        Catch ex As Exception
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("index : " & dbID, MessageType.MSGINFO)
        GBL.UpdateGridStatus(dbID, "identifiy required files")
        GBL.DeantaBallon("Update client doctype.", MessageType.MSGINFO)

        Try
            UpdateClientandDocumentType(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("create task list.", MessageType.MSGINFO)

        Try
            CreateTaskList(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try


        GBL.DeantaBallon("download main xml.", MessageType.MSGINFO)

        If ((GBL.DBDataList(dbID).TaskList IsNot Nothing) AndAlso (GBL.DBDataList(dbID).TaskList.Count > 0)) Then
            If (GBL.DBDataList(dbID).Stage <> DBStage.XMLTRIGGER) Then

                Try
                    DownloadMainXML(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon("DownloadMainXML - " & ex.Message, MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                    'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("download cover image.", MessageType.MSGINFO)

                Try
                    'DownloadCoverImage(dbID)
                    DownloadCoverImageFromAsset(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                    'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.DeantaBallon("identify book elements.", MessageType.MSGINFO)

                Try
                    'IdentifiyBookElement("C:\InDesignEngine\1522\tmp3544.tmp\LXEXML", index)
                    IdentifiyBookElement(DBdata.Folder.LXEXMLPath, dbID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                    'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try
                'GBL.DeantaBallon("download lxe xml.", MessageType.MSGINFO)
                'If (DBdata.IsXMLGenerated) Then
                '    Try
                '        DoDownloadLXEXMLFiles(dbID)
                '    Catch ex As Exception
                '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                '        MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "' where digitalbundle_id=" & DBdata.DigitalID)
                '        'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                '        GBL.DBDataList(dbID).IsProcessCompleted = True
                '        GBL.UpdateGridStatus(dbID, "Error")
                '        Continue For
                '    End Try
                'End If

                GBL.UpdateGridStatus(dbID, "download required files")

                GBL.DeantaBallon("download assets.", MessageType.MSGINFO)
                Try
                    DoDownloadInDesignDocumentAsset(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                    'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

                GBL.UpdateGridStatus(dbID, "Book creation")

                GBL.DeantaBallon("digitial bundle started.", MessageType.MSGINFO)

            Else
                Try
                    DownloadAssetZip(dbID)
                Catch ex As Exception
                    GBL.DeantaBallon("DownloadMainXML - " & ex.Message, MessageType.MSGERROR)
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                    'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                    GBL.DBDataList(dbID).IsProcessCompleted = True
                    GBL.UpdateGridStatus(dbID, "Error")
                    Return False
                End Try

            End If

        End If

        GBL.DeantaBallon("Update Page Number.", MessageType.MSGINFO)
        GBL.UpdateGridStatus(dbID, "Update page number")

        If (Not GenereateExportXMLFromInDesign(dbID)) Then
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='error while exporting xml' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End If

        If (GBL.DBDataList(dbID).Stage = DBStage.XMLTRIGGER) Then
            Try
                IdentifiyBookElement(DBdata.Folder.ExportXMLPath, dbID)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try
        End If

        Try
#If CONFIG = "Debug" Then
            'doInDesignCleanupandConversion("C:\InDesignEngine\446442\tmp9AF8.tmp\ExportXML", "C:\InDesignEngine\446442\tmp9AF8.tmp\FinalXML", LanstadClientType.TANDF)
            'doInDesignCleanupandConversion(DBdata.Folder.ExportXMLPath, DBdata.Folder.FinalXMLPath, LanstadClientType.TANDF)
#Else
            GBL.DeantaBallon("InDesign cleanup conversion started", MessageType.MSGINFO)
            doInDesignCleanupandConversion(0, DBdata.Folder.ExportXMLPath, DBdata.Folder.FinalXMLPath, LanstadClientType.TANDF)
#End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("Final XML conversion started", MessageType.MSGINFO)

        If ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.TANDFUK)) Then
            Try
                TFClientXMLConversion(dbID, DBdata.Folder.FinalXMLPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try
        Else
            Try
                BloomsburyClientXMLConversion(dbID, DBdata.Folder.FinalXMLPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try
        End If

        GBL.DeantaBallon("ePub XML conversion started", MessageType.MSGINFO)

        Try
            TFClientEPubConversion(dbID, DBdata.Folder.EPubPath)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("ePub Conversion started", MessageType.MSGINFO)

        Dim ePubConv As New ePubConversion(GBL.DBDataList(dbID).ClientePubXML)
        Try
            'ePubConv.DoePubConversion()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("ePub Conversion completed", MessageType.MSGINFO)
        If (DBdata.Stage = DBStage.XMLTRIGGER) Then
            GBL.DeantaBallon("Download cover image", MessageType.MSGINFO)
            Try
                DownloadCoverImageFromAsset(dbID)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End Try

            GBL.DeantaBallon("Download cover image completed", MessageType.MSGINFO)

        End If
        GBL.DeantaBallon("WEB PDF creation started", MessageType.MSGINFO)
        Try
            CreateBookBasedonMainXML(dbID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("WEB PDF creation completed", MessageType.MSGINFO)

        If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD)) Then
            GBL.DeantaBallon("POD PDF generation", MessageType.MSGINFO)
            Try
                PsToPDFGeneration(dbID)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            GBL.DeantaBallon("POD PDF generation completed.", MessageType.MSGINFO)
        End If

        'Try
        '    If (Not AddMetadata(dbID)) Then
        '        GBL.DeantaBallon("Error occurred while add metadata.", MessageType.MSGERROR)
        '    End If
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD)) Then
            GBL.DeantaBallon("Format package folder.", MessageType.MSGINFO)
            Try
                FormatPackageFolderforBloomsbury(dbID)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            GBL.DeantaBallon("Format package folder completed.", MessageType.MSGINFO)
        End If


        Try
            If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD)) Then
                GBL.DeantaBallon("Bloomsbury Upload started", MessageType.MSGINFO)
                UploadRequiredFilesForBloomsbury(dbID)
                GBL.DeantaBallon("Bloomsbury Upload completed", MessageType.MSGINFO)
            ElseIf ((DBdata.DocType = DocumentType.TANDF) Or (DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA) Or (DBdata.DocType = DocumentType.TANDFUK)) Then
                GBL.DeantaBallon("Tandf Upload started.", MessageType.MSGINFO)
                UploadRequiredFilesForTANDF(dbID, True)
                GBL.DeantaBallon("Tandf Upload completed.", MessageType.MSGINFO)
            End If

        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try

        GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)
        Try
            Dim NotifyMgr As New NotificationManager
            If (Not NotifyMgr.AddNotification(dbID)) Then
                GBL.DeantaBallon("Error while generating notification table", MessageType.MSGERROR)
                MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='Error while generating notification table' where digitalbundle_id=" & DBdata.DigitalID)
                'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
                GBL.DBDataList(dbID).IsProcessCompleted = True
                GBL.UpdateGridStatus(dbID, "Error")
                Return False
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=-1, end_date='" & GBL.GetPdfTime & "',remarks='" & ex.Message & "' where digitalbundle_id=" & DBdata.DigitalID)
            'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
            GBL.DBDataList(dbID).IsProcessCompleted = True
            GBL.UpdateGridStatus(dbID, "Error")
            Return False
        End Try
        GBL.DeantaBallon("Notification started.", MessageType.MSGINFO)
        MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set status=1, end_date='" & GBL.GetPdfTime & "',remarks='" & File.ReadAllText(GBL.LogFilePath).Replace("'", "''") & "' where digitalbundle_id=" & DBdata.DigitalID)
        'MySqlHelper.ExecuteNonQuery("Update tb_tasks set status_id=1,engine_process=1 where task_id=" & DBdata.TaskID)
        GBL.DBDataList(dbID).IsProcessCompleted = True
        GBL.UpdateGridStatus(dbID, "Completed.")
        Return True
    End Function
    Public Function AddMetadata(ByVal PDFPath As String, ByVal outPutFile As String) As Boolean
        Dim reader As PdfReader = New PdfReader(PDFPath)
        Dim stamp As PdfStamper = New PdfStamper(reader, New FileStream(outPutFile, FileMode.Create))
        Dim D As PdfDestination = New PdfDestination(PdfDestination.FIT, reader.GetPageSize(1).Left, reader.GetPageSize(1).Height, 1.0F)
        Dim OA As PdfAction = PdfAction.GotoLocalPage(1, D, stamp.Writer)
        stamp.Writer.SetOpenAction(OA)
        stamp.Writer.ViewerPreferences = PdfWriter.PageLayoutOneColumn
        stamp.Writer.ViewerPreferences = PdfWriter.PageModeUseOutlines
        stamp.Writer.ViewerPreferences = PdfWriter.DisplayDocTitle
        stamp.Writer.ViewerPreferences = PdfWriter.PageModeUseOutlines Or PdfWriter.PageLayoutSinglePage
        stamp.Writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_7)
        stamp.Writer.ExtraCatalog.Put(PdfName.LANG, New PdfString("EN"))
        stamp.Writer.GetVerticalPosition(False)
        stamp.Close()
        reader.Close()
        Return True
    End Function

    Public Function AddMetadata(ByVal Index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(Index)
        Dim InPDFFile As String = String.Empty
        Dim OutPutFile As String = String.Empty

        GBL.DeantaBallon("Metadata started.", MessageType.MSGERROR)

        If (Not Directory.Exists(DBData.Folder.DeliveryWEBPDF)) Then
            GBL.DeantaBallon("Could not able to find the web pdf folder" & DBData.Folder.DeliveryWEBPDF, MessageType.MSGERROR)
            Return False
        End If

        Dim WEBPDFFile As String = GetFileFromFolder(DBData.Folder.DeliveryWEBPDF, "*.pdf")

        InPDFFile = WEBPDFFile.Replace(".pdf", "BeforeMeta.pdf")
        OutPutFile = WEBPDFFile
        Try
            File.Copy(WEBPDFFile, InPDFFile, True)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        If (Not File.Exists(InPDFFile)) Then
            GBL.DeantaBallon("Could not able to find the beforemeta pdf file.", MessageType.MSGERROR)
            Return False
        End If
        Dim reader As PdfReader = New PdfReader(InPDFFile)
        Dim stamp As PdfStamper = New PdfStamper(reader, New FileStream(OutPutFile, FileMode.Create))
        Dim D As PdfDestination = New PdfDestination(PdfDestination.FITBH)
        Dim OA As PdfAction = PdfAction.GotoLocalPage(1, D, stamp.Writer)
        stamp.Writer.SetOpenAction(OA)
        stamp.Writer.ViewerPreferences = PdfWriter.PageLayoutOneColumn
        stamp.Writer.ViewerPreferences = PdfWriter.PageModeUseOutlines
        stamp.Writer.ViewerPreferences = PdfWriter.DisplayDocTitle
        stamp.Writer.ExtraCatalog.Put(PdfName.LANG, New PdfString("EN"))
        stamp.Close()
        reader.Close()
        Try
            File.Move(InPDFFile, InPDFFile.Replace(".pdf", "bak"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Return True
    End Function

    Private Function PsToPDFGeneration(index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim PSFile As String = String.Empty
        If (Not Directory.Exists(DBData.Folder.PODPdfPath)) Then
            GBL.DeantaBallon("Could not able to find the POD directory.", MessageType.MSGERROR)
            Return False
        End If
        PSFile = GetFileFromFolder(DBData.Folder.PODPdfPath, "*.ps")
        If (Not File.Exists(PSFile)) Then
            GBL.DeantaBallon("Could not able to find the PS File.", MessageType.MSGERROR)
            Return False
        End If
        'acrodist.exe
        'check the distriller Is open Or Not.
        Dim Distriller As New List(Of Process)
        Distriller.AddRange(Process.GetProcessesByName("acrodist"))
        Try
            If ((Distriller Is Nothing) AndAlso (Distriller.Count = 0)) Then
                Process.Start("C:\Program Files (x86)\Adobe\Acrobat 10.0\Acrobat\acrodist.exe")
            Else
                GBL.DeantaBallon("Distriller open.", MessageType.MSGINFO)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Dim MaxCount As Integer = 0
        Dim OutputPSFile As String = String.Empty
        OutputPSFile = Path.Combine(Path.Combine(GBL.PsWatchPath, "out"), Path.GetFileName(PSFile))
        If File.Exists(Path.Combine(Path.Combine(GBL.PsWatchPath, "in"), Path.GetFileName(PSFile))) Then
            File.Delete(Path.Combine(Path.Combine(GBL.PsWatchPath, "in"), Path.GetFileName(PSFile)))
        End If
        File.Copy(PSFile, Path.Combine(Path.Combine(GBL.PsWatchPath, "in"), Path.GetFileName(PSFile)), True)
        OutputPSFile = OutputPSFile.Replace(".ps", ".pdf")
        Thread.Sleep(1000)
        Try
            While (MaxCount < 60)
                If (File.Exists(OutputPSFile)) Then
                    Exit While
                End If
                MaxCount = MaxCount + 1
                Thread.Sleep(1000)
            End While
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        If (Not File.Exists(OutputPSFile)) Then
            GBL.DeantaBallon("Could not find the POD file : " & OutputPSFile, MessageType.MSGINFO)
            Return False
        End If
        File.Copy(OutputPSFile, Path.Combine(DBData.Folder.PODPdfPath, Path.GetFileName(OutputPSFile)), True)
        GBL.DeantaBallon("POD file : " & OutputPSFile, MessageType.MSGINFO)
        Return True
    End Function

    Private Function DownloadAbstractXML(ByVal index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim StatusUI As New UploadDownloadHelper
        Dim AbstData As String = String.Empty
        Dim AbstrFile As String = String.Empty
        AbstData = MySqlHelper.ExecuteScalar("select document_path from tb_documents where project_id=" & DBData.ProjectID & " and (document_name like '%_abst_docbook.xml%' or document_name like '%_abs_docbook.xml%') and document_type = '.xml'")
        If (String.IsNullOrEmpty(AbstData)) Then
            MailHelper.SendMail($"[Abstract XML not available]:: [{DBData.BookCode}]-{DBData.ProjectName}", "production3@deantaglobal.com", "edelivery@deantaglobal.com", $"Hi Team,{vbCrLf}The Chapter abstract documnet was not processed for this title.{vbCrLf}Please process the docuemnt and generate XML as well as upload to lanstad.{vbCrLf}", False)
            Return False
        End If
        AbstrFile = AbstData.Replace("resources/", "")
        If (Not String.IsNullOrEmpty(AbstrFile)) Then
            AbstrFile = Path.Combine(GBL.FTPResourcePath, AbstrFile).Replace("\", "/")
        End If
        If (Not Directory.Exists(DBData.Folder.WorkingPath)) Then
            Directory.CreateDirectory(DBData.Folder.WorkingPath)
        End If
        Try
#If CONFIG = "FinalXML" Then
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = Path.GetDirectoryName(DBData.Folder.WorkingPath), .UrlPath = AbstrFile, .Index = index})
            AzureHelper.DownloadFile(Path.GetFileName(AbstrFile), Path.GetDirectoryName(DBData.Folder.WorkingPath), LanstadPathOptions.ASSETPATH, String.Empty)
#Else
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBData.Folder.WorkingPath, .UrlPath = AbstrFile, .Index = index})
            AzureHelper.DownloadFile(Path.GetFileName(AbstrFile), DBData.Folder.WorkingPath, LanstadPathOptions.ASSETPATH, String.Empty)
#End If

            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
#If CONFIG = "FinalXML" Then
        AbstrFile = Path.Combine(Path.GetDirectoryName(DBData.Folder.WorkingPath), Path.GetFileName(AbstrFile))
#Else
        AbstrFile = Path.Combine(DBData.Folder.WorkingPath, Path.GetFileName(AbstrFile))
#End If

        If (Not File.Exists(AbstrFile)) Then
            GBL.DeantaBallon("Could not able to find the abstract document.", MessageType.MSGERROR)
            Return False
        End If
        GBL.DBDataList(index).AbstractXML = AbstrFile
        Return True
    End Function
    Private Function GenereateExportXMLFromInDesign(ByVal index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim BookFile As String = String.Empty
        Dim BookArgs As String = String.Empty
        Dim Server As String = "http://localhost:55508"
        For Each IndbFile As String In Directory.GetFiles(DBData.Folder.ApplicationPath, "*.indb", SearchOption.TopDirectoryOnly)
            BookFile = IndbFile
            Exit For
        Next
        If (GBL.DBDataList(index).Stage = DBStage.XMLTRIGGER) Then
            If (String.IsNullOrEmpty(BookFile)) Then
                GBL.DeantaBallon("Could not able to find the InDesign Book in the path.", MessageType.MSGERROR)
                Return False
            End If
        End If
        BookArgs = GBL.InDesignServerSoap & " -host """ & Server & """ """ & GBL.ExportXMLScript & """ logPath=""" & Path.Combine(DBData.Folder.WorkingPath, "exportxml.log") & """ docType=""" & DBData.DocType & """ exportXMLPath=""" & DBData.Folder.ExportXMLPath.Replace("\", "/") & "/" & """ gsPreviewXMLPath=""" & DBData.Folder.GSPreviewXMLPath.Replace("\", "/") & "/" & """ bookFilePath=""" & BookFile.Replace("\", "/") & """ indesignPath=""" & DBData.Folder.ApplicationPath.Replace("\", "/") & "/" & """ chunkISBN=""" & DBData.ApplicationISBN & """ bookName=""" & DBData.Folder.WEBPDFName & """ fileOrderList=""" & String.Join("|", DBData.InDesignFileList.ToArray()) & """"
        If (Not CreateBatAndRunFile(BookArgs, DBData.Folder.WorkingPath, "export_xml.bat")) Then
            GBL.DeantaBallon("Error occurred while create bat file.", MessageType.MSGERROR)
            Return False
        End If

        If (Not Directory.Exists(DBData.Folder.ExportXMLPath)) Then
            Return False
        End If
        If ((From n In DBData.InDesignFileList Where Not File.Exists(Path.Combine(DBData.Folder.ExportXMLPath, Path.GetFileName(n).Replace(".indd", "_out.xml"))) Select n).Any) Then
            GBL.DeantaBallon("Error occured in some files, while export xml.", MessageType.MSGERROR)
            Return False
        End If
        'If (GBL.DBDataList(index).Stage <> DBStage.XMLTRIGGER) Then
        '    Dim MainXML As String = GetFileFromFolder(DBData.Folder.ExportXMLPath, "Main.xml")
        '    If (File.Exists(MainXML)) Then
        '        File.Delete(MainXML)
        '    End If
        'End If
        Return True
    End Function

    Private Function TFBitsCleanup(ByVal tfXMLFile As String, ByVal tfOutXML As String) As Boolean
        Dim xmlContent As String = File.ReadAllText(tfXMLFile)

        While xmlContent.Contains(vbCr)
            xmlContent = xmlContent.Replace(vbCr, " ")
        End While

        While xmlContent.Contains(vbLf)
            xmlContent = xmlContent.Replace(vbLf, " ")
        End While

        While xmlContent.Contains(vbNewLine)
            xmlContent = xmlContent.Replace(vbNewLine, " ")
        End While
        While xmlContent.Contains("  ")
            xmlContent = xmlContent.Replace("  ", " ")
        End While

        While xmlContent.Contains("> <")
            xmlContent = xmlContent.Replace("> <", "><")
        End While

        xmlContent = xmlContent.Replace("<!--<!DOCTYPE book SYSTEM ""TFB.dtd"">-->", "")

        xmlContent = xmlContent.Replace("<uri xlink:href=""doi: 10.1111/ina.12232."">doi: 10.1111/ina.12232.</uri>", "doi: 10.1111/ina.12232.")
        xmlContent = xmlContent.Replace("<label />", "")
        xmlContent = Regex.Replace(xmlContent, "<notes notes-type=""tfb-origin""[\s]*/>", "<notes notes-type=""supplier""><p>Denata</p></notes>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        xmlContent = Regex.Replace(xmlContent, "(<title[^>]*>(((?!<(\/)?title>).)*)<\/title>)", AddressOf RemoveBoldTag, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = Regex.Replace(xmlContent, "(<label[^>]*>((?!<(\/)?label>).)*<\/label>)", AddressOf RemoveBoldTag, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = Regex.Replace(xmlContent, "<graphic xlink:href=""ITX000x001.jpg""[\s]*/>", "<graphic xlink:href=""fig0_1.jpg""/>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = Regex.Replace(xmlContent, "<graphic xlink:href=""ITX000x002.jpg""[\s]*/>", "<graphic xlink:href=""fig0_2.jpg""/>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = xmlContent.Replace("<title>BASIC FACTS FOR SAFE HOSPITALS<sup>5</sup></title>", "<title>Basic Facts for Safe Hospitals<sup>5</sup></title>")
        xmlContent = xmlContent.Replace("<title>LEED 2009 FOR HEALTHCARE PROJECT CHECKLIST<sup>4</sup></title>", "<title>Leed 2009 for Healthcare Project Checklist<sup>4</sup></title>")
        xmlContent = xmlContent.Replace("<title><target target-type=""page"" id=""page_45"">45</target>LEED 2009 FOR HEALTHCARE: OVERVIEW AND PROCESS<sup>4</sup></title>", "<title><target target-type=""page"" id=""page_45"">45</target>Leed 2009 for Healthcare: Overview and Process<sup>4</sup></title>")
        xmlContent = xmlContent.Replace("<publisher>", "<isbn publication-format=""ebk"">9781138032262</isbn><isbn publication-format=""hbk"">9781315393506</isbn><publisher>")
        xmlContent = xmlContent.Replace("<sec sec-type=""imprint-other""><p>CRC Press is an imprint of Taylor ", "<sec sec-type=""imprint-statement""><p>CRC Press is an imprint of Taylor ")
        xmlContent = xmlContent.Replace("<sec sec-type=""imprint-other""><p><bold>Library of Congress", "<sec sec-type=""imprint-cip-data""><p><bold>Library of Congress")
        xmlContent = xmlContent.Replace("<sec sec-type=""imprint-other""><p>This book contains information obtained from", "<sec sec-type=""imprint-reproduction""><p>This book contains information obtained from")
        xmlContent = xmlContent.Replace("<title>Acknowledgments</title>", "<title><target target-type=""page"" id=""page_xxv"">xxv</target>Acknowledgments</title>")
        xmlContent = xmlContent.Replace("<uri xlink:href=""LEED 2009 for Healthcare. www.usgbc.org"">LEED 2009 for Healthcare. www.usgbc.org</uri>", "LEED 2009 for Healthcare. <uri xlink:href=""www.usgbc.org"">www.usgbc.org</uri>")
        xmlContent = Regex.Replace(xmlContent, "<year[\s]*/> September 2004.", " September <year>2004</year>.", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = xmlContent.Replace("<string-name><given-names>Singh</given-names><surname>VK</surname>.</string-name>", "<string-name><surname>Singh</surname><given-names>VK</given-names>.</string-name>")
        xmlContent = xmlContent.Replace("<string-name><surname>Sadler</surname><given-names>BL.</given-names></string-name>", "<string-name><surname>Sadler</surname><given-names>BL</given-names>.</string-name>")
        xmlContent = xmlContent.Replace("<person-group person-group-type=""author""><string-name><surname>Reiling</surname><given-names>J.G. 2005.</given-names></string-name></person-group>", "<person-group person-group-type=""author""><string-name><surname>Reiling</surname><given-names>J.G.</given-names></string-name></person-group><year>2005</year>.")
        xmlContent = xmlContent.Replace("<string-name><given-names>Click here to enter text.</given-names><surname>Talati S</surname></string-name>", "<string-name><given-names>S</given-names><surname>Talati</surname></string-name>")
        xmlContent = xmlContent.Replace("<string-name><given-names>Click here to enter text.</given-names><surname>Bhatia P</surname></string-name>", "<string-name><given-names>P</given-names><surname>Bhatia</surname></string-name>")
        xmlContent = xmlContent.Replace("<string-name><given-names>Click here to enter text.</given-names><surname>Kumar A</surname></string-name>", "<string-name><given-names>A</given-names><surname>Kumar</surname></string-name>")
        xmlContent = xmlContent.Replace("<string-name><given-names>Click here to enter text.</given-names><surname>Gupta AK,</surname></string-name>", "<string-name><given-names>AK</given-names><surname>Gupta</surname></string-name>")
        xmlContent = xmlContent.Replace("<string-name><surname>Click here to enter text</surname>.<given-names>Ojha D.</given-names></string-name>", "<string-name><given-names>D.</given-names><surname>Ojha</surname></string-name>")
        xmlContent = xmlContent.Replace("<book-title><bold>Planning and Designing Healthcare Facilities</bold></book-title>", "<book-title>Planning and Designing Healthcare Facilities</book-title>")
        xmlContent = xmlContent.Replace("<target target-type=""page"" id=""page_50"">50</target>LEED 2009 FOR HEALTHCARE</title><p>100 base points; 6 possible Innovation in Design and 4 Regional Priority points</p>", "LEED 2009 FOR HEALTHCARE</title><p><target target-type=""page"" id=""page_50"">50</target>100 base points; 6 possible Innovation in Design and 4 Regional Priority points</p>")
        xmlContent = xmlContent.Replace(" and environment-friendly manner.</p>", " and environment-friendly manner.<xref rid=""ch5-CIT00002"" ref-type=""bibr""><sup>2</sup></xref></p>")
        xmlContent = xmlContent.Replace("Can a building help cure you?</p>", "Can a building help cure you?<xref rid=""ch5-CIT00004"" ref-type=""bibr""><sup>4</sup></xref></p>")
        xmlContent = xmlContent.Replace("<target target-type=""page"" id=""page_xv"">xv</target>Foreword</title></title-group>", "<target target-type=""page"" id=""page_xv"">xv</target>Foreword</title></title-group><contrib-group><contrib contrib-type=""author""><name name-style=""western""><surname>Singh</surname><given-names>V.K.</given-names></name></contrib><contrib contrib-type=""author""><name name-style=""western""><surname>Paul</surname><given-names>Lillrank</given-names></name></contrib></contrib-group>")
        xmlContent = Regex.Replace(xmlContent, "<graphic xlink:href=""ITX000x001.jpgimage/jpeg.tif""[\s]*/>", "<graphic xlink:href=""ITX000x001.jpg"" />", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        xmlContent = xmlContent.Replace("<title>Examples of Demand Categories and Corresponding Value Propositions, Key Resources, and Key Activities</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""0"">", "<title>Examples of Demand Categories and Corresponding Value Propositions, Key Resources, and Key Activities</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""4""><oasis:colspec colnum=""1"" colname=""col1"" align=""left""/><oasis:colspec colnum=""2"" colname=""col2"" align=""left""/><oasis:colspec colnum=""3"" colname=""col3"" align=""left""/><oasis:colspec colnum=""4"" colname=""col4"" align=""left""/>")
        xmlContent = xmlContent.Replace("<list>", "<list list-type=""bullet"">")
        xmlContent = xmlContent.Replace("<title>Steps for Lean A3 Approach</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""0"">", "<title>Steps for Lean A3 Approach</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""1""><oasis:colspec colnum=""1"" colname=""col1"" align=""left""/>")
        xmlContent = xmlContent.Replace("<title>Brief Outline on the Phases and the Steps Involved in EBD</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""0"">", "<title>Brief Outline on the Phases and the Steps Involved in EBD</title></caption><oasis:table frame=""topbot""><oasis:tgroup cols=""1""><oasis:colspec colnum=""1"" colname=""col1"" align=""left""/>")
        xmlContent = xmlContent.Replace("</article-title>, Materials and Design, <volume>21</volume>", "</article-title>, <source>Materials and Design</source>, <volume>21</volume>")
        xmlContent = xmlContent.Replace("<year>2015</year>). Hospital planning and design innovation for Lean operation. In VK Singh and P Lillrank (eds) <chapter-title>Innovations in Healthcare Management: In Cost Effective and Sustainable Solutions (pp. 107–30).</chapter-title>", "<year>2015</year>). <chapter-title>Hospital planning and design innovation for Lean operation</chapter-title>. In VK Singh and P Lillrank (eds) <source>Innovations in Healthcare Management: In Cost Effective and Sustainable Solutions (pp. 107–30).</source>")
        xmlContent = xmlContent.Replace("<target target-type=""page"" id=""page_9"">9</target>LEAN AND EBD</title>", "<target target-type=""page"" id=""page_9"">9</target>Lean and EBD</title>")
        xmlContent = xmlContent.Replace("xlink:href=""ITX000x002.jpgimage/jpeg.tif""", "xlink:href = ""ITX000x002.jpg""")
        xmlContent = xmlContent.Replace("<title>The Concept of a Hospital</title>", "<title><target target-type=""page"" id=""page_11"">11</target>The Concept of a Hospital</title>")
        xmlContent = xmlContent.Replace("<title>Innovative, Lean, and Evidence-Based Design</title>", "<title><target target-type=""page"" id=""page_5"">5</target>Innovative, Lean, and Evidence-Based Design</title>")
        xmlContent = xmlContent.Replace("<title>Patient First, Functions Next, and Design Later</title>", "<title><target target-type=""page"" id=""page_32"">32</target>Patient First, Functions Next, and Design Later</title>")
        xmlContent = xmlContent.Replace("<title>Designing a Patient-Centric Healthcare Facility Using Lean Methodology</title>", "<title><target target-type=""page"" id=""page_67"">67</target>Designing a Patient-Centric Healthcare Facility Using Lean Methodology</title>")
        xmlContent = xmlContent.Replace("<title>Creating Safer Healthcare Environments Using an Evidence-Based Design Process</title>", "<title><target target-type=""page"" id=""page_83"">83</target>Creating Safer Healthcare Environments Using an Evidence-Based Design Process</title>")
        xmlContent = xmlContent.Replace("<title>Evidence-Based Design in Hospitals: Theory to Implementation</title>", "<title><target target-type=""page"" id=""page_95"">95</target>Evidence-Based Design in Hospitals: Theory to Implementation</title>")
        xmlContent = xmlContent.Replace("<title><italic>Virtual Hospitals of the Future</italic></title>", "<title><target target-type=""page"" id=""page_131"">131</target><italic>Virtual Hospitals of the Future</italic></title>")
        xmlContent = xmlContent.Replace("<title><italic>Redefining Healthcare of Tomorrow in Smart City</italic></title>", "<title><target target-type=""page"" id=""page_141"">141</target><italic>Redefining Healthcare of Tomorrow in Smart City</italic></title>")
        xmlContent = xmlContent.Replace("<title>Delivering Inclusive Intelligent Healthcare by Innovative and Comprehensive e-Health System</title>", "<title><target target-type=""page"" id=""page_157"">157</target>Delivering Inclusive Intelligent Healthcare by Innovative and Comprehensive e-Health System</title>")
        xmlContent = xmlContent.Replace("<title>Planning Safe Hospitals</title>", "<title><target target-type=""page"" id=""page_173"">173</target>Planning Safe Hospitals</title>")
        xmlContent = xmlContent.Replace("<title>Designing Innovative Facilities: Contamination and Security Hazards at Hospitals</title>", "<title><target target-type=""page"" id=""page_193"">193</target>Designing Innovative Facilities: Contamination and Security Hazards at Hospitals</title>")
        xmlContent = xmlContent.Replace("<title>Adapt or Obsolesce: The Evolution of Singapore Health System</title>", "<title><target target-type=""page"" id=""page_215"">215</target>Adapt or Obsolesce: The Evolution of Singapore Health System</title>")
        xmlContent = xmlContent.Replace("<title-group><title>Foreword</title></title-group>", "<title-group><title>Foreword</title></title-group><contrib-group content-type=""author""><contrib contrib-type=""author""><name><surname>Singh</surname><given-names>Vijai Kumar</given-names></name></contrib></contrib-group>")
        xmlContent = xmlContent.Replace("<named-book-part-body><p><bold>Editors</bold></p>", "<book-part-meta><title-group><title><target target-type=""page"" id=""page_xxvii"">xxvii</target>Editors</title></title-group><contrib-group content-type=""author""><contrib contrib-type=""author""><name><surname>Singh</surname><given-names>Vijai Kumar</given-names></name></contrib></contrib-group><fpage>xxvii</fpage><lpage>xxix</lpage></book-part-meta><named-book-part-body>")
        xmlContent = xmlContent.Replace("<p><bold>Dr. S.K. Biswas</bold>, after a successful", "<p><named-content content-type=""bio-name"">Swapan Kumar Biswas, MPhil</named-content>, after a successful")
        xmlContent = xmlContent.Replace("<front-matter-part book-part-type=""chapter""><named-book-part-body><p><bold>Contributors</bold></p>", "<bio content-type=""author"" id=""bio1"">")
        xmlContent = xmlContent.Replace("MPhil in hospital and health system management.</p>", "MPhil in hospital and health system management.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Dr. Rajeev Boudhankar</bold> has 29 years", "<bio content-type=""author"" id=""bio2""><p><named-content content-type=""bio-name"">Rajeev Boudhankar, MD</named-content>, has 29 years")
        xmlContent = xmlContent.Replace("stage (drawing board) to operationalization.</p>", "stage (drawing board) to operationalization.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Michael Chamberlain</bold> is president of Simpler North", "<bio content-type=""author"" id=""bio3""><p><named-content content-type=""bio-name"">Michael Chamberlain</named-content> is president of Simpler North")
        xmlContent = xmlContent.Replace("innovation, convergence, and population health.</p>", "innovation, convergence, and population health.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Kim Chaney</bold> has over 20 years’ experience in delivering", "<bio content-type=""author"" id=""bio4""><p><named-content content-type=""bio-name"">Kim Chaney</named-content> has over 20 years’ experience in delivering")
        xmlContent = xmlContent.Replace("and patient flows in private practice environments.</p>", "and patient flows in private practice environments.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Raman Chawla</bold> has a PhD in toxicology", "<bio content-type=""author"" id=""bio5""><p><named-content content-type=""bio-name"">Raman Chawla, PhD</named-content>, earned a PhD in toxicology")
        xmlContent = xmlContent.Replace("publications in journals of international repute.</p>", "publications in journals of international repute.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Lai Chien-Wen</bold>, administrative vice-superintendent of", "<bio content-type=""author"" id=""bio6""><p><named-content content-type=""bio-name"">Lai Chien-Wen, PhD</named-content>, administrative vice-superintendent of")
        xmlContent = xmlContent.Replace("intelligent hospital Yuanlin Christian Hospital.</p>", "intelligent hospital Yuanlin Christian Hospital.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>John Gallagher</bold> has more", "<bio content-type=""author"" id=""bio7""><p><named-content content-type=""bio-name"">John Gallagher</named-content> has more")
        xmlContent = xmlContent.Replace("for the innovation practice at Simpler.</p>", "for the innovation practice at Simpler.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Sachin Gaur</bold> is a researcher", "<bio content-type=""author"" id=""bio8""><p><named-content content-type=""bio-name"">Sachin Gaur</named-content> is a researcher")
        xmlContent = xmlContent.Replace("book chapters on topics of innovation.</p>", "book chapters on topics of innovation.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Sushma Guleria</bold> has a PhD in disaster management", "<bio content-type=""author"" id=""bio9""><p><named-content content-type=""bio-name"">Sushma Guleria, PhD</named-content>, earned a PhD in disaster management")
        xmlContent = xmlContent.Replace("She is a trainer in disaster management.</p>", "She is a trainer in disaster management.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Anjali Joseph</bold> is Spartanburg Regional Healthcare", "<bio content-type=""author"" id=""bio10""><p><named-content content-type=""bio-name"">Anjali Joseph, PhD</named-content>, is Spartanburg Regional Healthcare")
        xmlContent = xmlContent.Replace("Technology and an M Arch from Kansas State University.</p>", "Technology and an M Arch from Kansas State University.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Ron Kwon</bold>, MD, FACP, has been site", "<bio content-type=""author"" id=""bio11""><p><named-content content-type=""bio-name"">Ron Kwon, MD, FACP</named-content>, has been site")
        xmlContent = xmlContent.Replace("reduced costs, and improved patient and staff satisfaction.</p>", "reduced costs, and improved patient and staff satisfaction.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Xiaobo Quan</bold> is professor of practice at", "<bio content-type=""author"" id=""bio13""><p><named-content content-type=""bio-name"">Xiaobo Quan, PhD</named-content>, is professor of practice at")
        xmlContent = xmlContent.Replace("in architecture from Southeast University in China.</p>", "in architecture from Southeast University in China.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Riikka-Leena Leskelä</bold> is senior manager at", "<bio content-type=""author"" id=""bio12""><p><named-content content-type=""bio-name"">Riikka-Leena Leskelä, PhD</named-content>, is senior manager at")
        xmlContent = xmlContent.Replace("healthcare system planning, and hospital design.</p>", "healthcare system planning, and hospital design.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Matthew Saunders</bold> is an architect registered", "<bio content-type=""author"" id=""bio14""><p><named-content content-type=""bio-name"">Matthew Saunders</named-content> is an architect registered")
        xmlContent = xmlContent.Replace("economics, cultures, and policies driving healthcare.</p>", "economics, cultures, and policies driving healthcare.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>B.R. Shetty</bold> After arriving in", "<bio content-type=""author"" id=""bio15""><p><named-content content-type=""bio-name"">Bavaguthu Raghuram Shetty</named-content> After arriving in")
        xmlContent = xmlContent.Replace("and Mumbai, in India and at Abu Dhabi in the UAE.</p>", "and Mumbai, in India and at Abu Dhabi in the UAE.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Dr. Kuo Shou-Jen</bold>, Ministry of Education–certified", "<bio content-type=""author"" id=""bio16""><p><named-content content-type=""bio-name"">Kuo Shou-Jen</named-content>, Ministry of Education–certified")
        xmlContent = xmlContent.Replace("receiving a total of 17 certifications from CCPC.</p>", "receiving a total of 17 certifications from CCPC.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Nimisha Singh</bold> is an MBA in hospital and health management", "<bio content-type=""author"" id=""bio17""><p><named-content content-type=""bio-name"">Nimisha Singh</named-content> earned an MBA in hospital and health management")
        xmlContent = xmlContent.Replace("Bronze in Lean healthcare, by Cardiff University.</p>", "Bronze in Lean healthcare, by Cardiff University.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Ellen Taylor</bold> is vice-president for research", "<bio content-type=""author"" id=""bio18""><p><named-content content-type=""bio-name"">Ellen Taylor, PhD</named-content>, is vice president for research")
        xmlContent = xmlContent.Replace("safety from Loughborough University in England.</p>", "safety from Loughborough University in England.</p></bio>")
        xmlContent = xmlContent.Replace("<p><bold>Olli Tolkki</bold> is director of sales at", "<bio content-type=""author"" id=""bio19""><p><named-content content-type=""bio-name"">Olli Tolkki</named-content> is director of sales at")
        xmlContent = xmlContent.Replace("elderly care improvement, and hospital design.</p></named-book-part-body></front-matter-part>", "elderly care improvement, and hospital design.</p></bio>")
        xmlContent = xmlContent.Replace("<ref id=""ch7-CIT00014""><mixed-citation publication-type=""book""><label>14</label><person-group person-Group-Type=""author"">", "<ref id=""ch7-CIT00014""><mixed-citation publication-type=""other""><label>14</label><person-group person-Group-Type=""author"">")
        xmlContent = xmlContent.Replace("<ref id=""ch7-CIT00025""><mixed-citation publication-type=""book""><label>25</label><person-group person-Group-Type=""author"">", "<ref id=""ch7-CIT00025""><mixed-citation publication-type=""other""><label>25</label><person-group person-Group-Type=""author"">")
        xmlContent = xmlContent.Replace("<ref id=""ch12-CIT00006""><mixed-citation publication-type=""book"">", "<ref id=""ch12-CIT00006""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch12-CIT00012""><mixed-citation publication-type=""book"">", "<ref id=""ch12-CIT00012""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch12-CIT00013""><mixed-citation publication-type=""book"">", "<ref id=""ch12-CIT00013""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch13-CIT00006""><mixed-citation publication-type=""book"">", "<ref id=""ch13-CIT00006""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch13-CIT00008""><mixed-citation publication-type=""book"">", "<ref id=""ch13-CIT00008""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch14-CIT00024""><mixed-citation publication-type=""book"">", "<ref id=""ch14-CIT00024""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch7-CIT00014""><mixed-citation publication-type=""book"">", "<ref id=""ch7-CIT00014""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<ref id=""ch12-CIT00006""><mixed-citation publication-type=""other"">", "<ref id=""ch12-CIT00006""><mixed-citation publication-type=""book"">")
        xmlContent = xmlContent.Replace("<ref id=""ch7-CIT00025""><mixed-citation publication-type=""book"">", "<ref id=""ch7-CIT00025""><mixed-citation publication-type=""other"">")
        xmlContent = xmlContent.Replace("<chapter-title>The Impact of Design on Infections in Healthcare Facilities</chapter-title>", "<italic>The Impact of Design on Infections in Healthcare Facilities</italic>")
        xmlContent = xmlContent.Replace("<chapter-title>Workshops on Hospital Preparedness for Disasters in India: A Geo-Hazards Society (GHS).</chapter-title> World Health Organization (WHO) India Initiative", "<source>Workshops on Hospital Preparedness for Disasters in India: A Geo-Hazards Society (GHS)</source>. <chapter-title>World Health Organization (WHO) India Initiative</chapter-title>")
        xmlContent = xmlContent.Replace("<sec sec-type=""imprint-cip-data""><p><bold>Library of Congress Cataloging‑in‑Publication Data</bold></p></sec>", "")

        xmlContent = DuplicateTargetCleanup(xmlContent)
        File.WriteAllText(tfOutXML, xmlContent)

        RenameFigureNames(tfOutXML)

        Return True
    End Function


    Private Function RenameFigureNames(ByVal ContentXMLFile As String) As Boolean
        Dim NLMContents As String = File.ReadAllText(ContentXMLFile)
        Dim FigurePath As String = String.Empty
        Dim NLMFigFileNames As New List(Of String)
        Dim FolderFigFileNames As New List(Of String)

        For Each FigMatch As Match In Regex.Matches(NLMContents, "\<graphic xlink:href\=(.*?)\ ", RegexOptions.Multiline)
            NLMFigFileNames.Add(FigMatch.Groups(1).Value.Replace("""", "").Replace("images/", "").Trim())
        Next

        FigurePath = Path.Combine(Path.GetDirectoryName(ContentXMLFile), "artwork")

        If ((NLMFigFileNames Is Nothing) OrElse (NLMFigFileNames.Count = 0)) Then
            Return True
        End If
        If (Directory.Exists(FigurePath)) Then
            Array.ForEach(Directory.GetFiles(FigurePath, "*.*", SearchOption.TopDirectoryOnly), Sub(fig As String)
                                                                                                    FolderFigFileNames.Add(Path.GetFileName(fig))
                                                                                                End Sub)
            Dim FoundFig As String = String.Empty
            Dim TempFig As String = String.Empty
            Dim ChangeFigList As New Dictionary(Of String, String)
            If ((FolderFigFileNames IsNot Nothing) AndAlso (FolderFigFileNames.Count > 0)) Then
                For Each dirFig As String In FolderFigFileNames
                    FoundFig = (From nfig In NLMFigFileNames Where (Not nfig.Contains("\") AndAlso (String.Compare(Path.GetFileNameWithoutExtension(nfig), Path.GetFileNameWithoutExtension(dirFig), True) = 0)) Select nfig).FirstOrDefault
                    TempFig = FoundFig
                    If (Not String.IsNullOrEmpty(FoundFig)) Then
                        FoundFig = (From nfig In NLMFigFileNames Where (String.Compare(nfig, dirFig, True) = 0) Select nfig).FirstOrDefault
                        If (String.IsNullOrEmpty(FoundFig)) Then
                            ChangeFigList.Add(TempFig, dirFig)
                        End If
                    End If
                Next

                For Each nlmfig As String In NLMFigFileNames
                    FoundFig = (From dirfig In FolderFigFileNames Where (String.Compare(Path.GetFileNameWithoutExtension(dirfig), Path.GetFileNameWithoutExtension(nlmfig), True) = 0) Select dirfig).FirstOrDefault
                    If (String.IsNullOrEmpty(FoundFig)) Then
                        GBL.DeantaBallon("Could not able to find the figure : " & nlmfig, MessageType.MSGERROR)
                    End If
                Next

                If ((ChangeFigList IsNot Nothing) AndAlso (ChangeFigList.Count > 0)) Then
                    For Each FigKey As KeyValuePair(Of String, String) In ChangeFigList
                        NLMContents = NLMContents.Replace(FigKey.Key, FigKey.Value)
                    Next
                End If

            Else
                GBL.DeantaBallon("Could not able to find the images in image folder.", MessageType.MSGERROR)
            End If
        Else
            GBL.DeantaBallon("Could not able to find the images folder.", MessageType.MSGERROR)
        End If

        File.WriteAllText(ContentXMLFile, NLMContents)
        Return True
    End Function

    Private Function DuplicateTargetCleanup(ByVal xmlString As String) As String
        Dim xmlClean As New XmlDocument
        xmlClean.PreserveWhitespace = True
        Try
            xmlClean.LoadXml(xmlString.Replace("&", "&amp;"))
        Catch ex As Exception
            Return xmlString
        End Try

        Dim targetList As XmlNodeList = xmlClean.SelectNodes("//target")
        For t As Integer = 0 To targetList.Count - 1
            If ((targetList(t).NextSibling IsNot Nothing) AndAlso (String.Compare(targetList(t).NextSibling.Name, "target", True) = 0)) Then
                targetList(t).NextSibling.ParentNode.RemoveChild(targetList(t).NextSibling)
            End If
        Next
        xmlString = xmlClean.OuterXml.Replace("&amp;", "&")
        Return xmlString
    End Function

    Private Function RemoveBoldTag(ByVal Mt As Match) As String
        Dim Result As String = Mt.Value
        If (Result.Contains("<title")) Then
            Result = Result.Replace("<bold>", "").Replace("</bold>", "")
            If ((Mt.Groups IsNot Nothing) AndAlso (Mt.Groups.Count > 0)) Then
                Dim TempTitle As String = Mt.Groups(2).Value
                TempTitle = Regex.Replace(TempTitle, "(<target[^>]*>(((?!<(\/)?target>).)*)<\/target>)", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                TempTitle = Regex.Replace(TempTitle, "(<sup[^>]*>(((?!<(\/)?sup>).)*)<\/sup>)", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                If (String.Compare(TempTitle, TempTitle.ToUpper(), False) = 0) Then
                    Dim culinfo As CultureInfo = Thread.CurrentThread.CurrentCulture
                    Dim txtinfo As TextInfo = culinfo.TextInfo
                    Result = Result.Replace(TempTitle, txtinfo.ToTitleCase(TempTitle.ToLower()))
                    Result = Result.Replace(" And ", " and ").Replace(" Of ", " of ").Replace(" An ", " an ")
                End If
            End If
        Else
            Result = Result.Replace("<bold>", "").Replace("</bold>", "")
        End If
        Return Result
    End Function

    Private Function GetNextTaskIDForUpload(ByVal index As Integer) As Boolean
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim tblTask As DataTable = MySqlHelper.ReadSqlData("select task_id,task_name,milestone_id,order_id from tb_tasks where task_id=" & DbData.TaskID & "")
        If ((tblTask Is Nothing) OrElse (tblTask.Rows Is Nothing) OrElse (tblTask.Rows.Count = 0)) Then
            Return False
        End If
        GBL.DBDataList(index).TaskName = Convert.ToString(tblTask.Rows(0).Item("task_name"))
        GBL.DBDataList(index).MilestoneID = Convert.ToInt64(tblTask.Rows(0).Item("milestone_id"))
        Dim SequnID As Integer = Convert.ToInt32(tblTask.Rows(0).Item("order_id"))
        SequnID = SequnID + 1
        GBL.DBDataList(index).UploadTaskID = MySqlHelper.ExecuteScalar("select task_id from tb_tasks where order_id = " & SequnID & " and milestone_id=" & DbData.MilestoneID & "")
        GBL.DeantaBallon("Upload task iD " & GBL.DBDataList(index).UploadTaskID, MessageType.MSGINFO)
        GBL.DBDataList(index).MilestoneName = MySqlHelper.ExecuteScalar("select milestone_title from tb_milestones where milestone_id=" & DbData.MilestoneID & "")
        Return True
    End Function

    Private Function CreateTaskList(ByVal index As Integer) As Boolean
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        GBL.DBDataList(index).TaskList.Clear()
        'If ((DbData.Stage = DBStage.XMLTRIGGER) Or (DbData.Stage = DBStage.FRISTPROFFXMLTRIGGER)) Then
        '    If ((DbData.DocType = DocumentType.TANDF) Or (DbData.DocType = DocumentType.TANDFUK)) Then
        '        GBL.DBDataList(index).TaskList.Add("6")
        '        GBL.DBDataList(index).TaskList.Add("7")
        '        GBL.DBDataList(index).TaskList.Add("8")
        '    ElseIf (DbData.DocType = DocumentType.BB) Then
        '        GBL.DBDataList(index).TaskList.Add("4")
        '        GBL.DBDataList(index).TaskList.Add("5")
        '        GBL.DBDataList(index).TaskList.Add("6")
        '        GBL.DBDataList(index).TaskList.Add("7")
        '        GBL.DBDataList(index).TaskList.Add("8")
        '    End If
        'Else
        If (DbData.IsBookPDFGenerated) Then
            GBL.DBDataList(index).TaskList.Add("2")
        End If
        If (DbData.IsXMLGenerated) Then
            GBL.DBDataList(index).TaskList.Add("3")
        End If
        If (DbData.IsPODGenerated) Then
            GBL.DBDataList(index).TaskList.Add("4")
        End If
        If (DbData.IsRTFGenerated) Then
            GBL.DBDataList(index).TaskList.Add("5")
        End If
        If (DbData.IsPackageGenerated) Then
            GBL.DBDataList(index).TaskList.Add("7")
        End If
        If (DbData.IsCoverGenerated) Then
            GBL.DBDataList(index).TaskList.Add("6")
        End If
        If (DbData.IsWEBPDFGeneratd) Then
            GBL.DBDataList(index).TaskList.Add("8")
            If (Not DbData.IsCoverGenerated) Then
                GBL.DBDataList(index).TaskList.Add("6")
            End If
        End If
        If (DbData.IsEpubGenerated) Then
            GBL.DBDataList(index).TaskList.Add("9")
        End If
        If (DbData.IsMOBIGenerated) Then
            GBL.DBDataList(index).TaskList.Add("10")
        End If
        'End If
        Return True
    End Function

    Private Function UpdateClientandDocumentType(Index As Integer) As Boolean
        Dim IsSuccess As Boolean = True
        Dim TmpLanData As DigitalBundleData = GBL.DBDataList(Index)
        Dim JournalAbbre As String = String.Empty

        Dim ClientID As Integer = 0
        Dim doctype As String = String.Empty
        If (TmpLanData Is Nothing) Then Return False
        Try
            JournalAbbre = MySqlHelper.ExecuteScalar("select abbreviation from tb_projects where project_id=" & GBL.DBDataList(Index).ProjectID)
            If ((JournalAbbre.Contains("_")) AndAlso ((JournalAbbre.Length - JournalAbbre.Replace("_", "").Length) = 1)) Then
                JournalAbbre = JournalAbbre.Split("_")(0)
            End If
        Catch ex As Exception
        End Try
        Try
            GBL.DBDataList(Index).TemplateFullName = MySqlHelper.ExecuteScalar("select template_name from tb_projects where project_id=" & GBL.DBDataList(Index).ProjectID)
        Catch ex As Exception
            Return String.Empty
        End Try
        Try
            GBL.DBDataList(Index).UserName = MySqlHelper.ExecuteScalar($"select concat(user_name,' ',user_lastname) from tb_users where user_id={GBL.DBDataList(Index).UserID}")
        Catch ex As Exception
            Return String.Empty
        End Try
        doctype = MySqlHelper.ExecuteScalar("select projectType from tb_projects where project_id=" & TmpLanData.ProjectID)
        If (ClientID = 19) Then
            doctype = "book"
        End If
        If (String.Compare(doctype, "book", True) = 0) Then
            Dim CompanyID As String = MySqlHelper.ExecuteScalar("select company_id from tb_projects where project_id=" & TmpLanData.ProjectID)
            If ((CompanyID = "73") Or (CompanyID = "75") Or (CompanyID = "80") Or (CompanyID = "85")) Then
                GBL.DBDataList(Index).DocType = DocumentType.BLOOMSBURY
            ElseIf ((CompanyID = "74") Or (CompanyID = "86") Or (CompanyID = "82")) Then
                GBL.DBDataList(Index).DocType = DocumentType.TANDF
            ElseIf (CompanyID = "13") Then
                GBL.DBDataList(Index).DocType = DocumentType.RL
            ElseIf (CompanyID = "81") Then
                GBL.DBDataList(Index).DocType = DocumentType.RL
            Else
                GBL.DBDataList(Index).DocType = DirectCast([Enum].Parse(GetType(DocumentType), CompanyID), Integer)
            End If

            GBL.DBDataList(Index).OrgDocType = DirectCast([Enum].Parse(GetType(DocumentType), CompanyID), Integer)

            If (GBL.DBDataList(Index).DocType = DocumentType.BLOOMSBURY) And (Not String.IsNullOrEmpty(GBL.DBDataList(Index).TemplateFullName)) Then
                If (Path.GetFileName(GBL.DBDataList(Index).TemplateFullName).StartsWith("TRD_")) Then
                    GBL.DBDataList(Index).OrgDocType = DocumentType.TRD
                End If
            End If

            'If (String.Compare(JournalAbbre, "tandfuk", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.TANDFUK
            'ElseIf (String.Compare(JournalAbbre, "tandf", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.TANDF
            'ElseIf (String.Compare(JournalAbbre, "manning", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.TANDF
            'ElseIf (String.Compare(JournalAbbre, "BDS", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.BDS
            'ElseIf (String.Compare(JournalAbbre, "BB", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.BB
            'ElseIf (String.Compare(JournalAbbre, "ANS", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.ANS
            'ElseIf (String.Compare(JournalAbbre, "CM", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.CM
            'ElseIf (String.Compare(JournalAbbre, "RL", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.RANDL
            'ElseIf (String.Compare(JournalAbbre, "MP", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.MUP
            'ElseIf (String.Compare(JournalAbbre, "ISTE", True) = 0) Then
            '    GBL.DBDataList(Index).DocType = DocumentType.ISTE
            'Else
            '    GBL.DBDataList(Index).DocType = DocumentType.BOOK
            'End If
        Else
            GBL.DBDataList(Index).DocType = DocumentType.JOURNAL
        End If
        Try
            FindInddTemplatePath(Index, GBL.DBDataList(Index).DocType)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            SetISBNNum(Index)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            GetNextTaskIDForUpload(Index)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Return IsSuccess
    End Function

    Private Function DownloadPrintandCopyRightPDforRL(ByVal index As Integer, ByVal PrintPDF As String) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DtAsset As New DataTable("DT")
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        Dim DBAssetName As String = String.Empty
        Dim DBAssetZipPath As String = String.Empty
        DBdata = GBL.DBDataList(index)
        Try
            DtAsset = MySqlHelper.ReadSqlData($"select * from tb_documents where project_id={DBdata.ProjectID} and chapter_id={DBdata.ChapterID} and document_name like '{PrintPDF}' order by document_id desc limit 1")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        If ((DtAsset Is Nothing) OrElse (DtAsset.Rows Is Nothing) OrElse (DtAsset.Rows.Count = 0)) Then
            GBL.DeantaBallon($"No {PrintPDF} found for RL", MessageType.MSGERROR)
            Return False
        End If
        DBAssetZipPath = Path.Combine(GBL.FTPResourcePath, Convert.ToString(DtAsset.Rows(0).Item("document_path")).Replace("resources/", "")).Replace("\", "/")
        DBAssetName = Convert.ToString(DtAsset.Rows(0).Item("document_name"))
        Try
            DBdata = GBL.DBDataList(index)
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.DeliveryPreview, .UrlPath = DBAssetZipPath, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(DBAssetZipPath), DBdata.Folder.DeliveryPreview, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon("DownloadMainXMLfun - " & GBL.DBDataList.Count & ex.Message & ex.StackTrace.ToString(), MessageType.MSGERROR)
            Return False
        End Try
        Dim LocalZipPath As String = Path.Combine(DBdata.Folder.DeliveryPreview, Path.GetFileName(DBAssetZipPath))
        If (Not File.Exists(LocalZipPath)) Then
            GBL.DeantaBallon("Could not able to find the zip.", MessageType.MSGERROR)
            Return False
        End If
        Dim LocalZipAsset As String = Path.Combine(Path.GetDirectoryName(LocalZipPath), DBAssetName)
        If (File.Exists(LocalZipAsset)) Then
            File.Delete(LocalZipAsset)
        End If
        Try
            File.Move(LocalZipPath, LocalZipAsset)
        Catch ex As Exception
            GBL.DeantaBallon("Could not able to find the zip.", MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function DownloadAssetZip(ByVal index As Integer) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DtAsset As New DataTable("DT")
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        Dim DBAssetName As String = String.Empty
        Dim DBAssetZipPath As String = String.Empty
        DBdata = GBL.DBDataList(index)
        Try
            DtAsset = MySqlHelper.ReadSqlData($"select * from tb_documents where document_id={DBdata.DocumentID}")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If ((DtAsset Is Nothing) OrElse (DtAsset.Rows Is Nothing) OrElse (DtAsset.Rows.Count = 0)) Then
            GBL.DeantaBallon($"No document_id found.", MessageType.MSGERROR)
            Return False
        End If
        DBAssetZipPath = Path.Combine(GBL.FTPResourcePath, Convert.ToString(DtAsset.Rows(0).Item("document_path")).Replace("resources/", "")).Replace("\", "/")
        DBAssetName = Convert.ToString(DtAsset.Rows(0).Item("document_name"))
        Try
            DBdata = GBL.DBDataList(index)
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.PackagePath, .UrlPath = DBAssetZipPath, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(DBAssetZipPath), DBdata.Folder.PackagePath, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon("DownloadMainXMLfun - " & GBL.DBDataList.Count & ex.Message & ex.StackTrace.ToString(), MessageType.MSGERROR)
            Return False
        End Try
        Dim LocalZipPath As String = Path.Combine(DBdata.Folder.PackagePath, Path.GetFileName(DBAssetZipPath))
        If (Not File.Exists(LocalZipPath)) Then
            GBL.DeantaBallon("Could not able to find the zip.", MessageType.MSGERROR)
            Return False
        End If
        Dim LocalZipAsset As String = Path.Combine(Path.GetDirectoryName(LocalZipPath), DBAssetName)
        If (File.Exists(LocalZipAsset)) Then
            File.Delete(LocalZipAsset)
        End If
        Try
            File.Move(LocalZipPath, LocalZipAsset)
        Catch ex As Exception
            GBL.DeantaBallon("Could not able to find the zip.", MessageType.MSGERROR)
            Return False
        End Try
        Try
            ExtractZipFile(Path.Combine(Path.GetDirectoryName(LocalZipPath), DBAssetName), DBdata.Folder.ApplicationPath)
        Catch ex As Exception
            GBL.DeantaBallon($"{ex.Message} - Could not able to extract the zip.", MessageType.MSGERROR)
            Return False
        End Try

        Try
            Directory.Move(Path.Combine(DBdata.Folder.ApplicationPath, "links"), Path.Combine(DBdata.Folder.ApplicationPath, "images"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            ExtractZipFile(Path.Combine(Path.GetDirectoryName(LocalZipPath), DBAssetName), DBdata.Folder.WEBPDFPath)
        Catch ex As Exception
            GBL.DeantaBallon("Could not able to extract the zip.", MessageType.MSGERROR)
            Return False
        End Try

        File.Delete(LocalZipAsset)

        If (DBdata.DocType = DocumentType.BLOOMSBURY) Then
            GBL.DeantaBallon("Download web images started", MessageType.MSGERROR)

            Try
                If (Not DoDownloadWebImageFromBloomsbury(index)) Then
                    GBL.DeantaBallon("Could not able to find the WEB images zip.", MessageType.MSGERROR)
                    Return False
                End If
            Catch ex As Exception
                GBL.DeantaBallon($"{ex.Message} - Error occurred while downloading the bloomsbury WEB images zip.", MessageType.MSGERROR)
                Return False
            End Try
            Try
                MoveBloomsburyLogoForWebPDF(index)
            Catch ex As Exception
                GBL.DeantaBallon("Error occurred while skip the bloomsbury logo WEB images." & ex.Message, MessageType.MSGERROR)
                Return False
            End Try
            GBL.DeantaBallon("Download web images completed.", MessageType.MSGERROR)
        ElseIf ((DBdata.DocType = DocumentType.RL) Or (DBdata.DocType = DocumentType.SEQUOIA)) Then
            GBL.DeantaBallon("Download web images started", MessageType.MSGERROR)
            Try
                If (Not DoDownloadWebImageFromRandL(index)) Then
                    GBL.DeantaBallon("Could not able to find WEB images in Lanstad.", MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon("Error occurred while downloading the RL WEB images", MessageType.MSGERROR)
            End Try
            GBL.DeantaBallon("Download web images completed.", MessageType.MSGERROR)
        Else
            Try
                Directory.Move(Path.Combine(DBdata.Folder.WEBPDFPath, "links"), Path.Combine(DBdata.Folder.WEBPDFPath, "images"))
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If
        Return True
    End Function

    Private Function MoveBloomsburyLogoForWebPDF(ByVal index As Integer) As Boolean
        Dim DBdata As DigitalBundleData = Nothing
        Dim SkipPrintImageList As New List(Of String)
        SkipPrintImageList.AddRange(New String() {"i.b.tauris", "bloomsb", "Adlard Coles L-O-NY-ND-S_241017", "FSC Logo_C020471", "Arden L-NY-O-ND-S_txt_flush", "Bloomsbury Academic L-NY-O-ND-S_flush", "Bloomsbury Academic NY-L-O-ND-S_flush", "Bloomsbury Business L-NY-O-ND-S_flush", "Bloomsbury Information L-NY-O-ND-S_flush", "Bloomsbury Visual Arts L-NY-O-ND-S_flush", "Fairchild NY-L-O-ND-S_flush", "IBTauris L-NY-O-ND-S_txt_blk", "Methuen L-NY-O-ND-S_txt_blk", "SOAS_LMEI_CMYK_50mm_black aw", "TandT Clark L-NY-O-ND-S_txt_blk", "TandT Clark NY-L-O-ND-S_txt_blk", "ZED logo", "TandT_Clark_L-NY-O-ND-S_txt_blk", "Bloomsbury_Academic_L-NY-O-ND-S_flush", "UEP Black text only", "UWIPressLogoB&W1_vector", "Bloomsbury", "Half title page", "Title page", "Anthem_Press_Logo"})
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error SkipBloomsburyLogoForWebPDF - : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        DBdata = GBL.DBDataList(index)
        Dim WEBImgPath As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images_web")
        Dim PrintImgPath As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images")
        If Directory.Exists(WEBImgPath) Then
            Try
                If ((DBdata.DocType = DocumentType.BLOOMSBURY) Or (DBdata.DocType = DocumentType.TRD)) Then
                    For Each strFile As String In Directory.GetFiles(PrintImgPath, "*.*", SearchOption.TopDirectoryOnly)
                        If ((From n In SkipPrintImageList Where Path.GetFileName(strFile).ToLower().Contains(n.ToLower()) Select n).Any) Then
                            File.Copy(strFile, Path.Combine(WEBImgPath, Path.GetFileName(strFile)), True)
                        End If
                    Next
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If
        Return True
    End Function

    Private Function DoDownloadWebImageFromRandL(ByVal index As Integer) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        DBdata = GBL.DBDataList(index)

        If (Not Directory.Exists(Path.Combine(DBdata.Folder.WEBPDFPath, "links"))) Then
            GBL.DeantaBallon("No links folder found.", MessageType.MSGERROR)
            Return False
        End If

        Try
            Directory.Move(Path.Combine(DBdata.Folder.WEBPDFPath, "links"), Path.Combine(DBdata.Folder.WEBPDFPath, "images"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim WebImgFolder As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images_web")
        If (Directory.Exists(WebImgFolder)) Then
            Dim WbFt As New DirectoryInfo(WebImgFolder)
            WbFt.Delete(True)
        End If
        Directory.CreateDirectory(WebImgFolder)

        Dim RlWebImages As DataTable = MySqlHelper.ReadSqlData($"select distinct(document_name) from tb_documents where project_id={DBdata.ProjectID} and (document_name like 'fig%.%' or document_name like 'inline%.%') and document_type = '.jpg' order by document_id desc")
        If ((RlWebImages IsNot Nothing) AndAlso (RlWebImages.Rows IsNot Nothing) AndAlso (RlWebImages.Rows.Count > 0)) Then
            For c As Int16 = 0 To RlWebImages.Rows.Count - 1
                Try
                    Dim RLWebImg As DataTable = MySqlHelper.ReadSqlData($"select * from tb_documents where project_id={DBdata.ProjectID} and document_name in ('{RlWebImages.Rows(c).Item("document_name")}') and document_type = '.jpg' order by document_id desc")
                    If ((RLWebImg IsNot Nothing) AndAlso (RLWebImg.Rows IsNot Nothing) AndAlso (RLWebImg.Rows.Count > 0)) Then
                        Try
                            Dim DocumentName As String = Convert.ToString(RLWebImg.Rows(0).Item("document_name"))
                            Dim ResourceName As String = Convert.ToString(RLWebImg.Rows(0).Item("document_path"))
                            ResourceName = Path.Combine(GBL.FTPResourcePath, ResourceName.Replace("resources/", "")).Replace("\", "/")
                            GBL.DeantaBallon($"Download RL WEB File : {ResourceName} - {DocumentName}", MessageType.MSGINFO)
                            Try
                                'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = WebImgFolder, .UrlPath = ResourceName, .Index = index})
                                'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                                'StatusUI.DoUploadDownload()
                                AzureHelper.DownloadFile(Path.GetFileName(ResourceName), WebImgFolder, LanstadPathOptions.ASSETPATH, String.Empty)
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                Return False
                            End Try
                            If (Not File.Exists(Path.Combine(WebImgFolder, Path.GetFileName(ResourceName)))) Then
                                GBL.DeantaBallon($"Could not able to download the RL web image :{DocumentName}", MessageType.MSGERROR)
                                Return False
                            End If
                            If (File.Exists(Path.Combine(WebImgFolder, DocumentName))) Then
                                File.Delete(Path.Combine(WebImgFolder, DocumentName))
                            End If
                            File.Move(Path.Combine(WebImgFolder, Path.GetFileName(ResourceName)), Path.Combine(WebImgFolder, DocumentName))
                        Catch ex As Exception
                            GBL.DeantaBallon($"{ex.Message} - DoDownloadWebImageFromRandL", MessageType.MSGERROR)
                            Continue For
                        End Try
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If

        Dim CopyPrintImgToWebList As New List(Of String)
        CopyPrintImgToWebList.AddRange(New String() {"trade", "AmericanWays", "ASBO Logo (bitmapped)", "Barclays_logo", "ECPR_Press_Logo", "ECPR_Press_Logo", "FEPS.ai_vectorisé", "FEPS.ai_vectorisé", "Infinity", "Infinity_symbol", "logo_Bernan", "logo_Bucknell", "logo_Bucknell_dvr_lores", "Policy Network logo", "RLI_logo"})
        Dim PrintImgFolder As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images")
        If (Directory.Exists(PrintImgFolder)) Then
            Dim tradeFiles As New List(Of String)
            tradeFiles.AddRange(Directory.GetFiles(PrintImgFolder, "*.*", SearchOption.TopDirectoryOnly).Where(Function(strF As String)
                                                                                                                   If ((From n In CopyPrintImgToWebList Where Path.GetFileName(strF).ToLower().Contains(n.ToLower()) Select n).Any) Then
                                                                                                                       Return True
                                                                                                                   End If
                                                                                                                   Return False
                                                                                                               End Function))

            If ((tradeFiles IsNot Nothing) AndAlso (tradeFiles.Count > 0)) Then
                For t As Int16 = 0 To tradeFiles.Count - 1
                    File.Copy(tradeFiles(t), Path.Combine(WebImgFolder, Path.GetFileName(tradeFiles(t))), True)
                Next
            End If
        End If
        Return True
    End Function

    Private Function DoDownloadWebImageFromBloomsbury(ByVal index As Integer) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        DBdata = GBL.DBDataList(index)

        If (Not Directory.Exists(Path.Combine(DBdata.Folder.WEBPDFPath, "links"))) Then
            GBL.DeantaBallon("No links folder found.", MessageType.MSGERROR)
            Return False
        End If

        Try
            Directory.Move(Path.Combine(DBdata.Folder.WEBPDFPath, "links"), Path.Combine(DBdata.Folder.WEBPDFPath, "images"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim WebPDFFile As String = MySqlHelper.ExecuteScalar("select document_path from tb_documents where project_id=" & DBdata.ProjectID & " and document_name in ('artwork_web.zip') and document_type in ('.zip') order by document_id desc limit 1")

        If (String.IsNullOrEmpty(WebPDFFile)) Then
            GBL.DeantaBallon("artwork_web.zip file not found for this title.", MessageType.MSGERROR)
            Return True
        End If

        Dim WebImgFolder As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images_web")
        If (Directory.Exists(WebImgFolder)) Then
            Dim WbFt As New DirectoryInfo(WebImgFolder)
            WbFt.Delete(True)
        End If
        Directory.CreateDirectory(WebImgFolder)

        WebPDFFile = Path.Combine(GBL.FTPResourcePath.Replace("resources", ""), WebPDFFile).Replace("\", "/")
        Try
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.PackagePath, .UrlPath = WebPDFFile, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(WebPDFFile), DBdata.Folder.PackagePath, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        WebPDFFile = Path.Combine(DBdata.Folder.PackagePath, Path.GetFileName(WebPDFFile))
        If (Not File.Exists(WebPDFFile)) Then
            GBL.DeantaBallon("Could not able to find the WEB images zip in package path.", MessageType.MSGERROR)
            Return False
        End If

        Try
            ExtractZipFile(WebPDFFile, WebImgFolder)
        Catch ex As Exception
            GBL.DeantaBallon("Could not able to extract the zip.", MessageType.MSGERROR)
            Return False
        End Try

        If (Directory.GetDirectories(WebImgFolder).Length > 0) Then
            For Each imgFolder As String In Directory.GetDirectories(WebImgFolder)
                For Each imgFile As String In Directory.GetFiles(imgFolder, "*.*", SearchOption.TopDirectoryOnly)
                    File.Move(imgFile, Path.Combine(WebImgFolder, Path.GetFileName(imgFile)))
                Next
                Directory.Delete(imgFolder)
            Next
        End If

        Try
            If (File.Exists(WebPDFFile)) Then
                File.Delete(WebPDFFile)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Return True
    End Function

    Private Function old_DoDownloadWebImageFromBloomsbury(ByVal index As Integer) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        DBdata = GBL.DBDataList(index)
        Dim WebPDFFile As String = MySqlHelper.ExecuteScalar("select document_path from tb_documents where project_id=" & DBdata.ProjectID & " and document_name in ('artwork_web.zip') and document_type in ('.zip') order by document_id desc limit 1")
        WebPDFFile = Path.Combine(GBL.FTPResourcePath.Replace("resources", ""), WebPDFFile).Replace("\", "/")
        Try
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.PackagePath, .UrlPath = WebPDFFile, .Index = index})
            AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        WebPDFFile = Path.Combine(DBdata.Folder.PackagePath, Path.GetFileName(WebPDFFile))
        If (Not File.Exists(WebPDFFile)) Then
            GBL.DeantaBallon("Could not able to find the WEB images zip in package path.", MessageType.MSGERROR)
            Return False
        End If
        Dim WebImgFolder As String = Path.Combine(DBdata.Folder.WEBPDFPath, "images")
        Try
            ExtractZipFile(WebPDFFile, WebImgFolder)
        Catch ex As Exception
            GBL.DeantaBallon("Could not able to extract the zip.", MessageType.MSGERROR)
            Return False
        End Try

        If (Directory.GetDirectories(WebImgFolder).Length > 0) Then
            For Each imgFolder As String In Directory.GetDirectories(WebImgFolder)
                For Each imgFile As String In Directory.GetFiles(imgFolder, "*.*", SearchOption.TopDirectoryOnly)
                    File.Move(imgFile, Path.Combine(WebImgFolder, Path.GetFileName(imgFile)))
                Next
                Directory.Delete(imgFolder)
            Next
        End If

        Try
            If (File.Exists(WebPDFFile)) Then
                File.Delete(WebPDFFile)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function DownloadMainXML(ByVal index As Integer) As Boolean
        Dim StatusUI As UploadDownloadHelper
        Dim DBdata As DigitalBundleData = Nothing
        If ((GBL.DBDataList Is Nothing) OrElse (GBL.DBDataList.Count = 0)) Then
            GBL.DeantaBallon("Error : DBDataList - " & GBL.DBDataList.Count, MessageType.MSGERROR)
            Return False
        End If
        Try
            DBdata = GBL.DBDataList(index)
            'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = DBdata.Folder.LXEXMLPath, .UrlPath = DBdata.XmlURL, .Index = index})
            'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            'StatusUI.DoUploadDownload()
            AzureHelper.DownloadFile(Path.GetFileName(DBdata.XmlURL), DBdata.Folder.LXEXMLPath, LanstadPathOptions.ASSETPATH, String.Empty)
        Catch ex As Exception
            GBL.DeantaBallon("DownloadMainXMLfun - " & GBL.DBDataList.Count & ex.Message & ex.StackTrace.ToString(), MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function doInDesignCleanupandConversion(ByVal Index As Integer, ByVal ExportXMLPath As String, ByVal FinalXMLPath As String, ByVal ClientID As LanstadClientType) As Boolean
        Dim InddConvert As New InDesignConversionCleanup
        Dim MainXML As String = String.Empty
        Dim OutFinalXML As String = String.Empty
        Dim DbData As DigitalBundleData = GBL.DBDataList(Index)
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
        GBL.DeantaBallon("Doc Type :" & ClientID, MessageType.MSGERROR)
        For Each xmlFile As String In Directory.GetFiles(ExportXMLPath, "*.xml", SearchOption.TopDirectoryOnly)
            Try
                If (xmlFile.ToLower().Contains("main.xml")) Then
                    MainXML = xmlFile
                    Continue For
                End If
                If (xmlFile.ToLower().Contains(("pagesection.xml"))) Then
                    Continue For
                End If
                OutFinalXML = Path.Combine(FinalXMLPath, Path.GetFileName(xmlFile))
                Try
                    If (Not InddConvert.ConvertInDesignXMLtoClient(ClientID, xmlFile, OutFinalXML)) Then
                        GBL.DeantaBallon("Error occured while converting indesign cleanup " & Path.GetFileName(xmlFile), MessageType.MSGERROR)
                        Return False
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon("Error occured while converting indesign cleanup " & Path.GetFileName(MainXML), MessageType.MSGERROR)
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                Try
                    File.Copy(OutFinalXML, Path.Combine(Path.Combine(Path.GetDirectoryName(ExportXMLPath), "EPub"), Path.GetFileName(OutFinalXML)), True)
                Catch ex As Exception
                    GBL.DeantaBallon("Error occured while copying xml to epub" & Path.GetFileName(MainXML), MessageType.MSGERROR)
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        Next
        GBL.DeantaBallon("Cleanup started: " & Path.GetFileName(MainXML), MessageType.MSGINFO)
        If (String.IsNullOrEmpty(MainXML)) Then
            GBL.DeantaBallon("Cleanup started: " & Path.GetFileName(MainXML), MessageType.MSGINFO)
        End If
        GBL.DeantaBallon("Cleanup completed: " & Path.GetFileName(MainXML), MessageType.MSGINFO)
        Return True
    End Function

    Private Function UpdatePageNumber(ByVal LxeXMLPath As String, ByVal ExportXMLPath As String) As Boolean
        Dim ExportContent As String = String.Empty
        Dim LxeContent As String = String.Empty
        Dim MissingPageNum As New List(Of String)
        Dim ExportFiles As New List(Of String)
        Dim LxeFiles As New List(Of String)
        ExportFiles.AddRange(Directory.GetFiles(ExportXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        LxeFiles.AddRange(Directory.GetFiles(LxeXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        For Each ExpFile As String In ExportFiles
            MissingPageNum.Clear()
            Dim LxeFile As String = (From n In LxeFiles Where (Path.GetFileNameWithoutExtension(ExpFile).Replace("TNF", "TF").Contains(Path.GetFileNameWithoutExtension(n))) Select n).FirstOrDefault
            If (String.IsNullOrEmpty(LxeFile)) Then
                GBL.DeantaBallon("File not found." & Path.GetFileNameWithoutExtension(ExpFile), MessageType.MSGERROR)
                Continue For
            End If
            If (Not File.Exists(LxeFile)) Then
                GBL.DeantaBallon("File not found." & Path.GetFileNameWithoutExtension(ExpFile), MessageType.MSGERROR)
                Continue For
            End If
            ExportContent = File.ReadAllText(ExpFile)
            LxeContent = File.ReadAllText(LxeFile)
            ExportContent = ExportContent.Replace(ChrW(8233), "")
            ExportContent = Regex.Replace(ExportContent, "(<a id=""page_[0-9ivx]+""[\s]*/>)", "<!--$1-->", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            ExportContent = TransformText(ExportContent)
            ExportContent = ExportContent.Replace("<!--", "").Replace("-->", "")
            ExportContent = Regex.Replace(ExportContent, "(<!--)*(<!DOCTYPE([^>]+)>)(-->)*", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            ExportContent = HtmlEncode(ExportContent).Replace("&#x200A;", "")
            Dim findPattern As String = String.Empty
            For Each ExpMatch As Match In Regex.Matches(ExportContent, "(.{0,10})(<a id=""page_[0-9ivx]+""[\s]*/>)(.{0,10})", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                Try
                    If (Not ExpMatch.Success) Then Continue For
                    If ((ExpMatch.Groups IsNot Nothing) AndAlso (ExpMatch.Groups.Count > 0)) Then
                        findPattern = GeneratePattern(ExpMatch.Groups(1).Value, ExpMatch.Groups(2).Value, ExpMatch.Groups(3).Value)
                        Dim PatMatches As MatchCollection = Regex.Matches(LxeContent, findPattern, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        If ((PatMatches IsNot Nothing) AndAlso (PatMatches.Count > 0)) Then
                            If ((PatMatches(0) IsNot Nothing) AndAlso (PatMatches(0).Groups IsNot Nothing) AndAlso (PatMatches(0).Groups.Count > 0)) Then
                                If (PatMatches(0).Groups.Count = 3) Then
                                    LxeContent = Regex.Replace(LxeContent, findPattern, "$1" & ExpMatch.Groups(2).Value & "$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                ElseIf (PatMatches(0).Groups.Count = 4) Then
                                    LxeContent = Regex.Replace(LxeContent, findPattern, "$1" & ExpMatch.Groups(2).Value & "$3", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                End If
                            End If
                        Else
                            If (Regex.IsMatch(ExpMatch.Groups(2).Value, "[0-9]+", RegexOptions.IgnoreCase Or RegexOptions.Singleline)) Then
                                MissingPageNum.Add(ExpMatch.Groups(2).Value)
                            End If
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
            LxeContent = UpdateMissingPageNum(LxeContent, MissingPageNum)
            File.WriteAllText(Path.Combine(LxeXMLPath, Path.GetFileName(LxeFile)), LxeContent)
        Next
        Return True
    End Function

    Public Function UpdateMissingPageNum(ByVal LxeContent As String, ByVal MissingPgNum As List(Of String)) As String
        Dim PageNm As String = String.Empty
        If ((MissingPgNum Is Nothing) OrElse (MissingPgNum.Count = 0)) Then
            Return LxeContent
        End If
        For Each PgNm As String In MissingPgNum
            Dim Num As String = Regex.Replace(PgNm, "[^0-9+]", "")
            If (Not String.IsNullOrEmpty(Num)) Then
                LxeContent = LxeContent.Replace(String.Format("<a id=""page_" & Num - 1 & """/>"), String.Format("<a id=""page_" & Num - 1 & """/>") & String.Format("<a id=""page_" & Num & """/>"))
            End If
        Next
        Return LxeContent
    End Function

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

    Private Function GeneratePattern(ByVal firstChar As String, ByVal PageNum As String, ByVal SecodeChar As String) As String
        Dim Pattern As String = String.Empty
        Dim PatternLast As String = String.Empty
        If (String.IsNullOrEmpty(firstChar)) Then
            Pattern = "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)"
        Else
            For Each Chr As String In firstChar
                Pattern = IIf(String.IsNullOrEmpty(Pattern), Chr.Replace("(", "\(").Replace(")", "\)").Replace(".", "\."), Pattern & "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)" & Chr.Replace("(", "\(").Replace(")", "\)").Replace(".", "\."))
            Next
        End If
        Pattern = "(" & Pattern & "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*))(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)?("
        If (String.IsNullOrEmpty(SecodeChar)) Then
            PatternLast = "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)"
        Else
            For Each Chr As String In SecodeChar
                'Pattern = Pattern & "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)" & Chr
                PatternLast = IIf(String.IsNullOrEmpty(PatternLast), Chr.Replace("(", "\(").Replace(")", "\)").Replace(".", "\."), PatternLast & "(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*)" & Chr.Replace("(", "\(").Replace(")", "\)").Replace(".", "\."))
            Next
        End If
        Return String.Format("{0}{1}(?:(?:(?:\s)*<(?:[^>]+)>(?:\s)*)*))", Pattern, PatternLast)
    End Function

    Public Function TransformText(ByVal InputContent As String) As String
        Dim OutputXml As New StringBuilder
        Dim xslt As New XslCompiledTransform()
        Dim writeSetting As New XmlWriterSettings
        writeSetting.ConformanceLevel = ConformanceLevel.Fragment
        Dim XsltReader As XmlTextReader = Nothing
        Dim DirectDocBookXSL As String = "<?xml version=""1.0"" encoding=""UTF-8""?><xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" exclude-result-prefixes=""xs"" version=""1.0""><xsl:output method=""xml""></xsl:output><xsl:template match=""@*|comment()""><xsl:copy><xsl:apply-templates select=""@*|comment()""/></xsl:copy></xsl:template></xsl:stylesheet>"
        XsltReader = New XmlTextReader(New StringReader(DirectDocBookXSL))
        Dim InputReader As XmlReader = XmlReader.Create(New StringReader(InputContent))
        Dim OutputWriter As XmlWriter = XmlWriter.Create(New StringWriter(OutputXml), writeSetting)

        xslt.Load(XsltReader)
        Try
            xslt.Transform(InputReader, OutputWriter)
        Catch ex As Exception
            Return String.Empty
        End Try
        Return OutputXml.ToString().Replace("<space />", " ").Replace("&amp;", "&")
    End Function

    Private Function DoDownloadLXEXMLFiles(ByVal index As Integer) As Boolean
        Dim Dbdata As DigitalBundleData = GBL.DBDataList(index)
        Dim StatusUI As UploadDownloadHelper
        If ((Dbdata.FileOrderList Is Nothing) OrElse (Dbdata.FileOrderList.Count = 0)) Then
            GBL.DeantaBallon("Could not able to find the file order list", MessageType.MSGERROR)
            Return False
        End If
        For Each XMLFile As String In Dbdata.FileOrderList
            Try
                StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOAD, .LocalPath = Dbdata.Folder.LXEXMLPath, .UrlPath = Path.Combine(Path.GetDirectoryName(Dbdata.XmlURL), String.Format("{0}.xml", XMLFile)).Replace("\", "/"), .Index = index})
                AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                StatusUI.DoUploadDownload()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next
        Return True
    End Function

    Private Function DoDownloadInDesignDocumentAsset(ByVal index As Integer) As Boolean
        Dim InDesignList As New DataTable("indt")
        Dim StatusUI As UploadDownloadHelper
        Dim Dbdata As DigitalBundleData = GBL.DBDataList(index)
        Dim TmpInddFile As String = String.Empty
        Dim LocalTmpInddFile As String = String.Empty
        Dim OrgInddFile As String = String.Empty
        'InDesignList = MySqlHelper.ReadSqlData("SELECT * FROM `tb_documents` WHERE `project_id`=" & Dbdata.ProjectID & " and document_type='.indd' and task_id='112515' order by `document_id` desc")
        If (Dbdata.FileOrderList Is Nothing) OrElse (Dbdata.FileOrderList.Count = 0) Then
            GBL.DeantaBallon("Could not able to find the chapters.", MessageType.MSGERROR)
            Return False
        End If

        Dim InDesignAssetLst As New List(Of String)
        Dim LanstadAssetLst As New List(Of AssetData)

        'Generate indd patterns
        Dim QueryText As String = String.Empty
        QueryText = QueryText & "SELECT * FROM `tb_documents` WHERE `project_id`=" & Dbdata.ProjectID & " and document_type='.indd' order by `document_id` desc"
        Try
            InDesignList = MySqlHelper.ReadSqlData(QueryText)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        LanstadAssetLst.Clear()
        For Each ChpFile As String In Dbdata.FileOrderList
            If ((InDesignList IsNot Nothing) AndAlso (InDesignList.Rows IsNot Nothing) AndAlso (InDesignList.Rows.Count > 0)) Then
                Dim dtRow As DataRow = (From dt As DataRow In InDesignList.Rows Order By Convert.ToSingle(dt.Item("document_id")) Descending Where (String.Compare(Convert.ToString(dt.Item("document_name")), $"{Path.GetFileNameWithoutExtension(ChpFile).Replace("_new_out", "_new")}_indd.indd", True) = 0) Select dt).FirstOrDefault
                If (dtRow Is Nothing) Then
                    GBL.DeantaBallon("Could not able to find the asset for " & Path.GetFileName(ChpFile), MessageType.MSGERROR)
                    Continue For
                End If
                TmpInddFile = GBL.FTPResourcePath & "/" & Convert.ToString(dtRow.Item("document_path")).Replace("resources/", "")
                OrgInddFile = Path.Combine(Dbdata.Folder.WEBPDFPath, Convert.ToString(dtRow.Item("document_name")))
                GBL.DBDataList(index).InDesignFileList.Add(Convert.ToString(dtRow.Item("document_name")))
                LocalTmpInddFile = Path.Combine(Dbdata.Folder.WEBPDFPath, Convert.ToString(dtRow.Item("document_path")).Replace("resources/", ""))
                GBL.UpdateGridStatus(index, "File:" & Path.GetFileName(OrgInddFile))
                LanstadAssetLst.Add(New AssetData With {.LanstadFile = LocalTmpInddFile, .LocalFile = OrgInddFile, .IsDownloaded = False, .IsCopied = False})
                InDesignAssetLst.Add(TmpInddFile)
            Else
                GBL.DeantaBallon("Could not able to find the asset for " & Path.GetFileName(ChpFile), MessageType.MSGERROR)
                Continue For
            End If
        Next

        If (Dbdata.FileOrderList.Count <> InDesignAssetLst.Count) Then
            Dim MissingList As New List(Of String)
            MissingList = Dbdata.FileOrderList.Except(InDesignAssetLst)
            GBL.DeantaBallon($"Some InDesign files missing.{String.Join(",", MissingList.ToArray())}", MessageType.MSGERROR)
            Return False
        End If

        Try
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOADLIST, .LocalPath = Dbdata.Folder.WEBPDFPath, .UrlPath = "", .Index = index, .NeededFileList = InDesignAssetLst})
            AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If ((LanstadAssetLst IsNot Nothing) AndAlso (LanstadAssetLst.Count > 0)) Then
            For sl As Integer = 0 To LanstadAssetLst.Count - 1
                Dim AssetDa As AssetData = LanstadAssetLst(sl)
                If (File.Exists(AssetDa.LocalFile)) Then
                    File.Delete(AssetDa.LocalFile)
                End If
                If (File.Exists(AssetDa.LanstadFile)) Then
                    File.Copy(AssetDa.LanstadFile, Path.Combine(Dbdata.Folder.ApplicationPath, Path.GetFileName(AssetDa.LocalFile)), True)
                    'File.Copy(AssetDa.LanstadFile, AssetDa.LocalFile)
                End If
                If (File.Exists(AssetDa.LanstadFile)) Then
                    File.Move(AssetDa.LanstadFile, AssetDa.LocalFile)
                End If
            Next
        End If

        Dim ImagePath As String = GBL.DBDataList(index).Folder.WEBPDFPath
        If (Not Directory.Exists(ImagePath)) Then
            Directory.CreateDirectory(ImagePath)
        End If
        GBL.UpdateGridStatus(index, "Image downloading started")
        Try
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.DOWNLOADWEBPDF, .LocalPath = ImagePath, .UrlPath = GBL.DBDataList(index).ImagePath, .Index = index})
            AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        GBL.UpdateGridStatus(index, "Image downloading completed")
        Return True
    End Function

    Private Function SetISBNNum(ByVal index As Integer)
        Dim DtIsbn As New DataTable("DT")
        DtIsbn = MySqlHelper.ReadSqlData("select * from tb_projects where project_id =" & GBL.DBDataList(index).ProjectID & "")
        If ((DtIsbn IsNot Nothing) AndAlso (DtIsbn.Rows IsNot Nothing) AndAlso (DtIsbn.Rows.Count > 0)) Then
            If ((GBL.DBDataList(index).DocType = DocumentType.BLOOMSBURY) Or (GBL.DBDataList(index).DocType = DocumentType.TRD)) Then
                GBL.DBDataList(index).ApplicationISBN = Convert.ToString(DtIsbn.Rows(0).Item("hardback_isbn"))
            Else
                GBL.DBDataList(index).ApplicationISBN = Convert.ToString(DtIsbn.Rows(0).Item("isbn"))
            End If
            GBL.DBDataList(index).WebPDFISBN = Convert.ToString(DtIsbn.Rows(0).Item("ebook_pdf"))
            GBL.DBDataList(index).CoverISBN = Convert.ToString(DtIsbn.Rows(0).Item("ebook_master"))
            GBL.DBDataList(index).ePubISBN = Convert.ToString(DtIsbn.Rows(0).Item("ebook_epub"))

            GBL.DBDataList(index).HardbackISBN = Convert.ToString(DtIsbn.Rows(0).Item("hardback_isbn"))
            GBL.DBDataList(index).PaperbackISBN = Convert.ToString(DtIsbn.Rows(0).Item("paperback_isbn"))

            If (GBL.DBDataList(index).OrgDocType = DocumentType.ANTHEM) AndAlso (String.IsNullOrEmpty(GBL.DBDataList(index).HardbackISBN)) Then
                GBL.DBDataList(index).HardbackISBN = Convert.ToString(DtIsbn.Rows(0).Item("paperback_isbn"))
            End If

            GBL.DBDataList(index).WebPDFISBN = GBL.DBDataList(index).WebPDFISBN.Replace("-", "").Replace("_", "")
                GBL.DBDataList(index).ePubISBN = GBL.DBDataList(index).ePubISBN.Replace("-", "").Replace("_", "")
                GBL.DBDataList(index).CoverISBN = GBL.DBDataList(index).CoverISBN.Replace("-", "").Replace("_", "")
                GBL.DBDataList(index).ApplicationISBN = GBL.DBDataList(index).ApplicationISBN.Replace("-", "")
                GBL.DBDataList(index).HardbackISBN = GBL.DBDataList(index).HardbackISBN.Replace("-", "").Replace("_", "")
                GBL.DBDataList(index).PaperbackISBN = GBL.DBDataList(index).PaperbackISBN.Replace("-", "").Replace("_", "")

                GBL.DBDataList(index).ProjectAbb = Convert.ToString(DtIsbn.Rows(0).Item("abbreviation"))
                GBL.DBDataList(index).Description = Convert.ToString(DtIsbn.Rows(0).Item("blurbs"))
                GBL.DBDataList(index).Keywords = Convert.ToString(DtIsbn.Rows(0).Item("kwd_group"))
            End If

            If ((GBL.DBDataList(index).DocType = DocumentType.BLOOMSBURY) Or (GBL.DBDataList(index).DocType = DocumentType.TRD) Or (GBL.DBDataList(index).DocType = DocumentType.RL) Or (GBL.DBDataList(index).DocType = DocumentType.SEQUOIA) Or (GBL.DBDataList(index).OrgDocType = DocumentType.MUP)) Then
            GBL.DBDataList(index).Folder.WEBPDFName = String.Format("{0}_web.indb", GBL.DBDataList(index).WebPDFISBN)
        ElseIf ((GBL.DBDataList(index).DocType = DocumentType.TANDF) Or (GBL.DBDataList(index).DocType = DocumentType.TANDFUK) Or (GBL.DBDataList(index).DocType = DocumentType.CRITICALPUB)) Then
            GBL.DBDataList(index).Folder.WEBPDFName = String.Format("{0}_webpdf.indb", GBL.DBDataList(index).WebPDFISBN)
        End If
        GBL.DeantaBallon($"WEBPDF Isbn {GBL.DBDataList(index).WebPDFISBN}", MessageType.MSGINFO)
        GBL.DeantaBallon($"Epub Isbn {GBL.DBDataList(index).ePubISBN}", MessageType.MSGINFO)
        GBL.DeantaBallon($"Cover Isbn {GBL.DBDataList(index).CoverISBN}", MessageType.MSGINFO)
        GBL.DeantaBallon($"Application Isbn {GBL.DBDataList(index).ApplicationISBN}", MessageType.MSGINFO)
        GBL.DeantaBallon($"HardbackISBN {GBL.DBDataList(index).HardbackISBN}", MessageType.MSGINFO)
        GBL.DeantaBallon($"PaperbackISBN Isbn {GBL.DBDataList(index).PaperbackISBN}", MessageType.MSGINFO)

        GBL.DeantaBallon($"Project abbrevation {GBL.DBDataList(index).ProjectAbb}", MessageType.MSGINFO)

        If (Not String.IsNullOrEmpty(GBL.DBDataList(index).Description)) Then
            Try
                GBL.DBDataList(index).Description = GetInnerText(GBL.DBDataList(index).Description, "blurb[@type='short']")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If
        If (Not String.IsNullOrEmpty(GBL.DBDataList(index).Keywords)) Then
            Try
                GBL.DBDataList(index).Keywords = GetInnerText(GBL.DBDataList(index).Keywords, "root")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If
        Return True
    End Function
    Private Function GetInnerText(ByVal Metadata As String, Element As String) As String
        Dim XmlMet As New XmlDocument
        XmlMet.PreserveWhitespace = True
        If (String.IsNullOrEmpty(Metadata)) Then Return String.Empty
        Try
            XmlMet.LoadXml($"<root>{Metadata}</root>")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return String.Empty
        End Try
        Dim ReqNode As XmlNode = XmlMet.SelectSingleNode($"//{Element}")
        If (ReqNode IsNot Nothing) Then
            Return Regex.Replace(ReqNode.InnerText, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("""", "")
        End If
        Return String.Empty
    End Function

    Private Function FormatPackageFolderforRandL(ByVal index As Integer) As Boolean
        Dim DBdata As DigitalBundleData = GBL.DBDataList(index)
        Dim AppIndbFile As String = String.Empty
        Dim PackIndbFile As String = String.Empty
        If (DBdata Is Nothing) Then Return False
        If ((String.IsNullOrEmpty(DBdata.Folder.PackagePath)) OrElse (Not Directory.Exists(DBdata.Folder.PackagePath))) Then
            GBL.DeantaBallon("Directory Not found: " & DBdata.Folder.PackagePath, MessageType.MSGERROR)
            Return False
        End If

        Try
            AppIndbFile = Directory.GetFiles(DBdata.Folder.ApplicationPath, "*.indb", SearchOption.TopDirectoryOnly)(0)
            PackIndbFile = Directory.GetFiles(DBdata.Folder.PackagePath, "*.indb", SearchOption.TopDirectoryOnly)(0)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        If (String.IsNullOrEmpty(AppIndbFile) OrElse (Not File.Exists(AppIndbFile))) Then
            GBL.DeantaBallon($"Could not able to find the indb file in application path", MessageType.MSGERROR)
            Return False
        End If

        If (String.IsNullOrEmpty(PackIndbFile) OrElse (Not File.Exists(PackIndbFile))) Then
            GBL.DeantaBallon($"Could not able to find the indb file in package path", MessageType.MSGERROR)
            Return False
        End If
        Try
            File.Delete(PackIndbFile)
            File.Copy(AppIndbFile, Path.Combine(DBdata.Folder.PackagePath, Path.GetFileName(AppIndbFile)), True)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function FormatPackageFolderforBloomsbury(ByVal index As Integer) As Boolean
        Dim DBdata As DigitalBundleData = GBL.DBDataList(index)
        If (DBdata Is Nothing) Then Return False
        Dim NewLinkFolder As String = String.Empty
        Dim NewFontFolder As String = String.Empty
        Dim NewAppFolder As String = String.Empty
        If ((String.IsNullOrEmpty(DBdata.Folder.PackagePath)) OrElse (Not Directory.Exists(DBdata.Folder.PackagePath))) Then
            GBL.DeantaBallon("Directory Not found: " & DBdata.Folder.PackagePath, MessageType.MSGERROR)
            Return False
        End If
        Dim FontFolder As String = Path.Combine(DBdata.Folder.PackagePath, "Document fonts")
        If (DBdata.OrgDocType = DocumentType.ANTHEM) Then
            NewFontFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_fonts", DBdata.HardbackISBN))
        Else
            NewFontFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_fonts", DBdata.ApplicationISBN))
        End If

        If (Directory.Exists(FontFolder)) Then
            Directory.Move(FontFolder, NewFontFolder)
        End If
        Dim LinkFolder As String = Path.Combine(DBdata.Folder.PackagePath, "Links")
        If (DBdata.OrgDocType = DocumentType.ANTHEM) Then
            NewLinkFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_images", DBdata.HardbackISBN))
        Else
            NewLinkFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_images", DBdata.ApplicationISBN))
        End If
        If (Directory.Exists(LinkFolder)) Then
            Directory.Move(LinkFolder, NewLinkFolder)
        End If
        If (File.Exists(DBdata.CoverImageFullName)) Then
            Try
                File.Copy(DBdata.CoverImageFullName, Path.Combine(NewLinkFolder, Path.GetFileName(DBdata.CoverImageFullName)), True)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & " - Copy cover image", MessageType.MSGERROR)
            End Try
        End If

        'If (DBdata.DocType = DocumentType.BLOOMSBURY) Then
        '    Try
        '        File.Copy(GBL.BloomsburyLogo, Path.Combine(NewLinkFolder, Path.GetFileName(GBL.BloomsburyLogo)), True)
        '    Catch ex As Exception
        '        GBL.DeantaBallon(ex.Message & " - client logo image", MessageType.MSGERROR)
        '    End Try
        'End If
        If (DBdata.OrgDocType = DocumentType.ANTHEM) Then
            NewAppFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_app", DBdata.HardbackISBN))
        Else
            NewAppFolder = Path.Combine(DBdata.Folder.PackagePath, String.Format("{0}_txt_app", DBdata.ApplicationISBN))
        End If

        If (Not Directory.Exists(NewAppFolder)) Then
            Directory.CreateDirectory(NewAppFolder)
        End If
        For Each InddFile As String In Directory.GetFiles(DBdata.Folder.PackagePath, "*.indd", SearchOption.TopDirectoryOnly)
            File.Move(InddFile, Path.Combine(NewAppFolder, Path.GetFileName(InddFile)))
        Next

        Dim IndbFile As New List(Of String)
        IndbFile.AddRange(Directory.GetFiles(DBdata.Folder.PackagePath, "*.indb", SearchOption.TopDirectoryOnly))
        If ((IndbFile IsNot Nothing) AndAlso (IndbFile.Count > 0)) Then
            For Each Indb As String In IndbFile
                Try
                    File.Delete(Indb)
                Catch ex As Exception
                End Try
            Next
        End If

        Dim RTFFile As String = GetFileFromFolder(DBdata.Folder.RTFPath, "*.rtf")
        Dim DocxFile As String = String.Empty
        If (File.Exists(RTFFile)) Then
            DocxFile = RTFFile.Replace(".rtf", ".doc")
            Try
                ConvertHtmltoDocx(RTFFile, DocxFile)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Else
            GBL.DeantaBallon("Could not able to find the RTF file", MessageType.MSGERROR)
        End If

        Return True
    End Function
    Private Function LoadMainXMLtoSequence(ByVal Index As Integer) As Boolean
        Dim xmlDoc As New XmlDocument
        Try
            xmlDoc.LoadXml(Regex.Replace(File.ReadAllText(GBL.DBDataList(Index).MainXML).Replace("xi:include", "include"), "<book[^>]*>", "<book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Dim ChapterList As XmlNodeList = xmlDoc.SelectNodes("//include")
        If ((ChapterList Is Nothing) AndAlso (ChapterList.Count = 0)) Then
            Return False
        End If
        GBL.DBDataList(Index).FileOrderList = New List(Of String)
        For Each ChpNode As XmlNode In ChapterList
            Try
                If (GBL.DBDataList(Index).Stage = DBStage.XMLTRIGGER) Then
                    GBL.DBDataList(Index).FileOrderList.Add(Path.GetFileName(ChpNode.Attributes("href").Value).Replace(".indd", "_out.xml"))
                Else
                    GBL.DBDataList(Index).FileOrderList.Add(ChpNode.Attributes("href").Value.Replace(".indd", "_out.xml").Replace(".xml", "_out.xml"))
                End If

                If (GBL.DBDataList(Index).Stage = DBStage.XMLTRIGGER) Then
                    GBL.DBDataList(Index).InDesignFileList.Add(Path.GetFileName(ChpNode.Attributes("href").Value))
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next
        Return True
    End Function
    Private Function IdentifiyBookElement(ByVal LXEXMLPath As String, ByVal index As Integer) As Boolean
        Dim xmlDoc As New XmlDocument
        xmlDoc.PreserveWhitespace = True
        xmlDoc.XmlResolver = Nothing
        Dim XMLFiles As New List(Of String)
        If (Not Directory.Exists(LXEXMLPath)) Then
            Return False
        End If
        XMLFiles.AddRange(Directory.GetFiles(LXEXMLPath, "Main.xml"))
        If (XMLFiles Is Nothing) OrElse (XMLFiles.Count = 0) Then
            GBL.DeantaBallon("Could not able to find the Main.xml", MessageType.MSGERROR)
            Return False
        End If
        GBL.DBDataList(index).MainXML = XMLFiles(0)
        If (String.IsNullOrEmpty(GBL.DBDataList(index).MainXML)) Then
            GBL.DeantaBallon("Main.xml is not found", MessageType.MSGERROR)
            Return False
        End If
        XMLFiles.Clear()
        XMLFiles.AddRange(Directory.GetFiles(LXEXMLPath, "pageSection.xml"))
        If (XMLFiles Is Nothing) OrElse (XMLFiles.Count = 0) Then
            GBL.DeantaBallon("Could not able to find the pageSection.xml", MessageType.MSGERROR)
        Else
            GBL.DBDataList(index).PageSectionXML = XMLFiles(0)
        End If
        Try
            xmlDoc.LoadXml(Regex.Replace(File.ReadAllText(GBL.DBDataList(index).MainXML).Replace("xi:include", "include"), "<book[^>]*>", "<book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Dim ChapterList As XmlNodeList = xmlDoc.SelectNodes("//include")
        If ((ChapterList Is Nothing) AndAlso (ChapterList.Count = 0)) Then
            Return False
        End If
        GBL.DBDataList(index).FileOrderList = New List(Of String)
        For Each ChpNode As XmlNode In ChapterList
            Try
                If (GBL.DBDataList(index).Stage = DBStage.XMLTRIGGER) Then
                    GBL.DBDataList(index).FileOrderList.Add(Path.GetFileName(ChpNode.Attributes("href").Value).Replace(".indd", "_out.xml"))
                Else
                    GBL.DBDataList(index).FileOrderList.Add(ChpNode.Attributes("href").Value.Replace(".indd", "_out.xml").Replace(".xml", "_out.xml"))
                End If

                If (GBL.DBDataList(index).Stage = DBStage.XMLTRIGGER) Then
                    GBL.DBDataList(index).InDesignFileList.Add(Path.GetFileName(ChpNode.Attributes("href").Value))
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next
        If ((GBL.DBDataList(index).DocType = DocumentType.TANDF) Or (GBL.DBDataList(index).DocType = DocumentType.TANDFUK) Or (GBL.DBDataList(index).DocType = DocumentType.CRITICALPUB)) Then
            GBL.DeantaBallon("GS Preview order and move GS preivew files.", MessageType.MSGINFO)
            Try
                GetOrderForGSPreview(index)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            GBL.DeantaBallon("Move GS preivew files completed", MessageType.MSGINFO)
            Try
                MoveGSPreviewInDesign(index)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            GBL.DeantaBallon("GS Preview order and move GS preivew files completed", MessageType.MSGINFO)
        End If
        Return True
    End Function

    Private Function MoveGSPreviewInDesign(ByVal Index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(Index)
        If ((DBData.InDesignFileList Is Nothing) OrElse (DBData.InDesignFileList.Count = 0)) Then
            GBL.DeantaBallon("No InDesign files available", MessageType.MSGERROR)
            Return False
        End If
        Dim GSInDesign As String = String.Empty
        For d As Integer = 0 To DBData.GSPreviewFileList.Count - 1
            GSInDesign = Path.Combine(DBData.Folder.ApplicationPath, DBData.GSPreviewFileList(d))
            If (File.Exists(GSInDesign)) Then
                File.Copy(GSInDesign, Path.Combine(DBData.Folder.GSPreviewInDesign, DBData.GSPreviewFileList(d)), True)
            End If
        Next
        Return True
    End Function

    Private Function GetOrderForGSPreview(ByVal Index As Integer) As Boolean
        GBL.DBDataList(Index).GSPreviewFileList.Clear()
        If ((GBL.DBDataList(Index).InDesignFileList Is Nothing) OrElse (GBL.DBDataList(Index).InDesignFileList.Count = 0)) Then
            GBL.DeantaBallon("No indesign file list found", MessageType.MSGERROR)
            Return False
        End If
        For g As Integer = 0 To GBL.DBDataList(Index).InDesignFileList.Count - 1
            If (GBL.DBDataList(Index).InDesignFileList(g).ToLower().Contains("_prelims_")) Then
                GBL.DBDataList(Index).GSPreviewFileList.Add(GBL.DBDataList(Index).InDesignFileList(g))
            Else
                If (GBL.DBDataList(Index).InDesignFileList(g).Contains("_S001_")) Then
                    GBL.DBDataList(Index).GSPreviewFileList.Add(GBL.DBDataList(Index).InDesignFileList(g))
                ElseIf (GBL.DBDataList(Index).InDesignFileList(g).Contains("_C001_")) Then
                    GBL.DBDataList(Index).GSPreviewFileList.Add(GBL.DBDataList(Index).InDesignFileList(g))
                ElseIf (GBL.DBDataList(Index).InDesignFileList(g).Contains("_INT_")) Then
                    GBL.DBDataList(Index).GSPreviewFileList.Add(GBL.DBDataList(Index).InDesignFileList(g))
                Else
                    Exit For
                End If
            End If
        Next
        If ((GBL.DBDataList(Index).GSPreviewFileList IsNot Nothing) AndAlso (GBL.DBDataList(Index).GSPreviewFileList.Count > 0)) Then
            GBL.DeantaBallon("GS FileList : " & String.Join(",", GBL.DBDataList(Index).GSPreviewFileList.ToArray()), MessageType.MSGINFO)
        Else
            GBL.DeantaBallon("Could not able to generate the GS preview order.", MessageType.MSGINFO)
        End If
        Return True
    End Function

    Private Function CombinedGSPreivewFile(ByVal Index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(Index)
        Dim GsXMLFiles As New List(Of String)
        If (String.IsNullOrEmpty(DBData.Folder.GSPreviewXMLPath)) Then
            GBL.DeantaBallon("GS preview path is empty", MessageType.MSGERROR)
            Return False
        End If
        If (Not Directory.Exists(DBData.Folder.GSPreviewXMLPath)) Then
            GBL.DeantaBallon("Could not able to find the GS preview foler", MessageType.MSGERROR)
            Return False
        End If
        GsXMLFiles.AddRange(Directory.GetFiles(DBData.Folder.GSPreviewXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        If ((GsXMLFiles Is Nothing) OrElse (GsXMLFiles.Count = 0)) Then
            GBL.DeantaBallon("Initial: No XML file in the GS preview foler _ " & DBData.Folder.GSPreviewXMLPath, MessageType.MSGERROR)
            Return False
        End If
        Dim GSContent As String = String.Empty
        DBData.GSPreviewCombinedXML = Path.Combine(DBData.Folder.GSPreviewXMLPath, "GSPreview.xml")
        If (File.Exists(DBData.GSPreviewCombinedXML)) Then
            File.Delete(DBData.GSPreviewCombinedXML)
        End If

        Dim GsDataBiblio As New GSPreviewData
        Dim GsDataNote As New GSPreviewData
        Try
            GsDataNote = GetBibliographyContents(DBData.Folder.GSPreviewXMLPath, "*_note_*.xml")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        If (GsDataNote Is Nothing) Then
            GBL.DeantaBallon("No Note contains found for GSPreview", MessageType.MSGERROR)
        End If
        If ((GsDataNote IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(GsDataNote.CombinedText))) Then
            If (String.IsNullOrEmpty(GsDataNote.NoteTitle)) Then
                GsDataNote.NoteTitle = "<title aid:pstyle=""1"">Notes</title>"
            End If
            GsDataNote.CombinedText = GsDataNote.CombinedText.Replace("<section role=""gs"">", $"<section role=""gs"">{vbLf}{GsDataNote.NoteTitle}{vbLf}")
        End If
        GBL.DeantaBallon("GSPreview - bio start", MessageType.MSGERROR)
        Try
            GsDataBiblio = GetBibliographyContents(DBData.Folder.GSPreviewXMLPath, "*_bib.xml")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        If (GsDataBiblio Is Nothing) Then
            GBL.DeantaBallon("Could not able to get the Bib GSPreview notes Contents", MessageType.MSGERROR)
        End If
        If ((GsDataBiblio IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(GsDataBiblio.CombinedText))) Then
            If (String.IsNullOrEmpty(GsDataBiblio.BiblioTitle)) Then
                GsDataBiblio.BiblioTitle = "<title aid:pstyle=""1"">Biobliography</title>"
            End If
            GsDataBiblio.CombinedText = GsDataBiblio.CombinedText.Replace("<bibliography role=""gs"">", $"<bibliography role=""gs"">{vbLf}{GsDataBiblio.BiblioTitle}{vbLf}")
        End If

        If ((GsDataNote IsNot Nothing) AndAlso (GsDataBiblio IsNot Nothing)) Then
            GSContent = $"<book xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:mml=""http://www.w3.org/1998/Math/MathML"" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/"" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/""><chapter></chapter><zroot>{GsDataBiblio.CombinedText}</zroot></book>"
        ElseIf ((GsDataNote Is Nothing) AndAlso (GsDataBiblio IsNot Nothing)) Then
            GSContent = $"<book xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:mml=""http://www.w3.org/1998/Math/MathML"" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/"" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/""><chapter></chapter><zroot>{GsDataBiblio.CombinedText}</zroot></book>"
        ElseIf ((GsDataNote IsNot Nothing) AndAlso (GsDataBiblio Is Nothing)) Then
            GSContent = $"<book xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:mml=""http://www.w3.org/1998/Math/MathML"" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/"" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/""><chapter></chapter><zroot>{GsDataNote.CombinedText}</zroot></book>"
        End If

        File.WriteAllText(DBData.GSPreviewCombinedXML, GSContent)
        Return True
    End Function

    Private Function GetBibliographyContents(ByVal XMLFolder As String, ByVal Pattern As String) As GSPreviewData
        Dim GsXMLFiles As New List(Of String)
        Dim GSData As New GSPreviewData
        Dim BiblioTitle As String = String.Empty
        Dim NoteTitle As String = String.Empty
        Dim GSContent As String = String.Empty
        GsXMLFiles.AddRange(Directory.GetFiles(XMLFolder, Pattern, SearchOption.TopDirectoryOnly))
        If ((GsXMLFiles Is Nothing) OrElse (GsXMLFiles.Count = 0)) Then
            GBL.DeantaBallon("Pattern: No XML file in the GS preview foler _ " & XMLFolder, MessageType.MSGERROR)
            Return Nothing
        End If
        For g As Integer = 0 To GsXMLFiles.Count - 1
            Dim content As String = File.ReadAllText(GsXMLFiles(g))
            content = content.Replace("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>", "")
            content = Regex.Replace(content, "<!--<a id=""page_[0-9]+""/>-->", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            If (g = 0) Then
                BiblioTitle = Regex.Match(content, "<bibliography[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?(<title[^>]*>((?:(?!<\/title>).)*)</title>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
                NoteTitle = Regex.Match(content, "<section[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?(<title[^>]*>((?:(?!<\/title>).)*)</title>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
                content = Regex.Replace(content, "<bibliography[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?<title[^>]*>((?:(?!<\/title>).)*)</title>", "<bibliography role=""gs"">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                content = Regex.Replace(content, "<section[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?<title[^>]*>((?:(?!<\/title>).)*)</title>", "<section role=""gs"">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                content = Regex.Replace(content, "<bibliography[^>]*>", "<bibliography role=""gs"">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                content = Regex.Replace(content, "<section[^>]*>", "<section role=""gs"">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Else
                content = Regex.Replace(content, "<bibliography[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?<title[^>]*>((?:(?!<\/title>).)*)</title>", "<bibliography>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                content = Regex.Replace(content, "<section[^>]*>[\r\n\u2029]*(?:<!--<[^>]*>-->)?<title[^>]*>((?:(?!<\/title>).)*)</title>", "<section>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            End If
            content = content.Replace("<bibliography role=""gs"">" & ChrW(8233), "<bibliography role=""gs"">")
            content = content.Replace("<section role=""gs"">" & ChrW(8233), "<section role=""gs"">")
            GSContent = GSContent & content.TrimStart(vbLf)
        Next
        GSContent = Regex.Replace(GSContent, $"({ChrW(8233)}</bibliography><bibliography[^>]*>)({ChrW(8233)})", $"$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        'GSContent = GSContent.Replace($"{ChrW(8233)}</bibliography><bibliography>{ChrW(8233)}", $"{ChrW(8233)}</bibliography><bibliography>")
        Return New GSPreviewData With {.CombinedText = GSContent, .NoteTitle = NoteTitle, .BiblioTitle = BiblioTitle}
    End Function
    Private Function CreateBookBasedonMainXML(ByVal index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim BookArgs As String = String.Empty
        Dim Server As String = "http://localhost:55508"
        File.WriteAllText(Path.Combine(DBData.Folder.WorkingPath, "filelist.txt"), String.Join("|", DBData.InDesignFileList.ToArray()))
        BookArgs = GBL.InDesignServerSoap & " -host """ & Server & """ """ & GBL.InDesignServerScript & """ digitialID=""" & DBData.DigitalID & """ orgDocType=""" & DBData.OrgDocType & """ docType=""" & DBData.DocType & """ chunkISBN=""" & DBData.ApplicationISBN & """ exportXMLPath=""" & DBData.Folder.ExportXMLPath.Replace("\", "/") & "/" & """ webPDFPath=""" & DBData.Folder.WEBPDFPath.Replace("\", "/") & "/" & """ imagePath=""" & DBData.Folder.WEBPDFPath.Replace("\", "/") & "/images/" & """ combinedRTFPath=""" & DBData.Folder.RTFPath.Replace("\", "/") & "/" & """ bookName=""" & DBData.Folder.WEBPDFName & """ logPath=" & GBL.LogFilePath.Replace("\", "/") & " fileOrderList=""" & Path.Combine(DBData.Folder.WorkingPath, "filelist.txt") & """ pageSectionXML=""" & DBData.PageSectionXML.Replace("\", "/") & """ webPDFDescription=""" & DBData.Description & """ webPDFKeyword=""" & DBData.Keywords & """ templateFile=""" & DBData.TemplateFullName.Replace("\", "/") & """ coverImage=""" & DBData.CoverImageFullName.Replace("\", "/") & """ packagePath=""" & DBData.Folder.PackagePath.Replace("\", "/") & "/" & """ printPresetPath=""" & GBL.PrintPresetPath.Replace("\", "/") & """ jobOptionPath=""" & GBL.JobOptionPath.Replace("\", "/") & """ PODPath=""" & DBData.Folder.PODPdfPath.Replace("\", "/") & "/" & """ bookPDFPath=""" & DBData.Folder.BookPDFPath.Replace("\", "/") & "/" & """ clientAbb=""UK"" taskList=""" & String.Join(",", DBData.TaskList) & """"
        If (Not CreateBatAndRunFile(BookArgs, DBData.Folder.WorkingPath, "webpdf.bat")) Then
            GBL.DeantaBallon("Error occurred While create bat file.", MessageType.MSGERROR)
            Return False
        End If
        Return True
    End Function

    Private Function CreateGSPreviewPDFGeneration(ByVal index As Integer) As Boolean
        Dim DBData As DigitalBundleData = GBL.DBDataList(index)
        Dim BookArgs As String = String.Empty
        Dim Server As String = "http://localhost:55508"
        BookArgs = GBL.InDesignServerSoap & " -host """ & Server & """ """ & GBL.GSPreviewScript & """ digitialID=""" & DBData.DigitalID & """ orgDocType=""" & DBData.OrgDocType & """ docType=""" & DBData.DocType & """ gsPreviewXML=""" & DBData.GSPreviewCombinedXML & """ webPDFPath=""" & DBData.Folder.GSPreviewInDesign.Replace("\", "/") & "/" & """ imagePath=""" & DBData.Folder.WEBPDFPath.Replace("\", "/") & "/images/" & """ bookName=""" & $"{DBData.CoverISBN}_GSpreview.indb" & """ logPath=" & GBL.LogFilePath.Replace("\", "/") & " fileOrderList=""" & String.Join("|", DBData.GSPreviewFileList.ToArray()) & """ printPresetPath=""" & GBL.PrintPresetPath.Replace("\", "/") & """ jobOptionPath=""" & GBL.JobOptionPath.Replace("\", "/") & """"
        If (Not CreateBatAndRunFile(BookArgs, DBData.Folder.WorkingPath, "gspdf.bat")) Then
            GBL.DeantaBallon("Error occurred while create bat file.", MessageType.MSGERROR)
            Return False
        End If
        Return True
    End Function

    Private Function FindInddTemplatePath(ByVal Index As Integer, ByVal TaskType As DocumentType) As String
        Dim TemplateFileName As String = String.Empty
        Dim TempStartName As String = String.Empty
        Dim InDesignTemplate As String = String.Empty
        Dim TemplatePath As String = String.Empty
        'TemplateFileName = TemplateFileName.Split(New String() {"-", "_"}, StringSplitOptions.RemoveEmptyEntries)(0)
        If (TaskType = DocumentType.JOURNAL) Then
            TemplatePath = GBL.JournalTemplate
        ElseIf ((TaskType = DocumentType.BOOK) Or (TaskType = DocumentType.TANDF) Or (TaskType = DocumentType.TANDFUK) Or (TaskType = DocumentType.BDS) Or (TaskType = DocumentType.BLOOMSBURY) Or (TaskType = DocumentType.TRD) Or (TaskType = DocumentType.CM)) Or (TaskType = DocumentType.INFORMALAW) Or (GBL.DBDataList(index).OrgDocType = DocumentType.MUP) Then
            TemplatePath = GBL.BookTemplate
        End If

        TemplatePath = GBL.BookTemplate

        If (Not Directory.Exists(TemplatePath)) Then
            GBL.DeantaBallon("Could Not able to find the InDesign template Directory." & TemplatePath, MessageType.MSGERROR)
        End If

        Array.ForEach(Directory.GetFiles(TemplatePath, "*.indt"), Sub(tmpfile As String)
                                                                      If (String.Compare(Path.GetFileNameWithoutExtension(tmpfile), GBL.DBDataList(Index).TemplateFullName, True) = 0) Then
                                                                          InDesignTemplate = tmpfile
                                                                      End If
                                                                  End Sub)

        GBL.DBDataList(Index).TemplateFullName = InDesignTemplate
        Return InDesignTemplate
    End Function


    'Private Function TFBitsConversion(ByVal index As Integer, ByVal FinalInXML As String, ByVal FinalOutXML As String) As Boolean
    '    Dim DbData As DigitalBundleData = GBL.DBDataList(index)
    '    If (Not File.Exists(FinalInXML)) Then
    '        Return False
    '    End If
    '    Dim OutputPath As String = String.Empty
    '    Dim BatFileContent As String = String.Empty
    '    OutputPath = Path.Combine(Path.Combine(Path.GetTempPath, Environment.UserName), "digitial")
    '    If (Directory.Exists(OutputPath)) Then
    '        Array.ForEach(Directory.GetFiles(OutputPath), Sub(sfile As String)
    '                                                          Try
    '                                                              File.Delete(sfile)
    '                                                          Catch ex As Exception
    '                                                          End Try
    '                                                      End Sub)
    '    Else
    '        Directory.CreateDirectory(OutputPath)
    '    End If

    '    If (File.Exists(FinalOutXML)) Then
    '        File.Delete(FinalOutXML)
    '    End If

    '    If File.Exists(Path.Combine(GBL.AppPath, "saxon9.jar")) Then File.Copy(Path.Combine(GBL.AppPath, "saxon9.jar"), OutputPath & "\saxon9.jar")
    '    If File.Exists(Path.Combine(GBL.AppPath, FinalInXML)) Then File.Copy(Path.Combine(GBL.AppPath, FinalInXML), OutputPath & "\" & Path.GetFileName(FinalInXML))
    '    If File.Exists(GBL.TNBitConversionXSL) Then File.Copy(GBL.TNBitConversionXSL, Path.Combine(OutputPath, Path.GetFileName(GBL.TNBitConversionXSL)))

    '    If (Not Directory.Exists(Path.Combine(OutputPath, "xsl"))) Then
    '        Directory.CreateDirectory(Path.Combine(OutputPath, "xsl"))
    '    End If

    '    Array.ForEach(Directory.GetFiles(GBL.TNBitXSLPath), Sub(sfile As String)
    '                                                            Try
    '                                                                File.Copy(sfile, Path.Combine(Path.Combine(OutputPath, "xsl"), Path.GetFileName(sfile)))
    '                                                            Catch ex As Exception
    '                                                            End Try
    '                                                        End Sub)

    '    BatFileContent = "java -jar """ & Path.GetFileName(Path.Combine(GBL.AppPath, "saxon9.jar")) & """ -s:""" & Path.GetFileName(FinalInXML) & """ -xsl:""" &
    '                                                       Path.GetFileName(GBL.TNBitConversionXSL) & """ -o:""" & Path.GetFileName(FinalOutXML) & """"

    '    Try
    '        Dim xslt As New FileInfo(Path.Combine(OutputPath, Path.GetFileName(GBL.TNBitConversionXSL)))
    '        Dim InputXML As New FileInfo(Path.Combine(OutputPath, Path.GetFileName(FinalInXML)))
    '        Dim outputXML As New FileInfo(Path.Combine(OutputPath, Path.GetFileName(FinalOutXML)))
    '        Dim xsltProcs As New Processor()
    '        Dim Complier = xsltProcs.NewXsltCompiler
    '        Dim execuable = Complier.Compile(New Uri(xslt.FullName))

    '        Dim destination As New DomDestination()
    '        Dim inputStream = InputXML.OpenRead()
    '        Dim transformer = execuable.Load()
    '        transformer.SetInputStream(inputStream, New Uri(InputXML.DirectoryName))
    '        transformer.Run(destination)
    '        destination.XmlDocument.Save(outputXML.FullName)
    '    Catch ex As Exception
    '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
    '    End Try

    '    If (File.Exists(Path.Combine(OutputPath, Path.GetFileName(FinalOutXML)))) Then
    '        File.Copy(Path.Combine(OutputPath, Path.GetFileName(FinalOutXML)), FinalOutXML, True)
    '    End If
    '    Return True
    'End Function

    Private Function BloomsburyClientXMLConversion(ByVal index As Integer, ByVal FinalXMLPath As String) As Boolean
        Dim IsSuccess As Boolean = False
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim BloomsMgr As New BloomsburyCleanupManager
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
        BloomsMgr.sISBN = "1234567890123"
        If (String.IsNullOrEmpty(DbData.MainXML)) Then
            BloomsMgr.FileSequence.AddRange(Directory.GetFiles(FinalXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        Else
            LoadMainXMLtoSequence(index)
            BloomsMgr.FileSequence = DbData.FileOrderList
        End If
        BloomsMgr.sXMLFileName = String.Format("{0}.xml", DbData.ApplicationISBN)
        BloomsMgr.AppPath = GBL.AppPath
#Else
        BloomsMgr.sISBN = DbData.ApplicationISBN
        BloomsMgr.FileSequence = DbData.FileOrderList
        BloomsMgr.sXMLFileName = String.Format("{0}.xml", DbData.ApplicationISBN)
        BloomsMgr.AppPath = GBL.AppPath
#End If
        GBL.DeantaBallon("Bloomsbury conversion started.", MessageType.MSGINFO)
        GBL.DeantaBallon("File sequence :" & String.Join(", ", BloomsMgr.FileSequence.ToArray()), MessageType.MSGINFO)

        Try
            IsSuccess = BloomsMgr.MainXMLPro(FinalXMLPath, False, DbData.ProjectID, DbData.AbstractXML)
#If CONFIG <> "Debug" Then

            If (IsSuccess) Then
                GBL.DBDataList(index).ClientXML = Path.Combine(DbData.Folder.ClientXMLPath, BloomsMgr.sXMLFileName)
                GBL.DBDataList(index).ClientCleanXML = Path.Combine(DbData.Folder.ClientXMLPath, BloomsMgr.sXMLFileName.Replace(".xml", "_tfbits.xml"))
                GBL.DBDataList(index).ClientOutXML = Path.Combine(DbData.Folder.ClientXMLPath, String.Format("{0}.xml", DbData.ApplicationISBN))

                GBL.DeantaBallon("Bloomsbury Clenaup : " & GBL.DBDataList(index).ClientXML, MessageType.MSGINFO)
                GBL.DeantaBallon("Bloomsbury Clenaup Out : " & GBL.DBDataList(index).ClientCleanXML, MessageType.MSGINFO)
                GBL.DeantaBallon("Bloomsbury Out : " & GBL.DBDataList(index).ClientOutXML, MessageType.MSGINFO)

                If (File.Exists(Path.Combine(FinalXMLPath, BloomsMgr.sXMLFileName))) Then
                    File.Copy(Path.Combine(FinalXMLPath, BloomsMgr.sXMLFileName), GBL.DBDataList(index).ClientXML, True)
                End If
            Else
                GBL.DeantaBallon("Error occur while converting T&F XML cleanup.", MessageType.MSGERROR)
            End If
            GBL.DeantaBallon("Bloomsbury conversion completed.", MessageType.MSGINFO)
#End If
            Return IsSuccess
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try


    End Function

    Private Function TFClientXMLConversion(ByVal index As Integer, ByVal FinalXMLPath As String) As Boolean
        Dim IsSuccess As Boolean = False
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim TFMgr As New TFXMLEpubManager
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
        TFMgr.sISBN = "1234567890123"
        If (String.IsNullOrEmpty(DbData.MainXML)) Then
            TFMgr.FileSequence.AddRange(Directory.GetFiles(FinalXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        Else
            LoadMainXMLtoSequence(index)
            TFMgr.FileSequence = DbData.FileOrderList
        End If
        TFMgr.sXMLFileName = String.Format("{0}.xml", DbData.ApplicationISBN)
        TFMgr.AppPath = GBL.AppPath
        TFMgr.DocType = DbData.DocType
        TFMgr.OrgDocType = DbData.OrgDocType
#Else
        TFMgr.sISBN = DbData.ApplicationISBN
        TFMgr.FileSequence = DbData.FileOrderList
        TFMgr.sXMLFileName = String.Format("{0}.xml", DbData.CoverISBN)
        TFMgr.AppPath = GBL.AppPath
        TFMgr.DocType = DbData.DocType
        TFMgr.OrgDocType = DbData.OrgDocType
#End If
        GBL.DeantaBallon("TNF conversion started.", MessageType.MSGINFO)
        GBL.DeantaBallon("File sequence :" & String.Join(", ", TFMgr.FileSequence.ToArray()), MessageType.MSGINFO)
        Try
            IsSuccess = TFMgr.MainXMLPro(FinalXMLPath, False, DbData.ProjectID, DbData.AbstractXML)
#If CONFIG <> "Debug" Then

            If (IsSuccess) Then
                GBL.DBDataList(index).ClientXML = Path.Combine(DbData.Folder.ClientXMLPath, TFMgr.sXMLFileName)
                GBL.DBDataList(index).ClientCleanXML = Path.Combine(DbData.Folder.ClientXMLPath, TFMgr.sXMLFileName.Replace(".xml", "_tfbits.xml"))
                GBL.DBDataList(index).ClientOutXML = Path.Combine(DbData.Folder.ClientXMLPath, String.Format("{0}.xml", DbData.ApplicationISBN))

                GBL.DeantaBallon("TNF Clenaup : " & GBL.DBDataList(index).ClientXML, MessageType.MSGINFO)
                GBL.DeantaBallon("TNF Clenaup Out : " & GBL.DBDataList(index).ClientCleanXML, MessageType.MSGINFO)
                GBL.DeantaBallon("TNF Out : " & GBL.DBDataList(index).ClientOutXML, MessageType.MSGINFO)

                If (File.Exists(Path.Combine(FinalXMLPath, TFMgr.sXMLFileName))) Then
                    File.Copy(Path.Combine(FinalXMLPath, TFMgr.sXMLFileName), GBL.DBDataList(index).ClientXML, True)
                End If
            Else
                GBL.DeantaBallon("Error occur while converting T&F XML cleanup.", MessageType.MSGERROR)
            End If
            GBL.DeantaBallon("TNF conversion completed.", MessageType.MSGINFO)
#End If
            Return IsSuccess
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try


    End Function

    'RLXMLCleanupManager
    Private Function RLClientXMLConversion(ByVal index As Integer, ByVal FinalXMLPath As String) As Boolean
        Dim IsSuccess As Boolean = False
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim RlMgr As New RLXMLCleanupManager
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
        If (String.IsNullOrEmpty(DbData.MainXML)) Then
            RlMgr.FileSequence.AddRange(Directory.GetFiles(FinalXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
            RlMgr.OutputFolder = Path.Combine($"{DbData.ApplicationISBN}_xml", "components")
        Else
            LoadMainXMLtoSequence(index)
            RlMgr.FileSequence = DbData.FileOrderList
        End If
        RlMgr.AppPath = GBL.AppPath
        RlMgr.ProjectName = DbData.ProjectName
#Else
        RlMgr.FileSequence = DbData.FileOrderList
        RlMgr.sXMLFileName = String.Format("{0}.xml", DbData.ePubISBN)
        RlMgr.AppPath = GBL.AppPath
        RlMgr.ProjectName = DbData.ProjectName
        RlMgr.OutputFolder = Path.Combine($"{DbData.ApplicationISBN}_xml", "components")
#End If
        GBL.DeantaBallon("RL XML conversion started.", MessageType.MSGINFO)
        GBL.DeantaBallon("File sequence :" & String.Join(",", RlMgr.FileSequence.ToArray()), MessageType.MSGINFO)
        Try
            IsSuccess = RlMgr.DoMainXMLPro(FinalXMLPath)
            GBL.DeantaBallon("RL XML conversion completed.", MessageType.MSGINFO)

        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            IsSuccess = False
        End Try
        Try
            CopyRLSchemaToOutput(Path.Combine(DbData.Folder.FinalXMLPath, RlMgr.OutputFolder))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Return IsSuccess
    End Function

    Private Function CopyRLSchemaToOutput(ByVal XMLFolder As String) As Boolean
        Dim SchemaFolder As String = String.Empty
        SchemaFolder = Path.Combine(GBL.AppPath, "RL_Schema")
        If (Not Directory.Exists(SchemaFolder)) Then
            GBL.DeantaBallon($"RL Schema folder is missing.- {SchemaFolder}", MessageType.MSGERROR)
            Return False
        End If
        If (Not Directory.Exists(XMLFolder)) Then
            GBL.DeantaBallon($"RL XML is missing.- {XMLFolder}", MessageType.MSGERROR)
            Return False
        End If
        For Each SchFile As String In Directory.GetFiles(SchemaFolder, "*.*", SearchOption.TopDirectoryOnly)
            Try
                File.Copy(SchFile, Path.Combine(XMLFolder, Path.GetFileName(SchFile)), True)
            Catch ex As Exception
            End Try
        Next
        Return True
    End Function

    'RLePubCleanupManager
    Private Function RLClientEPubConversion(ByVal index As Integer, ByVal FinalXMLPath As String) As Boolean
        Dim IsSuccess As Boolean = False
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim RlMgr As New RLePubCleanupManager
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
        RlMgr.sISBN = "9780429656187"
        If (String.IsNullOrEmpty(DbData.MainXML)) Then
            RlMgr.FileSequence.AddRange(Directory.GetFiles(FinalXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
        Else
            LoadMainXMLtoSequence(index)
            RlMgr.FileSequence = DbData.FileOrderList
        End If
        RlMgr.sXMLFileName = String.Format("{0}.xml", RlMgr.sISBN)
        RlMgr.AppPath = GBL.AppPath
#Else
        RlMgr.sISBN = DbData.ePubISBN
        RlMgr.FileSequence = DbData.FileOrderList
        RlMgr.sXMLFileName = String.Format("{0}.xml", DbData.ePubISBN)
        RlMgr.AppPath = GBL.AppPath
#End If
        GBL.DeantaBallon("RL ePub conversion started.", MessageType.MSGINFO)
        GBL.DeantaBallon("File sequence :" & String.Join(",", RlMgr.FileSequence.ToArray()), MessageType.MSGINFO)
        Try
            IsSuccess = RlMgr.MainRLePubXMLPro(FinalXMLPath, True)

            If (IsSuccess) Then
                GBL.DBDataList(index).ClientePubXML = Path.Combine(DbData.Folder.DeliveryePub, RlMgr.sXMLFileName)
                If (File.Exists(Path.Combine(FinalXMLPath, RlMgr.sXMLFileName))) Then
                    If (Directory.Exists(DbData.Folder.DeliveryePub)) Then
                        File.Copy(Path.Combine(FinalXMLPath, RlMgr.sXMLFileName), GBL.DBDataList(index).ClientePubXML, True)
                    End If
                    File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rl_epub.xsl"), Path.Combine(FinalXMLPath, "rl_epub.xsl"), True)
                    File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ent.xsl"), Path.Combine(FinalXMLPath, "ent.xsl"), True)
                End If
            Else
                GBL.DeantaBallon("Error occurred while converting ePub conversion.", MessageType.MSGERROR)
            End If
            GBL.DeantaBallon("RL conversion completed.", MessageType.MSGINFO)

            Return IsSuccess
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function


    Private Function TFClientEPubConversion(ByVal index As Integer, ByVal FinalXMLPath As String) As Boolean
        Dim IsSuccess As Boolean = False
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim TFMgr As New TFXMLEpubManager
        GBL.LogFilePath = Path.Combine(FinalXMLPath, String.Format("{0}.txt", DbData.DigitalID))
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
        TFMgr.sISBN = "9780429656187"
        If (String.IsNullOrEmpty(DbData.MainXML)) Then
            TFMgr.FileSequence.AddRange(Directory.GetFiles(FinalXMLPath, "*.xml", SearchOption.TopDirectoryOnly))
            Array.Sort(Of String)(TFMgr.FileSequence.ToArray())
        Else
            LoadMainXMLtoSequence(index)
            TFMgr.FileSequence = DbData.FileOrderList

        End If
        TFMgr.sXMLFileName = String.Format("{0}.xml", TFMgr.sISBN)
        TFMgr.AppPath = GBL.AppPath
        TFMgr.DocType = DbData.DocType
        TFMgr.OrgDocType = DbData.OrgDocType
#Else
        TFMgr.sISBN = DbData.ePubISBN
        TFMgr.FileSequence = DbData.FileOrderList
        TFMgr.sXMLFileName = String.Format("{0}.xml", DbData.ePubISBN)
        TFMgr.AppPath = GBL.AppPath
        TFMgr.DocType = IIf(DbData.DocType = DocumentType.BLOOMSBURY, DocumentType.RL, DbData.DocType)
        TFMgr.OrgDocType = DbData.OrgDocType
#End If
        GBL.DeantaBallon("TNF ePub conversion started.", MessageType.MSGINFO)
        GBL.DeantaBallon("File sequence :" & String.Join(",", TFMgr.FileSequence.ToArray()), MessageType.MSGINFO)
        GBL.DeantaBallon($"TNF ePub client type {TFMgr.OrgDocType.ToString()}", MessageType.MSGINFO)

        Try
            IsSuccess = TFMgr.MainXMLPro(FinalXMLPath, True)

            If (IsSuccess) Then
                GBL.DBDataList(index).ClientePubXML = Path.Combine(DbData.Folder.DeliveryePub, TFMgr.sXMLFileName)
                If (File.Exists(Path.Combine(FinalXMLPath, TFMgr.sXMLFileName))) Then
                    If (Directory.Exists(DbData.Folder.DeliveryePub)) Then
                        File.Copy(Path.Combine(FinalXMLPath, TFMgr.sXMLFileName), GBL.DBDataList(index).ClientePubXML, True)
                    End If
                    If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bls_epub.xsl"), Path.Combine(FinalXMLPath, "bls_epub.xsl"), True)
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ent.xsl"), Path.Combine(FinalXMLPath, "ent.xsl"), True)
                    ElseIf ((DbData.DocType = DocumentType.RL) Or (DbData.DocType = DocumentType.SEQUOIA)) Then
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rl_epub.xsl"), Path.Combine(FinalXMLPath, "rl_epub.xsl"), True)
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ent.xsl"), Path.Combine(FinalXMLPath, "ent.xsl"), True)
                    Else
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tnf_epub.xsl"), Path.Combine(FinalXMLPath, "tnf_epub.xsl"), True)
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ent.xsl"), Path.Combine(FinalXMLPath, "ent.xsl"), True)
                    End If
                End If
            Else
                GBL.DeantaBallon("Error occurred while converting ePub conversion.", MessageType.MSGERROR)
            End If
            GBL.DeantaBallon("TNF conversion completed.", MessageType.MSGINFO)

            Return IsSuccess
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

    End Function

    Private Function CreateBatAndRunFile(BatFileContent As String, OutputPath As String, Optional ByVal FileName As String = "") As Boolean
        If (String.IsNullOrEmpty(FileName)) Then
            FileName = "run.bat"
        End If
        Try
            If (File.Exists(Path.Combine(OutputPath, FileName))) Then File.Delete(Path.Combine(OutputPath, FileName))
            File.WriteAllText(Path.Combine(OutputPath, FileName), $"chcp 1252{vbNewLine}{BatFileContent}", Encoding.Default)
            While (File.Exists(Path.Combine(OutputPath, FileName)))
                Exit While
            End While
            Dim SaxjanProcessInfo As New ProcessStartInfo(Path.Combine(OutputPath, FileName))
            SaxjanProcessInfo.WorkingDirectory = OutputPath
            SaxjanProcessInfo.RedirectStandardError = True
            SaxjanProcessInfo.RedirectStandardOutput = True
            SaxjanProcessInfo.CreateNoWindow = True
            SaxjanProcessInfo.WindowStyle = ProcessWindowStyle.Hidden
            SaxjanProcessInfo.UseShellExecute = False
            Dim SaxjanProcess As Process = Process.Start(SaxjanProcessInfo)
            SaxjanProcess.WaitForExit()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub BatExited(sender As Object, e As EventArgs)
        MsgBox("")
    End Sub

    Private Function UploadRequiredFilesForBloomsbury(ByVal index As Integer, Optional ByVal isNeedUpload As Boolean = True, Optional ByVal Stage As DBStage = DBStage.NONE) As Boolean
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim WEBPDF As String = String.Empty
        Dim BookPDF As String = String.Empty

        GBL.DBDataList(index).FinalAssets.Clear()
        GBL.OutputDataList.Clear()
        Dim XMLZip As String = String.Empty

        If (Stage = DBStage.XMLTRIGGER) Then
            Try
                XMLZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_xml.zip", DbData.ApplicationISBN))

                If (GBL.DBDataList(index).IsLocalSetup) Then
                    Try
                        CreateZip(XMLZip, DbData.Folder.ClientXMLPath)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                Else
                    Try
                        CreateZip(XMLZip, DbData.Folder.FinalXMLPath)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                End If
                GBL.DeantaBallon($"Final XML started {XMLZip}", MessageType.MSGINFO)
                If ((isNeedUpload) And (File.Exists(XMLZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Final XML Generation", .DBOrderID = 6, .DBTaskType = DigitalBundleTask.TFXML, .FinalFileName = Path.GetFileName(XMLZip), .FinalFilePath = XMLZip})
                    UploadIntoResourcePath(XMLZip, DigitalBundleTask.TFXML, index)
                Else
                    GBL.DeantaBallon("XML zip Not exists :" & XMLZip, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & "XML zip", MessageType.MSGERROR)
                Return False
            End Try

            XMLZip = String.Empty
            'If (DbData.DocType = DocumentType.RL) Then ' 05-09-2021

            GBL.DeantaBallon("ePub Final output:" & DbData.Folder.EPubPath, MessageType.MSGERROR)

            XMLZip = Path.Combine(DbData.Folder.WorkingPath, $"{DbData.ePubISBN}_ePub_New.zip")
            Try
                CreateZip(XMLZip, DbData.Folder.EPubPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
            Try
                If ((isNeedUpload) And (File.Exists(XMLZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "EPUB Generation", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.EPUB, .FinalFileName = Path.GetFileName(XMLZip), .FinalFilePath = XMLZip})
                    UploadIntoResourcePath(XMLZip, DigitalBundleTask.EPUB, index)
                Else
                    GBL.DeantaBallon("XML zip not exists :" & XMLZip, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        'End If '05-09-2021

        If (Stage = DBStage.WEBTRIGGER) Then

            If (DbData.IsBookPDFGenerated) Then
                Try
                    BookPDF = GetFileFromFolder(DbData.Folder.BookPDFPath, "*_print.pdf")
                    If ((isNeedUpload) And (File.Exists(BookPDF))) Then
                        'GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Print PDF generation", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.BOOKPDF, .FinalFileName = Path.GetFileName(BookPDF), .FinalFilePath = BookPDF})
                        UploadIntoResourcePath(BookPDF, DigitalBundleTask.BOOKPDF, index)
                    Else
                        GBL.DeantaBallon("Book PDF Not exists :" & BookPDF, MessageType.MSGERROR)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & " - book pdf", MessageType.MSGERROR)
                End Try
            End If

            Try
                Dim PODPS As String = String.Empty
                PODPS = GetFileFromFolder(DbData.Folder.PODPdfPath, "*.pdf")

                If (String.IsNullOrEmpty(PODPS)) Then
                    PODPS = GetFileFromFolder(DbData.Folder.PODPdfPath, "*.ps")
                End If

                GBL.DeantaBallon("PS file :" & PODPS, MessageType.MSGINFO)

                If ((isNeedUpload) And (File.Exists(PODPS))) Then

                    If (Not Path.GetFileNameWithoutExtension(PODPS).Contains("_LS")) Then
                        GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "LS (POD) generation", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.POD, .FinalFileName = Path.GetFileName(PODPS), .FinalFilePath = PODPS})
                        'File.Move(PODPS, PODPS.Replace("*.pdf", "_LS.pdf"))
                    End If
                    UploadIntoResourcePath(PODPS, DigitalBundleTask.POD, index)
                Else
                    GBL.DeantaBallon("PS file Not exists :" & PODPS, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & "", MessageType.MSGERROR)
            End Try

            Try
                Dim LSPDF As String = String.Empty
                LSPDF = GetFileFromFolder(DbData.Folder.LSPDFPath, "*.pdf")

                If (String.IsNullOrEmpty(LSPDF)) Then
                    LSPDF = GetFileFromFolder(DbData.Folder.LSPDFPath, "*.ps")
                End If

                If ((isNeedUpload) And (File.Exists(LSPDF))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "LS (POD) generation", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.POD, .FinalFileName = Path.GetFileName(LSPDF), .FinalFilePath = LSPDF})
                    UploadIntoResourcePath(LSPDF, DigitalBundleTask.POD, index)
                Else
                    GBL.DeantaBallon("LS PDF file Not exists :" & LSPDF, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try


            Try
                Dim RTFfile As String = String.Empty
                Dim TmpRtfFile As String = String.Empty
                GBL.DeantaBallon($"RTF path: {DbData.Folder.RTFPath}", MessageType.MSGINFO)
                RTFfile = GetFileFromFolder(DbData.Folder.RTFPath, "*.doc*")
                GBL.DeantaBallon($"RTF file path: {RTFfile}", MessageType.MSGINFO)
                If (String.IsNullOrEmpty(DbData.WebPDFISBN) Or String.IsNullOrEmpty(DbData.HardbackISBN)) Then
                    TmpRtfFile = RTFfile.Replace("_web_txt.doc", "_txt_txt.doc").Replace("_web_txt.docx", "_txt_txt.docx").Replace("_web_txt.doc", "_txt_txt.doc").Replace("_web_txt.docx", "_txt_txt.docx")
                Else
                    TmpRtfFile = RTFfile.Replace("_web_txt.doc", "_txt_txt.doc").Replace("_web_txt.docx", "_txt_txt.docx").Replace(DbData.WebPDFISBN, DbData.HardbackISBN).Replace("_web_txt.doc", "_txt_txt.doc").Replace("_web_txt.docx", "_txt_txt.docx")
                End If

                File.Move(RTFfile, TmpRtfFile)

                GBL.DeantaBallon($"RTF started {TmpRtfFile}", MessageType.MSGINFO)
                If ((isNeedUpload) And (File.Exists(TmpRtfFile))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Combined Doc", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.COMBINEDRTF, .FinalFileName = Path.GetFileName(TmpRtfFile), .FinalFilePath = TmpRtfFile})
                    UploadIntoResourcePath(TmpRtfFile, DigitalBundleTask.COMBINEDRTF, index)
                Else
                    GBL.DeantaBallon("RTF file Not exists :" & TmpRtfFile, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & " -  docx", MessageType.MSGERROR)
            End Try

            Try
                Dim AppfileZip As String = String.Empty
                If ((DbData.DocType = DocumentType.RL) Or (DbData.DocType = DocumentType.SEQUOIA)) Then
                    AppfileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_InDesign.zip", DbData.ApplicationISBN))
                ElseIf (DbData.OrgDocType = DocumentType.ANTHEM) Then
                    AppfileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_app.zip", DbData.HardbackISBN))
                Else
                    AppfileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_app.zip", DbData.ApplicationISBN))
                End If


                Try
                    CreateZip(AppfileZip, DbData.Folder.PackagePath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try

                GBL.DeantaBallon($"Application zip started {AppfileZip}", MessageType.MSGINFO)
                If ((isNeedUpload) And (File.Exists(AppfileZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Package Generation", .DBOrderID = 3, .DBTaskType = DigitalBundleTask.PACKAGING, .FinalFileName = Path.GetFileName(AppfileZip), .FinalFilePath = AppfileZip})
                    UploadIntoResourcePath(AppfileZip, DigitalBundleTask.PACKAGING, index)
                Else
                    GBL.DeantaBallon("File Not exists :" & AppfileZip, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & " - package", MessageType.MSGERROR)
            End Try


            If (DbData.OrgDocType = DocumentType.EDWARDELGAR) Then
                Try
                    Dim IndvPDFZip As String = String.Empty
                    IndvPDFZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_Individual_PDF.zip", DbData.ApplicationISBN))
                    Try
                        CreateZip(IndvPDFZip, DbData.Folder.FinalIndividualPDF)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try

                    GBL.DeantaBallon($"IndividualPDF zip started {IndvPDFZip}", MessageType.MSGINFO)
                    If ((isNeedUpload) And (File.Exists(IndvPDFZip))) Then
                        GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "IndividualPDF", .DBOrderID = 3, .DBTaskType = DigitalBundleTask.PACKAGING, .FinalFileName = Path.GetFileName(IndvPDFZip), .FinalFilePath = IndvPDFZip})
                        UploadIntoResourcePath(IndvPDFZip, DigitalBundleTask.PACKAGING, index)
                    Else
                        GBL.DeantaBallon("File Not exists :" & IndvPDFZip, MessageType.MSGERROR)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & " - IndividualPDF", MessageType.MSGERROR)
                End Try
            End If

            'If ((DbData.DocType <> DocumentType.RL) And (DbData.DocType <> DocumentType.SEQUOIA) And (DbData.OrgDocType <> DocumentType.ANTHEM)) Then
            If ((DbData.OrgDocType = DocumentType.BLOOMSBURY) Or (DbData.OrgDocType = DocumentType.TRD)) Then

                If (Not String.IsNullOrEmpty(DbData.PaperbackISBN)) Then

                    Try
                        BloomsburyPaparbackPackage(DbData.Folder.PackagePath, DbData.Folder.PBPackagePath, DbData.HardbackISBN, DbData.PaperbackISBN)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try

                    Try
                        Dim AppfileZip As String = String.Empty
                        AppfileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_app.zip", DbData.PaperbackISBN))

                        Try
                            CreateZip(AppfileZip, DbData.Folder.PBPackagePath)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try

                        GBL.DeantaBallon($"Application zip started {AppfileZip}", MessageType.MSGINFO)
                        If ((isNeedUpload) And (File.Exists(AppfileZip))) Then
                            GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Package Generation", .DBOrderID = 3, .DBTaskType = DigitalBundleTask.PACKAGING, .FinalFileName = Path.GetFileName(AppfileZip), .FinalFilePath = AppfileZip})
                            UploadIntoResourcePath(AppfileZip, DigitalBundleTask.PACKAGING, index)
                        Else
                            GBL.DeantaBallon("File Not exists :" & AppfileZip, MessageType.MSGERROR)
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message & " - package", MessageType.MSGERROR)
                    End Try

                End If
            End If


            Try
                WEBPDF = Path.Combine(DbData.Folder.DeliveryWEBPDF, DbData.Folder.WEBPDFName.Replace(".indb", ".pdf"))
                If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                    If (File.Exists(WEBPDF)) Then
                        'File.Copy(WEBPDF, Path.Combine(DbData.Folder.DeliveryWEBPDF, Path.GetFileName(WEBPDF)), True)
                        Try
                            'File.Copy(WEBPDF, WEBPDF.Replace("_web.pdf", "_preview.pdf"), True)
                            File.Copy(WEBPDF, (Path.Combine(Path.GetDirectoryName(WEBPDF), $"{DbData.HardbackISBN}_preview.pdf")), True)
                            If (DbData.OrgDocType = DocumentType.ANTHEM) Then
                                File.Move(WEBPDF, WEBPDF.Replace("_web.pdf", "_ebook.pdf"))
                                WEBPDF = WEBPDF.Replace("_web.pdf", "_ebook.pdf")
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                        Try
                            File.Copy((Path.Combine(Path.GetDirectoryName(WEBPDF), $"{DbData.HardbackISBN}_preview.pdf")), Path.Combine(DbData.Folder.DeliveryPreview, $"{DbData.HardbackISBN}_preview.pdf"), True)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                    End If
                End If

                If ((isNeedUpload) And (File.Exists(WEBPDF))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "WEB PDF Generation", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.WEBPDF, .FinalFileName = Path.GetFileName(WEBPDF), .FinalFilePath = WEBPDF})
                    UploadIntoResourcePath(WEBPDF, DigitalBundleTask.WEBPDF, index)
                Else
                    GBL.DeantaBallon("Web PDF Not exists :" & WEBPDF, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & " web pdf", MessageType.MSGERROR)
            End Try

            If (DbData.OrgDocType <> DocumentType.ANTHEM) Then

                If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                    Try
                        Dim PrintPDF As String = String.Empty
                        PrintPDF = GetFileFromFolder(DbData.Folder.DeliveryPreview, "*_preview.pdf")
                        If ((isNeedUpload) And (File.Exists(PrintPDF))) Then
                            GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Preview PDF Generation", .DBOrderID = 5, .DBTaskType = DigitalBundleTask.PREVIERPDF, .FinalFileName = Path.GetFileName(PrintPDF), .FinalFilePath = PrintPDF})
                            UploadIntoResourcePath(PrintPDF, DigitalBundleTask.PREVIERPDF, index)
                        Else
                            GBL.DeantaBallon("Preview pdf file Not exists :" & PrintPDF, MessageType.MSGERROR)
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message & " preview ", MessageType.MSGERROR)
                    End Try

                End If

            End If

            Dim ePubFile As String = GetFileFromFolder(DbData.Folder.DeliveryePub, "*.epub")
            If ((isNeedUpload) And (File.Exists(ePubFile))) Then
                GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "EPUB Generation", .DBOrderID = 7, .DBTaskType = DigitalBundleTask.EPUB, .FinalFileName = Path.GetFileName(ePubFile), .FinalFilePath = ePubFile})
                UploadIntoResourcePath(ePubFile, DigitalBundleTask.EPUB, index)
            Else
                GBL.DeantaBallon("epub file Not exists :" & ePubFile, MessageType.MSGERROR)
            End If

            Dim MobiFile As String = GetFileFromFolder(DbData.Folder.MOBIPath, "*.mobi")
            If ((isNeedUpload) And (File.Exists(MobiFile))) Then
                GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "MOBI Generation", .DBOrderID = 8, .DBTaskType = DigitalBundleTask.MOBI, .FinalFileName = Path.GetFileName(MobiFile), .FinalFilePath = MobiFile})
                UploadIntoResourcePath(MobiFile, DigitalBundleTask.MOBI, index)
            Else
                GBL.DeantaBallon("Mobi file Not exists :" & ePubFile, MessageType.MSGERROR)
            End If

            If ((DbData.DocType <> DocumentType.RL) And (DbData.DocType <> DocumentType.SEQUOIA) And (DbData.OrgDocType <> DocumentType.ANTHEM)) Then
                Try
                    Dim ImageFileZip As String = String.Empty
                    ImageFileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_images.zip", DbData.ApplicationISBN))
                    Dim ImagePath As String = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images_print")

                    If (Not Directory.Exists(ImagePath)) Then
                        ImagePath = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images")
                    End If

                    If ((Not String.IsNullOrEmpty(ImagePath) AndAlso (Directory.Exists(ImagePath)))) Then
                        If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                            Try
                                If (Not DbData.IsLocalSetup) Then
                                    File.Copy(DbData.CoverImageFullName, Path.Combine(ImagePath, Path.GetFileName(DbData.CoverImageFullName)), True)
                                End If
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                            Try
                                File.Copy(GBL.BloomsburyLogo, Path.Combine(ImagePath, Path.GetFileName(GBL.BloomsburyLogo)), True)
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message & " - client logo image", MessageType.MSGERROR)
                            End Try
                        End If
                        Try
                            CreateZip(ImageFileZip, ImagePath)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try

                        If ((isNeedUpload) And (File.Exists(ImageFileZip))) Then
                            GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Image processing", .DBOrderID = 9, .DBTaskType = DigitalBundleTask.IMAGES, .FinalFileName = Path.GetFileName(ImageFileZip), .FinalFilePath = ImageFileZip})
                            UploadIntoResourcePath(ImageFileZip, DigitalBundleTask.IMAGES, index)
                        Else
                            GBL.DeantaBallon("File Not exists :" & ImageFileZip, MessageType.MSGERROR)
                        End If
                    Else
                        GBL.DeantaBallon("Could Not able to find the image folder for final output zip.", MessageType.MSGERROR)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & " - image zip", MessageType.MSGERROR)
                End Try

                If (DbData.OrgDocType <> DocumentType.ANTHEM) Then
                    If (Not String.IsNullOrEmpty(DbData.PaperbackISBN)) Then
                        Try
                            Dim ImageFileZip As String = String.Empty
                            ImageFileZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_txt_images.zip", DbData.PaperbackISBN))
                            Dim ImagePath As String = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images_print")

                            If (Not Directory.Exists(ImagePath)) Then
                                ImagePath = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images")
                            End If

                            If ((Not String.IsNullOrEmpty(ImagePath) AndAlso (Directory.Exists(ImagePath)))) Then
                                If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                                    Try
                                        If (Not DbData.IsLocalSetup) Then
                                            File.Copy(DbData.CoverImageFullName, Path.Combine(ImagePath, Path.GetFileName(DbData.CoverImageFullName)), True)
                                        End If
                                    Catch ex As Exception
                                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                    End Try
                                    Try
                                        File.Copy(GBL.BloomsburyLogo, Path.Combine(ImagePath, Path.GetFileName(GBL.BloomsburyLogo)), True)
                                    Catch ex As Exception
                                        GBL.DeantaBallon(ex.Message & " - client logo image", MessageType.MSGERROR)
                                    End Try
                                End If
                                Try
                                    CreateZip(ImageFileZip, ImagePath)
                                Catch ex As Exception
                                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                    Return False
                                End Try

                                If ((isNeedUpload) And (File.Exists(ImageFileZip))) Then
                                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Image processing", .DBOrderID = 9, .DBTaskType = DigitalBundleTask.IMAGES, .FinalFileName = Path.GetFileName(ImageFileZip), .FinalFilePath = ImageFileZip})
                                    UploadIntoResourcePath(ImageFileZip, DigitalBundleTask.IMAGES, index)
                                Else
                                    GBL.DeantaBallon("File Not exists :" & ImageFileZip, MessageType.MSGERROR)
                                End If
                            Else
                                GBL.DeantaBallon("Could Not able to find the image folder for final output zip.", MessageType.MSGERROR)
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message & " - image zip", MessageType.MSGERROR)
                        End Try
                    End If
                End If

            End If


            If ((DbData.DocType = DocumentType.BLOOMSBURY) Or (DbData.DocType = DocumentType.TRD) Or (DbData.DocType = DocumentType.RL) Or (DbData.DocType = DocumentType.SEQUOIA) Or (DbData.OrgDocType = DocumentType.MUP)) Then
                Dim WebZip As String = Path.Combine(DbData.Folder.WorkingPath, "For_ReferenceOnly_WEBPDF.zip")
                GBL.DeantaBallon("WEB PDF zip :" & WebZip, MessageType.MSGINFO)

                Try
                    CreateZip(WebZip, DbData.Folder.WEBPDFPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try

                If ((isNeedUpload) And (File.Exists(WebZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "GS Preview", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.ZIP, .FinalFileName = Path.GetFileName(WebZip), .FinalFilePath = WebZip})
                    UploadIntoResourcePath(WebZip, DigitalBundleTask.ZIP, index)
                Else
                    GBL.DeantaBallon("WEB PDF zip :" & WebZip, MessageType.MSGERROR)
                End If

            End If

            If ((DbData.DocType = DocumentType.RL) Or (DbData.DocType = DocumentType.SEQUOIA)) Then
                Dim PrintPDF As String = String.Empty
                PrintPDF = GetFileFromFolder(DbData.Folder.DeliveryPreview, "*_Print.pdf")
                If ((isNeedUpload) And (File.Exists(PrintPDF))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Print PDF", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.PRINTPDF, .FinalFileName = Path.GetFileName(PrintPDF), .FinalFilePath = PrintPDF})
                    UploadIntoResourcePath(PrintPDF, DigitalBundleTask.PRINTPDF, index)
                Else
                    GBL.DeantaBallon("RL Print PDF :" & PrintPDF, MessageType.MSGERROR)
                End If

                Dim CopyRightPDF As String = String.Empty
                CopyRightPDF = GetFileFromFolder(DbData.Folder.DeliveryPreview, "*_copyright*.pdf")
                If ((isNeedUpload) And (File.Exists(CopyRightPDF))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Copyright PDF", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.PREVIERPDF, .FinalFileName = Path.GetFileName(CopyRightPDF), .FinalFilePath = CopyRightPDF})
                    UploadIntoResourcePath(CopyRightPDF, DigitalBundleTask.PREVIERPDF, index)
                Else
                    GBL.DeantaBallon("RL Copyright PDF :" & CopyRightPDF, MessageType.MSGERROR)
                End If
            End If

        End If
        Return True
    End Function

    Private Function BloomsburyPaparbackPackage(ByVal Hardbackpath As String, ByVal PaparPackPath As String, ByVal HardbackISBN As String, ByVal PaperbackISBN As String) As Boolean
        Try
            CopyDirectory(Hardbackpath, PaparPackPath)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & ex.StackTrace, MessageType.MSGERROR)
        End Try
        For Each Folder As String In Directory.GetDirectories(PaparPackPath, "*.*", SearchOption.AllDirectories)
            Directory.Move(Folder, Folder.Replace(HardbackISBN, PaperbackISBN))
        Next
        For Each fsFile As String In Directory.GetFiles(PaparPackPath, "*.*", SearchOption.AllDirectories)
            File.Move(fsFile, fsFile.Replace(HardbackISBN, PaperbackISBN))
        Next
        Return True
    End Function

    Private Function UpdateDigitialBundleAsset(ByVal index As Integer) As Boolean
        Dim FinalAssetTbl As New DataTable("final")
        Dim DBFtpPath As String = String.Empty
        'SELECT * FROM `tbl_digitalbundle_status
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        If (Not DbData.IsLocalSetup) Then
            Return True
        End If
        Try
            FinalAssetTbl = MySqlHelper.ReadSqlData("select * from tbl_digitalbundle_status where digitalbundle_id=" & DbData.DigitalID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        DBFtpPath = Path.Combine(Path.GetDirectoryName(DbData.XmlURL), "DB").Replace("\", "/")
        Dim StatusID As String = String.Empty
        Dim ProcessName As String = String.Empty
        Dim FinalAsset As FinalResourceData = Nothing

        If ((FinalAssetTbl IsNot Nothing) AndAlso (FinalAssetTbl.Rows IsNot Nothing) AndAlso (FinalAssetTbl.Rows.Count > 0)) Then
            For fl As Int32 = 0 To FinalAssetTbl.Rows.Count - 1
                StatusID = FinalAssetTbl.Rows(fl).Item("digitalbundle_status_id")
                ProcessName = FinalAssetTbl.Rows(fl).Item("process_name")
                FinalAsset = (From n In DbData.FinalAssets Where String.Compare(ProcessName, n.DBProcessName, True) = 0 Select n).FirstOrDefault
                If (FinalAsset IsNot Nothing) Then
                    Try
                        MySqlHelper.ExecuteNonQuery("update tbl_digitalbundle_status set filename='" & FinalAsset.FinalFileName & "',output_path='" & Path.Combine(DBFtpPath, FinalAsset.FinalFileName).Replace("\", "/") & "' where digitalbundle_id=" & DbData.DigitalID & " and digitalbundle_status_id=" & StatusID)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                End If
            Next
        End If
        Return True
    End Function

    Private Function UploadFinalAssetInDBFolder(ByVal index As Integer) As Boolean
        Dim DBFtpPath As String = String.Empty
        Dim StatusUI As UploadDownloadHelper
        Dim FinalAssets As New List(Of String)
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        If (Not DbData.IsLocalSetup) Then
            Return True
        End If
        DBFtpPath = Path.Combine(Path.GetDirectoryName(DbData.XmlURL), "DB").Replace("\", "/")
        FinalAssets = (From n In GBL.DBDataList(index).FinalAssets Select n.FinalFilePath).ToList
        'Dim FinalAsset As FinalResourceData = Nothing
        'For fl As Int32 = 0 To DbData.FinalAsset.Count - 1
        '    FinalAsset = DbData.FinalAsset(fl)
        '    If (FinalAsset IsNot Nothing) Then
        '        Try
        '            Try
        '                StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = FinalAsset.FinalFilePath, .UrlPath = DBFtpPath})
        '                StatusUI.ShowDialog()
        '            Catch ex As Exception
        '                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '            End Try
        '        Catch ex As Exception
        '            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '            Continue For
        '        End Try
        '    End If
        'Next
        GBL.DeantaBallon($"Upload final files {String.Join(",", FinalAssets.ToArray())}", MessageType.MSGERROR)
        Try
            StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOADALLFILES, .LocalPath = DbData.WorkPath, .UrlPath = DBFtpPath, .NeededFileList = FinalAssets})
            AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
            StatusUI.DoUploadDownload()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Return True
    End Function

    Private Function ConvertHtmltoDocx(ByVal InputRtf As String, ByVal OutputDocx As String) As Boolean
        Try
            Using wordDocument As WordprocessingDocument = WordprocessingDocument.Create(OutputDocx, WordprocessingDocumentType.Document)
                Dim b As Body
                Dim d As Document
                Dim altChunkId As String = "AltChunkId1"
                Dim mainDocPart As MainDocumentPart = wordDocument.MainDocumentPart

                If mainDocPart Is Nothing Then
                    mainDocPart = wordDocument.AddMainDocumentPart()
                    b = New Body()
                    d = New Document(b)
                    d.Save(mainDocPart)
                End If

                Dim chunk As AlternativeFormatImportPart = mainDocPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Rtf, altChunkId)
                Dim rtfDocumentContent As String = File.ReadAllText(InputRtf, Encoding.ASCII)

                Using ms As MemoryStream = New MemoryStream(Encoding.ASCII.GetBytes(rtfDocumentContent))
                    chunk.FeedData(ms)
                End Using

                Dim altChunk As AltChunk = New AltChunk()
                altChunk.Id = altChunkId
                mainDocPart.Document.Body.InsertAt(altChunk, 0)
                mainDocPart.Document.Save()
            End Using
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function GetFileFromFolder(ByVal Folder As String, ByVal Pattern As String) As String
        Dim FileList As New List(Of String)
        If (Not Directory.Exists(Folder)) Then
            Return String.Empty
        End If
        FileList.AddRange(Directory.GetFiles(Folder, Pattern, SearchOption.TopDirectoryOnly))
        If ((FileList IsNot Nothing) AndAlso (FileList.Count > 0)) Then
            Return FileList(0)
        End If
        Return String.Empty
    End Function

    Private Function old_UpdateResourcePath(ByVal OutputFile As String, ByVal TaskName As DigitalBundleTask, ByVal index As Int32) As Boolean
        Dim ResourceID As String = String.Empty
        Dim ResourceFile As String = String.Empty
        ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
        ResourceFile = Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile)))
        Try
            LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = ResourceFile, .UrlPath = OutputFile})
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = OutputFile, .ResourceID = String.Format("{0}" & Path.GetExtension(OutputFile), ResourceID), .TaskName = TaskName})
        GBL.DBDataList(index).ResourceAssets.Add(New FinalResourceData With {.FinalFilePath = ResourceFile, .FinalFileName = String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile)), .DBTaskType = TaskName})
        Return False
    End Function

    Private Function UploadIntoResourcePath(ByVal OutputFile As String, ByVal TaskName As DigitalBundleTask, ByVal index As Int32) As Boolean
        Dim Stdw As New Stopwatch
        Dim StatusUI As UploadDownloadHelper
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)

        If (DbData.IsLocalSetup) Then
            '''Threading.Thread.Sleep(20000)
        End If

        Dim ResourceID As String = String.Empty
        ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
        Stdw.Start()
        GBL.DeantaBallon(TaskName.ToString() & " Started - " & Stdw.Elapsed.Seconds, MessageType.MSGINFO)

        'If (Not DbData.IsLocalSetup) Then
        Try
            LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))), .UrlPath = OutputFile})
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        'End If

        GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = OutputFile, .ResourceID = String.Format("{0}" & Path.GetExtension(OutputFile), ResourceID), .TaskName = TaskName})
        If (TaskName = DigitalBundleTask.BOOKPDF) Then
            GBL.DeantaBallon("File - from : " & Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))) & " -To: " & GBL.FTPBookPDFPath, MessageType.MSGERROR)
            If (Not DbData.IsLocalSetup) Then
                Try
                    'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))), .UrlPath = GBL.FTPBookPDFPath})
                    'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    'StatusUI.DoUploadDownload()
                    AzureHelper.UploadFile(Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))), LanstadPathOptions.ASSETPATH, String.Empty)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            End If
        Else
            Dim StartTime As String = DateTime.Now.ToString()
            GBL.DeantaBallon("File - from : " & Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
            Try
                'StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))), .UrlPath = GBL.FTPResourcePath})
                'AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                'StatusUI.DoUploadDownload()
                AzureHelper.UploadFile(Path.Combine(Path.GetDirectoryName(OutputFile), String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile))), LanstadPathOptions.ASSETPATH, String.Empty)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            Dim EndTime As String = DateTime.Now.ToString()
            GBL.DbUploadLog(Path.GetFileName(OutputFile), (((New FileInfo(OutputFile).Length) / 1024) / 1024).ToString(), StartTime, EndTime, DateDiff(DateInterval.Second, Date.Parse(StartTime), Date.Parse(EndTime)).ToString())
            If (DbData.IsLocalSetup) Then
                Try
                    UpdateNextTaskInDB(OutputFile, TaskName, index, String.Format("{0}{1}", ResourceID, Path.GetExtension(OutputFile)))
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            End If
        End If
        Stdw.Stop()
        GBL.DeantaBallon(TaskName.ToString() & " Completed - " & Stdw.Elapsed.Seconds, MessageType.MSGINFO)
        If (DbData.IsLocalSetup) Then
            ''Threading.Thread.Sleep(9000)
        End If

        Return True
    End Function

    Private Function UpdateNextTaskInDB(ByVal OutputFile As String, ByVal TaskName As DigitalBundleTask, ByVal index As Int32, ByVal ResouceName As String) As Boolean
        Dim FinalAsset As FinalResourceData = Nothing
        Dim TblDigitTask As New DataTable("dts")
        Dim CurOrderID As Integer = 0
        Dim NextOrderID As Integer = 0
        Dim StatusID As String = String.Empty
        Dim StatusUI As UploadDownloadHelper
        Dim DBFtpPath As String = String.Empty
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        DBFtpPath = Path.Combine(Path.GetDirectoryName(DbData.XmlURL), "DB").Replace("\", "/")
        Dim ProcessName As String = String.Empty
        Dim FinalAssetTbl As New DataTable("DDF")
        FinalAsset = (From n In DbData.FinalAssets Where n.DBTaskType = TaskName Select n).FirstOrDefault
        If (FinalAsset IsNot Nothing) Then
            Try
                TblDigitTask = MySqlHelper.ReadSqlData("select * from tbl_digitalbundle_status where digitalbundle_id=" & DbData.DigitalID & " and process_name='" & FinalAsset.DBProcessName & "'")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try

            If ((TblDigitTask Is Nothing) OrElse (TblDigitTask.Rows Is Nothing) OrElse (TblDigitTask.Rows.Count = 0)) Then
                Return False
            End If

            StatusID = TblDigitTask.Rows(0).Item("digitalbundle_status_id")
            CurOrderID = TblDigitTask.Rows(0).Item("order_id")
            NextOrderID = Convert.ToInt32(CurOrderID) + 1

            'If (DbData.IsLocalSetup) Then
            '    Try
            '        StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = OutputFile, .UrlPath = DBFtpPath})
            '        StatusUI.ShowDialog()
            '    Catch ex As Exception
            '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            '    End Try
            'End If

            Try
                MySqlHelper.ExecuteNonQuery("update tbl_digitalbundle_status set tool_status_id=1,filename='" & FinalAsset.FinalFileName & "',output_path='resources/" & ResouceName.Replace("\", "/") & "' where digitalbundle_id=" & DbData.DigitalID & " and digitalbundle_status_id=" & StatusID)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If
        Return True
    End Function

    Private Function UploadRequiredFilesForTANDF(ByVal index As Integer, ByVal isNeedUpload As Boolean, Optional ByVal Stage As DBStage = DBStage.NONE) As Boolean
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim BookPDF As String = String.Empty
        Dim LXEBookPDF As String = String.Empty
        Dim WEBPDF As String = String.Empty
        Dim ResourceID As String = String.Empty
        GBL.DBDataList(index).FinalAssets.Clear()
        GBL.OutputDataList.Clear()

        If (DbData.IsBookPDFGenerated) Then
            Try
                BookPDF = GetFileFromFolder(DbData.Folder.BookPDFPath, "*_print.pdf")
                If ((isNeedUpload) And (File.Exists(BookPDF))) Then
                    UploadIntoResourcePath(BookPDF, DigitalBundleTask.BOOKPDF, index)
                Else
                    GBL.DeantaBallon("Book PDF not exists :" & BookPDF, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message & " - book pdf", MessageType.MSGERROR)
                Return False
            End Try
        End If
        Dim XMLZip As String = String.Empty

        '22-01-2022
        If (DbData.IsLocalSetup) Then

            XMLZip = Path.Combine(DbData.Folder.WorkingPath, "9781315393506_xml.zip")
            GBL.DeantaBallon("Client XML Final:" & DbData.Folder.FinalXMLPath, MessageType.MSGERROR)
            Try
                CreateZip(XMLZip, DbData.Folder.ClientXMLPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
            Try
                If ((isNeedUpload) And (File.Exists(XMLZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Final XML Generation", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.TFXML, .FinalFileName = Path.GetFileName(XMLZip), .FinalFilePath = XMLZip})
                    UploadIntoResourcePath(XMLZip, DigitalBundleTask.TFXML, index)
                Else
                    GBL.DeantaBallon("XML zip not exists :" & XMLZip, MessageType.MSGERROR)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            Dim ArtworkZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_artwork.zip", DbData.ApplicationISBN))
            GBL.DeantaBallon("Artwork zip :" & ArtworkZip, MessageType.MSGINFO)
            Dim ArtPath As String = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images")
            GBL.DeantaBallon("Artwork Path :" & ArtPath, MessageType.MSGINFO)
            If (Directory.Exists(ArtPath)) Then
                Try
                    CreateZip(ArtworkZip, ArtPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try
                If ((isNeedUpload) And (File.Exists(ArtworkZip))) Then
                    'If (Directory.Exists(DbData.Folder.ApplicationPath)) Then
                    '    File.Move(ArtworkZip, Path.Combine(DbData.Folder.ApplicationPath, Path.GetFileName(ArtworkZip)))
                    'End If
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Image Package", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.IMAGES, .FinalFileName = Path.GetFileName(ArtworkZip), .FinalFilePath = ArtworkZip})
                    UploadIntoResourcePath(ArtworkZip, DigitalBundleTask.IMAGES, index)
                Else
                    GBL.DeantaBallon("XML zip not exists :" & ArtworkZip, MessageType.MSGERROR)
                End If
            End If

            Dim ApplicationZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_application_files.zip", DbData.ApplicationISBN))
            GBL.DeantaBallon("Application zip :" & ApplicationZip, MessageType.MSGINFO)
            Try
                CreateZip(ApplicationZip, DbData.Folder.ApplicationPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try

            If ((isNeedUpload) And (File.Exists(ApplicationZip))) Then
                GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Application Files Package", .DBOrderID = 3, .DBTaskType = DigitalBundleTask.APPLICATION, .FinalFileName = Path.GetFileName(ApplicationZip), .FinalFilePath = ApplicationZip})
                UploadIntoResourcePath(ApplicationZip, DigitalBundleTask.APPLICATION, index)
            Else
                GBL.DeantaBallon("Application zip not exists :" & ApplicationZip, MessageType.MSGERROR)
            End If

            GBL.DeantaBallon("ePub Final output:" & DbData.Folder.EPubPath, MessageType.MSGERROR)

            Dim eBookZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_eBooks.zip", DbData.ApplicationISBN))
            GBL.DeantaBallon("eBooks zip :" & eBookZip, MessageType.MSGINFO)

            Try
                CreateZip(eBookZip, DbData.Folder.DeliveryPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try

            If ((isNeedUpload) And (File.Exists(eBookZip))) Then
                GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "eBook Generation", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.EPUB, .FinalFileName = Path.GetFileName(eBookZip), .FinalFilePath = eBookZip})
                UploadIntoResourcePath(eBookZip, DigitalBundleTask.EPUB, index)
            Else
                GBL.DeantaBallon("XML zip not exists :" & eBookZip, MessageType.MSGERROR)
            End If

        End If


        XMLZip = String.Empty

        If (Not DbData.IsLocalSetup) Then

            If (Stage = DBStage.XMLTRIGGER) Then

                If (DbData.IsXMLGenerated) Then
                    GBL.DeantaBallon("XML Final output:" & DbData.Folder.FinalXMLPath, MessageType.MSGERROR)
                    If ((String.Compare(DbData.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(DbData.TaskName, "first pages typesetting", True) = 0)) Then
                        XMLZip = Path.Combine(DbData.Folder.WorkingPath, $"{DbData.ApplicationISBN}_xml_FP.zip")
                    Else
                        XMLZip = Path.Combine(DbData.Folder.WorkingPath, $"{DbData.ApplicationISBN}_xml.zip")
                    End If
                    Try
                        CreateZip(XMLZip, DbData.Folder.FinalXMLPath)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                    Try
                        If ((isNeedUpload) And (File.Exists(XMLZip))) Then
                            GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Final XML Generation", .DBOrderID = 1, .DBTaskType = DigitalBundleTask.TFXML, .FinalFileName = Path.GetFileName(XMLZip), .FinalFilePath = XMLZip})
                            UploadIntoResourcePath(XMLZip, DigitalBundleTask.TFXML, index)
                        Else
                            GBL.DeantaBallon("XML zip not exists :" & XMLZip, MessageType.MSGERROR)
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try

                    If ((DbData.IsWEBPDFGeneratd) Or (String.Compare(DbData.TaskName, "export xml to eproduct team", True) = 0)) Then
                        GBL.DeantaBallon("ePub Final output:" & DbData.Folder.EPubPath, MessageType.MSGERROR)

                        XMLZip = Path.Combine(DbData.Folder.WorkingPath, $"{DbData.ePubISBN}_ePub.zip")
                        Try
                            CreateZip(XMLZip, DbData.Folder.EPubPath)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try
                        Try
                            If ((isNeedUpload) And (File.Exists(XMLZip))) Then
                                GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "EPUB Generation", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.EPUB, .FinalFileName = Path.GetFileName(XMLZip), .FinalFilePath = XMLZip})
                                UploadIntoResourcePath(XMLZip, DigitalBundleTask.EPUB, index)
                            Else
                                GBL.DeantaBallon("XML zip not exists :" & XMLZip, MessageType.MSGERROR)
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                    End If
                End If
            End If

            If (Stage = DBStage.WEBTRIGGER) Then
                If (Directory.Exists(Path.Combine(DbData.Folder.WEBPDFPath, "images"))) Then
                    Dim IsEquationExists As Boolean = False
                    Dim MathMLFolder As String = Path.Combine(DbData.Folder.WorkingPath, "MathMLEqu")
                    Dim MathTypeZip As String = String.Empty
                    GBL.DeantaBallon("MathType output:" & Path.Combine(DbData.Folder.WEBPDFPath, "images"), MessageType.MSGERROR)

                    If (Directory.Exists(MathMLFolder)) Then
                        Dim dd As New DirectoryInfo(MathMLFolder)
                        dd.Delete(True)
                    End If
                    Directory.CreateDirectory(MathMLFolder)
                    For Each MathFile As String In Directory.GetFiles(Path.Combine(DbData.Folder.WEBPDFPath, "images"), "*_eqn*.eps", SearchOption.TopDirectoryOnly)
                        File.Copy(MathFile, Path.Combine(MathMLFolder, Path.GetFileName(MathFile)), True)
                        IsEquationExists = True
                    Next

                    If (IsEquationExists) Then
                        MathTypeZip = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_MathType.zip", DbData.ApplicationISBN))
                        GBL.DeantaBallon("Math Equation Path :" & MathMLFolder, MessageType.MSGINFO)
                        Try
                            CreateZip(MathTypeZip, MathMLFolder)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try
                    End If
                    If ((isNeedUpload) And (File.Exists(MathTypeZip))) Then
                        GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Math Package", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.MATHTYPE, .FinalFileName = Path.GetFileName(MathTypeZip), .FinalFilePath = MathTypeZip})
                        UploadIntoResourcePath(MathTypeZip, DigitalBundleTask.IMAGES, index)
                    Else
                        GBL.DeantaBallon("MathType zip not exists :" & XMLZip, MessageType.MSGERROR)
                    End If
                End If

                Dim ArtworkZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_artwork.zip", DbData.ApplicationISBN))
                GBL.DeantaBallon("Artwork zip :" & ArtworkZip, MessageType.MSGINFO)
                Dim ArtPath As String = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images")
                GBL.DeantaBallon("Artwork Path :" & ArtPath, MessageType.MSGINFO)
                If (Directory.Exists(ArtPath)) Then
                    Try
                        CreateZip(ArtworkZip, ArtPath)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                    If ((isNeedUpload) And (File.Exists(ArtworkZip))) Then
                        If (Directory.Exists(DbData.Folder.ApplicationPath)) Then
                            File.Move(ArtworkZip, Path.Combine(DbData.Folder.ApplicationPath, Path.GetFileName(ArtworkZip)))
                        End If
                        'GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Image Package", .DBOrderID = 2, .DBTaskType = DigitalBundleTask.IMAGES, .FinalFileName = Path.GetFileName(ArtworkZip), .FinalFilePath = ArtworkZip})
                        'UploadIntoResourcePath(ArtworkZip, DigitalBundleTask.IMAGES, index)
                    Else
                        GBL.DeantaBallon("XML zip not exists :" & ArtworkZip, MessageType.MSGERROR)
                    End If
                End If

                Dim ApplicationZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_application_files.zip", DbData.ApplicationISBN))
                GBL.DeantaBallon("Application zip :" & ApplicationZip, MessageType.MSGINFO)
                Try
                    CreateZip(ApplicationZip, DbData.Folder.ApplicationPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try

                If ((isNeedUpload) And (File.Exists(ApplicationZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "Application Files Package", .DBOrderID = 3, .DBTaskType = DigitalBundleTask.APPLICATION, .FinalFileName = Path.GetFileName(ApplicationZip), .FinalFilePath = ApplicationZip})
                    UploadIntoResourcePath(ApplicationZip, DigitalBundleTask.APPLICATION, index)
                Else
                    GBL.DeantaBallon("Application zip not exists :" & ApplicationZip, MessageType.MSGERROR)
                End If


                Dim eBookZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_eBooks.zip", DbData.ApplicationISBN))
                GBL.DeantaBallon("eBooks zip :" & eBookZip, MessageType.MSGINFO)

                Try
                    CreateZip(eBookZip, DbData.Folder.DeliveryPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try

                If ((isNeedUpload) And (File.Exists(eBookZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "eBook Generation", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.EPUB, .FinalFileName = Path.GetFileName(eBookZip), .FinalFilePath = eBookZip})
                    UploadIntoResourcePath(eBookZip, DigitalBundleTask.EPUB, index)
                Else
                    GBL.DeantaBallon("XML zip not exists :" & eBookZip, MessageType.MSGERROR)
                End If

                Dim WebZip As String = Path.Combine(DbData.Folder.WorkingPath, "For_ReferenceOnly_WEBPDF.zip")
                GBL.DeantaBallon("WEB PDF zip :" & WebZip, MessageType.MSGINFO)

                Try
                    CreateZip(WebZip, DbData.Folder.WEBPDFPath)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try

                If ((isNeedUpload) And (File.Exists(WebZip))) Then
                    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "GS Preview", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.ZIP, .FinalFileName = Path.GetFileName(WebZip), .FinalFilePath = WebZip})
                    UploadIntoResourcePath(WebZip, DigitalBundleTask.ZIP, index)
                Else
                    GBL.DeantaBallon("WEB PDF zip :" & WebZip, MessageType.MSGERROR)
                End If

                'Dim GsZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_GSPreview.zip", DbData.ApplicationISBN))
                'GBL.DeantaBallon("GS Preview zip :" & GsZip, MessageType.MSGINFO)

                'Try
                '    CreateZip(GsZip, DbData.Folder.GSPreviewInDesign)
                'Catch ex As Exception
                '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                'End Try

                'If ((isNeedUpload) And (File.Exists(GsZip))) Then
                '    GBL.DBDataList(index).FinalAssets.Add(New FinalResourceData With {.DBProcessName = "GS Preview", .DBOrderID = 4, .DBTaskType = DigitalBundleTask.ZIP, .FinalFileName = Path.GetFileName(GsZip), .FinalFilePath = GsZip})
                '    UploadIntoResourcePath(GsZip, DigitalBundleTask.ZIP, index)
                'Else
                '    GBL.DeantaBallon("GS Preview zip :" & GsZip, MessageType.MSGERROR)
                'End If

            End If

        End If

        Return True
    End Function

    Private Function old_dddUploadRequiredFilesForTANDF(ByVal index As Integer) As Boolean
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)
        Dim BookPDF As String = String.Empty
        Dim LXEBookPDF As String = String.Empty
        Dim WEBPDF As String = String.Empty
        Dim ResourceID As String = String.Empty
        GBL.OutputDataList.Clear()
        Dim StatusUI As UploadDownloadHelper
        If (DbData.IsBookPDFGenerated) Then
            BookPDF = Path.Combine(DbData.Folder.BookPDFPath, DbData.Folder.WEBPDFName.Replace(".indb", "_print.pdf"))
            If (File.Exists(BookPDF)) Then
                File.Move(BookPDF, BookPDF.Replace("_webpdf", "_bookpdf"))
                BookPDF = BookPDF.Replace("_webpdf", "_bookpdf")
            End If

            WEBPDF = Path.Combine(DbData.Folder.WEBPDFPath, DbData.Folder.WEBPDFName.Replace(".indb", ".pdf"))

            If (File.Exists(WEBPDF)) Then
                File.Copy(WEBPDF, Path.Combine(DbData.Folder.DeliveryWEBPDF, Path.GetFileName(WEBPDF)), True)
            End If

            LXEBookPDF = Path.Combine(Path.GetDirectoryName(DbData.XmlURL), Path.GetFileName(BookPDF)).Replace("\", "/")

            GBL.DeantaBallon("Lxe BookPDF : " & LXEBookPDF, MessageType.MSGINFO)

            If (File.Exists(BookPDF)) Then

                Try
                    MySqlHelper.ExecuteNonQuery("Update tbl_digitalbundle_pdf set bookpdfpath= '" & LXEBookPDF.Replace("\", "/") & "' where digitalbundle_id= " & DbData.DigitalID)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")

                Try
                    LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(BookPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(BookPDF))), .UrlPath = BookPDF})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.DeantaBallon("BookPDF - from : " & Path.Combine(Path.GetDirectoryName(BookPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(BookPDF))) & " -To: " & GBL.FTPBookPDFPath, MessageType.MSGERROR)

                GBL.OutputDataList.Add(New DigitalOutputData With {.TaskName = DigitalBundleTask.BOOKPDF, .OutputFile = BookPDF, .ResourceID = String.Format("{0}.pdf", ResourceID)})

                Try
                    StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(BookPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(BookPDF))), .UrlPath = GBL.FTPBookPDFPath, .Index = index})
                    AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    StatusUI.DoUploadDownload()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                'Try
                '    StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(BookPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(BookPDF))), .UrlPath = GBL.FTPResourcePath, .Index = index})
                '    StatusUI.ShowDialog()
                'Catch ex As Exception
                '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                'End Try
            Else
                GBL.DeantaBallon("File not exists :" & BookPDF, MessageType.MSGERROR)
            End If

        End If

        'If (DbData.IsWEBPDFGeneratd) Then

        '    If (File.Exists(WEBPDF)) Then
        '        ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
        '        Try
        '            LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(WEBPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(WEBPDF))), .UrlPath = WEBPDF})
        '        Catch ex As Exception
        '            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '        End Try
        '        GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = WEBPDF, .ResourceID = String.Format("{0}.pdf", ResourceID)})

        '        GBL.DeantaBallon("WebPDF - from : " & Path.Combine(Path.GetDirectoryName(WEBPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(WEBPDF))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)

        '        Try
        '            StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(WEBPDF), String.Format("{0}{1}", ResourceID, Path.GetExtension(WEBPDF))), .UrlPath = GBL.FTPResourcePath, .Index = index})
        '            StatusUI.ShowDialog()
        '        Catch ex As Exception
        '            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '        End Try
        '    Else
        '        GBL.DeantaBallon("File not exists :" & WEBPDF, MessageType.MSGERROR)
        '    End If
        'End If


        If (DbData.IsXMLGenerated) Then

            'Dim XMLZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_xml.zip", DbData.ISBN))
            'GBL.DeantaBallon("Zip  :" & WEBPDF, MessageType.MSGINFO)
            'Try
            '    CreateZip(XMLZip, DbData.Folder.ExportXMLPath)
            'Catch ex As Exception
            '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            'End Try

            'If (File.Exists(XMLZip)) Then
            '    ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
            '    Try
            '        LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))), .UrlPath = XMLZip})
            '    Catch ex As Exception
            '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            '    End Try

            '    GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = XMLZip, .ResourceID = String.Format("{0}.zip", ResourceID)})
            '    GBL.DeantaBallon("Zip - from : " & Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
            '    Try
            '        StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))), .UrlPath = GBL.FTPResourcePath, .Index = index})
            '        StatusUI.ShowDialog()
            '    Catch ex As Exception
            '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            '    End Try
            'Else
            '    GBL.DeantaBallon("File not exists :" & XMLZip, MessageType.MSGERROR)
            'End If

            Dim XMLZip As String = String.Empty
            XMLZip = Path.Combine(DbData.Folder.WorkingPath, "9781315393506_xml.zip")
            Try
                CreateZip(XMLZip, DbData.Folder.ClientXMLPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            If (File.Exists(XMLZip)) Then
                ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
                Try
                    LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))), .UrlPath = XMLZip})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = XMLZip, .ResourceID = String.Format("{0}.zip", ResourceID)})
                GBL.DeantaBallon("Zip - from : " & Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
                Try
                    StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(XMLZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(XMLZip))), .UrlPath = GBL.FTPResourcePath, .Index = index})
                    AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    StatusUI.DoUploadDownload()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Else
                GBL.DeantaBallon("File not exists :" & XMLZip, MessageType.MSGERROR)
            End If

            Dim ArtworkZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_artwork.zip", DbData.ApplicationISBN))
            GBL.DeantaBallon("Artwork zip :" & ArtworkZip, MessageType.MSGINFO)

            Dim ArtPath As String = Path.Combine(Path.Combine(DbData.Folder.WorkingPath, "WEBPDF"), "images")

            GBL.DeantaBallon("Artwork Path :" & ArtPath, MessageType.MSGINFO)

            Try
                CreateZip(ArtworkZip, ArtPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            If (File.Exists(ArtworkZip)) Then
                ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
                Try
                    LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(ArtworkZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ArtworkZip))), .UrlPath = ArtworkZip})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = ArtworkZip, .ResourceID = String.Format("{0}.zip", ResourceID)})
                GBL.DeantaBallon("Artwork - from : " & Path.Combine(Path.GetDirectoryName(ArtworkZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ArtworkZip))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
                Try
                    StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(ArtworkZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ArtworkZip))), .UrlPath = GBL.FTPResourcePath, .Index = index})
                    AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    StatusUI.DoUploadDownload()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Else
                GBL.DeantaBallon("File not exists :" & ArtworkZip, MessageType.MSGERROR)
            End If

            Dim ApplicationZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_application_files.zip", DbData.ApplicationISBN))

            GBL.DeantaBallon("Application zip :" & ApplicationZip, MessageType.MSGINFO)

            Try
                CreateZip(ApplicationZip, DbData.Folder.ApplicationPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            If (File.Exists(ApplicationZip)) Then
                ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
                Try
                    LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(ApplicationZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ApplicationZip))), .UrlPath = ApplicationZip})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = ApplicationZip, .ResourceID = String.Format("{0}.zip", ResourceID)})
                GBL.DeantaBallon("Application - from : " & Path.Combine(Path.GetDirectoryName(ApplicationZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ApplicationZip))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
                Try
                    StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(ApplicationZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(ApplicationZip))), .UrlPath = GBL.FTPResourcePath, .Index = index})
                    AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    StatusUI.DoUploadDownload()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Else
                GBL.DeantaBallon("File not exists :" & ApplicationZip, MessageType.MSGERROR)
            End If


            Dim eBookZip As String = Path.Combine(DbData.Folder.WorkingPath, String.Format("{0}_eBooks.zip", DbData.ApplicationISBN))
            GBL.DeantaBallon("eBooks zip :" & eBookZip, MessageType.MSGINFO)
            Try
                CreateZip(eBookZip, DbData.Folder.DeliveryPath)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            If (File.Exists(eBookZip)) Then
                ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
                Try
                    LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(eBookZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(eBookZip))), .UrlPath = eBookZip})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try

                GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = eBookZip, .ResourceID = String.Format("{0}.zip", ResourceID)})
                GBL.DeantaBallon("eBooks - from : " & Path.Combine(Path.GetDirectoryName(eBookZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(eBookZip))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
                Try
                    StatusUI = New UploadDownloadHelper(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(eBookZip), String.Format("{0}{1}", ResourceID, Path.GetExtension(eBookZip))), .UrlPath = GBL.FTPResourcePath, .Index = index})
                    AddHandler StatusUI.ProgressChanged, AddressOf ProgressChanged
                    StatusUI.DoUploadDownload()
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Else
                GBL.DeantaBallon("File not exists :" & eBookZip, MessageType.MSGERROR)
            End If

            'GBL.DeantaBallon("Client XML upload :" & DbData.ClientOutXML, MessageType.MSGINFO)

            'If (File.Exists(DbData.ClientOutXML)) Then
            '    ResourceID = DateTime.Now.ToString("yyyyMMddHHmmss")
            '    Try
            '        LocalDownloadFile(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(DbData.ClientOutXML), String.Format("{0}{1}", ResourceID, Path.GetExtension(DbData.ClientOutXML))), .UrlPath = DbData.ClientOutXML})
            '    Catch ex As Exception
            '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            '    End Try

            '    GBL.OutputDataList.Add(New DigitalOutputData With {.OutputFile = DbData.ClientOutXML, .ResourceID = String.Format("{0}.xml", ResourceID)})
            '    GBL.DeantaBallon("client xml - from : " & Path.Combine(Path.GetDirectoryName(DbData.ClientOutXML), String.Format("{0}{1}", ResourceID, Path.GetExtension(DbData.ClientOutXML))) & " -To: " & GBL.FTPResourcePath, MessageType.MSGERROR)
            '    Try
            '        StatusUI = New FtpStatusUI(New DownloadUploadArgs With {.FtpStatus = FtpStatusType.UPLOAD, .LocalPath = Path.Combine(Path.GetDirectoryName(DbData.ClientOutXML), String.Format("{0}{1}", ResourceID, Path.GetExtension(DbData.ClientOutXML))), .UrlPath = GBL.FTPResourcePath, .Index = index})
            '        StatusUI.ShowDialog()
            '    Catch ex As Exception
            '        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            '    End Try
            'Else
            '    GBL.DeantaBallon("File not exists :" & DbData.ClientOutXML, MessageType.MSGERROR)
            'End If

        End If
        Return True
    End Function

    Private Function GetLocalPathFile(ByVal Task As DigitalBundleData) As List(Of LocalResourceData)
        Dim CurrentLocalResourceList As List(Of LocalResourceData) = Nothing
        If (Task.Stage = DBStage.NONE) Then
            If ((GBL.LocalCopyList IsNot Nothing) AndAlso (GBL.LocalCopyList.Count > 0)) Then
                CurrentLocalResourceList = (From n In GBL.LocalCopyList Where n.docType = Task.DocType And n.ProjectID = Task.ProjectID Select n).ToList
            End If
        ElseIf (Task.Stage = DBStage.FRISTPROFFXMLTRIGGER) Then
            If ((GBL.LocalCopyList IsNot Nothing) AndAlso (GBL.LocalCopyList.Count > 0)) Then
                CurrentLocalResourceList = (From n In GBL.LocalCopyList Where n.docType = Task.DocType And n.TaskType = DigitalBundleTask.TFXML And n.ProjectID = Task.ProjectID Select n).ToList
                Threading.Thread.Sleep(50000)
            End If
        End If
        Return CurrentLocalResourceList
    End Function

    Private Sub LocalDownloadFile(FtpArgs As DownloadUploadArgs)
        Dim brReader As System.IO.BinaryReader
        Dim brWriter As System.IO.BinaryWriter
        Dim input As New FileStream(FtpArgs.UrlPath, FileMode.Open, FileAccess.Read)
        Dim output As New FileStream(Path.Combine(FtpArgs.LocalPath), FileMode.Create, FileAccess.Write)
        brReader = New System.IO.BinaryReader(input)
        brWriter = New System.IO.BinaryWriter(output)
        Dim FileLen As Long = My.Computer.FileSystem.GetFileInfo(FtpArgs.UrlPath).Length
        Dim count As Integer = 100 * 1048576 ' this is buffer size
        Dim buffer(count) As Byte
        Dim bytesRead As Integer
        While FileLen >= 0
            bytesRead = brReader.Read(buffer, 0, count)
            If bytesRead = 0 Then ' 0 means end of file reached
                Exit While
            End If
            brWriter.Write(buffer, 0, bytesRead)
            FileLen = FileLen - bytesRead
        End While
        brReader.Close()
        brWriter.Close()
        input.Close()
        output.Close()
    End Sub

    Public Sub CreateZip(outPathname As String, folderName As String)

        Dim fsOut As FileStream = File.Create(outPathname)
        Dim zipStream As New ZipOutputStream(fsOut)

        zipStream.SetLevel(3)       '0-9, 9 being the highest level of compression

        ' This setting will strip the leading part of the folder path in the entries, to
        ' make the entries relative to the starting folder.
        ' To include the full path for each entry up to the drive root, assign folderOffset = 0.
        Dim folderOffset As Integer = folderName.Length + (If(folderName.EndsWith("\"), 0, 1))

        CompressFolder(folderName, zipStream, folderOffset)

        zipStream.IsStreamOwner = True
        ' Makes the Close also Close the underlying stream
        zipStream.Close()
    End Sub

    Private Sub CompressFolder(path As String, zipStream As ZipOutputStream, folderOffset As Integer)

        Dim files As String() = Directory.GetFiles(path)

        For Each filename As String In files

            Dim fi As New FileInfo(filename)

            Dim entryName As String = filename.Substring(folderOffset)  ' Makes the name in zip based on the folder
            entryName = ZipEntry.CleanName(entryName)       ' Removes drive from name and fixes slash direction
            Dim newEntry As New ZipEntry(entryName)
            newEntry.DateTime = fi.LastWriteTime            ' Note the zip format stores 2 second granularity

            ' Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
            '   newEntry.AESKeySize = 256;

            ' To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
            ' you need to do one of the following: Specify UseZip64.Off, or set the Size.
            ' If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
            ' but the zip will be in Zip64 format which not all utilities can understand.
            '   zipStream.UseZip64 = UseZip64.Off;
            newEntry.Size = fi.Length

            zipStream.PutNextEntry(newEntry)

            ' Zip the file in buffered chunks
            ' the "using" will close the stream even if an exception occurs
            Dim buffer As Byte() = New Byte(4095) {}
            Using streamReader As FileStream = File.OpenRead(filename)
                StreamUtils.Copy(streamReader, zipStream, buffer)
            End Using
            zipStream.CloseEntry()
        Next
        Dim folders As String() = Directory.GetDirectories(path)
        For Each folder As String In folders
            CompressFolder(folder, zipStream, folderOffset)
        Next
    End Sub

    Public Sub ExtractZipFile(archiveFilenameIn As String, outFolder As String)
        Dim zf As ZipFile = Nothing
        Try
            Dim fs As FileStream = File.OpenRead(archiveFilenameIn)
            zf = New ZipFile(fs)
            For Each zipEntry As ZipEntry In zf
                If Not zipEntry.IsFile Then     ' Ignore directories
                    Continue For
                End If
                Dim entryFileName As [String] = zipEntry.Name
                ' to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                ' Optionally match entrynames against a selection list here to skip as desired.
                ' The unpacked length is available in the zipEntry.Size property.

                Dim buffer As Byte() = New Byte(4095) {}    ' 4K is optimum
                Dim zipStream As Stream = zf.GetInputStream(zipEntry)

                ' Manipulate the output filename here as desired.
                Dim fullZipToPath As [String] = Path.Combine(outFolder, entryFileName)
                Dim directoryName As String = Path.GetDirectoryName(fullZipToPath)
                If directoryName.Length > 0 Then
                    Directory.CreateDirectory(directoryName)
                End If

                ' Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                ' of the file, but does not waste memory.
                ' The "Using" will close the stream even if an exception occurs.
                Using streamWriter As FileStream = File.Create(fullZipToPath)
                    StreamUtils.Copy(zipStream, streamWriter, buffer)
                End Using
            Next
        Finally
            If zf IsNot Nothing Then
                zf.IsStreamOwner = True     ' Makes close also shut the underlying stream
                ' Ensure we release resources
                zf.Close()
            End If
        End Try
    End Sub

    'If ((dgvDigitialBun.SelectedRows IsNot Nothing) AndAlso (dgvDigitialBun.SelectedRows.Count > 0)) Then
    'Dim folderName As String = Convert.ToString(dgvDigitialBun.SelectedRows(0).Cells("WorkingPath").Value)
    '        If (Not String.IsNullOrEmpty(folderName)) Then
    '            Process.Start(folderName)
    '        End If
    '    End If

#End Region


    Private Sub Panel1_Click(sender As Object, e As EventArgs) Handles Panel1.Click

    End Sub

    Private Sub dgvDigitialBun_Click(sender As Object, e As EventArgs) Handles dgvDigitialBun.Click
        Try
            If ((dgvDigitialBun.SelectedRows IsNot Nothing) AndAlso (dgvDigitialBun.SelectedRows.Count > 0)) Then
                Dim folderName As String = Convert.ToString(dgvDigitialBun.SelectedRows(0).Cells("WorkPath").Value)
                If (Not String.IsNullOrEmpty(folderName)) Then
                    Process.Start("explorer.exe", folderName)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgvDigitialBun_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles dgvDigitialBun.DataError
        e.Cancel = True
    End Sub
End Class

Public Class AssetData

    Public Property LanstadFile As String = String.Empty
    Public Property LocalFile As String = String.Empty
    Public Property IsCopied As Boolean = False
    Public Property IsDownloaded As Boolean = False

End Class

Public Class GSPreviewData
    Public Property CombinedText As String = String.Empty
    Public Property BiblioTitle As String = String.Empty
    Public Property NoteTitle As String = String.Empty
End Class