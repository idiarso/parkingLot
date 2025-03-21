using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace SimpleParkingAdmin
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Form controls
        private GroupBox groupDatabase;
        private Label lblDBServer;
        private TextBox txtDBServer;
        private Label lblDBName;
        private TextBox txtDBName;
        private Label lblDBUsername;
        private TextBox txtDBUsername;
        private Label lblDBPassword;
        private TextBox txtDBPassword;
        private Button btnTest;
        
        private GroupBox groupApplication;
        private Label lblAppTitle;
        private TextBox txtAppTitle;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private Label lblLogPath;
        private TextBox txtLogPath;
        private Button btnBrowseLog;
        private Label lblLogLevel;
        private ComboBox cmbLogLevel;
        
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            // Form settings
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 510);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Application Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            
            // Initialize controls
            this.groupDatabase = new System.Windows.Forms.GroupBox();
            this.lblDBServer = new System.Windows.Forms.Label();
            this.txtDBServer = new System.Windows.Forms.TextBox();
            this.lblDBName = new System.Windows.Forms.Label();
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.lblDBUsername = new System.Windows.Forms.Label();
            this.txtDBUsername = new System.Windows.Forms.TextBox();
            this.lblDBPassword = new System.Windows.Forms.Label();
            this.txtDBPassword = new System.Windows.Forms.TextBox();
            this.btnTest = new System.Windows.Forms.Button();
            
            this.groupApplication = new System.Windows.Forms.GroupBox();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.txtAppTitle = new System.Windows.Forms.TextBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.btnBrowseLog = new System.Windows.Forms.Button();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            
            this.groupDatabase.SuspendLayout();
            this.groupApplication.SuspendLayout();
            this.SuspendLayout();
            
            // groupDatabase
            this.groupDatabase.Location = new System.Drawing.Point(20, 20);
            this.groupDatabase.Name = "groupDatabase";
            this.groupDatabase.Size = new System.Drawing.Size(560, 200);
            this.groupDatabase.TabIndex = 0;
            this.groupDatabase.TabStop = false;
            this.groupDatabase.Text = "Database Connection";
            
            // lblDBServer
            this.lblDBServer.AutoSize = true;
            this.lblDBServer.Location = new System.Drawing.Point(20, 30);
            this.lblDBServer.Name = "lblDBServer";
            this.lblDBServer.Size = new System.Drawing.Size(100, 20);
            this.lblDBServer.TabIndex = 0;
            this.lblDBServer.Text = "Server:";
            
            // txtDBServer
            this.txtDBServer.Location = new System.Drawing.Point(150, 30);
            this.txtDBServer.Name = "txtDBServer";
            this.txtDBServer.Size = new System.Drawing.Size(250, 25);
            this.txtDBServer.TabIndex = 1;
            
            // lblDBName
            this.lblDBName.AutoSize = true;
            this.lblDBName.Location = new System.Drawing.Point(20, 70);
            this.lblDBName.Name = "lblDBName";
            this.lblDBName.Size = new System.Drawing.Size(100, 20);
            this.lblDBName.TabIndex = 2;
            this.lblDBName.Text = "Database:";
            
            // txtDBName
            this.txtDBName.Location = new System.Drawing.Point(150, 70);
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Size = new System.Drawing.Size(250, 25);
            this.txtDBName.TabIndex = 3;
            
            // lblDBUsername
            this.lblDBUsername.AutoSize = true;
            this.lblDBUsername.Location = new System.Drawing.Point(20, 110);
            this.lblDBUsername.Name = "lblDBUsername";
            this.lblDBUsername.Size = new System.Drawing.Size(100, 20);
            this.lblDBUsername.TabIndex = 4;
            this.lblDBUsername.Text = "Username:";
            
            // txtDBUsername
            this.txtDBUsername.Location = new System.Drawing.Point(150, 110);
            this.txtDBUsername.Name = "txtDBUsername";
            this.txtDBUsername.Size = new System.Drawing.Size(250, 25);
            this.txtDBUsername.TabIndex = 5;
            
            // lblDBPassword
            this.lblDBPassword.AutoSize = true;
            this.lblDBPassword.Location = new System.Drawing.Point(20, 150);
            this.lblDBPassword.Name = "lblDBPassword";
            this.lblDBPassword.Size = new System.Drawing.Size(100, 20);
            this.lblDBPassword.TabIndex = 6;
            this.lblDBPassword.Text = "Password:";
            
            // txtDBPassword
            this.txtDBPassword.Location = new System.Drawing.Point(150, 150);
            this.txtDBPassword.Name = "txtDBPassword";
            this.txtDBPassword.Size = new System.Drawing.Size(250, 25);
            this.txtDBPassword.TabIndex = 7;
            this.txtDBPassword.PasswordChar = '*';
            
            // btnTest
            this.btnTest.Location = new System.Drawing.Point(420, 150);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(120, 30);
            this.btnTest.TabIndex = 8;
            this.btnTest.Text = "Test Connection";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            
            // groupApplication
            this.groupApplication.Location = new System.Drawing.Point(20, 240);
            this.groupApplication.Name = "groupApplication";
            this.groupApplication.Size = new System.Drawing.Size(560, 200);
            this.groupApplication.TabIndex = 1;
            this.groupApplication.TabStop = false;
            this.groupApplication.Text = "Application Settings";
            
            // lblAppTitle
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Location = new System.Drawing.Point(20, 30);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(100, 20);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "App Title:";
            
            // txtAppTitle
            this.txtAppTitle.Location = new System.Drawing.Point(150, 30);
            this.txtAppTitle.Name = "txtAppTitle";
            this.txtAppTitle.Size = new System.Drawing.Size(390, 25);
            this.txtAppTitle.TabIndex = 1;
            
            // lblLanguage
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(20, 70);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(100, 20);
            this.lblLanguage.TabIndex = 2;
            this.lblLanguage.Text = "Language:";
            
            // cmbLanguage
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Location = new System.Drawing.Point(150, 70);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(150, 25);
            this.cmbLanguage.TabIndex = 3;
            
            // lblLogPath
            this.lblLogPath.AutoSize = true;
            this.lblLogPath.Location = new System.Drawing.Point(20, 110);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(100, 20);
            this.lblLogPath.TabIndex = 4;
            this.lblLogPath.Text = "Log File:";
            
            // txtLogPath
            this.txtLogPath.Location = new System.Drawing.Point(150, 110);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(320, 25);
            this.txtLogPath.TabIndex = 5;
            
            // btnBrowseLog
            this.btnBrowseLog.Location = new System.Drawing.Point(490, 110);
            this.btnBrowseLog.Name = "btnBrowseLog";
            this.btnBrowseLog.Size = new System.Drawing.Size(50, 25);
            this.btnBrowseLog.TabIndex = 6;
            this.btnBrowseLog.Text = "...";
            this.btnBrowseLog.UseVisualStyleBackColor = true;
            this.btnBrowseLog.Click += new System.EventHandler(this.btnBrowseLog_Click);
            
            // lblLogLevel
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Location = new System.Drawing.Point(20, 150);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(100, 20);
            this.lblLogLevel.TabIndex = 7;
            this.lblLogLevel.Text = "Log Level:";
            
            // cmbLogLevel
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Location = new System.Drawing.Point(150, 150);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(150, 25);
            this.cmbLogLevel.TabIndex = 8;
            
            // btnSave
            this.btnSave.Location = new System.Drawing.Point(350, 460);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            
            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(480, 460);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            
            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 460);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 20);
            this.lblStatus.TabIndex = 4;
            
            // Form controls
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupApplication);
            this.Controls.Add(this.groupDatabase);
            
            // Add controls to groups
            this.groupDatabase.Controls.Add(this.lblDBServer);
            this.groupDatabase.Controls.Add(this.txtDBServer);
            this.groupDatabase.Controls.Add(this.lblDBName);
            this.groupDatabase.Controls.Add(this.txtDBName);
            this.groupDatabase.Controls.Add(this.lblDBUsername);
            this.groupDatabase.Controls.Add(this.txtDBUsername);
            this.groupDatabase.Controls.Add(this.lblDBPassword);
            this.groupDatabase.Controls.Add(this.txtDBPassword);
            this.groupDatabase.Controls.Add(this.btnTest);
            
            this.groupApplication.Controls.Add(this.lblAppTitle);
            this.groupApplication.Controls.Add(this.txtAppTitle);
            this.groupApplication.Controls.Add(this.lblLanguage);
            this.groupApplication.Controls.Add(this.cmbLanguage);
            this.groupApplication.Controls.Add(this.lblLogPath);
            this.groupApplication.Controls.Add(this.txtLogPath);
            this.groupApplication.Controls.Add(this.btnBrowseLog);
            this.groupApplication.Controls.Add(this.lblLogLevel);
            this.groupApplication.Controls.Add(this.cmbLogLevel);
            
            this.groupDatabase.ResumeLayout(false);
            this.groupDatabase.PerformLayout();
            this.groupApplication.ResumeLayout(false);
            this.groupApplication.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 