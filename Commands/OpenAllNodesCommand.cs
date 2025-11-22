namespace RadTreeView.Commands;

public class OpenAllNodesCommand : CommandBase
{
    protected override void Execute(object? item = null)
    {
        if (item is not IEnumerable<RowViewModelList> rowList) return;

        OnOpenAllNodes(rowList);
    }

    public OpenAllNodesCommand() : base("Открыть все узлы")
    {
        
    }

    private void OnOpenAllNodes(IEnumerable<RowViewModelList> rows)
    {
        if (rows == null) return;
        foreach (var rowViewModel in rows)
        {
            if (rowViewModel is not RowViewModelList rowViewModelList) continue;
            rowViewModelList.IsOpenChildren = true;
            rowViewModelList.OpenAllNodes(rowViewModelList);
        }
    }
}
