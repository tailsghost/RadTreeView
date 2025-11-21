using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace RadTreeView;

public class RowViewModel : BaseViewModel
{
    public const int RowOffsetImmutable = 25;
    private int _rowCount;
    private int _rowIndex;
    private int _rowOffset = 0;
    private int _rowHeight = 25;

    private bool _openChildren = false;

    private IList<RowViewModel> _topRows;

    public int GetTopRowsCount() => _topRows.Count;

    public RowViewModel Parent { get; private set; }

    public int GetIndexRowItem()
    {
        var index = 0;
        if(GetIndex(_topRows, this, ref index))
        {
            return index; 
        }
        return -1;
    }

    public bool FirstCompleted { get; set; }

    public RowViewModel? GetNextParentItem()
    {
        if(Parent != null)
        {
            if (Parent.Children.Last() == this) return this;
            for(var i =0; i < Parent.Children.Count; i++)
            {
                var child = Parent.Children[i];
                if (child == this) return child;
            }
        }
        else
        {
            if(_topRows.Last() == this) return this;
            for (var i = 0; i < _topRows.Count; i++)
            {
                var child = _topRows[i];
                if (child == this) return child;
            }
        }

            return null;
    }

    public ImageSource Image { get; set; }

    public bool IsOpenChildren
    {
        get => _openChildren;
        set => SetValue(ref _openChildren, value);
    }

    protected Thickness _thickness;
    public List<Content> RowContents { get; }

    public Content MainContent { get; }

    public virtual Thickness BorderThickness
    {
        get => _thickness;
        set => SetValue(ref _thickness, value);
    }

    public ObservableCollection<RowViewModel> Children = [];

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

    public RowViewModel AddChildren(IEnumerable<Content> contents)
    {
        var row = new RowViewModel(_rowCount, _topRows, contents, this)
        {
            RowOffset = _rowOffset,
        };
        Children.Add(row);
        IsOpenChildren = true;
        return row;
    }

    public RowViewModel(int rows, IList<RowViewModel> toprows, IEnumerable<Content> contents, RowViewModel? parent = null)
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



    private bool GetIndex(IEnumerable<RowViewModel> items, RowViewModel searchItem, ref int index)
    {
        foreach (var it in items)
        {
            if (it == searchItem)
            {
                return true;
            }
            index++;
            if (GetIndex(it.Children, searchItem, ref index))
            {
                return true;
            }
        }

        return false;
    }
}
