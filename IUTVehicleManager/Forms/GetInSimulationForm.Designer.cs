namespace IUTVehicleManager.Forms
{
    partial class GetInSimulationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            
            // Create simulation button
            this.btnSimulation = new System.Windows.Forms.Button();
            this.btnSimulation.Location = new System.Drawing.Point(20, 20);
            this.btnSimulation.Name = "btnSimulation";
            this.btnSimulation.Size = new System.Drawing.Size(120, 40);
            this.btnSimulation.TabIndex = 0;
            this.btnSimulation.Text = "Start Simulation";
            this.btnSimulation.UseVisualStyleBackColor = true;
            this.btnSimulation.Click += new System.EventHandler(this.BtnSimulation_Click);

            // Add controls to form
            this.Controls.Add(this.btnSimulation);

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "GET IN Simulation";
        }

        #endregion

        private System.Windows.Forms.Button btnSimulation;
    }
} 