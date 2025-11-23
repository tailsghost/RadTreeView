
namespace RadTreeView;

public class RowViewModelItem : RowViewModel
{
    public RowViewModelItem(int rows, IList<RowViewModelList> toprows, IEnumerable<Content> contents, RowViewModel? parent = null) : base(rows, toprows, contents, parent)
    {
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
