namespace RadTreeView;

public class RenameItem : RowViewModel
{
    private bool _isRenameMode = false;

    public bool IsRenameMode
    {
        get => _isRenameMode;
        set =>SetValue(ref  _isRenameMode, value);
    }



    public RenameItem(int rows, IList<RowViewModelList> toprows, RowViewModel? parent = null) : base(rows, toprows, parent)
    {
    }
}
