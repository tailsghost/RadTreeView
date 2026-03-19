using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace RadTreeView;


public class ColumnHolder
{
    public Collection<CommandHolder> Commands { get; set; } = [];
    public string Title { get; set; } = string.Empty;

    public int ColumnWidth { get; set; } = 0;
}
