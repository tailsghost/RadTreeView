using RadTreeView.Commands;
using RadTreeView.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace RadTreeView;

public abstract class RowViewModel : BaseViewModel, ITree
{
    public const int RowOffsetImmutable = 25;
    protected int _rowCount;
    private int _rowIndex;
    private int _rowOffset = 0;
    private int _rowHeight = 25;
    private RowViewModelList _topParent;

    private int _depthChildren = 0;

    protected IList<RowViewModelList> _topRows;

    public int GetTopRowsCount() => _topRows.Count;

    public IList<RowViewModelList> GetTopRows() => _topRows.ToList();

    public bool IsFirstTopRow()
    {
        if (_topRows.Count == 0) return false;
        return _topRows[0] == this;
    }

    public bool RowListEqualsLast()
    {
        if (_topRows.Count == 0) return false;
        return _topRows.Last() == this;
    }

    public List<CommandBase> Commands { get; set; }

    public RowViewModelList TopParent
    {
        get => _topParent;
        set
        {
            _topParent = value;
        }
    }

    public bool UpdateRowsPosition { get; set; }

    public int DepthChildren 
    { 
        get => _depthChildren; 
        set
        {
            if (_depthChildren != 0) return;
            _depthChildren = value;
        }
    }

    public RowViewModel Parent { get; private set; }

    public int GetIndexRowItem()
    {
        var index = 0;
        if (GetIndex(_topRows, this, ref index))
        {
            return index;
        }
        return -1;
    }


    public RowViewModel? GetNextParentItem()
    {
        if (Parent != null)
        {
            if(Parent is RowViewModelList list)
            {
                if (list.Children.Last() == this) return this;
                for (var i = 0; i < list.Children.Count; i++)
                {
                    var child = list.Children[i];
                    if (child == this) return child;
                }
            }
        }
        else
        {
            if (_topRows.Last() == this) return this;
            for (var i = 0; i < _topRows.Count; i++)
            {
                var child = _topRows[i];
                if (child == this) return child;
            }
        }

        return null;
    }

    public ImageSource Image { get; set; }

    protected Thickness _thickness;
    public List<Content> RowContents { get; }

    public Content MainContent { get; }

    public virtual Thickness BorderThickness
    {
        get => _thickness;
        set => SetValue(ref _thickness, value);
    }

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

    public RowViewModel(int rows, IList<RowViewModelList> toprows, IEnumerable<Content> contents, RowViewModel? parent = null)
    {
        Parent = parent;
        RowContents = new List<Content>(rows);
        _rowCount = rows;
        _topRows = toprows;
        foreach (var content in contents)
        {
            RowContents.Add(content);
        }
        MainContent = RowContents[0];
        BorderThickness = new Thickness(0.5, 0, 0, 0.5);
    }



    public void SelectedRow()
    {

    }


    protected bool GetIndex(IEnumerable<RowViewModel> items, RowViewModel searchItem, ref int index)
    {
        foreach (var it in items)
        {
            if (it == searchItem)
            {
                return true;
            }
            index++;
            if(it is RowViewModelList list)
            {
                if (GetIndex(list.Children, searchItem, ref index))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void Dispose()
    {
        
    }
}
