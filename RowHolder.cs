using RadTreeView.Commands;
using System.Windows.Media;

namespace RadTreeView;

public class RowHolder
{
    public List<Content> Contents { get; set; } = [];
    public List<CommandBase>? Commands { get; set; } = [];
    public bool IsUseStandartCommand { get; set; } = true;
    public ImageSource Image { get; set; }
}
