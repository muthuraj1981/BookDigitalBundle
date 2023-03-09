Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Namespace InDesign.Basic

    Public Class SearchManager

        Public Function SearchByText(SearchText As String) As Object
            Return SearchByText(Nothing, SearchText)
        End Function

        Public Function ReplaceMiniTocContent(Target As Object, MiniTocEle As Object)
            If (Target Is Nothing) Then
                Return False
            End If
            If ((MiniTocEle.Paragraphs Is Nothing) OrElse (MiniTocEle.Paragraphs.count() = 0)) Then
                Return False
            End If
            If (MiniTocEle Is Nothing) Then
                Return False
            End If
            For p As Integer = 1 To MiniTocEle.Paragraphs.Count()
                Try
                    MiniTocEle.xmlElements.add(INDDGBL.InDesignDoc.XMLTags.Item("para"), MiniTocEle.Paragraphs(p))
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
            'Dim FindTextPre As Object = Nothing
            'Dim ChangeTextPre As Object = Nothing
            'Dim SearchResult As Object = Nothing
            'INDDGBL.ClearSearch()
            'FindTextPre = INDDGBL.InDesignApp.FindGrepPreferences
            'ChangeTextPre = INDDGBL.InDesignApp.ChangeGrepPreferences
            'FindTextPre.FindWhat = "(.*?)\r"
            'FindTextPre.AppliedCharacterStyle = InDesignConstant.INDESIGN_NOTHING
            'ChangeTextPre.AppliedCharacterStyle = InDesignConstant.INDESIGN_NOTHING
            'ChangeTextPre.markupTag = INDDGBL.InDesignDoc.XMLTags.Item("para")
            'SearchResult = Target.ChangeGrep()
            'Return SearchResult
            Return True
        End Function



        Public Function SearchByText(Target As Object, SearchText As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            FindTextPre.FindWhat = SearchText
            If (Target Is Nothing) Then
                SearchResult = INDDGBL.InDesignDoc.FindText()
            Else
                SearchResult = Target.FindText()
            End If

            Return SearchResult
        End Function

        Public Function SearchByObject(Target As Object, ObjectStyleName As String) As Object
            Dim FindObjPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindObjPre = INDDGBL.InDesignApp.findObjectPreferences
            Try
                FindObjPre.appliedObjectStyles = INDDGBL.InDesignDoc.ObjectStyles.item(ObjectStyleName)
            Catch ex As Exception
                Return Nothing
            End Try
            If (Target Is Nothing) Then
                SearchResult = INDDGBL.InDesignDoc.FindObject()
            Else
                SearchResult = Target.FindObject()
            End If

            Return SearchResult
        End Function

        Public Function RemoveKerning() As Boolean
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            Try
                FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
                ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
                FindTextPre.KerningMethod = "Metrics"
                ChangeTextPre.KerningValue = 0
                SearchResult = INDDGBL.InDesignDoc.ChangeText()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function SearchFontPosition(Position As FontPosition) As Object
            Dim FindTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            FindTextPre.Position = Position
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            FindTextPre.FindWhat = InDesignConstant.INDESIGN_NOTHING
            SearchResult = INDDGBL.InDesignDoc.FindText()
            Return SearchResult
        End Function

        Public Function SearchFontPosition(Position As FontPosition, SearchText As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.Position = Position
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            FindTextPre.FindWhat = SearchText
            ChangeTextPre.Position = FontPosition.NORMAL
            SearchResult = INDDGBL.InDesignDoc.ChangeText()
            Return SearchResult
        End Function

        Public Function SearchFontStleApplyCharacterStyle(LocalFontStyle As String, CharacterStyleName As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            Dim CharaStyle As Object = Nothing
            INDDGBL.ClearSearch()
            Try
                CharaStyle = INDDGBL.InDesignDoc.CharacterStyles.Add()
                CharaStyle.name = CharacterStyleName
            Catch ex As Exception
                CharaStyle = INDDGBL.InDesignDoc.CharacterStyles.Item(CharacterStyleName)
            End Try
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.FontStyle = LocalFontStyle
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            FindTextPre.FindWhat = InDesignConstant.INDESIGN_NOTHING
            ChangeTextPre.AppliedCharacterStyle = CharaStyle
            SearchResult = INDDGBL.InDesignDoc.ChangeText()
            Return SearchResult
        End Function

        Public Function RemoveEntity(Target As Object, CharaStyle As String, Entity As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            Dim CharaStyleObj As Object = Nothing
            Try
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add()
                CharaStyleObj.Name = CharaStyle
            Catch ex As Exception
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(CharaStyle)
            End Try
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.FindWhat = Entity
            FindTextPre.AppliedCharacterStyle = CharaStyleObj
            ChangeTextPre.AppliedCharacterStyle = INDDGBL.InDesignDoc.CharacterStyles.item("[None]")
            ChangeTextPre.ChangeTo = ""
            SearchResult = Target.ChangeText()
            Return SearchResult
        End Function

        Public Function RemoveEntity(Entity As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.FindWhat = Entity
            FindTextPre.AppliedCharacterStyle = InDesignConstant.INDESIGN_NOTHING
            ChangeTextPre.AppliedCharacterStyle = InDesignConstant.INDESIGN_NOTHING
            ChangeTextPre.ChangeTo = ""
            SearchResult = INDDGBL.InDesignDoc.ChangeText()
            Return SearchResult
        End Function

        Public Function RemoveCharacterStyle(CharaStyle As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            Dim CharaStyleObj As Object = Nothing
            Try
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add()
                CharaStyleObj.Name = CharaStyle
            Catch ex As Exception
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(CharaStyle)
            End Try
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.FindWhat = InDesignConstant.INDESIGN_NOTHING
            FindTextPre.AppliedCharacterStyle = CharaStyleObj
            ChangeTextPre.AppliedCharacterStyle = INDDGBL.InDesignDoc.CharacterStyles.item("[None]")
            SearchResult = INDDGBL.InDesignDoc.ChangeText()
            Return SearchResult
        End Function

        Public Function SearchByParagraphStyle(ParaStyle As String, SearchText As String) As Object
            Return Me.SearchByParagraphStyle(Nothing, ParaStyle, SearchText)
        End Function

        Public Function SearchByParagraphStyle(Target As Object, ParaStyle As String) As Object
            Return Me.SearchByParagraphStyle(Target, ParaStyle, String.Empty)
        End Function

        Public Function SearchByParagraphStyle(ParaStyle As String) As Object
            Return Me.SearchByParagraphStyle(Nothing, ParaStyle, String.Empty)
        End Function

        Public Function SearchByParagraphStyle(Target As Object, ParaStyle As String, SearchText As String) As Object
            Dim FindTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            Dim ParaStyleObj As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            Try
                ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Add()
                ParaStyleObj.Name = ParaStyle
            Catch ex As Exception
                ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Item(ParaStyle)
                'MessageBox.Show("Please make sure the paragraph style [ " + ParaStyle + " ] exists in the active document.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
            FindTextPre.AppliedParagraphStyle = ParaStyleObj
            If (Not String.IsNullOrEmpty(SearchText)) Then
                FindTextPre.FindWhat = SearchText
            Else
                FindTextPre.FindWhat = InDesignConstant.INDESIGN_NOTHING
            End If

            If (Target Is Nothing) Then
                SearchResult = INDDGBL.InDesignDoc.FindText
            Else
                SearchResult = Target.FindText
            End If
            Return SearchResult
        End Function

        Public Function SearchByRegex(ParaStyleName As String, Pattern As String) As Object
            Dim FindGrepPre As Object = Nothing
            Dim ParaStyleObj As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindGrepPre = INDDGBL.InDesignApp.FindGrepPreferences
            Try
                ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Add()
                ParaStyleObj.Name = ParaStyleName
            Catch ex As Exception
                ParaStyleObj = INDDGBL.InDesignDoc.ParagraphStyles.Item(ParaStyleName)
            End Try
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedLayersForFind = True
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedStoriesForFind = True
            FindGrepPre.FindWhat = Pattern
            FindGrepPre.AppliedParagraphStyle = ParaStyleObj
            SearchResult = INDDGBL.InDesignDoc.FindGrep()
            If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
                Return SearchResult
            End If
            Return Nothing
        End Function

        Public Function SearchByRegex(Target As Object, Pattern As String) As Object
            Dim FindGrepPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindGrepPre = INDDGBL.InDesignApp.FindGrepPreferences
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedLayersForFind = True
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedStoriesForFind = True
            FindGrepPre.FindWhat = Pattern
            SearchResult = Target.FindGrep()
            If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
                Return SearchResult
            End If
            Return Nothing
        End Function
        Public Function FindandReplace(ByVal Target As Object, ByVal Pattern As String, ByVal ReplaceText As String)
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.FindWhat = Pattern
            FindTextPre.Tracking = ReplaceText
            ChangeTextPre.Tracking = 0
            Try
                INDDGBL.InDesignDoc.ChangeText()
            Catch ex As Exception
            End Try
            Return False
        End Function

        Public Function FindAndReplaceEquationsElement(ByVal Target As Object) As Boolean
            Dim FindOption As Object = Nothing
            Dim FindTextPre As Object = Nothing
            Dim SearchEqun As Object = Nothing
            Dim NewElement As Object = Nothing
            Dim XmlTagName As String = String.Empty
            Dim XmlTagObj As Object = Nothing
            Dim ParentElement As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim EquName As String = String.Empty
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            FindOption = INDDGBL.InDesignApp.FindChangeTextOptions
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindOption.includeFootnotes = True
            FindTextPre.FindWhat = "^a"
            Try
                SearchEqun = Target.findText
            Catch ex As Exception
            End Try
            If ((SearchEqun IsNot Nothing) AndAlso (SearchEqun.Count > 0)) Then
                For s As Int16 = 1 To SearchEqun.Count
                    Dim Eqn As Object = SearchEqun(s)
                    If ((Eqn.allGraphics IsNot Nothing) AndAlso (Eqn.allGraphics.Count > 0)) Then
                        Dim content As String = Eqn.texts(1).paragraphs(1).contents
                        content = content.Replace(vbTab, "").Replace(vbCr, "").Replace(vbLf, "").Replace(vbNewLine, "").Replace(ChrW(65532), "")
                        content = System.Text.RegularExpressions.Regex.Replace(content, "[\(\)\[\]0-9\. ]*", "", Text.RegularExpressions.RegexOptions.Singleline Or Text.RegularExpressions.RegexOptions.IgnoreCase)
                        If (String.IsNullOrEmpty(content.Trim())) Then
                            XmlTagName = "equation"
                        Else
                            XmlTagName = "inlineequation"
                        End If
                        EquName = Eqn.allGraphics(1).ItemLink.name
                        ParentElement = INDDGBL.GetXmlElement(Eqn)
                        Try
                            XmlTagObj = INDDGBL.InDesignDoc.XmlTags.Add(XmlTagName)
                        Catch ex As Exception
                            XmlTagObj = INDDGBL.InDesignDoc.XmlTags.Item(XmlTagName)
                        End Try
                        NewElement = ParentElement.XmlElements.Add(XmlTagObj)
                        NewElement.XmlAttributes.Add("href", EquName)
                        If (String.Compare(XmlTagName, "inlineequation", True) = 0) Then
                            Eqn.texts(1).Markup(NewElement)
                        ElseIf (String.Compare(XmlTagName, "equation", True) = 0) Then
                            Eqn.texts(1).paragraphs(1).Markup(NewElement)
                        End If

                    End If
                Next
            End If
            INDDGBL.InDesignApp.FindChangeTextOptions = InDesignConstant.INDESIGN_NOTHING
            Return True
        End Function

        Public Function SearchByRegex(Target As Object, Position As FontPosition, Pattern As String) As Object
            Dim FindGrepPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindGrepPre = INDDGBL.InDesignApp.FindGrepPreferences
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedLayersForFind = True
            INDDGBL.InDesignApp.FindChangeGrepOptions.IncludeLockedStoriesForFind = True
            FindGrepPre.Position = Position
            FindGrepPre.FindWhat = Pattern
            SearchResult = Target.FindGrep()
            If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
                Return SearchResult
            End If
            Return Nothing
        End Function

        Public Function SearchByStyleandTagged(Target As Object, CharaStyle As String, XmlTagName As String) As Boolean
            Dim FindTextPre As Object = Nothing
            Dim SearchResult As Object = Nothing
            Dim ParaStyleObj As Object = Nothing
            Dim CharaStyleObj As Object = Nothing
            Dim XmlTagObj As Object = Nothing
            Dim NewElement As Object = Nothing
            Dim ParentElement As Object = Nothing
            Dim SearchObj As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            Try
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add()
                CharaStyleObj.Name = CharaStyle
            Catch ex As Exception
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(CharaStyle)
            End Try
            FindTextPre.AppliedCharacterStyle = CharaStyleObj
            Try
                XmlTagObj = INDDGBL.InDesignDoc.XmlTags.Add(XmlTagName)
            Catch ex As Exception
                XmlTagObj = INDDGBL.InDesignDoc.XmlTags.Item(XmlTagName)
            End Try
            Try
                SearchResult = Target.FindText
            Catch ex As Exception
            End Try
            If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
                For srh As Integer = 1 To SearchResult.Count
                    SearchObj = SearchResult(srh)
                    If (SearchObj IsNot Nothing) AndAlso (SearchObj.Texts IsNot Nothing) Then
                        ParentElement = INDDGBL.GetXmlElement(SearchObj.Texts(1))
                        NewElement = ParentElement.XmlElements.Add(XmlTagObj)
                        If (String.Compare(XmlTagName, "entity", True) = 0) Then
                            NewElement.XmlAttributes.Add("val", Convert.ToString(Convert.ToInt32(AscW(SearchObj.Texts(1).Contents)), 16))
                        Else
                            NewElement.XmlAttributes.Add("role", CharaStyle)
                        End If
                        SearchObj.Texts(1).Markup(NewElement)
                    End If
                Next
            End If
            Return True
        End Function

        Public Function RemoveTrackingValues(TrackingValue As List(Of Double)) As Boolean
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            INDDGBL.ClearSearch()
            If ((TrackingValue Is Nothing) OrElse (TrackingValue.Count() = 0)) Then
                Return False
            End If
            For trk As Integer = 0 To TrackingValue.Count - 1
                FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
                ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
                FindTextPre.FindWhat = InDesignConstant.INDESIGN_NOTHING
                FindTextPre.Tracking = TrackingValue(trk)
                ChangeTextPre.Tracking = 0
                Try
                    INDDGBL.InDesignDoc.ChangeText()
                Catch ex As Exception
                    Continue For
                End Try
            Next
            Return False
        End Function

    End Class

    Public Class MiniTocData
        Public Property ParaID As String = String.Empty
        Public Property ParaContent As String = String.Empty
        Public Property SeparaterData As New List(Of MiniTocSeparatorData)
    End Class

    Public Class MiniTocSeparatorDataCollection
        Implements ICollection(Of MiniTocSeparatorData)
        Dim lst As New List(Of MiniTocSeparatorData)
        Public ReadOnly Property Count As Integer Implements ICollection(Of MiniTocSeparatorData).Count
            Get
                Return lst.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of MiniTocSeparatorData).IsReadOnly
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Sub Add(item As MiniTocSeparatorData) Implements ICollection(Of MiniTocSeparatorData).Add
            Dim index As Integer = 0
            index = lst.FindIndex(Function(ff As MiniTocSeparatorData)
                                      If (ff.CharCode = item.CharCode) Then
                                          Return True
                                      End If
                                      Return False
                                  End Function)
            If (index = -1) Then
                lst.Add(item)
            Else
                lst(index).Count = lst(index).Count + 1
            End If
        End Sub

        Public Sub Clear() Implements ICollection(Of MiniTocSeparatorData).Clear
            lst.Clear()
        End Sub

        Public Sub CopyTo(array() As MiniTocSeparatorData, arrayIndex As Integer) Implements ICollection(Of MiniTocSeparatorData).CopyTo
            Throw New NotImplementedException()
        End Sub

        Public Function Contains(item As MiniTocSeparatorData) As Boolean Implements ICollection(Of MiniTocSeparatorData).Contains
            Throw New NotImplementedException()
        End Function

        Public Function GetEnumerator() As IEnumerator(Of MiniTocSeparatorData) Implements IEnumerable(Of MiniTocSeparatorData).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Public Function Remove(item As MiniTocSeparatorData) As Boolean Implements ICollection(Of MiniTocSeparatorData).Remove
            Return lst.Remove(item)
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class MiniTocSeparatorData
        Public Property Character As String = String.Empty
        Public Property CharCode As String = String.Empty
        Public Property Count As Integer = 0
    End Class

End Namespace


