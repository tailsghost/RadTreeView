namespace RadTreeView.Commands;

public class RemoveItem : CommandBase
{
    public RemoveItem(string commandName) : base(commandName)
    {
    }

    protected override void Execute(object? item = null)
    {
        if (item is not RowViewModel row) return;
        OnRemoveItem(row);
    }

    private void OnRemoveItem(RowViewModel row)
    {
        if (row is RowViewModelList rowList)
        {
            if (rowList.Parent is not null and RowViewModelList parentList)
            {
                parentList.Children.Remove(rowList);
            }
            var count = rowList.Children.Count;
            for (int i = 0; i < count; i++)
            {
                var child = rowList.Children[0];
                rowList.Children.Remove(child);
                OnRemoveItem(child);
            }
        }
        else
        {
            if (row.Parent is not null and RowViewModelList parentList)
            {
                parentList.Children.Remove(row);
            }
        }
    }
}
