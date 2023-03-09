Imports Server_DigitalBundle.InDesign
Imports System.Text.RegularExpressions
Imports Server_DigitalBundle.InDesign.Advanced

Public Class INDDGBL

    Public Shared ReadOnly Property AppTitle As String
        Get
            Dim asm As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Return String.Format("::: {0}-{1}.{2}.i1 :::", asm.GetName().Name, asm.GetName().Version.Major, asm.GetName().Version.Minor)
        End Get
    End Property

    Public Shared Property InDesignApp As Object

    Public Shared Property InDesignDoc As Object

    Public Shared ReadOnly Property DocbookDTD As String
        Get
            Return "\\FSDEANTA\TechRelease\Accounts\Common\DeantaComposer\Testing\Publish\extra\DTD\Revert_docbook\docbook.dtd"
        End Get
    End Property

    Public Shared Property DocumentWidth As Double
        Get
            Return InDesignDoc.DocumentPreferences.PageWidth
        End Get
        Set(value As Double)
            InDesignDoc.DocumentPreferences.PageWidth = value
        End Set
    End Property

    Public Shared Property DocumentHeight As Double
        Get
            Return InDesignDoc.DocumentPreferences.PageHeight
        End Get
        Set(value As Double)
            InDesignDoc.DocumentPreferences.PageHeight = value
        End Set
    End Property

    Public Shared ReadOnly Property DocumentName As String
        Get
            Return InDesignDoc.Name
        End Get
    End Property

    Public Shared ReadOnly Property DocumentPath As String
        Get
            Return InDesignDoc.FilePath
        End Get
    End Property

    Public Shared ReadOnly Property DocumentSelection As Object
        Get
            If ((InDesignDoc.Selection IsNot Nothing) AndAlso (InDesignDoc.Selection.Count() > 0)) Then
                Return InDesignDoc.Selection(1)
            End If
            Return Nothing
        End Get
    End Property

    Public Shared Function GetXmlTag(XmlTagName As String) As Object
        Dim XmlTagObj As Object = Nothing
        Try
            XmlTagObj = INDDGBL.InDesignDoc.XMLTags.Add(XmlTagName)
        Catch ex As Exception
            XmlTagObj = INDDGBL.InDesignDoc.XMLTags.Item(XmlTagName)
        End Try
        Return XmlTagObj
    End Function

    Public Shared Function GetChildElements(Element As Object, ChildName As String) As Object
        If ((Element IsNot Nothing) AndAlso (Element.XmlElements IsNot Nothing)) Then
            If (Element.XmlElements.Count() > 0) Then
                For Each chd As Object In Element.XmlElements
                    If (String.Compare(chd.MarkupTag.Name.ToString(), ChildName, True) = 0) Then
                        Return chd
                    End If
                Next
            End If
        End If
        Return Nothing
    End Function

    Public Shared Function GetXmlElement(Target As Object) As Object
        If ((Target IsNot Nothing) AndAlso (Target.Texts IsNot Nothing)) Then
            If (Target.Texts.Count() > 0) Then
                If ((Target.Texts(1).AssociatedXMLElements IsNot Nothing) AndAlso (Target.Texts(1).AssociatedXMLElements.Count() > 0)) Then
                    Return Target.Texts(1).AssociatedXMLElements(1)
                End If
            End If
        End If
        Return Nothing
    End Function

    Public Shared Function GetCharacterStyle(CharaStyle As String) As Object
        Dim CharaStyleObj As Object = Nothing
        Try
            CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add(CharaStyle)
        Catch ex As Exception
            CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(CharaStyle)
        End Try
        Return CharaStyleObj
    End Function

    Public Shared Sub ClearSearch()
        If (InDesignApp IsNot Nothing) Then
            InDesignApp.FindChangeGlyphOptions = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindChangeGrepOptions = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindChangeObjectOptions = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindChangeTextOptions = InDesignConstant.INDESIGN_NOTHING

            InDesignApp.FindGlyphPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindGrepPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindObjectPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.FindTextPreferences = InDesignConstant.INDESIGN_NOTHING

            InDesignApp.ChangeGlyphPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.ChangeGrepPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.ChangeObjectPreferences = InDesignConstant.INDESIGN_NOTHING
            InDesignApp.ChangeTextPreferences = InDesignConstant.INDESIGN_NOTHING

        End If
    End Sub

    Public Shared ReadOnly Property ChapterNo() As String
        Get
            Dim ChapNo As String = String.Empty
            Dim ChapterNoList As Object = Nothing
            Dim XmlMgr As New XMLManager
            Try
                ChapterNoList = XmlMgr.GetXMLElementByName("chapter")
                If ((ChapterNoList IsNot Nothing) AndAlso (ChapterNoList.Count() > 0)) Then
                    ChapNo = ChapterNoList(0).XmlAttributes.Item("label").Value
                End If

                If (String.IsNullOrEmpty(ChapNo)) Then
                    ChapNo = Regex.Match(INDDGBL.DocumentName, "chapter \d*", RegexOptions.IgnoreCase).Value
                End If
                If (String.IsNullOrEmpty(ChapNo)) Then
                    ChapNo = Regex.Match(INDDGBL.DocumentName, "^\d*[^ |_]", RegexOptions.IgnoreCase).Value
                End If
                Return ChapNo
            Catch ex As Exception
                Return String.Empty
            End Try
        End Get
    End Property

    Public Shared Function RemoveOversetFrame(TextFrame As Object) As Boolean
        Dim Bounds As Object()
        Dim PgWidth As Double = INDDGBL.DocumentWidth
        If (TextFrame Is Nothing) Then Return False
        While (TextFrame.Overflows)
            Bounds = TextFrame.GeometricBounds
            Bounds(2) = Bounds(2) + 5
            TextFrame.GeometricBounds = Bounds
            If ((PgWidth > 0) AndAlso (PgWidth <= Math.Round(Bounds(2), 2))) Then
                Exit While
            End If
        End While
        Return True
    End Function
End Class

