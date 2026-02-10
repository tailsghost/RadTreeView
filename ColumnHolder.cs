using RadTreeView.Commands;
using System.Collections.ObjectModel;

namespace RadTreeView;


public class ColumnHolder
{
    public Collection<CommandHolder> Commands { get; set; } = [];
    public string Title { get; set; } = string.Empty;
}
