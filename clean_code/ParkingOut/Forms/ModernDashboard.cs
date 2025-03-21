using System.Windows.Forms;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin.Forms
{
    public partial class ModernDashboard : Form
    {
        private static readonly IAppLogger _logger = new FileLogger();
        
        public ModernDashboard()
        {
            InitializeComponent();
        }
    }
} 