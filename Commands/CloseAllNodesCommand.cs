namespace RadTreeView.Commands;

public class CloseAllNodesCommand<T>: RelayCommand<T>
{
    private static void Execute(T item)
    {
        OnCloseAllNodes((IEnumerable<RowViewModel>)item);
    }

    private static bool CanExecute(T item)
    {
        return item is IEnumerable<RowViewModel>;
    }

    public CloseAllNodesCommand() : base("Закрыть все узлы", Execute, CanExecute)
    {

    }

    private static void OnCloseAllNodes(IEnumerable<RowViewModel> rows)
    {
        if (rows == null) return;
        foreach (var rowViewModel in rows)
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
    }
}
