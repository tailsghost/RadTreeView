
namespace RadTreeView;

public class RowViewModelItem : RowViewModel
{
    public RowViewModelItem(int rows, IList<RowViewModelList> toprows, RowViewModel? parent = null) : base(rows, toprows, parent)
    {
    }
}
