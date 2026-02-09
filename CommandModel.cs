using RadTreeView.Commands;
using System.Windows;

namespace RadTreeView;

public class CommandModel : DependencyObject
{
    public string Header { get; set; }

    public RadTreeCommand Command 
    { 
        get; set;
    }
}
