using RadTreeView.Commands;

namespace RadTreeView;


public class ColumnHolder
{
    public string Title { get; set; }
    public List<CommandBase> Commands { get; set; }
}
