Imports System.Text
Imports System.Web
Imports System.IO
Imports System.Xml
Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports Server_DigitalBundle.InDesign
Imports Server_DigitalBundle.InDesign.Basic
Imports Server_DigitalBundle.InDesign.Advanced
Imports System.Globalization
Imports MTSDKDN.ConvertEquations

Public Enum ErrorType
    NONE = 0
    MSGERROR = 1
    MSGINFO = 2
End Enum

Public Class InDesignConversionCleanup

    Public Const INDESIGN_NOTHING = &H6E616461
    Dim IsOnPageFootnote As Boolean = False
    Public Property AppTitle As String = String.Empty
    Const SWP_NOZORDER As Integer = &H4
    Const SWP_NOACTIVATE As Integer = &H10
    Dim ExportFloatList As New List(Of ExportFloatData)
    Dim IsFultextArticle As Boolean = False
    Dim HeadingLevels As New List(Of HeadingLevelData)

    Dim DocType As DocumentType
    Dim JournalMetaList As New List(Of JournalMetaData)
    Dim SupplientFloatLinks As New List(Of JournalMetaPermission)
    Dim SupplientURL As String = String.Empty
    Dim JournalPermissions As New List(Of JournalMetaPermission)
    Dim _XMLString As String = String.Empty
    Dim JournalName As String = String.Empty
    Dim ArticleID As String = String.Empty
    Dim VolumeNo As String = String.Empty
    Dim IssueNo As String = String.Empty
    Dim IndesignName As String = String.Empty
    Dim ExportFileName As String = String.Empty
    Dim TempExportFile As String = String.Empty
    Dim FirstPage As String = String.Empty
    Dim LastPage As String = String.Empty
    Dim XMLDoc As New Xml.XmlDocument
    Dim iParagraphs As Object

    Public Property XMLString As String
        Get
            Return _XMLString
        End Get
        Set(value As String)
            _XMLString = value
            'Console.WriteLine("....")
        End Set
    End Property
    Private Function unTagTableElement() As Boolean
        Dim xmlTable As New XMLManager
        Dim TagtoTextList As New List(Of TagtoTextData)
        TagtoTextList.Add(New TagtoTextData With {.TagName = "italic", .TextName = "italic"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "bold", .TextName = "bold"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "sup", .TextName = "sup"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "sub", .TextName = "sub"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "sc", .TextName = "sc"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "tp", .TextName = "tp"})
        If (String.Compare(JournalName, "OIKOS", True) <> 0) Then
            TagtoTextList.Add(New TagtoTextData With {.TagName = "xref", .TextName = "xref"})
        End If
        TagtoTextList.Add(New TagtoTextData With {.TagName = "emphasis[@role='italic']", .TextName = "i"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "emphasis[@role='bold']", .TextName = "b"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "emphasis[@role='underline']", .TextName = "u"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "superscript", .TextName = "sup"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "suberscript", .TextName = "sub"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "smallcaps", .TextName = "sc"})
        TagtoTextList.Add(New TagtoTextData With {.TagName = "underline", .TextName = "underline"})
        Dim TableList As Object = Nothing
        Dim ItalicList As Object = Nothing
        If (String.Compare(JournalName, "BDS", True) = 0) Then
            TableList = xmlTable.GetXMLElementByXPath("//tgroup")
        ElseIf (DocType = LanstadClientType.JOURNAL) Then
            TableList = xmlTable.GetXMLElementByXPath("//table")
        Else
            TableList = xmlTable.GetXMLElementByXPath("//tgroup")
        End If
        If ((TableList Is Nothing) OrElse (TableList.count = 0)) Then
            Return False
        End If
        For ii As Integer = 0 To TableList.count - 1
            Try
            For Each Tag As TagtoTextData In TagtoTextList
                ItalicList = xmlTable.GetXMLElementByXPath(TableList(ii), "//" & Tag.TagName)
                If ((ItalicList IsNot Nothing) AndAlso (ItalicList.Count > 0)) Then
                    For Each itTag As Object In ItalicList
                        Try
                            itTag.contents = "<" & Tag.TextName & ">" & itTag.contents & "</" & Tag.TextName & ">"
                        Catch ex As Exception
                            Continue For
                        End Try
                    Next
                End If
            Next
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
            Try
                TableList(ii).untag()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next
        Return True
    End Function
    Private Function RetaingingEmphasisinTable(ByVal TableObj As Object) As Boolean
        If (TableObj Is Nothing) Then
            Return False
        End If
        Return True
    End Function

    Private Function RemoveUnwantedElementInInDesign() As Boolean
        Dim xmlMgr As New XMLManager
        Dim UnwantedElements As New List(Of UnwantedElementData)
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "comment", .AttributeName = "", .AttributeValue = "", .IsUnTag = False})
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "", .AttributeName = "enter_key", .AttributeValue = "", .IsUnTag = False})
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "", .AttributeName = "store", .AttributeValue = ""})
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "", .AttributeName = "class", .AttributeValue = ""})
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "", .AttributeName = "label_ref", .AttributeValue = ""})
        'UnwantedElements.Add(New UnwantedElementData With {.ElementName = "", .AttributeName = "label_ref", .AttributeValue = ""})
        If (Convert.ToString(INDDGBL.InDesignDoc.Name).Contains("_PRELIMS_")) Then
            UnwantedElements.Add(New UnwantedElementData With {.ElementName = "chapter", .AttributeName = "", .AttributeValue = "", .IsUnTag = True})
            UnwantedElements.Add(New UnwantedElementData With {.ElementName = "LRH", .AttributeName = "", .AttributeValue = "", .IsUnTag = True})
            UnwantedElements.Add(New UnwantedElementData With {.ElementName = "RRH", .AttributeName = "", .AttributeValue = "", .IsUnTag = True})
        End If

        For u As Int32 = 0 To UnwantedElements.Count - 1
            If (Not String.IsNullOrEmpty(UnwantedElements(u).ElementName)) Then
                Try
                    xmlMgr.RemoveXMLElementByName(UnwantedElements(u).ElementName, UnwantedElements(u).IsUnTag)
                Catch ex As Exception
                End Try
            ElseIf (String.IsNullOrEmpty(UnwantedElements(u).ElementName)) Then
                Try
                    xmlMgr.RemoveXMLElementByAttribute(UnwantedElements(u).AttributeName, UnwantedElements(u).AttributeValue)
                Catch ex As Exception
                End Try
            End If
        Next
        Return True
    End Function

    Private Function AddHeadingLevelAttribute(ByVal WordContent As String) As String
        Dim xmlMgr As New XmlDocument
        xmlMgr.PreserveWhitespace = True
        Try
            xmlMgr.LoadXml(WordContent.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return WordContent
        End Try
        HeadingLevels.Clear()
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"A HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"A HEAD_N"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"AB HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"B HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"C HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"D HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 5, .PossibleHeadNames = New List(Of String)(New String() {"E HEAD"})})

        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"1_1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"4"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 5, .PossibleHeadNames = New List(Of String)(New String() {"5"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"2_1"})})

        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"H1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"H2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"H2-H1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"H3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"H3-H2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"H4"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"H4-H3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"H1_Intro"})})

        Try
            Dim ColspecList As XmlNodeList = xmlMgr.SelectNodes("//colspec")
            If ((ColspecList IsNot Nothing) AndAlso (ColspecList.Count > 0)) Then
                For c As Integer = ColspecList.Count - 1 To 0 Step -1
                    Try
                        If (ColspecList(c).ParentNode IsNot Nothing) Then
                            ColspecList(c).ParentNode.RemoveChild(ColspecList(c))
                        End If
                    Catch ex As Exception
                    End Try
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim HeadingNodes As XmlNodeList
        Dim DispAttribute As Object = Nothing
        HeadingNodes = xmlMgr.SelectNodes("//section/title")
        Dim tmpId As Integer = 0
        Dim HeadingDataList As New List(Of HeadingLevelData)
        Dim PStyleName As String = String.Empty
        If ((HeadingNodes IsNot Nothing) AndAlso (HeadingNodes.Count > 0)) Then
            For hd As Integer = 0 To HeadingNodes.Count - 1
                tmpId = hd
                PStyleName = String.Empty
                If (HeadingNodes(hd) Is Nothing) Then Continue For
                If (String.IsNullOrEmpty(HeadingNodes(hd).InnerText)) Then Continue For
                HeadingDataList = (From n In HeadingLevels Where (String.Compare(n.ClientName, JournalName, True) = 0) Select n).ToList
                Try
                    PStyleName = HeadingNodes(hd).Attributes("aid:pstyle").Value
                Catch ex As Exception
                    PStyleName = String.Empty
                    Continue For
                End Try

                If ((HeadingDataList Is Nothing) OrElse (HeadingDataList.Count = 0)) Then Continue For

                Dim HdData As HeadingLevelData = (From n In HeadingDataList Where n.PossibleHeadNames.Exists(Function(hdTxt As String)
                                                                                                                 If (String.Compare(hdTxt, PStyleName, True) = 0) Then
                                                                                                                     Return True
                                                                                                                 End If
                                                                                                                 Return False
                                                                                                             End Function) Select n).FirstOrDefault
                If ((HdData IsNot Nothing) AndAlso (HeadingNodes(hd) IsNot Nothing)) Then
                    Try
                        If (String.Compare(HeadingNodes(hd).ParentNode.Name, "section", True) <> 0) Then
                            Continue For
                        End If
                        Dim DispLevel As XmlAttribute = xmlMgr.CreateNode(XmlNodeType.Attribute, "disp-level", "")
                        DispLevel.Value = HdData.HeadLevel.ToString()
                        HeadingNodes(hd).ParentNode.Attributes.Append(DispLevel)
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    End Try
                End If
            Next
        End If




        Return xmlMgr.OuterXml.Replace("&amp;", "&")
    End Function


    Private Function AddHeadingLevelAttribute() As Boolean
        Dim xmlMgr As New XMLManager
        HeadingLevels.Clear()
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"A HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"B HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"C HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"D HEAD"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNFUK", .HeadLevel = 5, .PossibleHeadNames = New List(Of String)(New String() {"E HEAD"})})

        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"4"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "TNF", .HeadLevel = 5, .PossibleHeadNames = New List(Of String)(New String() {"5"})})

        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"H1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"H2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 2, .PossibleHeadNames = New List(Of String)(New String() {"H2-H1"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"H3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 3, .PossibleHeadNames = New List(Of String)(New String() {"H3-H2"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"H4"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 4, .PossibleHeadNames = New List(Of String)(New String() {"H4-H3"})})
        HeadingLevels.Add(New HeadingLevelData With {.ClientName = "BDS", .HeadLevel = 1, .PossibleHeadNames = New List(Of String)(New String() {"H1_Intro"})})

        Dim HeadingNodes As New List(Of Object)
        Dim DispAttribute As Object = Nothing
        HeadingNodes = xmlMgr.GetXMLElementByName("title")
        Dim tmpId As Integer = 0
        If ((HeadingNodes Is Nothing) OrElse (HeadingNodes.Count = 0)) Then Return False
        Dim HeadingDataList As New List(Of HeadingLevelData)
        For hd As Integer = 0 To HeadingNodes.Count - 1
            tmpId = hd
            If ((HeadingNodes(hd).Texts Is Nothing) OrElse (HeadingNodes(hd).Texts.Count = 0)) Then Continue For
            If (HeadingNodes(hd).Texts(1).AppliedParagraphStyle Is Nothing) Then Continue For
            HeadingDataList = (From n In HeadingLevels Where (String.Compare(n.ClientName, JournalName, True) = 0) Select n).ToList
            If ((HeadingDataList Is Nothing) OrElse (HeadingDataList.Count = 0)) Then Continue For
            Dim HdData As HeadingLevelData = (From n In HeadingDataList Where n.PossibleHeadNames.Exists(Function(hdTxt As String)
                                                                                                             If (String.Compare(hdTxt, HeadingNodes(tmpId).Texts(1).AppliedParagraphStyle.Name, True) = 0) Then
                                                                                                                 Return True
                                                                                                             End If
                                                                                                             Return False
                                                                                                         End Function) Select n).FirstOrDefault
            If ((HdData IsNot Nothing) AndAlso (HeadingNodes(hd).Parent IsNot Nothing)) Then
                Try
                    If (String.Compare(HeadingNodes(hd).Parent.MarkupTag.Name, "section", True) <> 0) Then
                        Continue For
                    End If

                    DispAttribute = HeadingNodes(hd).Parent.XmlAttributes.Item("disp-level")
                    DispAttribute.Value = HdData.HeadLevel.ToString()
                Catch ex As Exception
                    DispAttribute = HeadingNodes(hd).Parent.XmlAttributes.Add("disp-level", HdData.HeadLevel.ToString())
                End Try
            End If
        Next
        Return True
    End Function



    Private Function AddTableBreak() As Boolean
        For Each TxtFrame As Object In INDDGBL.InDesignDoc.TextFrames
            If ((TxtFrame.Tables Is Nothing) OrElse (TxtFrame.Tables.Count() = 0)) Then Continue For
            For tbl As Integer = 1 To TxtFrame.Tables.Count
                For row As Integer = 1 To TxtFrame.Tables(tbl).Rows.Count
                    For cl As Integer = 1 To TxtFrame.Tables(tbl).Rows(row).Cells.Count
                        Try
                            TxtFrame.Tables(tbl).Rows(row).Cells(cl).Contents = TxtFrame.Tables(tbl).Rows(row).Cells(cl).Contents.ToString().Replace(vbCr, "<break />").Replace(vbLf, "<break />")
                        Catch ex As Exception
                        End Try
                    Next
                Next
            Next
        Next
        Return True
    End Function

    Public Function AddSupplementaryLinkInInDesign() As Boolean
        'Dim Pattern As String = "[S|s]upplementary ([t|T]able|[F|f]igure|[F|f]ig.|[M|m]aterial)~S([A-Z0-9]?[0-9]+)"
        Dim Pattern As String = "[S|s]upplementary ([t|T]able|[F|f]igure|[F|f]ig.|[M|m]aterials|[M|m]aterial|[A|a]ppendix) ([A-Z0-9]?[0-9A-Z]+)?[ and 0-9A-Z]+"
        Dim SupplURL As String = String.Empty
        Dim JnName As String = Regex.Replace(Path.GetFileNameWithoutExtension(INDDGBL.InDesignDoc.Name), "[0-9-_]+", "")
        Dim SupplementaryList As Object = Nothing
        Dim ArticleID As String = Regex.Replace(Path.GetFileNameWithoutExtension(INDDGBL.InDesignDoc.Name), "[^0-9]+", "")
        ClearSearch()
        INDDGBL.InDesignApp.FindGrepPreferences = &H6E616461
        INDDGBL.InDesignApp.ChangeGrepPreferences = &H6E616461
        INDDGBL.InDesignApp.FindGrepPreferences.FindWhat = Pattern
        SupplementaryList = INDDGBL.InDesignDoc.FindGrep()
        If ((SupplementaryList Is Nothing) OrElse (SupplementaryList.Count = 0)) Then Return False
        For Each Supp As Object In SupplementaryList
            If ((Supp.Texts Is Nothing) OrElse (Supp.Texts.Count = 0)) Then Continue For
            SupplURL = (From jd In SupplientFloatLinks Where (String.Compare(jd.JournalName, JnName, True) = 0) Select jd.PermissionData).FirstOrDefault
            If (Not String.IsNullOrEmpty(SupplURL)) Then
                SupplURL = SupplURL.Replace("<InDesignName>", String.Format("{0}-{1}", JnName, Regex.Replace(ArticleID.Replace("_", ""), "([0-9]{1,2})([0-9]{1,4})", "$1-$2")))
                SupplURL = SupplURL.Replace("<ext-link ext-link-type=""uri"" xlink:href=""", "").Replace(""">", "")
            End If
            If (String.IsNullOrEmpty(SupplURL)) Then Continue For
            Supp.Texts(1).Select()

            Dim Script As String = "var myDocument = app.documents[0];" &
            "var myHyperlinkURL = myDocument.hyperlinkURLDestinations.add(""" & SupplURL & """, {name:Math.random().toString(), hidden:true});" &
            "var myHyperlinkSource = myDocument.hyperlinkTextSources.add(app.selection[0], {name:Math.random().toString()});" &
            "var myHyperlink = myDocument.hyperlinks.add(myHyperlinkSource, myHyperlinkURL, {visible:false, hidden:false, name:Math.random().toString()});"

            'Dim Script As String = "var myDocument = app.documents[0];" & _
            '"var myHyperlinkURL = myDocument.hyperlinkPageDestinations.add(myDocument.pages.firstItem(), {name:Math.random().toString(), hidden:true});" & _
            '"myHyperlinkURL.viewSetting = 1212437367;" & _
            '"var myHyperlinkSource = myDocument.hyperlinkTextSources.add(app.selection[0], {name:Math.random().toString()});" & _
            '"var myHyperlink = myDocument.hyperlinks.add(myHyperlinkSource, myHyperlinkURL, {visible:false, hidden:false, name:""" & SupplURL & """});"

            Try
                INDDGBL.InDesignApp.DoScript(Script, &H4A534C67)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        Next
        ClearSearch()
        Return True
    End Function

    Public Sub ClearSearch()
        If (INDDGBL.InDesignApp IsNot Nothing) Then
            INDDGBL.InDesignApp.FindChangeGlyphOptions = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindChangeGrepOptions = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindChangeObjectOptions = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindChangeTextOptions = INDESIGN_NOTHING

            INDDGBL.InDesignApp.FindGlyphPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindGrepPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindObjectPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.FindTextPreferences = INDESIGN_NOTHING

            INDDGBL.InDesignApp.ChangeGlyphPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.ChangeGrepPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.ChangeObjectPreferences = INDESIGN_NOTHING
            INDDGBL.InDesignApp.ChangeTextPreferences = INDESIGN_NOTHING
        End If
    End Sub

    Private Function AddSupplementaryDataLinkInDesign() As Boolean
        Dim SupplementData As Object = Nothing
        Dim SuppPage As Object = Nothing
        Dim TxtParaStyle As Object = Nothing
        ClearSearch()
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "supplementary data"
        'Try
        '    TxtParaStyle = INDDGBL.InDesignDoc.ParagraphStyles.Item("TXI")
        'Catch ex As Exception
        '    Return False
        'End Try
        INDDGBL.InDesignApp.FindTextPreferences.AppliedParagraphStyle = INDESIGN_NOTHING
        SupplementData = INDDGBL.InDesignDoc.FindText()
        If ((SupplementData Is Nothing) OrElse (SupplementData.count() = 0)) Then Return False

        SuppPage = SearchSupplementaryLinkPage()

        If (SuppPage Is Nothing) Then Return False

        For sup As Int32 = 1 To SupplementData.Count
            Dim Supp As Object = SupplementData(sup)
            If ((Supp.Texts Is Nothing) OrElse (Supp.Texts.Count() = 0)) Then Continue For
            If ((String.Compare(Supp.Texts(1).AppliedParagraphStyle.Name, "TXI", True) = 0) Or (String.Compare(Supp.Texts(1).AppliedParagraphStyle.Name, "TXT", True) = 0)) Then
                Supp.Texts(1).Select()
                Dim Script As String = "var suppage = arguments[0]; var myDocument = app.documents[0];" &
                "var myHyperlinkURL = myDocument.hyperlinkPageDestinations.add(suppage, {name:Math.random().toString(), hidden:true});" &
                "myHyperlinkURL.viewSetting = 1212437367;" &
                "var myHyperlinkSource = myDocument.hyperlinkTextSources.add(app.selection[0], {name:Math.random().toString()});" &
                "var myHyperlink = myDocument.hyperlinks.add(myHyperlinkSource, myHyperlinkURL, {visible:false, hidden:false, name:""" & Supp.Texts(1).Contents & sup.ToString() & """});"
                Try
                    INDDGBL.InDesignApp.DoScript(Script, &H4A534C67, New Object() {SuppPage})
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            End If
        Next
        Return True
    End Function

    Private Function SearchSupplementaryLinkPage() As Object
        Dim SuppHeadStyle As Object = Nothing
        Dim SuppPageData As Object = Nothing
        ClearSearch()
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "Supplementary data"
        'Try
        '    SuppHeadStyle = INDDGBL.InDesignDoc.ParagraphStyles.Item("H4")
        'Catch ex As Exception
        '    Return False
        'End Try
        'INDDGBL.InDesignApp.FindTextPreferences.AppliedParagraphStyle = SuppHeadStyle
        INDDGBL.InDesignApp.FindTextPreferences.AppliedParagraphStyle = INDESIGN_NOTHING
        SuppPageData = INDDGBL.InDesignDoc.FindText()
        If ((SuppPageData Is Nothing) OrElse (SuppPageData.Count() = 0)) Then Return Nothing

        For sp As Int32 = 1 To SuppPageData.Count
            If ((String.Compare(SuppPageData(sp).AppliedParagraphStyle.Name, "H4", True) = 0) Or (String.Compare(SuppPageData(sp).AppliedParagraphStyle.Name, "H1", True) = 0) Or (String.Compare(SuppPageData(sp).AppliedParagraphStyle.Name, "BM_HEAD", True) = 0)) Then
                If ((SuppPageData(sp).ParentTextFrames IsNot Nothing) AndAlso (SuppPageData(sp).ParentTextFrames.Count() > 0)) Then
                    Return SuppPageData(sp).ParentTextFrames(1).ParentPage
                End If
            End If
        Next
        Return Nothing
    End Function

    Public Function GetXMLElementByName(ElementName As String) As List(Of Object)
        Dim ElementList As New List(Of Object)
        Dim XPath As String = "//*"
        Dim RuleProcesser As Object = Nothing
        Dim MatchData As Object = Nothing
        Dim Elemnet As Object = Nothing
        If (INDDGBL.InDesignDoc Is Nothing) Then
            Return New List(Of Object)
        End If
        RuleProcesser = INDDGBL.InDesignApp.XMLRuleProcessors.Add(New String() {XPath})
        MatchData = RuleProcesser.StartProcessingRuleSet(INDDGBL.InDesignDoc.XmlElements(1))
        Try
            While (MatchData IsNot Nothing)
                If (String.Compare(MatchData.Element.MarkupTag.Name.ToString(), ElementName, True) = 0) Then
                    ElementList.Add(MatchData.Element)
                End If
                MatchData = RuleProcesser.FindNextMatch()
            End While
            Return ElementList
        Catch ex As Exception
            Return New List(Of Object)
        Finally
            RuleProcesser.EndProcessingRuleSet()
        End Try
    End Function

    Private Function FindTextByCharStyle(ByVal StyleName As String)
        Dim FindTextPre As Object = Nothing
        Dim SearchResult As Object = Nothing
        Dim CharStyleObj As Object = Nothing
        FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
        Try
            CharStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add()
            CharStyleObj.Name = StyleName
        Catch ex As Exception
            CharStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(StyleName)
        End Try
        FindTextPre.AppliedCharacterStyle = CharStyleObj
        FindTextPre.FindWhat = &H6E616461
        SearchResult = INDDGBL.InDesignApp.FindText
        If ((SearchResult IsNot Nothing) AndAlso (SearchResult.Count() > 0)) Then
            Return SearchResult(1).Texts(1).Contents
        Else
            Return "0"
        End If
        Return String.Empty
    End Function

    Private Function InDesignDocumentCleanUp() As Boolean
        INDDGBL.InDesignApp.FindTextPreferences = &H6E616461
        INDDGBL.InDesignApp.ChangeTextPreferences = &H6E616461
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "<b>"
        INDDGBL.InDesignApp.ChangeTextPreferences.ChangeTo = ""
        INDDGBL.InDesignDoc.ChangeText()

        INDDGBL.InDesignApp.FindTextPreferences = &H6E616461
        INDDGBL.InDesignApp.ChangeTextPreferences = &H6E616461
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "<\b>"
        INDDGBL.InDesignApp.ChangeTextPreferences.ChangeTo = ""
        INDDGBL.InDesignDoc.ChangeText()


        INDDGBL.InDesignApp.FindTextPreferences = &H6E616461
        INDDGBL.InDesignApp.ChangeTextPreferences = &H6E616461
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "<\i>"
        INDDGBL.InDesignApp.ChangeTextPreferences.ChangeTo = ""
        INDDGBL.InDesignDoc.ChangeText()

        INDDGBL.InDesignApp.FindTextPreferences = &H6E616461
        INDDGBL.InDesignApp.ChangeTextPreferences = &H6E616461
        INDDGBL.InDesignApp.FindTextPreferences.FindWhat = "<i>"
        INDDGBL.InDesignApp.ChangeTextPreferences.ChangeTo = ""
        INDDGBL.InDesignDoc.ChangeText()

        'Try
        '    InDesignTableCleanup()
        'Catch ex As Exception
        '    Console.WriteLine(ex.Message)
        '    Console.ReadKey()
        'End Try
        Return True
    End Function

    Private Function convertBulletsAndNumberingToText() As Boolean

        Try
            Dim iApp As Object = INDDGBL.InDesignApp

            Dim idDoc As Object = iApp.ActiveDocument

            __DoProcess_ConvertBulletsToTxt(idDoc)

        Catch ex As Exception

        End Try
        Return True
    End Function

    Private Function __DoProcess_ConvertBulletsToTxt(ixmlElms) As Object

        Try
            For ielm As Integer = 1 To ixmlElms.xmlElements.count
                If ixmlElms.xmlElements.item(ielm).markupTag.name = "para" Then

                    If ixmlElms.xmlElements.item(ielm).parent.markupTag.name = "listitem" Then
                        If ixmlElms.xmlElements.item(ielm).insertionPoints.item(1).appliedParagraphStyle.Name = "LBF" And ixmlElms.xmlElements.item(ielm).insertionPoints.item(1).appliedParagraphStyle.Name = "LB" Then
                            iParagraphs = ixmlElms.xmlElements.item(ielm).paragraphs
                            For iprg = 1 To iParagraphs.count
                                iParagraphs.item(iprg).convertBulletsAndNumberingToText()
                            Next
                        End If
                    End If
                End If

                __DoProcess_ConvertBulletsToTxt(ixmlElms.xmlElements.item(ielm))
            Next
        Catch ex As Exception

        End Try
        Return True
    End Function

    Private Function RevertInDesignFootnote() As Boolean
        If (Not GetFootnotes()) Then
            Return False
            If (Not FormatFootnoteID()) Then
                Return False
            End If
        End If
        Return True
    End Function

    Private Function GetFootnotes() As Boolean '' update on 28-Aug-2018 by Harihara Sudhan [for symbol information export options]
        Try
            Dim iApp As Object = INDDGBL.InDesignApp

            Dim idDoc As Object = iApp.ActiveDocument

            'MsgBox(idDoc.footnoteoptions.footnotenumberingStyle.ToString)

            If ((idDoc.footnoteoptions.footnotenumberingStyle.ToString = 1181971321) Or (idDoc.footnoteoptions.footnotenumberingStyle.ToString = 1298232180)) Then ' Symbol format validating
                Unicode_GetFootnotes()
            Else
                Numbering_GetFootnotes()
                Return True
            End If

        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Return True
    End Function

    Private Function Unicode_GetFootnotes() As Boolean

        'INDDGBL.InDesignApp
        'INDDGBL.InDesignDoc 
        Dim iFnoteIndxArray As New ArrayList
        Dim iFnotes As Object
        Dim iStories As Object
        Dim iFnoteElm As Object
        Dim icounter As Integer = 0
        Dim iPreviousPage_Info As Integer

        Try

            Dim iApp As Object = INDDGBL.InDesignApp

            Dim idDoc As Object = iApp.ActiveDocument

            iFnoteIndxArray.Add("&#x002A;")
            iFnoteIndxArray.Add("&#x2020;")
            iFnoteIndxArray.Add("&#x00A7;")
            iFnoteIndxArray.Add("&#x00B6;")
            iFnoteIndxArray.Add("&#x2021;")
            iFnoteIndxArray.Add("&#x2051;")

            iStories = idDoc.stories

            For istr As Integer = 1 To iStories.count
                Try
                    iFnotes = iStories(istr).footnotes
                    For ifn As Integer = 1 To iFnotes.count
                        Try
                            iFnoteElm = iFnotes(ifn).storyoffset.associatedXMLElements.item(1)
                            Dim iCurntPage_Info As Integer = iFnoteElm.insertionpoints.item(1).parenttextframes.item(1).parentpage.name.ToString

                            'MsgBox(iCurntPage_Info)
                            'MsgBox(iPreviousPage_Info)
                            If icounter <> 6 Then
                                If iPreviousPage_Info = 0 Then
                                    iFnoteElm.xmlAttributes.add("ftlabel", iFnoteIndxArray.Item(icounter))
                                    'iFnotes(ifn).characters.item(1).insertionpoints.item(1).contents = String.Format("<ftentity>{0}</ftentity>", iFnoteIndxArray.Item(icounter))
                                    iPreviousPage_Info = iCurntPage_Info
                                    icounter = icounter + 1
                                ElseIf iPreviousPage_Info <> iCurntPage_Info Then
                                    iFnoteElm.xmlAttributes.add("ftlabel", iFnoteIndxArray.Item(0))
                                    'iFnotes(ifn).characters.item(1).insertionpoints.item(1).contents = String.Format("<ftentity>{0}</ftentity>", iFnoteIndxArray.Item(0))
                                    iPreviousPage_Info = iCurntPage_Info
                                    icounter = 1
                                ElseIf iPreviousPage_Info = iCurntPage_Info Then
                                    iFnoteElm.xmlAttributes.add("ftlabel", iFnoteIndxArray.Item(icounter))
                                    'iFnotes(ifn).characters.item(1).insertionpoints.item(1).contents = String.Format("<ftentity>{0}</ftentity>", iFnoteIndxArray.Item(icounter))
                                    icounter = icounter + 1
                                End If
                            ElseIf icounter > 6 Then
                                iFnoteElm.xmlAttributes.add("ftlabel", iFnoteIndxArray.Item(0))
                                'iFnotes(ifn).characters.item(0).insertionpoints.item(1).contents = String.Format("<ftentity>{0}</ftentity>", iFnoteIndxArray.Item(0))
                                icounter = icounter + 1
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try
                    Next
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next

            Dim FootnoteItem As Object = Nothing
            Dim ConvertedText As Object = Nothing
            Dim StyleMgr As New StyleToTagManager
            Dim ParagraphElement As Object = Nothing
            Try
                For istr_n As Integer = iStories.count To 1 Step -1
                    iFnotes = iStories(istr_n).footnotes
                    IsOnPageFootnote = True
                    If ((iFnotes Is Nothing) OrElse (iFnotes.Count = 0)) Then
                        Continue For
                    End If
                    For ifn_n As Integer = iFnotes.count To 1 Step -1
                        Try
                            FootnoteItem = iFnotes(ifn_n)
                            ConvertedText = FootnoteItem.ConvertToText()
                            StyleMgr.CreateStyleToTag(ConvertedText)
                            If ((ConvertedText.AssociatedXMLElements IsNot Nothing) AndAlso (ConvertedText.AssociatedXMLElements.Count() > 0)) Then
                                'ConvertedText.AssociatedXMLElements(1).MarkupTag = FootnoteTag
                                ParagraphElement = ConvertedText.AssociatedXMLElements(1).XmlElements.Add(INDDGBL.InDesignDoc.XmlTags("footnote"))
                                ParagraphElement.XmlAttributes.Add("xml:id", "")
                                ConvertedText.Texts(1).Markup(ParagraphElement)
                            End If
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Continue For
                        End Try
                    Next
                Next
            Catch ex As Exception
            End Try
        Catch ex As Exception
        End Try
        Return True
    End Function


    Private Function Numbering_GetFootnotes() As Boolean
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
            IsOnPageFootnote = True
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

    Private Function InDesignTableCleanup() As Boolean
        Dim InddTables As Object = Nothing
        InddTables = GetXMLElementByName("table")
        For Each TblElement As Object In InddTables
            If (TblElement.Texts Is Nothing) OrElse (TblElement.Texts.Count = 0) Then Continue For
            If (TblElement.Texts(1).Tables Is Nothing) OrElse (TblElement.Texts(1).Tables.Count = 0) Then Continue For
            Dim Tbl = TblElement.Texts(1).Tables(1)
            For r As Integer = 1 To Tbl.Rows.Count
                Try
                    Tbl.Rows(r).UnMerge()
                Catch ex As Exception
                End Try
            Next
            For c As Integer = 1 To Tbl.Columns.Count
                Try
                    Tbl.Columns(c).UnMerge()
                Catch ex As Exception
                End Try
            Next
        Next
        Return True
    End Function

    Private Function FormatFileName(InddName As String) As String
        Dim TempName As String = String.Empty
        TempName = Regex.Replace(InddName.Replace("-", "").Replace("_", ""), "([A-Z]{3,3})([0-9]{2,2})([0-9]{4,4})", "$1-$2-$3")
        ArticleID = Regex.Replace(InddName.Replace("-", "").Replace("_", ""), "([A-Z]{3,3})([0-9]{2,2})([0-9]{4,4})", "$2-$3")
        Return TempName
    End Function

    Public Function TandFExportCleanUp(WordContent As String) As String
        Dim XmlDoc As New XmlDocument
        XmlDoc.XmlResolver = Nothing
        Dim IsSuperFoot As Boolean = True
        Dim FootnoteNodes As XmlNodeList = Nothing
        Dim Label As String = String.Empty
        Dim RefNode As XmlNode = Nothing
        Dim IsFootnote As Boolean = False
        XmlDoc.PreserveWhitespace = True
        WordContent = WordContent.Replace(" xmlns=""""", "")
        WordContent = Regex.Replace(WordContent, "<!--<LRH[^>]*>(.*?)</RRH>-->", "")
        WordContent = WordContent.Replace("&amp;lt;ftentity&amp;gt;", "<ftentity>").Replace("&amp;lt;/ftentity&amp;gt;", "</ftentity>")
        'WordContent = Regex.Replace(WordContent, "(<label><emphasis[^>]+>F?f.*?)(</emphasis></label>)(&#x2002;)", "$1$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'WordContent = Regex.Replace(WordContent, "(<label><emphasis[^>]+>T?t.*?)(</emphasis></label>)(&#x2003;)", "$1$2", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        WordContent = Regex.Replace(WordContent, "</label>&#x2002; <caption>", "</label><caption>")
        WordContent = Regex.Replace(WordContent, "</label>&#x2003; <title>", "</label><title>")

        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XmlDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")

        Try
            XmlDoc.LoadXml(WordContent.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return String.Empty
        End Try

        Dim FootnoteList As XmlNodeList = XmlDoc.SelectNodes("//footnote[@role='end-ch-note']")
        If ((FootnoteList IsNot Nothing) AndAlso (FootnoteList.Count > 0)) Then
            GBL.DeantaBallon($"Footnote Re-number started. {Path.GetFileName(ExportFileName)}", MessageType.MSGINFO)
            For f As Integer = 0 To FootnoteList.Count - 1
                Label = String.Empty
                If (FootnoteList(f).OuterXml.Contains("<ftentity>")) Then
                    Continue For
                End If
                Try
                    Label = FootnoteList(f).Attributes("label").Value
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Label = String.Empty
                End Try
                If (String.IsNullOrEmpty(Label)) Then Continue For
                If (Not (String.Compare(Label, $"{f + 1}", True) = 0)) Then
                    GBL.DeantaBallon($"Footnote label attribute and display text mismatched. expected label - {f + 1} | actual label {Label}", MessageType.MSGERROR)
                    FootnoteList(f).Attributes("label").Value = $"{f + 1}"
                End If
            Next
        End If

        Dim FnMaxCount As Integer = 0
        Dim FnIndex As Integer = 0
        Dim FtEntityMat As Match = Nothing
        Dim FtLabel As String = String.Empty
        Dim FnNotes As XmlNode = Nothing
        If (IsOnPageFootnote) Then
            FootnoteNodes = XmlDoc.SelectNodes("//footnote[@role='end-ch-note'][not (preceding-sibling::*[1][local-name()='xref'])]", NameSpaceManager)
            If ((FootnoteNodes IsNot Nothing) AndAlso (FootnoteNodes.Count > 0)) Then
                FnMaxCount = FootnoteNodes.Count
                While ((FootnoteNodes IsNot Nothing) AndAlso (FootnoteNodes.Count > 0))
                    If (FnMaxCount < FnIndex) Then
                        Exit While
                    End If
                    FnNotes = FootnoteNodes(0)
                    Label = CType(FnNotes, XmlElement).GetAttribute("label")
                    'FtLabel = Label
                    If (String.IsNullOrEmpty(Label)) Then
                        Label = ""
                        FtLabel = ""
#If CONFIG = "Debug" Or CONFIG = "FinalXML" Then
                        If (FnNotes.ParentNode IsNot Nothing) Then
                            FnNotes.ParentNode.RemoveChild(FnNotes)
                        End If
                        GBL.DeantaBallon("Footnote Label Missing : " & FnNotes.OuterXml, MessageType.MSGERROR)
                        FootnoteNodes = XmlDoc.SelectNodes("//footnote[@role='end-ch-note'][not (preceding-sibling::*[1][local-name()='xref'])]", NameSpaceManager)
                        FnIndex += 1
                        Continue While
#Else
                        GBL.DeantaBallon("Footnote Label Missing : " & FnNotes.OuterXml, MessageType.MSGERROR)
                        Return String.Empty
#End If

                    End If
                    If (FnNotes.ParentNode IsNot Nothing) Then
                        If (FnNotes.OuterXml.Contains("<ftentity>")) Then
                            FtEntityMat = Regex.Match(FnNotes.OuterXml, "<ftentity[^>]*>((?:(?!<(\/ftentity)>).)*)</ftentity>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            'FtLabel = FtEntityMat.Groups(1).Value.Replace("&amp;amp;amp;", "&amp;")
                            FtLabel = Regex.Replace(FtEntityMat.Groups(1).Value, "&amp;(amp;)+", "&amp;", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                        ElseIf (FnNotes.OuterXml.Contains("ftlabel")) Then
                            FtLabel = CType(FnNotes, XmlElement).GetAttribute("ftlabel")
                            FtLabel = Regex.Replace(FtLabel, "&amp;(amp;)+", "&amp;", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                            Try
                                FnNotes.Attributes.Remove(FnNotes.Attributes("ftlabel"))
                            Catch ex As Exception
                            End Try
                        Else
                            FtLabel = CType(FnNotes, XmlElement).GetAttribute("label")
                        End If
                        If Not (Regex.Match(FtLabel, "&(amp;)?#x[A-F0-9]{4,4};", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) Then
                            If (String.Compare(Label.Trim(), FtLabel, True) <> 0) Then
                                FtLabel = Label
                                GBL.DeantaBallon("Footnote label and superscript text is not matched. See the footnote text : " & FnNotes.ParentNode.OuterXml, MessageType.MSGERROR)
                            End If
                        End If
                        'If (FtEntityMat IsNot Nothing) Then
                        If (IsOnPageFootnote) Then
                            FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & FtLabel & "</sup></xref>" & FnNotes.OuterXml.Replace(vbTab, ""))
                        Else
                            FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & FtLabel & "</sup></xref>" & FnNotes.OuterXml)
                        End If
                    End If
                    FootnoteNodes = XmlDoc.SelectNodes("//footnote[@role='end-ch-note'][not (preceding-sibling::*[1][local-name()='xref'])]", NameSpaceManager)
                    FnIndex += 1
                End While

            End If
            'For Each FnNotes As XmlNode In FootnoteNodes
            '    Label = CType(FnNotes, XmlElement).GetAttribute("label")
            '    If (String.IsNullOrEmpty(Label)) Then
            '        Label = ""
            '    End If
            '    If (FnNotes.ParentNode IsNot Nothing) Then
            '        FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & Label & "</sup></xref>" & FnNotes.OuterXml)
            '    End If
            'Next
        End If

        If (Path.GetFileName(ExportFileName).ToLower().Contains("notes")) Then
            GoTo footSkip
        End If

        '' Move Footnotes.
        Dim MaxCnt As Int16 = 0
        Dim IsFails As Boolean = False
        FootnoteNodes = XmlDoc.SelectNodes("//footnote[@role='end-bk-note'][not (preceding-sibling::*[1][local-name()='xref'])]/superscript")
        IsSuperFoot = False
        If ((FootnoteNodes Is Nothing) OrElse (FootnoteNodes.Count = 0)) Then
            FootnoteNodes = XmlDoc.SelectNodes("//footnote[@role='end-bk-note'][@xml:id][not (preceding-sibling::*[1][local-name()='xref'])]", NameSpaceManager)
            IsSuperFoot = True
        End If
        FnNotes = Nothing
        If ((FootnoteNodes IsNot Nothing) AndAlso (FootnoteNodes.Count > 0)) Then
            MaxCnt = FootnoteNodes.Count * 3
            While ((FootnoteNodes IsNot Nothing) AndAlso (FootnoteNodes.Count > 0))
                If (IsFails) Then
                    FnNotes = FootnoteNodes(FootnoteNodes.Count - 1)
                    IsFails = False
                Else
                    FnNotes = FootnoteNodes(0)
                End If

                If (MaxCnt = 0) Then
                    Exit While
                End If
                If (FnNotes.ParentNode IsNot Nothing) Then
                    If (IsFootnote) Then
                        Label = CType(FnNotes, XmlElement).GetAttribute("label")
                    Else
                        If (IsSuperFoot) Then
                            Label = CType(FnNotes, XmlElement).GetAttribute("label")
                        Else
                            Label = CType(FnNotes.ParentNode, XmlElement).GetAttribute("label")
                        End If
                    End If

                    RefNode = GetFootnoteRefNode(XmlDoc, Label)

                    If (RefNode Is Nothing) Then
                        GBL.DeantaBallon($"Could not able to find the footnote label soruce : {Label}. So conversion quitting.", MessageType.MSGERROR)
                        RefNode = GetFootnoteRefNode(XmlDoc, Label, True)
                    End If
                    If (RefNode Is Nothing) Then
                        GBL.DeantaBallon($"Could not able to find the footnote label with start text also. : {Label}. So conversion quitting.", MessageType.MSGERROR)
                        MaxCnt = MaxCnt - 1
                        IsFails = True
                        Continue While
                    End If
                    If (Not IsSuperFoot) Then
                        If (String.Compare(Label, FnNotes.InnerText.Trim(), True) <> 0) Then
                            Label = FnNotes.InnerText.Trim()
                            GBL.DeantaBallon("Footnote label and superscript text is not matched. See the footnote text : " & FnNotes.ParentNode.OuterXml, MessageType.MSGERROR)
                        End If
                    End If


                    If (IsFootnote) Then
                        FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & Label & "</sup></xref>" & FnNotes.OuterXml)
                        'FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & FnNotes.InnerText & "</sup></xref>" & Regex.Replace(RefNode.InnerXml, "(>[\s&amp;#x2002;0-9.\s]+)", ">"))
                    Else
                        If (IsSuperFoot) Then
                            Dim fnText As String = "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & FnNotes.InnerText & "</sup></xref>"
                            FnNotes.ParentNode.InnerXml = FnNotes.ParentNode.InnerXml.Replace(FnNotes.OuterXml, $"{fnText}{FnNotes.OuterXml.Replace($">{FnNotes.InnerText}<", ">" & Regex.Replace(RefNode.InnerXml, "(<para[^>]*>(\s+)?(&amp;#x2002;)?" & FnNotes.InnerText.Replace("*", "\*") & "(\.)?(\s+)?)", "<para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase) & "<")}")
                            'FnNotes.InnerXml = 
                        Else
                            FnNotes.ParentNode.InnerXml = "<xref ref-type=""fn"" rid=""fn" & Label & """><sup>" & FnNotes.InnerText & "</sup></xref>" & Regex.Replace(RefNode.InnerXml, "(<para[^>]*>(\s+)?(&amp;#x2002;)?" & FnNotes.InnerText.Replace("*", "\*") & "(\.)?(\s+)?)", "<para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                        End If
                        Try
                            RefNode.ParentNode.RemoveChild(RefNode)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            MaxCnt = MaxCnt - 1
                            Continue While
                        End Try
                    End If
                End If
                If (IsSuperFoot) Then
                    FootnoteNodes = XmlDoc.SelectNodes("//footnote[@xml:id][not (preceding-sibling::*[1][local-name()='xref'])]", NameSpaceManager)
                Else
                    FootnoteNodes = XmlDoc.SelectNodes("//footnote[not (preceding-sibling::*[1][local-name()='xref'])]/superscript")
                End If
                MaxCnt = MaxCnt - 1
                IsFails = False
            End While
        End If

footSkip:

        WordContent = XmlDoc.OuterXml.Replace("&amp;", "&")
        WordContent = Regex.Replace(WordContent, "(<footnote[^>]*>)(<xref[^>]*>((?:(?!</xref>).)+)</xref>)", "$2$1")

        Return WordContent
    End Function


    Private Function GetFloatNode(XmlDoc As XmlDocument, NameSpaceManager As System.Xml.XmlNamespaceManager, ByVal xmlID As String, ByVal floatname As String) As XmlNode
        Dim Floats As XmlNodeList = Nothing
        If (String.Compare(floatname, "figure", True) = 0) Then
            Floats = XmlDoc.SelectNodes("//figure[@xml:id='" & xmlID & "']", NameSpaceManager)
        Else
            Floats = XmlDoc.SelectNodes("//table[@xml:id='" & xmlID & "']", NameSpaceManager)
        End If
        If ((Floats IsNot Nothing) AndAlso (Floats.Count > 0)) Then
            Return Floats(0)
        End If
        Return Nothing
    End Function

    Private Function GetFootnoteWithDispLabelNode(XmlDoc As XmlDocument, Label As String) As XmlNode
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XmlDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        Dim FootnoteRefList As XmlNodeList = XmlDoc.SelectNodes($"//footnote[text()='{Label}']|//footnote/superscript[text()='{Label}']", NameSpaceManager)
        If ((FootnoteRefList Is Nothing) OrElse (FootnoteRefList.Count = 0)) Then Return Nothing
        For Each FtRef As XmlNode In FootnoteRefList
            If (String.Compare(FtRef.Name, "superscript", True) = 0) Then
                Return FtRef.ParentNode
            Else
                Return FtRef
            End If
        Next
        Return Nothing
    End Function

    Private Function GetFootnoteNode(XmlDoc As XmlDocument, Label As String, ByVal xmlID As String) As XmlNode
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XmlDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        Dim FootnoteRefList As XmlNodeList = XmlDoc.SelectNodes("//footnote[@label='" & Label & "' and @linkend='" & xmlID & "']", NameSpaceManager)
        If ((FootnoteRefList Is Nothing) OrElse (FootnoteRefList.Count = 0)) Then Return Nothing
        For Each FtRef As XmlNode In FootnoteRefList
            Return FtRef
        Next
        Return Nothing
    End Function

    Private Function GetFootnoteRefNode(XmlDoc As XmlDocument, Label As String, ByVal isLabel As Boolean) As XmlNode
        Dim FootnoteRefList As XmlNodeList = XmlDoc.SelectNodes("//footnote[not (@linkend)][@role='end-bk-note']")
        If ((FootnoteRefList Is Nothing) OrElse (FootnoteRefList.Count = 0)) Then Return Nothing
        For Each FtRef As XmlNode In FootnoteRefList
            Try
                Dim firstChild As XmlNode = FtRef.FirstChild
                If (firstChild.InnerXml.StartsWith(Label)) Then
                    Return FtRef
                End If
            Catch ex As Exception
                Continue For
            End Try
        Next
        Return Nothing
    End Function
    Private Function GetFootnoteRefNode(XmlDoc As XmlDocument, Label As String) As XmlNode
        Dim FootnoteRefList As XmlNodeList = XmlDoc.SelectNodes("//footnote[@role='end-bk-note'][@label='" & Label & "']")
        If ((FootnoteRefList Is Nothing) OrElse (FootnoteRefList.Count = 0)) Then Return Nothing
        For Each FtRef As XmlNode In FootnoteRefList
            If ((FtRef.HasChildNodes) AndAlso ((String.Compare(FtRef.ChildNodes(0).Name, "para", True) = 0) OrElse (String.Compare(FtRef.ChildNodes(0).Name, "blockquote", True) = 0))) Then
                Return FtRef
            End If
        Next
        Return Nothing
    End Function

    Public Function ConvertInDesignXMLtoClient(ByVal DocType As LanstadClientType, ByVal ExportedFile As String, ByVal OutputFile As String) As Boolean
        Dim WordContent As String = String.Empty
        Try
            InitializeJournalMetaData()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        XMLDoc.PreserveWhitespace = True
        XMLDoc.XmlResolver = Nothing
        ExportFileName = OutputFile
        Dim TmpFolder As String = Path.Combine(Path.GetDirectoryName(ExportedFile), "temp")
        If (Not Directory.Exists(TmpFolder)) Then
            Directory.CreateDirectory(TmpFolder)
        End If

        TempExportFile = Path.Combine(TmpFolder, Path.GetFileName(ExportedFile))

        JournalName = Path.GetFileNameWithoutExtension(TempExportFile)
        If (JournalName.Contains("_") Or JournalName.Contains("-")) Then
            JournalName = JournalName.Split(New Char() {"_", "-"})(0)
        Else
            GBL.DeantaBallon("Could not able to find the client abbrevation.", MessageType.MSGERROR)
            Return False
        End If

        File.Copy(ExportedFile, TempExportFile, True)

        XMLDoc.PreserveWhitespace = True
        XMLDoc.XmlResolver = Nothing
        If (Not File.Exists(TempExportFile)) Then
            GBL.DeantaBallon("Could not able to find the exported indesign xml file.", MessageType.MSGERROR)
            Return False
        End If

        WordContent = File.ReadAllText(TempExportFile).Replace("ftlabel=""&amp;", "ftlabel=""&").Replace("&amp;", "&#x0026;")

        WordContent = Regex.Replace(WordContent, "(<superscript[^>]*>)(<footnote[^>]*>)", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WordContent = RetainingMathContent(WordContent)

        'WordContent = RetainTableFromInDesignStyles(WordContent)

        WordContent = Regex.Replace(WordContent, "(<fig)( id=""F[0-9]+"")( fig-type=""figure"")", "$1$3$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "(<fig)( id=""F[0-9]+"")( position=""float"")( fig-type=""figure"")", "$1$4$2$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WordContent = Regex.Replace(WordContent, "</link><link[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "</uri><uri>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WordContent = WordContent.Replace(" standalone=""yes""?>", "?>")

        WordContent = WordContent.Replace(" role=""extract_group""", "")
        WordContent = WordContent.Replace(" role=""ExtractText""", "")
        WordContent = WordContent.Replace(" position=""float""", "")

        WordContent = WordContent.Replace(" standalone=""yes""?>", "?>")
        WordContent = WordContent.Replace(ChrW(8232), "")
        WordContent = Regex.Replace(WordContent, "<\!--(<a[^>]+/>)-->", "$1")

        WordContent = WordContent.Replace("&lt;", "&#x003C;")
        WordContent = WordContent.Replace("&gt;", "&#x003E;")

        WordContent = WordContent.Replace("&lt;\b&gt;", "</b>")
        WordContent = WordContent.Replace("&lt;/b&gt;", "</b>")

        WordContent = WordContent.Replace("&lt;xref&gt;", "<xreff>")
        WordContent = WordContent.Replace("&lt;/xref&gt;", "</xreff>")
        WordContent = WordContent.Replace("&lt;italic&gt;", "<italic>")
        WordContent = WordContent.Replace("&lt;/italic&gt;", "</italic>")
        WordContent = WordContent.Replace("&lt;bold&gt;", "<bold>")
        WordContent = WordContent.Replace("&lt;/bold&gt;", "</bold>")
        WordContent = WordContent.Replace("&lt;sup&gt;", "<sup>")
        WordContent = WordContent.Replace("&lt;/sup&gt;", "</sup>")
        WordContent = WordContent.Replace("&lt;i&gt;", "&#x003C;i&#x003E;")
        WordContent = WordContent.Replace("&lt;sub&gt;", "<Sub>")
        WordContent = WordContent.Replace("&lt;/sub&gt;", "</Sub>")
        If (WordContent.Contains("&lt;u&gt;") And WordContent.Contains("&lt;/u&gt;")) Then
            WordContent = WordContent.Replace("&lt;u&gt;", "<underline>")
            WordContent = WordContent.Replace("&lt;/u&gt;", "</underline>")
        End If
        WordContent = WordContent.Replace("&lt;underline&gt;", "<underline>")
        WordContent = WordContent.Replace("&lt;/underline&gt;", "</underline>")
        WordContent = WordContent.Replace("&lt;sc&gt;", "<sc>")
        WordContent = WordContent.Replace("&lt;/sc&gt;", "</sc>")
        WordContent = WordContent.Replace("&lt;b&gt;", "<b>")
        WordContent = WordContent.Replace("&lt;\i&gt;", "</i>")
        WordContent = WordContent.Replace("&lt;/i&gt;", "</i>")
        WordContent = WordContent.Replace("&lt;i&gt;", "<i>")
        Dim altTitle As String = String.Empty

        If (Not TempExportFile.ToUpper().Contains("_PRELIMS_")) Then
            WordContent = WordContent.Replace("<CHO>", "").Replace("</CHO>", "")
        ElseIf (TempExportFile.ToUpper().Contains("_PRELIMS_")) Then
            WordContent = Regex.Replace(WordContent, "<chapter[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</chapter>", "")
            If ((Regex.IsMatch(WordContent, "<CHO[^>]*><dedication", RegexOptions.IgnoreCase Or RegexOptions.Singleline)) Or (Regex.IsMatch(WordContent, "<CHO[^>]*><info", RegexOptions.IgnoreCase Or RegexOptions.Singleline))) Then
                WordContent = WordContent.Replace("<CHO>", "").Replace("</CHO>", "")
            End If
        End If

        altTitle = Regex.Match(WordContent, "(<alt-title alt-title-type=""rrh"">.*?</alt-title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        If (Not String.IsNullOrEmpty(altTitle)) Then
            WordContent = WordContent.Replace(altTitle, "")
            WordContent = Regex.Replace(WordContent, "(<title-group><article-title[^>]*>((?!<(\/)?article-title>).)*<\/article-title>)", "$1" & altTitle)
        End If

        altTitle = Regex.Match(WordContent, "(<alt-title alt-title-type=""lrh"">.*?</alt-title>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        If (Not String.IsNullOrEmpty(altTitle)) Then
            WordContent = WordContent.Replace(altTitle, "")
            WordContent = Regex.Replace(WordContent, "(<title-group><article-title[^>]*>((?!<(\/)?article-title>).)*<\/article-title>)", "$1" & altTitle)
        End If

        WordContent = WordContent.Replace(" aid:theader=""""", "")
        WordContent = Regex.Replace(WordContent, "<!--((?:(?:(?!-->).)+)-->)", AddressOf RefComentPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        WordContent = WordContent.Replace(ChrW(65279), "")

        WordContent = WordContent.Replace("<Story></Story>", "")
        WordContent = WordContent.Replace("&lt;br /&gt;", "<br/>")
        WordContent = WordContent.Replace("&lt;break /&gt;", "<break/>")
        WordContent = WordContent.Replace("<break/><break/>", "<break/>")
        WordContent = Regex.Replace(WordContent, "<emphasis[^>]*></emphasis>", "")

        '01-09-2020
        WordContent = Regex.Replace(WordContent, "(<footnote[^>]*>)(<superscript[^>]*>)(<footnote[^>]*>)", "$1$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "(</footnote>)(</superscript>)(</footnote>)", "$1$3", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        If ((Regex.Match(WordContent, "(<footnote[^>]*>)([\s]+)(<footnote[^>]*>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) And
                (Regex.Match(WordContent, "(</footnote>)((?:(?!<(\/)?footnote[^>]*>).)*)(</footnote>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success)) Then
            WordContent = Regex.Replace(WordContent, "(<footnote[^>]*>)([\s]+)(<footnote[^>]*>)", "$1<para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            WordContent = Regex.Replace(WordContent, "(</footnote>)((?:(?!<(\/)?footnote[^>]*>).)*)(</footnote>)", "$2$3</para>$4", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            WordContent = WordContent.Replace("</footnote></footnote>", "</para></footnote>")
            WordContent = WordContent.Replace("</footnote> </footnote>", "</para></footnote>")
        End If

        If ((Regex.Match(WordContent, "(<footnote[^>]*>)(<footnote[^>]*>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success) And
            (Regex.Match(WordContent, "(</footnote>)((?:(?!<(\/)?footnote[^>]*>).)*)(</footnote>)", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Success)) Then
            WordContent = Regex.Replace(WordContent, "(<footnote[^>]*>)(<footnote[^>]*>)", "$1<para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            WordContent = Regex.Replace(WordContent, "(</footnote>)((?:(?!<(\/)?footnote[^>]*>).)*)(</footnote>)", "$2$3</para>$4", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            WordContent = WordContent.Replace("</footnote></footnote>", "</para></footnote>")
            WordContent = WordContent.Replace("</footnote> </footnote>", "</para></footnote>")
        End If


        WordContent = HtmlEncode(WordContent)

        WordContent = WordContent.Replace("&#x200B;", "")
        WordContent = Regex.Replace(WordContent, "<x[^>]*>(&(?:amp;)?#x[A-F0-9]{1,4};)<\/x>", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        WordContent = Regex.Replace(WordContent, "<x role=""page"">(((?!<(\/)?x>).)*)<\/x>", "$1", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        WordContent = Regex.Replace(WordContent, "(<x( [^>]+)?>(((?!<(\/)?x>).)*)<\/x>)", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        If (DocType = LanstadClientType.JOURNAL) Then
            WordContent = Regex.Replace(WordContent, "<colspec[^>]*>((?!(</colspec>).)*)</colspec>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            WordContent = WordContent.Replace("<row>", "<tr>").Replace("</row>", "</tr>").Replace("<entry ", "<td ").Replace("</entry>", "</td>")
            WordContent = Regex.Replace(WordContent, "<td[^>]*>", Function(mt As Match)
                                                                      Return mt.Value.Replace(" colsep=""0""", "").Replace(" rowsep=""0""", "")
                                                                  End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        End If

        If (String.Compare(JournalName, "OIKOS", True) = 0) Then
            Dim entityCharsNo As New List(Of String)(New String() {"&#x003D;", "&#x003C;", "&#x003E;", "&#x002B;", "&#x00B1;", "&#x2260;", "&#x2264;", "&#x2265;", "&#x00D7;"})
            Dim MathPiEntity As New List(Of String)(New String() {"&#xE035;", "&#xE02C;", "&#xE02E;", "&#xE031;", "&#xE036;", "&#xE0DE;", "&#xE023;", "&#xE024;", "&#xE033;"})
            For en As Integer = 0 To MathPiEntity.Count - 1
                Dim Entity As String = MathPiEntity(en)
                WordContent = WordContent.Replace("<entity aid: cstyle =""MathPi1"">" & Entity & "</entity>", "<entity aid:cstyle = ""STIX General_Symbol"">" & entityCharsNo(en) & "</entity>")
            Next
        End If

        WordContent = Regex.Replace(WordContent, "<entity[^>]*></entity>", "")
        WordContent = Regex.Replace(WordContent, "<entity[^>]*> </entity>", "")

        WordContent = Regex.Replace(WordContent, "<!--INDPRO(.*?)-->", "$1")
        WordContent = WordContent.Replace("<permissions><copyright-statement>&#x00A9; 2015 Informa UK, Ltd.</copyright-statement><copyright-year>2015</copyright-year><copyright-holder>Informa UK, Ltd.</copyright-holder></permissions>", "")
        WordContent = Regex.Replace(WordContent, "(<entity[^>]*>)(&#x[A-F0-9]{1,4};)(</entity>)", "$2")
        WordContent = Regex.Replace(WordContent, "(<entity[^>]*>)(&amp;lt;)(</entity>)", "&#x003C;")
        WordContent = Regex.Replace(WordContent, "(<entity[^>]*>)(&amp;gt;)(</entity>)", "&#x003E;")
        WordContent = Regex.Replace(WordContent, "&#x00A0;", " ")
        WordContent = Regex.Replace(WordContent, "(&#x[A-F0-9]{1,4};)", "$1")
        'WordContent = Regex.Replace(WordContent, "&#x2029;", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        'WordContent = Regex.Replace(WordContent, "(<emphasis[^>]*>)((&amp;|&)#x[A-F0-9]{1,4};)(</emphasis>)", "$2")
        WordContent = Regex.Replace(WordContent, "<\?oxy_delete[^><]+\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?oxy_insert_start[^><]+\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?oxy_insert_end[\s]*\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?oxy_comment_start[^><]+\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?oxy_comment_end[\s]*\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?oxy_options[^><]*\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WordContent = Regex.Replace(WordContent, "<\?indd_options[^><]*\?>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        '' Harihara Sudhan @ 18_Sep_2018 Modified the List-Item Changes ===========================''


        'WordContent = Regex.Replace(WordContent, "<listitem><para[^>]*>((?:(?!<\/para>).)*)</para>(&#x[A-F0-9]{4,4};)(\t)?</listitem>", "<listitem><para>$2 $1</para></listitem>", RegexOptions.Multiline Or RegexOptions.IgnoreCase)



        'WordContent = Regex.Replace(WordContent, "</para>(&#x[A-F0-9]{4,4};)(\t)?<itemizedlist([^>]*)?><listitem><para([^>]*)?>((?:(?!<\/para>).)*)?</para>", "</para><itemizedlist$3><listitem><para$4>$1 $5</para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        'WordContent = Regex.Replace(WordContent, "</para>(&#x[A-F0-9]{4,4};)(\t)?</listitem><listitem><para([^>]*)?>((?:(?!<\/para>).)*)?</para>", "</para></listitem><listitem><para$3>$1 $4</para>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WordContent = Regex.Replace(WordContent, "</para>(&#x[A-F0-9]{4,4};)(\t)?</listitem><listitem><para([^>]*)?>((?:(?!<\/para>).)*)?", AddressOf CleanInDesignBulletList, RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WordContent = Regex.Replace(WordContent, "</para>(&#x[A-F0-9]{4,4};)(\t)?<itemizedlist([^>]*)?><listitem><para([^>]*)?>((?:(?!<\/para>).)*)?", "</para><itemizedlist$3><listitem><para$4>$1 $5", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        ' WordContent = Regex.Replace(WordContent, "([0-9\.]+)?(\t)?</para>", "<listitem><para>$2 $1</para></listitem>", RegexOptions.Multiline Or RegexOptions.IgnoreCase)

        File.WriteAllText(TempExportFile, WordContent)

        WordContent = File.ReadAllText(TempExportFile)

        If ((DocType = DocumentType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            Try
                WordContent = AddHeadingLevelAttribute(WordContent)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                INDDGBL.InDesignDoc.close(InDesignConstant.SAVEOPTION_NO)
                Return False
            End Try
        End If

        Try
            IsOnPageFootnote = IsCheckNopageFootnote(WordContent)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If (DocType <> DocumentType.JOURNAL) Then
            Try
                WordContent = RetaingInDesignTableWithRowstartandEnd(WordContent)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        If (TempExportFile.Contains("_PRELIMS_")) Then
            Try
                WordContent = MoveChoFrameTitle(WordContent)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        Try
            WordContent = RemoveEnterMarkinFootnote(WordContent)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        WordContent = Regex.Replace(WordContent, "&#x2029;", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        Try
            WordContent = RemoveOxyElements(WordContent)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            WordContent = AddHyperLink(WordContent)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If ((DocType = LanstadClientType.TANDF) Or (DocType = LanstadClientType.TANDFUK)) Then
            WordContent = TandFExportCleanUp(WordContent)
        Else
            WordContent = BookFootnoteCleanUp(WordContent)
        End If

        If (String.IsNullOrEmpty(WordContent)) Then
            Return False
        End If

        File.WriteAllText(TempExportFile, WordContent)

        Try
            XMLDoc.LoadXml(File.ReadAllText(TempExportFile).Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & "XML file not Well Formed!", MessageType.MSGERROR)
            Return False
        End Try

        If (DocType = LanstadClientType.JOURNAL) Then
            XmlDomCleanup()
        End If

        If ((DocType = LanstadClientType.BOOK) Or (DocType = LanstadClientType.TANDF) Or (DocType = DocumentType.TANDFUK) Or (DocType = LanstadClientType.RANDL)) Then
            If (String.Compare(JournalName, "NL", True) <> 0) Then
                Try
                    'CleanUpListItemNumber()  '27-10-2021 (REVA)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            End If
        End If

        'Try
        '    TableCleanUp()
        'Catch ex As Exception
        '    Console.WriteLine(ex.Message)
        '    Console.ReadKey()
        'End Try

        Try
            ReGeneratePageNums()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            If (Not RemoveUntaggedTable()) Then
                Return False
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If ((String.Compare(JournalName, "BDS", True) <> 0) And (DocType <> LanstadClientType.JOURNAL)) Then

            Try
                If (Not RetaingInDesignTable()) Then
                    Return False
                End If
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        'Try
        '    RetaingInDesignTableWithRowstartandEnd()
        'Catch ex As Exception

        'End Try

        Try
            TableDataCleanUp()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            AttributionCleanUp()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Try
            XMLString = XMLDoc.OuterXml.Replace("&amp;", "&")
            'XMLString = Regex.Replace(XMLString, "<x>((?!<(\/)?x>).)*</x>", "")
            InDesignCleanUp()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        If (DocType = LanstadClientType.JOURNAL) Then
            Try
                NLMCleanUp()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
            If (String.Compare(JournalName, "dove", True) <> 0) Then
                Try
                    Dim JournalPermis As JournalMetaPermission = Nothing
                    Dim PermissionData As String = String.Empty
                    JournalPermis = (From jd In JournalPermissions Where (String.Compare(jd.JournalName, JournalName, False) = 0) Select jd).FirstOrDefault
                    If (JournalPermis IsNot Nothing) Then
                        PermissionData = JournalPermis.PermissionData
                        XMLString = Regex.Replace(XMLString, "<self-uri ", PermissionData & "<self-uri ")
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try
            End If
            Dim HistoryNode As String = Regex.Match(XMLString, "(<history[^>]*>((?!<(\/)?history>).)*<\/history>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Groups(1).Value
            If (Not String.IsNullOrEmpty(HistoryNode)) Then
                XMLString = XMLString.Replace(HistoryNode, "").Replace("<permissions>", HistoryNode & "<permissions>")
            End If
            'XMLString = Regex.Replace(XMLString, "(<self-uri[^>]*)(>)", "$1 />", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            'XMLString = XMLString.Replace("</self-uri>", "")

            Dim ErpLicenseContent As String = "<license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/""><p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc/4.0/88x31.png"" /> This work is licensed under a <ext-link ext-link-type=""uri"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/"">Creative Commons Attribution-NonCommercial 4.0 International License</ext-link>.</p></license>"
            Dim EmdLicenseContent As String = "<license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/""><p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png"" /> This work is licensed under a <ext-link ext-link-type=""uri"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/"">Creative Commons Attribution-NonCommercial 4.0 International License</ext-link>.</p></license>"
            If (String.Compare(JournalName, "edm", True) = 0) Then
                XMLString = XMLString.Replace("</permissions>", EmdLicenseContent & "</permissions>")
            ElseIf ((String.Compare(JournalName, "erp", True) = 0) Or (String.Compare(JournalName, "ecc", True) = 0) Or (String.Compare(JournalName, "ec", True) = 0)) Then
                XMLString = XMLString.Replace("</permissions>", ErpLicenseContent & "</permissions>")
            End If

            XMLString = RenumberID(XMLString)

        End If


        'If (File.Exists(ExportFileName)) Then
        '    File.Delete(ExportFileName)
        'End If

        If ((DocType = LanstadClientType.BOOK) Or (DocType = LanstadClientType.RANDL)) Then
            'If (Not XMLString.Contains("<tgroup><thead")) Then
            '    XMLString = XMLString.Replace("<thead>", "<tgroup cols=""0""><thead>")
            'End If
            'If ((Not XMLString.Contains("</tgroup><tblsource>")) Or (Not XMLString.Contains("</tgroup><tblfn>"))) Then
            '    XMLString = XMLString.Replace("</table>", "</tgroup></table>")
            'End If
            'XMLString = Regex.Replace(XMLString, " linkend=""(.*?)""", "")
            XMLString = Regex.Replace(XMLString, "<section[^>]*><title>Notes</title></section>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            XMLString = Regex.Replace(XMLString, "<section[^>]*><title>Note</title></section>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        ElseIf ((DocType = LanstadClientType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            XMLString = XMLString.Replace("<Cell></Cell>", "")
            XMLString = Regex.Replace(XMLString, "<section[^>]*><title>Notes</title></section>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            XMLString = Regex.Replace(XMLString, "<section[^>]*><title>Note</title></section>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        ElseIf (DocType = LanstadClientType.JOURNAL) Then
            XMLString = Regex.Replace(XMLString, "(<tgroup[^><]*>)", "<table frame=""hsides"" rules=""groups"">", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            XMLString = XMLString.Replace("</tgroup>", "</table>")
        End If

        XMLString = XMLString.Replace("&#x2028;", "")



        If (String.Compare(JournalName, "OIKO", True) = 0) Then
            Try
                XMLString = OikosCleanUp(XMLString)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        If (String.Compare(JournalName, "dove", True) = 0) Then
            Try
                XMLString = DoveCleanUp(XMLString)
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Return False
            End Try
        End If

        If (DocType = LanstadClientType.JOURNAL) Then
            If (Regex.IsMatch(XMLString, "<!--(<!DOCTYPE([^>]+)>)-->")) Then
                XMLString = Regex.Replace(XMLString, "<!--(<!DOCTYPE([^>]+)>)-->", "<!DOCTYPE article PUBLIC ""-//NLM//DTD Journal Publishing DTD v2.3 20070202//EN"" ""journalpublishing.dtd"">")
            Else
                XMLString = XMLString.Replace("<article ", "<!DOCTYPE article PUBLIC ""-//NLM//DTD Journal Publishing DTD v2.3 20070202//EN"" ""journalpublishing.dtd""><article ")
                XMLString = Regex.Replace(XMLString, "<article [^>]*>", "<article dtd-version=""2.3"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:mml=""http://www.w3.org/1998/Math/MathML"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" article-type=""research-article"">")
            End If
        Else
            XMLString = Regex.Replace(XMLString, "<book[^>](.*?)>", "<book xmlns=""http://docbook.org/ns/docbook"" xmlns:xlink=""http://www.w3.org/1999/xlink"" version=""5.0"" xml:id=""a-157"" xml:lang=""en"" role=""fullText"">")
        End If

        XMLString = XMLString.Replace("&amp;", "&")

        If (DocType = LanstadClientType.JOURNAL) Then
            XMLString = XMLString.Replace("><", ">" & vbCrLf & "<")
            While XMLString.Contains(vbLf & vbLf)
                XMLString = XMLString.Replace(vbLf & vbLf, vbLf)
            End While

            While XMLString.Contains(vbCrLf & vbCrLf)
                XMLString = XMLString.Replace(vbCrLf & vbCrLf, vbCrLf)
            End While
            XMLString = XMLString.Replace(">" & vbCrLf & vbCrLf & "</", ">" & vbCrLf & "</")
        End If

        Dim objWriter As New System.IO.StreamWriter(OutputFile)
        objWriter.Write(XMLString)
        objWriter.Close()
        GBL.DeantaBallon("Export completed", MessageType.MSGINFO)
        Return True
    End Function

    Private Function RenumberID(ByVal WdContent As String) As String
        WdContent = Regex.Replace(WdContent, "<aff[^>]*>", Function(mt As Match)
                                                               Dim Str As String = mt.Value
                                                               Str = Regex.Replace(Str, """([AF]+[0]+)([0-9]+)""", """aff$2""", RegexOptions.IgnoreCase)
                                                               Return Str
                                                           End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WdContent = Regex.Replace(WdContent, "<ref[^>]*>", Function(mt As Match)
                                                               Dim Str As String = mt.Value
                                                               Str = Regex.Replace(Str, """([CIT]+[0]+)([0-9]+)""", """bib$2""", RegexOptions.IgnoreCase)
                                                               Return Str
                                                           End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WdContent = Regex.Replace(WdContent, "<xref[^>]*>", Function(mt As Match)
                                                                Dim Str As String = mt.Value
                                                                Str = Regex.Replace(Str, """([AF]+[0]+)([0-9]+)""", """aff$2""", RegexOptions.IgnoreCase)
                                                                Str = Regex.Replace(Str, """([CIT]+[0]+)([0-9]+)""", """bib$2""", RegexOptions.IgnoreCase)
                                                                Str = Regex.Replace(Str, """([F])([1-9]+)""", """fig$2""", RegexOptions.IgnoreCase)
                                                                Str = Regex.Replace(Str, """([T])([1-9]+)""", """tbl$2""", RegexOptions.IgnoreCase)
                                                                Str = Regex.Replace(Str, """([C])([1-9]+)""", """cor$2""", RegexOptions.IgnoreCase)
                                                                Return Str
                                                            End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        WdContent = Regex.Replace(WdContent, "<corresp[^>]*>", Function(mt As Match)
                                                                   Dim Str As String = mt.Value
                                                                   Str = Regex.Replace(Str, """([C])([1-9]+)""", """cor$2""", RegexOptions.IgnoreCase)
                                                                   Return Str
                                                               End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WdContent = Regex.Replace(WdContent, "<fig[^>]*>", Function(mt As Match)
                                                               Dim Str As String = mt.Value
                                                               Str = Regex.Replace(Str, """([F])([1-9]+)""", """fig$2""", RegexOptions.IgnoreCase)
                                                               Return Str
                                                           End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        WdContent = Regex.Replace(WdContent, "<table-wrap[^>]*>", Function(mt As Match)
                                                                      Dim Str As String = mt.Value
                                                                      Str = Regex.Replace(Str, """([T])([1-9]+)""", """tbl$2""", RegexOptions.IgnoreCase)
                                                                      Return Str
                                                                  End Function, RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        Return WdContent
    End Function

    Private Function CleanInDesignBulletList(ByVal Mat As Match) As String
        Dim Content As String = Mat.Value
        Content = "</para></listitem><listitem><para" & Mat.Groups(3).Value & ">" & Mat.Groups(1).Value & " " & Mat.Groups(4).Value
        Return Content
    End Function

    Private Function RemoveEnterMarkinFootnote(ByVal XmlContent As String) As String
        Dim tmpLinkDoc As New XmlDocument
        tmpLinkDoc.PreserveWhitespace = True
        Try
            tmpLinkDoc.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return XmlContent
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim footnoteLst As XmlNodeList = tmpLinkDoc.SelectNodes("//footnote[@role='end-ch-note']")
        If ((footnoteLst IsNot Nothing) AndAlso (footnoteLst.Count > 0)) Then
            For f As Int16 = 0 To footnoteLst.Count - 1
                footnoteLst(f).InnerXml = footnoteLst(f).InnerXml.Replace("&amp;#x2029;", vbNewLine)
            Next
        End If
        Return tmpLinkDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Private Function IsCheckNopageFootnote(ByVal XmlContent As String) As Boolean
        Dim tmpLinkDoc As New XmlDocument
        tmpLinkDoc.PreserveWhitespace = True
        Try
            tmpLinkDoc.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return False
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Dim Footnotes As XmlNodeList = tmpLinkDoc.SelectNodes("//footnote[@role='end-ch-note']")
        If ((Footnotes IsNot Nothing) AndAlso (Footnotes.Count > 0)) Then
            Return True
        End If
        Return False
    End Function

    Private Function MoveChoFrameTitle(ByVal XmlContent As String) As String
        Dim tmpLinkDoc As New XmlDocument
        tmpLinkDoc.PreserveWhitespace = True
        Try
            tmpLinkDoc.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return XmlContent
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Dim ChoNode As XmlNode = tmpLinkDoc.SelectSingleNode("//CHO")
        Dim NextChoNode As XmlNode = Nothing
        If (ChoNode IsNot Nothing) Then
            If (ChoNode.NextSibling IsNot Nothing) Then
                NextChoNode = ChoNode.NextSibling
            End If
            If ((NextChoNode IsNot Nothing) AndAlso (NextChoNode.NodeType = XmlNodeType.Element)) Then
                NextChoNode.InnerXml = ChoNode.InnerXml & NextChoNode.InnerXml
                If (ChoNode.ParentNode IsNot Nothing) Then
                    ChoNode.ParentNode.RemoveChild(ChoNode)
                End If
            End If
        End If
        Return tmpLinkDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Private Function RemoveOxyElements(ByVal XmlContent As String) As String
        Dim tmpLinkDoc As New XmlDocument
        tmpLinkDoc.PreserveWhitespace = True
        Try
            tmpLinkDoc.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return XmlContent
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(tmpLinkDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        Dim OxyCommentList As XmlNodeList = tmpLinkDoc.SelectNodes("//processing-instruction('')")
        If ((OxyCommentList IsNot Nothing) AndAlso (OxyCommentList.Count > 0)) Then
            For ox As Integer = 0 To OxyCommentList.Count - 1
                Dim OxyNode As XmlProcessingInstruction = CType(OxyCommentList(ox), XmlProcessingInstruction)
                Try
                    If (OxyNode.ParentNode IsNot Nothing) Then
                        OxyNode.ParentNode.RemoveChild(OxyNode)
                    End If
                Catch ex As Exception
                End Try
            Next
        End If

        Dim BibliomixedList As XmlNodeList = tmpLinkDoc.SelectNodes("//bibliomixed[@role=""""]", NameSpaceManager)
        Dim titleNode As XmlNode = Nothing
        If ((BibliomixedList IsNot Nothing) AndAlso (BibliomixedList.Count > 0)) Then
            For i As Integer = 0 To BibliomixedList.Count - 1
                titleNode = BibliomixedList(i).SelectSingleNode("./title[@role='booktitle']")
                If (titleNode IsNot Nothing) Then
                    Try
                        BibliomixedList(i).Attributes("role").Value = "book"
                    Catch ex As Exception
                    End Try
                Else
                    titleNode = BibliomixedList(i).SelectSingleNode("./title[@role='chapter-title']")
                    If (titleNode IsNot Nothing) Then
                        Try
                            BibliomixedList(i).Attributes("role").Value = "book"
                        Catch ex As Exception
                        End Try
                    Else
                        titleNode = BibliomixedList(i).SelectSingleNode("./collab")
                        If (titleNode IsNot Nothing) Then
                            Try
                                BibliomixedList(i).Attributes("role").Value = "other"
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                End If
                titleNode = BibliomixedList(i).SelectSingleNode("./bibliomset[@relation='journal']")
                If (titleNode IsNot Nothing) Then
                    BibliomixedList(i).Attributes("role").Value = "journal"
                End If
                If (String.IsNullOrEmpty(BibliomixedList(i).Attributes("role").Value)) Then
                    titleNode = BibliomixedList(i).SelectSingleNode("./publishername")
                    If (titleNode IsNot Nothing) Then
                        BibliomixedList(i).Attributes("role").Value = "journal"
                    End If
                End If
            Next
        End If
        Return tmpLinkDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Private Function AddHyperLink(ByVal wordContent As String) As String
        Dim tmpLinkDoc As New XmlDocument
        tmpLinkDoc.PreserveWhitespace = True
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(tmpLinkDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")

        Try
            tmpLinkDoc.LoadXml(wordContent.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return wordContent
        End Try
        Dim TextList As XmlNodeList = tmpLinkDoc.SelectNodes("//text()", NameSpaceManager)
        If ((TextList Is Nothing) AndAlso (TextList.Count = 0)) Then Return False
        For Each linkNode As XmlNode In TextList
            Dim LnkMatch As Match = Regex.Match(linkNode.InnerText, "(\b(https?://|www\.)\S+(/)?)", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If (LnkMatch.Success) Then
                Try
                    Dim Lnk As String = LnkMatch.Value
                    Lnk = Lnk.TrimEnd(".")
                    If ((Not String.IsNullOrEmpty(Lnk)) AndAlso (linkNode.ParentNode IsNot Nothing) AndAlso (String.Compare(linkNode.ParentNode.Name, "uri", True) <> 0)) Then
                        linkNode.ParentNode.InnerXml = linkNode.ParentNode.InnerXml.Replace(linkNode.InnerText.Replace("&", "&amp;"), linkNode.InnerText.Replace("&", "&amp;").Replace(Lnk, String.Format("<uri>{0}</uri>", Lnk)))
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try
            End If
        Next


        Return tmpLinkDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Private Function DoveCleanUp(ByVal XMLString As String) As String
        Dim XmlDoveDoc As New XmlDocument
        Dim DoveContent As String = String.Empty
        XmlDoveDoc.PreserveWhitespace = True
        XMLString = XMLString.Replace("<pub-date pub-type=""ppub""><month />", "<pub-date pub-type=""collection"">")
        XMLString = XMLString.Replace("<pub-date pub-type=""epub""><month />", "<pub-date pub-type=""epub""><day></day><month></month>")
        Try
            XmlDoveDoc.LoadXml(XMLString.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Try
            MoveFloatInBackSection(XmlDoveDoc)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        DoveContent = XmlDoveDoc.OuterXml.Replace("&amp;", "&")
        DoveContent = DoveContent.Replace(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
        DoveContent = DoveContent.Replace(" dtd-version=""2.3""", " xml:lang=""en""")
        DoveContent = DoveContent.Replace("<front>", "<front><journal-meta><journal-id journal-id-type=""publisher-id""></journal-id><journal-title></journal-title><issn pub-type=""epub""></issn><publisher><publisher-name>DoveMedical Press</publisher-name></publisher></journal-meta>")
        DoveContent = DoveContent.Replace("<article-meta>", "<article-meta><article-id pub-id-type=""doi""></article-id><article-id pub-id-type=""publisher-id"">aabc-10-001</article-id>")

        Return DoveContent
    End Function

    Private Function OikosCleanUp(ByVal XMLString As String) As String
        Dim XmlDoveDoc As New XmlDocument
        Dim OikosDoiList As New Hashtable
        OikosDoiList.Add("OIKOS", "10.1111/j.1600-0706.2015.002556")
        Dim DoveContent As String = String.Empty
        XmlDoveDoc.PreserveWhitespace = True
        Try
            XmlDoveDoc.LoadXml(XMLString.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try

        Dim AbstractNode As XmlNode = XmlDoveDoc.SelectSingleNode("//abstract")
        If (AbstractNode IsNot Nothing) Then
            AbstractNode.InnerXml = Regex.Replace(AbstractNode.InnerXml, "<sec[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</sec>", "")
            AbstractNode.InnerXml = Regex.Replace(AbstractNode.InnerXml, "<title[^>]*>((?!<(\/)?title>).)*</title>", "")
        End If
        DoveContent = XmlDoveDoc.OuterXml.Replace("&amp;", "&")
        'change section heading.
        For Each SecNode As Match In Regex.Matches(DoveContent, "<sec[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Dim TmpText As String = SecNode.Value
            TmpText = TmpText.Replace("sec-type=""H", "id=""ss")
            DoveContent = DoveContent.Replace(SecNode.Value, TmpText)
        Next

        For Each FigNode As Match In Regex.Matches(DoveContent, "<fig[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Dim TmpText As String = FigNode.Value
            TmpText = TmpText.Replace("id=""fig", "id=""F")
            DoveContent = DoveContent.Replace(FigNode.Value, TmpText)
        Next

        For Each TblNode As Match In Regex.Matches(DoveContent, "<fig[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Dim TmpText As String = TblNode.Value
            TmpText = TmpText.Replace("id=""tbl", "id=""T")
            DoveContent = DoveContent.Replace(TblNode.Value, TmpText)
        Next

        For Each TblNode As Match In Regex.Matches(DoveContent, "<ref[^>]*>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Dim TmpText As String = TblNode.Value
            Dim BibValue As String = Regex.Match(TmpText, "id=""(.*?)""", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Groups(1).Value
            If (Not String.IsNullOrEmpty(BibValue)) Then
                BibValue = Regex.Replace(BibValue, "[^0-9]", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
                TmpText = TmpText.Replace("bib" & BibValue, "CCIT" & Convert.ToInt32(BibValue).ToString("0000"))
                TmpText = TmpText.Replace("id=""CCIT", "id=""CIT")
                DoveContent = DoveContent.Replace(TblNode.Value, TblNode.Value & String.Format("<label>{0}</label>", BibValue))
                DoveContent = DoveContent.Replace(TblNode.Value, TmpText)
            End If
        Next

        DoveContent = DoveContent.Replace(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
        DoveContent = DoveContent.Replace(" dtd-version=""2.3""", " dtd-version=""2.2""")
        DoveContent = DoveContent.Replace("<front>", "<front><journal-meta><journal-id journal-id-type=""CATS"">" & JournalName & "</journal-id><journal-id journal-id-type=""publisher-code""></journal-id><journal-title></journal-title><abbrev-journal-title abbrev-type=""pubmed""></abbrev-journal-title><issn pub-type=""ppub""></issn><issn pub-type=""epub""></issn><publisher><publisher-name></publisher-name><publisher-loc></publisher-loc></publisher></journal-meta><article-meta><article-id pub-id-type=""doi"">" & OikosDoiList(JournalName) & "</article-id><article-id pub-id-type=""publisher-id"">" & ArticleID & "</article-id><article-categories><subj-group subj-group-type=""heading""><subject></subject></subj-group></article-categories>")
        DoveContent = DoveContent.Replace("<author-notes></author-notes><permissions></permissions>", "<author-notes><corresp id=""c1""></corresp></author-notes><pub-date pub-type=""ppub""><month></month><year>" & DateTime.Now.Year & "</year></pub-date><pub-date pub-type=""epub""><month></month><year>" & DateTime.Now.Year & "</year></pub-date><volume>0</volume><issue>0</issue><fpage>1</fpage><lpage></lpage><history><date date-type=""received""><day></day><month></month><year></year></date><date date-type=""accepted""><day></day><month></month><year></year></date></history><permissions><copyright-statement>&copy; " & DateTime.Now.Year & " </copyright-statement><copyright-year>" & DateTime.Now.Year & "</copyright-year><copyright-holder></copyright-holder></permissions>")
        Return DoveContent.Replace("&amp;", "&")
    End Function

    Private Function MoveFloatInBackSection(ByVal TmpDoveDoc As XmlDocument) As Boolean
        Dim BackTag As XmlNode = TmpDoveDoc.SelectSingleNode("//back")
        If (BackTag Is Nothing) Then Return False
        Dim FloatList As XmlNodeList = TmpDoveDoc.SelectNodes("//fig|//table-wrap")
        If ((FloatList Is Nothing) OrElse (FloatList.Count = 0)) Then Return False
        Dim SecNode As XmlNode = TmpDoveDoc.CreateNode(XmlNodeType.Element, "sec", "")
        Dim Sectypeatt As XmlNode = TmpDoveDoc.CreateNode(XmlNodeType.Attribute, "sec-Type", "")
        Sectypeatt.Value = "display-objects"
        SecNode.Attributes.Append(Sectypeatt)
        BackTag.InsertBefore(SecNode, BackTag.FirstChild)
        For Each FltNode As XmlNode In FloatList
            SecNode.PrependChild(FltNode)
        Next
        Return True
    End Function

    Private Function ReGeneratePageNums() As Boolean
        Dim PageNumsList As XmlNodeList = XMLDoc.SelectNodes("//pagenums")
        Dim SplitChar As String = "&amp;#x2013;"
        Dim FirstPage As String = String.Empty
        Dim Diff As String = String.Empty
        Dim SecondPage As String = String.Empty
        If ((PageNumsList Is Nothing) OrElse (PageNumsList.Count = 0)) Then
            Return False
        End If
        For Each PageNode As XmlNode In PageNumsList
            If (Not PageNode.InnerXml.Contains(SplitChar)) Then Continue For
            Try
                FirstPage = PageNode.InnerXml.Split(New String() {SplitChar}, StringSplitOptions.RemoveEmptyEntries)(0)
                SecondPage = PageNode.InnerXml.Split(New String() {SplitChar}, StringSplitOptions.RemoveEmptyEntries)(1)
                If ((FirstPage.Length <> SecondPage.Length) And (FirstPage.Length > SecondPage.Length)) Then
                    Diff = FirstPage.Substring(0, (FirstPage.Length - SecondPage.Length))
                    PageNode.InnerXml = PageNode.InnerXml.Replace(SplitChar & SecondPage, SplitChar & Diff & SecondPage)
                End If
            Catch ex As Exception
                Continue For
            End Try
        Next
        Return True
    End Function

    Private Function RemoveUntaggedTable() As Boolean
        Dim TableList As XmlNodeList = XMLDoc.SelectNodes("//table[@role]")
        If ((TableList IsNot Nothing) AndAlso (TableList.Count > 0)) Then
            For t As Integer = 0 To TableList.Count - 1
                Try
                    TableList(t).InnerXml = Regex.Replace(TableList(t).InnerXml, "<table[^>]*>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Replace("</table>", "")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Return False
                End Try
            Next
        End If
        Return True
    End Function



    Private Function RetainingMathContent(ByVal XmlContent As String) As String
        Dim Ce As New ConvertEquation
        Dim MathDoc As New XmlDocument
        Dim iMathCnt As Integer = 0
        Dim IInlineCnt As Integer = 0
        MathDoc.PreserveWhitespace = True
        MathDoc.XmlResolver = Nothing
        Try
            MathDoc.LoadXml(XmlContent.Replace("&", "&amp;"))
        Catch ex As Exception
            Return XmlContent
        End Try

        Dim FootEquLst As XmlNodeList = MathDoc.SelectNodes("//footnote/inlineequation|//footnote/equation")
        Dim HrefText As String = String.Empty
        If ((FootEquLst IsNot Nothing) AndAlso (FootEquLst.Count > 0)) Then
            For t As Int16 = 0 To FootEquLst.Count - 1
                HrefText = String.Empty
                Try
                    HrefText = FootEquLst(t).Attributes("href").Value
                    FootEquLst(t).Attributes.Remove(FootEquLst(t).Attributes("href"))
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                Try
                    If (Not String.IsNullOrEmpty(HrefText)) Then
                        If (String.IsNullOrEmpty(FootEquLst(t).InnerXml)) Then
                            FootEquLst(t).InnerXml = "<imagedata fileref=""images/" & HrefText & """ format=""image/jpeg""></imagedata>"
                        Else
                            FootEquLst(t).InnerXml = "<imagedata fileref=""images/" & HrefText & """ format=""image/jpeg""></imagedata>" & FootEquLst(t).InnerXml.Replace(vbTab, "").Replace(vbTab, "").Replace(ChrW(8233), "")
                        End If

                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
            Next
        End If

        Dim EqautionNodes As XmlNodeList = MathDoc.SelectNodes("//equation|//inlineequation")
        If ((EqautionNodes IsNot Nothing) AndAlso (EqautionNodes.Count > 0)) Then
            Dim ImageDataNode As XmlNode = Nothing
            Dim EpsPath As String = String.Empty
            Dim EpsContent As String = String.Empty
            Dim MathID As String = String.Empty
            Dim MathMLContent As String = String.Empty
            For Each EqnNode As XmlNode In EqautionNodes
                MathMLContent = String.Empty
                ImageDataNode = GetInnerChildNode(EqnNode, "imagedata")
                EpsPath = String.Empty
                If (ImageDataNode Is Nothing) Then Continue For
                If (String.Compare(EqnNode.Name, "equation", True) = 0) Then
                    iMathCnt = iMathCnt + 1
                    MathID = "math" & iMathCnt
                ElseIf (String.Compare(EqnNode.Name, "inlineequation", True) = 0) Then
                    IInlineCnt = IInlineCnt + 1
                    MathID = "inline-math" & IInlineCnt
                End If
                Try
                    EpsPath = ImageDataNode.Attributes("fileref").Value
                Catch ex As Exception
                    Continue For
                End Try
                EpsPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TempExportFile))), "Application"), EpsPath).Replace("/", "\")
                If (Not File.Exists(EpsPath)) Then
                    GBL.DeantaBallon("Could not able to find the equation files.", MessageType.MSGERROR)
                    Continue For
                End If
                Try
                    Ce.Convert(New EquationInputFileEPS(EpsPath), New EquationOutputFileText(EpsPath.Replace(".eps", ".txt"), "MathML2 (m namespace).tdl"), String.Empty)
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                End Try
                If File.Exists(EpsPath.Replace(".eps", ".txt")) Then
                    MathMLContent = File.ReadAllText(EpsPath.Replace(".eps", ".txt"))
                    MathMLContent = MathMLClean(MathMLContent)
                    If (Not String.IsNullOrEmpty(MathMLContent)) Then
                        EpsContent = MathMLContent.Replace("m:", "mml:")
                    Else
                        GBL.DeantaBallon("Could not able to parse the MathML contents from the MathFile: " & Path.GetFileName(EpsPath), MessageType.MSGERROR)
                        Return False
                    End If
                Else
                    EpsContent = GetMathContentFromEPS(EpsPath)
                End If
                If (String.Compare(EqnNode.Name, "equation", True) = 0) Then
                    EpsContent = Regex.Replace(EpsContent, "(<mml:math[^>]*>)", "<mml:math display=""block"">")
                ElseIf (String.Compare(EqnNode.Name, "inlineequation", True) = 0) Then
                    EpsContent = Regex.Replace(EpsContent, "(<mml:math[^>]*>)", "<mml:math display=""inline"">")
                End If
                Try
                    If (Not String.IsNullOrEmpty(EpsContent)) Then
                        Dim mathidatt As XmlAttribute = MathDoc.CreateAttribute("id", "")
                        mathidatt.Value = MathID
                        EqnNode.InnerXml = "<alternatives>" & EpsContent & "</alternatives>" & EqnNode.InnerXml
                        EqnNode.Attributes.Append(mathidatt)
                    Else
                        GBL.DeantaBallon("Error while extract mathml content from EPS...", MessageType.MSGINFO)
                        Continue For
                    End If
                    If File.Exists(EpsPath.Replace(".eps", ".txt")) Then
                        File.Delete(EpsPath.Replace(".eps", ".txt"))
                    End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If

        Dim CommentLst As XmlNodeList = MathDoc.SelectNodes("//comment")
        If ((CommentLst IsNot Nothing) AndAlso (CommentLst.Count > 0)) Then
            For ct As Integer = 0 To CommentLst.Count - 1
                If (CommentLst(ct).ParentNode IsNot Nothing) Then
                    CommentLst(ct).ParentNode.RemoveChild(CommentLst(ct))
                End If
            Next
        End If

        'Dim RemoveAttributelst As XmlNodeList = MathDoc.SelectNodes("//@title|//@data-username|//@data-time|//@store|//@enter_key")
        Dim RemoveAttributelst As XmlNodeList = MathDoc.SelectNodes("//*")
        If ((RemoveAttributelst IsNot Nothing) AndAlso (RemoveAttributelst.Count > 0)) Then
            For ta As Integer = 0 To RemoveAttributelst.Count - 1
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("title")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("data-username")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("data-time")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("store")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("enter_key")
                Catch ex As Exception
                End Try
            Next
        End If
        Dim TypeObj As XmlAttribute = Nothing
        Dim RefList As XmlNodeList = MathDoc.SelectNodes("//mixed-citation[@publication-type]")
        If ((RefList IsNot Nothing) AndAlso (RefList.Count > 0)) Then
            For tr As Integer = 0 To RefList.Count - 1
                Try
                    TypeObj = RefList(tr).Attributes("publication-type")
                Catch ex As Exception
                End Try
                If (TypeObj IsNot Nothing) Then
                    TypeObj.Value = "journal"
                End If
            Next
        End If

        Dim VolumeList As XmlNodeList = MathDoc.SelectNodes("//volume")
        If ((VolumeList IsNot Nothing) AndAlso (VolumeList.Count > 0)) Then
            For v As Integer = 0 To VolumeList.Count - 1
                VolumeList(v).InnerXml = VolumeList(v).InnerXml.Replace("<x>", "").Replace("</x>", "")
            Next
        End If

        Return MathDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Public Function MathMLClean(ByVal MathXMLContent As String) As String
        While MathXMLContent.Contains(vbCr)
            MathXMLContent = MathXMLContent.Replace(vbCr, " ")
        End While

        While MathXMLContent.Contains(vbLf)
            MathXMLContent = MathXMLContent.Replace(vbLf, " ")
        End While

        While MathXMLContent.Contains(vbCrLf)
            MathXMLContent = MathXMLContent.Replace(vbCrLf, " ")
        End While

        While MathXMLContent.Contains(vbNewLine)
            MathXMLContent = MathXMLContent.Replace(vbNewLine, " ")
        End While

        While MathXMLContent.Contains("  ")
            MathXMLContent = MathXMLContent.Replace("  ", " ")
        End While
        MathXMLContent = MathXMLContent.Replace("<m:semantics>", "").Replace("</m:semantics>", "")
        MathXMLContent = Regex.Replace(MathXMLContent, "<m:annotation[^>]*>(((?!<\/m:annotation>).)*)<\/m:annotation>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        MathXMLContent = Regex.Replace(MathXMLContent, "<!--(.*?)-->", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return MathXMLContent
    End Function

    Private Function old_RetainingMathContent(ByVal XmlContent As String) As String
        Dim MathDoc As New XmlDocument
        Dim iMathCnt As Integer = 0
        Dim IInlineCnt As Integer = 0
        MathDoc.PreserveWhitespace = True
        MathDoc.XmlResolver = Nothing
        Try
            MathDoc.LoadXml(XmlContent)
        Catch ex As Exception
            Return XmlContent
        End Try
        Dim EqautionNodes As XmlNodeList = MathDoc.SelectNodes("//equation|//inlineequation")
        If ((EqautionNodes IsNot Nothing) AndAlso (EqautionNodes.Count > 0)) Then
            Dim ImageDataNode As XmlNode = Nothing
            Dim EpsPath As String = String.Empty
            Dim EpsContent As String = String.Empty
            Dim MathID As String = String.Empty
            For Each EqnNode As XmlNode In EqautionNodes
                ImageDataNode = GetInnerChildNode(EqnNode, "imagedata")
                EpsPath = String.Empty
                If (ImageDataNode Is Nothing) Then Continue For
                If (String.Compare(EqnNode.Name, "equation", True) = 0) Then
                    iMathCnt = iMathCnt + 1
                    MathID = "math" & iMathCnt
                ElseIf (String.Compare(EqnNode.Name, "inlineequation", True) = 0) Then
                    IInlineCnt = IInlineCnt + 1
                    MathID = "inline-math" & IInlineCnt
                End If
                Try
                    EpsPath = ImageDataNode.Attributes("fileref").Value
                Catch ex As Exception
                    Continue For
                End Try
                EpsPath = Path.Combine(Path.GetDirectoryName(ExportFileName), EpsPath).Replace("/", "\")
                EpsContent = GetMathContentFromEPS(EpsPath)
                If (String.Compare(EqnNode.Name, "equation", True) = 0) Then
                    EpsContent = Regex.Replace(EpsContent, "(<mml:math[^>]*)(>)", "$1 display=""block""$2")
                ElseIf (String.Compare(EqnNode.Name, "inlineequation", True) = 0) Then
                    EpsContent = Regex.Replace(EpsContent, "(<mml:math[^>]*)(>)", "$1 display=""inline""$2")
                End If
                If (Not String.IsNullOrEmpty(EpsContent)) Then
                    Dim mathidatt As XmlAttribute = MathDoc.CreateAttribute("id", "")
                    mathidatt.Value = MathID
                    EqnNode.InnerXml = "<alternatives>" & EpsContent & "</alternatives>" & EqnNode.InnerXml
                    EqnNode.Attributes.Append(mathidatt)
                Else
                    GBL.DeantaBallon("Error while extract mathml content from EPS...", MessageType.MSGINFO)
                End If
            Next
        End If

        Dim CommentLst As XmlNodeList = MathDoc.SelectNodes("//comment")
        If ((CommentLst IsNot Nothing) AndAlso (CommentLst.Count > 0)) Then
            For ct As Integer = 0 To CommentLst.Count - 1
                If (CommentLst(ct).ParentNode IsNot Nothing) Then
                    CommentLst(ct).ParentNode.RemoveChild(CommentLst(ct))
                End If
            Next
        End If

        'Dim RemoveAttributelst As XmlNodeList = MathDoc.SelectNodes("//@title|//@data-username|//@data-time|//@store|//@enter_key")
        Dim RemoveAttributelst As XmlNodeList = MathDoc.SelectNodes("//*")
        If ((RemoveAttributelst IsNot Nothing) AndAlso (RemoveAttributelst.Count > 0)) Then
            For ta As Integer = 0 To RemoveAttributelst.Count - 1
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("title")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("data-username")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("data-time")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("store")
                Catch ex As Exception
                End Try
                Try
                    CType(RemoveAttributelst(ta), XmlElement).RemoveAttribute("enter_key")
                Catch ex As Exception
                End Try
            Next
        End If
        Dim TypeObj As XmlAttribute = Nothing
        Dim RefList As XmlNodeList = MathDoc.SelectNodes("//mixed-citation[@publication-type]")
        If ((RefList IsNot Nothing) AndAlso (RefList.Count > 0)) Then
            For tr As Integer = 0 To RefList.Count - 1
                Try
                    TypeObj = RefList(tr).Attributes("publication-type")
                Catch ex As Exception
                End Try
                If (TypeObj IsNot Nothing) Then
                    TypeObj.Value = "journal"
                End If
            Next
        End If
        Return MathDoc.OuterXml.Replace("&amp;", "&")
    End Function

    Private Function GetMathContentFromEPS(ByVal MathPath As String) As String
        If (Not File.Exists(MathPath)) Then
            Return String.Empty
        End If
        Dim content As String = Regex.Match(File.ReadAllText(MathPath).Replace(vbCrLf & "%", ""), "(<math[^><]+>(((?!</math>).)+)</math>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline).Value
        content = content.Replace(vbCr & "%", "").Replace(vbLf & "%", "").Replace(vbCrLf & "%", "").Replace("'", """").Replace("<moveraccent", "<mover accent").Replace("<mostretchy", "<mo stretchy").Replace(" display=""block""xmlns=", " display=""block"" xmlns=").Replace("<mstyledisplaystyle", "<mstyle displaystyle").Replace("<mostret ", "<mo stret").Replace("<mstyl edisplaystyle", "<mstyle displaystyle").Replace("<mathdisplay", "<math display").Replace("<mathxmlns", "<math xmlns")
        content = content.Replace("""block""xmlns", """block"" xmlns").Replace("'block'xmlns", "'block' xmlns").Replace("<mtdcolumnalign", "<mtd columnalign").Replace("<mtrcolumnalign", "<mtr columnalign").Replace("<mfracbevelled", "<mfrac bevelled").Replace("<mstylemathvariant", "<mstyle mathvariant").Replace("""displaystyle", """ displaystyle").Replace("<mstylemathsize", "<mstyle mathsize").Replace("""bold""mathsize", """bold"" mathsize").Replace("<mtablecolumnalign", "<mtable columnalign").Replace("<mstylescriptlevel", "<mstyle scriptlevel")
        content = content.Replace("<", "<mml:").Replace("<mml:/", "</mml:").Replace("<mml:mimathvariant", "<mml:mi mathvariant").Replace("<mml:mtablegroupalign", "<mml:mtable groupalign").Replace("<mml:munderaccentunder", "<mml:munder accentunder").Replace("<mml:menclosenotation=", "<mml:menclose notation=")
        content = content.Replace(" display=""block""", "").Replace("<mml:mfencedclose", "<mml:mfenced close").Replace("close=""}""open=""{""", "close=""}"" open=""{""").Replace("close=""&#x232A;""open=", "close=""&#x232A;"" open=").Replace("close=""|""open=""", "close=""|"" open=""")
        content = content.Replace("close=""]""open", "close=""]"" open").Replace("mtableequalrows=""true""equalcolumns=""true""", "mtable equalrows=""true"" equalcolumns=""true""").Replace("equalrows=""true""equalcolumns=""true""", "equalrows=""true"" equalcolumns=""true""").Replace("columnalign=""left""equalrows=""true""", "columnalign=""left"" equalrows=""true""")
        content = content.Replace("mfenced close=""""open=""(""", "mfenced close="""" open=""(""").Replace("close=""&#x2016;""open=""&#x2016;""", "close=""&#x2016;"" open=""&#x2016;""").Replace("close=""""open=""{""", "close="""" open=""{""").Replace("close="")""open=""""", "close="")"" open=""""")
        content = content.Replace("close=""}""open=""""", "close=""}"" open=""""").Replace("close=""|""open=""""", "close=""|"" open=""""").Replace("close=""""open=""|""", "close="""" open=""|""").Replace("close=""&#x230B;""open=""&#x230A;""", "close=""&#x230B;"" open=""&#x230A;""").Replace("close=""""open=""[""", "close="""" open=""[""")
        content = content.Replace("close=""&#x2016;""open=""""", "close=""&#x2016;"" open=""""").Replace("close=""&#x301B;""open=""&#x301A;""", "close=""&#x301B;"" open=""&#x301A;""").Replace("close=""[""open=""[""", "close=""["" open=""[""")
        content = content.Replace("""right""equalrows=""true""", """right"" equalrows=""true""").Replace("close=""[""open=""]""", "close=""["" open=""]""")
        content = content.Replace("close=""&#x2309;""open=""&#x2308;""", "close=""&#x2309;"" open=""&#x2308;""").Replace("close=""""open=""", "close="""" open=""")
        'Try
        '    content = RemoveDuplicateMathElement(content)
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        'End Try
        Return content
    End Function

    Private Function RemoveDuplicateMathElement(ByVal MathContent As String) As String
        Dim DuplicateList As New List(Of String)
        'DuplicateList.AddRange(New String() {"mml:math", "mml:mfencedclose", "mml:mi", "mml:mo", "mml:mrow", "mml:msub", "mml:mtext", "mml:frac", "mml:mstyle"})
        DuplicateList.AddRange(New String() {"mml:mi", "mml:mo"})
        For d As Int16 = 0 To DuplicateList.Count - 1
            MathContent = Regex.Replace(MathContent, String.Format("(<{0}[^>]*>)(<{0}[^>]*>)", DuplicateList(d)), "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            MathContent = Regex.Replace(MathContent, String.Format("(</{0}>)(</{0}>)", DuplicateList(d)), "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Next
        Return MathContent
    End Function

    Private Function RefComentPro(m As Match)
        Dim sResult As String = m.Value.ToString
        sResult = Regex.Replace(sResult, "<!-- page[^>]+page:  [^ ]+""", "<!--", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sResult = Regex.Replace(sResult, "<!-- ", "<!--", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sResult
    End Function

    Private Function CleanUpListItemNumber() As Boolean
        Dim ListItemNodes As XmlNodeList = XMLDoc.SelectNodes("//listitem")
        If ((ListItemNodes Is Nothing) OrElse (ListItemNodes.Count = 0)) Then Return False
        For Each paraNode As XmlNode In ListItemNodes
            paraNode.InnerXml = Regex.Replace(paraNode.InnerXml, "(<para[^>]*>)([0-9.]+)(\s)", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Next
        Return False
    End Function


    Private Sub TableDataCleanUp()
        Dim TblHeads As XmlNodeList = XMLDoc.SelectNodes("//thead")
        If ((TblHeads Is Nothing) OrElse (TblHeads.Count = 0)) Then Exit Sub
        For Each THead As XmlNode In TblHeads
            Try
                THead.InnerXml = THead.InnerXml.Replace("td", "th")
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Continue For
            End Try
        Next
    End Sub

    Private Function BookFootnoteCleanUp(WordContent As String) As String
        Dim XmlTepDoc As New XmlDocument
        XmlTepDoc.PreserveWhitespace = True
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XmlTepDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        Dim TmpString = String.Empty
        Try
            XmlTepDoc.LoadXml(WordContent.Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return WordContent
        End Try
        Dim Footnotes As XmlNodeList = XmlTepDoc.SelectNodes("//footnote[@xml:id][@role=""end-bk-note""]", NameSpaceManager)
        For f As Int16 = 0 To Footnotes.Count - 1
            Dim Label As String = CType(Footnotes(f), XmlElement).GetAttribute("label")
            If (String.IsNullOrEmpty(Footnotes(f).InnerText)) Then
                GBL.DeantaBallon($"Footnote has no child node {Footnotes(f).OuterXml}", MessageType.MSGERROR)
                Continue For
            End If
            If (Footnotes(f).FirstChild Is Nothing) Then
                GBL.DeantaBallon($"Footnote has no child node {Footnotes(f).OuterXml}", MessageType.MSGERROR)
                Continue For
            End If
            Dim DispLabel As String = Regex.Match(Footnotes(f).FirstChild.InnerText, "(^[\(\)0-9]+)[\.\t\s]+", RegexOptions.Singleline Or RegexOptions.IgnoreCase).Value
            If String.IsNullOrEmpty(DispLabel) Then
                GBL.DeantaBallon($"Could not able to find the display label in the footnote. {Footnotes(f).OuterXml}", MessageType.MSGERROR)
                Continue For
            End If
            DispLabel = Regex.Replace(DispLabel, "[^0-9]+", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Dim XMLID As String = CType(Footnotes(f), XmlElement).GetAttribute("xml:id")
            If (String.Compare(Label, DispLabel, True) <> 0) Then
                GBL.DeantaBallon($"footnote label and display text not matched. footnote : {Label} - display text: {DispLabel}", MessageType.MSGERROR)
                Label = DispLabel
            End If
            Dim RefNode As XmlNode = GetFootnoteWithDispLabelNode(XmlTepDoc, DispLabel)
            If (ExportFileName.ToLower().Contains("notes")) Then
                Exit For
            End If
            If (RefNode Is Nothing) Then
                GBL.DeantaBallon("Could not able to find the footnote label: " & Label, MessageType.MSGERROR)
                Continue For
            End If
            Try
                RefNode.Attributes("label").Value = Label
            Catch ex As Exception
            End Try
            'RefNode.InnerXml = Regex.Replace(Footnotes(f).InnerXml, "(>[\s0-9.\s]+)", ">")
            'RefNode.InnerXml = Regex.Replace(Footnotes(f).InnerXml, "(<para[^>]*>)([\s0-9.\s]+)", "$1")
            RefNode.InnerXml = Regex.Replace(Footnotes(f).InnerXml, "(<para[^>]*>)([\s\t]*" & Label & "[\.\s\t]*)", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
            Footnotes(f).ParentNode.RemoveChild(Footnotes(f))
        Next
        TmpString = XmlTepDoc.OuterXml.Replace("&amp;", "&")
        Return TmpString
    End Function

    Private Function AttributionCleanUp()
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XMLDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        Dim Attribution As XmlNodeList = XMLDoc.SelectNodes("//chapter/epigraph/attribution", NameSpaceManager)
        If (Attribution IsNot Nothing) AndAlso (Attribution.Count > 0) Then
            For Each Ele As XmlElement In Attribution
                Ele.ParentNode.PrependChild(Ele)
            Next
        End If

        Dim NewInsertedRefFromVXE As XmlNodeList = XMLDoc.SelectNodes("//ref[@role]")
        Dim RoleAttrib As XmlAttribute = Nothing
        Dim Newid As String = String.Empty
        If ((NewInsertedRefFromVXE IsNot Nothing) AndAlso (NewInsertedRefFromVXE.Count > 0)) Then
            For i As Integer = 0 To NewInsertedRefFromVXE.Count - 1
                Try
                    RoleAttrib = NewInsertedRefFromVXE(i).Attributes("role")
                    If (NewInsertedRefFromVXE(i).HasChildNodes) AndAlso (String.Compare(NewInsertedRefFromVXE(i).ChildNodes(1).Name, "mixed-citation", True) = 0) Then
                        Dim NewAttrib As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "publication-type", "")
                        NewAttrib.Value = RoleAttrib.Value
                        NewInsertedRefFromVXE(i).ChildNodes(1).Attributes.Append(NewAttrib)
                    End If
                    NewInsertedRefFromVXE(i).Attributes.RemoveNamedItem("role")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
                Try
                    Newid = NewInsertedRefFromVXE(i).Attributes("id").Value
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
                If (Not String.IsNullOrEmpty(Newid) AndAlso (Newid.Contains("CIT"))) Then
                    NewInsertedRefFromVXE(i).Attributes("id").Value = NewInsertedRefFromVXE(i).Attributes("id").Value.Replace("CIT", "bib")
                End If
            Next
        End If


        Return True
    End Function

    Public Function HtmlEncode(text As String) As String
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

    Public Sub NLMCleanUp()
        'XMLString = Regex.Replace(XMLString, "<!--(<!DOCTYPE([^>]+)>)-->", "<!DOCTYPE article PUBLIC ""-//NLM//DTD Journal Publishing DTD v2.3 20070202//EN"" ""journalpublishing.dtd"">")

        Dim itemChars As New List(Of String)(New String() {"2AA1", "210F", "2276", "2265", "2266", "227E", "018E", "2272", "2273", "227F", "2276", "2034", "2044", "22DE", "002B", "2A2F", "00F7", "033F", "2AB0", "2213", "1424", "0326", "FB21", "2200", "2A7D", "1D214", "2A7E", "02D9", "2AA2", "1D6BF", "25B3", "1D6BD", "1D6AA", "2261", "1D6B7", "1D6B9", "1D6B8", "2211", "03B8", "2126", "1D687", "1D6F6", "00D8", "1420", "2267", "2277", "221E", "03B1", "03B2", "1D6BF", "03B4", "03F5", "03B6", "03B3", "03B7", "1D704", "1D6CF", "03BA", "1D6EC", "03BC", "03BD", "03BF", "03C0", "1D717", "03C1", "03C2", "03C4", "03B8", "03C9", "03C6", "1D61", "03C5", "1D701", "2212", "2577", "2014", "221D", "2270", "2271", "03C2", "03B5", "1D715", "2D50", "226E", "226F", "2271", "019B", "03C7", "2270", "2260", "2271", "226E", "226F", "2270", "2A7D", "2A7E", "03F0"})

        For Each itemChar In itemChars
            XMLString = Regex.Replace(XMLString, "( )(\u" & itemChar & ")( )", "$2", RegexOptions.IgnoreCase)
        Next

        XMLString = Regex.Replace(XMLString, "&amp;lt;tp&amp;gt;", "")
        XMLString = Regex.Replace(XMLString, "&amp;lt;/tp&amp;gt;", "<break/>")

        XMLString = XMLString.Replace(" </tp><tp>", "")
        XMLString = XMLString.Replace("<tp>", "")
        XMLString = XMLString.Replace("</tp>", "<break/>")
        XMLString = XMLString.Replace("<break/></td>", "</td>")


        XMLString = XMLString.Replace(ChrW(8211), "&#x2013;")
        XMLString = XMLString.Replace(ChrW(8216), "&#x2018;")
        XMLString = XMLString.Replace(ChrW(8217), "&#x2019;")
        XMLString = XMLString.Replace(ChrW(8194), "&#x2002;")
        XMLString = XMLString.Replace("&#x2028;", "")

        XMLString = XMLString.Replace("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>", "<?xml version=""1.0"" encoding=""UTF-8""?>")

        'XMLString = Regex.Replace(XMLString, "<article-id pub-id-type=""doi"">([^>])+</article-id>", "<article-id pub-id-type=""doi"">10.1530/" & IndesignName.ToUpper & "</article-id>")
        'XMLString = XMLString.Replace("<article-id pub-id-type=""publisher-id"">2222222</article-id>", "<article-id pub-id-type=""publisher-id"">" & IndesignName.ToUpper.Replace("-", "") & "</article-id>")

        XMLString = Regex.Replace(XMLString, "<article-id pub-id-type=""doi"">([^>])+</article-id>", "<article-id pub-id-type=""doi"">10.1530/" & String.Format("{0}-{1}", JournalName, ArticleID) & "</article-id>")
        XMLString = XMLString.Replace("<article-id pub-id-type=""publisher-id"">2222222</article-id>", "<article-id pub-id-type=""publisher-id"">" & IndesignName.ToUpper.Replace("-", "").Replace("_", "") & "</article-id>")

        XMLString = XMLString.Replace("</xref>,<xref ref-type=""aff""", "</xref><xref ref-type=""aff""")
        XMLString = XMLString.Replace("</xref>, <xref ref-type=""aff""", "</xref><xref ref-type=""aff""")
        XMLString = XMLString.Replace("</xref><sup>,</sup>", ",</xref>")

        XMLString = XMLString.Replace("<alt-title alt-title-type=""rrh"">", "<alt-title alt-title-type=""short"">")

        XMLString = XMLString.Replace("<abstract><title>Abstract</title>", "<abstract>")
        XMLString = XMLString.Replace("<p><italic>Background</italic>: ", "<title>Background</title><p>")
        XMLString = XMLString.Replace("<p><italic>Objective</italic>: ", "<title>Objective</title><p>")
        XMLString = XMLString.Replace("<p><italic>Design</italic>: ", "<title>Design</title><p>")
        XMLString = XMLString.Replace("<p><italic>Methods</italic>: ", "<title>Methods</title><p>")
        XMLString = XMLString.Replace("<p><italic>Results</italic>: ", "<title>Results</title><p>")

        XMLString = XMLString.Replace(" pos=""rowstart""", "")
        'XMLString = XMLString.Replace("valign=""bottom""", "align=""top""")

        XMLString = Regex.Replace(XMLString, "<tr><td[^><]*>", AddressOf TableTdSpan, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<graphic[^>]*>", AddressOf GraphicReplacement, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = XMLString.Replace("></graphic>", " />")

        XMLString = XMLString.Replace("<italic>.</italic>", ".")
        XMLString = XMLString.Replace("<italic>,</italic>", ",")
        XMLString = XMLString.Replace("<bold>.</bold>", ".")
        XMLString = XMLString.Replace("<sup></sup>", "")
        XMLString = XMLString.Replace("<title>Key Words</title>", "<title>Keywords</title>")

        Dim AbstractContent As String = Regex.Match(XMLString, "(<abstract[^>]*>.*?<\/abstract>)").Value
        If ((Not String.IsNullOrEmpty(AbstractContent)) AndAlso (AbstractContent.Contains("<title>"))) Then
            XMLString = XMLString.Replace("<abstract>", "<abstract><sec>")
            XMLString = XMLString.Replace("</abstract>", "</sec></abstract>")
        End If

        XMLString = XMLString.Replace("<table frame=""box"">", "<table frame=""hsides"" rules=""groups"">")
        XMLString = XMLString.Replace("xmlns:xlink=""xlink:"" ", "")

        For Each LalMath As Match In Regex.Matches(XMLString, "(<label>((?!<(\/)?label>).)*<\/label>)")
            Dim tmp As String = LalMath.Value.Replace("&#x2003;", "")
            XMLString = XMLString.Replace(LalMath.Value, tmp)
        Next

        XMLString = XMLString.Replace("</p><title>Design", "</p></sec><sec><title>Design")
        XMLString = XMLString.Replace("</p><title>Methods", "</p></sec><sec><title>Methods")
        XMLString = XMLString.Replace("</p><title>Results", "</p></sec><sec><title>Results")

        XMLString = Regex.Replace(XMLString, "(<pub-id>)\((.*?)\)(</pub-id>)", "(<ext-link ext-link-type=""uri"" xlink:href=""http://dx.doi.org/doi:10.1097/$2"">$2</ext-link>)")

        XMLString = Regex.Replace(XMLString, "<mixed-citation(\s)+publication-type([^>]+)>", "<citation citation-type$2>")
        XMLString = XMLString.Replace("</mixed-citation>", "</citation>")
        XMLString = XMLString.Replace("dtd-version=""3.0""", "dtd-version=""2.3""")

        'XMLString = Regex.Replace(XMLString, " (sec-type=""H[0-9]+"")", "")
        XMLString = XMLString.Replace("http://dx.doi.org/doi:10.1097/doi:", "http://dx.doi.org/doi:")

        XMLString = XMLString.Replace(ChrW(8233), "")
        XMLString = Regex.Replace(XMLString, "<!--(.*?)-->", "")
        'XMLString = XMLString.Replace("><", ">" & vbCrLf & "<")
        'XMLString = Regex.Replace(XMLString, """([AF]+[0]+)([0-9]+)""", """aff$2""", RegexOptions.IgnoreCase)
        'XMLString = Regex.Replace(XMLString, "([CIT]+[0]+)([0-9]+)", "bib$2", RegexOptions.IgnoreCase)
        'XMLString = Regex.Replace(XMLString, """([C])([1-9]+)""", """cor$2""", RegexOptions.IgnoreCase)
        'XMLString = Regex.Replace(XMLString, """([F])([1-9]+)""", """fig$2""", RegexOptions.IgnoreCase)
        'XMLString = Regex.Replace(XMLString, """([T])([1-9]+)""", """tbl$2""", RegexOptions.IgnoreCase)
        If (String.Compare(JournalName, "OIKOS", True) <> 0) Then
            XMLString = Regex.Replace(XMLString, "<sec[^>]*>", "<sec>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
    End Sub

    Private Function TableTdSpan(ByVal MsMath As Match) As String
        Return MsMath.Value.Replace("align=""center""", "align=""top""")
    End Function

    Private Function GraphicReplacement(ByVal MsMath As Match) As String
        Dim GraContent As String = MsMath.Value
        GraContent = GraContent.Replace("type=""simple""", "xlink:type=""simple""")
        GraContent = GraContent.Replace(" xmlns:xlink=""http://www.w3.org/1999/xlink""", "")
        GraContent = Regex.Replace(GraContent, "(href=""file(.*?)"")", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return GraContent
    End Function


    Public Sub InDesignCleanUp()
        XMLString = XMLString.Replace("<journal-id journal-id-type=""CATS"">", "<journal-id journal-id-type=""hwp"">")
        XMLString = XMLString.Replace("<journal-id journal-id-type=""publisher-code"">", "<journal-id journal-id-type=""nlm-ta"">")
        XMLString = XMLString.Replace("<journal-title-group>", "")
        XMLString = XMLString.Replace("</journal-title-group>", "")
        XMLString = XMLString.Replace(" xmlns:aid5=""http://ns.adobe.com/AdobeInDesign/5.0/""", "")
        XMLString = XMLString.Replace(" xmlns:aid=""http://ns.adobe.com/AdobeInDesign/4.0/""", "")
        XMLString = XMLString.Replace(" xmlns:mml=""http://www.w3.org/1998/Math/MathML""", "")
        XMLString = Regex.Replace(XMLString, " aid:pos=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid5:crows=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:pstyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:pstyle=""""", "")
        XMLString = Regex.Replace(XMLString, " aid:tfooter=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:tfooter=""""", "")
        XMLString = Regex.Replace(XMLString, " aid5:pstyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:fstyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:theader=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:cstyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:ostyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid5:cellstyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:table=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:crows=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:ccols=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:ccolwidth=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid5:tablestyle=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid5:pos=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, "<tbllabel[^>]*>.*?<\/tbllabel>", "")
        'XMLString = Regex.Replace(XMLString, "(<link)( xlink:href=""[^""]+"")([^>]*)(>)", "$1$3$4")
        XMLString = Regex.Replace(XMLString, "<Punctable ", "<table ")
        XMLString = Regex.Replace(XMLString, "</Punctable>", "</table>")

        XMLString = Regex.Replace(XMLString, "<table[^>]*><thead>", "<thead>")
        'XMLString = XMLString.Replace(ChrW(10), "")
        XMLString = XMLString.Replace(">" & ChrW(13) & "<", "><")
        XMLString = XMLString.Replace(ChrW(160), " ")
        XMLString = XMLString.Replace(ChrW(8201), "")
        XMLString = XMLString.Replace(ChrW(8195), "")
        XMLString = XMLString.Replace(" aid:table=""table""", "")
        XMLString = Regex.Replace(XMLString, " aid:trows=""[^""]+""", "")
        XMLString = Regex.Replace(XMLString, " aid:tcols=""[^""]+""", "")
        XMLString = XMLString.Replace("&gt;", "&#x003E;")
        XMLString = XMLString.Replace("&lt;", "&#x003C;")
        XMLString = XMLString.Replace(" &amp; ", " &#x0026; ")
        XMLString = XMLString.Replace("R&amp;D", "R&#x0026;D")
        XMLString = XMLString.Replace("&amp;", "&")
        ' Remove unwanted junk presented in the refernce comment.
        For Each MatCmt As Match In Regex.Matches(XMLString, "<!--(.*?)-->")
            Dim TmpComment As String = MatCmt.Value
            TmpComment = Regex.Replace(TmpComment, "(<!--)( PAGE.*?;"" )", "$1")
            XMLString = XMLString.Replace(MatCmt.Value, TmpComment)
        Next
        XMLString = Regex.Replace(XMLString, "<!--([A-Z]+[0-9]+)\:(.*?)-->", "")
#If CONFIG = "BB_IDPeresit" Then
        XMLString = Regex.Replace(XMLString, "<!--punc(.*?)-->", "$1", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
#Else
        XMLString = Regex.Replace(XMLString, "<!--punc(.*?)(</remark>)-->", "$1$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "<!--punc(.*?)-->", "")
#End If

        XMLString = Regex.Replace(XMLString, "<!--([A-Z]+[0-9]*)\:((?:(?:(?!-->).)+)-->)", "")

        'XMLString = XMLString.Replace("<thead>", "<tgroup cols=""1""><thead>")
        'XMLString = XMLString.Replace("</tbody>", "</tbody></tgroup>")
        XMLString = XMLString.Replace("</table></table>", "</table>").Replace("<table frame=""bottom"">", "")
        'XMLString = Regex.Replace(XMLString, "(</info>)(<table[^>]*>)(<tgroup)", "$1$3") '11-09-2021
    End Sub

    Public Sub XmlDomCleanup()


        SupplientURL = (From jd In SupplientFloatLinks Where (String.Compare(jd.JournalName, JournalName, True) = 0) Select jd.PermissionData).FirstOrDefault
        If (Not String.IsNullOrEmpty(SupplientURL)) Then
            'SupplientURL = SupplientURL.Replace("<InDesignName>", IndesignName)
            SupplientURL = SupplientURL.Replace("<InDesignName>", Regex.Replace(ArticleID.Replace("_", ""), "([A-Z]{1,3})([0-9]{1,2})([0-9]{1,4})", "$1-$2-$3"))
        End If

        'Try
        '    EntityConversion()
        'Catch ex As Exception
        '    Console.WriteLine(ex.Message)
        '    Console.ReadKey()
        'End Try



        Try
            Dim ColspecList As XmlNodeList = XMLDoc.SelectNodes("//colspec")
            If ((ColspecList IsNot Nothing) AndAlso (ColspecList.Count > 0)) Then
                For c As Integer = ColspecList.Count - 1 To 0 Step -1
                    Try
                        If (ColspecList(c).ParentNode IsNot Nothing) Then
                            ColspecList(c).ParentNode.RemoveChild(ColspecList(c))
                        End If
                    Catch ex As Exception
                    End Try
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            TableColspanRowspanCleanup()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            ModifiyHistoryMonth()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            RemoveBoldInHeading()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            InterchangeFrontAuthors()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            AddLabeltoAffiliationSuperScript()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            ApplyCorrespondingAuthorElement()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            CleanAffilicationElement()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            UpateJournalMeta()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            CleanFloatCitation()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        If (String.Compare(JournalName, "dove", True) <> 0) Then
            Try
                AddSelfUri()
            Catch ex As Exception
                GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            End Try
        End If

        Try
            CleanSectionTitle()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            CleanEtalInReference()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            ExpandReferenceCitations()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            RenumberAffiliationText()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            SuppliementFloatLinkGeneration()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            VideoHeadingGeneration()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

        Try
            CleanTabinReferenceLabel()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try

    End Sub

    Private Function TableColspanRowspanCleanup() As Boolean
        Dim TdList As XmlNodeList = XMLDoc.SelectNodes("//td[@namest]")
        If ((TdList IsNot Nothing) AndAlso (TdList.Count > 0)) Then
            For cl As Integer = 0 To TdList.Count - 1
                Dim NameStart As Integer = TdList(cl).Attributes("namest").Value.Replace("c", "")
                Dim NameEnd As Integer = TdList(cl).Attributes("nameend").Value.Replace("c", "")
                Try
                    TdList(cl).Attributes.RemoveNamedItem("namest")
                    TdList(cl).Attributes.RemoveNamedItem("nameend")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
                Dim rowSpanAtt As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "colspan", "")
                rowSpanAtt.Value = ((NameEnd - NameStart) + 1)
                TdList(cl).Attributes.Prepend(rowSpanAtt)
            Next
        End If
        Return True
    End Function

    Private Function ModifiyHistoryMonth() As Boolean
        Dim MonthNodes As XmlNodeList = XMLDoc.SelectNodes("//history/date/month")
        If ((MonthNodes IsNot Nothing) AndAlso (MonthNodes.Count > 0)) Then
            For mt As Integer = 0 To MonthNodes.Count - 1
                Try
                    MonthNodes(mt).InnerText = Convert.ToDateTime("01-" & MonthNodes(mt).InnerText & "-2017").Month
                Catch ex As Exception
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function

    Private Function CleanTabinReferenceLabel() As Boolean
        Dim RefLabels As XmlNodeList = XMLDoc.SelectNodes("//ref/label")
        If ((RefLabels Is Nothing) OrElse (RefLabels.Count = 0)) Then Return False
        For Each Lbl As XmlNode In RefLabels
            Lbl.InnerText = Lbl.InnerText.Replace(vbTab, "")
        Next
        Return True
    End Function

    Private Function RenumberAffiliationText() As Boolean
        Dim ContribXrefs As XmlNodeList = Nothing
        Dim RidValue As String = String.Empty
        ContribXrefs = XMLDoc.SelectNodes("//contrib/xref[@ref-type='aff']")
        For Each Xref As XmlElement In ContribXrefs
            RidValue = Xref.GetAttribute("rid")
            If ((Not String.IsNullOrEmpty(RidValue) AndAlso (String.Compare(RidValue, "AFNaN", True) = 0))) Then
                If (Regex.Match(RidValue, "[0-9]+").Success) Then
                    Xref.SetAttribute("rid", "AF" & RidValue.PadLeft(4, "0"))
                End If
            End If
        Next
        Return True
    End Function

    Private Function ExpandReferenceCitations() As Boolean
        Dim RefCitations As XmlNodeList = Nothing
        Dim CitationContents As New List(Of String)
        Dim XrefParent As XmlNode = Nothing
        RefCitations = XMLDoc.SelectNodes("//xref[@ref-type='bibr']")
        If ((RefCitations Is Nothing) OrElse (RefCitations.Count = 0)) Then Return False
        For rf As Integer = 0 To RefCitations.Count - 1
            Dim RefCit As XmlElement = RefCitations(rf)
            XrefParent = RefCit.ParentNode
            CitationContents = New List(Of String)
            If (RefCit.InnerText.Trim().Contains(",") AndAlso (Not RefCit.InnerText.Trim.EndsWith(","))) Then
                CitationContents.AddRange(RefCit.InnerText.Split(New String() {", ", ","}, StringSplitOptions.RemoveEmptyEntries))
                'RefCit.InnerText = CitationContents(0)
                'RefCit.SetAttribute("rid", "CIT" & CitationContents(0).PadLeft(5, "0"))
                For i As Integer = 0 To CitationContents.Count - 1
                    Dim CitText As String = CitationContents(i)
                    Dim XrefNode As XmlElement = XMLDoc.CreateNode(XmlNodeType.Element, "xref", "")
                    XrefNode.SetAttribute("ref-type", "bibr")
                    XrefNode.SetAttribute("rid", "CIT" & CitationContents(i).PadLeft(5, "0"))
                    XrefNode.InnerText = CitText
                    XrefParent.InsertBefore(XrefNode, RefCit)
                    If (i <> CitationContents.Count - 1) Then
                        Dim SparaterNode As XmlNode = XMLDoc.CreateNode(XmlNodeType.Text, "comma", "")
                        SparaterNode.InnerText = ", "
                        XrefNode.ParentNode.InsertAfter(SparaterNode, XrefNode)
                    End If
                Next
                XrefParent.RemoveChild(RefCit)
            End If
        Next
        Return True
    End Function

    Private Function CleanEtalInReference() As Boolean
        Dim CitationGroups As XmlNodeList = Nothing
        CitationGroups = XMLDoc.SelectNodes("//mixed-citation|citation")
        If ((CitationGroups Is Nothing) OrElse (CitationGroups.Count = 0)) Then Return False
        For Each Citation As XmlNode In CitationGroups
            Citation.InnerXml = Citation.InnerXml.Replace(ChrW(160), " ")
            If (Regex.Match(Citation.InnerXml, "<bold[^>]*><italic>et al.</italic></bold>", RegexOptions.IgnoreCase).Success) Then
                Citation.InnerXml = Regex.Replace(Citation.InnerXml, "<bold[^>]*><italic>et al.</italic></bold>", "<etal/>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            ElseIf (Regex.Match(Citation.InnerXml, "<italic>( )*et al.</italic>", RegexOptions.IgnoreCase).Success) Then
                If (DocType = LanstadClientType.JOURNAL) Then
                    Citation.InnerXml = Citation.InnerXml.Replace("<italic>et al.</italic>", "<etal/>").Replace("<italic> et al.</italic>", "<etal/>")
                ElseIf ((DocType = LanstadClientType.BOOK) Or (DocType = LanstadClientType.RANDL)) Then
                    Citation.InnerXml = Citation.InnerXml.Replace("<italic>et al</italic>", "")
                    Dim PersonGroup As XmlNode = Citation.SelectSingleNode("./person-group")
                    PersonGroup.InnerXml = PersonGroup.InnerXml & " <etal/>"
                End If
            End If
        Next
        Return True
    End Function

    Private Function CleanSectionTitle() As Boolean
        Dim Titles As XmlNodeList = XMLDoc.SelectNodes("//sec/title")
        If ((Titles Is Nothing) OrElse (Titles.Count = 0)) Then Return False
        For Each title As XmlNode In Titles
            If (String.Compare(title.InnerText, "Supplementary data", True) = 0) Then
                Dim SecElement As XmlElement = title.ParentNode
                Dim SecType As XmlAttribute = SecElement.GetAttributeNode("sec-type")
                SecElement.SetAttribute("id", "supp" & Regex.Replace(SecElement.GetAttribute("sec-type"), "[^0-9]+", ""))
                'SecElement.SetAttribute("sec-type", "supp" & Regex.Replace(SecElement.GetAttribute("sec-type"), "[^0-9]+", ""))
                SecElement.Attributes.Remove(SecType)
            End If
        Next

        Dim SupplList As XmlNodeList = XMLDoc.SelectNodes("//p")
        If ((SupplList IsNot Nothing) AndAlso (SupplList.Count > 0)) Then
            For sp As Integer = 0 To SupplList.Count - 1
                Try
                    SupplList(sp).InnerXml = SupplList(sp).InnerXml.Replace("supplementary data", "<xref ref-type=""supplementary-material"" rid=""supp1"">supplementary data</xref>")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function

    Private Function AddSelfUri() As Boolean
        Dim SelfUri As XmlNode = XMLDoc.SelectSingleNode("//self-uri")
        Dim abstract As XmlNode = XMLDoc.SelectSingleNode("//abstract")
        If (abstract Is Nothing) Then Return False
        If (SelfUri Is Nothing) Then
            SelfUri = XMLDoc.CreateNode(XmlNodeType.Element, "self-uri", "")
            Dim ContentType As XmlAttribute = XMLDoc.CreateAttribute("content-type", "")
            ContentType.Value = "pdf"
            Dim Href As XmlAttribute = XMLDoc.CreateAttribute("xlink:href", "xlink:")
            Href.Value = FirstPage & ".pdf"
            SelfUri.Attributes.Append(ContentType)
            SelfUri.Attributes.Append(Href)
            abstract.ParentNode.InsertBefore(SelfUri, abstract)
        Else
            SelfUri.Attributes("xlink:href").Value = FirstPage & ".pdf"
        End If
        Return True
    End Function

    Public Function RemoveBoldInHeading() As Boolean
        'Dim InvalidBoldTag As New List(Of String)(New String() {"article-title", "title", "corresp", "label", "volume", "fpage", "lpage", "issue", "xref"})
        Dim InvalidBoldTag As New List(Of String)(New String() {"source", "corresp", "volume", "fpage", "lpage", "issue", "xref", "alt-title", "label"})
        Dim EmphasisTag As New List(Of String)(New String() {"bold", "italic", "sup"})
        For Each Emphasis As String In EmphasisTag
            For Each HeadTag As String In InvalidBoldTag
                Dim HeadingNodes As XmlNodeList = XMLDoc.GetElementsByTagName(HeadTag)
                If ((HeadingNodes Is Nothing) OrElse (HeadingNodes.Count = 0)) Then Continue For
                For tg As Integer = HeadingNodes.Count - 1 To 0 Step -1
                    Dim HeadingNode As XmlElement = HeadingNodes(tg)
                    Dim BoldNodes As XmlNodeList = HeadingNode.GetElementsByTagName(Emphasis)
                    If (String.Compare(HeadTag, "xref", True) = 0) Then
                        If (HeadingNode.ParentNode.Name <> "contrib") Then Continue For
                    End If
                    If ((BoldNodes Is Nothing) OrElse (BoldNodes.Count = 0)) Then Continue For
                    For bd As Integer = BoldNodes.Count - 1 To 0 Step -1
                        Dim BdNode As XmlNode = XMLDoc.CreateTextNode(Emphasis & "tmp")
                        BdNode.InnerText = BoldNodes(bd).InnerText
                        BoldNodes(bd).ParentNode.InsertBefore(BdNode, BoldNodes(bd))
                        BoldNodes(bd).ParentNode.RemoveChild(BoldNodes(bd))
                    Next
                Next
            Next
        Next
        Return True
    End Function

    Public Function AddLabeltoAffiliationSuperScript() As Boolean
        Dim AffSupElements As XmlNodeList
        AffSupElements = XMLDoc.SelectNodes("//aff/sup")
        If ((AffSupElements Is Nothing) OrElse (AffSupElements.Count = 0)) Then Return False
        For Each SupElement As XmlNode In AffSupElements
            Dim AffLabel As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, "label", "")
            SupElement.ParentNode.InsertBefore(AffLabel, SupElement)
            AffLabel.PrependChild(SupElement)
        Next
        Return True
    End Function

    Public Function InitializeJournalMetaData() As Boolean
        'JournalName = Path.GetFileNameWithoutExtension(ExportFileName)
        'If (Not JournalName.Contains("-")) Then
        '    Return False
        'End If
        'JournalName = Regex.Replace(JournalName, "[^A-Z]+", "")
        If ((JournalMetaList Is Nothing) OrElse (JournalMetaList.Count = 0)) Then
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "echo"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/journal-title-group/journal-title", "Echo Research and Practice"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/issn[@pub-type='epub']", "2055-0464"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/issn[@pub-type='ppub']", True))
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("ERP", "//article-meta/issue", IssueNo))

            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "EJE"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "EUR J ENDOCRINOL"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/journal-title-group/journal-title", "European Journal of Endocrinology"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/issn[@pub-type='ppub']", "0804-4643"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/issn[@pub-type='epub']", "1479-683X"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("EJE", "//article-meta/issue", IssueNo))

            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "EC"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "ENDOCRINE CONNECTIONS"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/journal-title-group/journal-title", "Endocrine Connections"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/issn[@pub-type='epub']", True))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/issn[@pub-type='epub']", "2049-3614"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("ECC", "//article-meta/issue", IssueNo))


            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "ERC"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "ENDOCR RELAT CANCER"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/journal-title-group/journal-title", "Endocrine-Related Cancer"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/issn[@pub-type='ppub']", "1351-0088"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/issn[@pub-type='epub']", "1479-6821"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("ERC", "//article-meta/issue", IssueNo))

            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "JME"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "J MOL ENDOCRINOL"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/journal-title-group/journal-title", "Journal of Molecular Endocrinology"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/issn[@pub-type='ppub']", "0952-5041"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/issn[@pub-type='epub']", "1479-6813"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("JME", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("JME", "//article-meta/issue", IssueNo))

            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "JOE"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "J ENDOCRINOL"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/journal-title-group/journal-title", "Journal of Endocrinology"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/issn[@pub-type='ppub']", "0022-0795"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/issn[@pub-type='epub']", "1479-6805"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/pub-date[@pub-type='epub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString()))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("JOE", "//article-meta/issue", IssueNo))

            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "REP"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "REPRODUCTION"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/journal-title-group/journal-title", "Reproduction"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/issn[@pub-type='ppub']", "1470-1626"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/issn[@pub-type='epub']", "1741-7899"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("REP", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='epub']/month", "00"))

            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/journal-id[@journal-id-type='CATS']", "VB"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/journal-id[@journal-id-type='publisher-code']", "VASCULAR BIOLOGY"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/journal-title-group/journal-title", "Vascular Biology"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/issn[@pub-type='ppub']", "0000-0000"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/issn[@pub-type='epub']", "0000-0000"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/publisher/publisher-name", "Bioscientifica Ltd"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//front/journal-meta/publisher/publisher-loc", "Bristol"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//article-meta/fpage", FirstPage))
            JournalMetaList.Add(New JournalMetaData("VAB", "//article-meta/lpage", LastPage))
            JournalMetaList.Add(New JournalMetaData("VAB", "//article-meta/pub-date[@pub-type='ppub']/month", "00"))
            JournalMetaList.Add(New JournalMetaData("VAB", "//article-meta/pub-date[@pub-type='epub']/month", "00"))

            'JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='epub']/year", DateTime.Now.Year.ToString()))
            'JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='ppub']/year", DateTime.Now.Year.ToString())) 

            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='epub']/year", "2018"))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/pub-date[@pub-type='ppub']/year", "2018"))

            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/volume", VolumeNo))
            JournalMetaList.Add(New JournalMetaData("REP", "//article-meta/issue", IssueNo))

        End If

        If ((JournalPermissions Is Nothing) OrElse (JournalPermissions.Count = 0)) Then
            'JournalPermissions.Add(New JournalMetaPermission("ERP", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 The authors</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>The authors</copyright-holder><license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/""><license-p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc/4.0/88x31.png"" />This work is licensed under a <ext-link xlink:type=""simple"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/"">Creative Commons Attribution-NonCommercial 4.0 International License</ext-link>.</license-p></license></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("EJE", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 European Society of Endocrinology</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>European Society of Endocrinology</copyright-holder></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("EC", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 The authors</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>The authors</copyright-holder><license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/""><p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png"" /> This work is licensed under a <ext-link ext-link-type=""uri"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/"">Creative Commons Attribution-NonCommercial-NoDerivs 4.0 International License</ext-link>.</p></license></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("ERC", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 Society for Endocrinology</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("JME", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 Society for Endocrinology</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("JOE", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 Society for Endocrinology</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            'JournalPermissions.Add(New JournalMetaPermission("REP", "<history><date date-type=""received""><day>00</day><month>00</month><year>2016</year></date><date date-type=""rev-recd""><day>00</day><month>00</month><year>2016</year></date><date date-type=""accepted""><day>00</day><month>00</month><year>2016</year></date></history><permissions><copyright-statement>&#x00A9; 2016 Society for Reproduction and Fertility</copyright-statement><copyright-year>2016</copyright-year><copyright-holder>Society for Reproduction and Fertility</copyright-holder></permissions>"))

            JournalPermissions.Add(New JournalMetaPermission("ERP", "<permissions><copyright-statement>&#x00A9; 2018 The authors</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>The authors</copyright-holder><license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/""><license-p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc/4.0/88x31.png"" />This work is licensed under a <ext-link xlink:type=""simple"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/"">Creative Commons Attribution-NonCommercial 4.0 International License</ext-link>.</license-p></license></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("EJE", "<permissions><copyright-statement>&#x00A9; 2018 European Society of Endocrinology</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>European Society of Endocrinology</copyright-holder></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("ECC", "<permissions><copyright-statement>&#x00A9; 2018 The authors</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>The authors</copyright-holder><license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/""><p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png"" /> This work is licensed under a <ext-link ext-link-type=""uri"" xlink:href=""http://creativecommons.org/licenses/by-nc-nd/4.0/"">Creative Commons Attribution-NonCommercial-NoDerivs 4.0 International License</ext-link>.</p></license></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("ERC", "<permissions><copyright-statement>&#x00A9; 2018 Society for Endocrinology</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("JME", "<permissions><copyright-statement>&#x00A9; 2018 Society for Endocrinology</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("JOE", "<permissions><copyright-statement>&#x00A9; 2018 Society for Endocrinology</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>Society for Endocrinology</copyright-holder></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("REP", "<permissions><copyright-statement>&#x00A9; 2018 Society for Reproduction and Fertility</copyright-statement><copyright-year>2018</copyright-year><copyright-holder>Society for Reproduction and Fertility</copyright-holder></permissions>"))
            JournalPermissions.Add(New JournalMetaPermission("VAB", "<permissions><copyright-statement>&#x00A9; 2019 The authors</copyright-statement><copyright-year>2019</copyright-year><copyright-holder>The authors</copyright-holder><license license-type=""open-access"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/""><license-p><inline-graphic xlink:href=""http://i.creativecommons.org/l/by-nc/4.0/88x31.png"" />This work is licensed under a <ext-link xlink:type=""simple"" xlink:href=""http://creativecommons.org/licenses/by-nc/4.0/"">Creative Commons Attribution-NonCommercial 4.0 International License</ext-link>.</license-p></license></permissions>"))

        End If

        If ((SupplientFloatLinks Is Nothing) OrElse (SupplientFloatLinks.Count = 0)) Then
            SupplientFloatLinks.Add(New JournalMetaPermission("EJE", "<ext-link ext-link-type=""uri"" xlink:href=""http://www.eje-online.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("ECC", "<ext-link ext-link-type=""uri"" xlink:href=""http://www.endocrineconnections.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("ERC", "<ext-link ext-link-type=""uri"" xlink:href=""http://erc.endocrinology-journals.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("JOE", "<ext-link ext-link-type=""uri"" xlink:href=""http://joe.endocrinology-journals.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("JME", "<ext-link ext-link-type=""uri"" xlink:href=""http://jme.endocrinology-journals.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("REP", "<ext-link ext-link-type=""uri"" xlink:href=""http://www.reproduction-online.org/cgi/content/full/<InDesignName>/DC1"">"))
            SupplientFloatLinks.Add(New JournalMetaPermission("ERP", "<ext-link ext-link-type=""uri"" xlink:href=""http://www.echorespract.com/cgi/content/full/<InDesignName>/DC1"">"))
        End If

        Return True
    End Function

    Public Function SuppliementFloatLinkGeneration() As Boolean
        Dim SupParttern As String = "(>)?(supplementary (table|figure|fig.)) ([A-Z0-9]?[0-9]+)(<)?"
        Dim ExtLinkNode As XmlNodeList = Nothing
        Dim SupplientList As New List(Of String)(New String() {""})
        If (String.IsNullOrEmpty(SupplientURL)) Then Return False
        For Each ExtLink As XmlElement In XMLDoc.SelectNodes("//p/ext-link")
            If (Not ExtLink.HasAttribute("ext-link-type")) Then
                ExtLink.SetAttribute("ext-link-type", "uri")
            End If
            If (Not ExtLink.HasAttribute("xlink:href")) Then
                ExtLink.SetAttribute("xlink:href", SupplientURL.Replace("<ext-link ext-link-type=""uri"" xlink:href=""", "").Replace(""">", ""))
            End If
        Next
        For Each ParaNode As XmlNode In XMLDoc.SelectNodes("//p")
            Dim SuppMatch As Match = Regex.Match(ParaNode.InnerXml, SupParttern, RegexOptions.IgnoreCase)
            If ((SuppMatch.Success) AndAlso (String.IsNullOrEmpty(SuppMatch.Groups(1).Value))) Then
                Try
                    ParaNode.InnerXml = ParaNode.InnerXml.Replace(SuppMatch.Value, String.Format("{0}{1}{2}", SupplientURL, SuppMatch.Value, "</ext-link>"))
                Catch ex As Exception
                End Try
            End If
        Next
        Return True
    End Function

    Public Function VideoHeadingGeneration() As Boolean
        Dim VideoId As String = String.Empty
        For Each VideoNode As XmlNode In XMLDoc.SelectNodes("//sec")
            If (VideoNode.InnerText.ToLower().StartsWith("video")) Then
                VideoId = Regex.Replace(VideoNode.ChildNodes(0).InnerText, "[^0-9]+", "")
                Dim SuppMaterial As XmlElement = XMLDoc.CreateNode(XmlNodeType.Element, "supplementary-material", "")
                SuppMaterial.SetAttribute("id", "SM" & VideoId)
                SuppMaterial.SetAttribute("specific-use", "collapsible")
                VideoNode.InnerXml = VideoNode.InnerXml.Replace("<title>", "<label>").Replace("</title>", "</label>")
                VideoNode.InnerXml = VideoNode.InnerXml.Replace("</p>", "</p><p><inline-supplementary-material mimetype=""video"" mime-subtype=""wmv"" xlink:href=""D1video_clip_" & VideoId & ".wmv"">Download Video 1</inline-supplementary-material></p>")
                SuppMaterial.InnerXml = VideoNode.InnerXml
                VideoNode.ParentNode.InsertBefore(SuppMaterial, VideoNode)
                VideoNode.ParentNode.RemoveChild(VideoNode)
            End If
        Next
        Return True
    End Function

    Public Function CleanAffilicationElement() As Boolean
        Dim Affilications As XmlNodeList = Nothing
        Affilications = XMLDoc.SelectNodes("//contrib-group/aff")
        For Each Aff As XmlNode In Affilications
            Aff.InnerXml = Aff.InnerXml.Replace("<country>", "").Replace("</country>", "").Replace("<institution>", "").Replace("</institution>", "")
        Next
        Dim TextNode As XmlNode = Nothing
        Dim AffIndex As Integer = 0
        Dim Content As String = String.Empty
        Affilications = XMLDoc.SelectNodes("//contrib-group/aff/text()")
        For af As Integer = 0 To Affilications.Count - 1
            TextNode = Affilications(af)
            If (TextNode.InnerText.Contains(",")) Then
                AffIndex = TextNode.InnerText.IndexOf(",")
                If (AffIndex > 0) Then
                    Dim InstiNode As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, "institution", "")
                    Dim AddressNode As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, "addr-line", "")
                    Content = TextNode.InnerText
                    InstiNode.InnerText = Content.Substring(0, AffIndex)
                    Content = Content.Replace(Content.Substring(0, AffIndex + 2), "")
                    AddressNode.InnerText = Content
                    TextNode.ParentNode.InsertBefore(InstiNode, TextNode)
                    TextNode.ParentNode.InsertBefore(AddressNode, TextNode)
                    TextNode.ParentNode.RemoveChild(TextNode)
                End If
            End If
        Next
        Return True
    End Function

    Public Function InterchangeFrontAuthors() As Boolean
        Dim Authors As XmlNodeList = Nothing
        Authors = XMLDoc.SelectNodes("//contrib/name")
        For Each author As XmlNode In Authors
            If (author.ChildNodes.Count > 0) Then
                'author.InnerXml = Regex.Replace(author.InnerXml, "(</surname>)(.*?)(<given-names/>)", "$2$1$3")
                Dim SurNode As XmlNode = GetChildNode(author, "surname")
                Dim GivenNode As XmlNode = GetChildNode(author, "given-names")
                If (SurNode IsNot Nothing) AndAlso (GivenNode IsNot Nothing) Then
                    Dim TmpSurname As String = SurNode.InnerText
                    SurNode.InnerText = GivenNode.InnerText
                    GivenNode.InnerText = TmpSurname
                End If
            End If
        Next
        Return True
    End Function


    Public Function GetChildNode(ParentElement As XmlNode, ChildName As String) As XmlNode
        If ((ParentElement.ChildNodes Is Nothing) OrElse (ParentElement.ChildNodes.Count = 0)) Then Return Nothing
        For Each Child As XmlNode In ParentElement.ChildNodes
            If (String.Compare(Child.Name, ChildName, False) = 0) Then
                Return Child
            End If
        Next
        Return Nothing
    End Function

    Public Function GetInnerChildNode(ParentElement As XmlNode, ChildName As String) As XmlNode
        If (String.Compare(ParentElement.Name, ChildName, True) = 0) Then
            Return ParentElement
        End If
        If ((ParentElement.ChildNodes Is Nothing) OrElse (ParentElement.ChildNodes.Count = 0)) Then Return Nothing
        For Each Child As XmlNode In ParentElement.ChildNodes
            Dim objNode As XmlNode = GetInnerChildNode(Child, ChildName)
            If (objNode IsNot Nothing) Then
                Return objNode
            End If
        Next
        Return Nothing
    End Function

    Public Function UpateJournalMeta() As Boolean
        Dim JourList As New List(Of JournalMetaData)
        JourList = (From jd In JournalMetaList Where (String.Compare(jd.JournalName, JournalName, False) = 0) Select jd).ToList
        For Each JourMeta As JournalMetaData In JourList
            Dim FrontElement As XmlNode = XMLDoc.SelectSingleNode(JourMeta.ElementXPath)
            If (JourMeta.IsRemoved) Then
                FrontElement.ParentNode.RemoveChild(FrontElement)
            Else
                If (FrontElement IsNot Nothing) Then
                    FrontElement.InnerText = JourMeta.ElementValue
                End If
            End If
        Next

        Dim Abbrev As XmlNode = XMLDoc.SelectSingleNode("//abbrev-journal-title")
        If (Abbrev IsNot Nothing) Then
            Abbrev.ParentNode.RemoveChild(Abbrev)
        End If

        Return True
    End Function

    Public Function ApplyCorrespondingAuthorElement() As Boolean
        Dim FrontAuthors As XmlNodeList = Nothing
        Dim CorresElement As XmlNode = Nothing
        Dim ContribGroups As XmlNodeList = Nothing
        ContribGroups = XMLDoc.SelectNodes("//contrib-group/contrib")
        For Each contrib As XmlNode In ContribGroups
            Dim CorresAttrib As XmlAttribute = contrib.Attributes("corresp")
            If (CorresAttrib IsNot Nothing) Then
                contrib.Attributes.Remove(CorresAttrib)
            End If
        Next
        CorresElement = XMLDoc.SelectSingleNode("//author-notes/corresp")
        If (CorresElement Is Nothing) Then Return True
        FrontAuthors = XMLDoc.SelectNodes("//contrib/name")
        For Each Author As XmlNode In FrontAuthors
            Dim SurNode As XmlNode = GetChildNode(Author, "surname")
            Dim GivenNode As XmlNode = GetChildNode(Author, "given-names")
            Dim AuthorText As String = String.Empty
            If ((SurNode IsNot Nothing) AndAlso (GivenNode IsNot Nothing)) Then
                'AuthorText = String.Format("{0} {1}", IIf(GivenNode.InnerText.Length > 1, GivenNode.InnerText(0), GivenNode.InnerText), SurNode.InnerText)
                AuthorText = String.Format("{0}", SurNode.InnerText)
            End If
            If ((Not String.IsNullOrEmpty(AuthorText)) AndAlso (CorresElement.InnerText.Contains(AuthorText))) Then
                Dim CorresAttrib As XmlAttribute = XMLDoc.CreateAttribute("corresp", "")
                CorresAttrib.Value = "yes"
                Author.ParentNode.Attributes.Append(CorresAttrib)
                Author.ParentNode.InnerXml = Author.ParentNode.InnerXml & "<xref ref-type=""corresp"" rid=""cor1""/>"
                'CorresElement.InnerXml = CorresElement.InnerXml.Replace(SurNode.InnerText, String.Format("<surname>{0}</surname>", SurNode.InnerText))
                'CorresElement.InnerXml = CorresElement.InnerXml.Replace(IIf(GivenNode.InnerText.Length > 1, GivenNode.InnerText(0), GivenNode.InnerText), String.Format("<given-names>{0}</given-names>", IIf(GivenNode.InnerText.Length > 1, GivenNode.InnerText(0), GivenNode.InnerText)))
            End If
        Next
        Dim EmailPattern As String = "[_a-z0-9-]+(.[a-z0-9-]+)@[a-z0-9-]+(.[a-z0-9-]+)*(.[a-z]{2,4})"
        CorresElement.InnerXml = CorresElement.InnerXml.Replace("\n", "").Replace("\r", "").Replace(vbLf, "").Replace(vbCr, "").Replace(vbCrLf, "")

        For Each email As String In CorresElement.InnerText.Split(" ")
            If (Regex.Match(email, EmailPattern).Success) Then
                CorresElement.InnerXml = CorresElement.InnerXml.Replace(email, String.Format("<email>{0}</email>", email))
            End If
        Next

        CorresElement.InnerXml = Regex.Replace(CorresElement.InnerXml, " (email) ", "; $1: ", RegexOptions.IgnoreCase)
        Return True
    End Function

    Public Function CleanFloatCitation() As Boolean
        Dim Floats As New List(Of String)(New String() {"fig", "table-wrap"})
        Dim FloatElements As XmlNodeList = Nothing
        Dim NameSpaceManager As New System.Xml.XmlNamespaceManager(XMLDoc.NameTable)
        NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
        NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
        NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
        NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
        For Each float As String In Floats
            FloatElements = XMLDoc.SelectNodes("//" & float, NameSpaceManager)
            If ((FloatElements Is Nothing) OrElse (FloatElements.Count = 0)) Then Continue For
            For ft As Integer = 0 To FloatElements.Count - 1
                Dim fltElement As XmlNode = FloatElements(ft)
                Dim TmpFig As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, float, "")
                For Each att As XmlAttribute In FloatElements(ft).Attributes
                    Dim tmpAtt As XmlAttribute = XMLDoc.CreateAttribute(att.Name, "")
                    tmpAtt.Value = att.Value
                    TmpFig.Attributes.Append(tmpAtt)
                Next
                TmpFig.InnerXml = fltElement.InnerXml
                fltElement.ParentNode.AppendChild(TmpFig)
                fltElement.ParentNode.RemoveChild(fltElement)
                'FltParent.InnerXml = FltParent.InnerXml & TmpFig.InnerXml
            Next
        Next

        If (String.Compare(JournalName, "OIKO", True) <> 0) Then
            FloatElements = XMLDoc.SelectNodes("//fig/graphic", NameSpaceManager)
            For Each graphic As XmlElement In FloatElements
                If ((graphic.PreviousSibling Is Nothing) OrElse (String.Compare(graphic.PreviousSibling.Name, "caption", True) <> 0)) Then
                    Dim Caption As XmlNode = GetChildNode(graphic.ParentNode, "caption")
                    If (Caption IsNot Nothing) Then
                        Caption.ParentNode.InsertAfter(graphic, Caption)
                    End If
                End If
                Dim Figname As String = String.Empty
                Figname = CType(graphic.ParentNode, XmlElement).GetAttribute("id")
                graphic.SetAttribute("xlink:href", FirstPage & "fig" & Regex.Replace(Figname, "[^0-9]+", ""))
                graphic.SetAttribute("position", CType(graphic.ParentNode, XmlElement).GetAttribute("position"))
                graphic.SetAttribute("type", "simple")
                'graphic.ParentNode.InnerXml = graphic.ParentNode.InnerXml.Replace("  xmlns:xlink=""http://www.w3.org/1999/xlink""", "").Replace("type=", "xlink:type=")
                'graphic.ParentNode.InnerXml = graphic.ParentNode.InnerXml.Replace("type=", "xlink:type=")
            Next
        End If
        Return True
    End Function

    Public Function EntityConversion() As Boolean
        Dim Entities As XmlNodeList
        Entities = XMLDoc.GetElementsByTagName("entity")
        If ((Entities Is Nothing) OrElse (Entities.Count = 0)) Then Return False
        For ent As Int32 = Entities.Count - 1 To 0 Step -1
            If (Not String.IsNullOrEmpty(Entities(ent).InnerText) AndAlso (AscW(Entities(ent).InnerText) <> 160)) Then
                Dim EntitTag As XmlNode = XMLDoc.CreateTextNode("ent")
                EntitTag.Value = String.Format("&#x{0};", Hex(AscW(Entities(ent).InnerText)).ToString().PadLeft(4, "0"))
                Entities(ent).ParentNode.InsertBefore(EntitTag, Entities(ent))
                Entities(ent).ParentNode.RemoveChild(Entities(ent))
            Else
                Entities(ent).ParentNode.RemoveChild(Entities(ent))
            End If
        Next
        Return True
    End Function


    Public Function CreateXmlAttribute(RootElement As XmlNode, AttribName As String, AttribValue As String) As Boolean
        Dim DtdAttribute As XmlAttribute = XMLDoc.CreateAttribute(AttribName)
        DtdAttribute.Value = AttribValue
        Try
            RootElement.AppendChild(DtdAttribute)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Public Function RetaingInDesignTableWithRowstartandEnd(ByVal XMLString As String) As String
        'remove the role="truncate-xml"
        XMLString = Regex.Replace(XMLString, "<entry[^>]*>((?:(?!<\/entry>).)*)</entry>", Function(mt)
                                                                                              If (mt.Value.Contains("role=""truncate-xml""")) Then
                                                                                                  Return String.Empty
                                                                                              End If
                                                                                              Return mt.Value
                                                                                          End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = Regex.Replace(XMLString, "<entry[^>]* aid5:pos=""rowstart""[^>]*>((?:(?!<\/entry>).)*)</entry>", Function(mt)
                                                                                                                         Dim Tmp As String = "<row>" & mt.Value
                                                                                                                         Return Tmp
                                                                                                                     End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<entry[^>]* aid5:pos=""rowend""[^>]*>((?:(?!<\/entry>).)*)</entry>", Function(mt)
                                                                                                                       Return mt.Value & "</row>"
                                                                                                                   End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = XMLString.Replace("</row><entry", "</row><row><entry")
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*/>)(<row>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*>(?:(?!<\/entry>).)*</entry>)(<row>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<tgroup[^>]*>)(<entry)", "$1<row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*/>)(</tgroup>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*>(?:(?!<\/entry>).)*</entry>)(</tgroup>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        XMLString = Regex.Replace(XMLString, "<td[^>]* aid5:pos=""rowstart""[^>]*>((?:(?!<\/td>).)*)</td>", Function(mt)
                                                                                                                Dim Tmp As String = "<tr>" & mt.Value
                                                                                                                Return Tmp
                                                                                                            End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<td[^>]* aid5:pos=""rowend""[^>]*>((?:(?!<\/td>).)*)</td>", Function(mt)
                                                                                                              Return mt.Value & "</tr>"
                                                                                                          End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = XMLString.Replace("</tr><td", "</tr><tr><td")
        XMLString = Regex.Replace(XMLString, "(<td[^>]*/>)(<tr>)", "$1</tr>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<td[^>]*>(?:(?!<\/td>).)*</td>)(<tr>)", "$1</tr>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<table[^>]*>)(<td)", "$1<tr>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<td[^>]*/>)(</table>)", "$1</tr>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<td[^>]*>(?:(?!<\/td>).)*</td>)(</table>)", "$1</tr>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)



        Return XMLString
    End Function
    Public Function RetaingInDesignTableWithRowstartandEnd_13012023(ByVal XMLString As String) As String

        XMLString = Regex.Replace(XMLString, "<entry[^>]* aid5:pos=""rowstart""[^>]*>((?:(?!<\/entry>).)*)</entry>", Function(mt)
                                                                                                                         Dim Tmp As String = "<row>" & mt.Value
                                                                                                                         Return Tmp
                                                                                                                     End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        XMLString = Regex.Replace(XMLString, "<entry[^>]* aid5:pos=""rowend""[^>]*>((?:(?!<\/entry>).)*)</entry>", Function(mt)
                                                                                                                       Return mt.Value & "</row>"
                                                                                                                   End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        XMLString = XMLString.Replace("</row><entry", "</row><row><entry")
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*/>)(<row>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*>(?:(?!<\/entry>).)*</entry>)(<row>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<tgroup[^>]*>)(<entry)", "$1<row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*/>)(</tgroup>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        XMLString = Regex.Replace(XMLString, "(<entry[^>]*>(?:(?!<\/entry>).)*</entry>)(</tgroup>)", "$1</row>$2", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Return XMLString
    End Function

    Public Function RetaingInDesignTableWithRowstartandEnd() As Boolean
        Dim Tables As XmlNodeList = Nothing
        Dim RetTable As XmlNode = Nothing
        Dim TblCRow As String = String.Empty
        Dim TblCData As String = String.Empty
        Dim TblCBody As String = String.Empty
        Dim TblCHead As String = String.Empty
        Dim TotalRowSpan As Integer = 0
        Dim TblRoot As String = String.Empty
        Dim RowCount As Integer = 0
        Dim CompleteRow As Integer = 0
        Dim ColumnCount As Integer = 0
        Dim TotalTDCount As Integer = 0
        Dim TblBody As XmlNode = Nothing
        Dim TblNodeList As XmlNodeList = Nothing
        Dim RowSpanCount As Int32 = 0
        Dim TablrRow As XmlNode = Nothing
        Dim TblHeadStyle As String = String.Empty
        Dim TblBodyStyle As String = String.Empty
        If (DocType = LanstadClientType.JOURNAL) Then
            TblCRow = "tr"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "td"
            Tables = XMLDoc.SelectNodes("//table")
            TblHeadStyle = "TCH"
            TblBodyStyle = "TB"
        ElseIf ((DocType = LanstadClientType.BOOK) Or (DocType = LanstadClientType.RANDL)) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        ElseIf ((DocType = LanstadClientType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        End If
        Dim iCounter As Integer = 0
        Dim TdContent As String = String.Empty
        For Each Tbl As XmlElement In Tables
            For Each TD As XmlNode In Tbl.SelectNodes(".//" & TblCData)
                TdContent = String.Empty
                If ((TD.Attributes IsNot Nothing) AndAlso (TD.Attributes("aid5:pos") IsNot Nothing)) Then
                    TdContent = TD.Attributes("aid5:pos").Value
                End If
                If (String.Compare(TdContent, "rowstart", True) = 0) Then
                    If (TD.ParentNode IsNot Nothing) Then
                        Try
                            TD.ParentNode.InnerXml = TD.ParentNode.InnerXml.Replace(TD.OuterXml, "|TRO|" & TD.OuterXml)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try
                    End If
                ElseIf (String.Compare(TdContent, "rowend", True) = 0) Then
                    If (TD.ParentNode IsNot Nothing) Then
                        Try
                            TD.ParentNode.InnerXml = TD.ParentNode.InnerXml.Replace(TD.OuterXml, "|TRC|" & TD.OuterXml)
                        Catch ex As Exception
                            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                            Return False
                        End Try

                    End If
                End If
            Next
        Next
        Return True
    End Function

    Public Function RetaingInDesignTable() As Boolean
        Dim Tables As XmlNodeList = Nothing
        'Dim RetTable As XmlNode = Nothing
        Dim TblCRow As String = String.Empty
        Dim TblCData As String = String.Empty
        Dim TblCBody As String = String.Empty
        Dim TblCHead As String = String.Empty
        Dim TotalRowSpan As Integer = 0
        Dim TblRoot As String = String.Empty
        Dim RowCount As Integer = 0
        Dim CompleteRow As Integer = 0
        Dim ColumnCount As Integer = 0
        Dim OrgColumnCount As Integer = 0
        Dim TotalTDCount As Integer = 0
        Dim TblBody As XmlNode = Nothing
        Dim TblNodeList As XmlNodeList = Nothing
        Dim RowSpanCount As Int32 = 0
        Dim ColSpanCount As Int32 = 0
        Dim TablrRow As XmlNode = Nothing
        Dim TblHeadStyle As String = String.Empty
        Dim TblBodyStyle As String = String.Empty

        TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        Dim iCounter As Integer = 0
        For Each Tbl As XmlElement In Tables
            OrgColumnCount = 0
            Try
                RowCount = Tbl.GetAttribute("aid:trows")
            Catch ex As Exception
                Dim TmGroup As XmlNode = Nothing
                If (String.Compare(Tbl.Name, "table", True) = 0) Then
                    TmGroup = GetChildNode(Tbl, "tgroup")
                ElseIf (String.Compare(Tbl.Name, "tgroup", True) = 0) Then
                    TmGroup = Tbl.ParentNode
                End If
                If (TmGroup Is Nothing) Then
                    GBL.DeantaBallon("Could not able to find the aid:trows attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End If
                Try
                    RowCount = TmGroup.Attributes("aid:trows").Value
                Catch ex1 As Exception
                    GBL.DeantaBallon("Could not able to find the aid:trows attributes in table", MessageType.MSGERROR)
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End Try
            End Try
            Try
                ColumnCount = Tbl.GetAttribute("aid:tcols")
                OrgColumnCount = ColumnCount
            Catch ex As Exception
                Dim TmGroup As XmlNode = Nothing
                If (String.Compare(Tbl.Name, "table", True) = 0) Then
                    TmGroup = GetChildNode(Tbl, "tgroup")
                ElseIf (String.Compare(Tbl.Name, "tgroup", True) = 0) Then
                    TmGroup = Tbl.ParentNode
                End If
                If (TmGroup Is Nothing) Then
                    GBL.DeantaBallon("Could not able to find the aid:tcols attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End If
                Try
                    RowCount = TmGroup.Attributes("aid:tcols").Value
                Catch ex1 As Exception
                    GBL.DeantaBallon("Could not able to find the aid:tcols attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End Try
            End Try
            iCounter = 0
            RowSpanCount = 0
            'Dim TblTr As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
            'RetTable = XMLDoc.CreateNode(XmlNodeType.Element, "tgroup", "")
            'RetTable.PrependChild(TblTr)
            'TablrRow = XMLDoc.CreateNode(XmlNodeType.Element, TblCHead, "")
            TblNodeList = Tbl.SelectNodes($".//{TblCData}[not (@valign)]")
            Dim colspanAttrib As XmlAttribute = Nothing
            While ((TblNodeList IsNot Nothing) AndAlso (TblNodeList.Count > 0))
                Try
                    Dim tblNd As XmlNode = TblNodeList(0)
                    If (tblNd Is Nothing) Then Continue While
                    Dim comman As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "valign", "")
                    comman.Value = "bottom"
                    Dim comman1 As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "align", "")
                    comman1.Value = "center"
                    tblNd.Attributes.Append(comman)
                    tblNd.Attributes.Append(comman1)
                    If ((tblNd.Attributes("aid:ccols") Is Nothing)) Then
                        iCounter = iCounter + 1
                    Else
                        iCounter = iCounter + Convert.ToInt32(tblNd.Attributes("aid:ccols").Value)
                        If (Convert.ToInt32(tblNd.Attributes("aid:ccols").Value) > 1) Then
                            colspanAttrib = XMLDoc.CreateNode(XmlNodeType.Attribute, "colspan", "")
                            colspanAttrib.Value = Convert.ToInt32(tblNd.Attributes("aid:ccols").Value)
                            tblNd.Attributes.Append(colspanAttrib)
                        End If
                    End If
                    If (tblNd.Attributes("aid:crows") Is Nothing) Then
                        RowSpanCount = RowSpanCount + 1
                    Else
                        If (Convert.ToInt32(tblNd.Attributes("aid:crows").Value) > 1) Then
                            RowSpanCount = RowSpanCount + (Convert.ToInt32(tblNd.Attributes("aid:crows").Value))
                            TotalRowSpan = TotalRowSpan + 1
                            If ((tblNd.Attributes IsNot Nothing) AndAlso (tblNd.Attributes("rowspan") IsNot Nothing)) Then
                                colspanAttrib = tblNd.Attributes("rowspan")
                            Else
                                colspanAttrib = XMLDoc.CreateNode(XmlNodeType.Attribute, "rowspan", "")
                                tblNd.Attributes.Append(colspanAttrib)
                            End If
                            colspanAttrib.Value = Convert.ToInt32(tblNd.Attributes("aid:crows").Value)
                        End If
                    End If
                    If ((tblNd.Attributes("aid:ccols") IsNot Nothing) AndAlso (tblNd.Attributes("aid:crows") IsNot Nothing)) Then
                        If ((Convert.ToInt32(tblNd.Attributes("aid:crows").Value) > 1) And (Convert.ToInt32(tblNd.Attributes("aid:ccols").Value) > 1)) Then
                            ColSpanCount = ColSpanCount + (Convert.ToInt32(tblNd.Attributes("aid:crows").Value) - 1)
                        End If
                    End If
                    'If (iCounter < ColumnCount) Then
                    '    TblTr.AppendChild(tblNd)
                    'Else
                    '    TblTr.AppendChild(tblNd)
                    '    RetTable.AppendChild(TblTr)
                    '    TblTr = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
                    '    If (RowSpanCount > 0) Then
                    '        RowSpanCount = (RowSpanCount - TotalRowSpan)
                    '    End If

                    '    If (RowSpanCount <> 0) Then
                    '        If ((TotalRowSpan + ColSpanCount) > 0) Then
                    '            ColumnCount = (OrgColumnCount - (TotalRowSpan + ColSpanCount))
                    '        End If
                    '        iCounter = 0
                    '        'ColSpanCount = 0
                    '    Else
                    '        iCounter = 0
                    '        ColumnCount = OrgColumnCount
                    '        'ColSpanCount = 0
                    '    End If
                    '    If (RowSpanCount = 0) Then
                    '        TotalRowSpan = 0
                    '        'ColSpanCount = 0
                    '    End If
                    '    ColSpanCount = 0
                    '    If (TotalRowSpan > 0) Then
                    '        TotalRowSpan = TotalRowSpan - 1
                    '    End If
                    '    'Continue While
                    'End If
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                Finally
                    TblNodeList = Tbl.SelectNodes($".//{TblCData}[not (@valign)]")
                End Try
            End While
            'RetTable.AppendChild(TblTr)
            AddTableHeadandBody(XMLDoc, Tbl, TblCHead, TblCBody, TblCRow)
            'Tbl.AppendChild(TablrRow)
        Next
        Return True
    End Function

    Public Function odl_RetaingInDesignTable() As Boolean
        Dim Tables As XmlNodeList = Nothing
        'Dim RetTable As XmlNode = Nothing
        Dim TblCRow As String = String.Empty
        Dim TblCData As String = String.Empty
        Dim TblCBody As String = String.Empty
        Dim TblCHead As String = String.Empty
        Dim TotalRowSpan As Integer = 0
        Dim TblRoot As String = String.Empty
        Dim RowCount As Integer = 0
        Dim CompleteRow As Integer = 0
        Dim ColumnCount As Integer = 0
        Dim OrgColumnCount As Integer = 0
        Dim TotalTDCount As Integer = 0
        Dim TblBody As XmlNode = Nothing
        Dim TblNodeList As XmlNodeList = Nothing
        Dim RowSpanCount As Int32 = 0
        Dim ColSpanCount As Int32 = 0
        Dim TablrRow As XmlNode = Nothing
        Dim TblHeadStyle As String = String.Empty
        Dim TblBodyStyle As String = String.Empty
        If (DocType = DocumentType.JOURNAL) Then
            TblCRow = "tr"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "td"
            Tables = XMLDoc.SelectNodes("//table")
            TblHeadStyle = "TCH"
            TblBodyStyle = "TB"
        ElseIf ((DocType = DocumentType.BOOK) Or (DocType = DocumentType.RL) Or (DocType = DocumentType.SEQUOIA) Or (DocType = DocumentType.BDS) Or (DocType = DocumentType.MUP)) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        ElseIf (DocType = DocumentType.TANDF) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        End If
        Dim iCounter As Integer = 0
        For Each Tbl As XmlElement In Tables
            OrgColumnCount = 0
            Try
                RowCount = Tbl.GetAttribute("aid:trows")
            Catch ex As Exception
                Dim TmGroup As XmlNode = Nothing
                If (String.Compare(Tbl.Name, "table", True) = 0) Then
                    TmGroup = GetChildNode(Tbl, "tgroup")
                ElseIf (String.Compare(Tbl.Name, "tgroup", True) = 0) Then
                    TmGroup = Tbl.ParentNode
                End If
                If (TmGroup Is Nothing) Then
                    GBL.DeantaBallon("Could not able to find the aid:trows attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End If
                Try
                    RowCount = TmGroup.Attributes("aid:trows").Value
                Catch ex1 As Exception
                    GBL.DeantaBallon("Could not able to find the aid:trows attributes in table", MessageType.MSGERROR)
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End Try
            End Try
            Try
                ColumnCount = Tbl.GetAttribute("aid:tcols")
                OrgColumnCount = ColumnCount
            Catch ex As Exception
                Dim TmGroup As XmlNode = Nothing
                If (String.Compare(Tbl.Name, "table", True) = 0) Then
                    TmGroup = GetChildNode(Tbl, "tgroup")
                ElseIf (String.Compare(Tbl.Name, "tgroup", True) = 0) Then
                    TmGroup = Tbl.ParentNode
                End If
                If (TmGroup Is Nothing) Then
                    GBL.DeantaBallon("Could not able to find the aid:tcols attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End If
                Try
                    RowCount = TmGroup.Attributes("aid:tcols").Value
                Catch ex1 As Exception
                    GBL.DeantaBallon("Could not able to find the aid:tcols attributes in table", MessageType.MSGERROR)
                    Return False
#If CONFIG = "Release" Then
                    Return False
#Else
                    Continue For
#End If
                End Try
            End Try
            iCounter = 0
            RowSpanCount = 0
            'Dim TblTr As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
            'RetTable = XMLDoc.CreateNode(XmlNodeType.Element, "tgroup", "")
            'RetTable.PrependChild(TblTr)
            'TablrRow = XMLDoc.CreateNode(XmlNodeType.Element, TblCHead, "")
            TblNodeList = Tbl.SelectNodes($".//{TblCData}[not (@valign)]")
            Dim colspanAttrib As XmlAttribute = Nothing
            While ((TblNodeList IsNot Nothing) AndAlso (TblNodeList.Count > 0))
                Dim tblNd As XmlNode = TblNodeList(0)
                If (tblNd Is Nothing) Then Continue While
                Dim comman As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "valign", "")
                comman.Value = "bottom"
                Dim comman1 As XmlAttribute = XMLDoc.CreateNode(XmlNodeType.Attribute, "align", "")
                comman1.Value = "center"
                tblNd.Attributes.Append(comman)
                tblNd.Attributes.Append(comman1)
                If ((tblNd.Attributes("aid:ccols") Is Nothing)) Then
                    iCounter = iCounter + 1
                Else
                    iCounter = iCounter + Convert.ToInt32(tblNd.Attributes("aid:ccols").Value)
                    If (Convert.ToInt32(tblNd.Attributes("aid:ccols").Value) > 1) Then
                        colspanAttrib = XMLDoc.CreateNode(XmlNodeType.Attribute, "colspan", "")
                        colspanAttrib.Value = Convert.ToInt32(tblNd.Attributes("aid:ccols").Value)
                        tblNd.Attributes.Append(colspanAttrib)
                    End If
                End If
                If (tblNd.Attributes("aid:crows") Is Nothing) Then
                    RowSpanCount = RowSpanCount + 1
                Else
                    If (Convert.ToInt32(tblNd.Attributes("aid:crows").Value) > 1) Then
                        RowSpanCount = RowSpanCount + (Convert.ToInt32(tblNd.Attributes("aid:crows").Value))
                        TotalRowSpan = TotalRowSpan + 1
                        If ((tblNd.Attributes IsNot Nothing) AndAlso (tblNd.Attributes("rowspan") IsNot Nothing)) Then
                            colspanAttrib = tblNd.Attributes("rowspan")
                        Else
                            colspanAttrib = XMLDoc.CreateNode(XmlNodeType.Attribute, "rowspan", "")
                            tblNd.Attributes.Append(colspanAttrib)
                        End If
                        colspanAttrib.Value = Convert.ToInt32(tblNd.Attributes("aid:crows").Value)
                    End If
                End If
                If ((tblNd.Attributes("aid:ccols") IsNot Nothing) AndAlso (tblNd.Attributes("aid:crows") IsNot Nothing)) Then
                    If ((Convert.ToInt32(tblNd.Attributes("aid:crows").Value) > 1) And (Convert.ToInt32(tblNd.Attributes("aid:ccols").Value) > 1)) Then
                        ColSpanCount = ColSpanCount + (Convert.ToInt32(tblNd.Attributes("aid:crows").Value) - 1)
                    End If
                End If
                'If (iCounter < ColumnCount) Then
                '    TblTr.AppendChild(tblNd)
                'Else
                '    TblTr.AppendChild(tblNd)
                '    RetTable.AppendChild(TblTr)
                '    TblTr = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
                '    If (RowSpanCount > 0) Then
                '        RowSpanCount = (RowSpanCount - TotalRowSpan)
                '    End If

                '    If (RowSpanCount <> 0) Then
                '        If ((TotalRowSpan + ColSpanCount) > 0) Then
                '            ColumnCount = (OrgColumnCount - (TotalRowSpan + ColSpanCount))
                '        End If
                '        iCounter = 0
                '        'ColSpanCount = 0
                '    Else
                '        iCounter = 0
                '        ColumnCount = OrgColumnCount
                '        'ColSpanCount = 0
                '    End If
                '    If (RowSpanCount = 0) Then
                '        TotalRowSpan = 0
                '        'ColSpanCount = 0
                '    End If
                '    ColSpanCount = 0
                '    If (TotalRowSpan > 0) Then
                '        TotalRowSpan = TotalRowSpan - 1
                '    End If
                '    'Continue While
                'End If
                TblNodeList = Tbl.SelectNodes($".//{TblCData}[not (@valign)]")
            End While
            'RetTable.AppendChild(TblTr)
            AddTableHeadandBody(XMLDoc, Tbl, TblCHead, TblCBody, TblCRow)
            'Tbl.AppendChild(TablrRow)
        Next
        Return True
    End Function

    Private Function AddTableHeadandBody(ByVal xmlDoc As XmlDocument, ByVal Tbl As XmlNode, ByVal TblHeadName As String, ByVal TblBodyName As String, ByVal TblRow As String) As Boolean
        If (Tbl Is Nothing) Then Return False
        Dim RowList As XmlNode = Nothing
        Dim iWithoutColCount As Integer = 1
        Dim Innerxml As String = String.Empty
        Try
            If ((Tbl.Attributes("aid:tcols").Value = 1) AndAlso (Regex.Matches(Tbl.InnerXml, "<row>").Count = 1)) Then
                Tbl.InnerXml = Tbl.InnerXml.Replace("</entry><entry", "</entry></row><row><entry")
            End If
        Catch ex As Exception
            GBL.DeantaBallon($"{ex.Message} - {ex.StackTrace}", MessageType.MSGERROR)
        End Try

        Dim TblHeadNode As XmlNode = xmlDoc.CreateElement(XmlNodeType.Element, TblHeadName, "")
        Dim TblBodyNode As XmlNode = xmlDoc.CreateElement(XmlNodeType.Element, TblBodyName, "")
        While (Tbl.SelectNodes(".//" & TblRow).Count > 0)
            Dim tblNd As XmlNode = Tbl.SelectNodes(".//" & TblRow)(0)
            If ((tblNd IsNot Nothing) AndAlso (tblNd.HasChildNodes)) Then
                If (tblNd.OuterXml.Contains("aid:pstyle=""TCH")) Then
                    'If ((tblNd.ChildNodes(0).Attributes("aid:pstyle") IsNot Nothing) AndAlso (tblNd.ChildNodes(0).Attributes("aid:pstyle").Value.StartsWith("TCH"))) Then
                    TblHeadNode.AppendChild(tblNd)
                Else
                    TblBodyNode.AppendChild(tblNd)
                End If
            End If
        End While
        Dim iColCount As Integer = 0
        If ((TblBodyNode.ChildNodes IsNot Nothing) AndAlso (TblBodyNode.ChildNodes.Count > 0)) Then
            RowList = TblBodyNode.ChildNodes(0)
        End If
        If ((RowList Is Nothing) AndAlso (TblHeadNode.ChildNodes IsNot Nothing) AndAlso (TblHeadNode.ChildNodes.Count > 0)) Then
                RowList = TblHeadNode.ChildNodes(0)
            End If
            If ((RowList IsNot Nothing) AndAlso (RowList.ChildNodes IsNot Nothing) AndAlso (RowList.ChildNodes.Count > 0)) Then
            For Each RwNode As XmlNode In RowList.ChildNodes
                If (RwNode.NodeType <> XmlNodeType.Element) Then Continue For
                If ((RwNode.Attributes("aid:ccols") IsNot Nothing) AndAlso (Convert.ToInt32(RwNode.Attributes("aid:ccols").Value) > 1)) Then
                    iColCount = IIf(iColCount = 0, Convert.ToInt32(RwNode.Attributes("aid:ccols").Value), iColCount + Convert.ToInt32(RwNode.Attributes("aid:ccols").Value))
                Else
                    iColCount = iColCount + 1
                End If
            Next
        End If
            If (Not Tbl.InnerXml.Contains("<colspec")) Then
                For tl As Integer = 1 To iColCount
                    Innerxml = IIf(String.IsNullOrEmpty(Innerxml), "<colspec colnum=""" & tl & """ colname=""col" & tl & """ align=""left""/>", Innerxml & "<colspec colnum=""" & tl & """ colname=""col" & tl & """ align=""left""/>")
                Next
                Tbl.InnerXml = Tbl.InnerXml & Innerxml
            End If
        Tbl.AppendChild(TblHeadNode)
        Tbl.AppendChild(TblBodyNode)
        Dim Colatt As XmlAttribute = xmlDoc.CreateAttribute("cols", "")
        Colatt.Value = iColCount
        Tbl.Attributes.Append(Colatt)
        Return True
    End Function

    Public Function TableCleanUp() As Boolean
        Dim Tables As XmlNodeList = Nothing
        Dim TblCRow As String = String.Empty
        Dim TblCData As String = String.Empty
        Dim TblCBody As String = String.Empty
        Dim TblCHead As String = String.Empty
        Dim TblRoot As String = String.Empty
        Dim RowCount As Integer = 0
        Dim CompleteRow As Integer = 0
        Dim ColumnCount As Integer = 0
        Dim TotalTDCount As Integer = 0
        Dim TblBody As XmlNode = Nothing
        Dim TablrRow As XmlNode = Nothing
        If (DocType = LanstadClientType.JOURNAL) Then
            TblCRow = "tr"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "td"
            Tables = XMLDoc.SelectNodes("//table")
        ElseIf ((DocType = LanstadClientType.BOOK) Or (DocType = LanstadClientType.RANDL)) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//table")
        ElseIf ((DocType = LanstadClientType.TANDF) Or (DocType = DocumentType.TANDFUK)) Then
            TblCRow = "row"
            TblCBody = "tbody"
            TblCHead = "thead"
            TblCData = "entry"
            Tables = XMLDoc.SelectNodes("//tgroup")
        End If
        If ((Tables Is Nothing) OrElse (Tables.Count = 0)) Then Return False
        For Each Tbl As XmlElement In Tables
            RowCount = Tbl.GetAttribute("aid:trows")
            ColumnCount = Tbl.GetAttribute("aid:tcols")
            TotalTDCount = (RowCount * ColumnCount)
            TblBody = XMLDoc.CreateNode(XmlNodeType.Element, "tbody", "")
            If (TotalTDCount <> Tbl.ChildNodes.Count) Then
                GBL.DeantaBallon("Table row count is mismatched." & "Expected :" & TotalTDCount & " Actual :" & Tbl.ChildNodes.Count, MessageType.MSGERROR)
                Return False
            End If
            CompleteRow = 0
            While (True)
                Dim TableData As XmlNodeList = Tbl.SelectNodes("./" & TblCData & "[position()<=" & ColumnCount & "]")
                TablrRow = Nothing
                If ((TableData Is Nothing) OrElse (TableData.Count = 0)) Then Exit While
                If (CompleteRow = 0) Then
                    TablrRow = XMLDoc.CreateNode(XmlNodeType.Element, "thead", "")
                    Dim Tr As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
                    TablrRow = TablrRow.PrependChild(Tr)
                Else
                    TablrRow = XMLDoc.CreateNode(XmlNodeType.Element, TblCRow, "")
                    TablrRow = TblBody.AppendChild(TablrRow)
                End If
                For Each Data As XmlNode In TableData
                    If (CompleteRow = 0) Then
                        Dim TblTh As XmlNode = XMLDoc.CreateNode(XmlNodeType.Element, TblCData, "")
                        TblTh.InnerXml = Data.InnerXml
                        TablrRow.AppendChild(TblTh)
                        Data.ParentNode.RemoveChild(Data)
                    Else
                        TablrRow.AppendChild(Data)
                    End If
                Next
                If (CompleteRow = 0) Then
                    Tbl.AppendChild(TablrRow.ParentNode)
                    'ElseIf (CompleteRow = 1) Then
                    '    Tbl.AppendChild(TablrRow.ParentNode)
                    'Else
                    '    Tbl.AppendChild(TablrRow)
                End If

                CompleteRow += 1
            End While
            Tbl.AppendChild(TblBody)
        Next
        Return True
    End Function

    ' Ashok Aug 02, 2016
    ' Cleanup for books
    Private iChap As Integer = 0, iHead As Integer = 0

    Private Function CleanupPro(sXMLContent As String) As String

        sXMLContent = Regex.Replace(sXMLContent, "<preface([^><]+)?>((?:(?!</preface>).)+)</preface>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<part([^><]+)?>((?:(?!</part>).)+)</part>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<dedication([^><]+)?>((?:(?!</dedication>).)+)</dedication>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<toc([^><]+)?>((?:(?!</toc>).)+)</toc>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<acknowledgements([^><]+)?>((?:(?!</acknowledgements>).)+)</acknowledgements>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<index([^><]+)?>((?:(?!</index>).)+)</index>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<abbreviation([^><]+)?>((?:(?!</abbreviation>).)+)</abbreviation>", AddressOf PRefacePro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        sXMLContent = Regex.Replace(sXMLContent, "<chapter([^><]+)?>", AddressOf ChapterPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        iChap = 0
        sXMLContent = Regex.Replace(sXMLContent, "<chapter([^><]+)?>((?:(?!</chapter>).)+)</chapter>", AddressOf SectionPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        Return sXMLContent
    End Function

    Private Function SectionPro(m As Match)
        Dim sChapterEle As String = m.Value.ToString
        iChap += 1
        iHead = 0
        sChapterEle = Regex.Replace(sChapterEle, "<section([^><]+)?>", AddressOf HeadingPro, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Return sChapterEle
    End Function

    Private Function HeadingPro(m As Match)
        iHead += 1
        'Return "<section xml:id=""ch" & iChap & "-sec" & iHead & Chr(34) & ">"
        Return "<section xml:id=""sec" & iChap & "_" & iHead & Chr(34) & ">"
    End Function

    Private Function ChapterPro(m As Match)
        iChap += 1
        Return "<chapter label=" & Chr(34) & iChap & Chr(34) & " xml:id=""b-001-chapter" & iChap & Chr(34) & ">"
    End Function

    Private Function PRefacePro(m As Match)
        Dim pref As String = m.Value.ToString
        Dim sTitle As Match = Regex.Match(pref, "<title([^><]+)?>((?:(?!</title>).)+)</title>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        Dim TitleFinal As String = Regex.Replace(sTitle.Groups(2).Value.ToString, "<\?[^><]+\?>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        If String.IsNullOrEmpty(TitleFinal) Then TitleFinal = "notitle"
        If pref.ToString.ToLower.StartsWith("<preface") Then
            pref = Regex.Replace(pref, "<preface([^><]+)?>", "<preface xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<part") Then
            pref = Regex.Replace(pref, "<part([^><]+)?>", "<part xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<dedi") Then
            pref = Regex.Replace(pref, "<dedication([^><]+)?>", "<dedication xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<toc") Then
            pref = Regex.Replace(pref, "<toc([^><]+)?>", "<toc xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<ack") Then
            pref = Regex.Replace(pref, "<acknowledgements([^><]+)?>", "<acknowledgements xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<index") Then
            pref = Regex.Replace(pref, "<index([^><]+)?>", "<index xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        ElseIf pref.ToString.ToLower.StartsWith("<abb") Then
            pref = Regex.Replace(pref, "<abbreviation([^><]+)?>", "<abbreviation xml:id=""b-001-" & TitleFinal & """>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        End If
        Return pref
    End Function

    Public Sub TestPro()

        XMLString = File.ReadAllText("C:\Users\ashokd\Desktop\Cleanup\final.xml")
        CleanupPro(XMLString)
    End Sub

End Class

Public Class ExportFloatData

    Sub New(ByVal Type As FloatType, ByVal floatName As String, ByVal NameList As List(Of String))
        Me.FloatName = floatName
        Me.FltType = Type
        Me.ExportNames.AddRange(NameList)
    End Sub

    Public Property FloatName As String = String.Empty
    Public Property ExportNames As New List(Of String)
    Public Property FltType As FloatType

End Class

Public Enum FloatType
    NONE = 0
    TABLE = 1
    FIGURE = 2
    SIDEBAR = 3
End Enum

Public Class HeadingLevelData

    Public Property ClientName As String = String.Empty
    Public Property HeadLevel As Int32 = 0
    Public Property PossibleHeadNames As List(Of String)

End Class

Public Class TagtoTextData
    Public Property TagName As String = String.Empty
    Public Property TextName As String = String.Empty
End Class

Public Class UnwantedElementData
    Public Property ElementName As String = String.Empty
    Public Property AttributeName As String = String.Empty
    Public Property AttributeValue As String = String.Empty

    Public Property IsUnTag As Boolean = False

End Class