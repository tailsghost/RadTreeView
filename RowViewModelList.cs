
using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace RadTreeView;

public class RowViewModelList : RowViewModel
{
    private bool _openChildren = false;

    public bool IsFolder { get; set; }
    public ObservableCollection<RowViewModel> Children = [];

    public RowViewModelList(int rows, IList<RowViewModelList> toprows, RowViewModelList? parent = null) : base(rows, toprows, parent)
    {
    }

    public void ChangeState()
    {
        IsOpenChildren = !IsOpenChildren;
    }

    public bool IsOpenChildren
    {
        get => _openChildren;
        set => SetValue(ref _openChildren, value);
    }


    public RowViewModelList AddChildrenList(RowHolder holder)
    {
        var row = new RowViewModelList(_rowCount, _topRows, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = holder.Image,
        };
        return AddChidlren(row) as RowViewModelList;
    }

    public RowViewModelItem AddChildrenItem(RowHolder holder)
    {
        var row = new RowViewModelItem(_rowCount, _topRows, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = holder.Image,
        };

        return AddChidlren(row) as RowViewModelItem;
    }

    public RowViewModelList AddChildrenListInsert(RowViewModelList row, int index)
    {
        row.RowOffset = RowOffset + RowOffsetImmutable;
        row.TopParent = TopParent;
        return AddChidlrenInsert(row, index) as RowViewModelList;
    }

    public RowViewModelList AddChildrenList(RowViewModelList row)
    {
        row.RowOffset = RowOffset + RowOffsetImmutable;
        row.TopParent = TopParent;
        return AddChidlren(row) as RowViewModelList;
    }

    public RowViewModelItem AddChildrenItem(RowViewModelItem row)
    {
        row.RowOffset = RowOffset + RowOffsetImmutable;
        row.TopParent = TopParent;
        return AddChidlren(row) as RowViewModelItem;
    }


    public void CloseAllNodes(RowViewModelList row)
    {
        foreach (var child in row.Children)
        {
            if (child is not RowViewModelList rowList) continue;
            if (rowList.IsOpenChildren)
            {
                rowList.UpdateRowsPosition = true;
                rowList.IsOpenChildren = false;
                rowList.UpdateRowsPosition = false;
                CloseAllNodes(rowList);
            }
        }
    }

    public void OpenAllNodes(RowViewModelList row)
    {
        foreach (var child in row.Children)
        {
            if (child is not RowViewModelList rowList) continue;
            if (!rowList.IsOpenChildren)
            {
                rowList.UpdateRowsPosition = true;
                rowList.IsOpenChildren = true;
                rowList.UpdateRowsPosition = false;
                OpenAllNodes(rowList);
            }
        }
    }

    private RowViewModel AddChidlren(RowViewModel item)
    {
        item.Parent = this;
        item.DepthChildren = DepthChildren + 1;
        Children.Add(item);
        item.TopParent = TopParent;
        item.Parent = this;
        item.UpdateRowsPosition = false;
        item.UpdateRowsPosition = true;
        return item;
    }

    private RowViewModel AddChidlrenInsert(RowViewModel item, int index)
    {
        item.Parent = this;
        item.DepthChildren = DepthChildren + 1;
        Children.Insert(index,item);
        item.TopParent = TopParent;
        item.Parent = this;
        item.UpdateRowsPosition = false;
        item.UpdateRowsPosition = true;
        return item;
    }

    public override void Dispose()
    {
        foreach(var child in Children)
        {
            child.Dispose(); 
        }
        Children.Clear();
        base.Dispose();
    }
}
