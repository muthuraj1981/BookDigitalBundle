Imports System.Data
Imports System.Data.OleDb
Imports MySql.Data.MySqlClient

Public Class OleDbHelper

    Private Shared OleCon As New OleDbConnection
    Private Shared OleAda As New OleDbDataAdapter
    Private Shared OleCmd As New OleDbCommand

    Public Sub New()

    End Sub

    Public Shared Function ExecuteSignleValue(ByVal Query As String) As String
        Dim Daset As New DataSet
        Dim RetObj As Object = Nothing
        If (String.IsNullOrEmpty(OleCon.ConnectionString)) Then
            OleCon.ConnectionString = GBL.MSSQLConnectionString
        End If
        Try
            If (OleCon.State = Data.ConnectionState.Closed) Then
                OleCon.Open()
            End If
            OleCmd = New OleDbCommand(Query, OleCon)
            OleCmd.CommandType = CommandType.Text
            RetObj = OleCmd.ExecuteScalar()
            Return Convert.ToString(RetObj)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
        Finally
            OleCon.Close()
        End Try
        Return Nothing
    End Function

    Public Shared Function ExecuteNonQuery(QueryString As String) As Integer
        Dim Result As Integer = 0
        If (String.IsNullOrEmpty(OleCon.ConnectionString)) Then
            OleCon.ConnectionString = GBL.ConnectionString
        End If
        If (OleCon.State = Data.ConnectionState.Closed) Then
            OleCon.Open()
        End If
        OleCmd.Connection = OleCon
        OleCmd.CommandText = QueryString
        Result = OleCmd.ExecuteNonQuery()
        If (OleCon.State = Data.ConnectionState.Open) Then
            OleCon.Close()
        End If
        Return Result
    End Function

    Public Shared Function ReadTableByName(tableName As String) As DataTable
        Dim Daset As New DataSet
        If (String.IsNullOrEmpty(oleCon.ConnectionString)) Then
            OleCon.ConnectionString = GBL.ConnectionString
        End If
        If (oleCon.State = Data.ConnectionState.Closed) Then
            oleCon.Open()
        End If
        OleAda.SelectCommand = New OleDbCommand("select * from " & tableName, OleCon)
        OleAda.Fill(Daset, tableName)
        If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
            Return Daset.Tables(0)
        End If
        Return Nothing
    End Function

    Public Shared Function ReadTable(tableName As String) As DataTable
        Dim Daset As New DataSet
        If (String.IsNullOrEmpty(oleCon.ConnectionString)) Then
            OleCon.ConnectionString = GBL.ConnectionString
        End If
        If (oleCon.State = Data.ConnectionState.Closed) Then
            oleCon.Open()
        End If
        OleAda.SelectCommand = New OleDbCommand(tableName, OleCon)
        OleAda.Fill(Daset, tableName)
        If ((Daset IsNot Nothing) AndAlso (Daset.Tables IsNot Nothing)) Then
            Return Daset.Tables(0)
        End If
        Return Nothing
    End Function

End Class
