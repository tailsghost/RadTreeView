namespace RadTreeView.Commands;

public class RemoveHeaderListCommand : CommandBase
{
    private Action<RowViewModelList> _removeList;
    public RemoveHeaderListCommand(string commandName, Action<RowViewModelList> removeList) : base(commandName)
    {
        _removeList = removeList;
    }

    protected override void Execute(object? item = null)
    {
        if(item is not RowViewModelList list) return;
        _removeList?.Invoke(list);
    }
}
