using RadTreeView.Interfaces;
using System.Collections.ObjectModel;

namespace RadTreeView;

public interface IRadTreeCommandStrategy
{
    void AddColumnCommand(ITree item, Collection<CommandModel> baseCommand, Collection<CommandHolder> baseHolder, object? parameter);
    void AddRowListCommand(ITree item, Collection<CommandModel> baseCommand, Collection<CommandHolder> baseHolder, object? parameter);
    void AddRowItemCommand(ITree item, Collection<CommandModel> baseCommand, Collection<CommandHolder> baseHolder, object? parameter);
}
