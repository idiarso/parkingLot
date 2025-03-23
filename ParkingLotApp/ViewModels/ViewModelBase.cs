using System.ComponentModel;
using ReactiveUI;

namespace ParkingLotApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
