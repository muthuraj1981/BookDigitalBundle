Imports System.IO
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Imports System.Net.WebException
Imports System.Net.Mail
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security

Public Class MailHelper

    Public Shared Property NetCredential As New System.Net.NetworkCredential("engines@deantaglobal.com", "RRK!Gio8_bUB")

    Public Shared Function SendMail(ByVal Subject As String, ByVal MailTo As String, MailCc As String, ByVal Message As String, Optional IncludeErrorLog As Boolean = True) As Boolean
        Dim MsgBody As String = String.Empty
        Dim FileList As String = String.Empty
        Dim MsgSubject As String = String.Empty
        Dim NoofDate As String = String.Empty
        Dim SmtpServer As New SmtpClient()
        Dim mail As New MailMessage()
        Try
            SmtpServer.UseDefaultCredentials = False
            SmtpServer.Credentials = NetCredential
            SmtpServer.Port = 587
            SmtpServer.Host = "server1.deantaglobal.com"
            SmtpServer.EnableSsl = True
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network
            ServicePointManager.ServerCertificateValidationCallback = Function(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) True
            mail = New MailMessage()
            mail.IsBodyHtml = True
            mail.Priority = MailPriority.High
            mail.From = New MailAddress("engines@deantaglobal.com", "InDesign server engine")
            If (Not String.IsNullOrEmpty(MailTo)) Then
                mail.To.Add(GBL.SupportMailID)
                mail.To.Add(MailTo)
            Else
                mail.To.Add(GBL.SupportMailID)
            End If
            If (MailCc.Contains(",")) Then
                For Each MTo As String In MailCc.Split(",")
                    mail.CC.Add(MTo)
                Next
            Else
                mail.CC.Add(MailCc)
            End If

            mail.Subject = Subject
            MsgBody = ConvertHTML(Message, IncludeErrorLog)
            mail.Body = MsgBody
            Try
                SmtpServer.Send(mail)
            Catch ex As Exception
                Return False
            Finally
                If (mail IsNot Nothing) Then
                    mail.Dispose()
                End If
            End Try
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Public Shared Function SendMail(ByVal ProjectID As String, FileName As String, Optional IsError As Boolean = False, Optional isFile As Boolean = False, Optional Mailto As String = "", Optional ByVal index As Integer = -1) As Boolean
        Dim MsgBody As String = String.Empty
        Dim FileList As String = String.Empty
        Dim MsgSubject As String = String.Empty
        Dim NoofDate As String = String.Empty
        Dim SmtpServer As New SmtpClient()
        Dim mail As New MailMessage()
        Try
            SmtpServer.UseDefaultCredentials = False
            SmtpServer.Credentials = NetCredential
            SmtpServer.Port = 587
            SmtpServer.Host = "server1.deantaglobal.com"
            SmtpServer.EnableSsl = True
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network
            ServicePointManager.ServerCertificateValidationCallback = Function(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) True
            mail = New MailMessage()
            mail.IsBodyHtml = True
            If (IsError) Then
                mail.Priority = MailPriority.High
            End If
            mail.From = New MailAddress("engines@deantaglobal.com", "InDesign server engine")
            mail.To.Add(GBL.SupportMailID)
            If (Not String.IsNullOrEmpty(Mailto)) Then
                mail.To.Add(GBL.SupportMailID)
                mail.To.Add(Mailto)
            Else
                mail.To.Add(GBL.SupportMailID)
            End If
            If (index <> -1) Then
                'Dim extraMail As MailConfiguration = (From n In GBL.ExtraMailIDList Where n.ClientName = GBL.LandstandDataList(index).ClientID Select n).FirstOrDefault
                'If ((extraMail IsNot Nothing) AndAlso (extraMail.Mailids IsNot Nothing)) Then
                '    If (extraMail.Mailids.Count > 0) Then
                '        mail.CC.Add(String.Join(",", extraMail.Mailids.ToArray()))
                '    End If
                'End If
            End If
            If (isFile) Then
                mail.Subject = String.Format("ProjectID: {0} - FileName : {1} ", ProjectID, Path.GetFileName(FileName))
            Else
                mail.Subject = String.Format("ProjectID: {0} ", ProjectID)
            End If
            If (IsError) Then
                mail.Subject = String.Format("[Fail] - {0} ", mail.Subject)
            Else
                mail.Subject = String.Format("[Success] - {0} ", mail.Subject)
                If (File.Exists(FileName)) Then
                    mail.Attachments.Add(New Mail.Attachment(FileName))
                End If
            End If
            If (isFile) Then
                If (File.Exists(FileName)) Then
                    MsgBody = File.ReadAllText(FileName)
                    mail.Body = ConvertHTML(MsgBody, True)
                Else
                    MsgBody = "Regards," & vbNewLine & "InDesign Server Engine"
                    mail.Body = ConvertHTML(MsgBody, True)
                End If
            Else
                MsgBody = ConvertHTML(FileName, True)
                mail.Body = MsgBody
            End If
            Try
                SmtpServer.Send(mail)
            Catch ex As Exception
                Return False
            Finally
                If (mail IsNot Nothing) Then
                    mail.Dispose()
                End If
            End Try
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Public Shared Function ConvertHTML(ByVal Contents As String, ByVal IncludeErrorLog As Boolean) As String
        Dim TmpContents As String = "<html><head></head><body>XXX</body></html>"
        Dim UserMessage As String = Contents
        If (IncludeErrorLog) Then
            If (File.Exists(GBL.LogFilePath)) Then
                Contents = File.ReadAllText(GBL.LogFilePath)
                Contents = Contents & "<h2>Engine Log</h2>"
                Contents = Contents & vbCrLf
                Contents = Contents & "<h3>Action</h3>" & UserMessage
            End If
        End If
        Contents = String.Format("<p>{0}</p>", String.Join("</p><p>", Contents.Split(vbCrLf)))
        Contents = Contents.Replace("</p></p>", "</p>")
        If (IncludeErrorLog) Then
            Contents = Contents & "<p>Regards,</p>" & vbNewLine & "<p>InDesign Server Engine</p>"
        End If
        TmpContents = TmpContents.Replace(">XXX<", ">" & Contents & "<")
        Return TmpContents
    End Function

    Public Shared Function GetServerIP() As String
        Dim HostName As String = Dns.GetHostName()
        Dim ServerIp As String = Dns.GetHostEntry(HostName).AddressList(0).ToString()
        Return ServerIp
    End Function

End Class
