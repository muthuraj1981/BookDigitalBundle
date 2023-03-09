Imports System.Data.SqlClient
Imports System.Data

Public Class DBHelper

    Public Shared Property ConnectionString() As String

    Private Shared Conn As New SqlConnection
    Private Shared Cmd As New SqlCommand
    Private Shared Ada As New SqlDataAdapter

    Public Sub New()

    End Sub

    Public Shared Function InsertCorrectedRef(ByRef RefID As String, ByVal PushedData As String, ByVal ApiData As String) As String
        Dim Result As String = String.Empty
        Dim ds As New DataSet
        Dim iCount As String = String.Empty
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.MSSQLConnectionString
        End If
        Try
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Cmd.Connection = Conn
            Cmd.CommandText = "usp_insertCorrectedRef"
            Cmd.CommandType = CommandType.StoredProcedure
            Cmd.Parameters.Clear()
            Cmd.Parameters.AddWithValue("@refid", RefID)
            Cmd.Parameters.AddWithValue("@pusheddata", PushedData)
            Cmd.Parameters.AddWithValue("@apidata", ApiData)
            Cmd.Parameters.AddWithValue("@machinename", Environment.MachineName)
            Cmd.Parameters.AddWithValue("@username", Environment.UserName)
            iCount = Cmd.ExecuteNonQuery()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return -1
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
        Return Result
    End Function

    Public Shared Function ExecuteNonQuery(QueryString As String) As Integer
        Dim Result As Integer = 0
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.MSSQLConnectionString
        End If
        If (Conn.State = Data.ConnectionState.Closed) Then
            Conn.Open()
        End If
        Cmd.Connection = Conn
        Cmd.CommandType = CommandType.Text
        Cmd.CommandText = QueryString
        Result = Cmd.ExecuteNonQuery()
        If (Conn.State = Data.ConnectionState.Open) Then
            Conn.Close()
        End If
        Return Result
    End Function

    Public Shared Function ExecuteScalar(QueryString As String) As Object
        Dim Result As Object = Nothing
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.MSSQLConnectionString
        End If
        Try
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Cmd.Connection = Conn
            Cmd.CommandType = CommandType.Text
            Cmd.CommandText = QueryString
            Result = Cmd.ExecuteScalar()
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
        Return Result
    End Function

    Public Shared Function ReadFullTable(tableName As String) As DataTable
        Dim Daset As New DataSet
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.MSSQLConnectionString
        End If
        Try
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Ada.SelectCommand = New SqlCommand("select * from " & tableName, Conn)
            Ada.Fill(Daset, tableName)
            If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
                Return Daset.Tables(0)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return Nothing
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
        Return Nothing
    End Function
    Public Shared Function ReadTable(tableName As String) As DataTable
        Dim Daset As New DataSet
        Try
            If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
                Conn.ConnectionString = GBL.MSSQLConnectionString
            End If
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Ada.SelectCommand = New SqlCommand(tableName, Conn)
            Ada.Fill(Daset, tableName)
            If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
                Return Daset.Tables(0)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return Nothing
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
        Return Nothing
    End Function

    Public Shared Function QueryTable(tableName As String) As DataTable
        Dim Daset As New DataSet
        Dim Ada As New SqlDataAdapter
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.MSSQLConnectionString
        End If
        Try
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Ada.SelectCommand = New SqlCommand(tableName, Conn)
            Ada.Fill(Daset, tableName)
            If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
                Return Daset.Tables(0)
            End If
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return Nothing
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
        Return Nothing
    End Function

    Public Shared Function ReadTableByName(tableName As String) As DataTable
        Dim Daset As New DataSet
        Dim Ada As New SqlDataAdapter
        If (String.IsNullOrEmpty(Conn.ConnectionString)) Then
            Conn.ConnectionString = GBL.ConnectionString
        End If
        Try
            If (Conn.State = Data.ConnectionState.Closed) Then
                Conn.Open()
            End If
            Ada.SelectCommand = New SqlCommand("select * from " & tableName, Conn)
            Ada.Fill(Daset, tableName)
            If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
                Return Daset.Tables(0)
            End If
            Return Nothing
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return Nothing
        Finally
            If (Conn.State = Data.ConnectionState.Open) Then
                Conn.Close()
            End If
        End Try
    End Function


End Class
