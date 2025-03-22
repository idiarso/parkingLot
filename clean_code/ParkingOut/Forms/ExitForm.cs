using System;
using System.Windows.Forms;
using ParkingOut.Models;

namespace ParkingOut.Forms
{
    public class ExitForm : Form
    {
        private readonly User _currentUser;
        
        public ExitForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Vehicle Exit";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
} 