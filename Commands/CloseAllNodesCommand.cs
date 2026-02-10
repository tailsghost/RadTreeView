namespace RadTreeView.Commands;

public class CloseAllNodesCommand<T> : RelayCommand<T>
{
    private void Execute(T item)
    {
        if(item is RowViewModelList list)
        {
            OnCloseAllNodes(list);
        }
    }

    public CloseAllNodesCommand() : base("Закрыть все узлы")
    {
        Init(Execute);
    }

    private void OnCloseAllNodes(RowViewModelList rows)
    {
        if (rows == null) return;
        foreach (var rowViewModel in rows.Children)
        {
            if (rowViewModel is not RowViewModelList rowViewModelList) continue;
            if (rowViewModelList.Children.Count == 0) continue;
            if (rowViewModelList.IsOpenChildren)
            {
                rowViewModelList.UpdateRowsPosition = true;
                rowViewModelList.IsOpenChildren = false;
                rowViewModelList.UpdateRowsPosition = false;
                rowViewModelList.CloseAllNodes(rowViewModelList);
            }
        }
        rows.IsOpenChildren = false;
    }
}
