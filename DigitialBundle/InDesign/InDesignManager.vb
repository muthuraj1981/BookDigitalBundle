Imports Server_DigitalBundle.InDesign
Imports System.Windows.Forms
Imports System.IO
Imports Server_DigitalBundle.InDesign.Advanced

Public Enum InDesignVersion
    NONE = 0
    CS2 = 1
    CS3 = 2
    CS4 = 3
    CS5 = 4
    CS5DOT5 = 5
    CS6 = 6
    CC = 7
    CC2014 = 2014
    CC2017 = 2017
    CC2018 = 2018
    CC2019 = 2019
End Enum


Namespace InDesign.Basic

    Public Class InDesignManager

        Public Function CreateInDesignApp(InddVersion As InDesignVersion) As Boolean
            If (InddVersion = InDesignVersion.NONE) Then
                MessageBox.Show("Invalid InDesign application version. Please check.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            Try
                INDDGBL.InDesignApp = CreateObject("InDesign.Application." & InddVersion.ToString().Replace("DOT", ".").Replace("2014", ".2014").Replace("2017", ".2017").Replace("2018", ".2018").Replace("2019", ".2019"))
                INDDGBL.InDesignApp.ScriptPreferences.UserInteractionLevel = InDesignConstant.USERINTERACTION_NEVERINTERACT
            Catch ex As Exception
                If (Not CreateInDesignApp(InDesignVersion.CC2017)) Then
                    If (Not CreateInDesignApp(InDesignVersion.CC2014)) Then
                        If (Not CreateInDesignApp(InDesignVersion.CS5DOT5)) Then
                            MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Return False
                        End If
                    End If
                End If
            End Try
            Return True
        End Function

        Public Sub SetActiveDocument()
            If (INDDGBL.InDesignApp IsNot Nothing) Then
                If ((INDDGBL.InDesignApp.Documents IsNot Nothing) AndAlso (INDDGBL.InDesignApp.Documents.Count() > 0)) Then
                    INDDGBL.InDesignDoc = INDDGBL.InDesignApp.Documents(1)
                End If
            End If
        End Sub

        Public Function LoadExtraDocument(TemplateFile As String) As Object
            Dim Document As Object = Nothing
            If (String.IsNullOrEmpty(TemplateFile)) Then
                Throw New ArgumentException("Template file name should not be empty. Please check the template file path.", TemplateFile)
            End If
            If (Not File.Exists(TemplateFile)) Then
                Throw New FileNotFoundException("Could not able to find the template file in the specified path.", TemplateFile)
            End If
            If (Not IsValidExtension(TemplateFile)) Then
                MessageBox.Show("Invalid InDesign file name.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return Nothing
            End If
            Try
                INDDGBL.InDesignApp.ScriptPreferences.UserInteractionLevel = InDesignConstant.USERINTERACTION_NEVERINTERACT
                Document = INDDGBL.InDesignApp.Open(TemplateFile)
                INDDGBL.InDesignApp.ScriptPreferences.UserInteractionLevel = InDesignConstant.USERINTERACTION_NEVERINTERACT
                Document.ViewPreferences.RulerOrigin = InDesignConstant.PAGEOPTION_PAGEORIGIN
                If (Document IsNot Nothing) Then
                    Return Document
                End If
                Return Nothing
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Function LoadDocument(TemplateFile As String) As Boolean
            If (String.IsNullOrEmpty(TemplateFile)) Then
                'Throw New ArgumentException("Template file name should not be empty. Please check the template file path.", TemplateFile)
            End If
            If (Not File.Exists(TemplateFile)) Then
                Throw New FileNotFoundException("Could not able to find the template file in the specified path.", TemplateFile)
            End If
            If (Not IsValidExtension(TemplateFile)) Then
                MessageBox.Show("Invalid InDesign file name.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            Try
                INDDGBL.InDesignDoc = INDDGBL.InDesignApp.Open(TemplateFile)
                INDDGBL.InDesignDoc.ViewPreferences.RulerOrigin = InDesignConstant.PAGEOPTION_PAGEORIGIN
                If (INDDGBL.InDesignDoc Is Nothing) Then
                    Return False
                End If
                Return True
            Catch ex As Exception
                GBL.DeantaBallon("Please select the proper InDesign application version.", MessageType.MSGERROR)
                Return False
            End Try
        End Function

        Public Function ExportXml(ExportFile As String) As Boolean
            If (String.IsNullOrEmpty(ExportFile)) Then
                MessageBox.Show("Export file name should not be empty.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            If (File.Exists(ExportFile)) Then
                File.Delete(ExportFile)
            End If
            If (INDDGBL.InDesignDoc IsNot Nothing) Then
                INDDGBL.InDesignDoc.Export(InDesignConstant.EXPORTFORMAT_XML, ExportFile)
            End If
            Return True
        End Function

        Public Sub SaveDocument()
            SaveDocument(String.Empty)
        End Sub

        Public Sub SaveDocument(SavePath As String)
            If (INDDGBL.InDesignDoc IsNot Nothing) Then
                If (String.IsNullOrEmpty(SavePath)) Then
                    INDDGBL.InDesignDoc.Save()
                Else
                    INDDGBL.InDesignDoc.Save(SavePath)
                End If
            End If
        End Sub

        Public Sub CloseDocument(SaveOption As Boolean)
            If (INDDGBL.InDesignDoc IsNot Nothing) Then
                INDDGBL.InDesignDoc.Close(InDesignConstant.SAVEOPTION_YES)
            End If
        End Sub

        Public Sub CloseDocument()
            If (INDDGBL.InDesignDoc IsNot Nothing) Then
                INDDGBL.InDesignDoc.Close(InDesignConstant.SAVEOPTION_NO)
            End If
        End Sub

        Public Function IsValidExtension(TemplateFile As String) As Boolean
            If ((String.Compare(Path.GetExtension(TemplateFile), ".indd", True) = 0) Or (String.Compare(Path.GetExtension(TemplateFile), ".indt", True) = 0)) Then
                Return True
            End If
            Return False
        End Function

        Public Function DoScript(ScriptFile As String) As Boolean
            If (String.IsNullOrEmpty(ScriptFile)) Then
                MessageBox.Show("Script file name should not be empty.", INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            If (Not File.Exists(ScriptFile)) Then
                MessageBox.Show("Could not able to find the script file." & ScriptFile, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            Try
                If (INDDGBL.InDesignApp IsNot Nothing) Then
                    INDDGBL.InDesignApp.DoScript(ScriptFile, InDesignConstant.SCRIPTLANG_JAVASCRIPT)
                End If
                Return True
            Catch ex As Exception
                MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End Function

        Public Function ImportExcelFile(ExcelFile As String, TableStyleName As String) As Boolean
            Return ImportExcelFile(ExcelFile, TableStyleName, "")
        End Function

        Public Function ImportExcelFile(ExcelFile As String, TableStyleName As String, SheetRange As String) As Boolean
            Dim PlaceTextFrame As Object = Nothing
            Dim TableStyleObj As Object = Nothing
            If (INDDGBL.InDesignApp Is Nothing) Then
                Return False
            End If
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return False
            End If
            Try
                TableStyleObj = INDDGBL.InDesignDoc.TableStyles.Add(TableStyleName)
            Catch ex As Exception
                TableStyleObj = INDDGBL.InDesignDoc.TableStyles.Item(TableStyleName)
            End Try
            PlaceTextFrame = GetEmptyFrameInDoc(INDDGBL.InDesignDoc.Pages(-1))
            If (PlaceTextFrame Is Nothing) Then
                Return False
            End If
            Try
                INDDGBL.InDesignApp.ExcelImportPreferences.SheetName = "FINAL"
                If (Not String.IsNullOrEmpty(SheetRange)) Then
                    INDDGBL.InDesignApp.ExcelImportPreferences.RangeName = SheetRange
                End If
                INDDGBL.InDesignApp.ExcelImportPreferences.TableFormatting = InDesignConstant.TABLEFORMATTING_EXCELUNFORMATTEDTABLE
                INDDGBL.InDesignApp.ExcelImportPreferences.ShowHiddenCells = False
                INDDGBL.InDesignApp.ExcelImportPreferences.UseTypographersQuotes = True
                PlaceTextFrame.Place(ExcelFile)
                If ((PlaceTextFrame.Tables IsNot Nothing) AndAlso (PlaceTextFrame.Tables.Count() > 0)) Then
                    PlaceTextFrame.Tables(1).AppliedTableStyle = TableStyleObj
                End If
                Return True
            Catch ex As Exception
                MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End Function

        Public Function GetTextFrameBasedOnScriptLabel(Label As String, Optional IsIncludePage As Boolean = False) As List(Of Object)
            Dim TextFrames As New List(Of Object)
            'For Each Mst As Object In INDDGBL.InDesignDoc.MasterSpreads
            For ms As Integer = 1 To INDDGBL.InDesignDoc.MasterSpreads.Count
                Dim Mst As Object = INDDGBL.InDesignDoc.MasterSpreads(ms)
                For txt As Integer = 1 To Mst.TextFrames.Count
                    If (String.Compare(Mst.TextFrames(txt).Label, Label, True) = 0) Then
                        TextFrames.Add(Mst.TextFrames(txt))
                    End If
                Next
            Next
            If (IsIncludePage) Then
                'For Each Mst As Object In INDDGBL.InDesignDoc.AllPageItems
                For al As Integer = 1 To INDDGBL.InDesignDoc.AllPageItems.Count
                    Dim Mst As Object = INDDGBL.InDesignDoc.AllPageItems(al)
                    If (String.Compare(TypeName(Mst), "textframe", True) = 0) Then
                        If (String.Compare(Mst.Label, Label, True) = 0) Then
                            TextFrames.Add(Mst)
                        End If
                    End If
                Next
            End If
            Return TextFrames
        End Function

        Public Function GetEmptyFrameInDoc(LastPageObj As Object) As Object
            Dim PlaceTextFrame As Object = Nothing
            INDDGBL.InDesignDoc.ViewPreferences.RulerOrigin = InDesignConstant.PAGEOPTION_PAGEORIGIN
            Dim NewPageObj As Object = Nothing
            If ((LastPageObj.TextFrames IsNot Nothing) AndAlso (LastPageObj.TextFrames.Count() > 0)) Then
                If ((((LastPageObj.TextFrames(1).Tables IsNot Nothing) AndAlso (LastPageObj.TextFrames(1).Tables.Count() > 0))) Or (Not String.IsNullOrEmpty(LastPageObj.TextFrames(1).Contents.ToString()))) Then
                    NewPageObj = INDDGBL.InDesignDoc.Pages.Add(InDesignConstant.LOCATIONOPTION_AFTER, LastPageObj)
                    NewPageObj.MarginPreferences.Top = LastPageObj.MarginPreferences.Top
                    NewPageObj.MarginPreferences.Left = LastPageObj.MarginPreferences.Left
                    NewPageObj.MarginPreferences.Right = LastPageObj.MarginPreferences.Right
                    NewPageObj.MarginPreferences.Bottom = LastPageObj.MarginPreferences.Bottom
                    PlaceTextFrame = NewPageObj.TextFrames.Add()
                    If (NewPageObj.Side = InDesignConstant.PAGESIDE_RIGHTHAND) Then
                        PlaceTextFrame.GeometricBounds = New Double() {NewPageObj.MarginPreferences.Top, NewPageObj.MarginPreferences.Left, (INDDGBL.DocumentHeight - NewPageObj.MarginPreferences.Bottom), (INDDGBL.DocumentWidth - NewPageObj.MarginPreferences.Right)}
                    ElseIf (NewPageObj.Side = InDesignConstant.PAGESIDE_LEFTHAND) Then
                        PlaceTextFrame.GeometricBounds = New Double() {NewPageObj.MarginPreferences.Top, NewPageObj.MarginPreferences.Right, (INDDGBL.DocumentHeight - NewPageObj.MarginPreferences.Bottom), (INDDGBL.DocumentWidth - NewPageObj.MarginPreferences.Left)}
                    End If
                Else
                    PlaceTextFrame = LastPageObj.TextFrames(1)
                End If
            Else
                PlaceTextFrame = LastPageObj.TextFrames.Add()
                If (LastPageObj.Side = InDesignConstant.PAGESIDE_RIGHTHAND) Then
                    PlaceTextFrame.GeometricBounds = New Double() {LastPageObj.MarginPreferences.Top, LastPageObj.MarginPreferences.Left, (INDDGBL.DocumentHeight - LastPageObj.MarginPreferences.Bottom), (INDDGBL.DocumentWidth - LastPageObj.MarginPreferences.Right)}
                ElseIf (LastPageObj.Side = InDesignConstant.PAGESIDE_LEFTHAND) Then
                    PlaceTextFrame.GeometricBounds = New Double() {LastPageObj.MarginPreferences.Top, LastPageObj.MarginPreferences.Right, (INDDGBL.DocumentHeight - LastPageObj.MarginPreferences.Bottom), (INDDGBL.DocumentWidth - LastPageObj.MarginPreferences.Left)}
                End If
            End If
            Return PlaceTextFrame
        End Function

        Public Function ImportXmlFile(InputXmlFile As String) As Boolean
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return False
            End If
            If (Not File.Exists(InputXmlFile)) Then
                MessageBox.Show("Could not able to find the input xml File." & InputXmlFile, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
            Try
                INDDGBL.InDesignDoc.ImportXml(InputXmlFile)
                Return True
            Catch ex As Exception
                MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End Function

        Public Function IsFootnoteExists() As Boolean
            Dim XmlInstrcut As Object = Nothing
            Dim BookElement As Object = Nothing
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return False
            End If
            If (INDDGBL.InDesignDoc.XmlElements IsNot Nothing) AndAlso (INDDGBL.InDesignDoc.XmlElements.Count()) Then
                If (String.Compare(INDDGBL.InDesignDoc.XmlElements(1).MarkupTag.Name, "book", True) = 0) Then
                    BookElement = INDDGBL.InDesignDoc.XmlElements(1)
                End If
            End If
            If (BookElement IsNot Nothing) Then
                If ((BookElement.XMLInstructions IsNot Nothing) AndAlso (BookElement.XMLInstructions.Count() > 0)) Then
                    For inst As Integer = 1 To BookElement.XMLInstructions.Count()
                        XmlInstrcut = BookElement.XMLInstructions(inst)
                        If (XmlInstrcut IsNot Nothing) Then
                            If ((String.Compare(XmlInstrcut.Target, "footnote", True) = 0) AndAlso (String.Compare(XmlInstrcut.Data, "value=""yes""", True) = 0)) Then
                                Return True
                            End If
                        End If
                    Next
                End If
            End If

            Return False
        End Function

        Public Function InsertLastTextFrameLabel() As Boolean
            Dim LastPage As Object = Nothing
            Dim PreviousPage As Object = Nothing
            Dim TextFrame As Object = Nothing
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return False
            End If
            LastPage = INDDGBL.InDesignDoc.Pages.LastItem()
            If ((LastPage.TextFrames Is Nothing) OrElse (LastPage.TextFrames.Count = 0)) Then
                TextFrame = GetEmptyFrameInDoc(LastPage)
                PreviousPage = INDDGBL.InDesignDoc.Pages.PreviousItem(LastPage)
                If (PreviousPage.TextFrames IsNot Nothing) AndAlso (PreviousPage.TextFrames.Count() > 0) Then
                    PreviousPage.TextFrames.LastItem().NextTextFrame = TextFrame
                End If
            End If
            For txt As Integer = 1 To LastPage.TextFrames.Count
                TextFrame = LastPage.TextFrames(txt)
                TextFrame.Label = "last"
                Return True
            Next
            Return False
        End Function

        Public Function GetArticleTitle(TagName As String) As String
            Dim XmlMgr As New XMLManager
            Dim TitleList As New List(Of Object)
            Dim Title As String = String.Empty
            TitleList = XmlMgr.GetXMLElementByName(TagName)
            If ((TitleList IsNot Nothing) AndAlso (TitleList.Count() > 0)) Then
                Title = CleanAuthorNames(TitleList(0).Texts(1).Contents)
                Title = Title.Replace(ChrW(10), "").Replace("  ", " ")
                Return Title
            Else
                Return String.Empty
            End If

        End Function

        Public Function GetDOI() As String
            Dim SearchMgr As New SearchManager
            Dim DOIObj As Object = Nothing
            DOIObj = SearchMgr.SearchByRegex(INDDGBL.InDesignDoc, "doi: (.*?)$")
            If ((DOIObj IsNot Nothing) AndAlso (DOIObj.Count() > 0)) Then
                For i As Integer = 1 To DOIObj.Count()
                    If ((DOIObj(i).Texts(1).ParentTextFrames IsNot Nothing) AndAlso (DOIObj(i).Texts(1).ParentTextFrames.Count() > 0)) Then
                        If (DOIObj(i).Texts(1).ParentTextFrames(1).ParentPage IsNot Nothing) Then
                            If (DOIObj(i).Texts(1).ParentTextFrames(1).ParentPage.Name = INDDGBL.InDesignDoc.Pages.FirstItem().Name) Then
                                Return DOIObj(i).Texts(1).Contents
                            End If
                        End If
                    End If
                Next
            End If
            Return String.Empty
        End Function

        Public Function CleanAuthorNames(JunkContent As String) As String
            JunkContent = JunkContent.Replace(ChrW(65279), "").Replace(ChrW(160), " ")
            Return JunkContent
        End Function

        Public Function CollectAuthorNames(TagName As String) As String
            Dim AuthorText As String = String.Empty
            Dim XmlMgr As New XMLManager
            Dim AuthorNames As New List(Of Object)
            AuthorNames = XmlMgr.GetXMLElementByName(TagName)
            If ((AuthorNames IsNot Nothing) AndAlso (AuthorNames.Count() > 0)) Then
                'For Each author In AuthorNames
                For au As Integer = 1 To AuthorNames.Count
                    Dim author As Object = AuthorNames(au)
                    AuthorText = IIf(String.IsNullOrEmpty(AuthorText), author.Texts(1).Contents, AuthorText & author.Texts(1).Contents)
                Next
                AuthorText = CleanAuthorNames(AuthorText)
                AuthorText = AuthorText.Replace(ChrW(10), "").Replace("  ", "")
                Return AuthorText
            Else
                Return String.Empty
            End If
        End Function

        Public Function CreateInDesignFootnotes() As Boolean
            Dim XmlMgr As New XMLManager
            Dim XElement As Object = Nothing
            Dim FootnoteList As New List(Of Object)
            Dim FootnoteObj As Object = Nothing
            Dim NewFootnote As Object = Nothing
            Dim FootnoteStyle As Object = Nothing
            FootnoteList = XmlMgr.GetXMLElementByName("footnote")
            If (FootnoteList Is Nothing) Then
                Return False
            End If
            Try
                FootnoteStyle = INDDGBL.InDesignDoc.ParagraphStyles.Add("FN")
            Catch ex As Exception
                FootnoteStyle = INDDGBL.InDesignDoc.ParagraphStyles.Item("FN")
            End Try
            For ft As Integer = FootnoteList.Count - 1 To 0 Step -1
                FootnoteObj = FootnoteList(ft)
                INDDGBL.InDesignApp.Select(FootnoteObj.Texts(1))
                INDDGBL.InDesignApp.Copy()
                XElement = FootnoteObj.Parent.XmlElements.PreviousItem(FootnoteObj)
                If (XElement IsNot Nothing) Then
                    XElement.InsertionPoints(1).Select()
                    FootnoteObj.Delete()
                    NewFootnote = XElement.InsertionPoints(1).Footnotes.Add()
                    NewFootnote.InsertionPoints(1).Select()
                    INDDGBL.InDesignApp.Paste()
                    NewFootnote.Texts(1).ApplyParagraphStyle(FootnoteStyle)
                End If
            Next
            Return True
        End Function

        


#Region "Public Property"

        


#End Region



    End Class

End Namespace
