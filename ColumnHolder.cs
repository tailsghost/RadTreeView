using RadTreeView.Commands;
using System.Collections.ObjectModel;

namespace RadTreeView;


public class ColumnHolder
{
    public Collection<CommandHolder> CommandsName { get; set; } = [];
    public List<RadTreeCommand> Commands
    {
        get;
        set;
    } = [];
    public string Title { get; set; } = string.Empty;
}
