Imports System.IO

Public Class NotificationManager

    Public Sub New()

    End Sub

    Public Function AddNotification(index As Integer) As Boolean
        Dim PdfPath As String = String.Empty
        Dim InddPath As String = String.Empty
        Dim CommentID As String = String.Empty
        Dim UniqueProcessID As String = String.Empty
        Dim TaskName As String = String.Empty
        Dim ResourceID As String = String.Empty
        Dim DbData As DigitalBundleData = GBL.DBDataList(index)

        If ((GBL.OutputDataList Is Nothing) OrElse (GBL.OutputDataList.Count = 0)) Then
            Return True
        End If

        UniqueProcessID = DateTime.Now.ToString("yyyyMMddHHmmss")

        Dim UserTo As String = String.Empty
        Dim UserList As DataTable = Nothing
        Try
            UserList = MySqlHelper.ReadSqlData("SELECT distinct(userid) FROM stb_authAssignments As st  INNER JOIN tb_projects_has_tb_users  As tb on tb.user_id = st.userid where (itemname ='Project Manager') And project_id = " & DbData.ProjectID & "")
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

        Try
            TaskName = MySqlHelper.ExecuteScalar("Select task_name from tb_tasks where task_id=" & DbData.TaskID)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " select task_name", MessageType.MSGERROR)
            Return False
        End Try

        Try
            MySqlHelper.ExecuteNonQuery("Insert into tb_comments (comment_date,comment_text,module_id,user_id,project_id,chapter_id,user_to,process_id,Isattachment,comment_active) values ('" & GBL.GetPdfTime & "','Dear User," & vbCrLf & vbCrLf & "Digitial bundle created sucessfully." & vbCrLf & vbCrLf & " Thanks," & vbCrLf & "InDesign Engine.',11," & DbData.UserID & "," & DbData.ProjectID & "," & DbData.ChapterID & ",'" & UserTo & "','" & UniqueProcessID & "',1,0)")
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message & " insert comment", MessageType.MSGERROR)
            Return False
        End Try

        'Dim INDIAN_ZONE As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
        'Dim indianTime As DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE)

        GBL.DeantaBallon("Uniqud ID :" & UniqueProcessID, MessageType.MSGINFO)

        Try
            CommentID = MySqlHelper.ExecuteScalar("SELECT * FROM  `tb_comments` where process_id='" & UniqueProcessID & "'")
        Catch ex As Exception
            GBL.DeantaBallon("Error get tb_comment-process_id" & UniqueProcessID, MessageType.MSGERROR)
            Return False
        End Try

        ' For Resource path
        GBL.DeantaBallon("Upload list : " & String.Join(",", (From n In GBL.OutputDataList Select n.OutputFile).ToArray()), MessageType.MSGINFO)
        If ((GBL.OutputDataList IsNot Nothing) AndAlso (GBL.OutputDataList.Count > 0)) Then
            For Each OutData As DigitalOutputData In GBL.OutputDataList
                If (OutData.TaskName = DigitalBundleTask.BOOKPDF) Then
                    Try
                        ResourceID = MySqlHelper.ExecuteScalar("Insert into tb_documents (project_id,chapter_id,task_id,document_name,document_uploadDate,document_description,document_path,document_revision,document_type,user_id,document_format,document_core, document_category,comment_id,process_id,document_datetime,pdf_stage) values (" & DbData.ProjectID & "," & DbData.ChapterID & ",0,'" & Path.GetFileName(OutData.OutputFile) & "','" & GBL.GetPdfTime & "','" & TaskName & "','" & String.Format("/wordplugins/bookpdf/{0}", OutData.ResourceID) & "',1,'" & Path.GetExtension(OutData.OutputFile) & "'," & DbData.UserID & ",0,0,0," & CommentID & ",'" & UniqueProcessID & "','" & GBL.GetPdfTime & "','bookpdf');SELECT LAST_INSERT_ID();")
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                Else
                    Try
                        ResourceID = MySqlHelper.ExecuteScalar("Insert into tb_documents (project_id,chapter_id,task_id,document_name,document_uploadDate,document_description,document_path,document_revision,document_type,user_id,document_format,document_core, document_category,comment_id,process_id,document_datetime) values (" & DbData.ProjectID & "," & DbData.ChapterID & "," & DbData.TaskID & ",'" & Path.GetFileName(OutData.OutputFile) & "','" & GBL.GetPdfTime & "','" & TaskName & "','" & String.Format("resources/{0}", OutData.ResourceID) & "',1,'" & Path.GetExtension(OutData.OutputFile) & "'," & DbData.UserID & ",0,0,0," & CommentID & ",'" & UniqueProcessID & "','" & GBL.GetPdfTime & "');SELECT LAST_INSERT_ID();")
                    Catch ex As Exception
                        GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
                        Return False
                    End Try
                End If
                
            Next
        End If

        'Try
        '    ResourceID = MySqlHelper.ExecuteScalar("Select document_id from tb_documents where process_id='" & UniqueProcessID & "'")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try

        If (String.IsNullOrEmpty(ResourceID)) Then
            GBL.DeantaBallon("Could not able to find the resource ID.", MessageType.MSGERROR)
            Return False
        End If

        GBL.DeantaBallon("Notification table", MessageType.MSGINFO)

        If (UserTo.Contains(",")) Then
            For Each UserID As String In UserTo.Split(New Char() {","}, System.StringSplitOptions.RemoveEmptyEntries)
                Try
                    MySqlHelper.ExecuteNonQuery("Insert into tb_notifications (notification_resourceid,user_id,module_id,project_id,chapter_id,comment_id,viewed) values(" & DbData.TaskID & "," & UserID & ",11," & DbData.ProjectID & "," & DbData.ChapterID & "," & Convert.ToInt32(CommentID) & ",0)")
                Catch ex As Exception
                    GBL.DeantaBallon(ex.Message & " Insert notification", MessageType.MSGERROR)
                    Return False
                End Try
            Next
        Else
            MySqlHelper.ExecuteNonQuery("Insert into tb_notifications (notification_resourceid,user_id,module_id,project_id,chapter_id,comment_id,viewed) values(" & DbData.TaskID & "," & UserTo & ",11," & DbData.ProjectID & "," & DbData.ChapterID & "," & Convert.ToInt32(CommentID) & ",0)")
        End If

        'Try
        '    MySqlHelper.ExecuteNonQuery("Update tb_comments set comment_resourceid=" & DbData.UploadTaskID & " where process_id='" & UniqueProcessID & "' AND project_id=" & DbData.ProjectID & " AND chapter_id =" & DbData.ChapterID & "")
        'Catch ex As Exception
        '    GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        '    Return False
        'End Try
        Return True
    End Function

End Class