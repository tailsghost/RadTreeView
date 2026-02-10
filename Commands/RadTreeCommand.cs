using System.Windows.Input;

namespace RadTreeView.Commands;


public abstract class RadTreeCommand: IDisposable, ICommand
{
    public bool IsDispose { get; private set; }
    public object? CommandParameter { get; set; }
    public string CommandName { get; protected set; }

    public virtual void Dispose()
    {
        CommandParameter = null;
        IsDispose = true;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public abstract void Execute(object? parameter);
    public abstract bool CanExecute(object? parameter);

    public abstract RadTreeCommand Copy();
}

public class RelayCommand : RadTreeCommand
{
    private Action _execute;
    private Func<bool> _canExecute;

    public RelayCommand(string commandName)
    {
        CommandName = commandName;
    }

    public RelayCommand(string commandName, Action exec, Func<bool> can = null)
    {
        CommandName = commandName;
        Init(exec, can);
    }

    public void Init(Action exec, Func<bool> can = null)
    {
        _execute = exec ?? throw new ArgumentNullException(nameof(exec));
        _canExecute = can;
    }

    public override bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public override void Execute(object parameter)
    {
        _execute();
    }

    public override RadTreeCommand Copy()
    {
        var command = new RelayCommand(CommandName);
        command.Init(_execute, _canExecute);
        return command;
    }
}

public class RelayCommand<T> : RadTreeCommand
{
    private Action<T> _execute;
    private Func<T, bool> _canExecute;
    public RelayCommand(string commandName)
    {
        CommandName = commandName;
    }
    public RelayCommand(string commandName, Action<T> exec, Func<T, bool> can = null)
    {
        CommandName = commandName;
        Init(exec, can);
    }

    public void Init(Action<T> exec, Func<T, bool> can = null)
    {
        _execute = exec ?? throw new ArgumentNullException(nameof(exec));
        _canExecute = can;
    }

    public override bool CanExecute(object parameter)
    {
        if (parameter == null && typeof(T).IsValueType)
            return false;

        return _canExecute == null || _canExecute((T)parameter);
    }

    public override void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    public override RadTreeCommand Copy()
    {
        var command = new RelayCommand<T>(CommandName);
        command.Init(_execute, _canExecute);
        return command;
    }
}
