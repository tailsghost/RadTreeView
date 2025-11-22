using RadTreeView.Commands;

namespace RadTreeView;


public class ColumnHolder
{
    public string Title { get; set; }
    public IEnumerable<CommandBase> Commands { get; set; }
}
