namespace RadTreeView.Commands;

public class CloseAllNodesCommand: CommandBase
{
    protected override void Execute(object? item = null)
    {
        if (item is not IEnumerable<RowViewModel> rowList) return;

        OnCloseAllNodes(rowList);
    }

    public CloseAllNodesCommand() : base("Закрыть все узлы")
    {

    }

    private void OnCloseAllNodes(IEnumerable<RowViewModel> rows)
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
