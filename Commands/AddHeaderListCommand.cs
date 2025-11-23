namespace RadTreeView.Commands;

public class AddHeaderListCommand : CommandBase
{
    private Action _create;
    public AddHeaderListCommand(string commandName, Action createNewList) : base(commandName)
    {
        _create = createNewList;
    }

    protected override void Execute(object? item = null)
    {
        _create?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        _create = null;
    }
}
