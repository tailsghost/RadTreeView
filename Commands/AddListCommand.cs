namespace RadTreeView.Commands;

public class AddListCommand<T> : RelayCommand<T>
{
    public AddListCommand(string commandName) : base(commandName, Execute)
    {
    }

    protected static void Execute(T item)
    {
        if (item is not RowViewModelList list) return;

        if (list.RaiseRowListHolder != null)
        {
            list.AddChildrenList(list.RaiseRowListHolder());
            list.IsOpenChildren = true;
        }
    }
}
