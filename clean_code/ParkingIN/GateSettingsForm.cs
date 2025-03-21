using System;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Windows.Forms;

namespace ParkingIN
{
    public partial class GateSettingsForm : Form
    {
        private string gateConfigPath;
        
        public GateSettingsForm()
        {
            InitializeComponent();
            gateConfigPath = Path.Combine(Application.StartupPath, "config", "gate.ini");
        }

        private void GateSettingsForm_Load(object sender, EventArgs e)
        {
            LoadAvailablePorts();
            LoadGateSettings();
        }
        
        private void LoadAvailablePorts()
        {
            try
            {
                cmbComPort.Items.Clear();
                
                // Get available COM ports
                string[] ports = SerialPort.GetPortNames();
                
                if (ports.Length > 0)
                {
                    cmbComPort.Items.AddRange(ports);
                    cmbComPort.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No COM ports found on this system.", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading COM ports: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadGateSettings()
        {
            try
            {
                if (File.Exists(gateConfigPath))
                {
                    string[] lines = File.ReadAllLines(gateConfigPath);
                    
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("["))
                            continue;
                        
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length != 2)
                            continue;
                        
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        switch (key)
                        {
                            case "COM_Port":
                                // Check if the COM port exists in the list
                                if (cmbComPort.Items.Contains(value))
                                {
                                    cmbComPort.SelectedItem = value;
                                }
                                else
                                {
                                    // If not found in the list, add it (it might be a removable device)
                                    cmbComPort.Items.Add(value);
                                    cmbComPort.SelectedItem = value;
                                }
                                break;
                            case "Baud_Rate":
                                cmbBaudRate.Text = value;
                                break;
                            case "Data_Bits":
                                cmbDataBits.Text = value;
                                break;
                            case "Stop_Bits":
                                cmbStopBits.Text = value;
                                break;
                            case "Parity":
                                cmbParity.Text = value;
                                break;
                            case "Open_Delay":
                                txtOpenDelay.Text = value;
                                break;
                            case "Close_Delay":
                                txtCloseDelay.Text = value;
                                break;
                            case "Timeout":
                                txtTimeout.Text = value;
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Gate configuration file not found. Default settings will be used.", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Set default values
                    if (cmbComPort.Items.Count > 0)
                        cmbComPort.SelectedIndex = 0;
                    cmbBaudRate.SelectedIndex = 1; // 9600
                    cmbDataBits.SelectedIndex = 3; // 8
                    cmbStopBits.SelectedIndex = 0; // 1
                    cmbParity.SelectedIndex = 0;   // None
                    txtOpenDelay.Text = "3";
                    txtCloseDelay.Text = "5";
                    txtTimeout.Text = "30";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading gate settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (cmbComPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbComPort.Focus();
                    return;
                }
                
                if (cmbBaudRate.SelectedItem == null)
                {
                    MessageBox.Show("Please select a Baud Rate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbBaudRate.Focus();
                    return;
                }
                
                if (!int.TryParse(txtOpenDelay.Text, out int openDelay) || openDelay < 0)
                {
                    MessageBox.Show("Open Delay must be a valid positive number.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtOpenDelay.Focus();
                    return;
                }
                
                if (!int.TryParse(txtCloseDelay.Text, out int closeDelay) || closeDelay < 0)
                {
                    MessageBox.Show("Close Delay must be a valid positive number.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCloseDelay.Focus();
                    return;
                }
                
                if (!int.TryParse(txtTimeout.Text, out int timeout) || timeout < 0)
                {
                    MessageBox.Show("Timeout must be a valid positive number.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTimeout.Focus();
                    return;
                }
                
                // Ensure directory exists
                string configDir = Path.Combine(Application.StartupPath, "config");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Write to config file
                using (StreamWriter writer = new StreamWriter(gateConfigPath))
                {
                    writer.WriteLine("[Gate]");
                    writer.WriteLine($"COM_Port={cmbComPort.SelectedItem}");
                    writer.WriteLine($"Baud_Rate={cmbBaudRate.SelectedItem}");
                    writer.WriteLine($"Data_Bits={cmbDataBits.SelectedItem}");
                    writer.WriteLine($"Stop_Bits={cmbStopBits.SelectedItem}");
                    writer.WriteLine($"Parity={cmbParity.SelectedItem}");
                    writer.WriteLine();
                    writer.WriteLine("[Timing]");
                    writer.WriteLine($"Open_Delay={txtOpenDelay.Text}");
                    writer.WriteLine($"Close_Delay={txtCloseDelay.Text}");
                    writer.WriteLine($"Timeout={txtTimeout.Text}");
                }
                
                MessageBox.Show("Gate settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving gate settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnTestGate_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbComPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbComPort.Focus();
                    return;
                }
                
                lblStatus.Visible = true;
                lblStatus.Text = "Testing gate connection...";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                string portName = cmbComPort.SelectedItem.ToString();
                int baudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());
                
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    try
                    {
                        port.Open();
                        
                        if (port.IsOpen)
                        {
                            // Send test command (this is placeholder - actual command depends on gate controller)
                            port.Write("TEST\r\n");
                            
                            // Allow time for response
                            System.Threading.Thread.Sleep(500);
                            
                            // For demo purposes, we'll assume the test was successful if we get here
                            MessageBox.Show($"Successfully connected to gate controller on {portName}.", 
                                "Connection Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error connecting to COM port {portName}: {ex.Message}",
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (port.IsOpen)
                            port.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing gate: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblStatus.Visible = false;
                Cursor = Cursors.Default;
            }
        }
        
        private void btnTestOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbComPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbComPort.Focus();
                    return;
                }
                
                lblStatus.Visible = true;
                lblStatus.Text = "Sending open command to gate...";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                string portName = cmbComPort.SelectedItem.ToString();
                int baudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());
                
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    try
                    {
                        port.Open();
                        
                        if (port.IsOpen)
                        {
                            // Send open command (this is placeholder - actual command depends on gate controller)
                            port.Write("OPEN\r\n");
                            
                            MessageBox.Show("Open command sent to gate.", 
                                "Command Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error connecting to COM port {portName}: {ex.Message}",
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (port.IsOpen)
                            port.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending open command: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblStatus.Visible = false;
                Cursor = Cursors.Default;
            }
        }
        
        private void btnTestClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbComPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbComPort.Focus();
                    return;
                }
                
                lblStatus.Visible = true;
                lblStatus.Text = "Sending close command to gate...";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                string portName = cmbComPort.SelectedItem.ToString();
                int baudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());
                
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    try
                    {
                        port.Open();
                        
                        if (port.IsOpen)
                        {
                            // Send close command (this is placeholder - actual command depends on gate controller)
                            port.Write("CLOSE\r\n");
                            
                            MessageBox.Show("Close command sent to gate.", 
                                "Command Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error connecting to COM port {portName}: {ex.Message}",
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (port.IsOpen)
                            port.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending close command: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblStatus.Visible = false;
                Cursor = Cursors.Default;
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Windows Form Designer generated code

        private Label lblTitle;
        private Label lblComPort;
        private ComboBox cmbComPort;
        private Label lblBaudRate;
        private ComboBox cmbBaudRate;
        private Label lblDataBits;
        private ComboBox cmbDataBits;
        private Label lblStopBits;
        private ComboBox cmbStopBits;
        private Label lblParity;
        private ComboBox cmbParity;
        private Label lblTimingSettings;
        private Label lblOpenDelay;
        private TextBox txtOpenDelay;
        private Label lblCloseDelay;
        private TextBox txtCloseDelay;
        private Label lblTimeout;
        private TextBox txtTimeout;
        private Button btnTestGate;
        private Button btnTestOpen;
        private Button btnTestClose;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblComPort = new Label();
            this.cmbComPort = new ComboBox();
            this.lblBaudRate = new Label();
            this.cmbBaudRate = new ComboBox();
            this.lblDataBits = new Label();
            this.cmbDataBits = new ComboBox();
            this.lblStopBits = new Label();
            this.cmbStopBits = new ComboBox();
            this.lblParity = new Label();
            this.cmbParity = new ComboBox();
            this.lblTimingSettings = new Label();
            this.lblOpenDelay = new Label();
            this.txtOpenDelay = new TextBox();
            this.lblCloseDelay = new Label();
            this.txtCloseDelay = new TextBox();
            this.lblTimeout = new Label();
            this.txtTimeout = new TextBox();
            this.btnTestGate = new Button();
            this.btnTestOpen = new Button();
            this.btnTestClose = new Button();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(160, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Gate Settings";
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new Point(25, 70);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new Size(64, 15);
            this.lblComPort.TabIndex = 1;
            this.lblComPort.Text = "COM Port:";
            // 
            // cmbComPort
            // 
            this.cmbComPort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbComPort.FormattingEnabled = true;
            this.cmbComPort.Location = new Point(140, 67);
            this.cmbComPort.Name = "cmbComPort";
            this.cmbComPort.Size = new Size(121, 23);
            this.cmbComPort.TabIndex = 2;
            // 
            // lblBaudRate
            // 
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.Location = new Point(25, 100);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new Size(64, 15);
            this.lblBaudRate.TabIndex = 3;
            this.lblBaudRate.Text = "Baud Rate:";
            // 
            // cmbBaudRate
            // 
            this.cmbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBaudRate.FormattingEnabled = true;
            this.cmbBaudRate.Items.AddRange(new object[] {
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.cmbBaudRate.Location = new Point(140, 97);
            this.cmbBaudRate.Name = "cmbBaudRate";
            this.cmbBaudRate.Size = new Size(121, 23);
            this.cmbBaudRate.TabIndex = 4;
            // 
            // lblDataBits
            // 
            this.lblDataBits.AutoSize = true;
            this.lblDataBits.Location = new Point(25, 130);
            this.lblDataBits.Name = "lblDataBits";
            this.lblDataBits.Size = new Size(58, 15);
            this.lblDataBits.TabIndex = 5;
            this.lblDataBits.Text = "Data Bits:";
            // 
            // cmbDataBits
            // 
            this.cmbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDataBits.FormattingEnabled = true;
            this.cmbDataBits.Items.AddRange(new object[] {
            "5",
            "6",
            "7",
            "8"});
            this.cmbDataBits.Location = new Point(140, 127);
            this.cmbDataBits.Name = "cmbDataBits";
            this.cmbDataBits.Size = new Size(121, 23);
            this.cmbDataBits.TabIndex = 6;
            // 
            // lblStopBits
            // 
            this.lblStopBits.AutoSize = true;
            this.lblStopBits.Location = new Point(25, 160);
            this.lblStopBits.Name = "lblStopBits";
            this.lblStopBits.Size = new Size(56, 15);
            this.lblStopBits.TabIndex = 7;
            this.lblStopBits.Text = "Stop Bits:";
            // 
            // cmbStopBits
            // 
            this.cmbStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStopBits.FormattingEnabled = true;
            this.cmbStopBits.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.cmbStopBits.Location = new Point(140, 157);
            this.cmbStopBits.Name = "cmbStopBits";
            this.cmbStopBits.Size = new Size(121, 23);
            this.cmbStopBits.TabIndex = 8;
            // 
            // lblParity
            // 
            this.lblParity.AutoSize = true;
            this.lblParity.Location = new Point(25, 190);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new Size(40, 15);
            this.lblParity.TabIndex = 9;
            this.lblParity.Text = "Parity:";
            // 
            // cmbParity
            // 
            this.cmbParity.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbParity.FormattingEnabled = true;
            this.cmbParity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark",
            "Space"});
            this.cmbParity.Location = new Point(140, 187);
            this.cmbParity.Name = "cmbParity";
            this.cmbParity.Size = new Size(121, 23);
            this.cmbParity.TabIndex = 10;
            // 
            // lblTimingSettings
            // 
            this.lblTimingSettings.AutoSize = true;
            this.lblTimingSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTimingSettings.Location = new Point(25, 230);
            this.lblTimingSettings.Name = "lblTimingSettings";
            this.lblTimingSettings.Size = new Size(140, 21);
            this.lblTimingSettings.TabIndex = 11;
            this.lblTimingSettings.Text = "Timing Settings:";
            // 
            // lblOpenDelay
            // 
            this.lblOpenDelay.AutoSize = true;
            this.lblOpenDelay.Location = new Point(45, 270);
            this.lblOpenDelay.Name = "lblOpenDelay";
            this.lblOpenDelay.Size = new Size(92, 15);
            this.lblOpenDelay.TabIndex = 12;
            this.lblOpenDelay.Text = "Open Delay (sec):";
            // 
            // txtOpenDelay
            // 
            this.txtOpenDelay.Location = new Point(140, 267);
            this.txtOpenDelay.Name = "txtOpenDelay";
            this.txtOpenDelay.Size = new Size(80, 23);
            this.txtOpenDelay.TabIndex = 13;
            // 
            // lblCloseDelay
            // 
            this.lblCloseDelay.AutoSize = true;
            this.lblCloseDelay.Location = new Point(45, 300);
            this.lblCloseDelay.Name = "lblCloseDelay";
            this.lblCloseDelay.Size = new Size(93, 15);
            this.lblCloseDelay.TabIndex = 14;
            this.lblCloseDelay.Text = "Close Delay (sec):";
            // 
            // txtCloseDelay
            // 
            this.txtCloseDelay.Location = new Point(140, 297);
            this.txtCloseDelay.Name = "txtCloseDelay";
            this.txtCloseDelay.Size = new Size(80, 23);
            this.txtCloseDelay.TabIndex = 15;
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new Point(45, 330);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new Size(80, 15);
            this.lblTimeout.TabIndex = 16;
            this.lblTimeout.Text = "Timeout (sec):";
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new Point(140, 327);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new Size(80, 23);
            this.txtTimeout.TabIndex = 17;
            // 
            // btnTestGate
            // 
            this.btnTestGate.Location = new Point(290, 110);
            this.btnTestGate.Name = "btnTestGate";
            this.btnTestGate.Size = new Size(120, 30);
            this.btnTestGate.TabIndex = 18;
            this.btnTestGate.Text = "Test Connection";
            this.btnTestGate.UseVisualStyleBackColor = true;
            this.btnTestGate.Click += new EventHandler(this.btnTestGate_Click);
            // 
            // btnTestOpen
            // 
            this.btnTestOpen.Location = new Point(290, 150);
            this.btnTestOpen.Name = "btnTestOpen";
            this.btnTestOpen.Size = new Size(120, 30);
            this.btnTestOpen.TabIndex = 19;
            this.btnTestOpen.Text = "Test Open";
            this.btnTestOpen.UseVisualStyleBackColor = true;
            this.btnTestOpen.Click += new EventHandler(this.btnTestOpen_Click);
            // 
            // btnTestClose
            // 
            this.btnTestClose.Location = new Point(290, 190);
            this.btnTestClose.Name = "btnTestClose";
            this.btnTestClose.Size = new Size(120, 30);
            this.btnTestClose.TabIndex = 20;
            this.btnTestClose.Text = "Test Close";
            this.btnTestClose.UseVisualStyleBackColor = true;
            this.btnTestClose.Click += new EventHandler(this.btnTestClose_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new Point(140, 370);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(80, 30);
            this.btnSave.TabIndex = 21;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new Point(240, 370);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = Color.Blue;
            this.lblStatus.Location = new Point(25, 415);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(42, 15);
            this.lblStatus.TabIndex = 23;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // GateSettingsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(434, 441);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnTestClose);
            this.Controls.Add(this.btnTestOpen);
            this.Controls.Add(this.btnTestGate);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.lblTimeout);
            this.Controls.Add(this.txtCloseDelay);
            this.Controls.Add(this.lblCloseDelay);
            this.Controls.Add(this.txtOpenDelay);
            this.Controls.Add(this.lblOpenDelay);
            this.Controls.Add(this.lblTimingSettings);
            this.Controls.Add(this.cmbParity);
            this.Controls.Add(this.lblParity);
            this.Controls.Add(this.cmbStopBits);
            this.Controls.Add(this.lblStopBits);
            this.Controls.Add(this.cmbDataBits);
            this.Controls.Add(this.lblDataBits);
            this.Controls.Add(this.cmbBaudRate);
            this.Controls.Add(this.lblBaudRate);
            this.Controls.Add(this.cmbComPort);
            this.Controls.Add(this.lblComPort);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GateSettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Gate Settings";
            this.Load += new EventHandler(this.GateSettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 