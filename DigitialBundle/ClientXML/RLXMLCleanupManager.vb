Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Web
Public Class RLXMLCleanupManager

    Public Property sXMLFileName As String = String.Empty
    Public Property OutputFolder As String = String.Empty
    Private bExecuteOnce As Boolean = False
    Public Property FileSequence As New List(Of String)
    Public Property AppPath As String = String.Empty
    Public Property iTblCnt As Int16 = 0
    Dim sXMLFilePath As String = String.Empty

    Private iPara As Integer = 0
    Private iChap As Integer = 0
    Private sFilePath As String = String.Empty
    Private iPrefaceCnt As Integer = 1
    Private sIndexentries As String = String.Empty
    Public Property ProjectName As String = String.Empty

    Private OutputFileList As New List(Of OutputFileManager)
    Private OutputCategory As New List(Of OutputFileCategory)

    Public Function DoMainXMLPro(ByVal XMLPath As String) As Boolean
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "INFO", .IsRegPattern = False, .OutputName = "A01-PRELIMS.xml", .ChapterTitle = "", .GroupName = "A"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "TOC", .IsRegPattern = False, .OutputName = "B-SEQ-contents.xml", .ChapterTitle = "", .GroupName = "B", .StartNum = 1})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "INT_", .IsRegPattern = False, .OutputName = "B-SEQ-INT.xml", .ChapterTitle = "", .GroupName = "B", .StartNum = 1})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "PREFACE", .IsRegPattern = False, .OutputName = "B-SEQ-preface.xml", .ChapterTitle = "", .GroupName = "B", .StartNum = 1})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "ACK", .IsRegPattern = False, .OutputName = "B-SEQ-ACK.xml", .ChapterTitle = "", .GroupName = "B", .StartNum = 1})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "ABB", .IsRegPattern = False, .OutputName = "B-SEQ-ABB.xml", .ChapterTitle = "", .GroupName = "B", .StartNum = 1})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "C[0-9]+", .IsRegPattern = True, .OutputName = $"C-SEQ-Chap-SEQ.xml", .ChapterTitle = "", .GroupName = "C"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "CON", .IsRegPattern = False, .OutputName = $"D-SEQ-CON.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "CON", .IsRegPattern = False, .OutputName = $"D-SEQ-CON.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "CONC", .IsRegPattern = False, .OutputName = $"D-SEQ-CON.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "bibliography", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "reference", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "references", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "cit worked", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "BIB", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "REF", .IsRegPattern = False, .OutputName = $"D-SEQ-BIB.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "AFT", .IsRegPattern = False, .OutputName = "D-SEQ-AFT.xml", .ChapterTitle = "", .GroupName = "D"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "INDEX", .IsRegPattern = False, .OutputName = $"I-SEQ-index-SEQ.xml", .ChapterTitle = "", .GroupName = "I"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "ATC", .IsRegPattern = False, .OutputName = $"J-SEQ-ATC.xml", .ChapterTitle = "", .GroupName = "J"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "ATA", .IsRegPattern = False, .OutputName = $"J-SEQ-ATA.xml", .ChapterTitle = "", .GroupName = "J"})
        OutputFileList.Add(New OutputFileManager With {.FileNamePattern = "ABT", .IsRegPattern = False, .OutputName = $"J-SEQ-ABT.xml", .ChapterTitle = "", .GroupName = "J"})

#If CONFIG = "FinalXML" Then
        AppPath = "\\fsdeanta\TechRelease\Accounts\Common\DeantaComposer\Publish\extra"
#Else
        AppPath = GBL.AppPath
