Imports System.Xml
Imports System.Xml.Xsl
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class ePubConversion

    Private InputXMLFile As String = String.Empty

    Private SaxonJar As String = "\\fsdeanta\TechRelease\Accounts\Common\TandFCleanup\extra\saxon9.jar"
    Private ePubXsl As String = "D:\DDrive\Documents\ServerDB_Test\ePub\xsl\epub.xsl"
    Private EntFile As String = "\\fsdeanta\TechRelease\Accounts\Common\TandFCleanup\extra\ent.xsl"

    Public Sub New(ByVal InputXML As String)
        Me.InputXMLFile = InputXML
#If (CONFIG <> "Release") Then
        SaxonJar = "\\fsdeanta\TechRelease\Accounts\Common\TandFCleanup\extra\saxon9.jar"
        ePubXsl = "D:\DDrive\Documents\ServerDB_Test\ePub\xsl\epub.xsl"
        EntFile = "\\fsdeanta\TechRelease\Accounts\Common\TandFCleanup\extra\ent.xsl"
#Else
        SaxonJar = Path.Combine(GBL.AppPath, "saxon9.jar")
        ePubXsl = Path.Combine(GBL.AppPath, "epub.xsl")
        EntFile = Path.Combine(GBL.AppPath, "ent.xsl")
#End If
    End Sub
    Public Function DoePubConversion() As Boolean
        Dim TmpFolder As String = String.Empty
        TmpFolder = CreateTmpFoderForConversion()
        If (Not CopyRequiredFilestoTemp(TmpFolder)) Then
            GBL.DeantaBallon("Error occurred while copying required files.", MessageType.MSGERROR)
            Return False
        End If
        Dim FinalXML As String = Path.Combine(TmpFolder, Path.GetFileName(Me.InputXMLFile))
        Try
            CleanupFinalXML(FinalXML)
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Dim ePubConv As String = "java -jar """ & Path.GetFileName(SaxonJar) & """ -s:""" & Path.GetFileName(Me.InputXMLFile) & """ -xsl:""" & Path.GetFileName(Me.ePubXsl) & """"
        If (Not CreateBatAndRunFile(ePubConv, TmpFolder)) Then
            GBL.DeantaBallon("Error occur while create bat file.", MessageType.MSGERROR)
            Return False
        End If
        Return True
    End Function

    Private Function CleanupFinalXML(ByVal FinalXML As String) As Boolean
        Dim xmlDoc As New XmlDocument
        Try
            xmlDoc.LoadXml(File.ReadAllText(FinalXML).Replace("&", "&amp;"))
        Catch ex As Exception
            GBL.DeantaBallon(ex.Message, MessageType.MSGERROR)
            Return False
        End Try
        Return True
    End Function

    Private Function CreateTmpFoderForConversion() As String
        Dim TmpFolder As String = Path.Combine(Path.GetDirectoryName(Me.InputXMLFile), Path.GetFileName(Path.GetTempFileName))
        If (Directory.Exists(TmpFolder)) Then
            Array.ForEach(Directory.GetFiles(TmpFolder), Sub(sfile As String)
                                                             Try
                                                                 File.Delete(sfile)
                                                             Catch ex As Exception
                                                             End Try
                                                         End Sub)
        Else
            Directory.CreateDirectory(TmpFolder)
        End If
        Return TmpFolder
    End Function

    Private Function CopyRequiredFilestoTemp(OutputPath As String) As Boolean
        Try
            File.Copy(Me.InputXMLFile, Path.Combine(OutputPath, Path.GetFileName(Me.InputXMLFile)), True)
            File.Copy(EntFile, Path.Combine(OutputPath, Path.GetFileName(EntFile)), True)
            File.Copy(SaxonJar, Path.Combine(OutputPath, Path.GetFileName(SaxonJar)), True)
            File.Copy(ePubXsl, Path.Combine(OutputPath, Path.GetFileName(ePubXsl)), True)
            File.Copy(EntFile, Path.Combine(OutputPath, Path.GetFileName(EntFile)), True)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Private Function CreateBatAndRunFile(BatFileContent As String, OutputPath As String) As Boolean
        Try
            If (File.Exists(Path.Combine(OutputPath, "run.bat"))) Then File.Delete(Path.Combine(OutputPath, "run.bat"))
            File.WriteAllText(Path.Combine(OutputPath, "run.bat"), BatFileContent)
            While (File.Exists(Path.Combine(OutputPath, "run.bat")))
                Exit While
            End While
            Dim SaxjanProcessInfo As New ProcessStartInfo(Path.Combine(OutputPath, "run.bat"))
            SaxjanProcessInfo.WorkingDirectory = OutputPath
            SaxjanProcessInfo.RedirectStandardError = True
            SaxjanProcessInfo.RedirectStandardOutput = True
            SaxjanProcessInfo.CreateNoWindow = True
            SaxjanProcessInfo.WindowStyle = ProcessWindowStyle.Hidden
            SaxjanProcessInfo.UseShellExecute = False
            Dim SaxjanProcess As Process = Process.Start(SaxjanProcessInfo)
            SaxjanProcess.WaitForExit()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
End Class
