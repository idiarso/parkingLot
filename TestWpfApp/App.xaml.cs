using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TestWpfApp.Services;
using TestWpfApp.Services.Interfaces;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IAppLogger Logger { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger = new FileLogger();
            TestOutput.OutputStartupMessage();
        }
    }
}
