namespace RadTreeView;

public class RowViewModel : BaseViewModel
{
    private int _rowCount;
    private int _rowIndex;
    private int _rowOffset = 25;
    private int _rowHeight = 25;

    private IList<RowViewModel> _topRows;

    public List<Content> RowContents { get; private set; }

    private List<RowViewModel> Children = [];

    public int RowIndex
    {
        get => _rowIndex;
        set => SetValue(ref _rowIndex, value);
    }

    public int RowOffset
    {
        get => _rowOffset;
        set => SetValue(ref _rowOffset, value);
    }

    public int RowHeight
    {
        get => _rowHeight;
        set => SetValue(ref _rowHeight, value);
    }

    public void AddChildren(IEnumerable<Content> contents)
    {
        Children.Add(new RowViewModel(_rowCount, _topRows, contents));
    }

    public RowViewModel(int rows, IList<RowViewModel> toprows, IEnumerable<Content> contents)
    {
        RowContents = new List<Content>(rows);
        _rowCount = rows;
        _topRows = toprows;
        foreach(var content in contents)
        {
            RowContents.Add(content);
        }
    }
}
