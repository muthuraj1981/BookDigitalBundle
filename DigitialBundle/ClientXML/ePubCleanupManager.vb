Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Xsl
Imports FuzzySharp

Public Class ePubCleanupManager

    Public Property XmlContent As String = String.Empty
    Public Property TocPageContent As String = String.Empty
    Public Property FolderPath As String = String.Empty
    Public Property DocType As DocumentType = DocumentType.NONE
    Public Property OrgDocType As DocumentType = DocumentType.NONE
    Public Property IsBookEndNote As Boolean = False

    Dim BoxCits As New List(Of String)
    Public Property EbpsPageMapContent As String = String.Empty
    Dim PageNumList As New List(Of PageNumData)
    Dim xmlEpub As New XmlDocument

    Public Sub New(ByVal FilePath As String)
        FolderPath = FilePath
        xmlEpub.PreserveWhitespace = True
        PageNumList.Clear()
    End Sub

    Public Function DoEpubCleanup(ByVal XmlContent As String) As Boolean
        Me.XmlContent = XmlContent
        Me.XmlContent = Me.XmlContent.Replace("xlink:href", "xlinkhref")
        Me.XmlContent = Me.XmlContent.Replace("mml:", "mml_").Replace("&#x2005;", " ")

        Me.XmlContent = Regex.Replace(Me.XmlContent, "(<link role=""page""[^>]*>[0-9]+[n]+[0-9]+)</link>(&#x2013;)<link role=""page""[^>]*>([0-9]+</link>)", "$1$2$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Me.XmlContent = Regex.Replace(Me.XmlContent, "(<link role=""page""[^>]*>[0-9]+[n]+[0-9]+)</link>(&#x2013;)<link role=""page""[^>]*>([0-9]+[n]+[0-9]+</link>)", "$1$2$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Me.XmlContent = Regex.Replace(Me.XmlContent, "(<link role=""page""[^>]*>[0-9]+)</link>(&#x2013;)<link role=""page""[^>]*>([0-9]+[n]+[0-9]+</link>)", "$1$2$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        'Try
        '    ExpandFloatCitation()
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try

        PageNumList.Clear()
        BoxCits.Clear()
        If (Not Directory.Exists(System.IO.Path.GetTempPath)) Then
            Directory.CreateDirectory(System.IO.Path.GetTempPath)
        End If
        Dim TmpFile As String = Path.Combine(Path.GetTempPath, Path.GetFileName(Path.GetTempFileName) & ".xml")
        System.IO.File.WriteAllText(TmpFile, String.Format("<eclean>{0}</eclean>", Me.XmlContent))


        Try
            xmlEpub.LoadXml(File.ReadAllText(TmpFile).Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(xmlEpub.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http: //www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")

        Dim IndexNode As XmlNode = xmlEpub.SelectSingleNode("//index")
        If (IndexNode IsNot Nothing) Then
            IndexNode.InnerXml = IndexNode.InnerXml.Replace(".</link>", "</link>.")
        End If

        Try
            RemoveDuplicateInfo()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            ChapterLevelLink()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            LinkHostURL()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Try
            InsertSectionLabelSpace()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            MoveFloatElementatEndofPara()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            ' AddPageNumber()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            AddMissingPageNumber()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK) Or (DocType = DocumentType.CRITICALPUB)) Then
            Try
                MovePageInsideInfo()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        Try
            ChangeFigureExtension()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            RetainingFootnoteLabel()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            RemoveFootnoteCue()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            RenameChapterID()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            InsertMiniToc()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            MoveItemizedlistInParaNode()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try



        Try
            GeneratedPageID()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            UpdateIndexPageNumber()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            ApplyClassForIndex()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            MathWhereCleanup()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Try
            OrderlistStartAttribute()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        '-Karthik PS-2503
        If ((DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
            Try
                RandLCleanup()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        If ((DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
            Dim Sections As XmlNodeList = xmlEpub.SelectNodes("//section")
            If ((Sections IsNot Nothing) AndAlso (Sections.Count > 0)) Then
                For s As Int16 = 0 To Sections.Count - 1
                    Dim titleNode As XmlNode = Sections(s).SelectSingleNode(".//title")
                    If ((titleNode IsNot Nothing) AndAlso ((String.Compare(titleNode.InnerText, "references", True) = 0) Or (String.Compare(titleNode.InnerText, "bibliography", True) = 0))) Then
                        Dim IdAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "id", "")
                        IdAttrib.Value = "1"
                        Sections(s).Attributes.Append(IdAttrib)
                    End If
                Next
            End If
        End If

        Dim SeeentryNodes As XmlNodeList = xmlEpub.SelectNodes("//see-entry[child::see-entry]")
        If ((SeeentryNodes IsNot Nothing) AndAlso (SeeentryNodes.Count > 0)) Then
            Dim MxCnt As Int16 = SeeentryNodes.Count * 3
            While ((SeeentryNodes IsNot Nothing) AndAlso (SeeentryNodes.Count > 0))
                If (MxCnt = 0) Then
                    Exit While
                End If
                If (SeeentryNodes(0).ParentNode IsNot Nothing) Then
                    Dim OutterXML As String = Regex.Replace(SeeentryNodes(0).InnerXml, "<see-entry[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</see-entry>", "")
                    Dim ParXML As String = Regex.Match(SeeentryNodes(0).OuterXml, "^<see-entry[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
                    SeeentryNodes(0).ParentNode.InnerXml = SeeentryNodes(0).ParentNode.InnerXml.Replace(SeeentryNodes(0).OuterXml, $"{ParXML}{OutterXML}</see-entry>")
                End If
                SeeentryNodes = xmlEpub.SelectNodes("//see-entry[child::see-entry]")
                MxCnt = MxCnt - 1
            End While
        End If


        Dim TableSourceLst As XmlNodeList = xmlEpub.SelectNodes("//table/tblfn|//itemizedlist/source")
        While ((TableSourceLst IsNot Nothing) AndAlso (TableSourceLst.Count > 0))
            Try
                If ((TableSourceLst(0).ParentNode IsNot Nothing) AndAlso (TableSourceLst(0).ParentNode.ParentNode IsNot Nothing)) Then
                    Dim Source As String = TableSourceLst(0).ParentNode.OuterXml.Replace(TableSourceLst(0).OuterXml, "")
                    TableSourceLst(0).ParentNode.ParentNode.InnerXml = TableSourceLst(0).ParentNode.ParentNode.InnerXml.Replace(TableSourceLst(0).ParentNode.OuterXml, Source & TableSourceLst(0).OuterXml)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            TableSourceLst = xmlEpub.SelectNodes("//table/tblfn|//itemizedlist/source")
        End While


        If ((DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
            TableSourceLst = xmlEpub.SelectNodes("//table/tblsource")
            If ((TableSourceLst IsNot Nothing) AndAlso (TableSourceLst.Count > 0)) Then
                While ((TableSourceLst IsNot Nothing) AndAlso (TableSourceLst.Count > 0))
                    Try
                        If ((TableSourceLst(0).ParentNode IsNot Nothing) AndAlso (TableSourceLst(0).ParentNode.ParentNode IsNot Nothing)) Then
                            If ((TableSourceLst(0).ParentNode.NextSibling IsNot Nothing) AndAlso (String.Compare(TableSourceLst(0).ParentNode.NextSibling.Name, "tblfn", True) = 0)) Then
                                If (TableSourceLst(0).ParentNode.NextSibling.ParentNode IsNot Nothing) Then
                                    TableSourceLst(0).ParentNode.NextSibling.ParentNode.InsertAfter(TableSourceLst(0), TableSourceLst(0).ParentNode.NextSibling)
                                End If
                            Else
                                Dim source As String = TableSourceLst(0).ParentNode.OuterXml.Replace(TableSourceLst(0).OuterXml, "")
                                TableSourceLst(0).ParentNode.ParentNode.InnerXml = TableSourceLst(0).ParentNode.ParentNode.InnerXml.Replace(TableSourceLst(0).ParentNode.OuterXml, source & TableSourceLst(0).OuterXml)
                            End If
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                    TableSourceLst = xmlEpub.SelectNodes("//table/tblsource")
                End While
            End If
        End If

        Dim TableSous As XmlNodeList = xmlEpub.SelectNodes("//table//label|//figure/label")
        If ((TableSous IsNot Nothing) AndAlso (TableSous.Count > 0)) Then
            For t As Int16 = 0 To TableSous.Count - 1
                TableSous(t).InnerXml = TableSous(t).InnerXml.Replace("&amp;#x2002;", " ")
            Next
        End If

        Dim TblImages As New List(Of String)
        If (Directory.Exists(Path.Combine(FolderPath, "ePubTables"))) Then
            TblImages.AddRange(Directory.GetFiles(Path.Combine(FolderPath, "ePubTables"), "*.jpg", SearchOption.TopDirectoryOnly))
            Dim TableFigures As XmlNodeList = Nothing
            If ((TblImages IsNot Nothing) AndAlso (TblImages.Count > 0)) Then
                For t As Int32 = 0 To TblImages.Count - 1
                    TableFigures = xmlEpub.SelectNodes("//table[@xml:id='" & Path.GetFileNameWithoutExtension(TblImages(t)) & "']", NameSpaceManager)
                    If ((TableFigures IsNot Nothing) AndAlso (TableFigures.Count > 0)) Then
                        For tf As Int32 = 0 To TableFigures.Count - 1
                            Try
                                Dim TGroupNode As XmlNode = TableFigures(tf).SelectSingleNode(".//tgroup")
                                If ((TGroupNode IsNot Nothing) AndAlso (TGroupNode.ParentNode IsNot Nothing)) Then
                                    TGroupNode.ParentNode.RemoveChild(TGroupNode)
                                    'TGroupNode.ParentNode.InnerXml = TGroupNode.ParentNode.InnerXml.Replace(TGroupNode.OuterXml, )
                                End If
                                Dim TblFnNodes As XmlNodeList = TableFigures(tf).SelectNodes(".//tblfn")
                                If ((TblFnNodes IsNot Nothing) AndAlso (TblFnNodes.Count > 0)) Then
                                    For g As Int32 = 0 To TblFnNodes.Count - 1
                                        If (TblFnNodes(g).ParentNode IsNot Nothing) Then
                                            TblFnNodes(g).ParentNode.RemoveChild(TblFnNodes(g))
                                        End If
                                    Next
                                End If
                                If (TableFigures(tf).ParentNode IsNot Nothing) Then
                                    TableFigures(tf).ParentNode.InnerXml = TableFigures(tf).ParentNode.InnerXml.Replace(TableFigures(tf).OuterXml, Regex.Replace(Regex.Replace(TableFigures(tf).OuterXml, "<title[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase), "<info[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("<table ", "<p ").Replace("</table>", "</p>").Replace("</label>", "<br/>").Replace("</title>", "").Replace("<label>", "").Replace("</info>", "") & $"<deletedfigure class=""image-tt""><deletedimg src=""../images/{Path.GetFileNameWithoutExtension(TblImages(t))}.jpg"" alt=""Images""/></deletedfigure>")
                                End If
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                Continue For
                            End Try
                        Next
                    End If
                Next
            End If
        End If

        Dim GlossaryNodes As XmlNodeList = xmlEpub.SelectNodes("//glossary[not (parent::chapter)]", NameSpaceManager)
        While ((GlossaryNodes IsNot Nothing) AndAlso (GlossaryNodes.Count > 0))
            Try
                If (GlossaryNodes(0).ParentNode IsNot Nothing) Then
                    GlossaryNodes(0).ParentNode.InnerXml = GlossaryNodes(0).ParentNode.InnerXml.Replace(GlossaryNodes(0).OuterXml, Regex.Replace(GlossaryNodes(0).OuterXml, "(<glossary)( [^>]*>)", "<section$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</glossary>", "</section>"))
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            GlossaryNodes = xmlEpub.SelectNodes("//glossary[not (parent::chapter)]", NameSpaceManager)
        End While



        'Merge multiple para in the list.
        'As per TNF 4.0 no need <br/> element in ePub. ' 22-07-2022
        'Dim ListParas As XmlNodeList = xmlEpub.SelectNodes("//listitem[count(child::para)>1]")
        'If ((ListParas IsNot Nothing) AndAlso (ListParas.Count > 0)) Then
        '    For l As Int16 = 0 To ListParas.Count - 1
        '        ListParas(l).InnerXml = Regex.Replace(ListParas(l).InnerXml, "</para><para[^>]*>", "<br/>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        '    Next
        'End If

        Dim LofList As New List(Of String)
        Dim LotList As New List(Of String)
        LofList.AddRange(New String() {"list of figures", "list of figure", "list of figures and tables", "figures", "figure", "list of illustrations", "list of illustration", "illustrations", "illustration"})
        LotList.AddRange(New String() {"list of tables", "list of table", "tables", "table"})

        Dim PrefaceNodes As XmlNodeList = xmlEpub.SelectNodes("//preface")
        If ((PrefaceNodes IsNot Nothing) AndAlso (PrefaceNodes.Count > 0)) Then
            For p As Int16 = 0 To PrefaceNodes.Count - 1
                Dim titleNode As XmlNode = PrefaceNodes(p).SelectSingleNode("./info/title")
                If (titleNode IsNot Nothing) Then
                    Select Case titleNode.InnerText.ToLower()
                        Case "contributors", "contributors", "contributor"
                            Try
                                PrefaceNodes(p).Attributes("xml:id").Value = Regex.Replace(PrefaceNodes(p).Attributes("xml:id").Value, "-preface[0-9]+", $"-{titleNode.InnerText}", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            Catch ex As Exception
                            End Try
                        Case Else
                            If (From n In LotList Where String.Compare(n, titleNode.InnerText, True) = 0 Select n).Any Then
                                Try
                                    PrefaceNodes(p).Attributes("xml:id").Value = Regex.Replace(PrefaceNodes(p).Attributes("xml:id").Value, "-preface[0-9]*", $"-tab", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                Catch ex As Exception
                                End Try
                            ElseIf (From n In LofList Where String.Compare(n, titleNode.InnerText, True) = 0 Select n).Any Then
                                Try
                                    PrefaceNodes(p).Attributes("xml:id").Value = Regex.Replace(PrefaceNodes(p).Attributes("xml:id").Value, "-preface[0-9]*", $"-fig", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                Catch ex As Exception
                                End Try
                            End If
                    End Select
                End If
            Next
        End If
        Try
            Dialogue()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim CopyRightNode As XmlNode = xmlEpub.SelectSingleNode("//biblioset/copyright")
        If ((CopyRightNode IsNot Nothing) AndAlso (CopyRightNode.ParentNode IsNot Nothing) AndAlso (CopyRightNode.ParentNode.ParentNode IsNot Nothing)) Then
            CopyRightNode.ParentNode.ParentNode.InnerXml = CopyRightNode.ParentNode.ParentNode.InnerXml.Replace(CopyRightNode.ParentNode.OuterXml, CopyRightNode.ParentNode.OuterXml.Replace(CopyRightNode.OuterXml, "") & CopyRightNode.OuterXml)
        End If
        CopyRightNode = xmlEpub.SelectSingleNode("//biblioset/bibliomisc[@role='imprint']")
        If ((CopyRightNode IsNot Nothing) AndAlso (CopyRightNode.ParentNode IsNot Nothing) AndAlso (CopyRightNode.ParentNode.ParentNode IsNot Nothing)) Then
            CopyRightNode.ParentNode.ParentNode.InnerXml = CopyRightNode.ParentNode.ParentNode.InnerXml.Replace(CopyRightNode.ParentNode.OuterXml, CopyRightNode.ParentNode.OuterXml.Replace(CopyRightNode.OuterXml, "") & CopyRightNode.OuterXml)
        End If

        Dim CopyrightNodes As XmlNodeList = xmlEpub.SelectNodes("//copyright")
        If ((CopyrightNodes IsNot Nothing) AndAlso (CopyrightNodes.Count = 2)) Then
            CopyRightNode = xmlEpub.SelectSingleNode("//biblioset/copyright")
            If (CopyRightNode IsNot Nothing) Then
                CopyRightNode.ParentNode.RemoveChild(CopyRightNode)
            End If
        End If

        Dim BibePubNode As XmlNode = xmlEpub.SelectSingleNode("//biblioid[@role='epub']")
        Dim AuthorInfo As XmlNode = xmlEpub.SelectSingleNode("//info[child::authorgroup]")
        If ((BibePubNode Is Nothing) And (AuthorInfo IsNot Nothing)) Then
            If (AuthorInfo.ParentNode IsNot Nothing) Then
                AuthorInfo.InnerXml = $"{AuthorInfo.InnerXml}<biblioset role=""isbns"" xml:id=""bs-000001""><biblioid class=""isbn"" role=""epub"">1234567890123</biblioid></biblioset>"
            End If
        End If

        If ((DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
            Dim Sections As XmlNodeList = xmlEpub.SelectNodes("//section[parent::section]/info/title[text()='References']")
            If ((Sections IsNot Nothing) AndAlso (Sections.Count > 0)) Then
                For s As Int16 = 0 To Sections.Count - 1
                    Dim SecNode As XmlNode = Sections(s).ParentNode.ParentNode.ParentNode
                    If ((SecNode IsNot Nothing) And (SecNode.ParentNode IsNot Nothing)) Then
                        SecNode.ParentNode.AppendChild(Sections(s).ParentNode.ParentNode)
                    End If
                Next
            End If
        End If

        If (((DocType = DocumentType.RL) And (IsBookEndNote)) Or ((DocType = DocumentType.SEQUOIA) And (IsBookEndNote))) Then
            Try
                AddChapterIDforBookEndNotes()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        Dim Entrys As XmlNodeList = xmlEpub.SelectNodes("//entry")
        For e As Int16 = 0 To Entrys.Count - 1
            Try
                If (String.IsNullOrEmpty(Entrys(e).InnerText.Trim())) Then
                    Entrys(e).InnerXml = $"{Entrys(e).InnerXml}&amp;#160;"
                End If
            Catch ex As Exception
            End Try
        Next

        Dim IndexPages As XmlNodeList = xmlEpub.SelectNodes("//indexterm/a[@id]")
        If ((IndexPages IsNot Nothing) AndAlso (IndexPages.Count > 0)) Then
            For i As Int16 = 0 To IndexPages.Count - 1
                If (IndexPages(i).PreviousSibling IsNot Nothing) Then
                    IndexPages(i).PreviousSibling.AppendChild(IndexPages(i))
                End If
            Next
        End If

        If (OrgDocType = DocumentType.BLOOMSBURY) Then
            Try
                xmlEpub.MoveXMLNode("//bibliomisc[@role='imprint']", "//biblioset[@role='publisher']", NodeMoveOption.MOVEBEFORE)
                xmlEpub.MoveXMLNode("//biblioset[@role='isbns']", "//colophon", NodeMoveOption.MOVEBEFORE)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        Dim BibNode As XmlNodeList = xmlEpub.SelectNodes("//bibliography")
        Dim isBookEnd As Boolean = False
        Dim BibID As String = String.Empty
        If ((BibNode IsNot Nothing) AndAlso (BibNode.Count > 0)) Then
            If (BibNode.Count = 1) Then
                Try
                    BibID = BibNode(0).SelectSingleNode(".//bibliomixed[@xml:id]", NameSpaceManager).Attributes("xml:id").Value
                    If (Not String.IsNullOrEmpty(BibID)) Then
                        BibID = BibID.Split("_")(0)
                        isBookEnd = True
                    End If
                Catch ex As Exception
                End Try
                If (isBookEnd) Then
                    Dim LmxCnt As Int16 = 0
                    Dim LinkNodes As XmlNodeList = xmlEpub.SelectNodes("//link[@role='bibr'][@linkend][not (contains(@linkend,'" & BibID & "'))]")
                    If ((LinkNodes IsNot Nothing) AndAlso (LinkNodes.Count > 0)) Then
                        LmxCnt = LinkNodes.Count * 3
                        While ((LinkNodes IsNot Nothing) AndAlso (LinkNodes.Count > 0))
                            If (LmxCnt = 0) Then
                                Exit While
                            End If
                            Try
                                Dim TmpID As String = LinkNodes(0).Attributes("linkend").Value.Split("_")(0)
                                LinkNodes(0).Attributes("linkend").Value = LinkNodes(0).Attributes("linkend").Value.Replace(TmpID, BibID)
                            Catch ex As Exception
                            End Try
                            LmxCnt = LmxCnt - 1
                            LinkNodes = xmlEpub.SelectNodes("//link[@role='bibr'][@linkend][not (contains(@linkend,'" & BibID & "'))]")
                        End While
                    End If
                End If
            End If
        End If

        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then



            Try
                AddFirstCitation("//link[@role='figure']")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            Try
                AddFirstCitation("//link[@role='table']")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try

            Dim BookTitles As XmlNodeList = xmlEpub.SelectNodes("//bibliomixed/title[count(child::emphasis[@role='italic'])>1]")
            If ((BookTitles IsNot Nothing) AndAlso (BookTitles.Count > 0)) Then
                For b As Int16 = 0 To BookTitles.Count - 1
                    BookTitles(b).InnerXml = BookTitles(b).InnerXml.Replace("</emphasis>, <emphasis role=""italic"">", ", ")
                Next
            End If

            Dim Cites As XmlNodeList = xmlEpub.SelectNodes("//bibliomixed")
            If ((Cites IsNot Nothing) AndAlso (Cites.Count > 0)) Then
                For c As Int16 = 0 To Cites.Count - 1
                    Try
                        Dim Titles As XmlNodeList = Cites(c).SelectNodes(".//title")
                        If ((Titles IsNot Nothing) AndAlso (Titles.Count > 0)) Then
                            Select Case Titles.Count
                                Case 1
                                    If ((Titles(0).ChildNodes IsNot Nothing) AndAlso (Titles(0).ChildNodes.Count > 0)) Then
                                        If ((Titles(0).ChildNodes(0).NodeType = XmlNodeType.Element) AndAlso (Titles(0).ChildNodes(0).Name = "emphasis")) Then
                                            Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).ChildNodes(0).InnerXml}</title_cite>")
                                            Continue For
                                        Else
                                            Dim Emphas As XmlNodeList = Cites(c).SelectNodes(".//emphasis[@role='italic'][not (parent::title)]")
                                            If ((Emphas IsNot Nothing) AndAlso (Emphas.Count > 0)) Then
                                                Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).InnerXml}</title_cite>")
                                                Continue For
                                            End If
                                        End If
                                    Else
                                        Dim Emphas As XmlNodeList = Cites(c).SelectNodes(".//emphasis[@role='italic'][not (parent::title)]")
                                        If ((Emphas IsNot Nothing) AndAlso (Emphas.Count > 0)) Then
                                            Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).InnerXml}</title_cite>")
                                            Continue For
                                        End If
                                    End If
                                    'If ((Titles(1).ChildNodes IsNot Nothing) AndAlso (Titles(1).ChildNodes.Count > 0)) Then
                                    '    If ((Titles(1).ChildNodes(0).NodeType = XmlNodeType.Element) AndAlso (Titles(1).ChildNodes(0).Name = "emphasis")) Then
                                    '        Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(1).OuterXml, $"<title_cite>{Titles(1).ChildNodes(0).InnerXml}</title_cite>")
                                    '        Continue For
                                    '    End If
                                    'End If
                                Case 2
                                    If ((Titles(0).ChildNodes IsNot Nothing) AndAlso (Titles(0).ChildNodes.Count > 0)) Then
                                        If ((Titles(0).ChildNodes(0).NodeType = XmlNodeType.Element) AndAlso (Titles(0).ChildNodes(0).Name = "emphasis")) Then
                                            Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).ChildNodes(0).InnerXml}</title_cite>")
                                            Continue For
                                        End If
                                    End If
                                    If ((Titles(1).ChildNodes IsNot Nothing) AndAlso (Titles(1).ChildNodes.Count > 0)) Then
                                        If ((Titles(1).ChildNodes(0).NodeType = XmlNodeType.Element) AndAlso (Titles(1).ChildNodes(0).Name = "emphasis")) Then
                                            Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(1).OuterXml, $"<title_cite>{Titles(1).ChildNodes(0).InnerXml}</title_cite>")
                                            Continue For
                                        End If
                                    End If
                            End Select
                        Else
                            Titles = Cites(c).SelectNodes(".//emphasis[@role='italic'][not (parent::title)]")
                            If ((Titles IsNot Nothing) AndAlso (Titles.Count > 0)) Then
                                Select Case Titles.Count
                                    Case 1
                                        Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).InnerXml}</title_cite>")
                                    Case 2
                                        Cites(c).InnerXml = Cites(c).InnerXml.Replace(Titles(0).OuterXml, $"<title_cite>{Titles(0).InnerXml}</title_cite>")
                                End Select
                            End If
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                Next

                Dim IsNumerRef As Boolean = IdentifyReferenceType(xmlEpub)
                If (Not IsNumerRef) Then
                    Cites = xmlEpub.SelectNodes("//bibliomixed")
                    Dim LinkNd As XmlNode = Nothing
                    If ((Cites IsNot Nothing) AndAlso (Cites.Count > 0)) Then
                        For c As Int16 = 0 To Cites.Count - 1
                            LinkNd = Nothing
                            Try
                                LinkNd = Cites(c).SelectSingleNode(".//author")
                                If (LinkNd Is Nothing) Then
                                    LinkNd = Cites(c).SelectSingleNode(".//collab")
                                End If
                                If (LinkNd Is Nothing) Then
                                    LinkNd = Cites(c).SelectSingleNode(".//editor")
                                End If
                                If ((LinkNd IsNot Nothing) AndAlso (LinkNd.ParentNode IsNot Nothing)) Then
                                    LinkNd.ParentNode.InnerXml = LinkNd.ParentNode.InnerXml.Replace(LinkNd.OuterXml, $"<biblink>{LinkNd.InnerXml}</biblink>")
                                End If
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                Continue For
                            End Try
                        Next
                    End If
                End If
            End If
        End If

        Try
            BoxCrossLinking()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        'Dim ProgramList As XmlNodeList = xmlEpub.SelectNodes("//programlisting")
        'If ((ProgramList IsNot Nothing) AndAlso (ProgramList.Count > 0)) Then
        '    For p As Int16 = 0 To ProgramList.Count - 1
        '        ProgramList(p).InnerXml = ProgramList(p).InnerXml.Replace(vbTab, "&amp;#x2002;&amp;#x2002;&amp;#x2002;&amp;#x2002;")
        '    Next
        'End If

        Me.XmlContent = xmlEpub.OuterXml
        If ((Regex.Match(Me.XmlContent, "<info[^>]*><info[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) AndAlso Me.XmlContent.Contains("</info></info>")) Then
            Me.XmlContent = Regex.Replace(Me.XmlContent, "(<info[^>]*>)(<info[^>]*>)", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Me.XmlContent = Me.XmlContent.Replace("</info></info>", "</info>")
        End If



        Me.XmlContent = Regex.Replace(Me.XmlContent, "<chapter[^>]*><info[^>]*><title[^>]*>((?:(?!<\/title>).)*)</title></info><index>", "<index>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Me.XmlContent = Me.XmlContent.Replace("</index></chapter>", "</index>")

        Me.XmlContent = Me.XmlContent.Replace("&#x003E;</a>", "</a>&#x003E;")


        Me.XmlContent = Me.XmlContent.Replace("&amp;", "&").Replace("<eclean>", "").Replace("</eclean>", "")
        If (DocType = DocumentType.RL) Then
            Me.XmlContent = Me.XmlContent.Replace("xlinkhref", "xlink:href")
        Else
            Me.XmlContent = Me.XmlContent.Replace("xlinkhref", "xlink:href")
        End If

        Me.XmlContent = Me.XmlContent.Replace("xlinkhref", "xlink:href")
        Me.XmlContent = Me.XmlContent.Replace("mml_", "mml:")
        'Me.XmlContent = Regex.Replace(Me.XmlContent, "<preface([^>]*)>", "<chapter$1>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</preface>", "</chapter>")
        'Me.XmlContent = Regex.Replace(Me.XmlContent, "<part([^>]*)>", "<chapter$1>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</part>", "</chapter>")
        Me.XmlContent = Regex.Replace(Me.XmlContent, "<a id=""page_([^""]+)""([^><]+)>", "<?page value=""$1""?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Me.XmlContent = Regex.Replace(Me.XmlContent, "<a1 id=""page_([^""]+)""([^><]+)>", "<?page1 value=""$1""?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Me.XmlContent = Me.XmlContent.Replace("  ", " ").Replace(" & ", " &#x0026; ").Replace("& ", "&#x0026; ")

        Me.XmlContent = Regex.Replace(Me.XmlContent, $"(b\-.*?\-)(chapter)([0-9]+)", "$1c$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Me.XmlContent = Regex.Replace(Me.XmlContent, $"ch\-([0-9]+)\-sec\-([0-9]+)", "h-$1-$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        Me.XmlContent = Regex.Replace(Me.XmlContent, $"(<para[^>]*>)([\s]*)(<token)", "$1$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)


        If ((OrgDocType = DocumentType.BLOOMSBURY) Or (OrgDocType = DocumentType.TRD)) Then
            If (Regex.Match(XmlContent, "(<chapter [^>]*>)(<bibliography[^>]*>)").Success And (XmlContent.Contains("</bibliography></chapter>"))) Then
                Me.XmlContent = Regex.Replace(Me.XmlContent, $"(<chapter [^>]*>)(<bibliography[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</bibliography></chapter>", "</bibliography>")
            End If
        End If

        If ((OrgDocType = DocumentType.RL) Or (OrgDocType = DocumentType.SEQUOIA)) Then
            Me.XmlContent = Regex.Replace(Me.XmlContent, $"(<chapter [^>]*>)(<index[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</index></chapter>", "</index>")
            Me.XmlContent = Regex.Replace(Me.XmlContent, "<equation[^><]+>((?:(?!</equation>).)+)</equation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            Me.XmlContent = Regex.Replace(Me.XmlContent, "<inlineequation[^><]*>((?:(?!</inlineequation>).)*)</inlineequation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If

        Me.XmlContent = Me.XmlContent.Replace(vbTab, "")

        'Me.XmlContent = Regex.Replace(Me.XmlContent, "([a-z])&([a-z])", "$1&#x0026;$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        'Me.XmlContent = Regex.Replace(Me.XmlContent, "([0-9])&([0-9])", "$1&#x0026;$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return True
    End Function

    Private Function IdentifyReferenceType(ByVal xmlTpDoc As XmlDocument) As Boolean
        Dim IsNumerRef As Boolean = False
        Dim RefNode As XmlNode = xmlTpDoc.SelectSingleNode("//bibliomixed/label")
        'If (From n In FileSequence Where (Path.GetFileName(n).Contains("_REF_") Or Path.GetFileName(n).Contains("_BIB_")) Select n).Any Then
        '    Return True
        'End If
        If (RefNode IsNot Nothing) Then
            Return True
        End If
        Return False
    End Function
    Private Function BoxCrossLinking() As Boolean
        Dim SideBars As XmlNodeList = xmlEpub.SelectNodes("//sidebar")
        Dim BoxMatch As Match = Nothing
        If ((SideBars Is Nothing) OrElse (SideBars.Count = 0)) Then Return False
        For s As Int16 = 0 To SideBars.Count - 1
            Try
                Dim Titles As XmlNodeList = SideBars(s).SelectNodes(".//title")
                If ((Titles Is Nothing) OrElse (Titles.Count = 0)) Then Continue For
                For t As Int16 = 0 To Titles.Count - 1
                    BoxMatch = Nothing
                    BoxMatch = Regex.Match(Titles(t).InnerText, "^box\s[0-9\.]+\s", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    If (Not BoxMatch.Success) Then Continue For
                    Dim BoxAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "id", "")
                    BoxAtt.Value = Regex.Replace(BoxMatch.Value, "\s", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToLower()
                    BoxCits.Add(BoxMatch.Value)
                    If (SideBars(s) IsNot Nothing) Then
                        SideBars(s).Attributes.Append(BoxAtt)
                    End If
                Next
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next

        Dim Paras As XmlNodeList = xmlEpub.SelectNodes("//para")
        'Box citation
        If ((BoxCits IsNot Nothing) AndAlso (BoxCits.Count > 0)) Then
            For p As Int16 = 0 To Paras.Count - 1
                For Each BoxCit As String In BoxCits
                    Paras(p).InnerXml = Regex.Replace(Paras(p).InnerXml, BoxCit, Function(BMat As Match)
                                                                                     If (BMat.Value.Contains("<") Or BMat.Value.Contains(">")) Then
                                                                                         Return BMat.Value
                                                                                     End If
                                                                                     Return $"<link role=""box"" linkend=""{Regex.Replace(BMat.Value, "\s", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToLower()}"">{BMat.Value.Trim()}</link> "
                                                                                 End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                Next

            Next
        End If

        Return True
    End Function

    Private Function ExpandFloatCitation() As Boolean
        Me.XmlContent = Regex.Replace(Me.XmlContent,
                                      "(<link role=""figure"" [^>]*>((?:(?!<\/link>).)*)</link>)(&#x2013;)(<link role=""figure"" [^>]*>((?:(?!<\/link>).)*)</link>)",
                                      Function(flMat As Match)
                                          Dim floatXt As String = String.Empty
                                          Dim Min As Double = Regex.Replace(flMat.Groups(2).Value, "[^0-9\.]", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                          Dim Max As Double = Regex.Replace(flMat.Groups(5).Value, "[^0-9\.]", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                          For i As Int16 = Min To Max
                                              floatXt = $"{floatXt}<link role=""figure"">{flMat.Groups(2).Value}</link>"
                                          Next
                                      End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return True
    End Function

    Private Function AddFirstCitation(ByVal XPath) As Boolean
        Dim Citations As New List(Of String)
        Dim FltCitList As New List(Of FloatCitData)
        Dim FitNodes As XmlNodeList = xmlEpub.SelectNodes(XPath)
        If ((FitNodes Is Nothing) OrElse (FitNodes.Count = 0)) Then Return False
        For f As Int16 = 0 To FitNodes.Count - 1
            FltCitList.Add(New FloatCitData With {.Index = f + 1, .CitNode = FitNodes(f), .CitText = FitNodes(f).InnerText})
        Next
        Dim DistFitCitList = (From n In FltCitList Order By n.Index Group By n.CitText Into Group Select Group).ToList
        For Each grp In DistFitCitList
            Dim FitNd As XmlNode = (From m In grp Order By m.Index Ascending Select m.CitNode).FirstOrDefault
            If (FitNd IsNot Nothing) Then
                Dim FtAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "index", "")
                FtAtt.Value = "first"
                FitNd.Attributes.Append(FtAtt)
            End If
        Next

        Dim Figures As XmlNodeList = xmlEpub.SelectNodes("//figure/label|//table/label")
        If ((Figures IsNot Nothing) AndAlso (Figures.Count > 0)) Then
            For t As Int16 = 0 To Figures.Count - 1
                Dim lbl As String = Regex.Match(Figures(t).InnerText, "(\s)+([0-9\.a-z]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(2).Value
                If (Not String.IsNullOrEmpty(lbl)) Then
                    Figures(t).InnerXml = Figures(t).InnerXml.Replace(lbl, "").Trim()
                    Dim FnNode As XmlNode = xmlEpub.CreateNode(XmlNodeType.Element, "fignum", "")
                    FnNode.InnerXml = lbl
                    If (Figures(t).ParentNode IsNot Nothing) Then
                        Figures(t).ParentNode.AppendChild(FnNode)
                    End If
                End If
            Next
        End If
        Return True
    End Function

    Private Function RandLCleanup() As Boolean
        ''xmlEpub.SelectNodes("")

        Dim Chapters As XmlNodeList = xmlEpub.SelectNodes("//chapter[@label]")
        If ((Chapters IsNot Nothing) AndAlso (Chapters.Count > 0)) Then
            For c As Int16 = 0 To Chapters.Count - 1
                Chapters(c).Attributes("label").Value = Chapters(c).Attributes("label").Value.ToLower().Replace("chapter ", "")
            Next
        End If

        Return True
    End Function

    Private Function MathCleanUp(ByVal EqnMat As Match) As String
        Dim Content As String = EqnMat.Value
        Content = Regex.Replace(Content, "<mml:math([^><]+)?>(((?!</mml:math>).)+)</mml:math>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Content = Content.Replace(".eps", ".jpg")
        'Dim text As String = "<table class=""equation""><tr><td class=""t-eqn"" id="""">MathXXX</td><td class=""t-eqn1""><p class=""eqn-r"">CaptionXXX</p></td></tr></table>"
        'Dim CapMath As Match = Regex.Match(Content, "<caption([^><]+)?>(((?!</caption>).)+)</caption>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'If (Not CapMath.Success) Then
        '    Return EqnMat.Value
        'End If
        'Dim Caption As String = CapMath.Value.Replace("<caption>", "").Replace("</caption>", "")
        'If (Not String.IsNullOrEmpty(Caption.Trim())) Then
        '    text = text.Replace("MathXXX", MatCotn).Replace("CaptionXXX", Caption)
        'Else
        '    Return EqnMat.Value
        'End If
        Return Content
    End Function
    Private Function AddChapterIDforBookEndNotes() As Boolean
        Dim FootnoteIndex As Int16 = 0
        Dim FootnoteID As String = String.Empty
        Dim Label As String = String.Empty
        Dim FootnoteList As XmlNodeList = xmlEpub.SelectNodes("//footnote[not (@linkend)]")
        If ((FootnoteList IsNot Nothing) AndAlso (FootnoteList.Count > 0)) Then
            For f As Int16 = 0 To FootnoteList.Count - 1
                Dim FtNode As XmlNode = FootnoteList(f)
                Try
                    FootnoteID = FtNode.Attributes("xml:id").Value
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    FootnoteIndex = FootnoteIndex + 1
                    Continue For
                End Try

                Try
                    If (FtNode.Attributes("label") IsNot Nothing) Then
                        Label = FtNode.Attributes("label").Value
                    Else
                        Dim LblAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "label", "")
                        LblAtt.Value = FootnoteIndex + 1
                        FtNode.Attributes.Append(LblAtt)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                Try
                    Dim DispAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "dispftid", "")
                    DispAtt.Value = Regex.Match(FootnoteList(f).InnerXml, $"^(<para [^>]*>)([0-9\.]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(2).Value
                    FootnoteList(f).Attributes.Append(DispAtt)
                    FootnoteList(f).InnerXml = Regex.Replace(FootnoteList(f).InnerXml, $"^(<para [^>]*>)([0-9\.]+)", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    FootnoteIndex = FootnoteIndex + 1
                    Continue For
                End Try
                If (String.IsNullOrEmpty(FootnoteID)) Then
                    GBL.DeantaBallon("xml:id attribute value not found. Please check." & FtNode.OuterXml, MessageType.MSGERROR)
                    FootnoteIndex = FootnoteIndex + 1
                    Continue For
                End If
                Try
                    Dim ChapNode As XmlNode = xmlEpub.SelectSingleNode($"//footnote[@linkend='{FootnoteID}']/ancestor::chapter|//footnote[@linkend='{FootnoteID}']/ancestor::preface")
                    If (ChapNode IsNot Nothing) Then
                        Dim chapteridAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "chapxmlid", "")
                        chapteridAtt.Value = ChapNode.Attributes("xml:id").Value.Split("-")(ChapNode.Attributes("xml:id").Value.Split("-").Count - 1)
                        chapteridAtt.Value = chapteridAtt.Value.Replace("chapter", "c")
                        FtNode.Attributes.Append(chapteridAtt)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        FootnoteList = xmlEpub.SelectNodes("//footnote[(@linkend)]")
        If ((FootnoteList IsNot Nothing) AndAlso (FootnoteList.Count > 0)) Then
            For f As Int16 = 0 To FootnoteList.Count - 1
                Try
                    FootnoteList(f).Attributes("role").Value = FootnoteList(f).Attributes("role").Value.Replace("end-bk-note", "end-bk1-note")
                Catch ex As Exception
                End Try
            Next
        End If

        Return True
    End Function

    Private Function OrderlistStartAttribute() As Boolean
        Dim OrderedList As XmlNodeList = xmlEpub.SelectNodes("//orderedlist/listitem[1]/para/token")
        Dim OrderNode As XmlNode = Nothing
        Dim Token As Int16 = 0
        If ((OrderedList IsNot Nothing) AndAlso (OrderedList.Count > 0)) Then
            For o As Int16 = 0 To OrderedList.Count - 1
                'If (OrderedList(o).ParentNode.ParentNode.InnerText.Contains("Marriage as the immediate shape of ")) Then
                '    MsgBox("OK")
                'End If
                If (Regex.Match(OrderedList(o).InnerText, "^[0-9]+\.[0-9]+$", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Continue For
                If (Regex.Match(OrderedList(o).InnerText, "^\([0-9]+\.[0-9]+\)+$", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Continue For
                If (Regex.Match(OrderedList(o).InnerText, "^[0-9]+\.[0-9]+\)\.+$", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Continue For
                If (Regex.Match(OrderedList(o).InnerText, "^[A-Za-z]{0,1}[0-9]+\.[0-9]+\.$", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Continue For
                If (Regex.Match(OrderedList(o).InnerText, "^[a-z]{0,1}\.$", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Continue For
                Try
                    Token = 0
                    OrderNode = Nothing
                    Token = Regex.Replace(OrderedList(o).InnerText, "[^0-9]", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    ''If ((Not String.IsNullOrEmpty(OrderedList(o).InnerText)) AndAlso Token <> 1) Then ''21-09-2021 (469198)
                    If (Not String.IsNullOrEmpty(OrderedList(o).InnerText)) Then
                        Try
                            OrderNode = OrderedList(o).ParentNode.ParentNode.ParentNode
                        Catch ex As Exception
                            OrderNode = Nothing
                        End Try
                        If (OrderNode IsNot Nothing) Then
                            Dim StartAt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "start", "")
                            StartAt.Value = Token.ToString()
                            OrderNode.Attributes.Append(StartAt)
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function

    Private Function MathWhereCleanup() As Boolean
        Dim MathWheres As XmlNodeList = xmlEpub.SelectNodes("//para")
        If ((MathWheres IsNot Nothing) AndAlso (MathWheres.Count > 0)) Then
            For m As Integer = 0 To MathWheres.Count - 1
                If (Not (String.IsNullOrEmpty(MathWheres(m).InnerText)) AndAlso (MathWheres(m).InnerText.ToLower().StartsWith("where"))) Then
                    If ((MathWheres(m).Attributes IsNot Nothing) AndAlso (MathWheres(m).Attributes("role") Is Nothing)) Then
                        Dim RoleAttrib As XmlAttribute = xmlEpub.CreateAttribute("role")
                        RoleAttrib.Value = "TXT"
                        MathWheres(m).Attributes.Append(RoleAttrib)
                    End If
                Else
                    Try
                        If (String.Compare(MathWheres(m).Attributes("role").Value, "bib_text", True) <> 0) Then
                            MathWheres(m).Attributes("role").Value = "TXT"
                        End If
                    Catch ex As Exception
                    End Try
                End If
            Next
        End If
        MathWheres = xmlEpub.SelectNodes("//para[@role=""Math_Where""]")
        Dim MathNode As XmlNode = Nothing
        Dim MaxCount As Integer = 20
        If ((MathWheres IsNot Nothing) AndAlso (MathWheres.Count > 0)) Then
            For m As Integer = 0 To MathWheres.Count - 1
                Try
                    MaxCount = 20
                    MathNode = MathWheres(m)
                    While (MaxCount <> 0)
                        If (MathNode IsNot Nothing) Then
                            If (MathNode.Attributes IsNot Nothing) AndAlso (MathNode.Attributes("role") IsNot Nothing) AndAlso (String.Compare(MathNode.Attributes("role").Value, "Math_where", True) = 0) Then
                                MathNode = MathNode.NextSibling
                                MaxCount = MaxCount - 1
                            Else
                                Exit While
                            End If
                        End If
                        MaxCount = MaxCount - 1
                    End While
                    If ((MathNode IsNot Nothing) AndAlso (MathNode.PreviousSibling IsNot Nothing)) Then
                        Dim RoleAttrib As XmlAttribute = xmlEpub.CreateAttribute("role1")
                        RoleAttrib.Value = "Math_where_last"
                        MathNode.PreviousSibling.Attributes.Append(RoleAttrib)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function



    'COPL - Move speaker inside the line
    Private Function Dialogue() As Boolean
        Dim SpeechList As XmlNodeList = xmlEpub.SelectNodes("//speech")
        If ((SpeechList Is Nothing) OrElse (SpeechList.Count = 0)) Then
            Return False
        End If
        For s As Integer = 0 To SpeechList.Count - 1
            Try
                Dim LineNode As XmlNode = GetChildNode(SpeechList(s), "line")
                Dim SpeakerNode As XmlNode = GetChildNode(SpeechList(s), "speaker")
                If ((LineNode IsNot Nothing) AndAlso (SpeakerNode IsNot Nothing)) Then
                    LineNode.InnerXml = $"{SpeakerNode.InnerXml.Trim()} {LineNode.InnerXml}"
                End If
                If ((SpeakerNode IsNot Nothing) AndAlso (SpeakerNode.ParentNode IsNot Nothing)) Then
                    SpeakerNode.ParentNode.RemoveChild(SpeakerNode)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next
        Return True
    End Function

    Private Function old_RenameChapterID() As Boolean
        Dim ChapterList As XmlNodeList = xmlEpub.SelectNodes("//chapter[@label]")
        Dim Label As String = String.Empty
        Dim XmlChapterID As String = String.Empty
        If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
            For c As Int16 = 0 To ChapterList.Count - 1
                Try
                    ChapterList(c).Attributes("label").Value = Regex.Replace(ChapterList(c).Attributes("label").Value, "chapter ", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    Label = ChapterList(c).Attributes("label").Value
                Catch ex As Exception
                    GBL.DeantaBallon("No label attribute found in the chapter.", MessageType.MSGERROR)
                    Continue For
                End Try
                If (String.IsNullOrEmpty(Label)) Then
                    GBL.DeantaBallon("Empty label value found in the chapter.", MessageType.MSGERROR)
                    Continue For
                End If
                Try
                    XmlChapterID = Regex.Match(ChapterList(c).Attributes("xml:id").Value, "(-chapter|-c)([0-9]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(2).Value
                    If (Not String.IsNullOrEmpty(XmlChapterID)) Then
                        ChapterList(c).Attributes("xml:id").Value = ChapterList(c).Attributes("xml:id").Value.Replace("-chapter" & XmlChapterID, "-c" & Label).Replace("-c" & XmlChapterID, "-c" & Label)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon("Empty label value found in the chapter.", MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Dim LastChapID As Int16 = 0
        Try
            LastChapID = Convert.ToInt16(XmlChapterID) + 1
        Catch ex As Exception
            LastChapID = ChapterList.Count + 1
        End Try

        ChapterList = xmlEpub.SelectNodes("//chapter[@label='1']")
        If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
            For c As Int16 = 0 To ChapterList.Count - 1
                Dim MtId As Match = Regex.Match(ChapterList(c).Attributes("xml:id").Value, "(-chapter|-c)([0-9]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                If (MtId.Success) Then
                    XmlChapterID = MtId.Groups(2).Value
                    If (Not String.IsNullOrEmpty(XmlChapterID)) Then
                        ChapterList(c).Attributes("xml:id").Value = ChapterList(c).Attributes("xml:id").Value.Replace("-chapter" & XmlChapterID, "-c" & LastChapID).Replace("-c" & XmlChapterID, "-c" & LastChapID)
                        LastChapID = LastChapID + 1
                    End If
                End If
            Next
        End If

        Return True
    End Function
    Private Function RenameChapterID() As Boolean
        Dim ChapterList As XmlNodeList = xmlEpub.SelectNodes("//chapter")
        Dim Label As String = String.Empty
        Dim ChapOrder As Int16 = 0
        Dim XmlChapterID As String = String.Empty
        If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
            For c As Int16 = 0 To ChapterList.Count - 1
                Try
                    If (String.IsNullOrEmpty(ChapterList(c).InnerText.Trim())) Then
                        If (ChapterList(c).ParentNode IsNot Nothing) Then
                            ChapterList(c).ParentNode.RemoveChild(ChapterList(c))
                        End If
                    End If
                Catch ex As Exception
                End Try
            Next
        End If

        ''18-08-2022 'INVS 
        'first needs to renumber the part title.
        Dim PartLst As XmlNodeList = xmlEpub.SelectNodes("//part")
        Dim PartID As String = String.Empty
        If ((PartLst IsNot Nothing) AndAlso (PartLst.Count > 0)) Then
            For p As Int16 = 0 To PartLst.Count - 1
                PartID = String.Empty
                Try
                    PartID = Regex.Match(PartLst(p).Attributes("xml:id").Value, "(-part)([0-9]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(2).Value
                Catch ex As Exception
                End Try
                If (Not String.IsNullOrEmpty(PartID)) Then
                    PartLst(p).Attributes("xml:id").Value = PartLst(p).Attributes("xml:id").Value.Replace("-part" & PartID, "-part" & (p + 1))
                Else
                    PartLst(p).Attributes("xml:id").Value = "b-part" & (p + 1)
                End If
                PartLst(p).InnerXml = Regex.Replace(PartLst(p).InnerXml, "<chapter[^>]*>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Replace("</chapter>", "")
            Next
        End If

        ChapterList = xmlEpub.SelectNodes("//chapter")
        If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
            For c As Int16 = 0 To ChapterList.Count - 1
                If ((ChapterList(c).Attributes("xml:id") IsNot Nothing) AndAlso (ChapterList(c).Attributes("xml:id").Value.Contains("-intro"))) Then
                    Continue For
                End If
                ChapOrder = ChapOrder + 1
                Label = ChapOrder.ToString()
                'If (ChapterList(c).Attributes("label") IsNot Nothing) Then
                '    Try
                '        ChapterList(c).Attributes("label").Value = Regex.Replace(ChapterList(c).Attributes("label").Value, "chapter ", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                '        Label = ChapterList(c).Attributes("label").Value
                '    Catch ex As Exception
                '        GBL.DeantaBallon("No label attribute found in the chapter.", MessageType.MSGERROR)
                '        Continue For
                '    End Try
                'Else
                '    If (ChapterList(c).Attributes("xml:id").Value.Contains("-intro")) Then
                '        Continue For
                '    End If
                '    Label = ChapOrder.ToString()
                'End If
                If (String.IsNullOrEmpty(Label)) Then
                    GBL.DeantaBallon("Empty label value found in the chapter.", MessageType.MSGERROR)
                    Continue For
                End If
                Try
                    XmlChapterID = Regex.Match(ChapterList(c).Attributes("xml:id").Value, "(-chapter|-c)([0-9]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(2).Value
                    If (String.IsNullOrEmpty(XmlChapterID)) Then
                        XmlChapterID = Label
                    End If
                    ChapterList(c).Attributes("xml:id").Value = ChapterList(c).Attributes("xml:id").Value.Replace("-chapter" & XmlChapterID, "-c" & Label).Replace("-c" & XmlChapterID, "-c" & Label)
                Catch ex As Exception
                    GBL.DeantaBallon("Empty label value found in the chapter.", MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        '04-01-2022

        Dim Colphons As XmlNodeList = xmlEpub.SelectNodes("//colophon/para")
        If ((Colphons IsNot Nothing) AndAlso (Colphons.Count > 0)) Then
            For c As Int16 = 0 To Colphons.Count - 1
                If (Colphons(c).InnerText.StartsWith("Printed and bound")) Then
                    If (Colphons(c).ParentNode IsNot Nothing) Then
                        Colphons(c).ParentNode.RemoveChild(Colphons(c))
                    End If
                End If
            Next
        End If
        '04-01-2022

        '04-01-2022
        'Dim LastChapID As Int16 = 0
        'Try
        '    LastChapID = Convert.ToInt16(XmlChapterID) + 1
        'Catch ex As Exception
        '    LastChapID = ChapterList.Count + 1
        'End Try

        'ChapterList = xmlEpub.SelectNodes("//chapter[@label='1']")
        'If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
        '    For c As Int16 = 0 To ChapterList.Count - 1
        '        Dim MtId As Match = Regex.Match(ChapterList(c).Attributes("xml:id").Value, "(-chapter|-c)([0-9]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        '        If (MtId.Success) Then
        '            XmlChapterID = MtId.Groups(2).Value
        '            If (Not String.IsNullOrEmpty(XmlChapterID)) Then
        '                ChapterList(c).Attributes("xml:id").Value = ChapterList(c).Attributes("xml:id").Value.Replace("-chapter" & XmlChapterID, "-c" & LastChapID).Replace("-c" & XmlChapterID, "-c" & LastChapID)
        '                LastChapID = LastChapID + 1
        '            End If
        '        End If
        '    Next
        'End If
        '04-01-2022
        Return True
    End Function

    Private Function ApplyClassForIndex() As Boolean
        Dim FirstNd As Boolean = False
        Dim PrimaryNodes As XmlNodeList = Nothing
        For ch As Int16 = 97 To 122
            PrimaryNodes = xmlEpub.SelectNodes("//primary/term[starts-with(.,'" & ChrW(ch) & "')]|//primary/term[starts-with(.,'" & ChrW(ch - 32) & "')]")
            FirstNd = True
            If ((PrimaryNodes IsNot Nothing) AndAlso (PrimaryNodes.Count > 0)) Then
                For p As Int16 = 0 To PrimaryNodes.Count - 1
                    If (PrimaryNodes(p).InnerText.ToLower().StartsWith("a ") Or PrimaryNodes(p).InnerText.ToLower().StartsWith("an ") Or PrimaryNodes(p).InnerText.ToLower().StartsWith("the ")) Then
                        Continue For
                    End If
                    If (PrimaryNodes(p).ParentNode IsNot Nothing) Then
                        Dim ClassAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "class", "")
                        If (FirstNd) Then
                            ClassAttrib.Value = "IND-1F"
                            FirstNd = False
                        Else
                            ClassAttrib.Value = "IND-1"
                        End If
                        Try
                            PrimaryNodes(p).ParentNode.Attributes.Append(ClassAttrib)
                        Catch ex As Exception
                        End Try
                    End If
                Next
            End If
        Next
        PrimaryNodes = xmlEpub.SelectNodes("//primary[not (@class)]/term")
        If ((PrimaryNodes IsNot Nothing) AndAlso (PrimaryNodes.Count > 0)) Then
            For p As Int16 = 0 To PrimaryNodes.Count - 1
                If (String.IsNullOrEmpty(PrimaryNodes(p).InnerText)) Then Continue For
                If (Regex.Match(PrimaryNodes(p).InnerText.First, "[0-9]+", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
                    Dim ClassAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "class", "")
                    ClassAttrib.Value = "IND-1"
                    Try
                        PrimaryNodes(p).ParentNode.Attributes.Append(ClassAttrib)
                    Catch ex As Exception
                    End Try
                ElseIf (Regex.Match(PrimaryNodes(p).InnerText, "^(&#x)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
                    Dim ClassAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "class", "")
                    ClassAttrib.Value = "IND-1"
                    Try
                        PrimaryNodes(p).ParentNode.Attributes.Append(ClassAttrib)
                    Catch ex As Exception
                    End Try
                End If
            Next
        End If

        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            Dim Paras As XmlNodeList = xmlEpub.SelectNodes("//para")
            If ((Paras IsNot Nothing) AndAlso (Paras.Count > 0)) Then
                For p As Int16 = 0 To Paras.Count - 1
                    If (Paras(p).InnerText.Contains("&#x002A; &#x002A; &#x002A;")) Then
                        Dim classNode As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "class", "")
                        classNode.Value = "center"
                        Paras(p).Attributes.Prepend(classNode)
                    End If
                Next
            End If
        End If
        Return True
    End Function


    Private Function RemoveFootnoteCue() As Boolean
        Dim FootnoteLst As XmlNodeList = xmlEpub.SelectNodes("//xref[@ref-type='fn']")
        If ((FootnoteLst IsNot Nothing) AndAlso (FootnoteLst.Count > 0)) Then
            For f As Integer = 0 To FootnoteLst.Count - 1
                Try
                    If (FootnoteLst(f).ParentNode IsNot Nothing) Then
                        FootnoteLst(f).ParentNode.RemoveChild(FootnoteLst(f))
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If


        'Dim LinkNodes As XmlNodeList = xmlEpub.SelectNodes("//link")
        'If ((LinkNodes IsNot Nothing) AndAlso (LinkNodes.Count > 0)) Then
        '    For l As Int16 = 0 To LinkNodes.Count - 1
        '        Try
        '            If ((String.IsNullOrEmpty(LinkNodes(l).InnerText.Trim())) AndAlso (LinkNodes(l).ParentNode IsNot Nothing)) Then
        '                LinkNodes(l).ParentNode.RemoveChild(LinkNodes(l))
        '            End If
        '        Catch ex As Exception
        '        End Try
        '    Next
        'End If
        Return True
    End Function

    Private Function RetainingFootnoteLabel() As Boolean
        Dim FootnoteLst As XmlNodeList = xmlEpub.SelectNodes("//footnote[@role='end-bk-note']")
        Dim TupResult As New Tuple(Of Boolean, String)(False, String.Empty)
        Dim LinkWord As String = String.Empty
        Dim CtAttrib As XmlNode = Nothing
        If ((DocType <> DocumentType.BLOOMSBURY) And (DocType <> DocumentType.TRD) And (DocType <> DocumentType.RL) And (DocType <> DocumentType.SEQUOIA)) Then

            If ((FootnoteLst IsNot Nothing) AndAlso (FootnoteLst.Count > 0)) Then
                For f As Integer = 0 To FootnoteLst.Count - 1
                    'If (FootnoteLst(f).InnerText.Contains("For comparable conceptualizations")) Then
                    '    MsgBox("OK")
                    'End If
                    CtAttrib = Nothing
                    TupResult = New Tuple(Of Boolean, String)(False, String.Empty)
                    If ((FootnoteLst(f).PreviousSibling IsNot Nothing) AndAlso ((String.Compare(FootnoteLst(f).PreviousSibling.Name, "xref", True) = 0))) Then
                        Try
                            FootnoteLst(f).Attributes("label").Value = FootnoteLst(f).PreviousSibling.InnerText
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try

                        Try
                            If ((FootnoteLst(f).InnerText.StartsWith("http")) Or (FootnoteLst(f).InnerText.StartsWith("www."))) Then
                                CtAttrib = xmlEpub.CreateNode(XmlNodeType.Attribute, "ftlinkcot", "")
                                LinkWord = "&#x00A0;&#x00A0;"
                                LinkWord = Regex.Replace(LinkWord, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                CtAttrib.Value = LinkWord
                                FootnoteLst(f).Attributes.Append(CtAttrib)
                            ElseIf (FootnoteLst(f).InnerText.Contains(" ")) Then
                                Try
                                    TupResult = GetFirstWordForePub(FootnoteLst(f))
                                Catch ex As Exception
                                End Try
                                If (TupResult.Item1) Then
                                    CtAttrib = xmlEpub.CreateNode(XmlNodeType.Attribute, "ftlinkcot1", "")
                                Else
                                    CtAttrib = xmlEpub.CreateNode(XmlNodeType.Attribute, "ftlinkcot", "")
                                End If
                                CtAttrib.Value = TupResult.Item2
                                FootnoteLst(f).Attributes.Append(CtAttrib)
                                'LinkWord = FootnoteLst(f).InnerText.Substring(0, FootnoteLst(f).InnerText.IndexOf(" "))
                                'LinkWord = Regex.Replace(LinkWord, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                                'CtAttrib.Value = LinkWord
                                'FootnoteLst(f).Attributes.Append(CtAttrib)
                                'FootnoteLst(f).InnerXml = Regex.Replace(FootnoteLst(f).InnerXml, $"(>{GBL.HtmlEncode(LinkWord)})", ">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            ElseIf (Not FootnoteLst(f).InnerText.Contains(" ")) Then
                                CtAttrib = xmlEpub.CreateNode(XmlNodeType.Attribute, "ftlinkcot", "")
                                CtAttrib.Value = FootnoteLst(f).InnerText
                                FootnoteLst(f).Attributes.Append(CtAttrib)
                                FootnoteLst(f).InnerXml = Regex.Replace(FootnoteLst(f).InnerXml, $"(>{GBL.HtmlEncode(FootnoteLst(f).InnerText)})", ">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            End If

                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try
                        Try
                            If (FootnoteLst(f).PreviousSibling.ParentNode IsNot Nothing) Then
                                FootnoteLst(f).PreviousSibling.ParentNode.RemoveChild(FootnoteLst(f).PreviousSibling)
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try

                    End If
                Next
            End If
        End If

        FootnoteLst = Nothing

        Dim chapterlst = xmlEpub.SelectNodes("//chapter|//preface")
        If ((chapterlst IsNot Nothing) AndAlso (chapterlst.Count > 0)) Then
            For c As Integer = 0 To chapterlst.Count - 1
                FootnoteLst = chapterlst(c).SelectNodes(".//footnote[@role='end-bk-note']")
                If ((FootnoteLst IsNot Nothing) AndAlso (FootnoteLst.Count > 0)) Then
                    For f As Integer = 0 To FootnoteLst.Count - 1
                        Try
                            Dim idattrib As XmlAttribute = xmlEpub.CreateAttribute(XmlNodeType.Attribute, "id", "")
                            idattrib.Value = (f + 1).ToString()
                            FootnoteLst(f).Attributes.Append(idattrib)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try
                    Next
                End If
            Next
        End If

        ' For bookend chapter and no need to renumber the footnote id as chapter wise.
        'FootnoteLst = xmlEpub.SelectNodes(".//footnote[@role='end-bk-note']")
        '        If ((FootnoteLst IsNot Nothing) AndAlso (FootnoteLst.Count > 0)) Then
        '            For f As Integer = 0 To FootnoteLst.Count - 1
        '                Try
        '                    Dim IDAttrib As XmlAttribute = xmlEpub.CreateAttribute(XmlNodeType.Attribute, "id", "")
        '                    IDAttrib.Value = (f + 1).ToString()
        '                    FootnoteLst(f).Attributes.Append(IDAttrib)
        '                Catch ex As Exception
        '                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '                    Continue For
        '                End Try
        '            Next
        '        End If

        Dim FloatNodes As XmlNodeList
        Dim Chapters As XmlNodeList = xmlEpub.SelectNodes("//chapter|//preface|//acknowledgements")
        If ((Chapters IsNot Nothing) AndAlso (Chapters.Count > 0)) Then
            For c As Int16 = 0 To Chapters.Count - 1
                FloatNodes = Chapters(c).SelectNodes(".//footnote[@role='end-ch-note']")
                If ((FloatNodes IsNot Nothing) AndAlso (FloatNodes.Count > 0)) Then
                    For f As Int16 = 0 To FloatNodes.Count - 1
                        Dim SeqAtt As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "seq", "")
                        SeqAtt.Value = $"{f + 1}"
                        FloatNodes(f).Attributes.Append(SeqAtt)
                    Next
                End If
            Next
        End If

        If ((DocType <> DocumentType.BLOOMSBURY) And (DocType <> DocumentType.TRD)) Then
            Dim ParaList As XmlNodeList = xmlEpub.SelectNodes("//para[child::footnote[@role='end-ch-note'][not (@cue)]]|//title[child::footnote[@role='end-ch-note'][not (@cue)]]|//personname[child::footnote[@role='end-ch-note'][not (@cue)]]|//line[child::footnote[@role='end-ch-note'][not (@cue)]]")
            Dim FootXML As String = String.Empty
            Dim MxFtCnt As Int16 = 0
            Dim ParaXML As String = String.Empty
            Dim ParaParent As XmlNode = Nothing
            If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
                MxFtCnt = ParaList.Count * 3
                While ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0))
                    Try
                        If (MxFtCnt = 0) Then
                            Exit While
                        End If
                        ParaXML = ParaList(0).InnerXml
                        FootXML = String.Empty
                        ParaParent = ParaList(0).ParentNode
                        FloatNodes = ParaList(0).SelectNodes(".//footnote[@role='end-ch-note']")
                        If ((FloatNodes IsNot Nothing) AndAlso (FloatNodes.Count > 0)) Then
                            For f As Integer = 0 To FloatNodes.Count - 1
                                Dim StrFt As String = Regex.Match(FloatNodes(f).OuterXml, "<footnote[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
                                StrFt = StrFt.Replace(">", " cue=""true"">")
                                ParaXML = ParaXML.Replace(FloatNodes(f).OuterXml, $"{StrFt}</footnote>")
                                FootXML = FootXML & FloatNodes(f).OuterXml
                            Next
                            ParaList(0).InnerXml = ParaXML
                            If (ParaParent IsNot Nothing) Then
                                ParaParent.InnerXml = ParaParent.InnerXml.Replace(ParaList(0).OuterXml, $"{ParaList(0).OuterXml}{FootXML}")
                            End If
                        End If
                        ParaList = xmlEpub.SelectNodes("//para[child::footnote[@role='end-ch-note'][not (@cue)]]|//title[child::footnote[@role='end-ch-note'][not (@cue)]]|//personname[child::footnote[@role='end-ch-note'][not (@cue)]]|//line[child::footnote[@role='end-ch-note'][not (@cue)]]")
                        MxFtCnt = MxFtCnt - 1
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        MxFtCnt = MxFtCnt - 1
                    End Try
                End While
            End If
        End If

        Return True
    End Function

    Private Function GetFirstWordForePub(ByVal FtNode As XmlNode) As Tuple(Of Boolean, String)
        FtNode.InnerXml = FtNode.InnerXml.Replace("</emphasis>.,", ".,</emphasis>")
        FtNode.InnerXml = FtNode.InnerXml.Replace("</emphasis>.", ".</emphasis>")
        Dim ParaNode As XmlNode = FtNode.ChildNodes(0)
        Dim IsChildNode As Boolean = False
        Dim LinkWord As String = String.Empty
        If (ParaNode Is Nothing) Then Return New Tuple(Of Boolean, String)(False, String.Empty)
        If ((ParaNode.ChildNodes Is Nothing) OrElse (ParaNode.ChildNodes.Count = 0)) Then
            LinkWord = FtNode.InnerText.Substring(0, FtNode.InnerText.IndexOf(" "))
            LinkWord = Regex.Replace(LinkWord, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            'ParaNode.InnerXml = Regex.Replace(ParaNode.InnerXml, $"(>{GBL.HtmlEncode(LinkWord)})", ">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            ParaNode.InnerXml = Replace(ParaNode.InnerXml, $"{GBL.HtmlEncode(LinkWord)}", $"<ftlinkcot>{GBL.HtmlEncode(LinkWord)}</ftlinkcot>", 1, 1)
            IsChildNode = False
        ElseIf ((ParaNode.ChildNodes IsNot Nothing) AndAlso (ParaNode.ChildNodes.Count > 0)) Then
            If (ParaNode.ChildNodes(0).NodeType <> XmlNodeType.Element) Then
                LinkWord = FtNode.InnerText.Substring(0, FtNode.InnerText.IndexOf(" "))
                LinkWord = Regex.Replace(LinkWord, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                'ParaNode.InnerXml = Regex.Replace(ParaNode.InnerXml, $"(>{GBL.HtmlEncode(LinkWord)})", ">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                ParaNode.InnerXml = Replace(ParaNode.InnerXml, $"{GBL.HtmlEncode(LinkWord)}", $"<ftlinkcot>{GBL.HtmlEncode(LinkWord)}</ftlinkcot>", , 1)
                IsChildNode = False
            ElseIf (ParaNode.ChildNodes(0).NodeType = XmlNodeType.Element) Then
                Dim eleCnt As String = String.Empty
                eleCnt = Regex.Match(ParaNode.ChildNodes(0).OuterXml, $"<{ParaNode.ChildNodes(0).Name}[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
                LinkWord = FtNode.InnerText.Substring(0, FtNode.InnerText.IndexOf(" "))
                LinkWord = Regex.Replace(LinkWord, "<[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                eleCnt = $"{eleCnt}{LinkWord}</{ParaNode.ChildNodes(0).Name}>"
                ParaNode.ChildNodes(0).InnerXml = ParaNode.ChildNodes(0).InnerXml.Replace(LinkWord.Replace("&", "&amp;"), "")
                ParaNode.InnerXml = ParaNode.InnerXml.Replace(ParaNode.ChildNodes(0).OuterXml, $"<ftlinkcot>{eleCnt}</ftlinkcot>{ParaNode.ChildNodes(0).OuterXml}")
                IsChildNode = True
            End If
        End If
        Return New Tuple(Of Boolean, String)(IsChildNode, LinkWord)
    End Function

    Private Function UpdateIndexPageNumber() As Boolean
        Dim TermNode As XmlNode = Nothing
        Dim PrevLinkNode As XmlNode = Nothing
        Dim PageTmpNum As String = String.Empty
        Dim PgNum As String = String.Empty
        Dim IndexNode As XmlNode = Nothing
        Dim IndexPageLst As XmlNodeList = xmlEpub.SelectNodes("//index//link[@role='page']")
        If ((IndexPageLst IsNot Nothing) AndAlso (IndexPageLst.Count > 0)) Then
            For p As Integer = 0 To IndexPageLst.Count - 1
                Try
                    PageTmpNum = String.Empty
                    PrevLinkNode = Nothing
                    PgNum = String.Empty
                    IndexNode = IndexPageLst(p)
                    If (IndexNode IsNot Nothing) Then
                        TermNode = IndexNode.ParentNode
                    End If
                    PgNum = IndexNode.InnerText
                    If ((PgNum.Contains("&#x")) And (PgNum.Contains("n"))) Then
                        Dim EntInx As Int16 = PgNum.IndexOf("&#x")
                        Dim NInx As Int16 = PgNum.IndexOf("n")
                        If (EntInx > NInx) Then
                            PgNum = PgNum.Split("n")(0)
                        Else
                            PgNum = PgNum.Split("&#x")(0)
                        End If
                    ElseIf (PgNum.Contains("&#x")) Then
                        PgNum = PgNum.Split("&#x")(0)
                    ElseIf (PgNum.Contains("n")) Then
                        PgNum = PgNum.Split("n")(0)
                    End If
                    If (TermNode.OuterXml.Contains("child/children, ")) Then
                        'MsgBox("OK")
                    End If
                    If (TermNode.OuterXml.Contains(String.Format("</link>&amp;#x2013;" & IndexNode.OuterXml))) Then
                        If ((IndexNode.PreviousSibling IsNot Nothing) AndAlso (IndexNode.PreviousSibling.PreviousSibling IsNot Nothing)) Then
                            Try
                                PrevLinkNode = IndexNode.PreviousSibling.PreviousSibling
                            Catch ex As Exception
                                PrevLinkNode = Nothing
                            End Try
                        End If
                        If (PrevLinkNode IsNot Nothing) Then
                            If (PrevLinkNode.InnerText.Length >= PgNum.Length) Then
                                Dim Diff As Integer = PrevLinkNode.InnerText.Length - PgNum.Length
                                PageTmpNum = String.Format("{0}{1}", (PrevLinkNode.InnerText.Substring(0, Diff)), PgNum)
                            Else
                                PageTmpNum = PgNum
                            End If
                        Else
                            PgNum = IndexNode.InnerText
                            If ((PgNum.Contains("&#x")) And (PgNum.Contains("n"))) Then
                                Dim EntInx As Int16 = PgNum.IndexOf("&#x")
                                Dim NInx As Int16 = PgNum.IndexOf("n")
                                If (EntInx > NInx) Then
                                    PgNum = PgNum.Split("n")(0)
                                Else
                                    PgNum = PgNum.Split("&#x")(0)
                                End If
                            ElseIf (PgNum.Contains("&#x")) Then
                                PgNum = PgNum.Split("&#x")(0)
                            ElseIf (PgNum.Contains("n")) Then
                                PgNum = PgNum.Split("n")(0)
                            End If
                            Dim ChapterName As String = (From n In Me.PageNumList Where n.PageNum = PgNum Select n.ChapterNum).FirstOrDefault
                            If (Not String.IsNullOrEmpty(ChapterName)) Then
                                Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "href", "")
                                HrefAttrib.Value = String.Format("{0}.xhtml#p{1}", ChapterName.ToLower().Replace("chapter", "c"), PgNum)
                                IndexPageLst(p).Attributes.Append(HrefAttrib)
                            End If
                        End If
                        If (Not String.IsNullOrEmpty(PageTmpNum)) Then
                            If (PageTmpNum.Contains("n")) Then
                                PageTmpNum = PageTmpNum.Split("n")(0)
                            End If
                            Dim ChapterName As String = (From n In Me.PageNumList Where n.PageNum = PageTmpNum Select n.ChapterNum).FirstOrDefault
                            If (Not String.IsNullOrEmpty(ChapterName)) Then
                                Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "href", "")
                                HrefAttrib.Value = String.Format("{0}.xhtml#p{1}", ChapterName.ToLower().Replace("chapter", "c"), PageTmpNum)
                                IndexPageLst(p).Attributes.Append(HrefAttrib)
                            End If
                        End If
                    Else
                        PgNum = IndexNode.InnerText
                        If ((PgNum.Contains("&#x")) And (PgNum.Contains("n"))) Then
                            Dim EntInx As Int16 = PgNum.IndexOf("&#x")
                            Dim NInx As Int16 = PgNum.IndexOf("n")
                            If (EntInx > NInx) Then
                                PgNum = PgNum.Split("n")(0)
                            Else
                                PgNum = PgNum.Split("&#x")(0)
                            End If
                        ElseIf (PgNum.Contains("&#x")) Then
                            PgNum = PgNum.Split("&#x")(0)
                        ElseIf (PgNum.Contains("n")) Then
                            PgNum = PgNum.Split("n")(0)
                        End If
                        Dim ChapterName As String = (From n In Me.PageNumList Where n.PageNum = PgNum Select n.ChapterNum).FirstOrDefault
                        If (Not String.IsNullOrEmpty(ChapterName)) Then
                            Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "href", "")
                            HrefAttrib.Value = String.Format("{0}.xhtml#p{1}", ChapterName.ToLower().Replace("chapter", "c"), PgNum)
                            IndexPageLst(p).Attributes.Append(HrefAttrib)
                        End If
                    End If
                Catch ex As Exception
                    Continue For
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If
        Return True
    End Function

    Private Function GeneratedPageID() As Boolean
        Me.TocPageContent = "<nav epub:type=""page-list""><ol>"
        Me.EbpsPageMapContent = "<page-map xmlns:epub=""http://www.idpf.org/2007/ops"" xmlns:mml=""http://www.w3.org/1998/Math/MathML"" xmlns=""http://www.idpf.org/2007/opf"">"
        Dim XmlID As String = String.Empty
        Dim PageText As String = String.Empty
        Dim ChapterNode As XmlNode = Nothing
        Dim ChapterLst As XmlNodeList = xmlEpub.SelectNodes("//a[@id]")
        If ((ChapterLst IsNot Nothing) AndAlso (ChapterLst.Count > 0)) Then
            For c As Integer = 0 To ChapterLst.Count - 1
                Try
                    PageText = ChapterLst(c).Attributes("id").Value
                Catch ex As Exception
                    PageText = String.Empty
                End Try
                Try
                    PageText = PageText.Replace("page_", "")
                    ChapterNode = ChapterLst(c).SelectSingleNode("./ancestor::chapter|./ancestor::index|./ancestor::part|./ancestor::preface")
                    If (ChapterNode IsNot Nothing) Then
                        Try
                            XmlID = ChapterNode.Attributes("xml:id").Value
                        Catch ex As Exception
                            XmlID = String.Empty
                        End Try
                    End If
                    If (Not String.IsNullOrEmpty(XmlID)) Then
                        If (XmlID.Contains("-")) Then
                            XmlID = XmlID.Split("-")(2)
                        Else
                            If (ChapterNode IsNot Nothing) Then
                                XmlID = ChapterNode.Name
                            End If
                        End If
                    Else
                        If (ChapterNode IsNot Nothing) Then
                            XmlID = ChapterNode.Name
                        End If
                    End If
                    'Me.TocPageContent = Me.TocPageContent & String.Format("<li><a href=""{0}.xhtml#page_{1}"">{1}</a></li>", XmlID, PageText)
                    'Me.EbpsPageMapContent = EbpsPageMapContent & String.Format("<page name=""{1}"" href=""xhtml/{0}.xhtml#page_{1}""/>", XmlID, PageText)
                    PageNumList.Add(New PageNumData() With {.PageNum = PageText, .ChapterNum = XmlID})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Dim TmpPageList As New List(Of PageNumData)
        TmpPageList = (From n In PageNumList Where Not IsNumeric(n.PageNum) Select n).ToList
        Dim TmpPageNumList As List(Of PageNumData)
        TmpPageNumList = (From n In PageNumList Where IsNumeric(n.PageNum) Order By Convert.ToInt32(n.PageNum) Select n).ToList
        TmpPageList.AddRange(TmpPageNumList)
        For i As Integer = 0 To TmpPageList.Count - 1
            Me.TocPageContent = Me.TocPageContent & String.Format("<li><a href=""{0}.xhtml#p{1}"">{1}</a></li>", TmpPageList(i).ChapterNum, TmpPageList(i).PageNum)
            Me.EbpsPageMapContent = EbpsPageMapContent & String.Format("<page name=""{1}"" href=""xhtml/{0}.xhtml#p{1}""/>", TmpPageList(i).ChapterNum, TmpPageList(i).PageNum)
        Next
        Me.TocPageContent = Me.TocPageContent & "</ol></nav>"
        Me.EbpsPageMapContent = Me.EbpsPageMapContent & "</page-map>"
        Return True
    End Function

    Private Function ChangeFigureExtension() As Boolean
        Dim Figures As XmlNodeList = xmlEpub.SelectNodes("//figure//imagedata|//informalfigure//imagedata")
        If ((Figures IsNot Nothing) AndAlso (Figures.Count > 0)) Then
            For f As Integer = 0 To Figures.Count - 1
                Try
                    Figures(f).Attributes("fileref").Value = Figures(f).Attributes("fileref").Value.Replace(".eps", ".jpg").Replace(".tif", ".jpg")
                Catch ex As Exception
                End Try
            Next
        End If
        Return True
    End Function

    Private Function AddPageNumber() As Boolean
        Dim ChapterList As XmlNodeList = xmlEpub.SelectNodes("//chapter")
        If ((ChapterList IsNot Nothing) AndAlso (ChapterList.Count > 0)) Then
            For pt As Integer = 0 To ChapterList.Count - 1
                Try
                    Dim XrefPageList As XmlNodeList = ChapterList(pt).SelectNodes(".//a[@id]")
                    If ((XrefPageList IsNot Nothing) AndAlso (XrefPageList.Count > 0)) Then
                        Dim TitleNode As XmlNode = ChapterList(pt).SelectSingleNode(".//info/title")
                        If (TitleNode IsNot Nothing) Then
                            TitleNode.InnerXml = String.Format("<a id=""page_{0}"" />", XrefPageList(0).Attributes("id").Value.ToLower().Replace("page_", "") - 1) & TitleNode.InnerXml
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function

    Public Function InsertMiniToc() As Boolean
        Dim ChapterNode As XmlNode = Nothing
        Dim ChapterID As String = String.Empty
        Dim SectionID As String = String.Empty
        Dim MiniTocLst As XmlNodeList = xmlEpub.SelectNodes("//chapter/info/minitoc|//section[child::info/title[./text()='Table of contents']]//para[parent::listitem]")
        If ((MiniTocLst IsNot Nothing) AndAlso (MiniTocLst.Count > 0)) Then
            For m As Integer = 0 To MiniTocLst.Count - 1
                If ((MiniTocLst(m).ChildNodes IsNot Nothing) AndAlso (MiniTocLst(m).ChildNodes.Count > 0)) Then
                    Try
                        'ChapterNode = MiniTocLst(m).ParentNode.ParentNode
                        ChapterNode = MiniTocLst(m).SelectSingleNode("./ancestor::chapter")
                        If (ChapterNode Is Nothing) Then
                            GBL.DeantaBallon("Could not able to find the chapter node for minitoc", MessageType.MSGERROR)
                            Continue For
                        End If
                        Try
                            ChapterID = ChapterNode.Attributes("xml:id").Value
                        Catch ex As Exception
                            GBL.DeantaBallon("Could not able to find the xml:id attribute in chapter for minitoc", MessageType.MSGERROR)
                            Continue For
                        End Try
                        Dim Paras As XmlNodeList = MiniTocLst(m).SelectNodes(".//para")
                        For c As Integer = Paras.Count - 1 To 0 Step -1
                            Dim MiniPara As XmlNode = Paras(c)
                            If (MiniPara.InnerText.Contains("Contents")) Then
                                MiniPara.ParentNode.RemoveChild(MiniPara)
                                Continue For
                            End If
                            SectionID = GetSectionIDForMiniToc(ChapterNode, MiniPara.InnerText)
                            If (Not String.IsNullOrEmpty(SectionID)) Then
                                Dim XMLIDAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "rid", "")
                                XMLIDAttrib.Value = SectionID
                                MiniPara.Attributes.Append(XMLIDAttrib)
                            Else
                                GBL.DeantaBallon("The title [" & MiniPara.InnerText & "] avail in MiniToc, but Not found in the chapter - [" & ChapterID & "]", MessageType.MSGERROR)
                                Continue For
                            End If
                        Next
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                End If
            Next
        End If

        MiniTocLst = xmlEpub.SelectNodes("//minitoc")
        Dim InfoNode As XmlNode = Nothing
        If ((MiniTocLst IsNot Nothing) AndAlso (MiniTocLst.Count > 0)) Then
            For m As Int16 = 0 To MiniTocLst.Count - 1
                Try
                    If (MiniTocLst(m).ParentNode IsNot Nothing) Then
                        InfoNode = MiniTocLst(m).ParentNode
                    End If
                    If ((InfoNode IsNot Nothing) AndAlso (InfoNode.ParentNode IsNot Nothing)) Then
                        InfoNode.ParentNode.InsertAfter(MiniTocLst(m), InfoNode)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If
        Return True
    End Function

    Public Function old_InsertMiniToc() As Boolean
        Dim ChapterNode As XmlNode = Nothing
        Dim ChapterID As String = String.Empty
        Dim SectionID As String = String.Empty
        Dim MiniTocLst As XmlNodeList = xmlEpub.SelectNodes("//chapter/info/minitoc|//section[child::info/title[./text()='Table of contents']]//para[parent::listitem]")
        If ((MiniTocLst IsNot Nothing) AndAlso (MiniTocLst.Count > 0)) Then
            For m As Integer = 0 To MiniTocLst.Count - 1
                If ((MiniTocLst(m).ChildNodes IsNot Nothing) AndAlso (MiniTocLst(m).ChildNodes.Count > 0)) Then
                    Try
                        'ChapterNode = MiniTocLst(m).ParentNode.ParentNode
                        ChapterNode = MiniTocLst(m).SelectSingleNode("./ancestor::chapter")
                        If (ChapterNode Is Nothing) Then
                            GBL.DeantaBallon("Could not able to find the chapter node for minitoc", MessageType.MSGERROR)
                            Continue For
                        End If
                        Try
                            ChapterID = ChapterNode.Attributes("xml:id").Value
                        Catch ex As Exception
                            GBL.DeantaBallon("Could not able to find the xml:id attribute in chapter for minitoc", MessageType.MSGERROR)
                            Continue For
                        End Try
                        For c As Integer = MiniTocLst(m).ChildNodes.Count - 1 To 0 Step -1
                            Dim MiniPara As XmlNode = MiniTocLst(m).ChildNodes(c)
                            If (MiniPara.InnerText.Contains("Contents")) Then
                                MiniPara.ParentNode.RemoveChild(MiniPara)
                                Continue For
                            End If
                            SectionID = GetSectionIDForMiniToc(ChapterNode, MiniPara.InnerText)
                            If (Not String.IsNullOrEmpty(SectionID)) Then
                                Dim XMLIDAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "rid", "")
                                XMLIDAttrib.Value = SectionID
                                MiniPara.Attributes.Append(XMLIDAttrib)
                            Else
                                GBL.DeantaBallon("The title [" & MiniPara.InnerText & "] avail in MiniToc, but Not found in the chapter - [" & ChapterID & "]", MessageType.MSGERROR)
                                Continue For
                            End If
                        Next
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                End If
            Next
        End If

        MiniTocLst = xmlEpub.SelectNodes("//minitoc")
        Dim InfoNode As XmlNode = Nothing
        If ((MiniTocLst IsNot Nothing) AndAlso (MiniTocLst.Count > 0)) Then
            For m As Int16 = 0 To MiniTocLst.Count - 1
                Try
                    If (MiniTocLst(m).ParentNode IsNot Nothing) Then
                        InfoNode = MiniTocLst(m).ParentNode
                    End If
                    If ((InfoNode IsNot Nothing) AndAlso (InfoNode.ParentNode IsNot Nothing)) Then
                        InfoNode.ParentNode.InsertAfter(MiniTocLst(m), InfoNode)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If
        Return True
    End Function

    Private Function GetSectionIDForMiniToc(ByVal ChapterNode As XmlNode, ByVal TitleText As String) As String
        Dim SectionId As String = String.Empty
        If (ChapterNode Is Nothing) Then
            Return String.Empty
        End If
        If (TitleText.Length > 20) Then
            TitleText = TitleText.Substring(0, TitleText.Length / 2)
        End If

        Dim SectionLst As XmlNodeList = ChapterNode.SelectNodes(".//section/info/title|.//bibliography/title|.//glossary/title")
        If ((SectionLst IsNot Nothing) AndAlso (SectionLst.Count > 0)) Then
            For s As Integer = 0 To SectionLst.Count - 1
                Try
                    'If (SectionLst(s).InnerText.Replace(" ", "").Replace("&#x2003;", "").Replace("&#x2002;", "").Replace(vbTab, "").StartsWith(TitleText.Replace(" ", "").Replace(vbTab, "").Replace("&#x2003;", "").Replace("&#x2002;", "").Replace(vbTab, ""))) Then
                    If Fuzz.PartialRatio(SectionLst(s).InnerText.Replace(" ", "").Replace("&#x2003;", "").Replace("&#x2002;", "").Replace(vbTab, ""), TitleText.Replace(" ", "").Replace(vbTab, "").Replace("&#x2003;", "").Replace("&#x2002;", "").Replace(vbTab, "")) > 90 Then
                        'If ((String.Compare(SectionLst(s).InnerText.Replace(" ", "").Replace(vbTab, ""), TitleText.Replace(" ", "").Replace("&#x2003;", "").Replace("&#x2002;", "").Replace(vbTab, ""), True) = 0)) Then
                        Select Case SectionLst(s).ParentNode.Name
                            Case "bibliography"
                                Return SectionLst(s).ParentNode.Attributes("xml:id").Value
                            Case Else
                                Return SectionLst(s).ParentNode.ParentNode.Attributes("xml:id").Value
                        End Select
                    End If
                Catch ex As Exception
                    Continue For
                End Try
            Next
        End If
        Return String.Empty
    End Function

    Private Function MoveItemizedlistInParaNode() As Boolean
        Dim Itemizedlists As XmlNodeList = xmlEpub.SelectNodes("//listitem/table|//itemizedlist//itemizedlist|//orderedlist//itemizedlist|//itemizedlist//orderedlist|//orderedlist//orderedlist")
        If ((Itemizedlists IsNot Nothing) AndAlso (Itemizedlists.Count > 0)) Then
            For iz As Integer = 0 To Itemizedlists.Count - 1
                Try
                    If (Itemizedlists(iz).PreviousSibling IsNot Nothing) Then
                        Dim PrevNode As XmlNode = Itemizedlists(iz).PreviousSibling
                        If ((PrevNode IsNot Nothing) AndAlso (String.Compare(PrevNode.Name, "para", True) = 0)) Then
                            PrevNode.AppendChild(Itemizedlists(iz))
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Itemizedlists = xmlEpub.SelectNodes("//glossdef/para/itemizedlist|//glossdef/para/itemizedlist")
        While ((Itemizedlists IsNot Nothing) AndAlso (Itemizedlists.Count > 0))
            Try
                Dim ParaNode As XmlNode = Itemizedlists(0)
                If ((ParaNode.ParentNode IsNot Nothing) AndAlso (ParaNode.ParentNode.ParentNode IsNot Nothing) AndAlso (ParaNode.ParentNode.ParentNode.ParentNode IsNot Nothing) AndAlso (ParaNode.ParentNode.ParentNode.ParentNode IsNot Nothing)) Then
                    ParaNode.ParentNode.ParentNode.ParentNode.ParentNode.InsertAfter(ParaNode, ParaNode.ParentNode.ParentNode.ParentNode)
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            Itemizedlists = xmlEpub.SelectNodes("//glossdef/para/itemizedlist|//glossdef/para/itemizedlist")
        End While
        Return True
    End Function

    Public Function MovePageInsideInfo() As Boolean
        Dim PageIDLst As XmlNodeList = xmlEpub.SelectNodes("//chapter/info/title|//abbreviation/info/title")
        If ((PageIDLst IsNot Nothing) AndAlso (PageIDLst.Count > 0)) Then
            For t As Int16 = 0 To PageIDLst.Count - 1
                Dim PgNode As XmlNode = PageIDLst(t).SelectSingleNode(".//a[@id]")
                If ((PgNode IsNot Nothing) AndAlso (PageIDLst(t).ParentNode IsNot Nothing)) Then
                    PageIDLst(t).ParentNode.InnerXml = PageIDLst(t).ParentNode.InnerXml.Replace(PageIDLst(t).OuterXml, $"{PgNode.OuterXml}{PageIDLst(t).OuterXml.Replace(PgNode.OuterXml, "")}")
                End If
            Next
        End If
        Return True
    End Function

    Public Function AddMissingPageNumber() As Boolean
        Dim PageIDLst As XmlNodeList = xmlEpub.SelectNodes("//a[@id]")
        Dim CurrentID As Integer = 0
        Dim NextID As Integer = 0
        For i As Integer = 0 To PageIDLst.Count - 1
            Try
                If (i <> (PageIDLst.Count - 1)) Then
                    If (Regex.Match(PageIDLst(i).Attributes("id").Value.Replace("page_", ""), "[^0-9]").Success) Then
                        Continue For
                    End If
                    CurrentID = PageIDLst(i).Attributes("id").Value.Replace("page_", "")
                    If (Not String.IsNullOrEmpty(CurrentID)) Then
                        NextID = (CurrentID + 1)
                        If (Not (From n In PageIDLst Where (String.Compare(n.Attributes("id").value, String.Format("page_" & NextID), True) = 0) Select n).Any) Then
                            If (PageIDLst(i).ParentNode IsNot Nothing) Then
                                Try
                                    Dim PageIDNode As XmlNode = xmlEpub.CreateNode(XmlNodeType.Element, "a1", "")
                                    Dim IdAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "id", "")
                                    IdAttrib.Value = "page_" & NextID
                                    PageIDNode.Attributes.Append(IdAttrib)
                                    PageIDLst(i).ParentNode.InsertAfter(PageIDNode, PageIDLst(i))
                                    'PageIDLst(i).ParentNode.InnerXml = PageIDLst(i).ParentNode.InnerXml.Replace(PageIDLst(i).OuterXml, PageIDLst(i).OuterXml & "<a id=""page_" & NextID & """ />")
                                Catch ex As Exception
                                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                                    Continue For
                                End Try
                            End If
                            GBL.DeantaBallon("New page : " & NextID, MessageType.MSGINFO)
                        End If
                    Else
                        GBL.DeantaBallon("Invalid page format" & PageIDLst(i).OuterXml, MessageType.MSGERROR)
                        Continue For
                    End If
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next
        Return True
    End Function

    Private Function MoveFloatElementatEndofPara() As Boolean
        'Dim ParaList As XmlNodeList = xmlEpub.SelectNodes("//para")
        'Dim ParaList As XmlNodeList = xmlEpub.SelectNodes("//para[not(parent::listitem)]")
        'Dim ParaXML As String = String.Empty
        'Dim FloatXML As String = String.Empty
        'If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
        '    For p As Integer = 0 To ParaList.Count - 1
        '        ParaXML = ParaList(p).OuterXml
        '        Dim FloatNodes As XmlNodeList = ParaList(p).SelectNodes(".//figure|.//table[@role='float']")
        '        If ((FloatNodes IsNot Nothing) AndAlso (FloatNodes.Count > 0)) Then
        '            For f As Integer = 0 To FloatNodes.Count - 1
        '                ParaXML = ParaXML.Replace(FloatNodes(f).OuterXml, "")
        '                FloatXML = FloatXML & FloatNodes(f).OuterXml
        '            Next
        '        End If
        '        If (ParaList(p).ParentNode IsNot Nothing) Then
        '            ParaList(p).ParentNode.InnerXml = ParaList(p).ParentNode.InnerXml.Replace(ParaList(p).OuterXml, ParaXML & FloatXML)
        '        End If
        '    Next
        'End If

        Dim ParaList As XmlNodeList = xmlEpub.SelectNodes("//para[not(parent::listitem)][not(parent::blockquote)]/figure|//para[not(parent::listitem)][not(parent::blockquote)]/table[@role='float']|//para/sidebar")
        Dim ParaXML As String = String.Empty
        Dim FloatXML As String = String.Empty
        If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
            For p As Integer = ParaList.Count - 1 To 0 Step -1
                Try
                    If ((ParaList(p).ParentNode IsNot Nothing) AndAlso (ParaList(p).ParentNode.ParentNode IsNot Nothing)) Then
                        ParaList(p).ParentNode.ParentNode.InsertAfter(ParaList(p), ParaList(p).ParentNode)
                    Else
                        GBL.DeantaBallon("Move figure outside para : not moved - " & ParaList(p).InnerText, MessageType.MSGERROR)
                        Continue For
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If

        ParaList = xmlEpub.SelectNodes("//itemizedlist//table[@role='float']|//itemizedlist//figure|//orderedlist//table[@role='float']|//orderedlist//figure")
        If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
            For p As Integer = ParaList.Count - 1 To 0 Step -1
                Try
                    Dim ListNode As XmlNode = GetParentList(ParaList(p))
                    If ((ListNode IsNot Nothing) And (ListNode.ParentNode IsNot Nothing) And (Not ListNode.Equals(xmlEpub.DocumentElement))) Then
                        ListNode.ParentNode.InsertAfter(ParaList(p), ListNode)
                    End If
                    'If ((ParaList(p).ParentNode IsNot Nothing) AndAlso (ParaList(p).ParentNode.ParentNode IsNot Nothing) AndAlso (ParaList(p).ParentNode.ParentNode.ParentNode.ParentNode IsNot Nothing)) Then
                    '    'ParaList(p).ParentNode.ParentNode.ParentNode.ParentNode.AppendChild(ParaList(p))
                    '    ParaList(p).ParentNode.ParentNode.ParentNode.ParentNode.InsertAfter(ParaList(p), ParaList(p).ParentNode.ParentNode.ParentNode)
                    'End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If

        ParaList = xmlEpub.SelectNodes("//blockquote//table[@role='float']|//blockquote//figure")
        If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
            For p As Integer = 0 To ParaList.Count - 1

                Try
                    If ((ParaList(p).ParentNode IsNot Nothing) AndAlso (ParaList(p).ParentNode.ParentNode IsNot Nothing) AndAlso (ParaList(p).ParentNode.ParentNode.ParentNode IsNot Nothing)) Then
                        ParaList(p).ParentNode.ParentNode.ParentNode.InsertAfter(ParaList(p), ParaList(p).ParentNode.ParentNode)
                    End If
                Catch ex As Exception
                End Try
            Next
        End If

        'Dim PossibleEleList As New List(Of String)
        'PossibleEleList.AddRange(New String() {"blockquote"})

        'For pt As Integer = 0 To PossibleEleList.Count
        '    Dim ParagraphList As XmlNodeList = xmlEpub.SelectNodes("//para")
        '    If ((ParagraphList IsNot Nothing) AndAlso (ParagraphList.Count > 0)) Then
        '        For e As Integer = 0 To ParagraphList.Count - 1
        '            If ((ParagraphList(e).NextSibling IsNot Nothing) AndAlso (ParagraphList(e).NextSibling.NodeType = XmlNodeType.Element)) Then
        '                Dim NextNode As XmlNode = ParagraphList(e).NextSibling
        '                If ((From n In PossibleEleList Where String.Compare(n, NextNode.Name, True) = 0 Select n).Any) Then
        '                    ParagraphList(e).AppendChild(NextNode)
        '                End If
        '            End If
        '        Next
        '    End If
        'Next

        Return True
    End Function

    Private Function GetParentList(ByVal CurNode As XmlNode) As XmlNode
        If (CurNode Is Nothing) Then Return Nothing
        If (CurNode.Equals(xmlEpub.DocumentElement)) Then Return Nothing
        If ((String.Compare(CurNode.Name, "itemizedlist", True) = 0) Or (String.Compare(CurNode.Name, "orderedlist", True) = 0)) Then
            Return CurNode
        End If
        Dim retNode As XmlNode = GetParentList(CurNode.ParentNode)
        If (retNode IsNot Nothing) Then
            Return retNode
        End If
        Return Nothing
    End Function

    Private Function InsertSectionLabelSpace() As Boolean
        Dim SectionLbls As XmlNodeList = xmlEpub.SelectNodes("//section//title")
        If ((SectionLbls IsNot Nothing) AndAlso (SectionLbls.Count > 0)) Then
            For s As Integer = 0 To SectionLbls.Count - 1
                Try
                    SectionLbls(s).InnerXml = Regex.Replace(SectionLbls(s).InnerXml, "(^[0-9\.]+)", "$1 ", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                Catch ex As Exception
                End Try
            Next
        End If
        Return True
    End Function

    Private Function LinkHostURL() As Boolean
        Dim UriList As XmlNodeList = xmlEpub.SelectNodes("//uri")
        Dim MoveOutsideList As New List(Of String)
        Dim MoveChar As String = String.Empty
        MoveOutsideList.AddRange(New String() {".", ")", ",", "(", "]", "["})

        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            UriList = xmlEpub.SelectNodes("//bibliomixed//uri[not (parent::link)]")
            If ((UriList IsNot Nothing) AndAlso (UriList.Count > 0)) Then
                For u As Int16 = 0 To UriList.Count - 1
                    Try
                        If (UriList(u).InnerXml.Contains("doi.org")) Then
                            If (UriList(u).ParentNode IsNot Nothing) Then
                                UriList(u).ParentNode.InnerXml = UriList(u).ParentNode.InnerXml.Replace(UriList(u).OuterXml, $"<link xlinkhref=""{UriList(u).InnerText}"">{UriList(u).OuterXml}</link>")
                            End If
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                Next
            End If
        End If

        For ul As Int16 = 0 To 3
            UriList = xmlEpub.SelectNodes("//uri")
            If ((UriList IsNot Nothing) AndAlso (UriList.Count > 0)) Then
                For l As Integer = 0 To UriList.Count - 1
                    Try
                        MoveChar = (From n In MoveOutsideList Where UriList(l).InnerText.EndsWith(n) Select n).FirstOrDefault
                        If (Not String.IsNullOrEmpty(MoveChar)) Then
                            If (UriList(l).ParentNode IsNot Nothing) Then
                                UriList(l).ParentNode.InnerXml = UriList(l).ParentNode.InnerXml.Replace(UriList(l).OuterXml, UriList(l).OuterXml.Replace($"{MoveChar}</uri>", $"</uri>{MoveChar}"))
                            End If
                        End If
                        If (UriList(l).InnerText.EndsWith("&#x003E;")) Then
                            If (UriList(l).ParentNode IsNot Nothing) Then
                                UriList(l).ParentNode.InnerXml = UriList(l).ParentNode.InnerXml.Replace(UriList(l).OuterXml, UriList(l).OuterXml.Replace($"&amp;#x003E;</uri>", $"</uri>&amp;#x003E;"))
                            End If
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                    If (UriList(l).Attributes("xlinkhref") IsNot Nothing) Then
                        MoveChar = (From n In MoveOutsideList Where UriList(l).Attributes("xlinkhref").Value.EndsWith(n) Select n).FirstOrDefault
                        Try
                            UriList(l).Attributes("xlinkhref").Value = UriList(l).Attributes("xlinkhref").Value.TrimEnd(MoveChar)
                        Catch ex As Exception
                        End Try
                    End If

                Next

            End If
        Next

        UriList = xmlEpub.SelectNodes("//uri")
            For l As Integer = 0 To UriList.Count - 1
                Dim doiMat As Match = Regex.Match(UriList(l).InnerText, "(?i)doi:")
                If (doiMat.Success) Then
                    Try
                        If (UriList(l).ParentNode IsNot Nothing) Then
                            UriList(l).ParentNode.InnerXml = UriList(l).ParentNode.InnerXml.Replace(UriList(l).OuterXml, $"{doiMat.Value}{UriList(l).OuterXml.Replace(doiMat.Value, "")}")
                        End If
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                End If
            Next
        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK) Or (DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
            UriList = xmlEpub.SelectNodes("//uri[not (@xlinkhref)]")
            For l As Integer = 0 To UriList.Count - 1

                If (UriList(l).InnerText.Contains("doi.org")) Then
                    Try
                        Dim UrlHost As Uri = New Uri(UriList(l).InnerText)
                        Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "xlinkhref", "")
                        HrefAttrib.Value = UriList(l).InnerText
                        UriList(l).Attributes.Append(HrefAttrib)
                    Catch ex As Exception
                        Continue For
                    End Try
                Else
                    Try
                        Dim UrlHost As Uri = New Uri(UriList(l).InnerText)
                        Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "xlinkhref", "")
                        If ((DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA)) Then
                            HrefAttrib.Value = UriList(l).InnerText
                        Else
                            HrefAttrib.Value = String.Format("{0}://{1}", UrlHost.Scheme, UrlHost.Host)
                        End If

                        UriList(l).Attributes.Append(HrefAttrib)
                    Catch ex As Exception
                        Dim HrefAttrib As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "xlinkhref", "")
                        HrefAttrib.Value = String.Format("http://{0}", UriList(l).InnerText.Split("/")(0).Replace("http", "").Replace("://", ""))
                        UriList(l).Attributes.Append(HrefAttrib)
                        'GBL.DeantaBallon(ex.Message & " - " & UriList(l).InnerText, MessageType.MSGERROR)
                        Continue For
                    End Try
                End If

                'Try
                '    UriList(l).InnerXml = UriList(l).InnerXml.Replace("&amp;", "&amp;#x38;")
                'Catch ex As Exception
                'End Try
            Next

            If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
                UriList = xmlEpub.SelectNodes("//uri[(@xlinkhref)]")
                For l As Integer = 0 To UriList.Count - 1
                    If (UriList(l).InnerText.Contains("doi.org")) Then
                        Continue For
                    End If
                    Try
                        Dim UrlHost As Uri = New Uri(IIf(UriList(l).InnerText.StartsWith("http"), UriList(l).InnerText.Replace("&#x00AD;", ""), $"http://{UriList(l).InnerText.Replace("&#x00AD;", "")}"))
                        Dim HrefAttrib As XmlAttribute = UriList(l).Attributes("xlinkhref")
                        HrefAttrib.Value = String.Format("{0}://{1}", UrlHost.Scheme, UrlHost.Host)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                Next
            End If
        End If


#If CONFIG <> "FinalXML" Then
        'URL Validation.
        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            UriList = xmlEpub.SelectNodes("//uri")
            If ((UriList IsNot Nothing) AndAlso (UriList.Count > 0)) Then
                For l As Integer = 0 To UriList.Count - 1
                    Try
                        If (UriList(l).InnerText.Contains("doi")) Then
                            Continue For
                        End If
                        If (UriList(l).InnerText.StartsWith("10.")) Then
                            Continue For
                        End If
                        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
                            Dim Valid As XmlAttribute = xmlEpub.CreateNode(XmlNodeType.Attribute, "valid", "")
                            If (Not DPWord.ClsGeneral.IsValidURL(UriList(l).Attributes("xlinkhref").Value)) Then
                                Valid.Value = "false"
                                GBL.DeantaBallon($"Invalid URL:{UriList(l).Attributes("xlinkhref").Value}", MessageType.MSGERROR)
                            Else
                                Valid.Value = "true"
                            End If
                            UriList(l).Attributes.Append(Valid)
                        Else
                            If (Not DPWord.ClsGeneral.IsValidURL(UriList(l).Attributes("xlinkhref").Value)) Then
                                GBL.DeantaBallon($"Invalid URL:{UriList(l).Attributes("xlinkhref").Value}", MessageType.MSGERROR)
                            End If
                        End If

                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                Next
            End If
        End If

#End If
        UriList = xmlEpub.SelectNodes("//uri")
        If ((UriList IsNot Nothing) AndAlso (UriList.Count > 0)) Then
            For l As Integer = 0 To UriList.Count - 1
                If (UriList(l).InnerText.StartsWith("10.")) Then
                    Try
                        UriList(l).Attributes("xlinkhref").Value = $"http://doi.org/{UriList(l).InnerText}"
                    Catch ex As Exception
                    End Try
                End If
            Next
        End If

        Dim Dedis As XmlNodeList = xmlEpub.SelectNodes("//dedication/para")
        If ((Dedis IsNot Nothing) AndAlso (Dedis.Count > 0)) Then
            Dim FirstAbb As XmlAttribute = xmlEpub.CreateAttribute("first")
            FirstAbb.Value = "true"
            Dedis(0).Attributes.Append(FirstAbb)
        End If

        Dim Phrases As XmlNodeList = xmlEpub.SelectNodes("//biblioset[@role='publisher']/phrase")
        If ((Phrases IsNot Nothing) AndAlso (Phrases.Count > 0)) Then
            For p As Int16 = 0 To Phrases.Count - 1
                If ((Phrases(p).InnerText.ToLower().Contains("by crc press")) Or (Phrases(p).InnerText.ToLower().Contains("by routledge"))) Then
                    Dim FirstAbb As XmlAttribute = xmlEpub.CreateAttribute("first")
                    FirstAbb.Value = "true"
                    Phrases(p).Attributes.Append(FirstAbb)
                    Exit For
                End If
            Next
        End If

        Return True
    End Function

    Private Function RemoveDuplicateInfo()
        Dim MaxCount As Int16 = 0
        Dim InfoList As XmlNodeList = xmlEpub.SelectNodes("//info/info")

        If ((InfoList IsNot Nothing) AndAlso (InfoList.Count > 0)) Then
            MaxCount = InfoList.Count * 2
            While ((InfoList IsNot Nothing) AndAlso (InfoList.Count > 0))
                If (MaxCount = 0) Then
                    Exit While
                End If
                Try
                    If (InfoList(0).ParentNode IsNot Nothing) Then
                        InfoList(0).ParentNode.InnerXml = InfoList(0).ParentNode.OuterXml.Replace(InfoList(0).OuterXml, InfoList(0).InnerXml)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                InfoList = xmlEpub.SelectNodes("//info/info")
                MaxCount = MaxCount - 1
            End While
        End If
        Return True
    End Function


    Private Sub ChapterLevelLink()
        'If ((Regex.Match(ChapNode.InnerXml, "((chapter(?:s)?[\s])([\d]+)(?:[\s]+(and)[\s]+([\d]+))?)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success)) Then
        Dim LinkedChapters As New List(Of String)
        Dim ParaXml As String = String.Empty
        Dim OrgXML As String = String.Empty
        Dim ParaNode As XmlNode = Nothing
        Dim ParaNodeList As XmlNodeList = xmlEpub.SelectNodes("//para")
        If ((ParaNodeList IsNot Nothing) AndAlso (ParaNodeList.Count > 0)) Then
            For c As Integer = 0 To ParaNodeList.Count - 1
                Try
                    ParaNode = ParaNodeList(c)
                    LinkedChapters.Clear()
                    If (ParaNode IsNot Nothing) Then
                        If ((Regex.Match(ParaNode.InnerXml, "(chapter(?:s)?[\s])([\d]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success)) Then
                            ParaXml = ParaNode.InnerXml
                            OrgXML = ParaNode.InnerXml
                            Dim ChapMatches As MatchCollection = Regex.Matches(ParaXml, "(chapter(?:s)?[\s])([\d]+)", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            For k As Integer = 0 To ChapMatches.Count - 1
                                Dim ChapMat As Match = ChapMatches(k)
                                If (ChapMat.Success) Then
                                    If ((ChapMat.Groups IsNot Nothing) AndAlso (ChapMat.Groups.Count = 3)) Then
                                        If ((LinkedChapters IsNot Nothing) AndAlso (LinkedChapters.Count = 0)) Then
                                            ParaXml = ParaXml.Replace(ChapMat.Value, "<link role=""chapter"" linkend=""c" & ChapMat.Groups(2).Value & """>" & ChapMat.Value & "</link>")
                                        Else
                                            If (Not (From n In LinkedChapters Where String.Compare(n, ChapMat.Value, True) = 0 Select n).Any) Then
                                                ParaXml = ParaXml.Replace(ChapMat.Value, "<link role=""chapter"" linkend=""c" & ChapMat.Groups(2).Value & """>" & ChapMat.Value & "</link>")
                                            End If
                                        End If
                                    End If
                                    If (Not LinkedChapters.Contains(ChapMat.Value)) Then
                                        LinkedChapters.Add(ChapMat.Value)
                                    End If
                                End If
                            Next
                            Try
                                ParaNode.InnerXml = ParaXml
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message & ParaXml, MessageType.MSGERROR)
                                ParaNode.InnerXml = OrgXML
                            End Try
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
    End Sub

    Private Function GetChildNode(ByVal ParNode As XmlNode, ByVal ChildName As String) As XmlNode
        If (String.Compare(ParNode.Name, ChildName, True) = 0) Then
            Return ParNode
        End If
        If ((ParNode IsNot Nothing) AndAlso (ParNode.ChildNodes IsNot Nothing) AndAlso (ParNode.ChildNodes.Count > 0)) Then
            For ch As Integer = 0 To ParNode.ChildNodes.Count - 1
                Dim ChNode As XmlNode = GetChildNode(ParNode.ChildNodes(ch), ChildName)
                If (ChNode IsNot Nothing) Then
                    Return ChNode
                End If
            Next
        End If
        Return Nothing
    End Function
End Class


Public Class PageNumData
    Public Property PageNum As String = String.Empty
    Public Property ChapterNum As String = String.Empty
End Class

Public Class PageNumXpathData
    Public Property PageNum As String = String.Empty
    Public Property Xpath As String = String.Empty
    Public Property XHtmlPageName As String = String.Empty
End Class

Public Class FloatCitData
    Public Property CitNode As XmlNode = Nothing
    Public Property Index As Int16 = 0
    Public Property CitText As String = String.Empty
End Class