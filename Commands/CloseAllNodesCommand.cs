namespace RadTreeView.Commands;

public class CloseAllNodesCommand: CommandBase
{
    protected override void Execute(object? item = null)
    {
        if (item is not IEnumerable<RowViewModelList> rowList) return;

        OnCloseAllNodes(rowList);
    }

    public CloseAllNodesCommand() : base("Закрыть все узлы")
    {

    }

    private void OnCloseAllNodes(IEnumerable<RowViewModelList> rows)
    {
        if (rows == null) return;
        foreach (var rowViewModel in rows)
        {
            if (rowViewModel is not RowViewModelList rowViewModelList) continue;
            rowViewModelList.IsOpenChildren = false;
            rowViewModelList.CloseAllNodes(rowViewModelList);
        }
    }
}
