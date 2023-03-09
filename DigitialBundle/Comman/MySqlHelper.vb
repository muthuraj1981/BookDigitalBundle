Imports System.IO
Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Net

Public Class MySqlHelper


    Private Shared SqlCon As New MySqlConnection
    Private Shared SqlAda As New MySqlDataAdapter
    Private Shared SqlCmd As New MySqlCommand
    Public Sub New()

    End Sub

    Public Shared Function ExecuteScalar(QueryString As String) As Object
        Dim Result As Object = Nothing
        Try
            If (String.IsNullOrEmpty(SqlCon.ConnectionString)) Then
                SqlCon.ConnectionString = GBL.ConnectionString
            End If
            If (SqlCon.State = Data.ConnectionState.Closed) Then
                SqlCon.Open()
            End If
            Dim cmd As MySqlCommand = New MySqlCommand(QueryString, SqlCon)
            cmd.CommandText = QueryString
            Result = cmd.ExecuteScalar()
            Return Result
        Catch ex As MySqlException
            GBL.DeantaBallon($"My SQL Error: {ex.ErrorCode} - {ex.Message} - {ex.InnerException.Message} - {ex.Source} - Timeout: {SqlCon.ConnectionTimeout}", MessageType.MSGERROR)
            System.Threading.Thread.Sleep(6000)
            Dim reObj As Object = ExecuteScalar(QueryString)
            If (reObj IsNot Nothing) Then
                Return reObj
            End If
        Finally
            SqlCon.Close()
        End Try
        Return Nothing
    End Function

    Public Shared Function ExecuteNonQuery(QueryString As String) As Integer
        Dim Result As Integer = 0
        If (String.IsNullOrEmpty(SqlCon.ConnectionString)) Then
            SqlCon.ConnectionString = GBL.ConnectionString
        End If
        If (SqlCon.State = Data.ConnectionState.Closed) Then
            SqlCon.Open()
        End If
        SqlCmd.Connection = SqlCon
        SqlCmd.CommandText = QueryString
        Try
            Result = SqlCmd.ExecuteNonQuery()
        Catch ex As MySqlException
            GBL.DeantaBallon($"My SQL Error: {ex.ErrorCode} - {ex.Message} - {ex.InnerException.Message} - {ex.Source} - Timeout: {SqlCon.ConnectionTimeout}", MessageType.MSGERROR)
            System.Threading.Thread.Sleep(6000)
            Dim Res As Integer = ExecuteNonQuery(QueryString)
            If (Res <> 0) Then
                Return Res
            End If
        Finally
            If (SqlCon.State = Data.ConnectionState.Open) Then
                SqlCon.Close()
            End If
        End Try
        Return Result
    End Function

    Public Shared Function ReadSqlData(QueryString As String) As DataTable
        Dim Dta As New DataTable
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
        Try
            If (String.IsNullOrEmpty(SqlCon.ConnectionString)) Then
                SqlCon.ConnectionString = GBL.ConnectionString
            End If
            If (SqlCon.State = Data.ConnectionState.Closed) Then
                SqlCon.Open()
            End If
            Dim cmd As MySqlCommand = New MySqlCommand(QueryString, SqlCon)
            Dim ada As MySqlDataAdapter = New MySqlDataAdapter(cmd)
            ada.Fill(Dta)
            Return Dta
        Catch ex As MySqlException
            GBL.DeantaBallon($"My SQL Error: {ex.ErrorCode} - {ex.Message} - {ex.InnerException.Message} - {ex.Source} - Timeout: {SqlCon.ConnectionTimeout}", MessageType.MSGERROR)
            System.Threading.Thread.Sleep(6000)
            Dim reObj As DataTable = ReadSqlData(QueryString)
            If (reObj IsNot Nothing) Then
                Return reObj
            End If
        Catch ex1 As Exception
            GBL.DeantaBallon($"My SQL Error: - {ex1.Message} - {ex1.InnerException.Message} - {ex1.Source} - Timeout: {SqlCon.ConnectionTimeout}", MessageType.MSGERROR)
            System.Threading.Thread.Sleep(6000)
            Dim reObj As DataTable = ReadSqlData(QueryString)
            If (reObj IsNot Nothing) Then
                Return reObj
            End If
        Finally
            SqlCon.Close()
        End Try
        Return Nothing
    End Function


End Class
