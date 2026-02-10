namespace RadTreeView.Commands;

public class RemoveHeaderListCommand<T> : RelayCommand<T>
{
    public RemoveHeaderListCommand(string commandName, Action<T> removeList) : base(commandName)
    {
        Init(removeList);
    }
}
