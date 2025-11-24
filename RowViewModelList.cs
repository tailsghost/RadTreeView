
using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace RadTreeView;

public class RowViewModelList : RenameItem
{
    private bool _openChildren = false;


    public ObservableCollection<RowViewModel> Children = [];

    public RowViewModelList(int rows, IList<RowViewModelList> toprows, RowViewModel? parent = null) : base(rows, toprows, parent)
    {
    }

    public Func<RowViewModelList> RaiseRowListHolder;
    public Func<RowViewModelItem> RaiseRowItemHolder;

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
            RaiseRowListHolder = RaiseRowListHolder,
            RaiseRowItemHolder = RaiseRowItemHolder,
            Image = holder.Image,
        };
        if (holder.IsUseStandartCommand)
        {
            List<CommandBase> commandsBase = [
                new OpenAllNodesCommand() { CommandParameter = new List<RowViewModelList> { row } }, 
                new CloseAllNodesCommand() { CommandParameter = new List<RowViewModelList> { row } },
                new AddListCommand("Добавить новый список") { CommandParameter = row },
                new AddItemCommand("Добавить новый айтем") { CommandParameter = row },
                new RemoveItem("Удалить") { CommandParameter = row },
                ];
            if (holder.Commands != null)
            {
                commandsBase.AddRange(holder.Commands);
            }
            return AddChidlren(row, commandsBase) as RowViewModelList;
        }
        else
        {
            return AddChidlren(row, holder.Commands != null ? holder.Commands : []) as RowViewModelList;
        }
    }

    public RowViewModelItem AddChildrenItem(RowHolder holder)
    {
        var row = new RowViewModelItem(_rowCount, _topRows, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = holder.Image,
        };

        if (holder.IsUseStandartCommand)
        {
            List<CommandBase> commandsBase = [new RemoveItem("Удалить") { CommandParameter = row }];
            if (holder.Commands != null)
            {
                commandsBase.AddRange(holder.Commands);
            }
            return AddChidlren(row, commandsBase) as RowViewModelItem;
        }
        else
        {
            return AddChidlren(row, holder.Commands != null ? holder.Commands : []) as RowViewModelItem;
        }
    }

    public RowViewModelList AddChildrenList(RowViewModelList row)
    {
        row.RowOffset = RowOffset + RowOffsetImmutable;
        row.TopParent = TopParent;
        row.RaiseRowListHolder = RaiseRowListHolder;
        row.RaiseRowItemHolder = RaiseRowItemHolder;
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
        Children.Add(item);
        item.DepthChildren = DepthChildren + 1;
        item.TopParent = TopParent;
        item.Parent = this;
        item.UpdateRowsPosition = false;
        IsOpenChildren = true;
        item.UpdateRowsPosition = true;
        return item;
    }

    private RowViewModel AddChidlren(RowViewModel item, IEnumerable<CommandBase> commandBases)
    {
        Children.Add(item);
        item.DepthChildren = DepthChildren + 1;
        item.TopParent = TopParent;
        item.UpdateRowsPosition = false;
        item.Commands = commandBases.ToList();
        IsOpenChildren = true;
        item.UpdateRowsPosition = true;
        return item;
    }
}
