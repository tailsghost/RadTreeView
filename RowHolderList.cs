using System.Collections.ObjectModel;

namespace RadTreeView;

public class RowHolderList : RowHolder
{
    public Collection<RowHolder> Rows { get; set; } = [];

    public override RowHolder Copy()
    {
        return new RowHolderList()
        {
            Rows = [.. Rows],
            Commands = [.. Commands],
            Image = Image,
            Title = Title,
        };
    }
}
