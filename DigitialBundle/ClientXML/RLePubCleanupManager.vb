Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml

Public Class RLePubCleanupManager

    Private XMLString As String = String.Empty
    Private iChap As Integer = 0, iHead As Integer = 0, iChpSec As Integer = 0

    Private iSec As Integer = 1
    Private iFootnote As Integer = 1

    Private epubISBN As String = String.Empty
    Private Const sMsgTitle = "XML Cleanup"
    Public Property sXMLFilePath As String = String.Empty

    Public Property sXMLFileName As String = String.Empty
    Private bExecuteOnce As Boolean = False
    Public Property FileSequence As New List(Of String)
    Public Property sISBN As String = String.Empty
    Public Property AppPath As String = String.Empty

    Public Property ProjectID As String = String.Empty

    Private iDedication As Integer = 0
    Public bNoError As Boolean = False
    Private bXMLorEpub As Boolean = False
    Public Function MainRLePubXMLPro(sXMLPath As String, Optional bxslExecution As Boolean = False) As Boolean
        Try
            Dim di As DirectoryInfo = New DirectoryInfo(sXMLPath.ToString)
            Dim aryFi() As FileInfo = di.GetFiles("*.xml")
            sXMLFilePath = sXMLPath

            bXMLorEpub = bxslExecution
            epubISBN = Regex.Replace(sISBN, "(\d{3})(\d)(\d{3})(\d{5})(\d)", "$1-$2-$3-$4-$5")
            If Not sXMLFileName.EndsWith(".xml") Then sXMLFileName = sXMLFileName & ".xml"
            If File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.ToString)) AndAlso Not bxslExecution Then
                Dim sText As String = File.ReadAllText(sXMLFilePath & "\" & sXMLFileName)
                Dim sxmlFileName1 As String = "" 'outputfile.
                If Not sxmlFileName1.ToString.ToLower.Contains(".xml") Then sxmlFileName1 = sxmlFileName1 & ".xml"

                If Not XSLPro(sText, Path.Combine(sXMLFilePath, sXMLFileName), bxslExecution) Then Return False
                XMLString = File.ReadAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))
                FinalCleanup(bxslExecution)
                File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), XMLString)

                If File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml"))) Then
                    My.Computer.FileSystem.DeleteFile(Path.Combine(sXMLFilePath, sXMLFileName))
                    My.Computer.FileSystem.RenameFile(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLFileName)
                End If
                GBL.DeantaBallon("xsl has been executed. Please check the file.", MessageType.MSGERROR)
                Return True
            ElseIf File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.ToString)) AndAlso bxslExecution Then
                Dim sText As String = File.ReadAllText(sXMLFilePath & "\" & sXMLFileName)
                XSLPro(sText, Path.Combine(sXMLFilePath, sXMLFileName), True)
                GBL.DeantaBallon("EPUB has been generated. Please check.", MessageType.MSGERROR)
                Return True
            End If
            If Not sXMLFileName.ToString.ToLower.EndsWith(".xml") Then sXMLFileName = sXMLFileName & ".xml"
            ' Merging takes place here...
            Dim sBookInfo As String = "<book xmlns=""http://docbook.org/ns/docbook"" version=""5.0"" xml:id=""b-" & sISBN.ToString & """ xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:lang=""en"" role=""fullText"">"
            Using XMLWrite As StreamWriter = File.CreateText(sXMLFilePath & "\" & sXMLFileName)
                XMLWrite.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
                XMLWrite.WriteLine("<?oxygen SCHSchema=""../../../docbook-mods.sch""?>")
                XMLWrite.WriteLine("<?oxygen RNGSchema=""../../../bloomsbury-mods.rnc"" type=""compact""?>")
                XMLWrite.WriteLine(sBookInfo.ToString)
            End Using
            Dim iChFnCnt As Integer = 0
            Dim sXMLTxt As String = String.Empty
            iSec = 1 : iVal = 0
            Dim iChap As Integer = 0
            Using XMLWrite As StreamWriter = File.AppendText(Path.Combine(sXMLFilePath, sXMLFileName))
                For i = 0 To FileSequence.Count - 1
                    Try
                        iChap = iChap + 1

                        Dim xmlFile As String = Path.Combine(Path.Combine(sXMLFilePath, FileSequence(i)))
                        If (Not File.Exists(xmlFile)) Then
                            GBL.DeantaBallon($"Could not able to find the file: {xmlFile}", MessageType.MSGERROR)
                            Continue For
                        End If


                        XMLString = File.ReadAllText(xmlFile)

                        XMLString = XMLString.Replace("<emphasis role=""entity"">&amp;</emphasis>", "&amp;")
                        XMLString = Regex.Replace(XMLString, " role=""""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        If Regex.IsMatch(XMLString, "(<chapter(?:(?!(-title|>)).)+>)((?:(?:(?!</info>).)+)</author></info>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter(?:(?!(-title|>)).)+>)((?:(?:(?!</info>).)+)</info>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        Else
                            XMLString = Regex.Replace(XMLString, "(<chapter(?:(?!(-title|>)).)+>)((?:(?:(?!</title>).)+)</title>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        End If
                        iChpSec = iChpSec + 1
                        XMLString = Regex.Replace(XMLString, "(<title([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        If (bxslExecution) Then
                            XMLString = Regex.Replace(XMLString, "<equation[^><]+>((?:(?!</equation>).)+)</equation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                            XMLString = Regex.Replace(XMLString, "<inlineequation[^><]+>((?:(?!</inlineequation>).)+)</inlineequation>", AddressOf MathCleanUp, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                            XMLString = Regex.Replace(XMLString, "(<mml:math[^>]*)(>)", "$1 alttext="""" $2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        End If
                        Try
                            XMLString = DocumentCleanUp(XMLString)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try


                        If Not bxslExecution Then
                            Try
                                Dim mtch As MatchCollection = Regex.Matches(XMLString, "<footnote[^><]+>((?:(?!</footnote>).)+)</footnote>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                Dim FootList As List(Of String) = mtch.Cast(Of Match)().Select(Function(m) m.Value).ToList

                                mtch = Regex.Matches(XMLString, "<footnote[^><]+>((?:(?!</footnote>).)+)</footnote>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                XMLString = Regex.Replace(XMLString, "<footnote[^><]+>((?:(?!</footnote>).)+)</footnote>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                                XMLString = Regex.Replace(XMLString, "</chapter>", "<fn-group><title>Notes</title></chapter>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                Dim in1 As Integer = 0
                                For Each mc As Match In mtch
                                    XMLString = Regex.Replace(XMLString, "</chapter>", FootList(in1).ToString & "</chapter>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                                    in1 = in1 + 1
                                Next
                                XMLString = Regex.Replace(XMLString, "</chapter>", "</fn-group></chapter>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        End If
                        XMLString = Regex.Replace(XMLString, "((<biblioid class=""isbn""[^><]+>((?:(?!</biblioid>).)+)</biblioid>)+)+", "<biblioset role=""isbns"" xml:id=""bs-000001"">" & "$1" & "</biblioset>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        Dim mt As Match = Regex.Match(XMLString, "<book([^><]+)?>((?:(?!</book>).)+)</book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        If mt.Success Then sXMLTxt = sXMLTxt & mt.Groups(2).Value.ToString.Trim & Environment.NewLine Else sXMLTxt = sXMLTxt & XMLString
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try

                Next

                If bxslExecution Then
                    sXMLTxt = Regex.Replace(sXMLTxt, "<biblioid class=""isbn""([^><]+)?>((?:(?!</biblioid>).)+)</biblioid>", AddressOf BiblioIdPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    sXMLTxt = Regex.Replace(sXMLTxt, "</biblioid></biblioset>", "</biblioid>" & "<biblioid class=""isbn"" role=""epub"">" & epubISBN & "</biblioid>" & "</biblioset>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    ' To retain bibliomixed attributes in bibliography
                    sXMLTxt = Regex.Replace(sXMLTxt, "<bibliography([^><]+)?>((?:(?!</bibliography>).)+)</bibliography>", AddressOf Bibliomixed, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                End If
                sXMLTxt = Regex.Replace(sXMLTxt, "<chapter([^><]+)?>(((?!</chapter>).)+)</chapter>", AddressOf ChapterIDSeq, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                sXMLTxt = Regex.Replace(sXMLTxt, "(<info[^><]+>)(" & Environment.NewLine & ")?(<section[^><]+>)", "$3$2$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = UpdatePro(sXMLTxt)
                sXMLTxt = Regex.Replace(sXMLTxt, "<refbibliomixed ", "<bibliomixed ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "([^ ])xml:id=", "$1 xml:id=", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'If Not bxslExecution Then
                sXMLTxt = Regex.Replace(sXMLTxt, "<caption([^><]+)?>(((?!</caption>).)+)</caption>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "<footnote([^><]+)?>(((?!</footnote>).)+)</footnote>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "<endnote([^><]+)?>(((?!</endnote>).)+)</endnote>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "(<figure([^><]+)?>(?:(?:(?!</figure>).)+)</figure>)((?:(?!</para>).)+)?</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "(<table([^><]+)?>(?:(?:(?!</table>).)+)</table>)((?:(?!</para>).)+)?</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "</1para>", "</para>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ' End If
                sXMLTxt = Regex.Replace(sXMLTxt, "xml:id="""" ", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                XMLWrite.WriteLine(sXMLTxt.ToString)
                XMLWrite.WriteLine("</book>")
            End Using
            ' XML TandF xsl
            'File.Copy(Path.Combine(sXMLFilePath, sXMLFileName), Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))
            If Not bxslExecution Then
                If Not XSLPro(sXMLTxt, Path.Combine(sXMLFilePath, sXMLFileName), False) Then Exit Try
                XMLString = File.ReadAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))
                FinalCleanup(bxslExecution)
                File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), XMLString)
            Else
                ' EPUB xsl

                Try
                    sXMLTxt = EpubCleanup(sXMLTxt)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try



                Dim eMgr As New RandLePubCleanupManager()
                eMgr.IsbnNum = epubISBN
                File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLTxt)
                Try
                    If (Not eMgr.DoePubCleanUp(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))) Then
                        Return False
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                sXMLTxt = eMgr.GetFinalePubContent()
                File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLTxt)

                'XSLPro(sXMLTxt, Path.Combine(sXMLFilePath, sXMLFileName), True)
            End If
            If File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml"))) Then
                My.Computer.FileSystem.DeleteFile(Path.Combine(sXMLFilePath, sXMLFileName))
                My.Computer.FileSystem.RenameFile(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLFileName)
            End If
            GBL.DeantaBallon("Merge and cleanup has been completed. Please check the xml file.", MessageType.MSGERROR)
            Return True
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        Finally
        End Try
        Return True
    End Function


    Private Function DocumentCleanUp(ByVal XmlContent As String) As String
        Dim XmlTmp As New XmlDocument
        XmlTmp.PreserveWhitespace = True
        XmlContent = Regex.Replace(XmlContent, "<book[^>]*>", "<book>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Try
            XmlTmp.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return XmlContent
        End Try

        Dim PrevCnt As Int32 = 0
        Dim PageTagList As XmlNodeList = XmlTmp.SelectNodes("//book/a[@id]|//info/a[@id]|//part/a[@id]")
        Dim ChapterPartNode As XmlNode = Nothing
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            ChapterPartNode = XmlTmp.SelectSingleNode("//dedication/para[1]|//toc/title|//primary[1]|//chapter/info/title|//acknowledgements/title|//part/title|//preface/title|//index/title|//preface/para[1]")
            If (ChapterPartNode IsNot Nothing) Then
                For pg As Integer = PageTagList.Count - 1 To 0 Step -1
                    ChapterPartNode.PrependChild(PageTagList(pg))
                Next
            End If
        End If

        If (bXMLorEpub) Then
            Dim BackTitles As New List(Of String)
            BackTitles.AddRange(New String() {"bibliography", "index", "about the author", "about the authors", "conclusion"})
            Dim ChapterNodes As XmlNodeList = XmlTmp.SelectNodes("//chapter[@label]")
            If ((ChapterNodes IsNot Nothing) AndAlso (ChapterNodes.Count > 0)) Then
                For c As Int16 = 0 To ChapterNodes.Count - 1
                    Dim TitleNode As XmlNode = ChapterNodes(c).SelectSingleNode(".//title")
                    If ((TitleNode IsNot Nothing) AndAlso (From n In BackTitles Where (String.Compare(TitleNode.InnerText, n, True) = 0) Select n).Any) Then
                        Try
                            ChapterNodes(c).Attributes.Remove(ChapterNodes(c).Attributes("label"))
                        Catch ex As Exception
                        End Try
                    End If
                Next
            End If
        End If

        'move floats
        Dim FloatID As String = String.Empty
        Dim FloatList As XmlNodeList = XmlTmp.SelectNodes("//figure|//table")
        If ((FloatList IsNot Nothing) AndAlso (FloatList.Count > 0)) Then
            For f As Int16 = 0 To FloatList.Count - 1
                Dim Float As XmlNode = FloatList(f)
                Try
                    FloatID = Float.Attributes("xml:id").Value
                Catch ex As Exception
                    FloatID = String.Empty
                End Try
                If (String.IsNullOrEmpty(FloatID)) Then Continue For
                Dim LinkNode As XmlNode = GetFloatNode(XmlTmp, FloatID, Float.Name)
                If ((LinkNode IsNot Nothing) AndAlso (LinkNode.ParentNode IsNot Nothing)) Then
                    LinkNode.ParentNode.AppendChild(Float)
                End If
            Next
        End If

        PageTagList = XmlTmp.SelectNodes("//a[@id]")
        PrevCnt = 0
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            For pg As Integer = 0 To PageTagList.Count - 1
                PrevCnt = 0
                If (PageTagList(pg).PreviousSibling Is Nothing) OrElse (PageTagList(pg).PreviousSibling.NodeType <> XmlNodeType.Element) Then
                    Continue For
                End If
                Dim PrevNode As XmlNode = PageTagList(pg).PreviousSibling
                While (PrevCnt <= 1)
                    If ((PrevNode IsNot Nothing) AndAlso (String.Compare(PrevNode.Name, "title", True) = 0)) Then
                        PrevNode.AppendChild(PageTagList(pg))
                        Exit While
                    End If
                    If (PrevNode IsNot Nothing) Then
                        PrevNode = PrevNode.PreviousSibling
                    Else
                        Exit While
                    End If
                    PrevCnt = PrevCnt + 1
                End While
            Next
        End If

        Dim IncludeList As New List(Of String)

        IncludeList.AddRange(New String() {"title", "chapter", "dedication", "preface"})
        'IncludeList.AddRange(New String() {"title", "chapter", "dedication", "preface"})
        PageTagList = XmlTmp.SelectNodes("//a[@id]")
        PrevCnt = 0
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            For pg As Integer = 0 To PageTagList.Count - 1
                PrevCnt = 0
                If (PageTagList(pg).NextSibling Is Nothing) OrElse (PageTagList(pg).NextSibling.NodeType <> XmlNodeType.Element) Then
                    Continue For
                End If
                Dim PrevNode As XmlNode = PageTagList(pg).NextSibling
                While (PrevCnt <= 4)
                    If ((PrevNode IsNot Nothing) AndAlso ((From n In IncludeList Where (String.Compare(PrevNode.Name, n, True) = 0) Select n).Any)) Then
                        PrevNode.AppendChild(PageTagList(pg))
                        Exit While
                    End If
                    If (PrevNode IsNot Nothing) Then
                        PrevNode = PrevNode.NextSibling
                    Else
                        Exit While
                    End If
                    PrevCnt = PrevCnt + 1
                End While
            Next
        End If

        PageTagList = XmlTmp.SelectNodes("//a[@id]")
        PrevCnt = 0
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            For pg As Integer = 0 To PageTagList.Count - 1
                PrevCnt = 0
                If (PageTagList(pg).NextSibling Is Nothing) OrElse (PageTagList(pg).NextSibling.NodeType <> XmlNodeType.Element) Then
                    Continue For
                End If
                Dim PrevNode As XmlNode = PageTagList(pg).NextSibling
                While (PrevCnt <= 4)
                    If ((PrevNode IsNot Nothing) AndAlso (String.Compare(PrevNode.Name, "para", True) = 0)) Then
                        PrevNode.AppendChild(PageTagList(pg))
                        Exit While
                    End If
                    If (PrevNode IsNot Nothing) Then
                        PrevNode = PrevNode.NextSibling
                    Else
                        Exit While
                    End If
                    PrevCnt = PrevCnt + 1
                End While
            Next
        End If

        Dim PageIds As XmlNodeList = XmlTmp.SelectNodes("//footnote/a[@id]")
        If ((PageIds IsNot Nothing) AndAlso (PageIds.Count > 0)) Then
            For p As Int16 = 0 To PageIds.Count - 1
                Dim ParaNode As XmlNode = PageIds(p).ParentNode.SelectSingleNode(".//para")
                If (ParaNode IsNot Nothing) Then
                    ParaNode.AppendChild(PageIds(p))
                End If
            Next
        End If

        XmlContent = XmlTmp.OuterXml.Replace("&amp;", "&")
        Return XmlContent
    End Function

    Private Function GetFloatNode(ByVal xmlDocTmp As XmlDocument, ByVal FloatID As String, ByVal FloatName As String) As XmlNode
        Dim LinkNode As XmlNode = xmlDocTmp.SelectSingleNode($"//link[@role='{FloatName}'][@linkend='{FloatID}']")
        If (LinkNode IsNot Nothing) Then
            Return LinkNode
        End If
        Return Nothing
    End Function

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
    Private Function MathCleanUp(ByVal EqnMat As Match) As String
        Dim Content As String = EqnMat.Value
        Content = Regex.Replace(Content, "<mml:math([^><]+)?>(((?!</mml:math>).)+)</mml:math>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
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

    Private Function MathCleanUp_old(ByVal EqnMat As Match) As String
        Dim Content As String = EqnMat.Value
        Dim MatCotn As String = Regex.Match(Content, "<mml:math([^><]+)?>(((?!</mml:math>).)+)</mml:math>", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        Dim text As String = "<table class=""equation""><tr><td class=""t-eqn"" id="""">MathXXX</td><td class=""t-eqn1""><p class=""eqn-r"">CaptionXXX</p></td></tr></table>"
        Dim CapMath As Match = Regex.Match(Content, "<caption([^><]+)?>(((?!</caption>).)+)</caption>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If (Not CapMath.Success) Then
            Return EqnMat.Value
        End If
        Dim Caption As String = CapMath.Value.Replace("<caption>", "").Replace("</caption>", "")
        If (Not String.IsNullOrEmpty(Caption.Trim())) Then
            text = text.Replace("MathXXX", MatCotn).Replace("CaptionXXX", Caption)
        Else
            Return EqnMat.Value
        End If
        Return text
    End Function

    Private iChapteridSeq As Integer = 1
    Private Function ChapterIDSeq(m As Match)
        Dim sResult As String = m.Value.ToString
        iSec = 1
        sResult = Regex.Replace(sResult, "(<title([^><]+)?>(((?!</title>).)+)</title>)?(<section([^><]+)?)>", AddressOf SectionPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iFootnote = 1
        sResult = Regex.Replace(sResult, "(<footnote([^><]+)?)>", AddressOf FootnotePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "(<bibliomixed xml:id="")([^""]+)""", "$1ch" & iChapteridSeq & "-$2" & Chr(34), RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iChapteridSeq = iChapteridSeq + 1
        Return sResult
    End Function

    Private Function FootnotePro1(m As Match)
        Dim sresult As String = m.Value.ToString
        If sresult.ToString.Contains("figure") Then
            sresult = Regex.Replace(sresult, "ch\d+\-", "", RegexOptions.IgnoreCase)
            Return sresult
        End If
        If Not sresult.ToString.Contains("rid=""fn") Then Return sresult
        sresult = Regex.Replace(sresult, "rid=""fn", "rid=""ch" & iChpSec & "-fn", RegexOptions.IgnoreCase)
        Return sresult
    End Function

    Private Function FootntSeqPro(m As Match)
        Dim sresult As String = m.Value.ToString
        If Not sresult.ToString.Contains("label=") Then Return sresult
        sresult = m.Groups(1).Value.ToString
        sresult = Regex.Replace(sresult, "label=""([^""]+)""", "label=""" & m.Groups(5).Value.ToString & """")
        Return sresult
    End Function

    Private Function FigureCaptionParaPro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "</para>", "</1para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return sResult
    End Function

    Private Function Bibliomixed(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "<bibliomixed ", "<refbibliomixed ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Sub FinalCleanup(Optional ByVal bXslExec As Boolean = False)
        XMLString = Regex.Replace(XMLString, "xmlns:fo=""http://www.w3.org/1999/XSL/Format"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:d=""http://docbook.org/ns/docbook"" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/"" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/"" xmlns:code=""urn:schemas-test-code""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<a>page_([^<>]+)</a>", "<xref ref-type=""page"" id=""page_$1""/>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ' To retain bibliomixed attributes in bibliography
        XMLString = Regex.Replace(XMLString, "<bibliography([^><]+)?>((?:(?!</bibliography>).)+)</bibliography>", AddressOf Bibliomixed, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = UpdatePro(XMLString)
        XMLString = Regex.Replace(XMLString, "<refbibliomixed ", "<bibliomixed ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<biblioset([^><]+)?>((?:(?!</biblioset>).)+)</biblioset>", AddressOf BibliosetPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "([^ ])xml:id=", "$1 xml:id=", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<?xml version=""1.0""[^><]+>)", "$1" & Environment.NewLine & "<!DOCTYPE book SYSTEM ""TFB.dtd"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        If Not bXslExec Then XMLString = Regex.Replace(XMLString, "<xref [^><]+>", AddressOf FootnotePro1, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<title((?:(?!(-group|>)).)+)>", "<title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<book-part[^><]+>", AddressOf ChapNos, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ' Removing unnecessary text
        XMLString = Regex.Replace(XMLString, "(5.0b-\d+enfullText|\t)", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<sec >|<biblioid>doi10.5040[^><]+</biblioid>|<imagedata>pdfs/[^><]+</imagedata>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<([^><]+) >", "<$1>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<toc([^><]+)?>((?:(?!</toc>).)+)</toc>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        iChapp = 0
        'XMLString = Regex.Replace(XMLString, "<sec([^><]+)?>", AddressOf SecNos, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<book-part  ?book-part-type=""(chapter|part)""><label>(((?!</label>).)+)</label>", AddressOf ChapterPro1, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = Regex.Replace(XMLString, "<book-part([^><]+)?book-part-type=""chapter[^><]+>((?:(?!</book-part>).)+)</book-part>", AddressOf BodyTagInto, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<book-part  ?book-part-type=""part[^><]+>((?:(?!(<book-part|<back)).)+)", AddressOf BodyTagIntroPart, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "([^ ])(book-part-type=)", "$1 $2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "( publication-type=[^><]+)><mixed-citation>", "><mixed-citation$1>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'XMLString = Regex.Replace(XMLString, "(<fn id=[^><]+>)<p>(\d+)( +)?", "$1<label>$2</label><p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<sec([^><]+)?><title([^><]+)?>Notes</title>((?:(?!</sec>).)+)</sec>", "<notes><title>Notes</title>$3</notes>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</disp-quote>(" & Environment.NewLine & ")?<disp-quote([^><]+)?>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'XMLString = Regex.Replace(XMLString, "(<fn([^><]+)?><label>((?:(?!</fn>).)+)</fn>(" & Environment.NewLine & ")?)+", "<fn-group>$1</fn-group>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<notes><title>Notes</title>)((?:(?!</notes>).)+)(</notes>)", "$1<fn-group>$2</fn-group>$3", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</title [^><]+>", "</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</(\w+) [^><]+>", "</$1>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<subtitle[^><]+>", "<subtitle>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<book-part-meta[^><]+>", "<book-part-meta>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</p>(<disp-quote([^><]+)?>(((?!</disp-quote>).)+)</disp-quote>)", "$1</p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</p>(<list([^><]+)?>(((?!</list>).)+)</list>)", "$1</p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<mixed-citation publication-type=""journal"">(((?!</mixed-citation>).)+)</mixed-citation>", AddressOf JnlVolume, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<mixed-citation publication-type=""(journal|book)"">(((?!</mixed-citation>).)+)</mixed-citation>", AddressOf RemoveItalics, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<string-name>(((?!</string-name>).)+)</string-name>", AddressOf EtalReplace, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, """fnch(\d+\-)(\d+"")", Chr(34) & "ch$1fn$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)


        Dim mtt1 As Match = Regex.Match(XMLString, "<phrase>((?:(?!</phrase>).)+)</phrase>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If mtt1.Success Then
            XMLString = Regex.Replace(XMLString, mtt1.Value.ToString, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            XMLString = Regex.Replace(XMLString, "(<imprint-meta>)", "$1<imprint-text type=""ImprintStatement"">" & mtt1.Groups(1).Value.ToString & "</imprint-text>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        XMLString = Regex.Replace(XMLString, "<imprint-text>((?:(?!</imprint-text>).)+)</imprint-text>", AddressOf ImprintPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<fig ([^><]+)?>(((?!</fig>).)+)</fig>", AddressOf FigureChangePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<sec([^><])+>(<title>(<[^><]+>)?preface</title>)", "<sec sec-type=""fm-chapter"">$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<book-part-meta><title-group><title([^><])?>(((?!</title>).)+)</title>)", "$1<alt-title alt-title-type=""running-head-verso""></alt-title><alt-title alt-title-type=""running-head-recto""></alt-title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = Regex.Replace(XMLString, "<graphic[^><]+>", AddressOf GraphicPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "&#x2003;", " ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<book-part[^><]+>((?:(?!</book-part>).)+)</book-part>", AddressOf SecidGeneration, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = Regex.Replace(XMLString, "<term[^><]+>", "<term>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</term>(</index-entry>)", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<term>(((?!<nav-pointer-group>).)+)<nav-pointer-group>", "<term>$1</term><nav-pointer-group>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = Regex.Replace(XMLString, "(<xref ref-type=""page[^><]+>)<p>", "<p>$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</p>(<xref ref-type=""page[^><]+>)", "$1</p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<speaker><italic>(((?!</italic>).)+)</italic>", "<speaker>$1</speaker><p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "</speaker></speech>", "</p></speech>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<sec([^><]+)?>)<title>((<(b|bold|i|italic)>)+)?(((\d+\.)+)?\d+)( |&#x\d+;)?", "$1<label>$6</label><title>$3", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<title>( )+", "<title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "( )+(</[^><]+>)", "$2$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<[^><]+>)( )+", "$1$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim sRepTxt As String = "xmlns:fo=~http://www.w3.org/1999/XSL/Format~ xmlns:xlink=~http://www.w3.org/1999/xlink~ xmlns:mml=~http://www.w3.org/1998/Math/MathML~ " &
            "xmlns:msxsl=~urn:schemas-microsoft-com:xslt~ xmlns:d=~http://docbook.org/ns/docbook~ xmlns:aid=~http://ns.adobe.com/AdobeInDesign/4.0/~ xmlns:aid5=~" &
            "http://ns.adobe.com/AdobeInDesign/5.0/~ xmlns:code=~urn:schemas-test-code~"
        XMLString = Regex.Replace(XMLString, sRepTxt.ToString.Replace("~", Chr(34)), "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(  )+", " ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<attrib>(?:(?:(?!</attrib>).)+)</attrib>)(<graphic([^><]+)?>)", "$2$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(doi:?)<uri(?:[^><]+)?>((?:(?!</uri>).)+)</uri>", "$1$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<given-names>)(\S+)(</given-names>)(\.)", "$1$2$4$3", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<given-names>)([A-z])(\.)(-)([A-z])(\.)(</given-names>)", "$1$2$3 $5$6$7", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<string-name>)(<surname>)(\S+)(</surname>, )(<given-names>)(\S+)(</given-names>)(\s)(<surname>)(\S+)(</surname>)(</string-name>)", "$1$2$3$4$5$6 $10$7$12", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "(<string-name>)(<surname>)(([A-z+])(\.))+(\s)?(\S+)(</surname>)(</string-name>)", "$1<given-name>$3</given-name> $2$7$8$9", RegexOptions.IgnoreCase Or RegexOptions.Singleline)


    End Sub
    Private iChappp As Integer = 0
    Private iSe As Integer = 1
    Private Function SecidGeneration(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim mtch As MatchCollection = Regex.Matches(sResult, "<sec[^><]+>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iChappp = iChappp + 1
        iSe = 1
        sResult = Regex.Replace(sResult, "(<sec id="")ch\-(\d+)\-sec\-(\d+)([^><]+>)", AddressOf SecidGenerationNew, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function EtalReplace(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim sMt As Match = Regex.Match(sResult, "<etal>(((?!</etal>).)+)</etal>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If Not sMt.Success Then Return sResult
        sResult = sResult.Replace(sMt.Value.ToString, "")
        Return sResult & sMt.Value.ToString
    End Function

    Private Function SecidGenerationNew(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = m.Groups(1).Value.ToString & "sec" & iChappp & "_" & iSe & m.Groups(4).Value.ToString
        iSe = iSe + 1
        Return sResult
    End Function

    Private Function GraphicPro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "\.(tif|jpg|jpeg|gif)", """ mime-subtype=""$1", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, " xmlns:xlink=""http://www.w3.org/1999/xlink""", " ", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, "<graphic ", "<graphic xmlns:xlink=""http://www.w3.org/1999/xlink"" ", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, "href=""([^\.>]+)\.", " xlink:href=""$1", RegexOptions.IgnoreCase)
        Return sResult
    End Function

    Private Function JnlVolume(m As Match)
        If m.Value.ToString.Contains("<volume>") Then Return m.Value.ToString
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "publication-type=""journal""", "publication-type=""other""", RegexOptions.IgnoreCase)
        Return sResult
    End Function

    Private Function RemoveItalics(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "(<source>)<italic>|</italic>(</source>)", "$1$2", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, "\b(et ?al\.)", "<etal>$1</etal>", RegexOptions.IgnoreCase)
        Return sResult
    End Function

    Private Function FigureChangePro(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim mt As Match = Regex.Match(sResult, "<graphic[^><]+>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If mt.Success Then
            Dim sGraphic As String = mt.Value.ToString
            sResult = Regex.Replace(sResult, sGraphic, "")
            sGraphic = Regex.Replace(sGraphic, "mime-subtype=""tif"" ", "")
            sGraphic = Regex.Replace(sGraphic, "f(\d+)\.(\d+)", "fig$1_$2.tif", RegexOptions.IgnoreCase)
            sResult = Regex.Replace(sResult, "</fig>", sGraphic & "</fig>")
            sResult = Regex.Replace(sResult, "<p>(((?!</p>).)+)</p>", "<caption><title>$1</title></caption>")
        End If
        Return sResult
    End Function

    Private Function ImprintPro(m As Match)
        Dim sResult As String = m.Value.ToString
        If sResult.ToLower.Contains("the right of scott wisor") Then
            sResult = Regex.Replace(sResult, "<imprint-text>", "<imprint-text type=""MoralRightsAssertion"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf sResult.ToLower.Contains("all rights reserved") Then
            sResult = Regex.Replace(sResult, "<imprint-text>", "<imprint-text type=""ReproductionStatement"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf sResult.ToLower.Contains("trademark notice") Then
            sResult = Regex.Replace(sResult, "<imprint-text>", "<imprint-text type=""TrademarkNotice"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf sResult.ToLower.Contains("british library") Then
            sResult = Regex.Replace(sResult, "<imprint-text>", "<imprint-text type=""BritishLibrary"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        Return sResult
    End Function

    Private iChapp As Integer = 1
    Private Function ChapNos(m As Match)
        Dim sInp As String = m.Value.ToString
        Dim mt As Match = Regex.Match(sInp, "(id=""chapter)""", RegexOptions.IgnoreCase)
        If mt.Success Then
            sInp = sInp.Replace(mt.Value.ToString, mt.Groups(1).Value.ToString & iChapp & Chr(34))
            iChapp = iChapp + 1
        End If
        Return sInp
    End Function

    Private Function ChapterPro1(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim mt As Match = Regex.Match(sResult, "<label>((?:(?!(</label>|<title>|<p>)).)+)</label>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If mt.Success Then
            Dim sLbl As String = mt.Groups(1).Value.ToString
            sLbl = Regex.Replace(sLbl, "<[^><]+>|part|chapter", "", RegexOptions.IgnoreCase).Trim
            sResult = Regex.Replace(sResult, "<label>((?:(?!</label>).)+)</label>", "", RegexOptions.IgnoreCase)
            sResult = Regex.Replace(sResult, ">", " id=""" & m.Groups(1).Value.ToString & sLbl & """ book-part-number=""" & sLbl & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        Return sResult
    End Function

    Private Function SecNos(m As Match)
        iChapp = iChapp + 1
        Return "<sec id=""sec_" & iChapp & """ disp-level="""">"
    End Function

    Private Function BodyTagIntroPart(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "(<book-part([^><]+)?>((?:(?!(<p>|<sec>)).)+))", "$1<body>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If sResult.ToString.Contains("<body>") Then sResult = sResult & "</body>"
        Return sResult
    End Function

    Dim iFtnote As String = String.Empty
    Private Function BodyTagInto(m As Match)
        Dim sResult As String = m.Value.ToString
        If sResult.Contains("book-part-type=""part""") Then Return sResult
        sResult = Regex.Replace(sResult, "(<book-part([^><]+)?>((?:(?!(<p>|<sec>)).)+))", "$1<body>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim mt As Match = Regex.Match(sResult, "((</p>|</sec>)((?:(?!(<back>|</book-part>|</p>|</sec>)).)+)?(<back>|</book-part>))", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim mtResult As String = mt.Value.ToString
        If mtResult.StartsWith("</sec>") AndAlso mtResult.Contains("</p>") Then
            mtResult = mtResult.Replace("</p>", "</p></body>")
        ElseIf mtResult.StartsWith("</p>") AndAlso mtResult.Contains("</sec>") Then
            mtResult = mtResult.Replace("</sec>", "</sec></body>")
        ElseIf mtResult.StartsWith("</p>") Then
            mtResult = mtResult.Replace("</p>", "</p></body>")
        ElseIf mtResult.StartsWith("</sec>") Then
            mtResult = mtResult.Replace("</sec>", "</sec></body>")
        Else
            Return sResult
        End If
        sResult = Regex.Replace(sResult, "( )+", " ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim mtt As Match = Regex.Match(sResult, "<book-part book-part-type=""chapter"" id=""chapter(\d+)""", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If mtt.Success Then
            iFtnote = mtt.Groups(1).Value.ToString
            sResult = Regex.Replace(sResult, "(<fn id=""fn)(\d+"">)", AddressOf FootntChapSeq, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        sResult = sResult.Replace(mt.Value.ToString, mtResult)
        Return sResult
    End Function

    Private Function FootntChapSeq(m As Match)
        If String.IsNullOrEmpty(iFtnote) Then Return m.Value.ToString
        Return m.Groups(1).Value.ToString & iFtnote & "_" & m.Groups(2).Value.ToString
    End Function

    Private Function FigurePlacementPro(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim mtch As MatchCollection
        If sResult.ToString.Contains("<figure") Then
            mtch = Regex.Matches(sResult, "<figure([^><]+)?>(?:(?:(?!</figure>).)+)</figure>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf sResult.ToString.Contains("<table") Then
            mtch = Regex.Matches(sResult, "<table([^><]+)?>(?:(?:(?!</table>).)+)</table>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Else
            Return sResult
        End If
        Dim FigureList As List(Of String) = mtch.Cast(Of Match)().Select(Function(m1) m1.Value).ToList
        For Each m2 As Match In mtch
            sResult = sResult.Replace(m2.Value.ToString, "")
        Next
        For Each lst In FigureList
            sResult = sResult & lst.ToString
        Next
        Return sResult
    End Function

    'Private Function FigurePlacementPro(m As Match)
    '    Dim sResult As String = m.Value.ToString
    '    If Not Regex.IsMatch(sResult, "<figure([^><]+)?>") Then Return sResult
    '    Dim mt As MatchCollection = Regex.Matches(sResult, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
    '    Dim sFigs As String = String.Empty
    '    For Each mc As Match In mt
    '        sFigs = sFigs & mc.Value.ToString
    '        sResult = Regex.Replace(sResult, mc.Value.ToString, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
    '    Next
    '    sResult = sResult & sFigs
    '    Return sResult
    'End Function

    Private Function XSLPro(xmlText As String, XMLPath As String, Optional ByVal isEpub As Boolean = False) As Boolean

        If Not isEpub Then
            If Not CheckValidXML(Path.Combine(sXMLFilePath, XMLPath), "TandF") Then Return False
            CallingXSLPro(Path.Combine(sXMLFilePath, XMLPath), "TNF-XML.xsl")
            xmlText = File.ReadAllText(Path.Combine(sXMLFilePath, XMLPath.Replace(".xml", "_xsl.xml")))
            xmlText = Regex.Replace(xmlText, "xmlns:fo=""http://www\.w3\.org/1999/XSL/Format"" xmlns:xlink=""http://www\.w3\.org/1999/xlink"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:d=""http://docbook\.org/ns/docbook"" xmlns:aid=""http://ns\.adobe\.com/AdobeInDesign/4\.0/"" xmlns:aid5=""http://ns\.adobe\.com/AdobeInDesign/5\.0/"" xmlns:code=""urn:schemas-test-code""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            xmlText = UpdatePro(xmlText)
            xmlText = Regex.Replace(xmlText, "<biblioset([^><]+)?>((?:(?!</biblioset>).)+)</biblioset>", AddressOf BibliosetPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            File.WriteAllText(Path.Combine(sXMLFilePath, XMLPath.Replace(".xml", "_xsl.xml")), xmlText)
        Else
            If Not CheckValidXML(Path.Combine(sXMLFilePath, XMLPath), "EPUB") Then Return False
            CallingXSLPro(Path.Combine(sXMLFilePath, XMLPath), "epub.xsl")
            Return False
        End If
        Return True
    End Function

    Private Function BibliosetPro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "</biblioset>", "<biblioid class=""isbn"" role=""epub"">ISBN:1234567891011</biblioid></biblioset>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function BiblioIdPro(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim sTxt As Match = Regex.Match(m.Groups(2).Value.ToString, "\d+[0-9\./\-]+", RegexOptions.IgnoreCase)
        sResult = sResult.Replace(m.Groups(2).Value.ToString, sTxt.Value.ToString.Trim)
        Return sResult
    End Function

    Private Sub TandFXMLPro()

        File.Copy(Path.Combine(sXMLFilePath, sXMLFileName), Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))
        CallingXSLPro(Path.Combine(sXMLFilePath, sXMLFileName), "epub.xsl")
        Dim sXMLTxt As String = File.ReadAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))

        Dim smtchcol As MatchCollection = Regex.Matches(sXMLTxt, "(<chapter(?:(?!(-title|>)).)+>)((?:(?!</chapter>).)+)</chapter>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        For Each mc As Match In smtchcol
            sXMLTxt = sXMLTxt.Replace(smtchcol.Item(1).Value.ToString, "</part>" & smtchcol.Item(1).Value.ToString)
            Exit For
        Next
        sXMLTxt = UpdatePro(sXMLTxt)
        iChap = 50
        sXMLTxt = Regex.Replace(sXMLTxt, "(  )+", " ")
        sXMLTxt = Regex.Replace(sXMLTxt, "([^""])((http:| www\.| mailto:)([^ ><]+))", "$1<link xlink:href=""$2""><uri>$2</uri></link>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<emphasis>(&#x201(9|8);)</emphasis>", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "([^ ])xml:id=", "$1 xml:id=", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLTxt)
    End Sub

    Private Function ChapInfoPro(m As Match)
        Dim sResult As String = m.Value.ToString
        iChap = +1
        sResult = Regex.Replace(sResult, "<info>", "<info xml:id=""ch" & iChap & "-ba-00000" & iChap & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function HardCorePro(m As Match)
        Dim sTxt As String = "</legalnotice>|<biblioid class=~doi~>10.5040/" & sISBN & "</biblioid>|<biblioid class=~other~ otherclass=~schemaVersion~>1</biblioid>|" &
            "<biblioid class=~other~ otherclass=~schematronVersion~>4</biblioid>|<abstract role=~blurb~ xml:id=~ba-blurb1~>|<para></para>|</abstract>|"
        sTxt = sTxt & m.Groups(1).Value.ToString & Environment.NewLine & m.Groups(3).Value.ToString
        sTxt = sTxt & "<part xml:id=~ba-FM-front~ role=~front~>|<info xml:id=~in-0002~>|<title xml:id=~tt-0002~>Front matter</title>|</info>|" &
            "<preface role=~prelims~ xml:id=~b-" & sISBN & "-title~>|<info xml:id=~ba-FM-" & sISBN & "-prelim-id~>|<title xml:id=~ba-FM-" & sISBN & "-prelim-id~>Title Pages</title>|" &
            "<pagenums/>|<mediaobject xml:id=~ba-FM-" & sISBN & "-prelim-id~>|<imageobject xml:id=~ba-FM-" & sISBN & "-prelim-id~>|<imagedata fileref=~pdfs/" & sISBN & ".0001.pdf~ format=~application/pdf~/>|" &
            "</imageobject>|</mediaobject>|</info>|<remark condition=~hidden~>Note that this is a placeholder for the pdf of the prelims and no full text content is included at this point</remark>|" &
            "</preface>|<dedication xml:id=~b-" & sISBN & "-dedi~>|<info xml:id=~bo-id~>|<title outputformat=~e-Only~ xml:id=~tt-003~>Dedication</title>|<pagenums/>|" &
            "<mediaobject xml:id=~ba-000000d4~>|<imageobject xml:id=~ba-000df0005~>|<imagedata fileref=~pdfs/" & sISBN & ".0002.pdf~ format=~application/pdf~/>|" &
            "</imageobject>|</mediaobject>|</info>|<para></para>|</dedication>|<toc xml:id=~b-" & sISBN & "-toc~>|<info xml:id=~in-0006~>|<title xml:id=~tt-00zsdf06~>" &
            "<?page value=~vii~?>Contents</title>|<pagenums>vii</pagenums>|<mediaobject xml:id=~ba-FM-toc-001c~>|<imageobject xml:id=~ba-FM-toc-001d~>|" &
            "<imagedata fileref=~pdfs/9781844864041.0003.pdf~ format=~application/pdf~/>|</imageobject>|</mediaobject>|</info>|</toc>"
        sTxt = sTxt.Replace("|", Environment.NewLine).Replace("~", Chr(34))
        Return sTxt
    End Function

    Private iVal As Integer = 0
    ' Updated on Sep 27, 2016 based on Jaffar request
    Private Function UpdatePro(ByVal sChapterTxt As String) As String
        '
        sChapterTxt = Regex.Replace(sChapterTxt, "(<acknowledgements([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sChapterTxt = Regex.Replace(sChapterTxt, "(<toc([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<abstract([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<address([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliodiv([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliolist([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        If bXMLorEpub Then sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliomixed([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliography([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<para([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<legalnotice([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0

        sChapterTxt = Regex.Replace(sChapterTxt, "(<part([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<partintro([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<preface([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<poetry([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<blockquote([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<caption([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<line([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<linegroup([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<subtitle([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<personblurb([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<printhistory([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<colophon([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<inlinemediaobject([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<imageobject([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<mediaobject([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<informaltable([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<entry([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<informalfigure([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<epigraph([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<sidebar([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<keywordset([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<keyword([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<itermset([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<tfoot([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<glossary([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<glosslist([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<glossentry([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<glossterm([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<glossdef([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<index([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<table([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<chapter((?:(?!(-title|>)).)+)?)>", AddressOf ChapterPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline ' muthu)
        sChapterTxt = Regex.Replace(sChapterTxt, "((<chapter((?:(?!(-title|>)).)+)?)>)((<label([^><]+)?>((?:(?!</label>).)+)</label>)?<title([^><]+)?>((?:(?!</title>).)+)</title>)", AddressOf ChapterPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sChapterTxt = Regex.Replace(sChapterTxt, "(<part([^><]+)?)>", AddressOf PartPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If Not bExecuteOnce Then
            iVal = 0
            sChapterTxt = Regex.Replace(sChapterTxt, "<preface([^><]+)?>((?:(?!</preface>).)+)</preface>", AddressOf PrefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Else
            iVal = 0
            sChapterTxt = Regex.Replace(sChapterTxt, "(<title((?:(?!-group>).)+))>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<abbreviation([^><]+)?)>", AddressOf PrefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        bExecuteOnce = True
        Return sChapterTxt
    End Function

    Private Function ChapterProHC(m As Match)
        Dim sAuthors As Match = Regex.Match(m.Value.ToString, "<author>(.+)</author>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'If Regex.IsMatch(m.Value.ToString, "<author>(.+)</author>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
        'End If
        Dim sResult As String = m.Groups(1).Value.ToString & Environment.NewLine & "<info xml:id=""ch" & iChap & "-ba-00000" & iChap & """>" & Environment.NewLine & m.Groups(3).Value.ToString
        Dim sHardcode As String = "|<pagenums></pagenums>|<biblioid class=~doi~>10.5040/" & sISBN & ".000" & iChap & "</biblioid>|<mediaobject xml:id=~ch" & iChap & "-ba-000000" & iChap & "~>|" &
            "<imageobject xml:id=~ch" & iChap & "-ba-0000005~>|<imagedata fileref=~pdfs/" & sISBN & ".0006.pdf~ format=~application/pdf~></imagedata>|</imageobject>|" &
            "</mediaobject>|"
        If sAuthors.Success Then
            sResult = Regex.Replace(sResult, sAuthors.Value.ToString, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sHardcode = Regex.Replace(sHardcode, "<pagenums>", "<authorgroup>|" & sAuthors.Value.ToString & "</authorgroup>|<pagenums>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        sHardcode = sHardcode.Replace("~", Chr(34)).Replace("|", Environment.NewLine)
        sResult = sResult & sHardcode
        If Regex.IsMatch(sResult, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
            Dim smtch As Match = Regex.Match(sResult, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If smtch.Value.ToString.ToLower.Contains("introduction") Then
                sResult = Regex.Replace(sResult, "-chapter\d+", "-Introduction", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            End If
        End If
        'If iChap = 1 Then sResult = sResult.Replace("<chapter", "</part><chapter")
        sResult = Regex.Replace(sResult, "<info([^><]+)?></info>", "", RegexOptions.IgnoreCase)
        If Not sResult.Contains("</info") Then sResult = sResult & "</info>" & Environment.NewLine
        Return sResult
    End Function

    Private Function PrefacePro(m As Match)
        Dim sInput As String = m.Value.ToString
        Dim sResults As String = String.Empty
        Dim sTxt As String = "<info xml:id=~ba-0000004e~>|<title xml:id=~b-0003g~></title>|<pagenums></pagenums>|<mediaobject xml:id=~ba-0000004f~>|" &
            "<imageobject xml:id=~ba-0000005f~>|<imagedata fileref=~pdfs/" & sISBN & ".0004.pdf~ format=~application/pdf~/>|</imageobject>|</mediaobject>|</info>"
        If Not String.IsNullOrEmpty(sInput) Then
            If Regex.IsMatch(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                Dim smt As Match = Regex.Match(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                If smt.Groups(2).Value.ToString.ToLower.Contains("contributors") Then
                    sResults = "<preface xml:id=""b-" & sISBN & "-contributors"">"
                Else
                    Dim sTit As String = Regex.Replace(smt.Groups(2).Value.ToString, "<emphasis[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</emphasis>", "")
                    sTit = Regex.Replace(sTit, "<a[^>]*/>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    '17-07-2020
                    'sTit = Regex.Replace(sTit, "(<superscript>(.+)</superscript>|<(/)?emphasis>)", "", RegexOptions.IgnoreCase)
                    Select Case sTit.ToLower()
                        Case "about the author"
                            sTit = "about"
                        Case "about the authors"
                            sTit = "about"
                    End Select
                    sResults = "<preface xml:id=""b-" & sISBN & "-" & sTit & Chr(34) & ">"
                End If
                Dim sMtch As Match = Regex.Match(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                If sMtch.Success Then
                    Dim sTit As String = Regex.Replace(sMtch.Groups(2).Value.ToString, "<emphasis[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</emphasis>", "")
                    '17-07-2020
                    'sTit = Regex.Replace(sTit, "(<superscript>(.+)</superscript>|<(/)?emphasis>)", "", RegexOptions.IgnoreCase)
                    sTxt = sTxt.Replace("</title>", sTit & "</title>")
                End If
                sResults = sResults & sTxt.Replace("|", Environment.NewLine).Replace("~", Chr(34))
                'sResults = Regex.Replace(sResults, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                sInput = Regex.Replace(sInput, "<preface([^><]+)?>", "", RegexOptions.IgnoreCase)
                'sResults = sResults & " xml:id=" & Chr(34) & "b-" & sISBN & "-" & sTag & Chr(34) & ">"
                sInput = Regex.Replace(sInput, smt.Value.ToString, sResults.ToString, RegexOptions.IgnoreCase)
            Else
                If (sInput.StartsWith("<abbreviation")) Then
                    sInput = Regex.Replace(sInput, "<abbreviation[^>]*>", "<abbreviation xml:id=""b-" & sISBN & "-abb" & Chr(34) & ">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                End If
            End If
        End If
        Return sInput
    End Function

    Private Function PartPro(m As Match)
        Dim sInput As String = m.Groups(1).Value.ToString
        If Not String.IsNullOrEmpty(sInput) Then
            sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        iVal = iVal + 1
        Return "<part xml:id=""b-" & sISBN & "-part" & iVal & """>"
    End Function

    Private Function ChapterPro(m As Match)
        If m.Value.ToString.Contains("/>") OrElse m.Value.ToString.Contains("chapter-title") Then Return m.Value.ToString
        Dim TmpStr As String = m.Value
        Dim sInput As String = m.Groups(1).Value.ToString
        Dim Title As String = m.Groups(5).Value.ToString
        If Not String.IsNullOrEmpty(sInput) Then
            sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        TmpStr = TmpStr.Replace(sInput, "")
        sInput = sInput.TrimEnd(">")
        If Title.ToString.ToLower.Contains("introduction") Then
            Return sInput & " xml:id=""b-" & sISBN & "-chapter0"">" & TmpStr
        Else
            iVal = iVal + 1
            Return sInput & " xml:id=""b-" & sISBN & "-chapter" & iVal & """>" & TmpStr
        End If
        'Return sInput & " xml:id=""b-" & sISBN & "-chapter" & iVal & """>"
    End Function

    Private Function old_ChapterPro(m As Match)
        If m.Value.ToString.Contains("/>") OrElse m.Value.ToString.Contains("chapter-title") Then Return m.Value.ToString
        Dim sInput As String = m.Groups(1).Value.ToString
        If Not String.IsNullOrEmpty(sInput) Then
            sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        iVal = iVal + 1
        'If Regex.IsMatch(m.Value, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
        '    Dim smtch As Match = Regex.Match(m.Value, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        '    If smtch.Value.ToString.ToLower.Contains("introduction") Then
        '        Return sInput & " xml:id=""b-" & sISBN & "-chapter0"">"
        '    Else
        '        Return sInput & " xml:id=""b-" & sISBN & "-chapter" & iVal & """>"
        '    End If
        'End If
        Return sInput & " xml:id=""b-" & sISBN & "-chapter" & iVal & """>"
    End Function

    Private Function FootnotePro(m As Match)
        If Not m.Value.ToString.EndsWith(">") Then Return m.Value.ToString
        Dim sInput As String = m.Groups(1).Value.ToString
        Dim smt As Match = Regex.Match(sInput, " ?label=""([^""]+)""", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, " ?label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, " ?role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, "xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If String.IsNullOrEmpty(smt.Groups(1).Value.ToString) Then
            sInput = sInput & " role=" & Chr(34) & "end-bk-note" & Chr(34) & " label=" & Chr(34) & iFootnote & Chr(34) & " xml:id=""note" & iFootnote & "-ba-" & String.Format("{0:00000}", iFootnote) & """>"
        Else
            sInput = sInput & " role=" & Chr(34) & "end-bk-note" & Chr(34) & " label=" & Chr(34) & "ch" & iChapteridSeq & "-" & smt.Groups(1).Value.ToString & Chr(34) & " xml:id=""note" & iFootnote & "-ba-" & String.Format("{0:00000}", iFootnote) & """>"
            sInput = sInput & "<label>" & smt.Groups(1).Value.ToString & "</label>"
        End If
        iFootnote += 1
        Return sInput
    End Function

    Private Function SectionPro(m As Match)
        Dim sInput As String = String.Empty
        If Not m.Value.ToString.EndsWith(">") Then Return m.Value.ToString
        Dim Title As String = m.Groups(1).Value.ToString
        If (Not m.Value.Contains("title")) Then
            sInput = m.Groups(0).Value.ToString
        Else
            sInput = m.Groups(5).Value.ToString
        End If
        sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = sInput.Trim().TrimEnd(">")
        'iVal = iVal + 1
        If (Not String.IsNullOrEmpty(Title) AndAlso (Title.ToLower().Contains("introduction"))) Then
            sInput = sInput & " xml:id=" & Chr(34) & "ch-0-sec-" & iSec & Chr(34) & ">"
            iChapteridSeq = iChapteridSeq - 1
        Else
            sInput = sInput & " xml:id=" & Chr(34) & "ch-" & iChapteridSeq & "-sec-" & iSec & Chr(34) & ">"
        End If
        iSec = iSec + 1
        If (Not String.IsNullOrEmpty(m.Groups(1).Value)) Then
            Return $"{m.Groups(1).Value}{sInput}"
        Else
            Return sInput
        End If

    End Function

    Private Function IDGen(m As Match)
        If Not m.Value.ToString.EndsWith(">") OrElse m.Value.ToString.Contains("/>") Then Return m.Value.ToString
        Dim sResults As String = String.Empty
        Dim sInput As String = m.Groups(1).Value.ToString
        sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal += 1
        iVal = String.Format("{0:0000000}", iVal)
        Dim sDigit As String = String.Empty
        If Convert.ToString(iVal).Length = 1 Then
            sDigit = "000000"
        ElseIf Convert.ToString(iVal).Length = 2 Then
            sDigit = "00000"
        ElseIf Convert.ToString(iVal).Length = 3 Then
            sDigit = "0000"
        ElseIf Convert.ToString(iVal).Length = 4 Then
            sDigit = "000"
        ElseIf Convert.ToString(iVal).Length = 5 Then
            sDigit = "00"
        ElseIf Convert.ToString(iVal).Length = 6 Then
            sDigit = "0"
        End If
        If m.Groups(1).Value.ToString.Contains("<para") Then
            sResults = sInput & " xml:id=" & Chr(34) & "pa-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<index") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-index" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliography") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-bib" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossary") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-glossary" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glosslist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "glossl-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossentry") Then
            sResults = sInput & " xml:id=" & Chr(34) & "glosse-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossterm") Then
            sResults = sInput & " xml:id=" & Chr(34) & "glosst-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossdef") Then
            sResults = sInput & " xml:id=" & Chr(34) & "glossd-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliolist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "bibl-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<partintro") Then
            sResults = sInput & " xml:id=" & Chr(34) & "ptint-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<keyword") Then
            sResults = sInput & " xml:id=" & Chr(34) & "key-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<keywordset") Then
            sResults = sInput & " xml:id=" & Chr(34) & "key-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<itemset") Then
            sResults = sInput & " xml:id=" & Chr(34) & "itms-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<tfoot") Then
            sResults = sInput & " xml:id=" & Chr(34) & "tfoot-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<sidebar") Then
            sResults = sInput & " xml:id=" & Chr(34) & "side-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<line") Then
            sResults = sInput & " xml:id=" & Chr(34) & "line-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<linegroup") Then
            sResults = sInput & " xml:id=" & Chr(34) & "lineg-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<abbreviation") Then
            sResults = sInput & " xml:id=" & Chr(34) & "abb-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<epigraph") Then
            sResults = sInput & " xml:id=" & Chr(34) & "epig-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<inlinemediaobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "inlinemedo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<imageobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "imgo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<mediaobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "medo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<informaltable") Then
            sResults = sInput & " xml:id=" & Chr(34) & "infotab-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<poetry") Then
            sResults = sInput & " xml:id=" & Chr(34) & "poet-" & sDigit & iVal & Chr(34) & ">"
            'ElseIf m.Groups(1).Value.ToString.Contains("<entry") Then
            '    sResults = sInput & " xml:id=" & Chr(34) & "entr-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<informalfigure") Then
            sResults = sInput & " xml:id=" & Chr(34) & "infofig-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<colophon") Then
            sResults = sInput & " xml:id=" & Chr(34) & "colph-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliodiv") Then
            sResults = sInput & " xml:id=" & Chr(34) & "bibd-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<address") Then
            sResults = sInput & " xml:id=" & Chr(34) & "adr-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<biblioset") Then
            sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " role=""publisher"" xml:id=" & Chr(34) & "bibs-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliomixed") Then
            sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " role=""series"" xml:id=" & Chr(34) & "bibm-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<abstract") Then
            sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " role=""blurb"" xml:id=" & Chr(34) & "abs-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<legalnotice") Then
            sResults = sInput & " xml:id=" & Chr(34) & "ba-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<preface") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<acknowledgements") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-ack" & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<toc") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-toc" & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<part") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-part" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<preface") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-preface" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<blockquote") Then
            sResults = sInput & " xml:id=" & Chr(34) & "bloq-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<caption") Then
            sResults = sInput & " xml:id=" & Chr(34) & "capt-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<title") Then
            If Not m.Groups(1).Value.ToString.Contains("<title-group") Then sResults = sInput & " xml:id=" & Chr(34) & "ti-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<printhistory") Then
            sResults = sInput & " xml:id=" & Chr(34) & "prih-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<subtitle") Then
            sResults = sInput & " xml:id=" & Chr(34) & "suti-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<table") Then
            sResults = "<table label=""" & iVal & """ frame=""all""" & "xml:id=" & Chr(34) & "tab-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<personblurb") Then
            sResults = sInput & " xml:id=" & Chr(34) & "pbl-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<cover") Then
            sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " role=""series"" xml:id=" & Chr(34) & "co-" & sDigit & iVal & Chr(34) & ">"
        End If

        Return sResults
    End Function

    Private Function CleanupPro(sXMLContent As String) As String
        Dim sTxt2Remove As String = "<book-meta xmlns:fo=~http://www.w3.org/1999/XSL/Format~ xmlns:xlink=~http://www.w3.org/1999/xlink~ xmlns:msxsl=~urn:schemas-microsoft-com:xslt~ " &
                "xmlns:d=~http://docbook.org/ns/docbook~ xmlns:aid=~http://ns.adobe.com/AdobeInDesign/4.0/~ xmlns:aid5=~http://ns.adobe.com/AdobeInDesign/5.0/~ xmlns:code=~urn:schemas-test-code~>"
        sXMLContent = Regex.Replace(sXMLContent, sTxt2Remove.ToString.Replace("~", Chr(34).ToString), "<book-meta>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sTxt2Remove = "<!DOCTYPE book SYSTEM ""\\fsdeanta\TechRelease\Accounts\Common\DeantaComposer\Publish\extra\DTD\TFB-DTD\TFB\TFB.dtd"">"
        If Regex.IsMatch(sXMLContent, "<!DOCTYPE book SYSTEM[^><]+><book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
            sXMLContent = Regex.Replace(sXMLContent, "<!DOCTYPE book SYSTEM[^><]+><book>", sTxt2Remove & "<book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Else
            sXMLContent = Regex.Replace(sXMLContent, "<book>", sTxt2Remove & "<book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        sXMLContent = Regex.Replace(sXMLContent, "<(ext-link|graphic)( [^><]+>)", AddressOf CiteAttribute, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "(<xref rid=""F[0-9]+"" ref-type="")figure("">)", "$1fig$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        sXMLContent = Regex.Replace(sXMLContent, "<fig(?:ure)?([^><]+)?>((?:(?!</fig(ure)?>).)+)</fig(ure)?>", AddressOf FigurePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sXMLContent = Regex.Replace(sXMLContent, "</book-meta>((?:(?!</body>).)+)</body>", "</book-meta><book-front>$1</book-front></body>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sXMLContent = Regex.Replace(sXMLContent, "<fpage>((?:(?!</fpage>).)+)</fpage>", AddressOf PageRangePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = sXMLContent.Replace(" xmlns:fo=""http://www.w3.org/1999/XSL/Format""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:xlink=""http://www.w3.org/1999/xlink""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:msxsl=""urn:schemas-microsoft-com:xslt""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:d=""http://docbook.org/ns/docbook""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/""", "")
        sXMLContent = sXMLContent.Replace(" xmlns:code=""urn:schemas-test-code""", "")
        Return sXMLContent
    End Function

    Private Function PageRangePro(m As Match)
        Dim sPage As String = m.Value
        Dim smtch As Match = Regex.Match(sPage, "(\d+)( ?" & Chr(45).ToString & "|" & ChrW(8212).ToString & "|" & ChrW(8211).ToString & " ?)(\d+)", RegexOptions.IgnoreCase)
        If smtch.Success Then
            sPage = "<fpage>" & smtch.Groups(1).Value.ToString & "</fpage>" & smtch.Groups(2).Value.ToString & "<lpage>" & smtch.Groups(2).Value.ToString & "</fpage>"
        End If
        Return sPage
    End Function

    Private Function FigurePro(m As Match)
        Dim sGraphic As String = m.Value.ToString
        Dim sMtch As Match = Regex.Match(sGraphic, "<graphic([^><]+)?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If sMtch.Success Then
            sGraphic = Regex.Replace(sGraphic, sMtch.Value, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sGraphic = Regex.Replace(sGraphic, "</fig>", sMtch.Value & "</fig>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        Return sGraphic
    End Function

    Private Function CiteAttribute(m As Match) As String
        If Not m.Value.ToString.Contains("http://www.w3.org/1999/xlink") Then
            Return "<" & m.Groups(1).Value.ToString & " xmlns:xlink=""http://www.w3.org/1999/xlink""" & m.Groups(2).Value.ToString
        Else
            Return m.Value
        End If

    End Function

    Private Function CheckValidXML(sFilePath As String, Optional sEpub As String = "") As Boolean
        Try
            Dim m_xmld As New XmlDocument
            m_xmld.Load(sFilePath)
            Return True
        Catch ex As Exception
            GBL.DeantaBallon("The XML file is not well formed and hence " & sEpub & " xsl has not been executed. Please check.", MessageType.MSGERROR)
            Return False
        End Try
    End Function

    ' Execute XSL file code from Muthu
    Private Sub CallingXSLPro(sXMLFile As String, ByVal xslName As String)
        Dim OutputPath As String = String.Empty
        Dim BatFileContent As String = String.Empty
        OutputPath = Path.Combine(Path.GetTempPath, Environment.UserName)
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
        'If File.Exists(Path.Combine(AppPath, "TNF-XML.xsl")) Then File.Copy(Path.Combine(AppPath, "TNF-XML.xsl"), OutputPath & "\TNF-XML.xsl") 
        If File.Exists(sXMLFile) Then File.Copy(sXMLFile, Path.Combine(OutputPath, Path.GetFileName(sXMLFile)))
        If File.Exists(Path.Combine(AppPath, "ent.xsl")) Then File.Copy(Path.Combine(AppPath, "ent.xsl"), OutputPath & "\ent.xsl")

        BatFileContent = "java -jar """ & Path.GetFileName(Path.Combine(AppPath, "saxon9.jar")) & """ -s:""" & Path.GetFileName(sXMLFile) & """ -xsl:""" &
                                                           Path.GetFileName(Path.Combine(AppPath, xslName)) & """ -o:""" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml" & """"
        'File.Copy(Path.Combine(sXMLFilePath, sXMLFile), Path.Combine(sXMLFilePath, sXMLFile.Replace(".xml", "_xsl.xml")))
        If (Not CreateBatAndRunFile(BatFileContent, OutputPath)) Then
            'GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
            GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
        End If
        If File.Exists(OutputPath & "\" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml") Then
            File.Copy(OutputPath & "\" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml", Path.Combine(Path.GetDirectoryName(sXMLFile), Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml"), True)
            If Directory.Exists(Path.Combine(OutputPath, sISBN)) Then
                My.Computer.FileSystem.CopyDirectory(Path.Combine(OutputPath, sISBN), Path.Combine(sXMLFilePath, sISBN), True)
            ElseIf Directory.Exists(Path.Combine(OutputPath, "1234567890000")) Then
                My.Computer.FileSystem.CopyDirectory(Path.Combine(OutputPath, "1234567890000"), Path.Combine(sXMLFilePath, sISBN), True)
            End If
        End If
    End Sub

    Private Function EPubBasicDomCleanup(ByVal XmlCtr As String) As String
        Dim xmlTmp As New XmlDocument
        Dim PageTagList As XmlNodeList = Nothing
        Dim ChapterPartNode As XmlNode = Nothing
        Dim PrevCnt As Int16 = 0
        xmlTmp.PreserveWhitespace = True

        Dim BookStr As String = Regex.Match(XmlCtr, "<book([^>]*)>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
        XmlCtr = Regex.Replace(XmlCtr, "<book[^>]*>", "<book>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        Try
            xmlTmp.LoadXml(XmlCtr.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Return True


    End Function


    ' Epub Cleaning
    Private Function EpubCleanup(sContent As String)
        sContent = Regex.Replace(sContent, "</?info([^><]+)?>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "<footnote([^><]+)?>(((?!</footnote>).)+)</footnote>", AddressOf FootnoteInfo, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "<xref [^><]+><sup>(((?!</sup>).)+)</sup></xref>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        '17-07-2020
        'sContent = Regex.Replace(sContent, "(<chapter[^><]+>)((?:(?!<para([^><]+)?>).)+)(<para([^><]+)?>)", AddressOf ChapterInfo, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sContent = Regex.Replace(sContent, "(<section[^><]+>)((?:(?!<para([^><]+)?>).)+)(<para([^><]+)?>)", AddressOf ChapterInfo, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        sContent = Regex.Replace(sContent, "(<cover([^><]+)?>)(<bibliolist([^><]+)?>)?(<bibliomixed([^><]+)?>)?([^><]+)(<para([^><]+)?>)", "$1$3$5<para xml:id=""pa-000000001"">$7</para>$8", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        '17-07-2020
        'sContent = Regex.Replace(sContent, "(<title([^><]+)?>(?:(?:(?!(<info>|<cover[^><]+>)).)+))(<info><cover[^><]+>|<cover[^><]+>)(<bibliolist([^><]+)?>)?(<bibliomixed([^><]+)?>)?", AddressOf TitleBib, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "((<label>(((?!</label>).)+)</label>)?<title([^><]+)?>(((?!</title>).)+)</title>((<subtitle[^><]+>(((?!</subtitle>).)+)</subtitle>|<footnote([^><]+)?>(((?!</footnote>).)+)</footnote>|(<authorgroup>(((?!</authorgroup>).)+)</authorgroup>|<author>(((?!</author>).)+)</author>))+)?)",
                                 AddressOf InfoTags, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        sContent = Regex.Replace(sContent, "<a id=""page_([^""]+)""([^><]+)>", "<?page value=""$1""?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "( )+", " ", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        '21-09-2020
        'sContent = Regex.Replace(sContent, "</cover>", "</cover></info>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "<info([^><]+)?>(((?!</info>).)+)</info>", AddressOf InfoRepeat, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sContent = Regex.Replace(sContent, "<bibliography([^><]+)?>(((?!</bibliography>).)+)</bibliography>", AddressOf InfoRepeatinBib, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim PreContent As String = "<?xml version=~1.0~ encoding=~UTF-8~ standalone=~yes~?><?oxygen SCHSchema=~docbook-mods.sch~?><?oxygen RNGSchema=~bloomsbury-mods.rnc~ type=~compact~?>" &
            "<book xmlns=~http://docbook.org/ns/docbook~ version=~5.0~ xml:id=~b-9781474279437~ xmlns:xlink=~http://www.w3.org/1999/xlink~ xml:lang=~en~ role=~fullText~ xmlns:mml=~http://www.w3.org/1998/Math/MathML~>" & Environment.NewLine
        Return PreContent.Replace("~", Chr(34).ToString) & sContent & "</book>"
    End Function

    Private iXMLid As Integer = 0

    Private Function InfoRepeatinBib(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "</?info([^><]+)?>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function
    Private Function InfoRepeat(m As Match)
        Dim sResult As String = m.Value.ToString
        If Regex.Matches(sResult, "<info([^><]+)?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Count > 1 Then
            sResult = m.Groups(2).Value.ToString
            sResult = Regex.Replace(sResult, "<info([^><]+)?>|</info>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            Return "<info" & m.Groups(1).Value.ToString & ">" & sResult
        End If
        Return sResult
    End Function

    Private Function InfoTags(m As Match)
        iXMLid = iXMLid + 1
        Return "<info xml:id=""ba-000000" & iXMLid & "e"">" & m.Value.ToString & "</info>"
    End Function

    Private Function AttriWeb(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = sResult.Replace("http", "h123ttp").Replace("www", "w123ww")
        Return sResult
    End Function

    Private Function WebLinkPro(m As Match)
        If Regex.IsMatch(m.Value.ToString, "http|www") Then Return m.Value.ToString
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "[^ ><]+(\.com)", "<link xlink:href=""http://" & m.Value.ToString & """><uri>" & m.Value.ToString & "</uri></link>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function ChapterInfo(m As Match)
        Dim sResult As String = m.Value.ToString
        '17-07-2020
        'sResult = Regex.Replace(sResult, "<chapter ([^><]+)?(xml:id=""[^""]+"")([^><]+)?><label>((?:(?!</label>).)+)</label>", "<chapter $2 label=""$4"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        If Regex.Match(sResult, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Groups(2).Value.ToString.ToLower.Equals("introduction") Then
            sResult = Regex.Replace(sResult, "(xml:id=""[^""]+\-)(chapter(\d+)?)"">", "$1intro"">", RegexOptions.IgnoreCase)
        End If
        Dim st As Match = Regex.Match(sResult, "<label>((?:(?!</label>).)+)</label>", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, " label=""[^""]+""", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        If st.Success Then
            sResult = Regex.Replace(sResult, "(xml:id=""[^""]+\-)(chapter(\d+)?)"">", " label=""" & st.Groups(1).Value.ToString & """ $1" & st.Groups(1).Value.ToString & """>", RegexOptions.IgnoreCase)
            sResult = Regex.Replace(sResult, st.Value.ToString & "(( )+)?", "", RegexOptions.IgnoreCase)
        End If
        If Regex.IsMatch(sResult, "<section", RegexOptions.IgnoreCase) Then
            sResult = Regex.Replace(sResult, "disp-level=""[^""]+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        Return sResult
    End Function

    Private Function FootnoteInfo(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "<footnote([^><]+)? (role=""[^""]+"")([^><]+)?(xml:id=""[^""]+"")([^><]+)?><label>((?:(?!</label>).)+)</label>", "<footnote $2 label=""$6"" $4>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sResult = Regex.Replace(sResult, "<para([^><]+)?>", "<ppara$1>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sResult = Regex.Replace(sResult, "</para>", "</ppara>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function TitleBib(m As Match) ' >(((?!>).)+)</para>
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "<info([^><]+)?>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = "<info xml:id=""ti-000001"">" & m.Groups(1).Value.ToString & m.Groups(4).Value.ToString & m.Groups(5).Value.ToString & m.Groups(7).Value.ToString
        sResult = Regex.Replace(sResult, "<info>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "<para([^><]+)?>", "<bibliomisc role=""description"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "</para>", "</bibliomisc>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim dt As Match = Regex.Match(sResult, "(<cover([^><]+)?>)(<bibliolist([^><]+)?>)?(<bibliomixed([^><]+)?>)?", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, dt.Value.ToString, "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "(<info([^><]+)?>)", "$1" & dt.Value.ToString, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

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
            'Threading.Thread.Sleep(2000)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class


Public Class RandLePubCleanupManager

    Dim xmlContent As String = String.Empty
    Dim xmlePubDoc As New XmlDocument
    Public Property IsbnNum As String = String.Empty
    Dim NameSpaceManager As XmlNamespaceManager
    Public Sub New()
        Me.xmlContent = String.Empty
    End Sub

    Public Function DoePubCleanUp(ByVal FinalXMLFile As String) As Boolean
        xmlePubDoc.XmlResolver = Nothing
        Dim TmpXmlCont As String = String.Empty
        xmlePubDoc.PreserveWhitespace = True

        TmpXmlCont = File.ReadAllText(FinalXMLFile)
        TmpXmlCont = TmpXmlCont.Replace(" xmlns=""http://docbook.org/ns/docbook""", "")
        TmpXmlCont = TmpXmlCont.Replace(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")

        TmpXmlCont = Regex.Replace(TmpXmlCont, "<dialogue[^>]*>(((?!<\/dialogue>).)*)</dialogue>", Function(mt As Match)
                                                                                                       Dim TmpDialogue As String = mt.Value
                                                                                                       TmpDialogue = TmpDialogue.Replace("<entity>" & vbTab & "</entity>", " ")
                                                                                                       TmpDialogue = TmpDialogue.Replace("<line role=""noindent"">", "<line>")
                                                                                                       TmpDialogue = TmpDialogue.Replace(vbTab, "")
                                                                                                       TmpDialogue = TmpDialogue.Replace("</speaker> <line", "</speaker><line")
                                                                                                       TmpDialogue = TmpDialogue.Replace("<line", "<speech><line").Replace("</line>", "</line></speech>")
                                                                                                       TmpDialogue = TmpDialogue.Replace("<speaker>", "<speech><speaker>").Replace("</speaker>", "</speaker></speech>")
                                                                                                       TmpDialogue = TmpDialogue.Replace("</speech><speech><line", "<line").Replace("</speaker></speech><speech>", "</speaker>")
                                                                                                       Return TmpDialogue
                                                                                                   End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)


        While (TmpXmlCont.Contains(vbCrLf & vbCrLf))
            TmpXmlCont = TmpXmlCont.Replace(vbCrLf & vbCrLf, "")
        End While
        While (TmpXmlCont.Contains(vbCrLf))
            TmpXmlCont = TmpXmlCont.Replace(vbCrLf, "")
        End While
        While (TmpXmlCont.Contains(vbLf))
            TmpXmlCont = TmpXmlCont.Replace(vbLf, "")
        End While
        While (TmpXmlCont.Contains(vbCr))
            TmpXmlCont = TmpXmlCont.Replace(vbCr, "")
        End While
        Try
            xmlePubDoc.LoadXml(TmpXmlCont.Replace("&", "&amp;"))
        Catch ex As Exception
            File.WriteAllText(FinalXMLFile, TmpXmlCont)
            GBL.DeantaBallon(ex.Message & vbNewLine & vbNewLine & "Please wellform the [final_xsl.xml] file and try again.", True)
            Return False
        End Try


        'NameSpaceManager = New System.Xml.XmlNamespaceManager(xmlePubDoc.NameTable)
        'NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        'NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        'NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        'NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        'NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")

        Try
            Dim ParaNodelist As XmlNodeList = xmlePubDoc.SelectNodes("//chapter/para[1]|//preface/para[1]")
            If ((ParaNodelist IsNot Nothing) AndAlso (ParaNodelist.Count > 0)) Then
                For p As Int16 = 0 To ParaNodelist.Count - 1
                    Dim roleAt As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "poss", "")
                    roleAt.Value = "first"
                    ParaNodelist(p).Attributes.Append(roleAt)
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try


        Try
            Dialogue()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            RemoveDuplicateInfo()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            RemoveUnwantedElements()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            AddEpubBiblioSet()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            RenumberChapterXMLID()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            PartSequeue()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            ApplyClassForIndex()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            AddIDforFigure()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Return True
    End Function

    Private Function AddIDforFigure() As Boolean
        Dim Figures As XmlNodeList = xmlePubDoc.SelectNodes("//figure//imagedata")
        If ((Figures IsNot Nothing) AndAlso (Figures.Count > 0)) Then
            For f As Int16 = 0 To Figures.Count - 1
                Try
                    Dim FileAtr As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "fileid", "")
                    FileAtr.Value = Path.GetFileNameWithoutExtension(Figures(f).Attributes("fileref").Value)
                    Figures(f).Attributes.Append(FileAtr)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If
        Return True
    End Function

    Private Function Dialogue() As Boolean
        Dim SpeechList As XmlNodeList = xmlePubDoc.SelectNodes("//speech")
        If ((SpeechList Is Nothing) OrElse (SpeechList.Count = 0)) Then
            Return False
        End If
        For s As Integer = 0 To SpeechList.Count - 1
            Try
                Dim LineNode As XmlNode = GetChildNode(SpeechList(s), "line")
                Dim SpeakerNode As XmlNode = GetChildNode(SpeechList(s), "speaker")
                If ((LineNode IsNot Nothing) AndAlso (SpeakerNode IsNot Nothing)) Then
                    LineNode.InnerXml = $"<emphasis role=""italic"">{SpeakerNode.InnerXml.Trim()}</emphasis> {LineNode.InnerXml}"
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

    Private Function RemoveDuplicateInfo()
        Dim InfoList As XmlNodeList = xmlePubDoc.SelectNodes("//info/info")
        If ((InfoList IsNot Nothing) AndAlso (InfoList.Count > 0)) Then
            For f As Int16 = 0 To InfoList.Count - 1
                Try
                    If (InfoList(f).ParentNode IsNot Nothing) Then
                        InfoList(f).ParentNode.InnerXml = InfoList(f).ParentNode.OuterXml.Replace(InfoList(f).OuterXml, InfoList(f).InnerXml)
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, False)
                End Try
            Next
        End If

        InfoList = xmlePubDoc.SelectNodes("//book/info")
        If (InfoList IsNot Nothing) AndAlso (InfoList.Count > 0) Then
            If (InfoList.Count > 1) Then
                If ((InfoList(0).ChildNodes IsNot Nothing) AndAlso (InfoList(0).ChildNodes.Count > 0)) Then
                    If (InfoList(0).ChildNodes.Count = 1) Then
                        InfoList(0).ParentNode.RemoveChild(InfoList(0))
                    End If
                End If
            End If
        End If

        Dim GlossaryNodes As XmlNodeList = xmlePubDoc.SelectNodes("//glossary[not (parent::chapter)]")
        While ((GlossaryNodes IsNot Nothing) AndAlso (GlossaryNodes.Count > 0))
            Try
                If (GlossaryNodes(0).ParentNode IsNot Nothing) Then
                    GlossaryNodes(0).ParentNode.InnerXml = GlossaryNodes(0).ParentNode.InnerXml.Replace(GlossaryNodes(0).OuterXml, Regex.Replace(GlossaryNodes(0).OuterXml, "(<glossary)( [^>]*>)", "<section$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</glossary>", "</section>"))
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
            GlossaryNodes = xmlePubDoc.SelectNodes("//glossary[not (parent::chapter)]")
        End While

        Return True
    End Function


    Private Function ApplyClassForIndex() As Boolean
        Dim PrimaryNodes As XmlNodeList = Nothing
        For ch As Int16 = 97 To 122
            PrimaryNodes = xmlePubDoc.SelectNodes("//primary/term[starts-with(.,'" & ChrW(ch) & "')]|//primary/term[starts-with(.,'" & ChrW(ch - 32) & "')]")
            If ((PrimaryNodes IsNot Nothing) AndAlso (PrimaryNodes.Count > 0)) Then
                For p As Int16 = 0 To PrimaryNodes.Count - 1
                    If (PrimaryNodes(p).ParentNode IsNot Nothing) Then
                        Dim ClassAttrib As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "class", "")
                        If (p = 0) Then
                            ClassAttrib.Value = "IND-F"
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
        PrimaryNodes = xmlePubDoc.SelectNodes("//primary[not (@class)]/term")
        If ((PrimaryNodes IsNot Nothing) AndAlso (PrimaryNodes.Count > 0)) Then
            For p As Int16 = 0 To PrimaryNodes.Count - 1
                If (String.IsNullOrEmpty(PrimaryNodes(p).InnerText)) Then Continue For
                If (Regex.Match(PrimaryNodes(p).InnerText.First, "[0-9]+", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
                    Dim ClassAttrib As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "class", "")
                    ClassAttrib.Value = "IND-1"
                    Try
                        PrimaryNodes(p).ParentNode.Attributes.Append(ClassAttrib)
                    Catch ex As Exception
                    End Try
                ElseIf (Regex.Match(PrimaryNodes(p).InnerText, "^(&#x)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
                    Dim ClassAttrib As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "class", "")
                    ClassAttrib.Value = "IND-1"
                    Try
                        PrimaryNodes(p).ParentNode.Attributes.Append(ClassAttrib)
                    Catch ex As Exception
                    End Try
                End If
            Next
        End If
        Return True
    End Function

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

    Private Function RenumberChapterXMLID() As Boolean
        Dim ChapterNodes As XmlNodeList = xmlePubDoc.SelectNodes("//chapter")
        Dim XMLID As String = String.Empty
        Dim TitleNode As XmlNode = Nothing
        Dim IsBackMatter As Boolean = False
        Dim LableNode As XmlNode = Nothing
        If ((ChapterNodes IsNot Nothing) AndAlso (ChapterNodes.Count > 0)) Then
            For c As Int16 = 0 To ChapterNodes.Count - 1
                XMLID = String.Empty
                IsBackMatter = False
                Try
                    LableNode = ChapterNodes(c).SelectSingleNode(".//label")
                    If (LableNode IsNot Nothing) Then
                        XMLID = Regex.Replace(LableNode.InnerText, "[^0-9]+", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                        If (Not String.IsNullOrEmpty(XMLID)) Then
                            ChapterNodes(c).Attributes("xml:id").Value = Regex.Replace(ChapterNodes(c).Attributes("xml:id").Value, "(chapter)([\s0-9]+)", $"chapter{XMLID}", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            ChapterNodes(c).Attributes("label").Value = XMLID
                        Else
                            ChapterNodes(c).Attributes("xml:id").Value = Regex.Replace(ChapterNodes(c).Attributes("xml:id").Value, "(chapter)([\s0-9]+)", $"chapter{c + 1}", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            ChapterNodes(c).Attributes("label").Value = XMLID
                        End If
                    Else
                        Try
                            If ((ChapterNodes(c).Attributes Is Nothing) OrElse (ChapterNodes(c).Attributes.Count = 0)) Then
                                Dim xmlIDAtt As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "xml:id", "xml")
                                ChapterNodes(c).Attributes.Append(xmlIDAtt)
                            End If
                        Catch ex As Exception

                        End Try
                        TitleNode = ChapterNodes(c).SelectSingleNode(".//title")
                        If (TitleNode IsNot Nothing) Then
                            Select Case TitleNode.InnerText.ToLower()
                                Case "bibliography"
                                    XMLID = "bib"
                                    IsBackMatter = True
                                Case "index", "subject index"
                                    XMLID = "index"
                                    IsBackMatter = True
                                Case "introdcution"
                                    XMLID = "intro"
                                Case "about the contributors"
                                    XMLID = "aboutcontrib"
                                Case "about the author"
                                    XMLID = "about"
                                Case "preface"
                                    XMLID = "preface"
                                Case "abbreviations"
                                    XMLID = "abb"
                                Case "education resource guide"
                                    XMLID = "education"
                                Case "glossary"
                                    IsBackMatter = True
                                    XMLID = "gloss"
                                Case "references", "reference"
                                    IsBackMatter = True
                                    XMLID = "ref"
                                Case "conclusion"
                                    IsBackMatter = True
                                    XMLID = "conc"
                            End Select
                            If (Not String.IsNullOrEmpty(XMLID)) Then
                                ChapterNodes(c).Attributes("xml:id").Value = Regex.Replace(ChapterNodes(c).Attributes("xml:id").Value, "(chapter)([\s0-9]+)", $"{XMLID}", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            Else
                                ChapterNodes(c).Attributes("xml:id").Value = Regex.Replace(ChapterNodes(c).Attributes("xml:id").Value, "(chapter)([\s0-9]+)", $"{c + 1}", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            End If
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try

                Try
                    If (IsBackMatter) Then
                        ChapterNodes(c).Attributes.Remove(ChapterNodes(c).Attributes("label"))
                    End If
                Catch ex As Exception
                End Try

            Next
        End If

        Dim IndexNodelst As XmlNodeList = xmlePubDoc.SelectNodes("//chapter/index")
        If ((IndexNodelst IsNot Nothing) AndAlso (IndexNodelst.Count > 0)) Then
            While ((IndexNodelst IsNot Nothing) AndAlso (IndexNodelst.Count > 0))
                If (IndexNodelst(0).ParentNode.ParentNode IsNot Nothing) Then
                    IndexNodelst(0).ParentNode.ParentNode.InnerXml = IndexNodelst(0).ParentNode.ParentNode.InnerXml.Replace(IndexNodelst(0).ParentNode.OuterXml, IndexNodelst(0).ParentNode.InnerXml)
                End If
                IndexNodelst = xmlePubDoc.SelectNodes("//chapter/index")
            End While
        End If

        Dim CopyrightLst As XmlNodeList = xmlePubDoc.SelectNodes("//copyright")
        If ((CopyrightLst IsNot Nothing) AndAlso (CopyrightLst.Count > 0)) Then
            If (CopyrightLst.Count > 1) Then
                If (CopyrightLst(0).ParentNode IsNot Nothing) Then
                    CopyrightLst(0).ParentNode.RemoveChild(CopyrightLst(0))
                End If
            End If
        End If
        Return True
    End Function

    Private Function PartSequeue() As Boolean
        Dim PartLst As XmlNodeList = xmlePubDoc.SelectNodes("//part")
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
        Return True
    End Function

    Private Function RemoveUnwantedElements() As Boolean
        Dim RemoveList As New List(Of String)
        RemoveList.AddRange(New String() {"//chapter/pagenums", "//chapter/biblioid[@class='doi']", "//chapter/mediaobject[not (parent::figure)]"})
        For Each RemoveXpath As String In RemoveList
            Dim RemoveNodeList As XmlNodeList = xmlePubDoc.SelectNodes($"{RemoveXpath}")
            While ((RemoveNodeList IsNot Nothing) AndAlso (RemoveNodeList.Count > 0))
                Dim RemoveNode As XmlNode = RemoveNodeList(0)
                If (RemoveNode.ParentNode IsNot Nothing) Then
                    RemoveNode.ParentNode.RemoveChild(RemoveNode)
                End If
                RemoveNodeList = xmlePubDoc.SelectNodes($"{RemoveXpath}")
            End While
        Next

        Dim BibliosetNode As XmlNodeList = xmlePubDoc.SelectNodes("//biblioset[@role='isbns']/biblioid[@class='isbn'][@role='epub']")
        If ((BibliosetNode IsNot Nothing) AndAlso (BibliosetNode.Count > 0)) Then
            If (BibliosetNode(BibliosetNode.Count - 1).ParentNode IsNot Nothing) Then
                BibliosetNode(BibliosetNode.Count - 1).ParentNode.RemoveChild(BibliosetNode(BibliosetNode.Count - 1))
            End If
        End If

        Return True
    End Function
    Private Function AddEpubBiblioSet() As Boolean
        Dim BioblioidNode As XmlNode = Nothing
        Dim BibliosetNodes As XmlNodeList = xmlePubDoc.SelectNodes("//biblioset[@role='isbns']/biblioid[@class='isbn'][@role='epub']")
        If ((BibliosetNodes Is Nothing) OrElse (BibliosetNodes.Count = 0)) Then
            Dim BibliosetNode As XmlNode = xmlePubDoc.SelectSingleNode("//biblioset")
            If (BibliosetNode IsNot Nothing) Then
                BioblioidNode = BibliosetNode.SelectSingleNode(".//biblioid[@role='epub']")
                If (BioblioidNode Is Nothing) Then
                    BioblioidNode = xmlePubDoc.CreateNode(XmlNodeType.Element, "biblioid", "")
                    Dim RoleAttrb As XmlAttribute = xmlePubDoc.CreateNode(XmlNodeType.Attribute, "role", "")
                    RoleAttrb.Value = "epub"
                    BioblioidNode.Attributes.Append(RoleAttrb)
                    BioblioidNode.InnerText = IsbnNum
                    BibliosetNode.PrependChild(BioblioidNode)
                End If
            End If
        End If

        Dim RunTitles As XmlNodeList = xmlePubDoc.SelectNodes("//LRH|//RRH")
        If ((RunTitles IsNot Nothing) AndAlso (RunTitles.Count > 0)) Then
            While ((RunTitles IsNot Nothing) AndAlso (RunTitles.Count > 0))
                Dim RunTitle As XmlNode = RunTitles(0)
                If (RunTitle.ParentNode IsNot Nothing) Then
                    RunTitle.ParentNode.RemoveChild(RunTitle)
                End If
                RunTitles = xmlePubDoc.SelectNodes("//LRH|//RRH")
            End While
        End If

        Return True
    End Function

    Public Function GetFinalePubContent() As String
        Me.xmlContent = xmlePubDoc.OuterXml.Replace("&amp;", "&")
        Me.xmlContent = Me.xmlContent.Replace("end-bk-note", "end-ch-note")
        Me.xmlContent = Me.xmlContent.Replace("role=""extract_group""", "")
        Me.xmlContent = Me.xmlContent.Replace("role=""ExtractTitle""", "")

        Me.xmlContent = Regex.Replace(Me.xmlContent, "(<book )([^>]*>)", "$1xmlns=""http://docbook.org/ns/docbook"" $2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return Me.xmlContent
    End Function
End Class
