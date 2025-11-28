using RadTreeView.RelayCommands;

namespace RadTreeView.Commands;

public abstract class CommandBase : IDisposable
{
    public bool IsDispose { get; private set; }
    public object? CommandParameter { get; set; }
    public string CommandName { get; }
    public RelayCommand Command { get; private set; }

    protected CommandBase(string commandName)
    {
        Command = new RelayCommand(Execute, CanExecute);
        CommandName = commandName;
    }

    public void RaiseExecute(object? item = null) => Execute(item);

    protected abstract void Execute(object? item = null);
    protected virtual bool CanExecute(object? parameter = null)
    {
        return true;
    }

    public virtual void Dispose()
    {
        Command.Dispose();
        Command = null;
        CommandParameter = null;
        IsDispose = true;
    }
}
