
namespace RadTreeView;

public class RowViewModelItem : RenameItem
{
    public RowViewModelItem(int rows, IList<RowViewModelList> toprows, RowViewModel? parent = null) : base(rows, toprows, parent)
    {
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
