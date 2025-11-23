namespace RadTreeView.Commands;

public class AddListCommand : CommandBase
{
    public AddListCommand(string commandName) : base(commandName)
    {
    }

    protected override void Execute(object? item = null)
    {
        if (item is not RowViewModelList list) return;

        if (list.RaiseRowListHolder != null)
        {
            list.AddChildrenList(list.RaiseRowListHolder());
        }
    }
}
