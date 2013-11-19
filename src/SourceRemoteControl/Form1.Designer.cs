namespace SourceRemoteControl
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageAll = new System.Windows.Forms.TabPage();
            this.gbOutputs = new System.Windows.Forms.GroupBox();
            this.gbCurrentLimits = new System.Windows.Forms.GroupBox();
            this.gboxVoltage = new System.Windows.Forms.GroupBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.tbOut = new System.Windows.Forms.TextBox();
            this.btnConnection = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panelFunction = new System.Windows.Forms.Panel();
            this.lboxFunctions = new System.Windows.Forms.ListBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panelChecks = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.nudCheckCount = new System.Windows.Forms.NumericUpDown();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.lblPluginInformation = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panelSettings = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.cbSourceType = new System.Windows.Forms.ComboBox();
            this.btnLogSave = new System.Windows.Forms.Button();
            this.btnLogClear = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageAll.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCheckCount)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageAll);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(507, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageAll
            // 
            this.tabPageAll.Controls.Add(this.btnLogClear);
            this.tabPageAll.Controls.Add(this.btnLogSave);
            this.tabPageAll.Controls.Add(this.gbOutputs);
            this.tabPageAll.Controls.Add(this.gbCurrentLimits);
            this.tabPageAll.Controls.Add(this.gboxVoltage);
            this.tabPageAll.Controls.Add(this.btnTest);
            this.tabPageAll.Controls.Add(this.btnDisconnect);
            this.tabPageAll.Controls.Add(this.tbOut);
            this.tabPageAll.Controls.Add(this.btnConnection);
            this.tabPageAll.Location = new System.Drawing.Point(4, 22);
            this.tabPageAll.Name = "tabPageAll";
            this.tabPageAll.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAll.Size = new System.Drawing.Size(499, 400);
            this.tabPageAll.TabIndex = 0;
            this.tabPageAll.Text = "Allgemein";
            this.tabPageAll.UseVisualStyleBackColor = true;
            // 
            // gbOutputs
            // 
            this.gbOutputs.Location = new System.Drawing.Point(6, 151);
            this.gbOutputs.Name = "gbOutputs";
            this.gbOutputs.Size = new System.Drawing.Size(486, 52);
            this.gbOutputs.TabIndex = 13;
            this.gbOutputs.TabStop = false;
            this.gbOutputs.Text = "Ausgänge";
            this.gbOutputs.Visible = false;
            // 
            // gbCurrentLimits
            // 
            this.gbCurrentLimits.Location = new System.Drawing.Point(7, 93);
            this.gbCurrentLimits.Name = "gbCurrentLimits";
            this.gbCurrentLimits.Size = new System.Drawing.Size(486, 52);
            this.gbCurrentLimits.TabIndex = 13;
            this.gbCurrentLimits.TabStop = false;
            this.gbCurrentLimits.Text = "Strombegrenzung";
            this.gbCurrentLimits.Visible = false;
            // 
            // gboxVoltage
            // 
            this.gboxVoltage.Location = new System.Drawing.Point(7, 35);
            this.gboxVoltage.Name = "gboxVoltage";
            this.gboxVoltage.Size = new System.Drawing.Size(486, 52);
            this.gboxVoltage.TabIndex = 13;
            this.gboxVoltage.TabStop = false;
            this.gboxVoltage.Text = "Spannungen";
            this.gboxVoltage.Visible = false;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(421, 6);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 11;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(83, 6);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 10;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // tbOut
            // 
            this.tbOut.Location = new System.Drawing.Point(3, 221);
            this.tbOut.Multiline = true;
            this.tbOut.Name = "tbOut";
            this.tbOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbOut.Size = new System.Drawing.Size(493, 147);
            this.tbOut.TabIndex = 1;
            // 
            // btnConnection
            // 
            this.btnConnection.Location = new System.Drawing.Point(6, 6);
            this.btnConnection.Name = "btnConnection";
            this.btnConnection.Size = new System.Drawing.Size(75, 23);
            this.btnConnection.TabIndex = 0;
            this.btnConnection.Text = "Verbinden";
            this.btnConnection.UseVisualStyleBackColor = true;
            this.btnConnection.Click += new System.EventHandler(this.btnConnection_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panelFunction);
            this.tabPage2.Controls.Add(this.lboxFunctions);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(499, 400);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Funktionen";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panelFunction
            // 
            this.panelFunction.Location = new System.Drawing.Point(132, 6);
            this.panelFunction.Name = "panelFunction";
            this.panelFunction.Size = new System.Drawing.Size(361, 381);
            this.panelFunction.TabIndex = 1;
            // 
            // lboxFunctions
            // 
            this.lboxFunctions.FormattingEnabled = true;
            this.lboxFunctions.Location = new System.Drawing.Point(6, 6);
            this.lboxFunctions.Name = "lboxFunctions";
            this.lboxFunctions.Size = new System.Drawing.Size(120, 381);
            this.lboxFunctions.TabIndex = 0;
            this.lboxFunctions.SelectedIndexChanged += new System.EventHandler(this.lboxFunctions_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.panelChecks);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.nudCheckCount);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(499, 400);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Überprüfungen";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panelChecks
            // 
            this.panelChecks.Location = new System.Drawing.Point(7, 33);
            this.panelChecks.Name = "panelChecks";
            this.panelChecks.Size = new System.Drawing.Size(489, 364);
            this.panelChecks.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Anzahl Überprüfungen";
            // 
            // nudCheckCount
            // 
            this.nudCheckCount.Location = new System.Drawing.Point(123, 7);
            this.nudCheckCount.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.nudCheckCount.Name = "nudCheckCount";
            this.nudCheckCount.Size = new System.Drawing.Size(53, 20);
            this.nudCheckCount.TabIndex = 0;
            this.nudCheckCount.ValueChanged += new System.EventHandler(this.nudCheckCount_ValueChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.lblPluginInformation);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.panelSettings);
            this.tabPage4.Controls.Add(this.label7);
            this.tabPage4.Controls.Add(this.cbSourceType);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(499, 400);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Einstellungen";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(29, 54);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(115, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Verbindungs-Optionen:";
            // 
            // lblPluginInformation
            // 
            this.lblPluginInformation.AutoSize = true;
            this.lblPluginInformation.Location = new System.Drawing.Point(29, 349);
            this.lblPluginInformation.Name = "lblPluginInformation";
            this.lblPluginInformation.Size = new System.Drawing.Size(93, 13);
            this.lblPluginInformation.TabIndex = 4;
            this.lblPluginInformation.Text = "no plugin selected";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(29, 332);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Plugin-Informationen";
            // 
            // panelSettings
            // 
            this.panelSettings.Location = new System.Drawing.Point(29, 73);
            this.panelSettings.Name = "panelSettings";
            this.panelSettings.Size = new System.Drawing.Size(452, 252);
            this.panelSettings.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Quellentyp:";
            // 
            // cbSourceType
            // 
            this.cbSourceType.FormattingEnabled = true;
            this.cbSourceType.Location = new System.Drawing.Point(113, 26);
            this.cbSourceType.Name = "cbSourceType";
            this.cbSourceType.Size = new System.Drawing.Size(368, 21);
            this.cbSourceType.TabIndex = 0;
            this.cbSourceType.SelectedIndexChanged += new System.EventHandler(this.cbSourceType_SelectedIndexChanged);
            // 
            // btnLogSave
            // 
            this.btnLogSave.Location = new System.Drawing.Point(3, 374);
            this.btnLogSave.Name = "btnLogSave";
            this.btnLogSave.Size = new System.Drawing.Size(75, 23);
            this.btnLogSave.TabIndex = 14;
            this.btnLogSave.Text = "Speichern...";
            this.btnLogSave.UseVisualStyleBackColor = true;
            this.btnLogSave.Click += new System.EventHandler(this.btnLogSave_Click);
            // 
            // btnLogClear
            // 
            this.btnLogClear.Location = new System.Drawing.Point(84, 374);
            this.btnLogClear.Name = "btnLogClear";
            this.btnLogClear.Size = new System.Drawing.Size(75, 23);
            this.btnLogClear.TabIndex = 15;
            this.btnLogClear.Text = "Löschen";
            this.btnLogClear.UseVisualStyleBackColor = true;
            this.btnLogClear.Click += new System.EventHandler(this.btnLogClear_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 449);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Source Remote Control";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.tabControl1.ResumeLayout(false);
            this.tabPageAll.ResumeLayout(false);
            this.tabPageAll.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCheckCount)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageAll;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnConnection;
        private System.Windows.Forms.TextBox tbOut;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Panel panelSettings;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbSourceType;
        private System.Windows.Forms.Label lblPluginInformation;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox gboxVoltage;
        private System.Windows.Forms.GroupBox gbCurrentLimits;
        private System.Windows.Forms.Panel panelFunction;
        private System.Windows.Forms.ListBox lboxFunctions;
        private System.Windows.Forms.GroupBox gbOutputs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudCheckCount;
        private System.Windows.Forms.Panel panelChecks;
        private System.Windows.Forms.Button btnLogClear;
        private System.Windows.Forms.Button btnLogSave;
    }
}

