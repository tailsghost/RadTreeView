using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RadTreeView;

public class RowHolder
{
    public Collection<string> CommandsName { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public ImageSource Image { get; set; }
}
