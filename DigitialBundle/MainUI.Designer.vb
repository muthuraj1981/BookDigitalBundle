<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainUI
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainUI))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.dgvDigitialBun = New System.Windows.Forms.DataGridView()
        Me.DigitalID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ProjectID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TaskID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.UploadTaskID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ChapterID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.MilestoneID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DocumentID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.UserID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ProjectName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ApplicationISBN = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.CoverISBN = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WebPDFISBN = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ePubISBN = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ProjectAbb = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.HardbackNum = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PaperbackNum = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.XmlURL = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WorkPath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ImagePath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsXMLGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsPODGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsWEBPDFGeneratd = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsEpubGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsPackageGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsRTFGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsCoverGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsBookPDFGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsMOBIGenerated = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Status = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.CurrentStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Folder = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DocType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.MainXML = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClientXML = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClientePubXML = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClientOutXML = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClientCleanXML = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FileOrderList = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.InDesignFileList = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsProcessCompleted = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.TemplateFullName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.CoverImageFullName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ClientAbbrevation = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TaskList = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsLocalSetup = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.FinalAssets = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ResourceAssets = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Stage = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.LogFilePath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.TssLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.TssPgBar = New System.Windows.Forms.ToolStripProgressBar()
        Me.Panel2.SuspendLayout()
        CType(Me.dgvDigitialBun, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1176, 115)
        Me.Panel1.TabIndex = 0
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.dgvDigitialBun)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(0, 115)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(1176, 348)
        Me.Panel2.TabIndex = 1
        '
        'dgvDigitialBun
        '
        Me.dgvDigitialBun.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvDigitialBun.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DigitalID, Me.ProjectID, Me.TaskID, Me.UploadTaskID, Me.ChapterID, Me.MilestoneID, Me.DocumentID, Me.UserID, Me.ProjectName, Me.ApplicationISBN, Me.CoverISBN, Me.WebPDFISBN, Me.ePubISBN, Me.ProjectAbb, Me.HardbackNum, Me.PaperbackNum, Me.XmlURL, Me.WorkPath, Me.ImagePath, Me.IsXMLGenerated, Me.IsPODGenerated, Me.IsWEBPDFGeneratd, Me.IsEpubGenerated, Me.IsPackageGenerated, Me.IsRTFGenerated, Me.IsCoverGenerated, Me.IsBookPDFGenerated, Me.IsMOBIGenerated, Me.Status, Me.CurrentStatus, Me.Folder, Me.DocType, Me.MainXML, Me.ClientXML, Me.ClientePubXML, Me.ClientOutXML, Me.ClientCleanXML, Me.FileOrderList, Me.InDesignFileList, Me.IsProcessCompleted, Me.TemplateFullName, Me.CoverImageFullName, Me.ClientAbbrevation, Me.TaskList, Me.IsLocalSetup, Me.FinalAssets, Me.ResourceAssets, Me.Stage, Me.LogFilePath})
        Me.dgvDigitialBun.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvDigitialBun.Location = New System.Drawing.Point(0, 0)
        Me.dgvDigitialBun.Name = "dgvDigitialBun"
        Me.dgvDigitialBun.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvDigitialBun.Size = New System.Drawing.Size(1176, 348)
        Me.dgvDigitialBun.TabIndex = 0
        '
        'DigitalID
        '
        Me.DigitalID.DataPropertyName = "DigitalID"
        Me.DigitalID.HeaderText = "DigitalID"
        Me.DigitalID.Name = "DigitalID"
        '
        'ProjectID
        '
        Me.ProjectID.DataPropertyName = "ProjectID"
        Me.ProjectID.HeaderText = "ProjectID"
        Me.ProjectID.Name = "ProjectID"
        Me.ProjectID.Visible = False
        '
        'TaskID
        '
        Me.TaskID.DataPropertyName = "TaskID"
        Me.TaskID.HeaderText = "TaskID"
        Me.TaskID.Name = "TaskID"
        Me.TaskID.Visible = False
        '
        'UploadTaskID
        '
        Me.UploadTaskID.DataPropertyName = "UploadTaskID"
        Me.UploadTaskID.HeaderText = "UploadTaskID"
        Me.UploadTaskID.Name = "UploadTaskID"
        Me.UploadTaskID.Visible = False
        '
        'ChapterID
        '
        Me.ChapterID.DataPropertyName = "ChapterID"
        Me.ChapterID.HeaderText = "ChapterID"
        Me.ChapterID.Name = "ChapterID"
        Me.ChapterID.Visible = False
        '
        'MilestoneID
        '
        Me.MilestoneID.DataPropertyName = "MilestoneID"
        Me.MilestoneID.HeaderText = "MilestoneID"
        Me.MilestoneID.Name = "MilestoneID"
        Me.MilestoneID.Visible = False
        '
        'DocumentID
        '
        Me.DocumentID.DataPropertyName = "DocumentID"
        Me.DocumentID.HeaderText = "DocumentID"
        Me.DocumentID.Name = "DocumentID"
        Me.DocumentID.Visible = False
        '
        'UserID
        '
        Me.UserID.DataPropertyName = "UserID"
        Me.UserID.HeaderText = "UserID"
        Me.UserID.Name = "UserID"
        '
        'ProjectName
        '
        Me.ProjectName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ProjectName.DataPropertyName = "ProjectName"
        Me.ProjectName.HeaderText = "ProjectName"
        Me.ProjectName.Name = "ProjectName"
        '
        'ApplicationISBN
        '
        Me.ApplicationISBN.DataPropertyName = "ApplicationISBN"
        Me.ApplicationISBN.HeaderText = "ApplicationISBN"
        Me.ApplicationISBN.Name = "ApplicationISBN"
        Me.ApplicationISBN.Visible = False
        '
        'CoverISBN
        '
        Me.CoverISBN.DataPropertyName = "CoverISBN"
        Me.CoverISBN.HeaderText = "CoverISBN"
        Me.CoverISBN.Name = "CoverISBN"
        Me.CoverISBN.Visible = False
        '
        'WebPDFISBN
        '
        Me.WebPDFISBN.DataPropertyName = "WebPDFISBN"
        Me.WebPDFISBN.HeaderText = "WebPDFISBN"
        Me.WebPDFISBN.Name = "WebPDFISBN"
        Me.WebPDFISBN.Visible = False
        '
        'ePubISBN
        '
        Me.ePubISBN.DataPropertyName = "ePubISBN"
        Me.ePubISBN.HeaderText = "ePubISBN"
        Me.ePubISBN.Name = "ePubISBN"
        Me.ePubISBN.Visible = False
        '
        'ProjectAbb
        '
        Me.ProjectAbb.DataPropertyName = "ProjectAbb"
        Me.ProjectAbb.HeaderText = "ProjectAbb"
        Me.ProjectAbb.Name = "ProjectAbb"
        Me.ProjectAbb.Visible = False
        '
        'HardbackNum
        '
        Me.HardbackNum.DataPropertyName = "HardbackNum"
        Me.HardbackNum.HeaderText = "HardbackNum"
        Me.HardbackNum.Name = "HardbackNum"
        Me.HardbackNum.Visible = False
        '
        'PaperbackNum
        '
        Me.PaperbackNum.DataPropertyName = "PaperbackNum"
        Me.PaperbackNum.HeaderText = "PaperbackNum"
        Me.PaperbackNum.Name = "PaperbackNum"
        Me.PaperbackNum.Visible = False
        '
        'XmlURL
        '
        Me.XmlURL.DataPropertyName = "XmlURL"
        Me.XmlURL.HeaderText = "XmlURL"
        Me.XmlURL.Name = "XmlURL"
        Me.XmlURL.Visible = False
        '
        'WorkPath
        '
        Me.WorkPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.WorkPath.DataPropertyName = "WorkPath"
        Me.WorkPath.HeaderText = "WorkPath"
        Me.WorkPath.Name = "WorkPath"
        '
        'ImagePath
        '
        Me.ImagePath.DataPropertyName = "ImagePath"
        Me.ImagePath.HeaderText = "ImagePath"
        Me.ImagePath.Name = "ImagePath"
        Me.ImagePath.Visible = False
        '
        'IsXMLGenerated
        '
        Me.IsXMLGenerated.DataPropertyName = "IsXMLGenerated"
        Me.IsXMLGenerated.HeaderText = "IsXMLGenerated"
        Me.IsXMLGenerated.Name = "IsXMLGenerated"
        Me.IsXMLGenerated.Visible = False
        '
        'IsPODGenerated
        '
        Me.IsPODGenerated.DataPropertyName = "IsPODGenerated"
        Me.IsPODGenerated.HeaderText = "IsPODGenerated"
        Me.IsPODGenerated.Name = "IsPODGenerated"
        Me.IsPODGenerated.Visible = False
        '
        'IsWEBPDFGeneratd
        '
        Me.IsWEBPDFGeneratd.DataPropertyName = "IsWEBPDFGeneratd"
        Me.IsWEBPDFGeneratd.HeaderText = "IsWEBPDFGeneratd"
        Me.IsWEBPDFGeneratd.Name = "IsWEBPDFGeneratd"
        Me.IsWEBPDFGeneratd.Visible = False
        '
        'IsEpubGenerated
        '
        Me.IsEpubGenerated.DataPropertyName = "IsEpubGenerated"
        Me.IsEpubGenerated.HeaderText = "IsEpubGenerated"
        Me.IsEpubGenerated.Name = "IsEpubGenerated"
        Me.IsEpubGenerated.Visible = False
        '
        'IsPackageGenerated
        '
        Me.IsPackageGenerated.DataPropertyName = "IsPackageGenerated"
        Me.IsPackageGenerated.HeaderText = "IsPackageGenerated"
        Me.IsPackageGenerated.Name = "IsPackageGenerated"
        Me.IsPackageGenerated.Visible = False
        '
        'IsRTFGenerated
        '
        Me.IsRTFGenerated.DataPropertyName = "IsRTFGenerated"
        Me.IsRTFGenerated.HeaderText = "IsRTFGenerated"
        Me.IsRTFGenerated.Name = "IsRTFGenerated"
        Me.IsRTFGenerated.Visible = False
        '
        'IsCoverGenerated
        '
        Me.IsCoverGenerated.DataPropertyName = "IsCoverGenerated"
        Me.IsCoverGenerated.HeaderText = "IsCoverGenerated"
        Me.IsCoverGenerated.Name = "IsCoverGenerated"
        Me.IsCoverGenerated.Visible = False
        '
        'IsBookPDFGenerated
        '
        Me.IsBookPDFGenerated.DataPropertyName = "IsBookPDFGenerated"
        Me.IsBookPDFGenerated.HeaderText = "IsBookPDFGenerated"
        Me.IsBookPDFGenerated.Name = "IsBookPDFGenerated"
        Me.IsBookPDFGenerated.Visible = False
        '
        'IsMOBIGenerated
        '
        Me.IsMOBIGenerated.DataPropertyName = "IsMOBIGenerated"
        Me.IsMOBIGenerated.HeaderText = "IsMOBIGenerated"
        Me.IsMOBIGenerated.Name = "IsMOBIGenerated"
        Me.IsMOBIGenerated.Visible = False
        '
        'Status
        '
        Me.Status.DataPropertyName = "Status"
        Me.Status.HeaderText = "Status"
        Me.Status.Name = "Status"
        Me.Status.Visible = False
        '
        'CurrentStatus
        '
        Me.CurrentStatus.DataPropertyName = "CurrentStatus"
        Me.CurrentStatus.HeaderText = "CurrentStatus"
        Me.CurrentStatus.Name = "CurrentStatus"
        '
        'Folder
        '
        Me.Folder.DataPropertyName = "Folder"
        Me.Folder.HeaderText = "Folder"
        Me.Folder.Name = "Folder"
        Me.Folder.Visible = False
        '
        'DocType
        '
        Me.DocType.DataPropertyName = "DocType"
        Me.DocType.HeaderText = "DocType"
        Me.DocType.Name = "DocType"
        Me.DocType.Visible = False
        '
        'MainXML
        '
        Me.MainXML.DataPropertyName = "MainXML"
        Me.MainXML.HeaderText = "MainXML"
        Me.MainXML.Name = "MainXML"
        Me.MainXML.Visible = False
        '
        'ClientXML
        '
        Me.ClientXML.DataPropertyName = "ClientXML"
        Me.ClientXML.HeaderText = "ClientXML"
        Me.ClientXML.Name = "ClientXML"
        Me.ClientXML.Visible = False
        '
        'ClientePubXML
        '
        Me.ClientePubXML.DataPropertyName = "ClientePubXML"
        Me.ClientePubXML.HeaderText = "ClientePubXML"
        Me.ClientePubXML.Name = "ClientePubXML"
        Me.ClientePubXML.Visible = False
        '
        'ClientOutXML
        '
        Me.ClientOutXML.DataPropertyName = "ClientOutXML"
        Me.ClientOutXML.HeaderText = "ClientOutXML"
        Me.ClientOutXML.Name = "ClientOutXML"
        Me.ClientOutXML.Visible = False
        '
        'ClientCleanXML
        '
        Me.ClientCleanXML.DataPropertyName = "ClientCleanXML"
        Me.ClientCleanXML.HeaderText = "ClientCleanXML"
        Me.ClientCleanXML.Name = "ClientCleanXML"
        Me.ClientCleanXML.Visible = False
        '
        'FileOrderList
        '
        Me.FileOrderList.DataPropertyName = "FileOrderList"
        Me.FileOrderList.HeaderText = "FileOrderList"
        Me.FileOrderList.Name = "FileOrderList"
        Me.FileOrderList.Visible = False
        '
        'InDesignFileList
        '
        Me.InDesignFileList.DataPropertyName = "InDesignFileList"
        Me.InDesignFileList.HeaderText = "InDesignFileList"
        Me.InDesignFileList.Name = "InDesignFileList"
        Me.InDesignFileList.Visible = False
        '
        'IsProcessCompleted
        '
        Me.IsProcessCompleted.DataPropertyName = "IsProcessCompleted"
        Me.IsProcessCompleted.HeaderText = "IsProcessCompleted"
        Me.IsProcessCompleted.Name = "IsProcessCompleted"
        '
        'TemplateFullName
        '
        Me.TemplateFullName.DataPropertyName = "TemplateFullName"
        Me.TemplateFullName.HeaderText = "TemplateFullName"
        Me.TemplateFullName.Name = "TemplateFullName"
        Me.TemplateFullName.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.TemplateFullName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.TemplateFullName.Visible = False
        '
        'CoverImageFullName
        '
        Me.CoverImageFullName.DataPropertyName = "CoverImageFullName"
        Me.CoverImageFullName.HeaderText = "CoverImageFullName"
        Me.CoverImageFullName.Name = "CoverImageFullName"
        Me.CoverImageFullName.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.CoverImageFullName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.CoverImageFullName.Visible = False
        '
        'ClientAbbrevation
        '
        Me.ClientAbbrevation.DataPropertyName = "ClientAbbrevation"
        Me.ClientAbbrevation.HeaderText = "ClientAbbrevation"
        Me.ClientAbbrevation.Name = "ClientAbbrevation"
        Me.ClientAbbrevation.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.ClientAbbrevation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.ClientAbbrevation.Visible = False
        '
        'TaskList
        '
        Me.TaskList.DataPropertyName = "TaskList"
        Me.TaskList.HeaderText = "TaskList"
        Me.TaskList.Name = "TaskList"
        Me.TaskList.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.TaskList.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.TaskList.Visible = False
        '
        'IsLocalSetup
        '
        Me.IsLocalSetup.DataPropertyName = "IsLocalSetup"
        Me.IsLocalSetup.HeaderText = "IsLocalSetup"
        Me.IsLocalSetup.Name = "IsLocalSetup"
        Me.IsLocalSetup.Visible = False
        '
        'FinalAssets
        '
        Me.FinalAssets.DataPropertyName = "FinalAssets"
        Me.FinalAssets.HeaderText = "FinalAssets"
        Me.FinalAssets.Name = "FinalAssets"
        Me.FinalAssets.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.FinalAssets.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.FinalAssets.Visible = False
        '
        'ResourceAssets
        '
        Me.ResourceAssets.DataPropertyName = "ResourceAssets"
        Me.ResourceAssets.HeaderText = "ResourceAssets"
        Me.ResourceAssets.Name = "ResourceAssets"
        Me.ResourceAssets.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.ResourceAssets.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.ResourceAssets.Visible = False
        '
        'Stage
        '
        Me.Stage.DataPropertyName = "Stage"
        Me.Stage.HeaderText = "Stage"
        Me.Stage.Name = "Stage"
        Me.Stage.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Stage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Stage.Visible = False
        '
        'LogFilePath
        '
        Me.LogFilePath.DataPropertyName = "LogFilePath"
        Me.LogFilePath.HeaderText = "LogFilePath"
        Me.LogFilePath.Name = "LogFilePath"
        Me.LogFilePath.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.LogFilePath.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.LogFilePath.Visible = False
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TssLabel, Me.TssPgBar})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 463)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1176, 22)
        Me.StatusStrip1.TabIndex = 2
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'TssLabel
        '
        Me.TssLabel.Name = "TssLabel"
        Me.TssLabel.Size = New System.Drawing.Size(26, 17)
        Me.TssLabel.Text = "0 %"
        '
        'TssPgBar
        '
        Me.TssPgBar.Name = "TssPgBar"
        Me.TssPgBar.Size = New System.Drawing.Size(100, 16)
        '
        'MainUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1176, 485)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Font = New System.Drawing.Font("Calibri", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "MainUI"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MainUI"
        Me.Panel2.ResumeLayout(False)
        CType(Me.dgvDigitialBun, System.ComponentModel.ISupportInitialize).EndInit()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents dgvDigitialBun As System.Windows.Forms.DataGridView
    Friend WithEvents DigitalID As DataGridViewTextBoxColumn
    Friend WithEvents ProjectID As DataGridViewTextBoxColumn
    Friend WithEvents TaskID As DataGridViewTextBoxColumn
    Friend WithEvents UploadTaskID As DataGridViewTextBoxColumn
    Friend WithEvents ChapterID As DataGridViewTextBoxColumn
    Friend WithEvents MilestoneID As DataGridViewTextBoxColumn
    Friend WithEvents DocumentID As DataGridViewTextBoxColumn
    Friend WithEvents UserID As DataGridViewTextBoxColumn
    Friend WithEvents ProjectName As DataGridViewTextBoxColumn
    Friend WithEvents ApplicationISBN As DataGridViewTextBoxColumn
    Friend WithEvents CoverISBN As DataGridViewTextBoxColumn
    Friend WithEvents WebPDFISBN As DataGridViewTextBoxColumn
    Friend WithEvents ePubISBN As DataGridViewTextBoxColumn
    Friend WithEvents ProjectAbb As DataGridViewTextBoxColumn
    Friend WithEvents HardbackNum As DataGridViewTextBoxColumn
    Friend WithEvents PaperbackNum As DataGridViewTextBoxColumn
    Friend WithEvents XmlURL As DataGridViewTextBoxColumn
    Friend WithEvents WorkPath As DataGridViewTextBoxColumn
    Friend WithEvents ImagePath As DataGridViewTextBoxColumn
    Friend WithEvents IsXMLGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsPODGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsWEBPDFGeneratd As DataGridViewTextBoxColumn
    Friend WithEvents IsEpubGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsPackageGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsRTFGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsCoverGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsBookPDFGenerated As DataGridViewTextBoxColumn
    Friend WithEvents IsMOBIGenerated As DataGridViewTextBoxColumn
    Friend WithEvents Status As DataGridViewTextBoxColumn
    Friend WithEvents CurrentStatus As DataGridViewTextBoxColumn
    Friend WithEvents Folder As DataGridViewTextBoxColumn
    Friend WithEvents DocType As DataGridViewTextBoxColumn
    Friend WithEvents MainXML As DataGridViewTextBoxColumn
    Friend WithEvents ClientXML As DataGridViewTextBoxColumn
    Friend WithEvents ClientePubXML As DataGridViewTextBoxColumn
    Friend WithEvents ClientOutXML As DataGridViewTextBoxColumn
    Friend WithEvents ClientCleanXML As DataGridViewTextBoxColumn
    Friend WithEvents FileOrderList As DataGridViewTextBoxColumn
    Friend WithEvents InDesignFileList As DataGridViewTextBoxColumn
    Friend WithEvents IsProcessCompleted As DataGridViewCheckBoxColumn
    Friend WithEvents TemplateFullName As DataGridViewTextBoxColumn
    Friend WithEvents CoverImageFullName As DataGridViewTextBoxColumn
    Friend WithEvents ClientAbbrevation As DataGridViewTextBoxColumn
    Friend WithEvents TaskList As DataGridViewTextBoxColumn
    Friend WithEvents IsLocalSetup As DataGridViewCheckBoxColumn
    Friend WithEvents FinalAssets As DataGridViewTextBoxColumn
    Friend WithEvents ResourceAssets As DataGridViewTextBoxColumn
    Friend WithEvents Stage As DataGridViewTextBoxColumn
    Friend WithEvents LogFilePath As DataGridViewTextBoxColumn
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents TssLabel As ToolStripStatusLabel
    Friend WithEvents TssPgBar As ToolStripProgressBar
End Class
