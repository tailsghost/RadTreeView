
using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace RadTreeView;

public class RowViewModelList : RowViewModel
{
    private bool _openChildren = false;


    public ObservableCollection<RowViewModel> Children = [];

    public RowViewModelList(int rows, IList<RowViewModelList> toprows, IEnumerable<Content> contents, RowViewModel? parent = null) : base(rows, toprows, contents, parent)
    {
    }

    public Func<RowHolder> RaiseRowListHolder;
    public Func<RowHolder> RaiseRowItemHolder;

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
        var row = new RowViewModelList(_rowCount, _topRows, holder.Contents, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            RaiseRowListHolder = RaiseRowListHolder,
            RaiseRowItemHolder = RaiseRowItemHolder,
            Image = new BitmapImage(
            new Uri("pack://application:,,,/RadTreeViewTest;component/Assets/Project_Property_Icon.png")),
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
        var row = new RowViewModelItem(_rowCount, _topRows, holder.Contents, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = new BitmapImage(
                new Uri("pack://application:,,,/RadTreeViewTest;component/Assets/TagItem.png")),
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

    private RowViewModel AddChidlren(RowViewModel item, List<CommandBase> commands)
    {
        Children.Add(item);
        item.DepthChildren = DepthChildren + 1;
        item.Commands = commands;
        item.TopParent = TopParent;
        item.UpdateRowsPosition = false;
        IsOpenChildren = true;
        item.UpdateRowsPosition = true;
        return item;
    }
}
