using RadTreeView.Commands;

namespace RadTreeView.Interfaces;

public interface ITree : IDisposable
{
    List<RadTreeCommand> Commands { get; set; }
    List<CommandHolder> CommandHolder { get; }
}
