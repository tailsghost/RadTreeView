using System.Collections.Generic;

namespace RadTreeView.Commands;

public class OpenAllNodesCommand<T> : RelayCommand<T>
{
    public OpenAllNodesCommand() : base("Открыть все узлы")
    {
        Init(Execute);
    }

    private void Execute(T item)
    {
        if (item is not RowViewModelList list) return;
        foreach (var rowViewModel in list.Children)
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
        list.IsOpenChildren = true;
    }
}
