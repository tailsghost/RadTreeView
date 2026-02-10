using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RadTreeView;

public abstract class RowHolder
{
    public Collection<CommandModel> Commands { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public ImageSource Image { get; set; }

    public abstract RowHolder Copy();
}
