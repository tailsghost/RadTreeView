namespace RadTreeView;

public class ColumnViewModel : BaseViewModel
{
    private int _columnIndex;
    private int _columnWidth = 200;
    private int _columnHeight = 25;

    public int ColumnIndex
    {
        get => _columnIndex;
        set => SetValue(ref _columnIndex, value);
    }

    public int ColumnWidth
    {
        get => _columnWidth;
        set => SetValue(ref _columnWidth, value);
    }

    public int ColumnHeight
    {
        get => _columnHeight;
        set => SetValue(ref _columnHeight, value);
    }

    public string Title { get; }

    public bool IsLast { get; }

    public ColumnViewModel(string title, bool isLast)
    {
        Title = title;
        IsLast = isLast;
    }
}
