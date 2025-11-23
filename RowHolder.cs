using RadTreeView.Commands;

namespace RadTreeView;

public class RowHolder
{
    public List<Content> Contents { get; set; } = [];
    public List<CommandBase>? Commands { get; set; } = [];
    public bool IsUseStandartCommand { get; set; } = true;
}
