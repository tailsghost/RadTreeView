namespace RadTreeView.Commands;

public class AddItemCommand<T> : RelayCommand<T>
{
    public AddItemCommand(string commandName) : base(commandName, Execute)
    {
    }

    private static void Execute(T item)
    {
        if (item is not RowViewModelList list) return;

        if(list.RaiseRowItemHolder != null)
        {
            list.AddChildrenItem(list.RaiseRowItemHolder());
            list.IsOpenChildren = true;
        }
    }
}
