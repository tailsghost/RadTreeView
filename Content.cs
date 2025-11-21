using System.Windows;

namespace RadTreeView;

public class Content : BaseViewModel
{
    private GridLength _width;

    public GridLength Width
    {
        get => _width;
        set => SetValue(ref _width, value);
    }

    private FrameworkElement _value;
    public FrameworkElement Value 
    { 
        get => _value; 
        set => SetValue(ref _value, value); 
    }
}
