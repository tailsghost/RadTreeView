namespace RadTreeView.Commands;

public class AddItemCommand<T> : RelayCommand<T>
{
    private RowHolderItem _baseHolder;
    public AddItemCommand(string commandName, RowHolderItem baseHolder) : base(commandName)
    {
        _baseHolder = baseHolder;
        Init(Execute);
    }

    private void Execute(T item)
    {
        if (item is not RowViewModelList list) return;

        if(_baseHolder != null)
        {
            list.AddChildrenItem(_baseHolder.Copy());
            list.IsOpenChildren = true;
        }
    }

    public override void Dispose()
    {
        _baseHolder = null;
        base.Dispose();
    }
}
