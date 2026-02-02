using System.Collections.Generic;

namespace RadTreeView.Commands;

public class OpenAllNodesCommand<T> : RelayCommand<T>
{
    public OpenAllNodesCommand() : base("Открыть все узлы", Execute, CanExecute)
    {
        
    }

    private static void Execute(T item)
    {
        var rows = (IEnumerable<RowViewModel>)item;
        foreach (var rowViewModel in rows)
        {
            if (rowViewModel is not RowViewModelList rowViewModelList) continue;
            if (rowViewModelList.Children.Count == 0) continue;
            if (!rowViewModelList.IsOpenChildren)
            {
                rowViewModelList.UpdateRowsPosition = true;
                rowViewModelList.IsOpenChildren = true;
                rowViewModelList.UpdateRowsPosition = false;
                rowViewModelList.OpenAllNodes(rowViewModelList);
            }
        }
    }

    private static bool CanExecute(T item)
    {
        return item is IEnumerable<RowViewModel>;
    }
}