#End If

        sXMLFilePath = XMLPath

        Array.Sort(Of String)(Me.FileSequence.ToArray())
        Try
            MergePrelimsandDED()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            XMLPro()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            CreateBookmapFile()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Return True
    End Function
    Private Function GetImageInfo(ByVal ImgPath As String) As Boolean
        Dim ArtErrorLog As String = Path.Combine(Path.GetDirectoryName(ImgPath), "ArtError_log.txt")
        Dim ErrorFiles As New List(Of String)
        For Each Img As String In Directory.GetFiles(ImgPath, "*.*", SearchOption.TopDirectoryOnly)
            Try
                Dim bmp As New Bitmap(Img)
                If (bmp.Height > 800) Then
                    ErrorFiles.Add($"FileName : {Path.GetFileName(Img)} - Height: {bmp.Height} - Width: {bmp.Width}{vbCrLf}")
                End If
            Catch ex As Exception
                ErrorFiles.Add($"Error: {ex.Message}")
            End Try
        Next
        If ((ErrorFiles IsNot Nothing) AndAlso (ErrorFiles.Count > 0)) Then
            File.AppendAllText(ArtErrorLog, $"Art error log - {ProjectName}{vbCrLf}")
            File.AppendAllText(ArtErrorLog, String.Join(vbCrLf, ErrorFiles.ToArray()))
        End If
        Return True
    End Function

    Private Function CreateBookmapFile() As Boolean
        Dim CompontFolder As String = String.Empty
        CompontFolder = Path.Combine(sXMLFilePath, OutputFolder)
        Dim BookmarkupFile As String = Path.Combine(Path.Combine(sXMLFilePath, Path.GetDirectoryName(OutputFolder)), $"{ProjectName}.bookmap")
        Dim BookContent As String = String.Empty
        BookContent = "<?xml version=""1.0"" encoding=""UTF-8""?><bookmap xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xml:lang=""en-us"" xsi:noNamespaceSchemaLocation=""bookmap.xsd"">"
        BookContent = $"{BookContent}<title>{Path.GetDirectoryName(OutputFolder)}</title>"
        Dim FileList As New List(Of String)
        FileList.AddRange(Directory.GetFiles(CompontFolder, "*.*", SearchOption.TopDirectoryOnly))
        Array.Sort(Of String)(FileList.ToArray())
        For Each xmlFile As String In FileList
            BookContent = $"{BookContent}<compref base=""{ProjectName}/components/{Path.GetFileName(xmlFile)}"" href=""components/{Path.GetFileName(xmlFile)}"" navtitle=""{Path.GetFileName(xmlFile)}""/>{vbCrLf}"
        Next
        BookContent = $"{BookContent}</bookmap>"
        If (File.Exists(BookmarkupFile)) Then
            File.Delete(BookmarkupFile)
        End If
        File.AppendAllText(BookmarkupFile, BookContent)
        Return True
    End Function
    Private Function MergePrelimsandDED() As Boolean
        Dim Prelims As String = String.Empty
        Dim DedFile As String = String.Empty
        Prelims = (From n In FileSequence Where n.Contains("PRELIMS_INFO") Select n).FirstOrDefault
        DedFile = (From n In FileSequence Where n.Contains("_DED_") Select n).FirstOrDefault
        If ((String.IsNullOrEmpty(Prelims)) Or String.IsNullOrEmpty(Prelims)) Then
            Return False
        End If
        File.WriteAllText(Prelims, File.ReadAllText(Prelims).Replace("</book>", Regex.Match(File.ReadAllText(DedFile), "<book[^>]*>((?:(?!<\/book>).)*)</book>").Groups(1).Value & "</book>"))
        FileSequence.Remove(DedFile)
        Return True
    End Function

    Private Function MathCleanUp(ByVal EqnMat As Match) As String
        Dim Content As String = EqnMat.Value
        Content = Regex.Replace(Content, "<mml_math([^><]+)?>(((?!</mml_math>).)+)</mml_math>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return Content
    End Function


    Private Function InitialCleanup(ByVal xmlFile As String) As Boolean
        Dim xmlDoc As New XmlDocument
        xmlDoc.PreserveWhitespace = True
        Dim sXMLText As String = File.ReadAllText(xmlFile)
        sXMLText = sXMLText.Replace(" xmlns=""http://docbook.org/ns/docbook""", "")
        sXMLText = sXMLText.Replace("xlink:href", "xlink_href")
        sXMLText = sXMLText.Replace("mml:", "mml_")

        sXMLText = Regex.Replace(sXMLText, "<equation[^><]+>((?:(?!</equation>).)+)</equation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLText = Regex.Replace(sXMLText, "<inlineequation[^><]*>((?:(?!</inlineequation>).)*)</inlineequation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        Try
            xmlDoc.LoadXml(sXMLText.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon($"{ex.Message} - {xmlFile}", MessageType.MSGERROR)
            Return False
        End Try
        Dim footnoteList As XmlNodeList = xmlDoc.SelectNodes("//footnote/a")
        For Each ftNode As XmlNode In footnoteList
            If (ftNode.ParentNode IsNot Nothing) Then
                ftNode.ParentNode.RemoveChild(ftNode)
            End If
        Next

        Dim PrevCnt As Int32 = 0
        Dim PageTagList As XmlNodeList = xmlDoc.SelectNodes("//book/a[@id]|//info/a[@id]|//part/a[@id]//chapter/a[@id]|//abbreviation/a[@id]")
        Dim ChapterPartNode As XmlNode = Nothing
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            ChapterPartNode = xmlDoc.SelectSingleNode("//preface/para[1]|//primary[1]|//acknowledgements/title|//chapter/title|//part/title|//preface/title|//index/title|//bibliography/title|//abbreviation/title")
            If (ChapterPartNode IsNot Nothing) Then
                For pg As Integer = PageTagList.Count - 1 To 0 Step -1
                    ChapterPartNode.PrependChild(PageTagList(pg))
                Next
            End If
        End If

        If (Path.GetFileName(xmlFile).ToLower().Contains("info")) Then
            Dim Titles As XmlNodeList = xmlDoc.SelectNodes("//book/info/title")
            If ((Titles IsNot Nothing) AndAlso (Titles.Count > 0)) Then
                If (Titles.Count = 2) Then
                    If (String.Compare(Titles(0).InnerText, Titles(1).InnerText, True) = 0) Then
                        Titles(0).ParentNode.RemoveChild(Titles(0))
                    End If
                End If
            End If
        End If

        sXMLText = xmlDoc.OuterXml

        If (Path.GetFileName(xmlFile).ToLower().Contains("toc")) Then
            If (Not sXMLText.Contains("<toc>")) Then
                sXMLText = sXMLText.Replace("<book>", "<toc><para>").Replace("</book>", "</para></toc>")
                sXMLText = sXMLText.Replace(vbTab, "</para><para>")
            End If
        End If

        sXMLText = Regex.Replace(sXMLText, $"(<book[^>]*>)(<preface[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</preface></book>", "</preface>")
        If (Regex.IsMatch(sXMLText, $"(<book[^>]*>)(<acknowledgements[^>]*>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) And (sXMLText.Contains("</acknowledgements></book>"))) Then
            sXMLText = Regex.Replace(sXMLText, $"(<book[^>]*>)(<acknowledgements[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</acknowledgements></book>", "</acknowledgements>")
        End If

        sXMLText = Regex.Replace(sXMLText, $"(<book[^>]*>)(<abbreviation[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</abbreviation></book>", "</abbreviation>")
        If (Regex.IsMatch(sXMLText, $"(<book[^>]*>)(<chapter[^>]*>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) And (sXMLText.Contains("</chapter></book>"))) Then
            sXMLText = Regex.Replace(sXMLText, $"(<book[^>]*>)(<chapter[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</chapter></book>", "</chapter>")
        End If
        sXMLText = Regex.Replace(sXMLText, $"(<chapter[^>]*>)(<index[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</index></chapter>", "</index>")
        sXMLText = HtmlEncode(sXMLText).Replace("&amp;", "&")
        File.WriteAllText(xmlFile, sXMLText)
        Return True
    End Function

    Private Function CopyPrintImageFile() As Boolean
        Dim PrintFolder As String = String.Empty
        Dim XMLImgFolder As String = String.Empty
        XMLImgFolder = Path.Combine(sXMLFilePath, Path.GetDirectoryName(OutputFolder), "images")
        PrintFolder = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GBL.LogFilePath)), "Application")
        PrintFolder = Path.Combine(PrintFolder, "images")
        If (Not Directory.Exists(PrintFolder)) Then
            GBL.DeantaBallon("Could not able to find the print images folder.", MessageType.MSGERROR)
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

        Try
            GetImageInfo(XMLImgFolder)
        Catch ex As Exception
        End Try

        Return True
    End Function

    Private Sub XMLPro()
        Dim sXMLText As String = String.Empty
        Dim FileNames As New List(Of String)
        FileNames.AddRange({"author", "conclusion", "biblio", "index", "intro", "fm_"})
        Dim iFlCnt As Integer = 1
        iTblCnt = 0
        If Not Directory.Exists(Path.Combine(sXMLFilePath, OutputFolder)) Then Directory.CreateDirectory(Path.Combine(sXMLFilePath, OutputFolder))

        GBL.DeantaBallon($"Copying print images. {GBL.LogFilePath}", MessageType.MSGINFO)
        Try
            CopyPrintImageFile()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        GBL.DeantaBallon("Copying print images - Completed.", MessageType.MSGINFO)

        For i = 0 To Me.FileSequence.Count - 1 'FileSequence.Count - 1
            Dim xmlFile As String = Path.Combine(Path.Combine(sXMLFilePath, FileSequence(i)))
            If (Not File.Exists(xmlFile)) Then
                GBL.DeantaBallon($"Could not able to find the file: {xmlFile}", MessageType.MSGERROR)
                Continue For
            End If
            iPara = 0

            sXMLText = Regex.Replace(sXMLText, "<preface([^><]+)?>(((?!</preface>).)+)</preface>", AddressOf FigureTblSeparate, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

            CallingXSLPro(Path.Combine(sXMLFilePath, FileSequence(i).ToString), "RNL-XML_New.xsl")

            If File.Exists(Path.Combine(sXMLFilePath, OutputFolder, Path.GetFileName(FileSequence(i).ToString))) Then
                sXMLText = File.ReadAllText(Path.Combine(sXMLFilePath, OutputFolder, Path.GetFileName(FileSequence(i).ToString)))
                sXMLText = sXMLText.Replace("&amp;", "&")
                sXMLText = sXMLText.Replace("&amp;amp;", "&#x0026;")
                sXMLText = sXMLText.Replace("<CHO>", "").Replace("</CHO>", "")
                'iPara = 0 '05-11-2021
                'If FileSequence(i). iChap = i + 1
                Dim sRemove As String = " xmlns:fo=~http://www.w3.org/1999/XSL/Format~ xmlns:xlink=~http://www.w3.org/1999/xlink~ xmlns:mml=~http://www.w3.org/1998/Math/MathML~"
                sXMLText = Regex.Replace(sXMLText, sRemove.Replace("~", Chr(34).ToString), "", RegexOptions.Singleline)
                sRemove = " xmlns:msxsl=~urn:schemas-microsoft-com:xslt~ xmlns:d=~http://docbook.org/ns/docbook~ xmlns:aid=~http://ns.adobe.com/AdobeInDesign/4.0/~ xmlns:aid5=~http://ns.adobe.com/AdobeInDesign/5.0/~ xmlns:code=~urn:schemas-test-code~"
                sXMLText = Regex.Replace(sXMLText, sRemove.Replace("~", Chr(34).ToString), "", RegexOptions.Singleline)
                ' Para tag id generation
                'If Not FileSequence(i).ToString.ToLower.Contains("fm_") Then
                '13-02-2021
                If (Regex.IsMatch(Path.GetFileNameWithoutExtension(FileSequence(i).ToString), "(_C[0-9]{1,3})|(_S[0-9]{1,3})", RegexOptions.Singleline Or RegexOptions.IgnoreCase)) Then
                    iChap = iChap + 1
                    iPara = 0
                    sXMLText = Regex.Replace(sXMLText, "<p([^><]+)?>", AddressOf ParaSeq, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                Else
                    Dim TmpCont As String = File.ReadAllText(xmlFile)
                    If ((TmpCont.Contains("<chapter") And TmpCont.Contains("<p")) And Not Path.GetFileName(xmlFile).ToLower().Contains("prelims") And Not Path.GetFileName(xmlFile).ToLower().Contains("int_")) Then
                        iChap = iChap + 1
                        iPara = 0
                        sXMLText = Regex.Replace(sXMLText, "<p([^><]+)?>", AddressOf ParaSeq, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    End If
                End If
                iPara = 0 '05-11-2021
                ' Footnote id generation
                sXMLText = Regex.Replace(sXMLText, "<footnote([^><]+)?>", AddressOf ParaSeq, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'iPara = 0 '05-11-2021
                ' Figure separation file
                sXMLText = Regex.Replace(sXMLText, "<figure([^><]+)?>(((?!</figure>).)+)</figure>", AddressOf FigureTblSeparate, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ' Table separation file
                'iPara = 0 '05-11-2021
                If (Not Path.GetFileName(FileSequence(i).ToString).Contains("_ABB_")) Then
                    sXMLText = Regex.Replace(sXMLText, "<table([^><]+)?>(((?!</table>).)+)</table>", AddressOf FigureTblSeparate, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                End If
                ' Preface tag cleaning
                sXMLText = Regex.Replace(sXMLText, "<preface([^><]+)?>(((?!</preface>).)+)</preface>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                ' Cleanups if any
                sXMLText = Regex.Replace(sXMLText, "<title(( )+)?>", "<title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                ' Move para, lrh, rrh tags within title tag
                sXMLText = Regex.Replace(sXMLText, "<preface([^><]+)?>(((?!</preface>).)+)</preface>", AddressOf TitlePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                Dim sNewFileName As String = String.Empty
                sNewFileName = GetFinalOutputFileName(sXMLText, Path.GetFileName(xmlFile))
                'If (String.IsNullOrEmpty(sNewFileName)) Then
                '    sNewFileName = Path.GetFileName(xmlFile)
                'End If
                Try
                    If Not Path.GetFileName(xmlFile).Contains("index") Then sIndexentries = String.Concat(sIndexentries, sXMLText)
                Catch ex As Exception
                    sIndexentries = String.Concat(sIndexentries, sXMLText)
                End Try

                sXMLText = Regex.Replace(sXMLText, "<a id=""page_[0-9ivx]+""[\s]*/>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

                sXMLText = sXMLText.Replace("xlink_href", "xlink:href")

                sXMLText = sXMLText.Replace("<sc>", "") 'karthik
                sXMLText = sXMLText.Replace("</sc>", "")
                sXMLText = sXMLText.Replace("<token>", "")
                sXMLText = sXMLText.Replace("</token>", "")
                sXMLText = sXMLText.Replace("<body></body>", "")
                sXMLText = sXMLText.Replace("<entity>", "")
                sXMLText = sXMLText.Replace("</entity>", "")
                sXMLText = sXMLText.Replace("<emphasis", "<i")
                sXMLText = sXMLText.Replace("<underline>", "<u>")
                sXMLText = sXMLText.Replace("<uri href=", "<eref href=")
                sXMLText = sXMLText.Replace("</uri>", "</eref>")

                Try
                    If String.IsNullOrEmpty(sNewFileName) Then
                        File.WriteAllText(Path.Combine(sXMLFilePath, OutputFolder, Path.GetFileName(FileSequence(i))), sXMLText.ToString.Trim)
                    Else
                        File.WriteAllText(Path.Combine(sXMLFilePath, OutputFolder, sNewFileName), sXMLText.ToString.Trim)
                        If File.Exists(Path.Combine(sXMLFilePath, OutputFolder, Path.GetFileName(FileSequence(i).ToString()))) Then My.Computer.FileSystem.DeleteFile(Path.Combine(sXMLFilePath, OutputFolder, Path.GetFileName(FileSequence(i))))
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Else
                GBL.DeantaBallon($"Could not able to convert the XSL conversion. - {Path.GetFileName(FileSequence(i).ToString)}", MessageType.MSGERROR)
                Continue For
            End If
        Next
        If (Not IndexProcess()) Then
            Exit Sub
        End If
        If (Not File.Exists(Path.Combine(sXMLFilePath, OutputFolder, "B01-contents.xml"))) Then
            ' Creation of separate content file
            Dim sTOC As String = "<?xml version=~1.0~ encoding=~UTF-8~?>|<component no_toc=~~ xmlns:xsi=~http://www.w3.org/2001/XMLSchema-instance~ xsi:noNamespaceSchemaLocation=" &
                                "~component.xsd~ xml:lang=~en-US~>|<publish_instruction toc_level=~chapter~/>|<title>Contents</title>|<body>||</body>|</component>"
            File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath, OutputFolder), "B01-contents.xml"), sTOC.ToString.Replace("~", Chr(34)).Replace("|", Environment.NewLine))

        End If

        GBL.DeantaBallon("XML process has been completed. Please check the OUT folder.", MessageType.MSGERROR)
        'Catch ex As Exception
        '    MessageBox.Show("Sorry for the inconvenience." & ex.Message, sMsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        'Finally
        '    Label1.Visible = False
        'End Try
    End Sub

    Public Function GetFinalOutputFileName(ByVal XmlContent As String, ByVal FileName As String) As String
        Dim outMgr As New OutputFileManager
        Dim outFile As String = String.Empty
        Dim Title As String = String.Empty
        outMgr = (From n In OutputFileList Where (Not String.IsNullOrEmpty(n.FileNamePattern) And (FileName.ToLower().Contains(n.FileNamePattern.ToLower())) And Not (n.IsRegPattern)) Select n).FirstOrDefault
        If ((outMgr Is Nothing) OrElse (String.IsNullOrEmpty(outMgr.OutputName))) Then
            outMgr = (From n In OutputFileList Where ((Regex.Match(FileName, n.FileNamePattern, RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) And (n.IsRegPattern)) Select n).FirstOrDefault
        End If
        If ((outMgr Is Nothing) OrElse (String.IsNullOrEmpty(outMgr.OutputName))) Then
            Title = Regex.Match(XmlContent, "<title[^>]*>(((?!<\title>).)*)</title>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
        End If
        If (Not String.IsNullOrEmpty(Title) And ((outMgr Is Nothing) OrElse (String.IsNullOrEmpty(outMgr.OutputName)))) Then
            outMgr = (From n In OutputFileList Where (String.Compare(n.ChapterTitle, Title, True) = 0) Select n).FirstOrDefault
        End If
        If ((outMgr Is Nothing) OrElse (String.IsNullOrEmpty(outMgr.OutputName))) Then
            Return String.Empty
        End If
        Try
            If ((outMgr IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(outMgr.OutputName))) Then
                outFile = GetCatagoryWiseSeq(outMgr)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Return outFile
    End Function

    Private Function GetCatagoryWiseSeq(outMgr As OutputFileManager) As String
        Dim Groupname As String = outMgr.GroupName
        Dim index As Int16 = 0
        If ((OutputCategory Is Nothing) OrElse (OutputCategory.Count = 0)) Then
            OutputCategory.Add(New OutputFileCategory With {.GroupName = outMgr.GroupName, .Count = outMgr.StartNum})
            Return outMgr.OutputName.Replace("-SEQ", outMgr.StartNum.ToString("00"))
        Else
            index = OutputCategory.FindIndex(Function(ot As OutputFileCategory)
                                                 If (String.Compare(ot.GroupName, outMgr.GroupName, True) = 0) Then
                                                     Return True
                                                 End If
                                                 Return False
                                             End Function)
            If (index <> -1) Then
                OutputCategory(index).Count = OutputCategory(index).Count + 1
                Return outMgr.OutputName.Replace("-SEQ", OutputCategory(index).Count.ToString("00"))
            Else
                OutputCategory.Add(New OutputFileCategory With {.GroupName = outMgr.GroupName, .Count = outMgr.StartNum})
                Return outMgr.OutputName.Replace("-SEQ", outMgr.StartNum.ToString("00"))
            End If
        End If
        Return String.Empty
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

    Private Function IndexProcess() As Boolean
        Dim PageContent As String = String.Empty
        Dim iNumText As String = String.Empty
        Dim IndexFiles As List(Of String)
        IndexFiles = GBL.GetFileFromFolder(Path.Combine(sXMLFilePath, OutputFolder), "*index*.xml")
        If ((IndexFiles Is Nothing) OrElse (IndexFiles.Count = 0)) Then
            Return False
        End If
        For Each Indexfile As String In IndexFiles
            Try
                If File.Exists(Indexfile) Then
                    Dim sIndexTxt As String = File.ReadAllText(Indexfile)
                    sIndexTxt = sIndexTxt.Replace(".</link>", "</link>.")
                    sIndexTxt = Regex.Replace(sIndexTxt, "<inum>(((?!<inum>).)+)</inum>", Function(ByVal mt As Match)
                                                                                              Return mt.Value.Replace("<i>", "").Replace("</i>", "")
                                                                                          End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    Dim sText As MatchCollection = Regex.Matches(sIndexTxt, "<itext>(((?!<itext>).)+)</itext>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    Dim aText() As String
                    For Each mt As Match In sText
                        If Regex.IsMatch(mt.Value.ToString, "<inum>", RegexOptions.IgnoreCase) Then
                            Dim sFindTxt As String = Regex.Match(mt.Value.ToString, "<itext>(((?!<inum>).)+)", RegexOptions.IgnoreCase).Groups(1).Value.ToString.Trim
                            If sFindTxt.EndsWith(",") Then sFindTxt = sFindTxt.Substring(0, sFindTxt.Length - 1)
                            If sFindTxt.Contains("/") Then sFindTxt = sFindTxt.Replace("/", ",")
                            aText = sFindTxt.Split(",")
                            For I = 0 To aText.Length - 1
                                Dim iNum As MatchCollection = Regex.Matches(mt.Value.ToString, "<inum>(((?!<inum>).)+)</inum>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                For Each imt As Match In iNum
                                    Try
                                        iNumText = imt.Groups(1).Value
                                        iNumText = iNumText.TrimEnd(",").TrimEnd(" ").TrimEnd(".")
                                        If Not IsNumeric(iNumText) Then Continue For
                                        Dim MtNew As Match = Regex.Match(sIndexentries, "<a id=""page_" & iNumText & """/>(((?!<a id=).)+)<a id=""page_" & Convert.ToInt32(iNumText) + 1 & """/>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                        If Not MtNew.Success Then
                                            PageContent = sIndexentries
                                        Else
                                            PageContent = MtNew.Value.ToString
                                        End If
                                        'If (Not MtNew.Success) Then
                                        '    File.AppendAllText("d:\indexxml.xml", String.Format("Input: {0}, page no:{1}", sFindTxt, imt.Value) & vbCrLf)
                                        '    Continue For
                                        'End If
                                        'Dim iPara As MatchCollection = Regex.Matches(MtNew.Value.ToString, "<p imark=""([^><]+)"">(((?!</p>).)+)</p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                        Dim iPara As MatchCollection = Regex.Matches(PageContent, "<p imark=""([^><]+)"">(((?!<\/p>).)+)<\/p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                        Dim Flag As Boolean = False
                                        'If ((iPara Is Nothing) OrElse (iPara.Count = 0)) Then
                                        '    File.AppendAllText("d:\indexxml.xml", String.Format("Input: {0}, page no:{1}", sFindTxt, imt.Value) & vbCrLf)
                                        'End If
                                        For Each mPara As Match In iPara
                                            If Regex.IsMatch(mPara.Value.ToString, aText(I).ToString.Replace("(", "\(").Replace(")", "\)"), RegexOptions.IgnoreCase) Then
                                                sIndexTxt = Regex.Replace(sIndexTxt, imt.Value.ToString, "<inum>" & mPara.Groups(1).Value & "</inum>")
                                                Flag = True
                                                Exit For
                                            End If
                                        Next
                                        If Flag = True Then
                                            'sIndexTxt = Regex.Replace(sIndexTxt, imt.Value.ToString, "<inum>" & "Saai" & "</inum>")
                                            'MsgBox(iPara.Item(0).Groups(1).Value & "::" & imt.Value.ToString)
                                            sIndexTxt = Regex.Replace(sIndexTxt, imt.Value.ToString, "<inum>" & iPara.Item(0).Groups(1).Value & "</inum>")

                                        End If
                                    Catch ex As Exception
                                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                        Continue For
                                    End Try
                                Next
                            Next
                        End If
                    Next

                    'sIndexTxt = sIndexTxt.Replace("</inum>, <inum>", "</inum> <inum>")
                    'Dim sMt As MatchCollection = Regex.Matches(sIndexTxt, "<inum>(((?!<inum>).)+)</inum>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    'For Each mt As Match In sMt
                    '    Dim MtNew As Match = Regex.Match(sIndexentries, "<a id=""page_" & mt.Groups(1).Value & """/>(((?!<a id=).)+)<a id=""page_" & Convert.ToInt32(mt.Groups(1).Value) + 1 & """/>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                    '    Dim sMt1 As Match = Regex.Match(sIndexentries, "<a id=""page_" & mt.Groups(1).Value.ToString & """/>(?:(?:(?!<p imark[^><]+).)+)?<p imark=""([^><]+)"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    '    If sMt1.Success Then
                    '        sIndexTxt = Regex.Replace(sIndexTxt, mt.Value.ToString, "<inum>" & sMt1.Groups(1).Value & "</inum>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    '    End If
                    'Next
                    File.WriteAllText(Indexfile, sIndexTxt.ToString.Trim)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next

        For Each Indexfile As String In IndexFiles
            Try
                Dim xmlDoc As New XmlDocument
                xmlDoc.PreserveWhitespace = True
                Try
                    Dim strContent As String = File.ReadAllText(Indexfile)
                    strContent = strContent.Replace("&amp;", "&")
                    strContent = HtmlDecode(strContent)
                    strContent = strContent.Replace("<i>;", ";<i>")
                    xmlDoc.LoadXml(strContent)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For 
                End Try
                Dim PrimaryList As XmlNodeList = xmlDoc.SelectNodes("//index1/itext")
                Dim CommaCnt As Integer = 0
                Dim ColnList As New List(Of String)
                For pt As Integer = 0 To PrimaryList.Count - 1
                    ColnList.Clear()
                    Try
                        If (Not String.IsNullOrEmpty(PrimaryList(pt).InnerXml) AndAlso (PrimaryList(pt).InnerXml.Contains(";"))) Then
                            CommaCnt = (PrimaryList(pt).InnerXml.Length - PrimaryList(pt).InnerXml.Replace(";", "").Length)
                            'If (PrimaryList(pt).InnerXml.Contains("historical development")) Then
                            '    MsgBox("OK")
                            'End If

                            If (CommaCnt = 1) Then
                                PrimaryList(pt).InnerXml = PrimaryList(pt).InnerXml.Replace(";", "<index1><itext>") & "</itext></index1>"
                            ElseIf (CommaCnt > 1) Then
                                ColnList.AddRange(PrimaryList(pt).InnerXml.Split(";"))
                                If ((ColnList IsNot Nothing) AndAlso (ColnList.Count > 0)) Then
                                    For it As Integer = 0 To ColnList.Count - 1
                                        If ((ColnList(it).Contains("(") And Not ColnList(it).Contains(")")) Or (Not ColnList(it).Contains("(") And ColnList(it).Contains(")"))) Then
                                            Continue For
                                        End If
                                        Try
                                            If ((ColnList(it).Contains("<i>")) Or (ColnList(it).Contains("</i>"))) Then
                                                If ((ColnList(it).Contains("<i>")) And (ColnList(it).Contains("</i>"))) Then
                                                    PrimaryList(pt).InnerXml = PrimaryList(pt).InnerXml.Replace(ColnList(it) & ";", String.Format("<index1><itext>{0}</itext></index1>", ColnList(it)))
                                                End If
                                            Else
                                                PrimaryList(pt).InnerXml = PrimaryList(pt).InnerXml.Replace(ColnList(it) & ";", String.Format("<index1><itext>{0}</itext></index1>", ColnList(it)))
                                            End If

                                        Catch ex As Exception
                                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                            Continue For
                                        End Try
                                    Next
                                End If
                            End If
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try

                Next
                'File.WriteAllText(sXMLFilePath & "\OUT\I01-index_.xml", xmlDoc.OuterXml.Replace("&amp*", "&"))
                PrimaryList = xmlDoc.SelectNodes("//index1[parent::itext]")
                Dim partn As XmlNode = Nothing
                For tt As Integer = PrimaryList.Count - 1 To 0 Step -1
                    Try
                        If (PrimaryList(tt).ParentNode IsNot Nothing) Then
                            partn = PrimaryList(tt).ParentNode.ParentNode
                        End If
                        If (partn IsNot Nothing) Then
                            partn.ParentNode.InsertAfter(PrimaryList(tt), PrimaryList(tt).ParentNode.ParentNode)
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                Next

                PrimaryList = xmlDoc.SelectNodes("//index1") ''05-11-2021 ' RESU
                If ((PrimaryList IsNot Nothing) AndAlso (PrimaryList.Count > 0)) Then
                    For p As Int16 = 0 To PrimaryList.Count - 1
                        Try
                            If (PrimaryList(p).InnerXml.StartsWith("<itext>") And PrimaryList(p).InnerXml.EndsWith("</itext>") And (PrimaryList(p).InnerXml.Contains("<inum>"))) Then
                                Dim inums As XmlNodeList = PrimaryList(p).SelectNodes(".//inum")
                                If ((inums IsNot Nothing) AndAlso (inums.Count > 0)) Then
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace("</itext>", "").ReplaceFirstOccur($"{inums(0).OuterXml}", $"</itext> {inums(0).OuterXml}", True) '15-11-2022
                                    'PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace("</itext>", "").Replace($" {inums(0).OuterXml}", $"</itext> {inums(0).OuterXml}") '15-11-2022
                                End If
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try

                    Next
                End If

                PrimaryList = xmlDoc.SelectNodes("//index1")

                Dim TmpSeeContent As String = String.Empty
                If ((PrimaryList IsNot Nothing) AndAlso (PrimaryList.Count > 0)) Then
                    For p As Int16 = 0 To PrimaryList.Count - 1
                        Try
                            If (PrimaryList(p).InnerXml.Contains("<i>See also</i>")) Then
                                Dim SeeIndex As Integer = PrimaryList(p).InnerXml.IndexOf("<i>See also</i>")
                                Dim SeeContent As String = PrimaryList(p).InnerXml.Substring(SeeIndex, (PrimaryList(p).InnerXml.Length - SeeIndex))
                                If (SeeContent.Contains(";")) Then
                                    TmpSeeContent = SeeContent.Replace(";", "</ialso>").Replace("<i>See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                ElseIf (SeeContent.Contains("</itext>")) Then
                                    TmpSeeContent = SeeContent.Replace("</itext>", "</ialso></itext>").Replace("<i>See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                Else
                                    TmpSeeContent = $"{SeeContent}</ialso>".Replace("<i>See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                End If
                            ElseIf (PrimaryList(p).InnerXml.Contains("<i>. See also</i>")) Then
                                Dim SeeIndex As Integer = PrimaryList(p).InnerXml.IndexOf("<i>. See also</i>")
                                Dim SeeContent As String = PrimaryList(p).InnerXml.Substring(SeeIndex, (PrimaryList(p).InnerXml.Length - SeeIndex))
                                If (SeeContent.Contains(";")) Then
                                    TmpSeeContent = SeeContent.Replace(";", "</ialso>").Replace("<i>. See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                ElseIf (SeeContent.Contains("</itext>")) Then
                                    TmpSeeContent = SeeContent.Replace("</itext>", "</ialso></itext>").Replace("<i>. See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                Else
                                    TmpSeeContent = $"{SeeContent}</ialso>".Replace("<i>. See also</i> ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                End If
                            ElseIf (PrimaryList(p).InnerXml.Contains(" See also")) Then
                                Dim SeeIndex As Integer = PrimaryList(p).InnerXml.IndexOf(" See also")
                                Dim SeeContent As String = PrimaryList(p).InnerXml.Substring(SeeIndex, (PrimaryList(p).InnerXml.Length - SeeIndex))
                                If (SeeContent.Contains(";")) Then
                                    TmpSeeContent = SeeContent.Replace(";", "</ialso>").Replace(" See also  ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                ElseIf (SeeContent.Contains("</itext>")) Then
                                    TmpSeeContent = SeeContent.Replace("</itext>", "</ialso></itext>").Replace(" See also ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                Else
                                    TmpSeeContent = $"{SeeContent}</ialso>".Replace(" See also ", "<ialso>")
                                    PrimaryList(p).InnerXml = PrimaryList(p).InnerXml.Replace(SeeContent, TmpSeeContent)
                                End If
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try
                    Next
                End If

                'Dim PrimaryLst As XmlNodeList = xmlDoc.SelectNodes("//index1")
                'If ((PrimaryLst IsNot Nothing) AndAlso (PrimaryLst.Count > 0)) Then
                '    For pg As Integer = 0 To PrimaryLst.Count - 1
                '        Try
                '            PrimaryLst(pg).InnerXml = Regex.Replace(PrimaryLst(pg).InnerXml.Replace("</itext>", ""), "(<itext>(((?!<inum).)*))", "$1</itext>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                '        Catch ex As Exception
                '            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                '        End Try

                '    Next
                'End If

                Dim indexContn As String = xmlDoc.OuterXml.Replace("&amp;", "&")
                indexContn = HtmlDecode(indexContn)
                indexContn = Regex.Replace(indexContn, "<sub_title[\s]*/>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                indexContn = indexContn.Replace("<itext></itext>", "").Replace("<index1></index1>", "")
                indexContn = indexContn.Replace("</inum>, <inum>", "</inum> <inum>")
                indexContn = indexContn.Replace("<itext> ", "<itext>")
                indexContn = indexContn.Replace("<i> ", "<i>")
                indexContn = HtmlEncode(indexContn)
                indexContn = indexContn.Replace("</inum>&#x2013;<inum>", "&#x2013;")
                If (File.Exists(Indexfile)) Then
                    File.Delete(Indexfile)
                End If

                File.WriteAllText(Indexfile, indexContn)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next

        Return True
    End Function

    Public Function HtmlDecode(text As String) As String
        Dim Input As String = String.Empty
        Input = text.ToString().Replace("&lt;", "<").Replace("&gt;", ">")
        For Each Dit As System.Text.RegularExpressions.Match In Regex.Matches(Input, "(&#x)([0-9A-F]{1,4})(;)")
            If (Dit.Success) Then
                Input = Input.Replace(Dit.Value, String.Format("{1}", Dit.Groups(1).Value, ChrW(Integer.Parse(Dit.Groups(2).Value, Globalization.NumberStyles.HexNumber))))
            End If
        Next
        Return Input
    End Function

    Private Function TitlePro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "((<(lrh|rrh|p ([^><]+))>(((?!</(lrh|rrh|p ([^><]+))>).)+)</(lrh|rrh|p ([^><]+))>)+)(<title>)(((?!</title>).)+)</title>", "$11$1$12</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "</?subtitle>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "<title>(((?!</title>).)+)</title>", "<title><subtitle>$1</subtitle></title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function FigureTblSeparate(m As Match)
        Try
            iPara = iPara + 1
            iTblCnt = iTblCnt + 1
            Dim sResult As String = m.Value.ToString
            sResult = Regex.Replace(sResult, vbTab, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            Dim sHeader As String = "<library xmlns:xsi=~http://www.w3.org/2001/XMLSchema-instance~ xsi:noNamespaceSchemaLocation=~library.xsd~ xml:lang=~en-US~>" & Environment.NewLine & "<body>"
            sHeader = sHeader.Replace("~", Chr(34).ToString)
            sResult = sHeader & sResult
            sResult = Regex.Replace(sResult, "(" & vbLf & ")+", vbLf, RegexOptions.Singleline)
            If Regex.IsMatch(m.Value.ToString, "<figure") Then
                Dim ChapParaID As String = Regex.Match(sResult, " imark=""([^""]+)""", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value.Replace("F", "")
                sResult = sResult.Replace("</b><b>", "")
                sResult = Regex.Replace(sResult, "(<image href=""[^""]+""[\s]*/>)(&#x2003;)", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                sResult = sResult.Replace("<uri", "<eref").Replace("</uri>", "</eref>")
                File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath.ToString, OutputFolder), $"F{Convert.ToInt16(ChapParaID.Split(".")(0)).ToString("00")}_{Convert.ToInt32(ChapParaID.Split(".")(1)).ToString("000")}.xml"), sResult & "</body>" & Environment.NewLine & "</library>")
                'If iChap <= 9 Then
                '    File.WriteAllText(sXMLFilePath.ToString & "\OUT\F0" & iChap & "_00" & iPara & ".xml", sResult & "</body>" & Environment.NewLine & "</library>")
                'Else
                '    File.WriteAllText(sXMLFilePath.ToString & "\OUT\F" & iChap & "_00" & iPara & ".xml", sResult & "</body>" & Environment.NewLine & "</library>")
                'End If
            ElseIf Regex.IsMatch(m.Value.ToString, "<table") Then
                Dim ChapParaID As String = Regex.Match(sResult, " imark=""([^""]+)""", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value.Replace("T", "")
                Dim tblFnt As String = Regex.Match(sResult, "<tblfoot>(((?!</tblfoot>).)+)</tblfoot>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
                If (Not String.IsNullOrEmpty(tblFnt)) Then
                    sResult = sResult.Replace($"<tblfoot>{tblFnt}</tblfoot>", "").Replace($"</table>", $"</table>{tblFnt}")
                End If
                sResult = sResult.Replace("</caption>&#x2003;", "</caption>")
                'If iChap <= 9 Then
                '    File.WriteAllText(sXMLFilePath.ToString & "\OUT\T0" & iChap & "_" & iTblCnt.ToString("000") & ".xml", sResult & "</body>" & Environment.NewLine & "</library>")
                'Else
                Dim FName As String = String.Empty
                If (Not String.IsNullOrEmpty(ChapParaID)) Then
                    ChapParaID = Regex.Replace(ChapParaID, "[^0-9\.]+", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                End If
                If (ChapParaID.Contains(".")) Then
                    Try
                        FName = $"{Convert.ToInt16(ChapParaID.Split(".")(0)).ToString("00")}_{Convert.ToInt32(ChapParaID.Split(".")(1)).ToString("000")}"
                    Catch ex As Exception
                        FName = ChapParaID
                    End Try
                Else
                    FName = ChapParaID
                End If
                File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath.ToString, OutputFolder), $"T{FName}.xml"), sResult & "</body>" & Environment.NewLine & "</library>") '' 15-11-2022
                'File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath.ToString, OutputFolder), $"T{Convert.ToInt16(ChapParaID.Split(".")(0)).ToString("00")}_{Convert.ToInt32(ChapParaID.Split(".")(1)).ToString("000")}.xml"), sResult & "</body>" & Environment.NewLine & "</library>") '' 15-11-2022
                'End If
            ElseIf Regex.IsMatch(m.Value.ToString, "<preface") Then
                sHeader = "<?xml version=~1.0~ encoding=~UTF-8~?>" & Environment.NewLine & "<component xmlns:xsi=~http://www.w3.org/2001/XMLSchema-instance~ xsi:noNamespaceSchemaLocation=~component.xsd~ xml:lang=~en-US~>" & Environment.NewLine & "<body>"
                sHeader = sHeader.Replace("~", Chr(34).ToString)
                sResult = sHeader & m.Groups(2).Value.ToString.Trim
                sResult = Regex.Replace(sResult, "<emphasis role=""italic"">""(((?!</emphasis>).)+)</emphasis>", "<i>$1</i>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sResult = Regex.Replace(sResult, "<emphasis role=""bold"">""(((?!</emphasis>).)+)</emphasis>", "<b>$1</b>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sResult = Regex.Replace(sResult, "<para([^><]+)?>", "<p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sResult = Regex.Replace(sResult, "</para>", "</p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                Dim mt1 As Match = Regex.Match(sResult, "<title>(((?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                Dim TmpFileName As String = mt1.Groups(1).Value.ToString
                TmpFileName = Regex.Replace(TmpFileName, "<emphasis[^>]*>", "").Replace(" ", "").Replace("</emphasis>", "")
                TmpFileName = TmpFileName.Replace("/", "").Replace("\", "").Replace(":", "")
                If mt1.Success Then
                    File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath.ToString, OutputFolder), "B0" & iPara & "-" & TmpFileName & ".xml"), sResult & "</body>" & Environment.NewLine & "</component>")
                Else
                    File.WriteAllText(Path.Combine(Path.Combine(sXMLFilePath.ToString, OutputFolder), "B0" & iPara & "-" & ".xml"), sResult & "</body>" & Environment.NewLine & "</component>")
                End If
                iPrefaceCnt = iPrefaceCnt + 1
            End If
            Return ""
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return ""
        End Try
        Return ""
    End Function

    Private Function ParaSeq(m As Match)
        iPara = iPara + 1 '05-11-2021
        If (m.Value.ToString().Contains("<p class")) Then
            Return m.Value.ToString()
        End If
        If m.Value.ToString.StartsWith("<p") Then
            Return "<p imark=" & Chr(34) & iChap & "." & iPara & Chr(34) & ">"
        ElseIf m.Value.ToString.StartsWith("<footnote") Then
            Return "<footnote imark=" & Chr(34) & iChap & "n" & iPara & Chr(34) & ">"
        Else
            Return m.Value.ToString
        End If
    End Function

    Private Sub CallingXSLPro(sXMLFile As String, ByVal xslName As String)
        Dim OutputPath As String = String.Empty
        Dim BatFileContent As String = String.Empty
        OutputPath = Path.Combine(Path.GetDirectoryName(sXMLFile), "XSLConv")
        If (Directory.Exists(OutputPath)) Then
            Array.ForEach(Directory.GetFiles(OutputPath), Sub(sfile As String)
                                                              Try
                                                                  File.Delete(sfile)
                                                              Catch ex As Exception
                                                              End Try
                                                          End Sub)
        Else
            Directory.CreateDirectory(OutputPath)
        End If
        If File.Exists(Path.Combine(AppPath, "saxon9.jar")) Then File.Copy(Path.Combine(AppPath, "saxon9.jar"), OutputPath & "\saxon9.jar")
        If File.Exists(Path.Combine(AppPath, xslName)) Then File.Copy(Path.Combine(AppPath, xslName), OutputPath & "\" & xslName)
        If File.Exists(sXMLFile) Then File.Copy(sXMLFile, Path.Combine(OutputPath, Path.GetFileName(sXMLFile)))
        'If File.Exists(Path.Combine(AppPath, "ent.xsl")) Then File.Copy(Path.Combine(AppPath, "ent.xsl"), OutputPath & "\ent.xsl")

        Dim TmpInptXML As String = Path.Combine(OutputPath, Path.GetFileName(sXMLFile))

        Try
            InitialCleanup(TmpInptXML)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        BatFileContent = "java -jar """ & Path.GetFileName(Path.Combine(AppPath, "saxon9.jar")) & """ -s:""" & Path.GetFileName(sXMLFile) & """ -xsl:""" &
                                                           Path.GetFileName(Path.Combine(AppPath, xslName)) & """ -o:""" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml" & """"

        If (Not CreateBatAndRunFile(BatFileContent, OutputPath)) Then
            GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
        End If
        If File.Exists(Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml")) Then
            File.Copy(Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml"), Path.Combine(Path.Combine(Path.GetDirectoryName(sXMLFile), OutputFolder), Path.GetFileNameWithoutExtension(sXMLFile) & ".xml"))
        Else
            GBL.DeantaBallon("XSL cannot be properly executed for the file : ", MessageType.MSGERROR)
        End If
    End Sub

    Private Function CreateBatAndRunFile(BatFileContent As String, OutputPath As String) As Boolean
        Try
            If (File.Exists(Path.Combine(OutputPath, "run.bat"))) Then File.Delete(Path.Combine(OutputPath, "run.bat"))
            File.WriteAllText(Path.Combine(OutputPath, "run.bat"), BatFileContent)
            While (File.Exists(Path.Combine(OutputPath, "run.bat")))
                Exit While
            End While
            Dim SaxjanProcessInfo As New ProcessStartInfo(Path.Combine(OutputPath, "run.bat"))
            SaxjanProcessInfo.WorkingDirectory = OutputPath
            SaxjanProcessInfo.RedirectStandardError = True
            SaxjanProcessInfo.RedirectStandardOutput = True
            SaxjanProcessInfo.CreateNoWindow = True
            SaxjanProcessInfo.WindowStyle = ProcessWindowStyle.Hidden
            SaxjanProcessInfo.UseShellExecute = False
            Dim SaxjanProcess As Process = Process.Start(SaxjanProcessInfo)
            SaxjanProcess.WaitForExit()
            'Threading.Thread.Sleep(2500)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class

Public Class OutputFileManager
    Public Property FileNamePattern As String = String.Empty
    Public Property IsRegPattern As Boolean = True
    Public Property OutputName As String = String.Empty
    Public Property ChapterTitle As String = String.Empty
    Public Property GroupName As String = String.Empty
    Public Property StartNum As Int16 = 1
End Class


Public Class OutputFileCategory
    Public Property GroupName As String = String.Empty
    Public Property Count As Int16 = 0
End Class