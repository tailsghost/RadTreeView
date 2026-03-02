using RadTreeView.Commands;

namespace RadTreeView.Interfaces;

public interface ITree : IDisposable
{
    List<RadTreeCommand> Commands { get; set; }
    void AddCommand(RadTreeCommand command);
    void RemoveCommand(RadTreeCommand command);
    List<CommandHolder> CommandHolder { get; }
}
