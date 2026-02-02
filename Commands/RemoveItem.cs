namespace RadTreeView.Commands;

public class RemoveItem<T> : RelayCommand<T>
{
    public RemoveItem(string commandName) : base(commandName, Execute)
    {
    }

    private static void Execute(T item)
    {
        if (item is not RowViewModel row) return;
        OnRemoveItem(row);
    }


    private static void OnRemoveItem(RowViewModel row)
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
        else if(row is RowViewModelItem item)
        {
            if (item.Parent is not null and RowViewModelList parentList)
            {
                parentList.Children.Remove(item);
            }
        }
    }
}
