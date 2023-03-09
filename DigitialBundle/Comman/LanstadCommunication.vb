Imports System.Net.WebException
Imports System.Net.Mail
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security

Public Class LanstadCommunication

    Dim DBdata As DigitalBundleData = Nothing
    Dim index As Integer = 0
    Dim eMailList As New List(Of ClientEmailData)
    Public Shared Property NetCredential As New System.Net.NetworkCredential("engines@deantaglobal.com", "RRK!Gio8_bUB")
    Public Sub New(ByVal dbID As Integer)
        Me.index = dbID
        Me.DBdata = GBL.DBDataList(dbID)
        InitializeEmailID()
    End Sub

    Private Sub InitializeEmailID()
        If ((eMailList IsNot Nothing) AndAlso (eMailList.Count > 0)) Then Exit Sub
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.BDS, .eMailDs = "bds@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.BLOOMSBURY, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.RANDL, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.TANDF, .eMailDs = "production3@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.TANDFUK, .eMailDs = "production3@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.ANTHEM, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.CRITICALPUB, .eMailDs = "production3@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.CVP, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.PELAGIC, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.EDWARDELGAR, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.UEPress, .eMailDs = "production2@deantaglobal.com"})
        eMailList.Add(New ClientEmailData With {.ClientType = LanstadClientType.UWIP, .eMailDs = "production2@deantaglobal.com"})
    End Sub

    Public Function StartLanstadTask() As Boolean
        Me.DBdata.DeantaBallon("Commuication send email ", MessageType.MSGERROR)
        'Try
        '    Dim Statuschange As Integer = MySqlHelper.ExecuteScalar("update tb_tasks set status_id=6,task_percentage=50,last_modified =1,user_id=" & DBdata.UserID & " where task_id=" & DBdata.TaskID & "")
        'Catch ex As Exception
        '    DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try
        'Try
        '    AddNotification(DBdata.ProjectID, DBdata.ChapterID, DBdata.TaskID, DBdata.UserID, "The task has been started by")
        'Catch ex As Exception
        '    DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try
        Try
            SendMail(DBdata.ProjectID, DBdata.MilestoneID, DBdata.TaskID, DBdata.ChapterID, DBdata.UserID, "The " & DBdata.TaskName & " task under " & DBdata.MilestoneName & " milestone has been started by", False)
        Catch ex As Exception
            DBdata.DeantaBallon($"StartLanstadTask - {ex.Message}", MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Public Function EndLanstadTask() As Boolean
        'Try
        '    Dim Statuschange As Boolean = MySqlHelper.ExecuteScalar("update tb_tasks set status_id=4,task_percentage=100,last_modified =0,task_completionDate='" & GBL.GetPdfTime & "' where task_id=" & DBdata.TaskID & " and user_id=" & DBdata.UserID & "")
        'Catch ex As Exception
        '    DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try
        'Try
        '    AddNotification(DBdata.ProjectID, DBdata.ChapterID, DBdata.TaskID, DBdata.UserID, "The task has been finished by")
        'Catch ex As Exception
        '    DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try
        Try
            SendMail(DBdata.ProjectID, DBdata.MilestoneID, DBdata.TaskID, DBdata.ChapterID, DBdata.UserID, "The " & DBdata.TaskName & " task under " & DBdata.MilestoneName & " milestone has been finished by", True)
        Catch ex As Exception
            DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Public Function MathTypeEmailToArtwork() As Boolean
        Try
            SendMail(DBdata.ProjectID, DBdata.MilestoneID, DBdata.TaskID, DBdata.ChapterID, DBdata.UserID, "Please process the MathType.zip asset available in the " & DBdata.TaskName & " task under " & DBdata.MilestoneName & " milestone", True)
        Catch ex As Exception
            DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Public Function AddNotification(ProjectID As Integer, ChapterID As Integer, TaskID As Integer, UserID As Integer, Taskdetails As String)
        Dim CommentID As String = String.Empty
        Dim UniqueProcessID As String = String.Empty
        Dim mFirstName As String = String.Empty
        Dim mLastName As String = String.Empty
        Dim GetFullName As String = String.Empty
        Dim UserTo As String = String.Empty
        Dim UserList As DataTable = Nothing
        Dim GetUserName As New DataTable

        Try
            UserList = MySqlHelper.ReadSqlData("Select distinct(userid) FROM stb_authAssignments As st  INNER JOIN tb_projects_has_tb_users  As tb On tb.user_id = st.userid where (st.itemname ='Project Manager' or st.itemname = 'Asst. Project Manager') And project_id = " & ProjectID & "")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " select stb_autho", MessageType.MSGERROR)
            UserTo = "287"
        End Try

        If ((UserList Is Nothing) Or (UserList.Rows Is Nothing) Or (UserList.Rows.Count = 0)) Then
            UserTo = "287"
        Else
            For Each dtRow As DataRow In UserList.Rows
                UserTo = Convert.ToString(IIf(String.IsNullOrEmpty(UserTo), Convert.ToString(dtRow.Item("userid")), String.Format("{0},{1}", UserTo, Convert.ToString(dtRow.Item("userid")))))
            Next
        End If
        UniqueProcessID = DateTime.Now.ToString("yyyyMMddHHmmss")
        Try
            'GetUserName = MySqlHelper.ExecuteScalar("SELECT  user_name,user_lastname from tb_users where user_id=" & UserID)
            'If ((GetUserName.Rows IsNot Nothing) AndAlso (GetUserName.Rows.Count > 0)) Then
            '    mFirstName = GetUserName.Rows(0).Item("user_name")
            '    mLastName = GetUserName.Rows(0).Item("user_lastname")
            '    GetFullName = mFirstName & " " & mLastName
            'End If
            GetFullName = "Digital Bunld Engine"
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " select comment", MessageType.MSGERROR)
            Return False
        End Try

        Try
            MySqlHelper.ExecuteNonQuery("Insert into tb_comments (comment_date,comment_text,comment_resourceid,module_id,user_id,project_id,chapter_id,user_to,process_id,Isattachment,comment_active) values ('" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "','Dear All," & vbCrLf & vbCrLf & Taskdetails & " " & GetFullName & "." & vbCrLf & vbCrLf & " Regards," & vbCrLf & GetFullName & "." & "'," & TaskID & ",11," & UserID & "," & ProjectID & "," & ChapterID & ",'" & UserTo & "','" & UniqueProcessID & "',0,0)")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " insert comment", MessageType.MSGERROR)
            Return False
        End Try

        Try
            CommentID = MySqlHelper.ExecuteScalar("SELECT comment_id FROM  `tb_comments` where process_id='" & UniqueProcessID & "'")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " select comment", MessageType.MSGERROR)
            Return False
        End Try


        If (UserTo.Contains(",")) Then
            For Each UserID1 As String In UserTo.Split(New Char() {","}, System.StringSplitOptions.RemoveEmptyEntries)
                Try
                    MySqlHelper.ExecuteNonQuery("Insert into tb_notifications (notification_resourceid,user_id,module_id,project_id,chapter_id,comment_id,viewed) values(" & TaskID & "," & UserID1 & ",11," & ProjectID & "," & ChapterID & "," & Convert.ToInt32(CommentID) & ",0)")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & " Insert notification", MessageType.MSGERROR)
                    Return False
                End Try
            Next
        Else
            MySqlHelper.ExecuteNonQuery("Insert into tb_notifications (notification_resourceid,user_id,module_id,project_id,chapter_id,comment_id,viewed) values(" & TaskID & "," & UserTo & ",11," & ProjectID & "," & ChapterID & "," & Convert.ToInt32(CommentID) & ",0)")
        End If

        Return True
    End Function

    Public Sub SendMail(ByVal ProjectID1 As String, ByVal MileStoneID1 As String, ByVal TaskID1 As String, ByVal chapterId1 As String, ByVal UserID1 As String, ByVal Taskdetails As String, ByVal mfinishhold As Boolean)
        Try
            Dim FromMailId As String
            Dim Subject As String
            Dim MsgBody As String
            Dim ToMailID As String

            Dim SmtpServer As New SmtpClient()
            Dim mail As New MailMessage()
            SmtpServer.UseDefaultCredentials = False
            SmtpServer.Credentials = NetCredential
            SmtpServer.Port = 587
            SmtpServer.Host = "server1.deantaglobal.com"
            SmtpServer.EnableSsl = True
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network
            ServicePointManager.ServerCertificateValidationCallback = Function(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) True
            mail = New MailMessage()
            mail.IsBodyHtml = True
            mail.From = New MailAddress("engines@deantaglobal.com", "Digital Bundle Engine")
            Dim mFirstName As String = String.Empty
            Dim mLastName As String = String.Empty
            Dim GetFullName As String = String.Empty
            Dim UserTo As String = String.Empty
            Dim UserList As DataTable = Nothing
            Dim dtUserTable As New DataTable
            Dim dtProject As New DataTable

            Dim GetChapterName As New DataTable
            Dim GetCompanyName As New DataTable

            Dim mchapterName As String = String.Empty
            Dim mProjectName As String = String.Empty
            Dim mCompanyID As Integer = 0
            Dim mCompanyName As String = String.Empty
            Dim Chapwise As Integer = 0
            Dim Protype As String = 0

            dtProject = MySqlHelper.ReadSqlData("SELECT project_name,milestone_title,task_name,Proj.company_id,projectType,chapter_wise,bookcode FROM tb_projects  As Proj INNER JOIN tb_milestones As Mile ON Proj.project_id=Mile.project_id AND Proj.project_id=" & ProjectID1 & " AND Mile.milestone_id=" & MileStoneID1 & " INNER JOIN  tb_tasks AS Task ON Proj.project_id=Task.project_id AND Proj.project_id=" & ProjectID1 & " AND Mile.milestone_id=" & MileStoneID1 & " AND Task.task_id=" & TaskID1 & "")

            If ((dtProject.Rows IsNot Nothing) AndAlso (dtProject.Rows.Count > 0)) Then

                mProjectName = dtProject.Rows(0).Item("project_name")
                mCompanyID = dtProject.Rows(0).Item("company_id")
                Protype = dtProject.Rows(0).Item("projectType")
                Chapwise = dtProject.Rows(0).Item("chapter_wise")
                GBL.DBDataList(index).ProjectName = mProjectName
                GBL.DBDataList(index).BookCode = dtProject.Rows(0).Item("bookcode")
            End If

            Try
                dtUserTable = MySqlHelper.ReadSqlData("SELECT user_name,user_lastname from tb_users where user_id=" & UserID1)
                If ((dtUserTable.Rows IsNot Nothing) AndAlso (dtUserTable.Rows.Count > 0)) Then
                    mFirstName = dtUserTable.Rows(0).Item("user_name")
                    mLastName = dtUserTable.Rows(0).Item("user_lastname")

                    GetFullName = mFirstName & " " & mLastName
                End If
            Catch ex As Exception
                DBdata.DeantaBallon(ex.Message & " select comment", MessageType.MSGERROR)
            End Try

            If ((Chapwise = 1) And (Protype = "1")) Then

                GetChapterName = MySqlHelper.ExecuteScalar("select chapter_title from tb_articles where project_id='" & ProjectID1 & "' AND chapter_id='" & chapterId1 & "'  ")
                If ((GetChapterName.Rows IsNot Nothing) AndAlso (GetChapterName.Rows.Count > 0)) Then
                    mchapterName = GetChapterName.Rows(0).Item("chapter_title")
                End If
            ElseIf ((Chapwise = 1) And (Protype.ToLower = "book")) Then
                GetChapterName = MySqlHelper.ExecuteScalar("select chapter_title from tb_chapters where project_id='" & ProjectID1 & "' AND chapter_id='" & chapterId1 & "'  ")
                If ((GetChapterName.Rows IsNot Nothing) AndAlso (GetChapterName.Rows.Count > 0)) Then
                    mchapterName = GetChapterName.Rows(0).Item("chapter_title")
                End If
            End If

            GetCompanyName = MySqlHelper.ReadSqlData("select company_name from  tb_companies where company_id=" & mCompanyID & " ")

            If ((GetCompanyName.Rows IsNot Nothing) AndAlso (GetCompanyName.Rows.Count > 0)) Then
                mCompanyName = GetCompanyName.Rows(0).Item("company_name")
                mCompanyName = mCompanyName.Trim()
            End If

            If chapterId1 = 0 Then
                Subject = " " & GetFullName & "-Status: " & mCompanyName & "/" & mProjectName & "/" & DBdata.MilestoneName & "/" & DBdata.TaskName & "   "
            Else
                Subject = " " & GetFullName & "-Status: " & mCompanyName & "/" & mProjectName & "/" & mchapterName & "/" & DBdata.MilestoneName & "/" & DBdata.TaskName & "   "
            End If

            FromMailId = "engines@deantaglobal.com"
            ToMailID = (From n In eMailList Where n.ClientType = mCompanyID Select n.eMailDs).FirstOrDefault
            mail.From = New MailAddress("engines@deantaglobal.com", "Digital Bundle Engine")
            If ((String.Compare(DBdata.TaskName, "first pages to pm and for xml validation", True) = 0) Or (String.Compare(DBdata.TaskName, "first pages typesetting", True) = 0)) Then
                mail.To.Add("layoutaudit@deantaglobal.com")
                mail.CC.Add("edelivery@deantaglobal.com")
            Else
                mail.To.Add(ToMailID)
                mail.CC.Add("edelivery@deantaglobal.com")
            End If
            mail.To.Add(GBL.SupportMailID)
            MsgBody = ""
            Dim mFullName As String = String.Empty

            Dim GetUserName1 As New DataTable

            GetUserName1 = MySqlHelper.ReadSqlData("SELECT  CONCAT(user_name,' ',user_lastname) As Name,user_email from tb_users where user_id=" & Convert.ToString(UserID1))

            If ((GetUserName1.Rows IsNot Nothing) AndAlso (GetUserName1.Rows.Count > 0)) Then
                mFullName = GetUserName1.Rows(0).Item("Name")
            End If

            If mfinishhold = True Then
                MsgBody = "Dear " & mFullName & "," & vbCrLf & vbCrLf & Taskdetails & " " & GetFullName & " on (" & GBL.GetPdfTime & ")" & vbCrLf & vbCrLf & "Regards," & vbCrLf & "Digital Bundle Engine." & vbCrLf & vbCrLf & "http://www.lanstad.com/epublishing/?r=projects/view&id=<pid>&chapter_id=<cid>&milestoneid=<mid>&taskid=<tid>" & ""
            Else
                MsgBody = "Dear " & mFullName & "," & vbCrLf & vbCrLf & Taskdetails & " " & GetFullName & " On (" & GBL.GetPdfTime & ")" & vbCrLf & vbCrLf & "Regards," & vbCrLf & "Digital Bundle Engine." & vbCrLf & vbCrLf & "http://www.lanstad.com/epublishing/?r=projects/view&id=<pid>&chapter_id=<cid>&milestoneid=<mid>&taskid=<tid>" & ""
            End If

            MsgBody = MsgBody.Replace("<pid>", ProjectID1).Replace("<cid>", chapterId1).Replace("<mid>", MileStoneID1).Replace("<tid>", TaskID1)
            mail.IsBodyHtml = False
            mail.Subject = Subject
            mail.Body = MsgBody
            Try
                SmtpServer.Send(mail)
            Catch ex As Exception
                DBdata.DeantaBallon($"SmtpServer - {ex.Message} - { ex.InnerException.Message}", MessageType.MSGERROR)
                Exit Sub
            Finally
                If (mail IsNot Nothing) Then
                    mail.Dispose()
                End If
            End Try
        Catch ex As Exception
            DBdata.DeantaBallon(ex.Message, MessageType.MSGERROR)
        End Try
    End Sub

End Class

Public Class ClientEmailData
    Public Property ClientType As LanstadClientType = LanstadClientType.NONE
    Public Property eMailDs As String = String.Empty

End Class