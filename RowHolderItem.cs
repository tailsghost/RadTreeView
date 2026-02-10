namespace RadTreeView;

public class RowHolderItem : RowHolder
{
    public override RowHolder Copy()
    {
        return new RowHolderItem()
        {
            Commands = [.. Commands],
            Title = Title,
            Image = Image,
        };
    }
}
