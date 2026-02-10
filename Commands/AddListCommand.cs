namespace RadTreeView.Commands;

public class AddListCommand<T> : RelayCommand<T>
{
    private RowHolderList _baseHolder;
    public AddListCommand(string commandName, RowHolderList baseHolder) : base(commandName)
    {
        _baseHolder = baseHolder;
        Init(Execute);
    }

    protected void Execute(T item)
    {
        if (item is not RowViewModelList list) return;

        if (_baseHolder != null)
        {
            list.AddChildrenList(_baseHolder.Copy());
            list.IsOpenChildren = true;
        }
    }
}
