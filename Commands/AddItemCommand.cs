namespace RadTreeView.Commands;

public class AddItemCommand : CommandBase
{
    public AddItemCommand(string commandName) : base(commandName)
    {
    }

    protected override void Execute(object? item = null)
    {
        if (item is not RowViewModelList list) return;

        if(list.RaiseRowItemHolder != null)
        {
            list.AddChildrenItem(list.RaiseRowItemHolder());
        }
    }
}
