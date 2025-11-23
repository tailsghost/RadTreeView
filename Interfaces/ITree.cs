using RadTreeView.Commands;

namespace RadTreeView.Interfaces;

public interface ITree : IDisposable
{
    List<CommandBase> Commands { get; set; }
}
