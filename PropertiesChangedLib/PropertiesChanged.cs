using System.ComponentModel;
using PropertyChanged;

namespace PropertiesChangedLib
{
    [AddINotifyPropertyChangedInterface]
    public class PropertiesChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
    }
}
