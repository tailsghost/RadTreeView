using RadTreeView.Interfaces;

namespace RadTreeView;

public interface IRadTreeCommandStrategy
{
    void AddColumnCommand(ITree item, ColumnHolder holder, ICollectionCommand commands);
    void AddRowCommand(ITree item,RowHolder holder, ICollectionCommand commands);
}
