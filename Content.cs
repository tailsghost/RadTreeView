using System.Windows;

namespace RadTreeView;

public class Content : BaseViewModel
{
    private FrameworkElement _value;
    public FrameworkElement Value 
    { 
        get => _value; 
        set => SetValue(ref _value, value); 
    }
}
