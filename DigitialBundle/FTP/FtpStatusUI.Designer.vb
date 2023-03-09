<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FtpStatusUI
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PgBar = New System.Windows.Forms.ProgressBar()
        Me.LblStatus = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'PgBar
        '
        Me.PgBar.Location = New System.Drawing.Point(12, 35)
        Me.PgBar.Name = "PgBar"
        Me.PgBar.Size = New System.Drawing.Size(397, 23)
        Me.PgBar.TabIndex = 0
        '
        'LblStatus
        '
        Me.LblStatus.Font = New System.Drawing.Font("Calibri", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblStatus.Location = New System.Drawing.Point(12, 9)
        Me.LblStatus.Name = "LblStatus"
        Me.LblStatus.Size = New System.Drawing.Size(397, 23)
        Me.LblStatus.TabIndex = 1
        '
        'FtpStatusUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(421, 77)
        Me.ControlBox = False
        Me.Controls.Add(Me.LblStatus)
        Me.Controls.Add(Me.PgBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FtpStatusUI"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Ftp Status"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents PgBar As System.Windows.Forms.ProgressBar
    Friend WithEvents LblStatus As System.Windows.Forms.Label
End Class
