using System.Windows.Input;

namespace RadTreeView.Commands;


public class RadTreeCommand: IDisposable
{
    public bool IsDispose { get; private set; }
    public object? CommandParameter { get; set; }
    public string CommandName { get; protected set; }

    public virtual void Dispose()
    {
        CommandParameter = null;
        IsDispose = true;
    }
}

public class RelayCommand : RadTreeCommand, ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(string commandName, Action exec, Func<bool> can = null)
    {
        CommandName = commandName;
        _execute = exec ?? throw new ArgumentNullException(nameof(exec));
        _canExecute = can;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public void Execute(object parameter)
    {
        _execute();
    }
}

public class RelayCommand<T> : RadTreeCommand, ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    public RelayCommand(string commandName, Action<T> exec, Func<T,bool> can = null)
    {
        CommandName = commandName;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter)
    {
        if (parameter == null && typeof(T).IsValueType)
            return false;

        return _canExecute == null || _canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}
