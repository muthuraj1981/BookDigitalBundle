Imports System.Windows.Forms

Namespace InDesign.Advanced

    Public Class XMLManager

        Public Sub New()

        End Sub

        Public Function GetXMLElementByName(ElementNames As List(Of String)) As List(Of Object)
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
                    If (String.Compare(MatchData.Element.MarkupTag.Name, "table", True) = 0) Then
                        If ((MatchData.Element.XmlAttributes IsNot Nothing) AndAlso (MatchData.Element.XmlAttributes.Count > 0)) Then
                            Dim AttribValue As String = GetAttributeValue(MatchData.Element, "role")
                            If (String.IsNullOrEmpty(AttribValue) AndAlso (String.Compare(AttribValue, "inline", True) = 0)) Then
                                MatchData = RuleProcesser.FindNextMatch()
                                Continue While
                            End If
                        End If
                    End If
                    If (IsElementExists(ElementNames, MatchData.Element.MarkupTag.Name.ToString())) Then
                        ElementList.Add(MatchData.Element)
                    End If
                    MatchData = RuleProcesser.FindNextMatch()
                    GC.Collect()
                End While
                Return ElementList
            Catch ex As Exception
                Return New List(Of Object)
            Finally
                RuleProcesser.EndProcessingRuleSet()
            End Try
        End Function

        Private Function IsElementExists(ElementList As List(Of String), Name As String) As Boolean
            Dim IsExits As Boolean = False
            IsExits = ElementList.Exists(Function(EleName As String)
                                             If (String.Compare(EleName, Name, True) = 0) Then
                                                 Return True
                                             End If
                                             Return False
                                         End Function)
            Return IsExits
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

        Public Function RemoveXMLElementByName(ElementName As String, ByVal Optional isUntag As Boolean = False) As List(Of Object)
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
                        If (isUntag) Then
                            MatchData.Element.select()
                            'INDDGBL.InDesignApp.MenuActions.itemByID("78612").Invoke()
                            INDDGBL.InDesignApp.DoScript("app.menuActions.itemByID(78612).invoke()", InDesignConstant.SCRIPTLANG_JAVASCRIPT)
                        Else
                            MatchData.Element.Delete()
                        End If
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

        Public Function RemoveXMLElementByAttribute(ByVal AttribName As String, ByVal AttribValue As String) As List(Of Object)
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
                    If ((MatchData.Element.XmlAttributes IsNot Nothing) AndAlso (MatchData.Element.XmlAttributes.Count > 0)) Then
                        For a As Int32 = 1 To MatchData.Element.XmlAttributes.Count
                            If ((Not String.IsNullOrEmpty(AttribName)) AndAlso (Not String.IsNullOrEmpty(AttribValue))) Then
                                If (String.Compare(MatchData.Element.XmlAttributes(a).Name, AttribName, True) = 0) Then
                                    If (String.Compare(MatchData.Element.XmlAttributes(a).value, AttribValue, True) = 0) Then
                                        MatchData.Element.XmlAttributes(a).delete()
                                    End If
                                End If
                            ElseIf ((Not String.IsNullOrEmpty(AttribName)) AndAlso (String.IsNullOrEmpty(AttribValue))) Then
                                If (String.Compare(MatchData.Element.XmlAttributes(a).Name, AttribName, True) = 0) Then
                                    MatchData.Element.XmlAttributes(a).delete()
                                End If
                            ElseIf ((String.IsNullOrEmpty(AttribName)) AndAlso (Not String.IsNullOrEmpty(AttribValue))) Then
                                If (String.Compare(MatchData.Element.XmlAttributes(a).value, AttribValue, True) = 0) Then
                                    MatchData.Element.XmlAttributes(a).delete()
                                End If
                            End If
                        Next
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

        Public Function GetXMLElementByAttribute(ByVal ElementName As String, ByVal AttribName As String, ByVal AttribValue As String) As List(Of Object)
            Dim ElementList As New List(Of Object)
            Dim XPath As String = $"//{ElementName}"
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
                    If ((MatchData.Element.XmlAttributes IsNot Nothing) AndAlso (MatchData.Element.XmlAttributes.Count > 0)) Then
                        For a As Int32 = 1 To MatchData.Element.XmlAttributes.Count
                            If (String.Compare(MatchData.Element.XmlAttributes(a).Name, AttribName, True) = 0) Then
                                If (String.Compare(MatchData.Element.XmlAttributes(a).value, AttribValue, True) = 0) Then
                                    ElementList.Add(MatchData.Element)
                                End If
                            End If
                        Next
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

        Private Function GetAttributeValue(Element As Object, AttributeName As String) As String
            If ((Element.XmlAttributes IsNot Nothing) AndAlso (Element.XmlAttributes.Count > 0)) Then
                For Each ElAtt As Object In Element.XmlAttributes
                    If (String.Compare(ElAtt.Name, AttributeName, False) = 0) Then
                        Return ElAtt.Value
                    End If
                Next
            End If
            Return String.Empty
        End Function

        Public Function GetXmlComments() As List(Of Object)
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
                    If ((MatchData.Element.XmlComments IsNot Nothing) AndAlso (MatchData.Element.XmlComments.Count() > 0)) Then
                        For cmt As Integer = 1 To MatchData.Element.XmlComments.Count
                            ElementList.Add(MatchData.Element.XmlComments(cmt))
                        Next
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

        Public Function GetXMLElementByXPath(XPath As String) As List(Of Object)
            Dim ElementList As New List(Of Object)
            'Dim XPath As String = "//*"
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
                    ElementList.Add(MatchData.Element)
                    MatchData = RuleProcesser.FindNextMatch()
                End While
                Return ElementList
            Catch ex As Exception
                Return New List(Of Object)
            Finally
                RuleProcesser.EndProcessingRuleSet()
            End Try
        End Function

        Public Function GetXMLElementByXPath(ByVal source As Object, ByVal XPath As String) As List(Of Object)
            Dim ElementList As New List(Of Object)
            'Dim XPath As String = "//*"
            Dim RuleProcesser As Object = Nothing
            Dim MatchData As Object = Nothing
            Dim Elemnet As Object = Nothing
            If (INDDGBL.InDesignDoc Is Nothing) Then
                Return New List(Of Object)
            End If
            RuleProcesser = INDDGBL.InDesignApp.XMLRuleProcessors.Add(New String() {XPath})
            MatchData = RuleProcesser.StartProcessingRuleSet(source)
            Try
                While (MatchData IsNot Nothing)
                    ElementList.Add(MatchData.Element)
                    MatchData = RuleProcesser.FindNextMatch()
                End While
                Return ElementList
            Catch ex As Exception
                Return New List(Of Object)
            Finally
                RuleProcesser.EndProcessingRuleSet()
            End Try
        End Function

    End Class

End Namespace

