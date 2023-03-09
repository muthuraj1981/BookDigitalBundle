Imports System.Windows.Forms
Imports Server_DigitalBundle.InDesign.Basic

Namespace InDesign.Advanced

    Public Class StyleToTagManager

        Dim SearchMgr As New SearchManager

        Public Property LocalStyles As New List(Of String)

        Public Sub New()
            LocalStyles.AddRange(New String() {"bold", "italic", "sup", "sub", "entity", "bold italic"})
        End Sub

        Public Function ApplyLocalFormatting(CharaStyleName As String, LocalFormat As String) As Boolean
            Dim FindTextPre As Object = Nothing
            Dim ChangeTextPre As Object = Nothing
            Dim CharaStyleObj As Object = Nothing
            Dim SearchResult As Object = Nothing
            INDDGBL.ClearSearch()
            FindTextPre = INDDGBL.InDesignApp.FindTextPreferences
            ChangeTextPre = INDDGBL.InDesignApp.ChangeTextPreferences
            FindTextPre.AppliedParagraphStyle = InDesignConstant.INDESIGN_NOTHING
            Try
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Add()
                CharaStyleObj.Name = CharaStyleName
                CharaStyleObj.FontStyle = LocalFormat
            Catch ex As Exception
                CharaStyleObj.Delete()
                CharaStyleObj = INDDGBL.InDesignDoc.CharacterStyles.Item(CharaStyleName)
            End Try
            Try
                FindTextPre.FontStyle = LocalFormat
                ChangeTextPre.AppliedCharacterStyle = CharaStyleObj
                SearchResult = INDDGBL.InDesignDoc.ChangeText
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function CreateStyleToTag(Target As Object) As Boolean

            Try
                SearchMgr.SearchByStyleandTagged(Target, "cItalic", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "ITALIC", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "italic", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "BOLD", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "cBold", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "bold", "emphasis")
                SearchMgr.SearchByStyleandTagged(Target, "entity", "entity")
            Catch ex As Exception
                MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
            Try
                SearchMgr.FindAndReplaceEquationsElement(Target)
            Catch ex As Exception
                MessageBox.Show(ex.Message, INDDGBL.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
            Return True
        End Function

    End Class

End Namespace

