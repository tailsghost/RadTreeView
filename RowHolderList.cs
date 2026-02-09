using System.Collections.ObjectModel;

namespace RadTreeView;

public class RowHolderList : RowHolder
{
    public Collection<RowHolder> Rows { get; set; } = [];
}
