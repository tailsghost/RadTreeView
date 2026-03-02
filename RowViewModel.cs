using RadTreeView.Commands;
using RadTreeView.Interfaces;
using System.Windows;
using System.Windows.Input;
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
    private string _title = string.Empty;
    private string _description = string.Empty;
    private bool _isEnable = true;
    private int _depthChildren = 0;

    public bool IsCommandVisibility { 
        get => field;
        set => SetValue(ref field, value);
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsRenameMode
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public List<CommandHolder> CommandHolder { get; set; } = [];

    public bool IsEnable
    {
        get => _isEnable;
        set => SetValue(ref _isEnable, value);
    }

    private void UpdateIsEnable(RowViewModel row, bool value)
    {
        row.IsEnable = value;
        if(row is RowViewModelList rowList)
        {
            foreach(var child in rowList.Children)
            {
                UpdateIsEnable(child, value);
            }
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if(_title  != value)
            {
                var old = _title;
                _title = value;
                TopParent?.Owner?.Rename(this, old);
                OnPropertyChanged();
            }
        }
    }

    public string Description
    {
        get => _description;
        set => SetValue(ref _description, value);
    }

    protected IList<RowViewModelList> _topRows;

    public int GetTopRowsCount() => _topRows.Count;

    public IList<RowViewModelList> GetTopRows() => [.. _topRows];

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

    public List<RadTreeCommand> Commands { get; set; } = [];

    public void AddCommand(RadTreeCommand command)
    {
        Commands.Add(command);
        IsCommandVisibility = Commands.Count>0;
    }

    public void RemoveCommand(RadTreeCommand command)
    {
        Commands.Remove(command);
        IsCommandVisibility = Commands.Count > 0;
    }

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

    public RowViewModelList Parent { get; set; }

    public int GetIndexRowItem()
    {
        var index = 0;
        foreach (var row in _topRows)
        {
            if (GetIndex(row, this, ref index))
            {
                return index;
            }
        }
        return index;
    }


    public RowViewModel? GetNextParentItem()
    {
        if (Parent != null)
        {
            if (Parent is RowViewModelList list)
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

    public RowViewModel(int rows, IList<RowViewModelList> toprows, RowViewModelList? parent = null)
    {
        Parent = parent;
        _rowCount = rows;
        _topRows = toprows;
        BorderThickness = new Thickness(0.5, 0, 0, 0.5);
    }



    public void SelectedRow()
    {
        TopParent?.Owner?.SelectedItem = this;
    }


    protected bool GetIndex(RowViewModel current, RowViewModel searchItem, ref int index)
    {
        if (current is not RowViewModelList rowList)
        {
            index++;
            return false;
        }
        index++;
        if (current == searchItem) return true;
        if (!rowList.IsOpenChildren)
        {
            return current == searchItem;
        }
        foreach (var it in rowList.Children)
        {
            if (it == searchItem)
            {
                return true;
            }
            if (GetIndex(it, searchItem, ref index))
            {
                return true;
            }

        }

        return false;
    }

    public virtual void Dispose()
    {
        CommandHolder.Clear();
        _topRows = null;
        foreach(var command in Commands)
            command.Dispose();
        Commands.Clear();
        TopParent = null;
        Parent = null;
    }
}
