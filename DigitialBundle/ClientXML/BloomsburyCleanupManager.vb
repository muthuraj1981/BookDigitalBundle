Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Xml
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class BloomsburyCleanupManager
    Private XMLString As String = String.Empty
    Dim xmlDoc As New XmlDocument
    Private iChap As Integer = 0, iHead As Integer = 0
    Public sISBN As String = String.Empty
    Public Property ProjectID As String = String.Empty
    Public Property FileSequence As New List(Of String)
    Public Property sXMLFileName As String = String.Empty
    Public Const sMsgTitle = "XML Cleanup"
    Private AbstractXML As String = String.Empty
    Private sXMLFilePath As String = String.Empty
    Private bExecuteOnce As Boolean = False
    Private iDedication As Integer = 0
    Public bNoError As Boolean = False
    Public Property AppPath As String
    Public Function MainXMLPro(sXMLPath As String, Optional bxslExecution As Boolean = False, Optional Project_ID As String = "", Optional AbstractXML As String = "") As Boolean
#If CONFIG = "FinalXML" Then
        AppPath = "\\fsdeanta\TechRelease\Accounts\Common\DeantaComposer\Publish\extra"
#Else
        AppPath = GBL.AppPath
#End If

        'Try
        Dim di As DirectoryInfo = New DirectoryInfo(sXMLPath.ToString)
        Dim aryFi() As FileInfo = di.GetFiles("*.xml")
        sXMLFilePath = sXMLPath
        Me.AbstractXML = AbstractXML
        'If String.IsNullOrEmpty(sXMLFileName) Then Return Nothing
        If Not sXMLFileName.ToString.ToLower.EndsWith(".xml") Then sXMLFileName = sXMLFileName & ".xml"
        Dim sBookInfo As String = "<book xmlns=""http://docbook.org/ns/docbook"" version=""5.0"" xml:id=""b-" & sISBN.ToString & """ xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:lang=""en"" role=""fullText"">"
        Dim sXMLTxt As String = String.Empty
        If Not File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.ToString)) Then
            ' Merging takes place here...
            'sBookInfo = "<book xmlns=""http://docbook.org/ns/docbook"" version=""5.0"" xml:id=""b-" & sISBN.ToString & """ xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:lang=""en"" role=""fullText"">"
            Using XMLWrite As StreamWriter = File.CreateText(sXMLFilePath & "\" & sXMLFileName)
                XMLWrite.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
                XMLWrite.WriteLine("<?oxygen SCHSchema=""docbook-mods.sch""?>")
                XMLWrite.WriteLine("<?oxygen RNGSchema=""bloomsbury-mods.rnc"" type=""compact""?>")
                XMLWrite.WriteLine(sBookInfo.ToString)
            End Using
            Dim iChFnCnt As Integer = 0
            sXMLTxt = String.Empty
            iSec = 0 : iVal = 0
            Dim iChap As Integer = 0
            Using XMLWrite As StreamWriter = File.AppendText(Path.Combine(sXMLFilePath, sXMLFileName))
                For i = 0 To FileSequence.Count - 1
                    Try
                        iChap = +1
#If CONFIG = "FinalXML" Then
                        XMLString = File.ReadAllText(sXMLFilePath & "\" & Path.GetFileName(FileSequence(i)))
#Else
                        XMLString = File.ReadAllText(sXMLFilePath & "\" & FileSequence(i))
#End If

                        XMLString = XMLString.Replace("<emphasis role=""entity"">&amp;</emphasis>", "&amp;")
                        XMLString = XMLString.Replace("mml:", "mml_")
                        XMLString = Regex.Replace(XMLString, "<chapter[^><]*>", AddressOf ChapterNameSpace, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        XMLString = Regex.Replace(XMLString, "(<LRH[^><]*>)(((?!</LRH>).)*)(</LRH>)", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        XMLString = Regex.Replace(XMLString, "(<RRH[^><]*>)(((?!</RRH>).)*)(</RRH>)", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        XMLString = Regex.Replace(XMLString, " role=""""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        XMLString = XMLString.Replace("<chapter></chapter>", "")
                        XMLString = XMLString.Replace("&#x200B;", "")
                        XMLString = XMLString.Replace("<info><minitoc></minitoc></info>", "")
                        XMLString = XMLString.Replace("</label>&#x2002;<title", "</label><title")
                        XMLString = XMLString.Replace("</label>&#x2002;<colspec", "</label><colspec")
                        If (Regex.Match(XMLString, "(<chapter [^>]*>)(<bibliography[^>]*>)").Success And (XMLString.Contains("</chapter>"))) Then
                            XMLString = Regex.Replace(XMLString, $"(<chapter [^>]*>)(<bibliography[^>]*>)", "$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</chapter>", "")
                        End If

                        XMLString = Regex.Replace(XMLString, "</legalnotice><legalnotice[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                        XMLString = Regex.Replace(XMLString, "<section[^>]*><title[^>]*>Note</title></section>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                        XMLString = XMLString.Replace("<token>", "").Replace("</token>", "")
                        XMLString = XMLString.Replace("<LRH></LRH>", "").Replace("<RRH></RRH>", "")
                        'Dim Fig As String = Regex.Match(XMLString, "<bibliomixed[^>]*>((?:(?!<bibliomixed[^>]*>).)*)<\/bibliomixed>", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
                        'Fig = AdvanceXMLManager.RearrageAttribute(Fig, "role")

                        If Regex.IsMatch(XMLString, "<chapter([^><]+)?>((?:(?:(?!</info>).)+)</author></info>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</info>).)+)</info>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</informalfigure>|</title>).)+)</informalfigure>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</informalfigure>|</title>).)+)</informalfigure>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!(</title>|</para>)).)+)</title>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<part([^><]+)?>)((?:(?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                            XMLString = Regex.Replace(XMLString, "(<part([^><]+)?>)((?:(?:(?!</title>).)+)</title>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<acknowledgements([^><]+)?>)((?:(?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                            XMLString = Regex.Replace(XMLString, "(<acknowledgements([^><]+)?>)((?:(?:(?!</title>).)+)</title>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</subtitle>).)+)</subtitle>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</subtitle>).)+)</subtitle>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</epigraph>).)+)</epigraph>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</epigraph>).)+)</epigraph>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</para>).)+)</para>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) Then
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</para>|<blockquote>).)+)</para>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        ElseIf Regex.IsMatch(XMLString, "(<dedication([^><]+)?>)((?:(?:(?!</para>).)+)</para>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase) Then
                            XMLString = Regex.Replace(XMLString, "(<dedication([^><]+)?>)((?:(?:(?!</para>).)+)</para>)", AddressOf ChapterProDED, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        Else
                            XMLString = Regex.Replace(XMLString, "(<chapter([^><]+)?>)((?:(?:(?!</title>|</informalfigure>).)+)</title>)", AddressOf ChapterProHC, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        End If

                        XMLString = Regex.Replace(XMLString, "<preface([^><]+)?>((?:(?!</preface>).)+)</preface>", AddressOf PrefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                        iSec = +1
                        XMLString = Regex.Replace(XMLString, "(<section([^><]+)?)>", AddressOf SectionPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        XMLString = Regex.Replace(XMLString, "(<footnote([^><]+)?)>", AddressOf FootnotePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        'iVal = 0
                        XMLString = Regex.Replace(XMLString, "(<title([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        'If Regex.IsMatch(XMLString, "<chapter[^><]+>", RegexOptions.IgnoreCase) Then
                        '    iChFnCnt += 1
                        '    XMLString = Regex.Replace(XMLString, "(<fn id="")fn(\d+"">)", "$1ch" & iChFnCnt & "-fn$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        'End If

                        ''29-10-2021
                        'Dim mt1 As Match = Regex.Match(XMLString, "<copyright([^></]+)?>((?:(?!</copyright>).)+)</copyright>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        'If mt1.Success Then
                        '    Dim smtchcol1 As MatchCollection = Regex.Matches(XMLString, "<legalnotice([^></]+)?>((?:(?!</legalnotice>).)+)</legalnotice>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        '    For Each mc As Match In smtchcol1
                        '        XMLString = Regex.Replace(XMLString, mt1.Value.ToString, "", RegexOptions.Singleline)
                        '        XMLString = Regex.Replace(XMLString, mc.Value.ToString, mt1.Value.ToString & mc.Value.ToString, RegexOptions.Singleline)
                        '        Exit For
                        '    Next
                        'End If
                        ''29-10-2021
                        XMLString = Regex.Replace(XMLString, "((<biblioid class=""isbn""[^><]+>((?:(?!</biblioid>).)+)</biblioid>)+)+", "<biblioset role=""isbns"" xml:id=""bs-000001"">" & "$1" & "</biblioset>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                        Try
                            XMLString = ChapterCleanup(XMLString, FileSequence(i))
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try

                        XMLString = Regex.Replace(XMLString, "<pre11([^><]+)?>((?:(?!</pre11>).)+)</pre11>", AddressOf PrefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                        Dim mt As Match = Regex.Match(XMLString, "<book([^><]+)?>((?:(?!</book>).)+)</book>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                        If mt.Success Then sXMLTxt = sXMLTxt & mt.Groups(2).Value.ToString.Trim & Environment.NewLine
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                    'XMLWrite.WriteLine(mt.Groups(2).Value.ToString.Trim)
                Next
                ' Footnote replacement
                ''Try
                ''    Dim mtch As MatchCollection = Regex.Matches(sXMLTxt, "<footnote[^><]+><para([^><]+)?>((?:(?!</footnote>).)+)</footnote>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ''    Dim FootList As List(Of String) = mtch.Cast(Of Match)().Select(Function(m) m.Value).ToList
                ''    mtch = Regex.Matches(sXMLTxt, "<footnote[^><]+><superscript>((?:(?!</footnote>).)+)</footnote>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ''    Dim in1 As Integer = 0
                ''    For Each mc As Match In mtch
                ''        sXMLTxt = Regex.Replace(sXMLTxt, mc.Value.ToString, FootList(in1).ToString, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ''        in1 = in1 + 1
                ''    Next
                ''    sXMLTxt = Regex.Replace(sXMLTxt, "(<footnote[^><]+><para([^><]+)?>)(( |\t)+)?(\d+)", AddressOf FootntSeqPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                ''Catch ex As Exception

                ''End Try

                sXMLTxt = Regex.Replace(sXMLTxt, "<para>(( )+)?", "<para>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                sXMLTxt = BibliographyCleanUp(sXMLTxt)

                sXMLTxt = UpdatePro(sXMLTxt)

                sXMLTxt = Regex.Replace(sXMLTxt, "([^ ])xml:id=", "$1 xml:id=", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "<caption([^><]+)?>(((?!</caption>).)+)</caption>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "<footnote([^><]+)?>(((?!</footnote>).)+)</footnote>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "<endnote([^><]+)?>(((?!</endnote>).)+)</endnote>", AddressOf FigureCaptionParaPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'sXMLTxt = Regex.Replace(sXMLTxt, "(<link role=""figure""[^><]+>(?:(?:(?!</link>).)+)</link><figure([^><]+)?>(?:(?:(?!</figure>).)+)</figure>)(?:(?:(?!</para>).)+)</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'sXMLTxt = Regex.Replace(sXMLTxt, "(<link role=""table""[^><]+>(?:(?:(?!</link>).)+)</link><table([^><]+)?>(?:(?:(?!</table>).)+)</table>)(?:(?:(?!</para>).)+)</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'sXMLTxt = Regex.Replace(sXMLTxt, "(<figure([^><]+)?>(?:(?:(?!</figure>).)+)</figure>)((?:(?!</para>).)+)?</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                'sXMLTxt = Regex.Replace(sXMLTxt, "(<table([^><]+)?>(?:(?:(?!</table>).)+)</table>)((?:(?!</para>).)+)?</para>", AddressOf FigurePlacementPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                sXMLTxt = Regex.Replace(sXMLTxt, "</1para>", "</para>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                XMLWrite.WriteLine(sXMLTxt.ToString.Replace("&", "&amp;"))
                XMLWrite.WriteLine("</book>")
            End Using
        End If
        If Not CheckValidXML(Path.Combine(sXMLFilePath, sXMLFileName)) Then
            File.Delete(Path.Combine(sXMLFilePath, sXMLFileName))
            Return False
        End If
        File.Copy(Path.Combine(sXMLFilePath, sXMLFileName), Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), True)
        Thread.Sleep(500)
        CallingXSLPro(Path.Combine(sXMLFilePath, sXMLFileName))
        ' After xsl execution these changes has to be taken care
        ' ------------
        sXMLTxt = File.ReadAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))



        sXMLTxt = sXMLTxt.Replace("&amp;", "&")
        sXMLTxt = Regex.Replace(sXMLTxt, "(( )?xmlns=""http://docbook.org/ns/docbook""|( )?xmlns="""")", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ' XSL removed some text so here again the ide generation and all calling

        ' Some hardcode text given by jaffar
        ' Removing toc content 
        sXMLTxt = Regex.Replace(sXMLTxt, "xmlns:fo=""http://www.w3.org/1999/XSL/Format"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:d=""http://docbook.org/ns/docbook"" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/"" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/"" xmlns:code=""urn:schemas-test-code""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sXMLTxt = Regex.Replace(sXMLTxt, "<toc([^><]+)?>((?:(?!</toc>).)+)</toc>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline) '18-04-2022
        sXMLTxt = Regex.Replace(sXMLTxt, " xml:id=""""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<a id=""page_([^""></]+)""/>", "<?page value=""$1""?>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ' Text insertion between last legalnotice and colophon ' 18-05-2020
        'sXMLTxt = Regex.Replace(sXMLTxt, "</legalnotice>((?:(?!<legalnotice([^><]+)?>).)+)(</colophon([^><]+)?>)", AddressOf HardCorePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.RightToLeft)
        'Dim smtchcol As MatchCollection = Regex.Matches(sXMLTxt, "<chapter([^></]+)?>((?:(?!</chapter>).)+)</chapter>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'For Each mc As Match In smtchcol
        '    sXMLTxt = sXMLTxt.Replace(mc.Value.ToString, "</part>" & mc.Value.ToString)
        '    Exit For
        'Next

        sXMLTxt = UpdatePro(sXMLTxt)

        Try
            sXMLTxt = PostXMLCleanup(sXMLTxt)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If (String.IsNullOrEmpty(sXMLTxt)) Then
            Return False
        End If

        iChap = 50
        sXMLTxt = Regex.Replace(sXMLTxt, "(<footnote[^><]+><para[^><]+>)\d+( ?\t?)+", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<info>", AddressOf ChapInfoPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "(<chapter([^><]+)?>)((?:(?:(?!</info>).)+)</info>)", AddressOf ChapterPro1, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<biblioid([^><]+)?>((?:(?!</biblioid>).)+)</biblioid>", AddressOf BiblioIdPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "(  )+", " ")
        sXMLTxt = Regex.Replace(sXMLTxt, "<book>", sBookInfo.ToString, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        'sXMLTxt = Regex.Replace(sXMLTxt, "([^""])((http:| www\.| mailto:)([^ ><]+))", "$1<link xlink:href=""$2""><uri>$2</uri></link>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        'sXMLTxt = Regex.Replace(sXMLTxt, "(<uri>)?((http:|https:|www\.| mailto:)([^ ><]+))</uri>", "<link xlink:href=""$2""><uri>$2</uri></link>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) '04-05-2021

        sXMLTxt = Regex.Replace(sXMLTxt, "(<link[^><]*>)(<link[^><]*>)", "$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Replace("</link></link>", "</link>")

        sXMLTxt = Regex.Replace(sXMLTxt, "<chapter[\s]*/>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        sXMLTxt = Regex.Replace(sXMLTxt, "(<bibliomixed[^><]*>)(((?!</bibliomixed>).)+)(</bibliomixed>)", AddressOf BiblioGraphyClean, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        sXMLTxt = Regex.Replace(sXMLTxt, "<emphasis>(&#x201(9|8);)</emphasis>", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "([^ ])xml:id=", "$1 xml:id=", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "</blockquote><blockquote[^><]+>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<section([^><]+)?>((?:(?!</section>).)+)</section>", AddressOf SectionNoPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = Regex.Replace(sXMLTxt, "<biblioset role=""publisher"">((?:(?!</biblioset>).)+)</biblioset>", AddressOf Biboloset, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        '04-05-2021
        sXMLTxt = sXMLTxt.Replace("<link role=""bibr""", "<link role=""bib""")
        sXMLTxt = sXMLTxt.Replace("</surname>,", ",</surname>")
        sXMLTxt = sXMLTxt.Replace("mml_", "mml:")
        'XMLString = Regex.Replace(XMLString, "5.0b-" & sISBN & "enfullText", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLTxt = sXMLTxt.Replace("<partttt", "<part").Replace("</partttt>", "</part>")
        sXMLTxt = sXMLTxt.Replace("<pre11", "<preface").Replace("</pre11>", "</preface>")


        sXMLTxt = Regex.Replace(sXMLTxt, "(</para>)(<\?page value=""[^""]+""\?>)", "$2$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        sXMLTxt = sXMLTxt.Replace("<entity>", "").Replace("</entity>", "")
        sXMLTxt = sXMLTxt.Replace("&amp;#xFB01", "f")
        sXMLTxt = sXMLTxt.Replace("<emphasis>", "<emphasis role=""italic"">")

        sXMLTxt = Regex.Replace(sXMLTxt, " < book()?> ", " < book xmlns=""http://docbook.org/ns/docbook"" version=""5.0"" xml:id=""b-" & sISBN.ToString & """ xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:lang=""en"" role=""fullText"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'sXMLTxt = Regex.Replace(sXMLTxt, "(<footnote([^><]+)?)>", AddressOf FootnotePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If (File.Exists(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))) Then
            File.Delete(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")))
        End If

        File.WriteAllText(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), sXMLTxt.Replace("&amp;", "&"))
        If (File.Exists(Path.Combine(sXMLFilePath, sXMLFileName))) Then
            File.Delete(Path.Combine(sXMLFilePath, sXMLFileName))
        End If
        File.Move(Path.Combine(sXMLFilePath, sXMLFileName.Replace(".xml", "_xsl.xml")), Path.Combine(sXMLFilePath, sXMLFileName))
        GBL.DeantaBallon("Merge and cleanup has been completed. Please check the xml file.", MessageType.MSGERROR)
        Return True
    End Function

    Private Function PostXMLCleanup(ByVal XMLCont As String) As String
        Dim xmlPost As New XmlDocument
        xmlPost.PreserveWhitespace = True

        If (XMLCont.Contains("</authorgroup> and <authorgroup>") And XMLCont.Contains("</authorgroup><authorgroup>")) Then
            XMLCont = XMLCont.Replace("</authorgroup> and <authorgroup>", "</authorgroup><authorgroup>").Replace("</authorgroup><authorgroup>", "")
        End If

        If ((XMLCont.Contains("<authorgroup><authorgroup>")) And (XMLCont.Contains("</authorgroup></authorgroup>"))) Then
            XMLCont = XMLCont.Replace("<authorgroup><authorgroup>", "<authorgroup>").Replace("</authorgroup></authorgroup>", "</authorgroup>")
        End If

        XMLCont = Regex.Replace(XMLCont, "</info><info[^>]*><authorgroup>", "<authorgroup>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        Try
            xmlPost.LoadXml(XMLCont.Replace("&", "&amp;").Replace("xlink:href", "xlink_href"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return String.Empty
        End Try

        Dim TocNode As XmlNode = xmlPost.SelectSingleNode("//toc")
        Dim PartNode As XmlNode = xmlPost.SelectSingleNode("//partttt")
        If ((TocNode IsNot Nothing) AndAlso (PartNode IsNot Nothing)) Then
            Try
                PartNode.AppendChild(TocNode)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        Dim FirstNames As XmlNodeList = xmlPost.SelectNodes("//firstname")
        For f As Int16 = 0 To FirstNames.Count - 1
            Try
                If ((FirstNames(f).InnerText.Contains(".")) And (FirstNames(f).InnerText.Contains(" "))) Then
                    Dim SpaceIndex As Int16 = FirstNames(f).InnerText.IndexOf(" ")
                    If (SpaceIndex > 0) Then
                        Dim NameText As String = FirstNames(f).InnerText.Substring(0, SpaceIndex)
                        Dim SecondText As String = FirstNames(f).InnerText.Substring(SpaceIndex + 1, (FirstNames(f).InnerText.Length - SpaceIndex) - 1)
                        Dim OtherNode As XmlNode = xmlPost.CreateNode(XmlNodeType.Element, "othername", "")
                        Dim OtherAtt As XmlAttribute = xmlPost.CreateNode(XmlNodeType.Attribute, "role", "")
                        If (NameText.Contains(".")) Then
                            OtherAtt.Value = "middle"
                        ElseIf (SecondText.Contains(".")) Then
                            OtherAtt.Value = "middle-initials"
                        End If
                        OtherNode.Attributes.Append(OtherAtt)
                        OtherNode.InnerText = SecondText
                        FirstNames(f).InnerText = NameText
                        FirstNames(f).ParentNode.InsertAfter(OtherNode, FirstNames(f))
                    End If
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next

        FirstNames = xmlPost.SelectNodes("//firstname[@role='initials']")
        If ((FirstNames IsNot Nothing) AndAlso (FirstNames.Count > 0)) Then
            For f As Int16 = 0 To FirstNames.Count - 1
                If (Not FirstNames(f).InnerText.Contains(".")) Then
                    Try
                        If (String.Compare(FirstNames(f).Attributes("role").Value, "initials", True) = 0) Then
                            FirstNames(f).Attributes.Remove(FirstNames(f).Attributes("role"))
                        End If
                    Catch ex As Exception
                    End Try
                End If
            Next
        End If

        Dim PartChapters As XmlNodeList = xmlPost.SelectNodes("//part")
        If ((PartChapters IsNot Nothing) AndAlso (PartChapters.Count > 0)) Then
            For p As Int16 = 0 To PartChapters.Count - 1
                If (PartChapters(p).Attributes("label") Is Nothing) Then
                    Dim Label As XmlAttribute = xmlPost.CreateNode(XmlNodeType.Attribute, "label", "")
                    Label.Value = $"{p + 1}"
                    PartChapters(p).Attributes.Append(Label)
                End If
            Next
        End If

        Dim PartLst As XmlNodeList = xmlPost.SelectNodes("//part")
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
            Next
        End If

        Dim Tables As XmlNodeList = xmlPost.SelectNodes("//table")
        If ((Tables IsNot Nothing) AndAlso (Tables.Count > 0)) Then
            For t As Int16 = 0 To Tables.Count - 1
                Dim Label As XmlNode = Tables(t).SelectSingleNode(".//label")
                If (Label IsNot Nothing) Then
                    Dim frame As XmlAttribute = xmlPost.CreateNode(XmlNodeType.Attribute, "frame", "")
                    frame.Value = "all"
                    Dim lblAtt As XmlAttribute = xmlPost.CreateNode(XmlNodeType.Attribute, "label", "")
                    lblAtt.Value = Regex.Replace(Label.InnerText, "[^0-9\.]", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    Tables(t).Attributes.Prepend(lblAtt)
                    Tables(t).Attributes.Prepend(frame)
                    Tables(t).RemoveChild(Label)
                End If
            Next
        End If

        Dim UriNodes As XmlNodeList = xmlPost.SelectNodes("//uri[not (parent::link)]")
        Dim MxCt As Int16 = 0
        If ((UriNodes IsNot Nothing) AndAlso (UriNodes.Count > 0)) Then
            MxCt = UriNodes.Count * 3
            While ((UriNodes IsNot Nothing) AndAlso (UriNodes.Count > 0))
                If (MxCt = 0) Then
                    Exit While
                End If
                If (UriNodes(0).ParentNode IsNot Nothing) Then
                    UriNodes(0).ParentNode.InnerXml = UriNodes(0).ParentNode.InnerXml.Replace(UriNodes(0).OuterXml, $"<link xlink_href=""{UriNodes(0).InnerText}""><uri>{UriNodes(0).InnerXml}</uri></link>")
                End If
                UriNodes = xmlPost.SelectNodes("//uri[not (parent::link)]")
                MxCt = MxCt - 1
            End While
        End If



        Dim LegalNode As XmlNode = xmlPost.SelectSingleNode("//legalnotice")
        Dim CopyRightNode As XmlNode = xmlPost.SelectSingleNode("//copyright")
        If ((CopyRightNode Is Nothing) And (LegalNode.ParentNode IsNot Nothing)) Then
            LegalNode.ParentNode.InnerXml = LegalNode.ParentNode.InnerXml.Replace(LegalNode.OuterXml, $"<copyright><year>2021</year><holder>Paula A. Michaels, Christina Twomey and Contributors</holder></copyright>{LegalNode.OuterXml}")
        ElseIf ((CopyRightNode IsNot Nothing) And (LegalNode.ParentNode IsNot Nothing)) Then
            LegalNode.ParentNode.InsertBefore(CopyRightNode, LegalNode)
        End If

        CopyRightNode = xmlPost.SelectSingleNode("//copyright")
        If (CopyRightNode IsNot Nothing) Then
            Dim YearText As String = Regex.Match(CopyRightNode.InnerText, " [0-9]{4,4}", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
            If (Not String.IsNullOrEmpty(YearText)) Then
                CopyRightNode.InnerXml = CopyRightNode.InnerXml.Replace(YearText, "").Trim().Replace(",</", "</")
                Dim YearNode As XmlNode = xmlPost.CreateNode(XmlNodeType.Element, "year", "")
                YearNode.InnerText = YearText.Trim()
                CopyRightNode.PrependChild(YearNode)
            End If
        End If

        Try
            AddChapterAbstract(xmlPost)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & "AddChapterAbstract", MessageType.MSGERROR)
        End Try

        Dim AbstrNodes As XmlNodeList = xmlPost.SelectNodes("//abstract[@abstract-type='abstract']/para")
        Dim KeywordText As String = String.Empty
        Dim FinalText As String = String.Empty
        If ((AbstrNodes IsNot Nothing) AndAlso (AbstrNodes.Count > 0)) Then
            For a As Int16 = 0 To AbstrNodes.Count - 1
                Try
                    KeywordText = String.Empty
                    FinalText = String.Empty
                    If (AbstrNodes(a).InnerText.ToLower().StartsWith("keywords:")) Then
                        KeywordText = AbstrNodes(a).InnerText.Replace("Keywords: ", "")
                        FinalText = "<keywordset>"
                        For Each strKey As String In KeywordText.Split(", ")
                            FinalText = $"{FinalText}<keyword>{strKey}</keyword>"
                        Next
                        FinalText = $"{FinalText}</keywordset>"
                    End If
                    If (FinalText.Contains("<keyword>") And AbstrNodes(a).ParentNode IsNot Nothing And AbstrNodes(a).ParentNode.ParentNode IsNot Nothing) Then
                        AbstrNodes(a).ParentNode.ParentNode.InnerXml = AbstrNodes(a).ParentNode.ParentNode.InnerXml.Replace(AbstrNodes(a).ParentNode.OuterXml, AbstrNodes(a).ParentNode.OuterXml.Replace(AbstrNodes(a).OuterXml, "") & FinalText)
                        FinalText = String.Empty
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If

        Try
            xmlPost.RemoveXMLNode("//info/para", NodeMoveOption.TEXTCONTAINS, "Edited by")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            xmlPost.RemoveAttribute("//bibliomisc[@map='Series_Title']", "map")
            xmlPost.RemoveAttribute("//bibliomisc[@map='Series_Editor']", "map")
            xmlPost.RemoveAttribute("//bibliomisc[@map='Series_Text']", "map")
            xmlPost.RemoveAttribute("//legalnotice[@role='CIP']", "role")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim PageInsts As XmlNodeList = xmlPost.SelectNodes("//info/title/processing-instruction('page')")
        If ((PageInsts IsNot Nothing) AndAlso (PageInsts.Count > 0)) Then
            For p As Int16 = 0 To PageInsts.Count - 1
                Dim NextNd As XmlNode = PageInsts(p).ParentNode.ParentNode.NextSibling
                If ((NextNd IsNot Nothing) AndAlso (NextNd.NodeType = XmlNodeType.Element)) Then
                    NextNd.PrependChild(PageInsts(p))
                End If
            Next
        End If

        Dim Figures As XmlNodeList = xmlPost.SelectNodes("//informalfigure")
        If ((Figures IsNot Nothing) AndAlso (Figures.Count > 0)) Then
            For g As Int16 = 0 To Figures.Count - 1
                Dim LblNode As XmlNode = Figures(g).SelectSingleNode(".//label")
                If (LblNode IsNot Nothing) Then
                    Figures(g).InnerXml = Regex.Replace(Figures(g).InnerXml.Replace($"{LblNode.OuterXml}&amp;#x2002;", ""), "<figsource[^>]*>", "<para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</figsource>", "</para>")
                End If
            Next
        End If

        Dim FMPart As XmlNode = xmlPost.SelectSingleNode("//partttt")
        Dim Book As XmlNode = xmlPost.SelectSingleNode("//book")
        If ((FMPart IsNot Nothing) AndAlso (Book IsNot Nothing)) Then
            For b As Int16 = 0 To Book.ChildNodes.Count - 1
                If (Book.ChildNodes(b).NodeType <> XmlNodeType.Element) Then
                    Continue For
                End If
                Select Case Book.ChildNodes(b).Name
                    Case "info"
                        Continue For
                    Case "chapter"
                        Exit For
                    Case "preface", "dedication", "pre11"
                    Case Else
                        Continue For
                End Select
                If (Book.ChildNodes(b).NodeType = XmlNodeType.Element) Then
                    FMPart.AppendChild(Book.ChildNodes(b))
                End If
            Next
        End If
        Return xmlPost.OuterXml.Replace("&amp;", "&").Replace("xlink_href", "xlink:href")
    End Function

    Private Function GetKeywordInsertion(ByVal xmlMeta As XmlDocument) As Boolean
        Dim DtProj As New DataTable("Proj")
        DtProj = MySqlHelper.ReadSqlData($"select kwd_group from tb_projects where project_id={ProjectID}")
        If ((DtProj Is Nothing) OrElse (DtProj.Rows Is Nothing) OrElse (DtProj.Rows.Count = 0)) Then Return False

        If (String.IsNullOrEmpty(Convert.ToString(DtProj.Rows(0).Item("kwd_group")))) Then Return False
        Dim PermissionNode As XmlNode = xmlMeta.SelectSingleNode("//legalnotice")
        If (PermissionNode IsNot Nothing) Then
            Try
                If (PermissionNode.ParentNode IsNot Nothing) Then
                    PermissionNode.ParentNode.InnerXml = PermissionNode.ParentNode.InnerXml.Replace(PermissionNode.OuterXml, $"{PermissionNode.OuterXml}<kwd-group>{Convert.ToString(DtProj.Rows(0).Item("kwd_group"))}</kwd-group>")
                End If
            Catch ex As Exception
                GBL.DeantaBallon("Error occurred while inserting the Book keyword information", MessageType.MSGERROR)
            End Try
        End If
        Return True
    End Function

    Private Function GetChapterAbtract(ByVal xmlAbst As XmlDocument, ByVal ChapID As String) As XmlNode
        Dim ChapNodes As XmlNodeList = xmlAbst.SelectNodes("//section/title")
        Dim AbstNode As XmlNode = Nothing
        If ((ChapNodes IsNot Nothing) AndAlso (ChapNodes.Count > 0)) Then
            For c As Int16 = 0 To ChapNodes.Count - 1
                If (ChapNodes(c).InnerText.Replace(" ", "").ToLower().Contains(ChapID.ToLower())) Then
                    AbstNode = GetChildNode(ChapNodes(c).ParentNode, "abstract")
                    If (AbstNode Is Nothing) Then
                        AbstNode = ChapNodes(c).ParentNode.SelectSingleNode(".//para")
                    End If
                    If (AbstNode IsNot Nothing) Then
                        Return AbstNode
                    End If
                End If
            Next
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


    Private Function AddChapterAbstract(ByVal xmlMeta As XmlDocument) As Boolean
        Dim xmlAbst As XmlDocument = Nothing
        Try
            xmlAbst = LoadAbstractXML()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        If (xmlAbst Is Nothing) Then
            Return False
        End If
        Dim BookPartNodes As XmlNodeList = xmlMeta.SelectNodes("//chapter")
        Dim Chapterid As String = String.Empty
        Dim AbstNode As XmlNode = Nothing
        Dim InfoNode As XmlNode = Nothing
        If ((BookPartNodes IsNot Nothing) AndAlso (BookPartNodes.Count > 0)) Then
            For b As Int16 = 0 To BookPartNodes.Count - 1
                Try
                    Chapterid = BookPartNodes(b).Attributes("xml:id").Value
                Catch ex As Exception
                    Chapterid = String.Empty
                End Try
                If (String.IsNullOrEmpty(Chapterid)) Then Continue For
                Chapterid = Chapterid.Replace($"b-{sISBN}-", "")
                Try
                    AbstNode = GetChapterAbtract(xmlAbst, Chapterid)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
                Try
                    If (AbstNode Is Nothing) Then Continue For
                    InfoNode = BookPartNodes(b).SelectSingleNode(".//info[child::mediaobject]")
                    If (InfoNode IsNot Nothing) Then
                        If (Not AbstNode.OuterXml.Contains("<abstract")) Then
                            InfoNode.InnerXml = $"{InfoNode.InnerXml}<abstract abstract-type=""abstract"">{AbstNode.OuterXml}</abstract>"
                        Else
                            InfoNode.InnerXml = InfoNode.InnerXml & AbstNode.OuterXml
                        End If
                    Else
                        GBL.DeantaBallon($"Could not able to find the info element for append abstract. - {Chapterid}", MessageType.MSGERROR)
                        Continue For
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If
        Return True
    End Function


    Private Function LoadAbstractXML() As XmlDocument
        Dim xmlAbst As New XmlDocument
        xmlAbst.PreserveWhitespace = True
        If (String.IsNullOrEmpty(Me.AbstractXML)) Then
            GBL.DeantaBallon("Book Abstract XML file is empty.", MessageType.MSGERROR)
            Return Nothing
        End If
        If (Not File.Exists(Me.AbstractXML)) Then
            GBL.DeantaBallon("Book Abstract XML file is not found.", MessageType.MSGERROR)
            Return Nothing
        End If
        Dim AbstrContent As String = File.ReadAllText(Me.AbstractXML)
        AbstrContent = Regex.Replace(Regex.Replace(AbstrContent, "(<!--)+(<!DOCTYPE([^>]+)>)(-->)+", "$2"), "(<!DOCTYPE([^>]+)>)", "<!--$1-->")
        If (Not AbstrContent.Contains("<abstract")) Then
            AbstrContent = AbstrContent.Replace("<para>", "<abstract abstract-type=""abstract""><para>").Replace("</para>", "</para></abstract>")
            AbstrContent = AbstrContent.Replace("</abstract><abstract abstract-type=""abstract"">", "")
        End If
        AbstrContent = AbstrContent.Replace("xlink:href", "xlink_href")
        Try
            xmlAbst.LoadXml(AbstrContent.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon($"Load abstract XML{ex.Message}", MessageType.MSGERROR)
            Return Nothing
        End Try
        Return xmlAbst
    End Function

    Private Function ChapterCleanup(ByVal XmlContents As String, ByVal XmlFileName As String) As String
        xmlDoc = New XmlDocument
        xmlDoc.PreserveWhitespace = True
        Dim PgMapList As New List(Of PageMapData)
        PgMapList.Add(New PageMapData With {.SourceXPath = "//book/a[parent::book]", .DestinationXPath = "//part/info/title"})
        PgMapList.Add(New PageMapData With {.SourceXPath = "//book/a[parent::book]", .DestinationXPath = "//chapter/info/title"})
        PgMapList.Add(New PageMapData With {.SourceXPath = "//book/a[parent::book]", .DestinationXPath = "//preface/title"})
        PgMapList.Add(New PageMapData With {.SourceXPath = "//info/authorgroup/a[@id]", .DestinationXPath = "//info/authorgroup", .Choice = NodeMoveOption.MOVEAFTER})
        XmlContents = XmlContents.Replace("&amp;", "&#x0026;").Replace("&", "&amp;")
        XmlContents = XmlContents.Replace(" xmlns=""http://docbook.org/ns/docbook""", "")

        While XmlContents.Contains(vbCr)
            XmlContents = XmlContents.Replace(vbCr, " ")
        End While

        While XmlContents.Contains(vbLf)
            XmlContents = XmlContents.Replace(vbLf, " ")
        End While

        While XmlContents.Contains(vbCrLf)
            XmlContents = XmlContents.Replace(vbCrLf, " ")
        End While

        While XmlContents.Contains(vbNewLine)
            XmlContents = XmlContents.Replace(vbNewLine, " ")
        End While

        While XmlContents.Contains("  ")
            XmlContents = XmlContents.Replace("  ", " ")
        End While

        XmlContents = XmlContents.Replace("xml:id=""""", "")

        XmlContents = XmlContents.Replace("</author> and <author>", "</author><author>")

        Try
            xmlDoc.LoadXml(XmlContents)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & "Chapter cleanup", MessageType.MSGERROR)
            Return XmlContents
        End Try

        Try
            xmlDoc.AddXMLChildNode("//authorgroup/author/personname", "personblurb", "<para>xxx</para>", NodeMoveOption.MOVEAFTER)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            xmlDoc.AddXMLChildNode("//colophon", "partttt", $"<info><title>Front matter</title></info>", NodeMoveOption.MOVEAFTER)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            xmlDoc.AddXMLChildNode("//partttt/info", "pre11", $"<title>Title Pages</title><remark condition=""hidden"">Note that this is a placeholder for the pdf of the prelims and no full text content is included at this point</remark>", NodeMoveOption.MOVEAFTER)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            xmlDoc.AddXMLAttribute("//pre11", "role", "prelims", NodeMoveOption.FIRSTCHILD)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim PageTagList As XmlNodeList = xmlDoc.SelectNodes("//a[@id]")
        For p As Int16 = 0 To PgMapList.Count - 1
            Dim SourcePageNodes As XmlNodeList = xmlDoc.SelectNodes(PgMapList(p).SourceXPath)
            Dim DestNode As XmlNode = xmlDoc.SelectSingleNode(PgMapList(p).DestinationXPath)
            If (DestNode Is Nothing) Then
                Continue For
            End If
            If ((SourcePageNodes Is Nothing) OrElse (SourcePageNodes.Count = 0)) Then
                Continue For
            End If
            For s As Int16 = 0 To SourcePageNodes.Count - 1
                Select Case PgMapList(p).Choice
                    Case NodeMoveOption.FIRSTCHILD
                        DestNode.PrependChild(SourcePageNodes(s))
                    Case NodeMoveOption.MOVEAFTER
                        DestNode.ParentNode.InsertAfter(SourcePageNodes(s), DestNode)
                End Select
            Next
        Next

        '24-05-2021

        PageTagList = xmlDoc.SelectNodes("//a[@id]")
        Dim PrevCnt As Int32 = 0
        If ((PageTagList IsNot Nothing) AndAlso (PageTagList.Count > 0)) Then
            For pg As Integer = 0 To PageTagList.Count - 1
                PrevCnt = 0
                If (PageTagList(pg).PreviousSibling Is Nothing) OrElse (PageTagList(pg).PreviousSibling.NodeType <> XmlNodeType.Element) Then
                    Continue For
                End If
                Dim PrevNode As XmlNode = PageTagList(pg).PreviousSibling
                While (PrevCnt <= 4)
                    If ((PrevNode IsNot Nothing) AndAlso (String.Compare(PrevNode.Name, "line", True) = 0)) Then
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
        Dim StartPage As String = String.Empty
        Dim EndPage As String = String.Empty
        Dim PageNumNode As XmlNode = xmlDoc.SelectSingleNode("//pagenums")
        Dim PageList As XmlNodeList = xmlDoc.SelectNodes("//a[@id]")
        If ((PageNumNode IsNot Nothing) AndAlso (PageList IsNot Nothing) AndAlso (PageList.Count > 0)) Then
            Try
                If (PageList.Count > 1) Then
                    StartPage = PageList(0).Attributes("id").Value.Replace("page_", "")
                    Try
                        EndPage = (From n In PageList Select Convert.ToInt32(n.Attributes("id").Value.Replace("page_", ""))).Max
                    Catch ex As Exception
                        EndPage = PageList(PageList.Count - 1).Attributes("id").Value.Replace("page_", "")
                    End Try
                    PageNumNode.InnerText = $"{StartPage}&#x2013;{EndPage}"
                ElseIf (PageList.Count = 1) Then
                    StartPage = PageList(0).Attributes("id").Value.Replace("page_", "")
                    PageNumNode.InnerText = StartPage
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        Dim PartLabels As XmlNodeList = xmlDoc.SelectNodes("//part/info/label")
        If ((PartLabels IsNot Nothing) AndAlso (PartLabels.Count > 0)) Then
            For p As Int32 = 0 To PartLabels.Count - 1
                If (PartLabels(p).ParentNode IsNot Nothing) Then
                    PartLabels(p).ParentNode.RemoveChild(PartLabels(p))
                End If
            Next
        End If

        Dim PartChapters As XmlNodeList = xmlDoc.SelectNodes("//part/chapter")
        If ((PartChapters IsNot Nothing) AndAlso (PartChapters.Count > 0)) Then
            For c As Int32 = 0 To PartChapters.Count - 1
                If (Not String.IsNullOrEmpty(PartChapters(c).InnerText)) Then
                    If (PartChapters(c).ParentNode IsNot Nothing) Then
                        PartChapters(c).ParentNode.InnerXml = Regex.Replace(PartChapters(c).ParentNode.InnerXml, "(<chapter)([^>]*>)", "<partintro$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</chapter>", "</partintro>")
                    End If
                Else
                    If (PartChapters(c).ParentNode IsNot Nothing) Then
                        PartChapters(c).ParentNode.RemoveChild(PartChapters(c))
                    End If
                End If
            Next
        End If

        PartChapters = xmlDoc.SelectNodes("//part/subtitle")
        If ((PartChapters IsNot Nothing) AndAlso (PartChapters.Count > 0)) Then
            For c As Int32 = 0 To PartChapters.Count - 1
                Dim title As XmlNode = PartChapters(c).ParentNode.SelectSingleNode(".//info/title")
                If ((title IsNot Nothing) AndAlso (title.ParentNode IsNot Nothing)) Then
                    title.ParentNode.InsertAfter(PartChapters(c), title)
                End If
            Next
        End If
        Dim CoverNode As XmlNode = xmlDoc.SelectSingleNode("//cover")
        Dim AuthorGroup As XmlNode = xmlDoc.SelectSingleNode("//book/info/authorgroup")

        If ((CoverNode IsNot Nothing) AndAlso (AuthorGroup IsNot Nothing)) Then
            If (CoverNode.ParentNode IsNot Nothing) Then
                CoverNode.ParentNode.InsertAfter(CoverNode, AuthorGroup)
            End If
        End If

        Dim BlosLogNode As XmlNode = xmlDoc.SelectSingleNode("//book/info/para[child::informalfigure][count(child::node())=1]")
        If ((BlosLogNode IsNot Nothing) AndAlso (BlosLogNode.ParentNode IsNot Nothing)) Then
            BlosLogNode.ParentNode.RemoveChild(BlosLogNode)
        End If

        Dim PageNodes As XmlNodeList = xmlDoc.SelectNodes("//book/info/a[@id]|//book/a[@id]")
        Dim MxCunt As Int16 = 0
        If ((PageNodes IsNot Nothing) AndAlso (PageNodes.Count > 0)) Then
            MxCunt = PageNodes.Count * 3
            While ((PageNodes IsNot Nothing) AndAlso (PageNodes.Count > 0))
                If (MxCunt = 0) Then
                    Exit While
                End If
                If (PageNodes(0).ParentNode IsNot Nothing) Then
                    PageNodes(0).ParentNode.RemoveChild(PageNodes(0))
                End If
                PageNodes = xmlDoc.SelectNodes("//book/info/a[@id]|//book/a[@id]")
            End While
        End If

        Dim titleNodes As XmlNodeList = xmlDoc.SelectNodes("//book/info/title")
        If ((titleNodes IsNot Nothing) AndAlso (titleNodes.Count > 0)) Then
            If ((titleNodes.Count = 2) AndAlso (String.Compare(titleNodes(0).InnerText, titleNodes(1).InnerText, True) = 0)) Then
                titleNodes(0).ParentNode.RemoveChild(titleNodes(0))
            End If
        End If

        Dim LegalnoticeNodes As XmlNodeList = xmlDoc.SelectNodes("//legalnotice")
        If ((LegalnoticeNodes IsNot Nothing) AndAlso (LegalnoticeNodes.Count > 0)) Then
            For l As Int16 = 0 To LegalnoticeNodes.Count - 1
                Dim roleAt As XmlAttribute = xmlDoc.CreateNode(XmlNodeType.Attribute, "role", "")
                roleAt.Value = "CIP"
                LegalnoticeNodes(l).Attributes.Prepend(roleAt)
            Next
        End If

        Dim Colophon As XmlNode = xmlDoc.SelectSingleNode("//colophon")
        If ((Colophon IsNot Nothing) AndAlso (Colophon.ParentNode IsNot Nothing)) Then
            Colophon.ParentNode.InnerXml = Colophon.ParentNode.InnerXml.Replace(Colophon.OuterXml, $"<biblioid Class=""doi"">10.5040/{sISBN}</biblioid><biblioid Class=""other"" otherclass=""schemaVersion"">1</biblioid><biblioid Class=""other"" otherclass=""schematronVersion"">5</biblioid><abstract role=""blurb"" xml:id=""ba-blurb1""><para xml:id=""pa-0000009a""></para></abstract>{Colophon.OuterXml}")
        End If

        'Colophon = xmlDoc.SelectSingleNode("//colophon")
        'Dim InfoNode As XmlNode = xmlDoc.SelectSingleNode("//book/info")
        'If (InfoNode IsNot Nothing) AndAlso (InfoNode.ParentNode IsNot Nothing) Then
        '    InfoNode.ParentNode.InsertAfter(Colophon, InfoNode)
        'End If

        Dim Tables As XmlNodeList = xmlDoc.SelectNodes("//table|//sidebar")
        If ((Tables IsNot Nothing) AndAlso (Tables.Count > 0)) Then
            For t As Int16 = 0 To Tables.Count - 1
                Tables(t).InnerXml = Tables(t).InnerXml.Replace("<title", $"<info xml:id=""b-{sISBN}""><title").Replace("</title>", "</title></info>")
            Next
        End If

        Dim PrelimsTexts As New List(Of String)
        PrelimsTexts.AddRange(New String() {"Contents", "Figures", "Tables", "photo", "Illustrations", "boxes", "maps", "list of exhibits", "what do i need help with?"})

        Dim Paras As XmlNodeList = Nothing
        'Prelims List of figure and table update.
        If (XmlFileName.Contains("_PRELIMS_")) Then
            Dim Sections As XmlNodeList = xmlDoc.SelectNodes("//section/title|//preface/info/title")
            If ((Sections IsNot Nothing) AndAlso (Sections.Count > 0)) Then
                For s As Int16 = 0 To Sections.Count - 1
                    If (From n In PrelimsTexts Where (String.Compare(Sections(s).InnerText, n, True) = 0) Select n).Any Then
                        If (Sections(s).ParentNode.Name = "info") Then
                            Paras = Sections(s).ParentNode.ParentNode.SelectNodes("./para")
                        Else
                            Paras = Sections(s).ParentNode.SelectNodes("./para")
                        End If
                        If ((Paras IsNot Nothing) AndAlso (Paras.Count > 0)) Then
                            For p As Int16 = 0 To Paras.Count - 1
                                Dim floatFig As String = Regex.Match(Paras(p).InnerText, "(^[0-9a-z\. ]+)\t", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
                                'Paras(p).InnerXml = Regex.Replace(Paras(p).InnerXml, "\t[0-9]+$", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase) ' 26-12-2022
                                Paras(p).InnerXml = Regex.Replace(Paras(p).InnerXml, "(((?<=\t[ixvlc0-9]*).)*)$", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase) ' 26-12-2022
                                If (Not String.IsNullOrEmpty(floatFig)) Then
                                    If (Sections(s).InnerText.ToLower().Contains("figure")) Then
                                        Paras(p).InnerXml = Paras(p).InnerXml.Replace(floatFig, $"<link role=""figure"" linkend=""F{floatFig.Replace(" ", "")}"">{floatFig}</link>")
                                    ElseIf (Sections(s).InnerText.ToLower().Contains("table")) Then
                                        Paras(p).InnerXml = Paras(p).InnerXml.Replace(floatFig, $"<link role=""table"" linkend=""T{floatFig.Replace(" ", "")}"">{floatFig}</link>")
                                    Else
                                        Paras(p).InnerXml = Paras(p).InnerXml.Replace(floatFig, $"<link role=""xxxx"" linkend=""X{floatFig.Replace(" ", "")}"">{floatFig}</link>")
                                    End If

                                End If
                            Next
                        End If
                    End If
                Next
            End If
        End If
        XmlContents = xmlDoc.OuterXml.Replace("&amp;", "&")
        Return XmlContents
    End Function

    Private Function ChapterNameSpace(ByVal mt As Match) As String
        Dim sResult As String = mt.Value
        sResult = sResult.Replace(" xmlns:xlink=""http://www.w3.org/1999/xlink""", "").Replace(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
        Return sResult
    End Function

    Private Function BibliographyCleanUp(ByVal XmlContent As String) As String
        xmlDoc = New XmlDocument
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(xmlDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        xmlDoc.XmlResolver = Nothing
        xmlDoc.PreserveWhitespace = True

        XmlContent = Regex.Replace(XmlContent, "<bibliomset[^><]*>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Replace("</bibliomset>", "")
        XmlContent = Regex.Replace(XmlContent, "</line>[\s]+<speaker>", "</line><speaker>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Try
            xmlDoc.LoadXml("<mfep xmlns:xlink=""http://www.w3.org/1999/xlink/"">" & XmlContent.Replace("&", "&amp;") & "</mfep>")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & "Bibligraphy cleanup", MessageType.MSGERROR)
            Return XmlContent
        End Try

        Dim BibliomixedList As XmlNodeList = xmlDoc.SelectNodes("//bibliomixed")
        If ((BibliomixedList IsNot Nothing) AndAlso (BibliomixedList.Count > 0)) Then
            For bb As Integer = 0 To BibliomixedList.Count - 1
                Try
                    If (String.IsNullOrEmpty(BibliomixedList(bb).InnerText)) Then
                        BibliomixedList(bb).ParentNode.RemoveChild(BibliomixedList(bb))
                    End If

                    BibliomixedList(bb).InnerXml = BibliomixedList(bb).InnerXml.Replace("<orgname><orgname>", "<orgname>").Replace("<authorgroup><author>", "<author>").Replace("</orgname></orgname>", "</orgname>").Replace("</author></authorgroup>", "</author>")

                    If (BibliomixedList(bb).InnerXml.Contains("<volumenum>") Or BibliomixedList(bb).InnerXml.Contains("<issuenum>")) Then
                        Dim titleList As XmlNodeList = BibliomixedList(bb).SelectNodes(".//title")
                        If ((titleList IsNot Nothing) AndAlso (titleList.Count > 0) AndAlso (titleList.Count = 2)) Then
                            BibliomixedList(bb).InnerXml = Regex.Replace(BibliomixedList(bb).InnerXml, "<bibliomset[^>]*>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Replace("</bibliomset>", "")
                            BibliomixedList(bb).InnerXml = "<bibliomset relation=""article"">" & BibliomixedList(bb).InnerXml.Replace(titleList(0).OuterXml, titleList(0).OuterXml & "</bibliomset><bibliomset relation=""journal"">") & "</bibliomset>"
                        End If
                        Try
                            BibliomixedList(bb).Attributes("role").Value = "article"
                        Catch ex As Exception
                            Continue For
                        End Try
                    ElseIf (BibliomixedList(bb).InnerXml.Contains("<publishername>") AndAlso BibliomixedList(bb).InnerXml.Contains("<address>")) Then
                        Dim titleList As XmlNodeList = BibliomixedList(bb).SelectNodes(".//title")
                        If ((titleList IsNot Nothing) AndAlso (titleList.Count > 0) AndAlso (titleList.Count = 2)) Then
                            BibliomixedList(bb).InnerXml = Regex.Replace(BibliomixedList(bb).InnerXml, "<bibliomset[^>]*>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Replace("</bibliomset>", "")
                            BibliomixedList(bb).InnerXml = "<bibliomset relation=""part"">" & BibliomixedList(bb).InnerXml.Replace(titleList(0).OuterXml, titleList(0).OuterXml & "</bibliomset><bibliomset relation=""book"">") & "</bibliomset>"
                            Try
                                BibliomixedList(bb).Attributes("role").Value = "contribution"
                            Catch ex As Exception
                                Continue For
                            End Try
                        ElseIf ((titleList IsNot Nothing) AndAlso (titleList.Count > 0) AndAlso (titleList.Count = 1)) Then
                            Try
                                BibliomixedList(bb).Attributes("role").Value = "monograph"
                            Catch ex As Exception
                                Continue For
                            End Try
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If

        'move figure outside the para
        Dim ParaList As XmlNodeList = xmlDoc.SelectNodes("//para[child::figure|child::table]")
        Dim ParaXML As String = String.Empty
        Dim FloatXML As String = String.Empty
        Dim MxCnt As Int16 = 0
        If ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0)) Then
            MxCnt = ParaList.Count * 3
            While ((ParaList IsNot Nothing) AndAlso (ParaList.Count > 0))
                If (MxCnt = 0) Then
                    Exit While
                End If
                ParaXML = String.Empty
                FloatXML = String.Empty
                ParaXML = ParaList(0).OuterXml
                Dim FloatNodes As XmlNodeList = ParaList(0).SelectNodes(".//figure|.//table")
                If ((FloatNodes IsNot Nothing) AndAlso (FloatNodes.Count > 0)) Then
                    For f As Integer = 0 To FloatNodes.Count - 1
                        ParaXML = ParaXML.Replace(FloatNodes(f).OuterXml, "")
                        FloatXML = FloatXML & FloatNodes(f).OuterXml
                    Next
                End If
                If (Not String.IsNullOrEmpty(FloatXML)) Then
                    'ParaList(p).InnerXml = ParaXML
                    If (ParaList(0).ParentNode IsNot Nothing) Then
                        ParaList(0).ParentNode.InnerXml = ParaList(0).ParentNode.InnerXml.Replace(ParaList(0).OuterXml, ParaXML & FloatXML)
                    End If
                End If
                ParaList = xmlDoc.SelectNodes("//para[child::figure|child::table]")
                MxCnt = MxCnt - 1
            End While
        End If

        Dim DialogueList As XmlNodeList = xmlDoc.SelectNodes("//dialogue/figure")

        If ((DialogueList IsNot Nothing) AndAlso (DialogueList.Count > 0)) Then
            MxCnt = DialogueList.Count * 3
            While (((DialogueList IsNot Nothing) AndAlso (DialogueList.Count > 0)))
                If (MxCnt = 0) Then
                    Exit While
                End If
                Try
                    If ((DialogueList(0).PreviousSibling IsNot Nothing) AndAlso (String.Compare(DialogueList(0).PreviousSibling.Name, "line", True) = 0)) Then
                        If (DialogueList(0).ParentNode IsNot Nothing) Then
                            DialogueList(0).PreviousSibling.PrependChild(DialogueList(0))
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGINFO)
                End Try
                DialogueList = xmlDoc.SelectNodes("//dialogue/figure")
                MxCnt = MxCnt - 1
            End While
        End If

        Dim SpeakerList As XmlNodeList = xmlDoc.SelectNodes("//dialogue")
        For dt As Integer = 0 To SpeakerList.Count - 1
            Try
                If (SpeakerList(dt).InnerXml.StartsWith("<speaker")) Then
                    'SpeakerList(dt).InnerXml = Regex.Replace(SpeakerList(dt).InnerXml, "(</line><a[^>]*>)(<speaker>)", "$1</linegroup>$2").Replace("</line><speaker>", "</line></linegroup><speaker>").Replace("<speaker>", "<linegroup><speaker>") & "</linegroup>"
                    If (SpeakerList(dt).InnerXml.EndsWith("</speaker>")) Then
                        SpeakerList(dt).InnerXml = Regex.Replace(SpeakerList(dt).InnerXml.Replace("<speaker>", "<linegroup><speaker>").Replace("</line>", "</line></linegroup>"), "</speaker>[\s]*<linegroup>", "</speaker>", RegexOptions.Singleline Or RegexOptions.IgnoreCase) & "</linegroup>"
                    Else
                        SpeakerList(dt).InnerXml = SpeakerList(dt).InnerXml.Replace("<speaker>", "<linegroup><speaker>").Replace("</line>", "</line></linegroup>").Replace("</speaker><linegroup>", "</speaker>")
                    End If
                    SpeakerList(dt).InnerXml = Regex.Replace(SpeakerList(dt).InnerXml, "(<speaker[^>]*>)", "$1<person><personname>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</speaker>", "</personname></person></speaker>")
                ElseIf (SpeakerList(dt).InnerXml.StartsWith("<line")) Then
                    SpeakerList(dt).InnerXml = "<linegroup>" & Regex.Replace(SpeakerList(dt).InnerXml, "(</line><a[^>]*>)(<speaker>)", "$1</linegroup>$2").Replace("</line><speaker>", "</line></linegroup><speaker>").Replace("<speaker>", "<linegroup><speaker>") & "</linegroup>"
                    SpeakerList(dt).InnerXml = Regex.Replace(SpeakerList(dt).InnerXml, "(<speaker[^>]*>)", "$1<person><personname>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</speaker>", "</personname></person></speaker>")
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next

        Dim LinkLst As XmlNodeList = xmlDoc.SelectNodes("//text()")
        Dim MaxList As Integer = 0
        Dim UrlXml As String = String.Empty
        Dim OrgUrlXML As String = String.Empty
        MaxList = LinkLst.Count - 1
        If ((LinkLst IsNot Nothing) AndAlso (LinkLst.Count > 0)) Then
            For k As Integer = 0 To LinkLst.Count - 1
                Try
                    If (String.IsNullOrEmpty(LinkLst(k).InnerText.Trim())) Then
                        Continue For
                    End If
                    If ((LinkLst(k).ParentNode IsNot Nothing) AndAlso (String.Compare(LinkLst(k).ParentNode.Name, "uri", True) = 0)) Then
                        Continue For
                    End If
                    UrlXml = LinkLst(k).InnerText
                    OrgUrlXML = UrlXml
                    Dim UrlMatchs As MatchCollection = Regex.Matches(UrlXml, "((http:|https:|www\\.)([^ ><)]+))", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                    If ((UrlMatchs IsNot Nothing) AndAlso (UrlMatchs.Count > 0)) Then
                        For l As Integer = 0 To UrlMatchs.Count - 1
                            Dim Urltxt As String = UrlMatchs(0).Value
                            If (Urltxt.EndsWith(".")) Then
                                Urltxt = Urltxt.TrimEnd(".")
                                UrlXml = UrlXml.Replace(UrlMatchs(l).Value, String.Format("<link xlink:href=""{0}""><uri>{0}</uri></link>.", Urltxt))
                            Else
                                UrlXml = UrlXml.Replace(UrlMatchs(l).Value, String.Format("<link xlink:href=""{0}""><uri>{0}</uri></link>", UrlMatchs(0).Value))
                            End If
                        Next
                        If (LinkLst(k).ParentNode IsNot Nothing) Then
                            LinkLst(k).ParentNode.InnerXml = LinkLst(k).ParentNode.InnerXml.Replace(OrgUrlXML.Replace("&", "&amp;"), UrlXml.Replace("&", "&amp;"))
                        End If
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If

        'true based on label, false based on ID
        Try
            MoveFootnotes(NameSpaceManager, False)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        '04-05-2021
        Dim Figures As XmlNodeList = xmlDoc.SelectNodes("//figure//imagedata|//informalfigure//imagedata")
        If ((Figures IsNot Nothing) AndAlso (Figures.Count > 0)) Then
            For f As Integer = 0 To Figures.Count - 1
                Try
                    Figures(f).Attributes("fileref").Value = Figures(f).Attributes("fileref").Value.Replace(".eps", ".jpg").Replace(".tif", ".jpg")
                Catch ex As Exception
                End Try
            Next
        End If

        Dim NoteChapterNode As XmlNode = xmlDoc.SelectSingleNode("//chapter/title[text()='Notes']")
        If (NoteChapterNode IsNot Nothing) Then
            If ((NoteChapterNode.ParentNode IsNot Nothing) AndAlso (NoteChapterNode.ParentNode.ParentNode IsNot Nothing)) Then
                NoteChapterNode.ParentNode.ParentNode.RemoveChild(NoteChapterNode.ParentNode)
            End If
        End If

        Dim BibliNodes As XmlNodeList = xmlDoc.SelectNodes("//bibliomixed[@role='misc']")
        If ((BibliNodes IsNot Nothing) AndAlso (BibliNodes.Count > 0)) Then
            For b As Int16 = 0 To BibliNodes.Count - 1
                Try
                    BibliNodes(b).Attributes("role").Value = "series"
                Catch ex As Exception
                End Try
            Next
        End If

        Dim MdCunt As Int16 = 0
        Dim Colspecs As XmlNodeList = xmlDoc.SelectNodes("//colspec")
        If ((Colspecs IsNot Nothing) AndAlso (Colspecs.Count > 0)) Then
            MdCunt = Colspecs.Count * 2
            While ((Colspecs IsNot Nothing) AndAlso (Colspecs.Count > 0))
                If (MdCunt = 0) Then
                    Exit While
                End If
                If (Colspecs(0).ParentNode IsNot Nothing) Then
                    Colspecs(0).ParentNode.RemoveChild(Colspecs(0))
                End If
                Colspecs = xmlDoc.SelectNodes("//colspec")
                MdCunt = MdCunt - 1
            End While
        End If

        Dim EntryNodes As XmlNodeList = xmlDoc.SelectNodes("//entry[count(child::tp)=1]/tp")
        If ((EntryNodes IsNot Nothing) AndAlso (EntryNodes.Count > 0)) Then
            For e As Int16 = 0 To EntryNodes.Count - 1
                If (EntryNodes(e).ParentNode IsNot Nothing) Then
                    EntryNodes(e).ParentNode.InnerXml = EntryNodes(e).ParentNode.InnerXml.Replace(EntryNodes(e).OuterXml, EntryNodes(e).InnerXml)
                End If
            Next
        End If

        EntryNodes = xmlDoc.SelectNodes("//entry[count(child::tp)>1]")
        If ((EntryNodes IsNot Nothing) AndAlso (EntryNodes.Count > 0)) Then
            For e As Int16 = 0 To EntryNodes.Count - 1
                EntryNodes(e).InnerXml = EntryNodes(e).InnerXml.Replace("<tp>", "<para>").Replace("</tp>", "</para>")
            Next
        End If



        XmlContent = xmlDoc.OuterXml.Replace("&amp;", "&")
        XmlContent = XmlContent.Replace("<bibliomset relation=""book""> ", "<bibliomset relation=""book"">")

        XmlContent = Regex.Replace(XmlContent, "<mfep[^>]*>", "").Replace("</mfep>", "")
        Return XmlContent
    End Function

    Private Function MoveFootnotes(ByVal NameSpaceManager As XmlNamespaceManager, ByVal IsBasedOnLabel As Boolean) As Boolean
        Dim FootnoteList As XmlNodeList = xmlDoc.SelectNodes("//footnote[@linkend]")
        Dim FootnoteID As String = String.Empty
        Dim Label As String = String.Empty
        If ((FootnoteList IsNot Nothing) AndAlso (FootnoteList.Count > 0)) Then
            For ft As Integer = 0 To FootnoteList.Count - 1
                Try
                    Dim FtNode As XmlNode = FootnoteList(ft)
                    Try
                        FootnoteID = FtNode.Attributes("linkend").Value
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                    Try
                        Label = FtNode.Attributes("label").Value
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                    If (String.IsNullOrEmpty(FootnoteID)) Then
                        GBL.DeantaBallon("xml:id attribute value not found. Please check." & FtNode.OuterXml, MessageType.MSGERROR)
                        Continue For
                    End If
                    Dim SecFtNote As XmlNode = GetFootnoteSection(FootnoteID, NameSpaceManager, Label, IsBasedOnLabel)
                    If (SecFtNote Is Nothing) Then
                        GBL.DeantaBallon("linkend id found, but the respective footnote xml:id not found." & FootnoteID, MessageType.MSGERROR)
                        Continue For
                    End If
                    If ((String.IsNullOrEmpty(FtNode.InnerText)) OrElse (String.IsNullOrEmpty(SecFtNote.InnerText))) Then
                        Try
                            GBL.DeantaBallon("footntoe removed: " & FtNode.OuterXml, MessageType.MSGERROR)
                            Try
                                FtNode.ParentNode.RemoveChild(FtNode)
                                SecFtNote.ParentNode.RemoveChild(SecFtNote)
                            Catch ex As Exception
                                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            End Try
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        End Try
                    End If

                    Try
                        FtNode.InnerXml = SecFtNote.InnerXml
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Continue For
                    End Try
                    Try
                        FtNode.Attributes.Remove(FtNode.Attributes("linkend"))
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                    Try
                        Dim xmlIDAttrib As XmlAttribute = xmlDoc.CreateNode(XmlNodeType.Attribute, "xml:id", "xml")
                        xmlIDAttrib.Value = FootnoteID
                        FtNode.Attributes.Append(xmlIDAttrib)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                    SecFtNote.ParentNode.RemoveChild(SecFtNote)

                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function
    Private Function GetFootnoteSection(ByVal FootNoteID As String, ByVal NameSpaceManager As XmlNamespaceManager, ByVal Label As String, ByVal IsBasedOnLabel As Boolean) As XmlNode
        Dim FtXMLNode As XmlNode = Nothing
        Dim sFtID As String = String.Empty
        Dim ParticalID As String = String.Empty
        Dim footnoteList As XmlNodeList = Nothing
        If (IsBasedOnLabel) Then
            Try
                ParticalID = FootNoteID.Split("_")(0)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return Nothing
            End Try
            footnoteList = xmlDoc.SelectNodes($"//footnote[contains(@xml:id,'{ParticalID}') and @label='{Label}']", NameSpaceManager)
            If ((footnoteList IsNot Nothing) AndAlso (footnoteList.Count > 0)) Then
                Return footnoteList(0)
            End If
        Else
            footnoteList = xmlDoc.SelectNodes($"//footnote[@xml:id='{FootNoteID}']", NameSpaceManager)
            If ((footnoteList IsNot Nothing) AndAlso (footnoteList.Count > 0)) Then
                Return footnoteList(0)
            End If
        End If

        'If ((footnoteList IsNot Nothing) AndAlso (footnoteList.Count > 0)) Then
        '    For Each FtXMLNode In footnoteList
        '        Try
        '            sFtID = FtXMLNode.Attributes("xml:id").Value
        '        Catch ex As Exception
        '            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '            Continue For
        '        End Try
        '        If (String.IsNullOrEmpty(sFtID)) Then
        '            GBL.DeantaBallon("linkend attribute not found." & FtXMLNode.OuterXml, MessageType.MSGERROR)
        '            Continue For
        '        End If
        '        If (String.Compare(sFtID, FootNoteID, True) = 0) Then
        '            Return FtXMLNode
        '        End If
        '    Next
        'End If
        Return Nothing
    End Function


    Private Function FigureCaptionParaPro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "</para>", "</1para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return sResult
    End Function

    Private Function BiblioGraphyClean(m As Match)
        Dim sResult As String = m.Value
        sResult = sResult.Replace("<authorgroup>", "").Replace("</authorgroup>", "")
        Dim biblio As String = m.Groups(1).Value
        If (biblio.Contains("<bibliomixed role=""contributors""")) Then
            sResult = sResult.Replace("<bibliomixed role=""contributors""", "<bibliomixed role=""contribution""")
        End If
        If (biblio.Contains(" role=""newspaper""")) Then
            sResult = sResult.Replace(" role=""newspaper""", " role=""series""")
        End If
        If (m.Groups(2).Value.Contains("<uri>http")) Then
            biblio = biblio.Replace("role=""other""", "role=""website""")
        End If
        sResult = sResult.Replace(m.Groups(1).Value, biblio)
        Return sResult
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

    Private Function FootntSeqPro(m As Match)
        Dim sresult As String = m.Value.ToString
        If Not sresult.ToString.Contains("label=") Then Return sresult
        sresult = m.Groups(1).Value.ToString
        sresult = Regex.Replace(sresult, "label=""([^""]+)""", "label=""" & m.Groups(5).Value.ToString & """")
        Return sresult
    End Function

    Private Function SectionNoPro(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim mt As Match = Regex.Match(sResult, "<title([^><]+)?>(\d+)&#x2002;", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If Not mt.Success Then Return sResult
        'sResult = Regex.Replace(sResult, "(<title([^><]+)?>)(\d+)&#x2002;", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline) '29-06-2021
        sResult = Regex.Replace(sResult, "(<title([^><]+)?>)&#x2002;", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline) '29-06-2021
        sResult = Regex.Replace(sResult, "<section([^><]+)?>", "<section label=""" & mt.Groups(2).Value.ToString & """$1>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function Biboloset(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim Year As String = Regex.Match(sResult, "<printhistory[^>]*>((?:(?!<\/printhistory>).)*)</printhistory>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
        Year = Regex.Match(Year, " [0-9]{4,4}", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value.Trim()
        If (String.IsNullOrEmpty(Year)) Then
            Year = DateTime.Now.Year
        End If
        Dim Bibliomisc As String = Regex.Match(sResult, "<bibliomisc role=""imprint""[^>]*>((?:(?!<\/bibliomisc>).)*)</bibliomisc>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
        If (String.IsNullOrEmpty(Bibliomisc)) Then
            Bibliomisc = "BLOOMSBURY ACADEMIC"
        End If
        Dim sTxt As String = $"<edition role={Chr(34)}1{Chr(34)}>First edition</edition>" & Environment.NewLine & $"<pubdate role={Chr(34)}published{Chr(34)}>{Year}</pubdate>" & Environment.NewLine &
                $"<biblioset role={Chr(34)}publisher{Chr(34)} xml:id={Chr(34)}ba-0005{Chr(34)}>" & Environment.NewLine &
                $"<printhistory xml:id={Chr(34)}ba-FM-0005a{Chr(34)}><para xml:id={Chr(34)}pa-FM-0000002{Chr(34)}>First published in Great Britain {Year}</para></printhistory>" & Environment.NewLine &
                $"<bibliomisc role={Chr(34)}imprint{Chr(34)}>{Bibliomisc}</bibliomisc>" & Environment.NewLine & "<publisher>" & Environment.NewLine &
                $"<publishername>Bloomsbury Publishing Plc</publishername>" & Environment.NewLine & $"<address xml:id={Chr(34)}adr-0001{Chr(34)}>" & Environment.NewLine &
                $"<street>50 Bedford Square</street> <city>London</city> <postcode>WC1B 3DP</postcode> <country>UK</country>" & Environment.NewLine &
                $"</address>" & Environment.NewLine & $"<address xml:id={Chr(34)}adr-0002{Chr(34)}>" & Environment.NewLine &
                $"<street>1385 Broadway</street> <city>New York</city> <postcode>NY 10018</postcode> <country>USA</country>" & Environment.NewLine &
                $"</address>{Environment.NewLine}<address xml:id={Chr(34)}adr-0003{Chr(34)}>" & Environment.NewLine &
                $"<street>29 Earlsfort Terrace</street> <city>Dublin 2</city> <country>Ireland</country>" & Environment.NewLine &
                $"<phrase>BLOOMSBURY, BLOOMSBURY ACADEMIC and the Diana logo are trademarks of Bloomsbury Publishing Plc</phrase>" & Environment.NewLine &
                $"</address>" & Environment.NewLine & "</publisher></biblioset>"
        'sTxt = sTxt.Replace("~", Chr(34))
        'sResult = sResult.Replace(m.Groups(1).Value.ToString, sTxt) '24-06-2020
        'Return sResult
        Return sTxt
    End Function

    Private Function BiblioIdPro(m As Match)
        Dim sResult As String = m.Value.ToString
        Dim sTxt As String = Regex.Replace(m.Groups(2).Value.ToString, "[a-z:]", "", RegexOptions.IgnoreCase).Trim
        sResult = sResult.Replace(m.Groups(2).Value.ToString, sTxt)
        Return sResult
    End Function

    Private Function ChapInfoPro(m As Match)
        Dim sResult As String = m.Value.ToString
        iChap = +1
        sResult = Regex.Replace(sResult, "<info>", $"<info xml:id=""b-{sISBN}-ch{iChap}-ba-00000{iChap}"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function HardCorePro(m As Match)
        Dim sTxt As String = $"</legalnotice>|<biblioid class={Chr(34)}doi{Chr(34)}>10.5040/" & sISBN & $"</biblioid>|<biblioid class={Chr(34)}other{Chr(34)} otherclass={Chr(34)}schemaVersion{Chr(34)}>1</biblioid>|" &
            $"<biblioid class={Chr(34)}other{Chr(34)} otherclass={Chr(34)}schematronVersion{Chr(34)}>4</biblioid>|<abstract role={Chr(34)}blurb{Chr(34)} xml:id={Chr(34)}ba-blurb1{Chr(34)}>|<para></para>|</abstract>|"
        sTxt = sTxt & m.Groups(1).Value.ToString & Environment.NewLine & m.Groups(3).Value.ToString
        sTxt = sTxt & $"<part xml:id={Chr(34)}ba-FM-front{Chr(34)} role={Chr(34)}front{Chr(34)}>|<info xml:id={Chr(34)}in-0002{Chr(34)}>|<title xml:id={Chr(34)}tt-0002{Chr(34)}>Front matter</title>|</info>|" &
            $"<preface role={Chr(34)}prelims{Chr(34)} xml:id={Chr(34)}b-" & sISBN & $"-title{Chr(34)}>|<info xml:id={Chr(34)}ba-FM-" & sISBN & $"-prelim-id{Chr(34)}>|<title xml:id={Chr(34)}ba-FM-" & sISBN & "-prelim-id{Chr(34)}>Title Pages</title>|" &
            $"<pagenums/>|<mediaobject xml:id={Chr(34)}ba-FM-" & sISBN & $"-prelim-id{Chr(34)}>|<imageobject xml:id={Chr(34)}ba-FM-" & sISBN & $"-prelim-id{Chr(34)}>|<imagedata fileref={Chr(34)}pdfs/" & sISBN & $".0001.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}/>|" &
            $"</imageobject>|</mediaobject>|</info>|<remark condition={Chr(34)}hidden{Chr(34)}>Note that this is a placeholder for the pdf of the prelims and no full text content is included at this point</remark>|" &
            $"</preface>|<dedication xml:id={Chr(34)}b-" & sISBN & $"-dedi{Chr(34)}>|<info xml:id={Chr(34)}bo-id{Chr(34)}>|<title outputformat={Chr(34)}e-Only{Chr(34)} xml:id={Chr(34)}tt-003{Chr(34)}>Dedication</title>|<pagenums/>|" &
            $"<mediaobject xml:id={Chr(34)}ba-000000d4{Chr(34)}>|<imageobject xml:id={Chr(34)}ba-000df0005{Chr(34)}>|<imagedata fileref={Chr(34)}pdfs/" & sISBN & $".0002.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}/>|" &
            $"</imageobject>|</mediaobject>|</info>|<para></para>|</dedication>|<toc xml:id={Chr(34)}b-" & sISBN & $"-toc{Chr(34)}>|<info xml:id={Chr(34)}in-0006{Chr(34)}>|<title xml:id={Chr(34)}tt-00zsdf06{Chr(34)}>" &
            $"<?page value={Chr(34)}vii{Chr(34)}?>Contents</title>|<pagenums>vii</pagenums>|<mediaobject xml:id={Chr(34)}ba-FM-toc-001c{Chr(34)}>|<imageobject xml:id={Chr(34)}ba-FM-toc-001d{Chr(34)}>|" &
            $"<imagedata fileref={Chr(34)}pdfs/9781844864041.0003.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}/>|</imageobject>|</mediaobject>|</info>|</toc>"
        sTxt = sTxt.Replace("|", Environment.NewLine)
        'sTxt = sTxt & Environment.NewLine & m.Groups(3).Value.ToString
        Return sTxt
    End Function

    Private iVal As Integer = 0
    ' Updated on Sep 27, 2016 based on Jaffar request
    Private Function UpdatePro(ByVal sChapterTxt As String) As String
        '
        sChapterTxt = Regex.Replace(sChapterTxt, "(<acknowledgements([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sChapterTxt = Regex.Replace(sChapterTxt, "(<toc([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<cover([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<abstract([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<footnote([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<address([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliodiv([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliolist([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliomixed role=""series""([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<bibliography([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<para([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<legalnotice([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If Not bExecuteOnce Then
            iVal = 0
            sChapterTxt = Regex.Replace(sChapterTxt, "(<part([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
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
        sChapterTxt = Regex.Replace(sChapterTxt, "(<dialogue([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<speaker([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<itemizedlist([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<listitem([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<orderedlist([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
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
        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<entry([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<informalfigure([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
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
        iVal = 0
        'sChapterTxt = Regex.Replace(sChapterTxt, "(<table([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        iVal = 0
        sChapterTxt = Regex.Replace(sChapterTxt, "(<chapter([^><]+)?)>", AddressOf ChapterPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        If Not bExecuteOnce Then
            iVal = 0

            sChapterTxt = Regex.Replace(sChapterTxt, "(<part ([^><]+)?)>", AddressOf PartPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Else
            iVal = 0
            sChapterTxt = Regex.Replace(sChapterTxt, "(<title([^><]+)?)>", AddressOf IDGen, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        bExecuteOnce = True
        Return sChapterTxt
    End Function

    Private Function ChapterProDED(m As Match)
        Dim sHardcode As String = $"<info xml:id=""ch{iChap}-ba-00000{iChap}""><title outputformat={Chr(34)}e-Only{Chr(34)} xml:id={Chr(34)}pref-55-bo-000sdf26{Chr(34)}>Dedication</title><pagenums></pagenums>|<biblioid class={Chr(34)}doi{Chr(34)}>10.5040/" & sISBN & ".000" & iChap & $"</biblioid>|<mediaobject xml:id={Chr(34)}ch" & iChap & "-ba-000000" & iChap & $"{Chr(34)}>|
            <imageobject xml:id={Chr(34)}ch" & iChap & $"-ba-0000005{Chr(34)}>|<imagedata fileref={Chr(34)}pdfs/" & sISBN & $".0006.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}></imagedata>|</imageobject>|
            </mediaobject>|</info>"
        sHardcode = sHardcode.Replace("|", "")
        Dim sResult As String = m.Groups(1).Value.ToString
        sResult = Regex.Replace(sResult, "<dedication[^>]*>", "<dedication xml:id=""b-" & sISBN & "-dedication"">", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        sResult = sResult & Environment.NewLine & sHardcode & Environment.NewLine & m.Groups(3).Value.ToString
        Return sResult
    End Function
    Private Function ChapterProHC(m As Match)
        Dim IsBiblio As Boolean = False
        Dim IsIndex As Boolean = False
        Dim sAuthors As Match = Regex.Match(m.Value.ToString, "<author>(.+)</author>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'If Regex.IsMatch(m.Value.ToString, "<author>(.+)</author>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
        'End If
        If (Regex.Match(m.Value.ToString(), "<chapter[^>]*><glossary>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then
            Return m.Value.ToString()
        End If
        If (Regex.Match(m.Value.ToString(), "<chapter[^>]*><para><informalfigure>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then
            Return m.Value.ToString()
        End If


        If (Regex.Match(m.Value.ToString(), "<figure", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then
            Return m.Value.ToString()
        End If

        Dim sResult As String = m.Groups(1).Value.ToString & Environment.NewLine & "<info xml:id=""ch" & iChap & "-ba-00000" & iChap & """>" & Environment.NewLine & m.Groups(3).Value.ToString

        If (sResult.Contains("<section role=""")) Then Return m.Value
        If (Regex.Match(sResult, "<title[^>]*>Notes</title>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then Return m.Value

        Dim sHardcode As String = $"|<pagenums></pagenums>|<biblioid class={Chr(34)}doi{Chr(34)}>10.5040/" & sISBN & ".000" & iChap & $"</biblioid>|<mediaobject xml:id={Chr(34)}ch" & iChap & "-ba-000000" & iChap & $"{Chr(34)}>|
            <imageobject xml:id={Chr(34)}ch" & iChap & $"-ba-0000005{Chr(34)}>|<imagedata fileref={Chr(34)}pdfs/" & sISBN & $".0006.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}></imagedata>|</imageobject>|
            </mediaobject>|"
        If sAuthors.Success Then
            sResult = sResult.Replace(sAuthors.Value.ToString, "")
            sHardcode = Regex.Replace(sHardcode, "<pagenums>", "<authorgroup>|" & sAuthors.Value.ToString & "</authorgroup>|<pagenums>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        sHardcode = sHardcode.Replace("|", "")
        sResult = sResult & sHardcode

        'If iChap = 1 Then sResult = sResult.Replace("<chapter", "</part><chapter")
        sResult = Regex.Replace(sResult, "<info ([^><]+)?></info>", "", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, "<info></info>", "", RegexOptions.IgnoreCase)
        sResult = Regex.Replace(sResult, "<info[/s]*/>", "", RegexOptions.IgnoreCase)
        If Not sResult.EndsWith("</info>") Then
            sResult = sResult & "</info>" & Environment.NewLine
        End If
        If (Regex.IsMatch(sResult, "(<section>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline)) Then
            sResult = Regex.Replace(sResult, "(<section>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", "$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResult = sResult & "<section>"
        End If

        If (Regex.Match(sResult, "(<bibliography>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
            IsBiblio = True
        End If
        If (Regex.Match(sResult, "(<index>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Success) Then
            IsIndex = True
        End If
        sResult = Regex.Replace(sResult, "(<bibliography>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", "$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "(<index>)(<title([^><]+)?>((?:(?!</title>).)+)</title>)", "$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If (IsBiblio) Then
            sResult = sResult & "<bibliography>"
        End If
        If (IsIndex) Then
            sResult = sResult & "<index>"
        End If
        Return sResult.Replace(vbCrLf, "")
    End Function

    Private Function ChapterPro1(m As Match)
        Dim sResult As String = m.Value.ToString
        If Regex.IsMatch(sResult, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
            Dim smtch As Match = Regex.Match(sResult, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If smtch.Value.ToString.ToLower.Contains("introduction") Then
                sResult = Regex.Replace(sResult, "-chapter\d+", "-intro", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            End If
        End If
        Dim mt As Match = Regex.Match(sResult, "<label>((?:(?!</label>).)+)</label>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If mt.Success Then
            Dim sLbl As String = mt.Groups(1).Value.ToString
            sLbl = Regex.Replace(sLbl, "<[^><]+>", "", RegexOptions.IgnoreCase)
            sResult = Regex.Replace(sResult, "<label>((?:(?!</label>).)+)</label>", "", RegexOptions.IgnoreCase)
            sResult = Regex.Replace(sResult, "<chapter", "<chapter label=""" & sLbl & """", RegexOptions.IgnoreCase)
        End If
        Return sResult
    End Function

    Private Function PrefacePro(m As Match)
        Dim sInput As String = m.Value.ToString
        Dim sResults As String = String.Empty
        Dim sTxt As String = $"<info xml:id={Chr(34)}ba-0000004e{Chr(34)}>|<title xml:id={Chr(34)}b-0003g{Chr(34)}></title>|<pagenums></pagenums>|<mediaobject xml:id={Chr(34)}ba-0000004f{Chr(34)}>|" &
            $"<imageobject xml:id={Chr(34)}ba-0000005f{Chr(34)}>|<imagedata fileref={Chr(34)}pdfs/" & sISBN & $".0004.pdf{Chr(34)} format={Chr(34)}application/pdf{Chr(34)}/>|</imageobject>|</mediaobject>|</info>"
        If Not String.IsNullOrEmpty(sInput) Then
            If Regex.IsMatch(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline) Then
                Dim smt As Match = Regex.Match(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                If smt.Groups(2).Value.ToString.ToLower.Contains("contributors") Then
                    sResults = "<preface xml:id=""b-" & sISBN & "-contributors"">"
                ElseIf smt.Groups(2).Value.ToString.ToLower.Contains("abbreviation") Then
                    sResults = "<preface xml:id=""b-" & sISBN & "-abbrev"">"
                Else
                    Dim sTit As String = Regex.Replace(smt.Groups(2).Value.ToString, "<footnote[^>]*>(((?!<\/footnote[^>]*>).)*)<\/footnote[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                    'sTit = Regex.Replace(sTit, "(<superscript>(.+)</superscript>|<(/)?emphasis>)", "", RegexOptions.IgnoreCase)
                    sTit = Regex.Replace(sTit, "<[^>]*>", "", RegexOptions.IgnoreCase) '22-11-2021
                    sResults = "<preface xml:id=""b-" & sISBN & "-" & sTit & Chr(34) & ">"
                End If
                Dim sMtch As Match = Regex.Match(sInput, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                If sMtch.Success Then
                    Dim sTit As String = sMtch.Groups(2).Value.ToString
                    sTit = Regex.Replace(sTit, "(<superscript>(.+)</superscript>)", "", RegexOptions.IgnoreCase)
                    sTxt = sTxt.Replace("</title>", sTit & "</title>")
                End If
                sResults = sResults & sTxt.Replace("|", Environment.NewLine)
                'sResults = Regex.Replace(sResults, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                sInput = Regex.Replace(sInput, "<preface([^><]*)?>", "", RegexOptions.IgnoreCase)
                'sResults = sResults & " xml:id=" & Chr(34) & "b-" & sISBN & "-" & sTag & Chr(34) & ">"
                If (sInput.StartsWith("<title")) Then
                    sInput = sInput.Replace(smt.Value.ToString, sResults.ToString)
                Else
                    sInput = sInput.Replace(smt.Value.ToString, "")
                    sInput = sResults.ToString & sInput
                End If
            End If
        End If
        Return sInput.Replace("</pre11>", "</preface>").Replace("<pre11 role=""prelims"">", "")
    End Function

    Private Function PartPro(m As Match)
        Dim sInput As String = m.Groups(1).Value.ToString
        If Not String.IsNullOrEmpty(sInput) Then
            sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        iVal = +1
        Return "<part xml:id=""b-" & sISBN & "-part" & iVal & """>"
    End Function

    Private Function ChapterPro(m As Match)
        If m.Value.ToString.Contains("/>") Then Return m.Value.ToString
        Dim sInput As String = m.Groups(1).Value.ToString
        If Not String.IsNullOrEmpty(sInput) Then
            sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        iVal = iVal + 1
        Return sInput & " xml:id=""b-" & sISBN & "-chapter" & iVal & """>"
    End Function

    Private iSec As Integer = 1
    Private iFootnote As Integer = 1

    Private Function FootnotePro(m As Match)
        If Not m.Value.ToString.EndsWith(">") Then Return m.Value.ToString
        Dim sInput As String = m.Groups(1).Value.ToString
        Dim xmlid As String = String.Empty
        If (sInput.Contains("linkend=""")) Then Return m.Value
        xmlid = Regex.Match(sInput, "xml:id=""([^""])+""", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        xmlid = xmlid.Replace("xml:id=", "").Replace("""", "")
        Dim smt As Match = Regex.Match(sInput, " label=""(([^""])+)""", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, " label=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = Regex.Replace(sInput, "xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim sLbl As String = smt.Groups(1).Value.ToString
        If String.IsNullOrEmpty(sLbl) Then sLbl = iFootnote
        sInput = sInput & " role=" & Chr(34) & "end-bk-note" & Chr(34) & " label=" & Chr(34) & sLbl & Chr(34) & " xml:id=""" & xmlid & """>"
        'If String.IsNullOrEmpty(smt.Groups(1).Value.ToString) Then
        '    sInput = sInput & " role=" & Chr(34) & "end-bk-note" & Chr(34) & " label=" & Chr(34) & sLbl & Chr(34) & " xml:id=""note" & iFootnote & "-ba-" & String.Format("{0:00000}", iFootnote) & """>"
        'Else
        '    sInput = sInput & " role=" & Chr(34) & "end-bk-note" & Chr(34) & " label=" & Chr(34) & sLbl & Chr(34) & " xml:id=""note" & iFootnote & "-ba-" & String.Format("{0:00000}", iFootnote) & """>"
        'End If
        iFootnote = iFootnote + 1
        Return sInput
    End Function

    Private Function SectionPro(m As Match)
        If Not m.Value.ToString.EndsWith(">") Then Return m.Value.ToString
        Dim sInput As String = m.Groups(1).Value.ToString
        sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sInput = sInput & " xml:id=" & Chr(34) & "ch-" & iVal & "-sec-" & iSec & Chr(34) & ">"
        Return sInput
    End Function

    Private Function IDGen(m As Match)
        If Not m.Value.ToString.EndsWith(">") OrElse m.Value.ToString.Contains("/>") Then Return m.Value.ToString
        Dim sResults As String = String.Empty
        Dim OldXMLID As String = String.Empty
        Dim sInput As String = m.Groups(1).Value.ToString
        OldXMLID = Regex.Match(sInput, " xml:id=""([^""])+""", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        sInput = Regex.Replace(sInput, " xml:id=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If (Not String.IsNullOrEmpty(OldXMLID)) Then
            OldXMLID = Regex.Match(OldXMLID, "\-(.*?)_", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Groups(1).Value
        End If
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
            sResults = sInput & " xml:id=" & Chr(34) & "b-pa-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<index") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-index" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<footnote") Then
            If (String.IsNullOrEmpty(OldXMLID)) Then
                sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-footnote" & iVal & Chr(34) & ">"
            Else
                sResults = sInput & " xml:id=" & Chr(34) & $"b-{sISBN}-{OldXMLID}-b-{sDigit}{iVal}"">"
            End If
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliography") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-bib" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossary") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-glossary" & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<dialogue") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-da-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<itemizedlist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-item-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<listitem") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-list-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<orderedlist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-order-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<speaker") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-sp-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glosslist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-glossl-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossentry") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-glosse-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossterm") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-glosst-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<glossdef") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-glossd-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliolist") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-bibl-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<partintro") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-ptint-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<keyword") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-key-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<keywordset") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-key-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<itemset") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-itms-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<tfoot") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-tfoot-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<sidebar") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-side-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<line") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-line-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<linegroup") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-lineg-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<epigraph") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-epig-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<inlinemediaobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-inlinemedo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<imageobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-imgo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<mediaobject") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-medo-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<informaltable") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-infotab-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<poetry") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-poet-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<entry") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-entr-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<informalfigure") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-infofig-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<colophon") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-colph-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliodiv") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-bibd-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<address") Then
            sResults = sInput & " xml:id=" & Chr(34) & "b-bibd-adr-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<biblioset") Then
            sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " role=""publisher"" xml:id=" & Chr(34) & "bibs-" & sDigit & iVal & Chr(34) & ">"
        ElseIf m.Groups(1).Value.ToString.Contains("<bibliomixed") Then
            'sInput = Regex.Replace(sInput, " role=""([^""])+""", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            sResults = sInput & " xml:id=" & Chr(34) & "b-bibl-" & sDigit & iVal & Chr(34) & ">"
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
            sResults = sInput & " xml:id=" & Chr(34) & "b-" & sISBN & "-" & sDigit & iVal & Chr(34) & ">"
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
        Dim sTxt2Remove As String = $"<book-meta xmlns:fo={Chr(34)}http://www.w3.org/1999/XSL/Format{Chr(34)} xmlns:xlink={Chr(34)}http://www.w3.org/1999/xlink{Chr(34)} xmlns:msxsl={Chr(34)}urn:schemas-microsoft-com:xslt{Chr(34)} " &
                $"xmlns:d={Chr(34)}http://docbook.org/ns/docbook{Chr(34)} xmlns:aid={Chr(34)}http://ns.adobe.com/AdobeInDesign/4.0/{Chr(34)} xmlns:aid5={Chr(34)}http://ns.adobe.com/AdobeInDesign/5.0/{Chr(34)} xmlns:code={Chr(34)}urn:schemas-test-code{Chr(34)}>"
        sXMLContent = Regex.Replace(sXMLContent, sTxt2Remove.ToString, "<book-meta>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
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

    ' Execute XSL file code from Muthu
    Private Sub CallingXSLPro(sXMLFile As String)
        Dim OutputPath As String = String.Empty
        Dim BatFileContent As String = String.Empty
        OutputPath = Path.Combine(Path.GetDirectoryName(sXMLFile), "BlsConversion")
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
        If File.Exists(Path.Combine(AppPath, "docschemtron.xsl")) Then File.Copy(Path.Combine(AppPath, "docschemtron.xsl"), OutputPath & "\docschemtron.xsl")
        'If File.Exists(Path.Combine(AppPath, "TNF-XML.xsl")) Then File.Copy(Path.Combine(AppPath, "TNF-XML.xsl"), OutputPath & "\TNF-XML.xsl") 
        If File.Exists(sXMLFile) Then File.Copy(sXMLFile, Path.Combine(OutputPath, Path.GetFileName(sXMLFile)))

        BatFileContent = "java -jar """ & Path.GetFileName(Path.Combine(AppPath, "saxon9.jar")) & """ -s:""" & Path.GetFileName(sXMLFile) & """ -xsl:""" &
                                                           Path.GetFileName(Path.Combine(AppPath, "docschemtron.xsl")) & """ -o:""" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml" & """"

        If (Not CreateBatAndRunFile(BatFileContent, OutputPath)) Then
            'GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
            GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
        End If
        If File.Exists(OutputPath & "\" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml") Then
            File.Copy(OutputPath & "\" & Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml", Path.Combine(Path.GetDirectoryName(sXMLFile), Path.GetFileNameWithoutExtension(sXMLFile) & "_xsl.xml"), True)
        End If
    End Sub
    Public Function UpdateIbidinFootnote(ByVal FinalXML As String) As Boolean
        Dim xmlBlsDoc As New XmlDocument
        xmlBlsDoc.PreserveWhitespace = True
        Try
            xmlBlsDoc.LoadXml(File.ReadAllText(FinalXML).Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(xmlBlsDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        NameSpaceManager.AddNamespace("d", "http://docbook.org/ns/docbook")
        Dim FtNodes As XmlNodeList = xmlBlsDoc.SelectNodes("//d:footnote/d:para[not (child::d:alt)]|//fn/p[not (child::d:alt)]", NameSpaceManager)
        Dim Author As String = String.Empty
        Dim AltNode As XmlNode = Nothing
        For f As Int16 = 0 To FtNodes.Count - 1
            Try
                If (Not FtNodes(f).InnerText.ToLower().Contains("ibid")) Then Continue For
                Dim PrevFtNode As XmlNode = FtNodes(f - 1)
                If (PrevFtNode Is Nothing) Then Continue For
                AltNode = PrevFtNode.SelectSingleNode(".//d:alt", NameSpaceManager)
                If (AltNode IsNot Nothing) Then
                    Author = AltNode.InnerText
                Else
                    If (PrevFtNode.InnerText.Contains(",")) Then
                        Author = PrevFtNode.InnerText.Substring(0, PrevFtNode.InnerText.IndexOf(","))
                    Else
                        Author = PrevFtNode.InnerText
                    End If
                End If

                Dim linkCnt As String = $"<link role=""xref"" linkend=""{PrevFtNode.ParentNode.Attributes("id").Value}""><alt>{Author}</alt>Ibid</link>"
                FtNodes(f).InnerXml = FtNodes(f).InnerXml.Replace("Ibid", linkCnt)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next
        File.WriteAllText(FinalXML.Replace(".xml", "_out.xml"), xmlBlsDoc.OuterXml.Replace("&amp;", "&"))
        Return True
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
    ' Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extra")



End Class


Public Class PageMapData
    Public Property SourceXPath As String = String.Empty
    Public Property DestinationXPath As String = String.Empty
    Public Property Choice As NodeMoveOption = NodeMoveOption.FIRSTCHILD

End Class
