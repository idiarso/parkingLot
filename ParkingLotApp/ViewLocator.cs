using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ParkingLotApp.ViewModels;
using ParkingLotApp.Views;

namespace ParkingLotApp;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (param is DashboardViewModel) return new DashboardView();
        if (param is VehicleEntryViewModel) return new VehicleEntryView();
        if (param is VehicleExitViewModel) return new VehicleExitView();
        if (param is ReportsViewModel) return new ReportsView();
        if (param is UserManagementViewModel) return new UserManagementView();
        if (param is SettingsViewModel) return new SettingsView();
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        if (data is DashboardViewModel) return true;
        if (data is VehicleEntryViewModel) return true;
        if (data is VehicleExitViewModel) return true;
        if (data is ReportsViewModel) return true;
        if (data is UserManagementViewModel) return true;
        if (data is SettingsViewModel) return true;
        return false;
    }
}
