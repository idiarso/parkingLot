using System;
using System.Drawing;
using System.Windows.Forms;

namespace IUTVehicleManager.Forms
{
    public partial class GetInSimulationForm : Form
    {
        private PictureBox cameraPreview;
        private Button btnCapture;
        private Button btnPrint;
        private Button btnOpenGate;
        private Button btnCloseGate;
        private Label lblStatus;
        private Label lblPlateNumber;
        private System.Windows.Forms.Timer simulationTimer;
        private bool isGateOpen = false;
        private bool isSimulating = false;

        public GetInSimulationForm()
        {
            InitializeComponent();
            InitializeSimulationControls();
        }

        private void InitializeSimulationControls()
        {
            // Set form properties
            this.Text = "GET IN Simulation";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create camera preview
            cameraPreview = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(400, 300),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };

            // Create status label
            lblStatus = new Label
            {
                Location = new Point(20, 340),
                Size = new Size(400, 20),
                Text = "Status: Ready",
                Font = new Font("Segoe UI", 10F)
            };

            // Create plate number label
            lblPlateNumber = new Label
            {
                Location = new Point(20, 370),
                Size = new Size(400, 20),
                Text = "Plate Number: -",
                Font = new Font("Segoe UI", 10F)
            };

            // Create control buttons
            btnCapture = new Button
            {
                Location = new Point(20, 400),
                Size = new Size(120, 40),
                Text = "Capture",
                Font = new Font("Segoe UI", 10F)
            };
            btnCapture.Click += BtnCapture_Click;

            btnPrint = new Button
            {
                Location = new Point(160, 400),
                Size = new Size(120, 40),
                Text = "Print Ticket",
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnPrint.Click += BtnPrint_Click;

            btnOpenGate = new Button
            {
                Location = new Point(300, 400),
                Size = new Size(120, 40),
                Text = "Open Gate",
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnOpenGate.Click += BtnOpenGate_Click;

            btnCloseGate = new Button
            {
                Location = new Point(440, 400),
                Size = new Size(120, 40),
                Text = "Close Gate",
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnCloseGate.Click += BtnCloseGate_Click;

            // Create simulation timer
            simulationTimer = new System.Windows.Forms.Timer();
            simulationTimer.Interval = 1000; // 1 second
            simulationTimer.Tick += SimulationTimer_Tick;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                cameraPreview,
                lblStatus,
                lblPlateNumber,
                btnCapture,
                btnPrint,
                btnOpenGate,
                btnCloseGate
            });
        }

        private void BtnCapture_Click(object sender, EventArgs e)
        {
            if (isSimulating) return;

            try
            {
                isSimulating = true;
                btnCapture.Enabled = false;
                lblStatus.Text = "Status: Capturing...";

                // Simulate camera capture
                System.Threading.Thread.Sleep(1000);
                
                // Generate random plate number for simulation
                string plateNumber = GenerateRandomPlateNumber();
                lblPlateNumber.Text = $"Plate Number: {plateNumber}";

                // Simulate image processing
                System.Threading.Thread.Sleep(1000);

                // Enable print button
                btnPrint.Enabled = true;
                lblStatus.Text = "Status: Ready to print ticket";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during capture: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Status: Error during capture";
            }
            finally
            {
                isSimulating = false;
                btnCapture.Enabled = true;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (isSimulating) return;

            try
            {
                isSimulating = true;
                btnPrint.Enabled = false;
                lblStatus.Text = "Status: Printing ticket...";

                // Simulate ticket printing
                System.Threading.Thread.Sleep(1000);

                // Enable gate control
                btnOpenGate.Enabled = true;
                lblStatus.Text = "Status: Ticket printed, ready to open gate";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during printing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Status: Error during printing";
            }
            finally
            {
                isSimulating = false;
                btnPrint.Enabled = true;
            }
        }

        private void BtnOpenGate_Click(object sender, EventArgs e)
        {
            if (isSimulating) return;

            try
            {
                isSimulating = true;
                btnOpenGate.Enabled = false;
                lblStatus.Text = "Status: Opening gate...";

                // Simulate gate opening
                System.Threading.Thread.Sleep(1000);
                isGateOpen = true;

                // Enable close gate button
                btnCloseGate.Enabled = true;
                lblStatus.Text = "Status: Gate is open";

                // Start simulation timer
                simulationTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening gate: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Status: Error opening gate";
            }
            finally
            {
                isSimulating = false;
                btnOpenGate.Enabled = true;
            }
        }

        private void BtnCloseGate_Click(object sender, EventArgs e)
        {
            if (isSimulating) return;

            try
            {
                isSimulating = true;
                btnCloseGate.Enabled = false;
                lblStatus.Text = "Status: Closing gate...";

                // Simulate gate closing
                System.Threading.Thread.Sleep(1000);
                isGateOpen = false;

                // Reset controls
                btnPrint.Enabled = false;
                btnOpenGate.Enabled = false;
                lblPlateNumber.Text = "Plate Number: -";
                lblStatus.Text = "Status: Ready";

                // Stop simulation timer
                simulationTimer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing gate: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Status: Error closing gate";
            }
            finally
            {
                isSimulating = false;
                btnCloseGate.Enabled = true;
            }
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            // Simulate vehicle passing through gate
            if (isGateOpen)
            {
                // Add visual feedback for vehicle passing
                cameraPreview.BackColor = Color.Green;
                System.Threading.Thread.Sleep(500);
                cameraPreview.BackColor = Color.Black;
            }
        }

        private string GenerateRandomPlateNumber()
        {
            Random random = new Random();
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "0123456789";
            string plate = "";

            // Generate 2 letters
            for (int i = 0; i < 2; i++)
            {
                plate += letters[random.Next(letters.Length)];
            }

            // Add space
            plate += " ";

            // Generate 4 numbers
            for (int i = 0; i < 4; i++)
            {
                plate += numbers[random.Next(numbers.Length)];
            }

            // Generate 1 letter
            plate += " " + letters[random.Next(letters.Length)];

            return plate;
        }

        private void BtnSimulation_Click(object sender, EventArgs e)
        {
            if (isSimulating)
            {
                // Stop simulation
                simulationTimer.Stop();
                btnSimulation.Text = "Start Simulation";
                lblStatus.Text = "Status: Simulation stopped";
                isSimulating = false;
                
                // Reset controls
                btnCapture.Enabled = true;
                btnPrint.Enabled = false;
                btnOpenGate.Enabled = false;
                btnCloseGate.Enabled = false;
                lblPlateNumber.Text = "Plate Number: -";
            }
            else
            {
                // Start simulation
                simulationTimer.Start();
                btnSimulation.Text = "Stop Simulation";
                lblStatus.Text = "Status: Simulation running";
                isSimulating = true;
                
                // Enable capture button to start the process
                btnCapture.Enabled = true;
            }
        }
    }
} 