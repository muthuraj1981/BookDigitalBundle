Imports System.Xml
Imports System.Runtime.CompilerServices

Module XMLExtension

    <Extension()>
    Public Function ReplaceFirstOccur(ByVal self As String, ByVal oldString As String, ByVal newString As String, ByVal Optional firstOccurrenceOnly As Boolean = False) As String
        If Not firstOccurrenceOnly Then Return self.Replace(oldString, newString)
        Dim pos As Integer = self.IndexOf(oldString)
        If pos < 0 Then Return self
        Return self.Substring(0, pos) & newString & self.Substring(pos + oldString.Length)
    End Function

    <Extension>
    Public Function ReplaceLastOccurrence(ByVal Source As String, ByVal Find As String, ByVal Replace As String) As String
        Dim place As Int16 = Source.LastIndexOf(Find)
        If (place = -1) Then
            Return Source
        End If
        Dim result As String = Source.Remove(place, Find.Length).Insert(place, Replace)
        Return result
    End Function

    <Extension()>
    Public Function RemoveXMLNode(ByVal XmlDoc As XmlDocument, ByVal XPath As String, Optional Choice As NodeMoveOption = NodeMoveOption.NONE, Optional ByVal xmlContent As String = "")
        Try
            Dim EmptyNodes As XmlNodeList = XmlDoc.SelectNodes(XPath)
            If ((EmptyNodes IsNot Nothing) AndAlso (EmptyNodes.Count > 0)) Then
                For f As Int16 = 0 To EmptyNodes.Count - 1
                    Select Case Choice
                        Case NodeMoveOption.EMPTYTEXT
                            If (String.IsNullOrEmpty(EmptyNodes(f).InnerText)) Then
                                If (EmptyNodes(f).ParentNode IsNot Nothing) Then
                                    EmptyNodes(f).ParentNode.RemoveChild(EmptyNodes(f))
                                End If
                            End If
                        Case NodeMoveOption.NONE
                            If (EmptyNodes(f).ParentNode IsNot Nothing) Then
                                EmptyNodes(f).ParentNode.RemoveChild(EmptyNodes(f))
                            End If
                        Case NodeMoveOption.TEXTCONTAINS
                            If (Not String.IsNullOrEmpty(EmptyNodes(f).InnerText)) And (Not String.IsNullOrEmpty(xmlContent)) And (EmptyNodes(f).InnerText.Contains(xmlContent)) Then
                                If (EmptyNodes(f).ParentNode IsNot Nothing) Then
                                    EmptyNodes(f).ParentNode.RemoveChild(EmptyNodes(f))
                                End If
                            End If
                        Case NodeMoveOption.ENDSWITH
                            If (Not String.IsNullOrEmpty(EmptyNodes(f).InnerText)) And (Not String.IsNullOrEmpty(xmlContent)) And (EmptyNodes(f).InnerText.EndsWith(xmlContent)) Then
                                If (EmptyNodes(f).ParentNode IsNot Nothing) Then
                                    EmptyNodes(f).ParentNode.RemoveChild(EmptyNodes(f))
                                End If
                            End If
                    End Select
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    <Extension()>
    Public Function UpdateInnerText(ByVal xmlDoc As XmlDocument, ByVal xPath As String, ByVal InnerText As String)
        Try
            Dim Fpage As XmlNode = xmlDoc.SelectSingleNode(xPath)
            If (Fpage IsNot Nothing) Then
                Fpage.InnerText = InnerText
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function


    <Extension()>
    Public Function TrimInnerText(ByVal xmlDoc As XmlDocument, ByVal xPath As String)
        Try
            Dim Fpage As XmlNodeList = xmlDoc.SelectNodes(xPath)
            For t As Int16 = 0 To Fpage.Count - 1
                Fpage(t).InnerXml = Fpage(t).InnerXml.Trim()
            Next
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    <Extension()>
    Public Function MoveXMLNode(ByVal xmlDoc As XmlDocument, ByVal SourcexPath As String, ByVal DestxPath As String, ByVal Choice As NodeMoveOption) As Boolean
        Dim SourceNode As XmlNode = Nothing
        Dim DestNode As XmlNode = Nothing
        SourceNode = xmlDoc.SelectSingleNode(SourcexPath)
        If (SourceNode Is Nothing) Then
            Return False
        End If
        DestNode = xmlDoc.SelectSingleNode(DestxPath)
        If (DestNode Is Nothing) Then
            Return False
        End If
        If (DestNode.ParentNode Is Nothing) Then
            Return False
        End If
        Select Case Choice
            Case NodeMoveOption.MOVEAFTER
                DestNode.ParentNode.InsertAfter(SourceNode, DestNode)
            Case NodeMoveOption.MOVEBEFORE
                DestNode.ParentNode.InsertBefore(SourceNode, DestNode)
            Case NodeMoveOption.FIRSTCHILD
                DestNode.PrependChild(SourceNode)
            Case NodeMoveOption.LASTCHILD
                DestNode.AppendChild(SourceNode)
        End Select
        Return True
    End Function

    <Extension()>
    Public Function NumberToText(ByVal n As Integer) As String
        Select Case n
            Case 0
                Return ""

            Case 1 To 19
                Dim arr() As String = {"One", "Two", "Three", "Four", "Five", "Six", "Seven",
                  "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen",
                    "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"}
                Return arr(n - 1) & " "

            Case 20 To 99
                Dim arr() As String = {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"}
                Return arr(n \ 10 - 2) & " " & NumberToText(n Mod 10)

            Case 100 To 199
                Return "One Hundred " & NumberToText(n Mod 100)

            Case 200 To 999
                Return NumberToText(n \ 100) & "Hundreds " & NumberToText(n Mod 100)

            Case 1000 To 1999
                Return "One Thousand " & NumberToText(n Mod 1000)

            Case 2000 To 999999
                Return NumberToText(n \ 1000) & "Thousands " & NumberToText(n Mod 1000)

            Case 1000000 To 1999999
                Return "One Million " & NumberToText(n Mod 1000000)

            Case 1000000 To 999999999
                Return NumberToText(n \ 1000000) & "Millions " & NumberToText(n Mod 1000000)

            Case 1000000000 To 1999999999
                Return "One Billion " & NumberToText(n Mod 1000000000)

            Case Else
                Return NumberToText(n \ 1000000000) & "Billion " _
                  & NumberToText(n Mod 1000000000)
        End Select
    End Function

    <Extension()>
    Public Function RemoveXMLNode(ByVal XmlDoc As XmlDocument, ByVal XPath As String)
        Try
            Dim Fpage As XmlNode = XmlDoc.SelectSingleNode(XPath)
            If ((Fpage IsNot Nothing) AndAlso (Fpage.ParentNode IsNot Nothing)) Then
                Fpage.ParentNode.RemoveChild(Fpage)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    <Extension()>
    Public Function RemoveXMLNodes(ByVal XmlDoc As XmlDocument, ByVal XPath As String)
        Try
            Dim Fpage As XmlNodeList = XmlDoc.SelectNodes(XPath)
            If ((Fpage IsNot Nothing) AndAlso (Fpage.Count > 0)) Then
                For t As Int16 = 0 To Fpage.Count - 1
                    If (Fpage(t).ParentNode IsNot Nothing) Then
                        Fpage(t).ParentNode.RemoveChild(Fpage(t))
                    End If
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    <Extension()>
    Public Function RemoveXMLNodeOnly(ByVal XmlDoc As XmlDocument, ByVal XPath As String)
        Try
            Dim Fpage As XmlNodeList = XmlDoc.SelectNodes(XPath)
            If ((Fpage IsNot Nothing) AndAlso (Fpage.Count > 0)) Then
                For t As Int16 = 0 To Fpage.Count - 1
                    If (Fpage(t).ParentNode IsNot Nothing) Then
                        Fpage(t).ParentNode.InnerXml = Fpage(t).ParentNode.InnerXml.Replace(Fpage(t).OuterXml, Fpage(t).InnerXml)
                    End If
                Next
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function


    <Extension()>
    Public Function ChangeAttributeValue(ByVal xmlDoc As XmlDocument, ByVal xPath As String, ByVal AttribName As String, ByVal AttribValue As String) As Boolean
        If (xmlDoc Is Nothing) Then Return False
        Dim xmlAttrib As XmlAttribute = Nothing
        Dim Nodes As XmlNodeList = xmlDoc.SelectNodes(xPath)
        If ((Nodes IsNot Nothing) AndAlso (Nodes.Count > 0)) Then
            For n As Int16 = 0 To Nodes.Count - 1
                Try
                    xmlAttrib = Nodes(n).Attributes(AttribName)
                Catch ex As Exception
                    xmlAttrib = Nothing
                End Try
                If (xmlAttrib IsNot Nothing) Then
                    xmlAttrib.Value = AttribValue
                End If
            Next
        End If
        Return True
    End Function

    <Extension()>
    Public Function AddXMLChildNode(ByVal xmlDoc As XmlDocument, ByVal xPath As String, ByVal NodeName As String, ByVal InnerXML As String, ByVal Position As NodeMoveOption) As Boolean
        Dim NewNode As XmlNode = Nothing
        Dim ParNodes As XmlNodeList = xmlDoc.SelectNodes(xPath)
        If ((ParNodes IsNot Nothing) AndAlso (ParNodes.Count > 0)) Then
            For p As Int16 = 0 To ParNodes.Count - 1
                If ((Position = NodeMoveOption.LASTCHILD) Or (Position = NodeMoveOption.FIRSTCHILD)) Then
                    If (ParNodes(p).SelectNodes($".//{NodeName}").Count > 0) Then
                        Continue For
                    End If
                End If

                NewNode = xmlDoc.CreateNode(XmlNodeType.Element, NodeName, "")
                NewNode.InnerXml = InnerXML
                Select Case Position
                    Case NodeMoveOption.FIRSTCHILD
                        ParNodes(p).InsertBefore(NewNode, ParNodes(p).FirstChild)
                    Case NodeMoveOption.LASTCHILD
                        ParNodes(p).AppendChild(NewNode)
                    Case NodeMoveOption.MOVEAFTER
                        If (ParNodes(p).ParentNode IsNot Nothing) Then
                            ParNodes(p).ParentNode.InsertAfter(NewNode, ParNodes(p))
                        End If
                    Case NodeMoveOption.MOVEBEFORE
                        If (ParNodes(p).ParentNode IsNot Nothing) Then
                            ParNodes(p).ParentNode.InsertBefore(NewNode, ParNodes(p))
                        End If
                End Select
            Next
        End If
        Return True
    End Function

    <Extension()>
    Public Function UnTagXMLNode(ByVal xmlDoc As XmlDocument, ByVal xPath As String) As Boolean
        Dim NewNode As XmlNode = Nothing
        Dim ParNodes As XmlNodeList = xmlDoc.SelectNodes(xPath)
        If ((ParNodes IsNot Nothing) AndAlso (ParNodes.Count > 0)) Then
            For p As Int16 = 0 To ParNodes.Count - 1
                If (ParNodes(p).ParentNode IsNot Nothing) Then
                    ParNodes(p).ParentNode.InnerXml = ParNodes(p).ParentNode.InnerXml.Replace(ParNodes(p).OuterXml, ParNodes(p).InnerXml)
                End If
            Next
        End If
        Return True
    End Function

    <Extension()>
    Public Function RemoveAttribute(ByVal xmlDoc As XmlDocument, ByVal xPath As String, attributeName As String) As Boolean
        Dim Attribs As XmlNodeList = xmlDoc.SelectNodes(xPath)
        If ((Attribs IsNot Nothing) AndAlso (Attribs.Count > 0)) Then
            For a As Int16 = 0 To Attribs.Count - 1
                Try
                    Attribs(a).Attributes.Remove(Attribs(a).Attributes(attributeName))
                Catch ex As Exception
                    Continue For
                End Try
            Next
        End If
        Return True
    End Function


    <Extension()>
    Public Function AddXMLAttribute(ByVal xmlDoc As XmlDocument, ByVal SourcexPath As String, ByVal AttribName As String, ByVal AttribValue As String, Optional ByVal Position As NodeMoveOption = NodeMoveOption.FIRSTCHILD) As Boolean
        Dim SourceNode As XmlNode = Nothing
        Dim NewAttrib As XmlAttribute = Nothing
        Dim SourList As XmlNodeList = xmlDoc.SelectNodes(SourcexPath)
        If ((SourList IsNot Nothing) AndAlso (SourList.Count > 0)) Then
            For s As Int16 = 0 To SourList.Count - 1
                SourceNode = SourList(s)
                If (SourceNode IsNot Nothing) Then
                    Try
                        NewAttrib = SourceNode.Attributes(AttribName)
                    Catch ex As Exception
                        NewAttrib = xmlDoc.CreateNode(XmlNodeType.Attribute, AttribName, "")
                    End Try
                    If (NewAttrib IsNot Nothing) Then
                        NewAttrib.Value = AttribValue
                    Else
                        NewAttrib = xmlDoc.CreateNode(XmlNodeType.Attribute, AttribName, "")
                        NewAttrib.Value = AttribValue
                    End If
                    Select Case Position
                        Case NodeMoveOption.FIRSTCHILD
                            SourceNode.Attributes.Prepend(NewAttrib)
                        Case NodeMoveOption.LASTCHILD
                            If ((SourceNode.Attributes Is Nothing) OrElse (SourceNode.Attributes.Count = 0)) Then
                                SourceNode.Attributes.Prepend(NewAttrib)
                            Else
                                SourceNode.Attributes.Append(NewAttrib)
                            End If
                    End Select
                End If
            Next
        End If
        Return True
    End Function
End Module

