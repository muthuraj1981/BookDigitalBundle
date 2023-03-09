Public Class JournalMetaData

    Public Sub New()
        Me.JournalName = String.Empty
        Me.ElementValue = String.Empty
        Me.ElementXPath = String.Empty
    End Sub

    Public Sub New(JournalName As String, XPath As String, EValue As String)
        Me.JournalName = JournalName
        Me.ElementXPath = XPath
        Me.ElementValue = EValue
        Me.IsRemoved = False
    End Sub

    Public Sub New(JournalName As String, XPath As String, Removed As Boolean)
        Me.JournalName = JournalName
        Me.ElementXPath = XPath
        Me.IsRemoved = Removed
    End Sub

    Public Property JournalName As String = String.Empty
    Public Property ElementXPath As String = String.Empty
    Public Property ElementValue As String = String.Empty
    Public Property IsRemoved As Boolean = False

End Class


Public Class JournalMetaPermission

    Public Sub New()
        Me.JournalName = String.Empty
        Me.PermissionData = String.Empty
    End Sub

    Public Sub New(Name As String, Data As String)
        Me.JournalName = Name
        Me.PermissionData = Data
    End Sub

    Public Property JournalName As String = String.Empty
    Public Property PermissionData As String = String.Empty

End Class

