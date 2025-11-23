namespace RadTreeView.Commands;

public class OpenAllNodesCommand : CommandBase
{
    protected override void Execute(object? item = null)
    {
        if (item is not IEnumerable<RowViewModel> rowList) return;

        OnOpenAllNodes(rowList);
    }

    public OpenAllNodesCommand() : base("Открыть все узлы")
    {
        
    }

    private void OnOpenAllNodes(IEnumerable<RowViewModel> rows)
    {
        if (rows == null) return;
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
}
