using System.Windows.Input;

namespace RadTreeView.RelayCommands;

public class RelayCommand(Action<object> exec, Func<object, bool> can = null) : ICommand, IDisposable
{
    private Action<object?> _execute { get; set; } = exec;
    private Func<object, bool> _can { get; set; } = can;

    public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _execute?.Invoke(parameter);
        }
    }

    public void Dispose()
    {
        _execute = null;
        _can = null;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
