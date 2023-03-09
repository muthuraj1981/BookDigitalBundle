Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Imports Server_DigitalBundle.InDesign.Basic

Public Enum FontPosition
    NONE = 0
    NORMAL = 1852797549
    OTSUBSCRIPT = 1884247138
    OTSUPERSCRIPT = 1884247155
    SUBSCRIPT = 1935831907
    SUPERSCRIPT = 1936749411
End Enum


Namespace InDesign.Advanced

    Public Class FootnoteManager

        Dim FootNoteList As New List(Of Object)

        Public Sub New()

        End Sub

        Private Function GetFootnotes() As Boolean
            Dim StyleMgr As New StyleToTagManager
            Dim FootnoteTag As Object = Nothing
            Dim SearchFootNotes As Object = Nothing
            Dim ParagraphElement As Object = Nothing
            Dim FootnoteItem As Object = Nothing
            Dim ConvertedText As Object = Nothing
            If ((INDDGBL.InDesignApp Is Nothing) OrElse (INDDGBL.InDesignDoc Is Nothing)) Then
                Return False
            End If
            Try
                FootnoteTag = INDDGBL.InDesignDoc.XmlTags.Add("footnote")
            Catch ex As Exception
                FootnoteTag = INDDGBL.InDesignDoc.XmlTags.Item("footnote")
            End Try
            SearchFootNotes = INDDGBL.InDesignApp.DoScript("main(); function main() { return app.activeDocument.stories.everyItem().footnotes.everyItem().getElements();}", InDesignConstant.SCRIPTLANG_JAVASCRIPT, New Object() {})
            If ((SearchFootNotes IsNot Nothing) AndAlso (SearchFootNotes.Length > 0)) Then
                For fnt As Integer = 0 To SearchFootNotes.Length - 1
                    FootnoteItem = SearchFootNotes(fnt)
                    ConvertedText = FootnoteItem.ConvertToText()
                    StyleMgr.CreateStyleToTag(ConvertedText)
                    If ((ConvertedText.AssociatedXMLElements IsNot Nothing) AndAlso (ConvertedText.AssociatedXMLElements.Count() > 0)) Then
                        'ConvertedText.AssociatedXMLElements(1).MarkupTag = FootnoteTag
                        ParagraphElement = ConvertedText.AssociatedXMLElements(1).XmlElements.Add(INDDGBL.InDesignDoc.XmlTags("footnote"))
                        ParagraphElement.XmlAttributes.Add("xml:id", "")
                        ConvertedText.Texts(1).Markup(ParagraphElement)
                    End If
                Next
            End If
            Return True
        End Function

        Private Sub TagFootnoteContents(FootnoteText As Object)
            Dim FootnoteElement As Object = Nothing
            Dim FootnoteTag As Object = Nothing
            Try
                FootnoteTag = INDDGBL.InDesignDoc.XmlTags.Add("footnote")
            Catch ex As Exception
                FootnoteTag = INDDGBL.InDesignDoc.XmlTags.Item("footnote")
            End Try
            If ((FootnoteText.AssociatedXMLElements IsNot Nothing) AndAlso (FootnoteText.AssociatedXMLElements.Count() > 0)) Then
                FootnoteElement = FootnoteText.AssociatedXMLElements(1).XmlElements.Add(FootnoteTag)
            End If
            If (FootnoteElement IsNot Nothing) Then
                FootnoteText.Texts(1).Markup(FootnoteElement)
            End If
        End Sub

        Public Function ConvertFootnoteToText() As Boolean
            If (Not GetFootnotes()) Then
                Return False
            End If
            If (Not FormatFootnoteID()) Then
                Return False
            End If
            Return True
        End Function

        Public Function CreateTwowayLinkToFootnote() As Boolean
            Dim SearchMgr As New SearchManager
            Dim CitationList As Object = Nothing, TempCitationList As New List(Of Object)
            Dim EndNoteList As Object = Nothing
            Dim EndNoteTextSource As Object = Nothing
            Dim SuperTextSource As Object = Nothing, EndNoteTextDest As Object = Nothing
            Dim SuperTextDest As Object = Nothing, TempEndNote As Object = Nothing
            Dim EndNoteLabel As String = Nothing, EndNoteLblObj As Object = Nothing
            Dim CitationLblObj As Object = Nothing, CitationLabel As String = Nothing
            Dim FormatFootnoteStyleObj As Object = Nothing
            Dim CharCount As Integer = 0
            Dim ParaInx As Integer = 0

            CitationList = SearchMgr.SearchByRegex(INDDGBL.InDesignDoc, FontPosition.SUPERSCRIPT, "\d+")
            EndNoteList = SearchMgr.SearchByParagraphStyle("NOTE2-1Dig")

            If ((CitationList Is Nothing) OrElse (CitationList.Count = 0)) Then
                Return False
            End If
            If ((EndNoteList Is Nothing) OrElse (EndNoteList.Count = 0)) Then
                If (MessageBox.Show("Endnote citation text found, but no end note contents found. Please make sure the end note content paragraph style is NOTE2-1Dig.", INDDGBL.AppTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) = DialogResult.Retry) Then
                    CreateTwowayLinkToFootnote()
                End If
                Return False
            End If
            Dim DialogOut As DialogResult
            Dim TepCitationList As Object = Nothing

            If (CitationList.Count() > EndNoteList(1).Paragraphs.Count) Then
                If (MessageBox.Show("Endnote citation count is greather then EndNote count. Please check the super script contents.", INDDGBL.AppTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) = DialogResult.OK) Then
                    For supindx As Integer = 1 To CitationList.Count()
                        If ((CitationList(supindx).Texts IsNot Nothing) AndAlso (CitationList(supindx).Texts.Count() > 0)) Then
                            CitationList(supindx).Texts(1).Select()
                            INDDGBL.InDesignApp.LayoutWindows(1).ZoomPercentage = 100
                            INDDGBL.InDesignApp.LayoutWindows(1).Zoom(InDesignConstant.ZOOMOPTION_FITSPREAD)
                            INDDGBL.InDesignApp.LayoutWindows(1).ZoomPercentage = 250
                            INDDGBL.InDesignApp.LayoutWindows(1).ActivePage = CitationList(supindx).Texts(1).ParentTextFrames(1).ParentPage
                            DialogOut = MessageBox.Show("Please confirm the selected text is Endnote citation label", INDDGBL.AppTitle, MessageBoxButtons.YesNoCancel)
                            If (DialogOut = DialogResult.Yes) Then
                                TempCitationList.Add(CitationList(supindx))
                            ElseIf DialogOut = DialogResult.Cancel Then
                                Return False
                            End If
                        End If
                    Next
                Else
                    Return False
                End If
            Else
                For supinx As Integer = 1 To CitationList.Count()
                    TempCitationList.Add(CitationList(supinx))
                Next
            End If
            ParaInx = 0
            'For PaInx As Integer = 1 To EndNoteList(1).Paragraphs.Count()
            For cinx As Integer = 0 To TempCitationList.Count() - 1
                ParaInx += 1
                CitationLblObj = SelectLabelObject(TempCitationList(cinx), (cinx + 1).ToString().Length - 1)
                CitationLabel = CitationLblObj.Texts(1).Contents
                CitationLabel = CitationLabel.Replace("\t", "").Replace(".", "").Replace(ChrW(9), "").Replace(ChrW(13), "").Replace(ChrW(20), "").Replace(ChrW(10), "")
                TempEndNote = EndNoteList(1).Paragraphs(ParaInx)
                EndNoteLabel = ExtractLabel(TempEndNote, CitationLabel)
                EndNoteLblObj = SelectLabelObject(TempEndNote, (cinx + 1).ToString().Length - 1)
                If (String.Compare(EndNoteLabel.Trim(), CitationLabel.Trim(), True) = 0) Then
                    CreateTwowayHyperLink(CitationLblObj, CitationLabel, EndNoteLblObj, EndNoteLabel)
                Else
                    If (Not IsNumeric(CitationLabel)) Then
                        ParaInx += 1
                    ElseIf (Not IsNumeric(EndNoteLabel)) Then
                        cinx -= 1
                    End If
                End If
                FormatFootnoteStyleObj = GetFootnoteStyle(ParaInx)
                If (FormatFootnoteStyleObj IsNot Nothing) Then
                    TempEndNote.ApplyParagraphStyle(FormatFootnoteStyleObj)
                End If
            Next
            Return True
        End Function

        Public Function GetFootnoteStyle(FootnoteInDex As Int32) As Object
            Dim StyleName As String = "NOTE2-#Dig"
            Dim FootNoteStyleObj As Object = Nothing
            Dim StyleIndex As String = String.Empty
            If (FootnoteInDex > 0) And (FootnoteInDex <= 9) Then
                StyleIndex = 1
            ElseIf (FootnoteInDex > 9) And (FootnoteInDex <= 99) Then
                StyleIndex = 2
            ElseIf (FootnoteInDex > 99) And (FootnoteInDex <= 999) Then
                StyleIndex = 3
            ElseIf (FootnoteInDex > 999) And (FootnoteInDex <= 9999) Then
                StyleIndex = 4
            End If
            Try
                FootNoteStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Add()
                FootNoteStyleObj.Name = StyleName.Replace("#", StyleIndex)
            Catch ex As Exception
                FootNoteStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Item(StyleName.Replace("#", StyleIndex))
            End Try
            Return FootNoteStyleObj
        End Function

        Public Function CreateTwowayHyperLink(Source As Object, SourceContent As String, Destination As Object, DestinationContent As String) As Boolean
            Dim SourceHyperTextObj As Object = Nothing
            Dim SourceHyperDestObj As Object = Nothing
            Dim DestHyperTextObj As Object = Nothing
            Dim DestHyperDestObj As Object = Nothing
            Try
                DestHyperDestObj = INDDGBL.InDesignDoc.HyperlinkTextDestinations.Add(Destination.Texts(1).InsertionPoints(1))
                DestHyperDestObj.Name = String.Format("ch{0}-en-{1}", INDDGBL.ChapterNo, DestinationContent)

                SourceHyperDestObj = INDDGBL.InDesignDoc.HyperlinkTextDestinations.Add(Source.Texts(1).InsertionPoints(1))
                SourceHyperDestObj.name = String.Format("ch{0}-fn-{1}", INDDGBL.ChapterNo, SourceContent)

                DestHyperTextObj = INDDGBL.InDesignDoc.HyperlinkTextSources.Add(Destination.Texts(1))
                DestHyperTextObj.Name = String.Format("ch{0}-en-{1}", INDDGBL.ChapterNo, DestinationContent)
                'DestHyperTextObj.AppliedCharacterStyle = INDDGBL.InDesignDoc.CharacterStyles.item("[None]")

                SourceHyperTextObj = INDDGBL.InDesignDoc.HyperlinkTextSources.Add(Source.Texts(1))
                SourceHyperTextObj.Name = String.Format("ch{0}-fn-{1}", INDDGBL.ChapterNo, SourceContent)
                'SourceHyperTextObj.AppliedCharacterStyle = INDDGBL.InDesignDoc.CharacterStyles.item("[None]")

                INDDGBL.InDesignDoc.HyperLinks.Add(DestHyperTextObj, SourceHyperDestObj)
                INDDGBL.InDesignDoc.HyperLinks.Add(SourceHyperTextObj, DestHyperDestObj)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function ExtractLabel(ParagraphObj As Object, CitationLabel As String) As String
            Dim SearchMgr As New SearchManager
            Dim TmpLabel As String = String.Empty
            Dim FootnoteLabel As String = String.Empty
            Dim SearchResult As Object = Nothing
            SearchResult = SearchMgr.SearchByRegex(ParagraphObj.Texts(1), "\d+\.\t")
            If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
                FootnoteLabel = SearchResult(1).Texts(1).Contents()
                If (String.IsNullOrEmpty(FootnoteLabel)) Then
                    Return String.Empty
                End If
                FootnoteLabel = FootnoteLabel.Replace("\t", "").Replace(".", "").Replace(ChrW(9), "").Replace(ChrW(13), "")
            Else
                TmpLabel = ParagraphObj.Texts(1).Contents.ToString()
                If (TmpLabel.StartsWith(CitationLabel)) Then
                    Return CitationLabel
                End If
            End If
            Return FootnoteLabel
        End Function

        Public Function SelectLabelObject(ParagraphObj As Object, NoofCount As Integer) As Object
            Dim returnObj As Object = Nothing
            returnObj = INDDGBL.InDesignApp.DoScript("var paraObj = arguments[0]; var charcount = arguments[1]; main(); function main(){ var chars =  paraObj.characters.itemByRange(0,charcount); return chars; }", InDesignConstant.SCRIPTLANG_JAVASCRIPT, New Object() {ParagraphObj, NoofCount})
            Return returnObj
        End Function

        Public Function CleanEndnoteStyle(ParaStyleName As String) As Boolean
            Dim SearchMgr As New SearchManager
            Dim EndNoteStartPara As Object = Nothing
            Dim EndNoteFrame As Object = Nothing
            Dim ParaStyleList() As Object = Nothing
            Dim NoteTextObj As Object = Nothing
            Dim LastEndNoteFrame As Object = Nothing
            Dim ParaStyleObj As Object = Nothing
            Dim EndNoteCitationList As Object = Nothing
            EndNoteCitationList = SearchMgr.SearchFontPosition(FontPosition.SUPERSCRIPT)
            If ((EndNoteCitationList Is Nothing) OrElse (EndNoteCitationList.Count() = 0)) Then
                Return False
            End If
            NoteTextObj = SearchMgr.SearchByParagraphStyle("H1", "Notes")
            If ((NoteTextObj Is Nothing) OrElse (NoteTextObj.Count() = 0)) Then
                If (MessageBox.Show("Could not able to find the [NOTES] Text. Please make sure the text is styled as 'H1'.", INDDGBL.AppTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) = DialogResult.Retry) Then
                    NoteTextObj = SearchMgr.SearchByParagraphStyle("H1", "Note")
                Else
                    Return False
                End If
            End If
            If ((NoteTextObj Is Nothing) AndAlso (NoteTextObj.Count() = 0)) Then
                Return False
            End If
            EndNoteFrame = NoteTextObj(1).Paragraphs(1).ParentTextFrames(1)
            If (EndNoteFrame Is Nothing) Then
                Return False
            End If
            If ((EndNoteFrame.Paragraphs IsNot Nothing) AndAlso (EndNoteFrame.Paragraphs.Count() > 0)) Then
                EndNoteStartPara = EndNoteFrame.Paragraphs.NextItem(NoteTextObj(1).Paragraphs(1))
            End If
            If ((EndNoteStartPara.Texts Is Nothing) AndAlso (EndNoteStartPara.Texts.Count() = 0)) Then
                Return False
            End If
            If (Not Regex.IsMatch(EndNoteStartPara.Texts(1).Contents().ToString(), "\d+\.\t")) Then
                Return False
            End If
            INDDGBL.InDesignDoc.Select(InDesignConstant.INDESIGN_NOTHING)
            EndNoteStartPara.InsertionPoints(1).Select()
            If (EndNoteFrame.EndTextFrame IsNot Nothing) Then
                If (EndNoteFrame.ID = EndNoteFrame.EndTextFrame.ID) Then
                    INDDGBL.InDesignDoc.Select(EndNoteFrame.Texts(1).InsertionPoints(-1), InDesignConstant.SELECTIONOPTION_ADDTO)
                ElseIf (EndNoteFrame.ID <> EndNoteFrame.EndTextFrame.ID) Then
                    LastEndNoteFrame = FindLastFrame(EndNoteFrame)
                    INDDGBL.InDesignDoc.Select(LastEndNoteFrame.Texts(1).InsertionPoints(-1), InDesignConstant.SELECTIONOPTION_ADDTO)
                End If
            End If
            If ((INDDGBL.InDesignDoc.Selection IsNot Nothing) AndAlso (INDDGBL.InDesignDoc.Selection.Count() > 0)) Then
                Dim H1StyleList As Object = Nothing
                ParaStyleList = INDDGBL.InDesignApp.DoScript("main(); function main(){var styleList = new Array(); var para = app.selection[0].texts[0].paragraphs.everyItem().appliedParagraphStyle;for each(st in para) { styleList.push(st.name); } return styleList;}", InDesignConstant.SCRIPTLANG_JAVASCRIPT, New Object() {})
                If ((ParaStyleList IsNot Nothing) AndAlso (ParaStyleList.Count() > 0)) Then
                    H1StyleList = ParaStyleList.ToList().FindAll(Function(style As String)
                                                                     If (style.ToLower() = "h1") Then
                                                                         Return True
                                                                     End If
                                                                     Return False
                                                                 End Function)
                    If ((H1StyleList IsNot Nothing) AndAlso (H1StyleList.Count() > 0)) Then
                        If (Not MessageBox.Show("H1 title presented with inthe selected Endnote section. Please re-select the EndNote section [1 to...n]and retry.", INDDGBL.AppTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) = DialogResult.Retry) Then
                            Return False
                        End If
                    End If
                End If
                Try
                    ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Add()
                    ParaStyleObj.Name = ParaStyleName
                Catch ex As Exception
                    ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Item(ParaStyleName)
                End Try
                INDDGBL.InDesignDoc.Selection(1).ApplyParagraphStyle(ParaStyleObj)
                Return True
            End If
            Return False
        End Function

        Public Function FindLastFrame(EndNoteFrame As Object) As Object
            Dim EndnotePage As Object = Nothing
            Dim LastFrame As Object = Nothing
            Dim CharaCount As Int32 = 0
            LastFrame = EndNoteFrame.EndTextFrame
            If ((LastFrame.Texts IsNot Nothing) AndAlso (LastFrame.Texts.Count() > 0)) Then
                CharaCount = LastFrame.Texts(1).Contents.ToString().Length
            End If
            While (CharaCount < 2)
                EndnotePage = LastFrame.ParentPage
                EndnotePage = INDDGBL.InDesignDoc.Pages.PreviousItem(EndnotePage)
                LastFrame = EndnotePage.TextFrames.FirstItem
                If ((LastFrame.Texts IsNot Nothing) AndAlso (LastFrame.Texts.Count() > 0)) Then
                    CharaCount = LastFrame.Texts(1).Contents.ToString().Length
                End If
            End While
            Return LastFrame
        End Function

        Public Function ConvertElementToInDesignFootnote() As Boolean
            Dim FootNoteStyle As Object = Nothing
            Dim XmlMgr As New XMLManager
            Dim FootNoteElements As List(Of Object)

            Try
                FootNoteStyle = INDDGBL.InDesignDoc.ParagraphStyles.Item("FN")
            Catch ex As Exception

            End Try
            FootNoteElements = XmlMgr.GetXMLElementByXPath("//footnote[@role='end-ch-note']")
            If ((FootNoteElements Is Nothing) OrElse (FootNoteElements.Count = 0)) Then
                Return False
            End If
            Dim InsertionElement As Object = Nothing
            For ft As Integer = 0 To FootNoteElements.Count - 1
                Dim FtElement As Object = FootNoteElements(ft)
                FtElement.XmlContent.Select()
                INDDGBL.InDesignApp.Copy()
                Dim FtTmpElement As Object = GetChildElements(FtElement, "para")
                If (FtTmpElement Is Nothing) Then Continue For
                'FtElement.InsertionPoints(1).select()
                'If (FtElement.Parent.XmlElements.FirstItem().ID = FtElement.ID) Then
                '    InsertionElement = FtElement.Parent.XmlElements.FirstItem()
                '    InsertionElement.InsertionPoints(-1).select()
                'Else
                '    InsertionElement = FtElement.Parent.XmlElements.PreviousItem(FtElement)
                '    InsertionElement.InsertionPoints(1).select()
                'End If

                'Dim IndFootNote As Object = INDDGBL.InDesignDoc.Parent.selection(1).InsertionPoints(1).footnotes.add()
                Dim IndFootNote As Object = FtElement.InsertionPoints(1).footnotes.add()
                IndFootNote.InsertionPoints(-1).Select()
                INDDGBL.InDesignApp.Paste()
                If ((FtTmpElement.Parent IsNot Nothing) AndAlso (String.Compare(TypeName(FtTmpElement.Parent), "xmlelement", True) = 0)) Then
                    If (String.Compare(FtTmpElement.Parent.MarkupTag.Name, "footnote", True) = 0) Then
                        FtTmpElement.Parent.Delete()
                    End If
                End If
                If (FootNoteStyle IsNot Nothing) Then
                    IndFootNote.InsertionPoints(1).ApplyParagraphStyle(FootNoteStyle)
                End If
            Next
            Return True
        End Function

        Public Function GetChildElements(Element As Object, ChildName As String) As Object
            If (String.Compare(Element.MarkupTag.Name.ToString(), ChildName, True) = 0) Then
                Return Element
            End If
            If ((Element IsNot Nothing) AndAlso (Element.XmlElements IsNot Nothing)) Then
                If (Element.XmlElements.Count() > 0) Then
                    For Each chd As Object In Element.XmlElements
                        Dim ChildElement As Object = GetChildElements(chd, ChildName)
                        If (ChildElement IsNot Nothing) Then
                            Return ChildElement
                        End If
                    Next
                End If
            End If
            Return Nothing
        End Function

        Public Function FormatFootnoteID() As Boolean
            Dim XmlMgr As New XMLManager
            Dim FootNoteElements As New List(Of Object)
            Dim IDAttribute As Object = Nothing
            Dim ParaElement As Object = Nothing
            Dim ChapterNum As String = String.Empty
            Dim NewIDValue As String = String.Empty
            Dim FootNote As Object = Nothing
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return False
            End If
            FootNoteElements = XmlMgr.GetXMLElementByName("footnote")
            If ((FootNoteElements Is Nothing) OrElse (FootNoteElements.Count = 0)) Then
                Return False
            End If

            ChapterNum = INDDGBL.ChapterNo.ToLower().ToLower()

            For fnt As Integer = 0 To FootNoteElements.Count - 1
                FootNote = FootNoteElements(fnt)
                NewIDValue = String.Format("{0}-fn-{1}", IIf(ChapterNum.Contains("chapter"), ChapterNum.Replace("chapter ", "ch"), "ch" & ChapterNum), fnt + 1)
                Try
                    IDAttribute = FootNote.XmlAttributes.Add("xml:id", "")
                Catch ex As Exception
                    IDAttribute = FootNote.XmlAttributes.Item("xml:id")
                End Try
                IDAttribute.Value = NewIDValue
                If ((FootNote.XmlElements IsNot Nothing) AndAlso (FootNote.XmlElements.Count() > 0)) Then
                    For pa As Integer = 1 To FootNote.XmlElements.Count
                        ParaElement = FootNote.XmlElements(pa)
                        If (String.Compare(ParaElement.MarkupTag.Name, "para", True) = 0) Then
                            NewIDValue = String.Format("{0}-fn-pa-{1}", IIf(ChapterNum.ToLower().Contains("chapter"), ChapterNum.ToLower().Replace("chapter ", "ch"), "ch" & ChapterNum.ToLower()), fnt + 1)
                            Try
                                IDAttribute = ParaElement.XmlAttributes.Add("xml:id")
                            Catch ex As Exception
                                IDAttribute = ParaElement.XmlAttributes.Item("xml:id")
                            End Try
                            IDAttribute.value = NewIDValue
                        End If
                    Next
                End If
            Next
            Return True
        End Function

    End Class

End Namespace