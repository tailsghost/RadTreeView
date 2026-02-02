using RadTreeView.Commands;
using System.Collections.ObjectModel;

namespace RadTreeView;

public class RadTreeViewModel : BaseViewModel
{
    private int _count;
    private RowViewModel _selectedItem;

    public ObservableCollection<RowViewModelList> Rows = [];
    public ObservableCollection<ColumnViewModel> Columns;

    public bool IsInitialMode = false;

    public int Count 
    { 
        get => _count; 
        set => SetValue(ref _count, value); 
    }

    public int ColumnCount
    {
        get => Columns.Count;
    }
    public int RowsCount
    {
        get => Rows.Count;
    }

    public Func<RowViewModelItem> RaiseRowItemHolder;
    public Func<RowViewModelList> RaiseRowListHolder;

    public event Action<RowViewModel> AddItem;
    public event Action<RowViewModel> ChangeSelectedItem;

    public event Action InitialMenuHandler;

    public void InitialMenu()
    {
        InitialMenuHandler?.Invoke();
    }

    public RowViewModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if(SetValue(ref _selectedItem, value))
                ChangeSelectedItem?.Invoke(value);
        }
    }

    public void RaiseAddItem(RowViewModel item) => AddItem?.Invoke(item);

    public RowViewModelList AddRow(RowHolder holder)
    {
        var row = new RowViewModelList(Columns.Count, Rows)
        {
            Image = holder.Image,
        };
        if (holder.IsUseStandartCommand)
        {
            List<RadTreeCommand> commandsBase = [
                    new OpenAllNodesCommand<IEnumerable<RowViewModel>>() { CommandParameter = new List<RowViewModelList> { row } },
                    new CloseAllNodesCommand<IEnumerable<RowViewModel>>() { CommandParameter = new List<RowViewModelList> { row } },
                    new AddListCommand<RowViewModelList>("Добавить новый список") { CommandParameter = row },
                    new AddItemCommand<RowViewModelList>("Добавить новый айтем") { CommandParameter = row },
                    new RemoveHeaderListCommand<RowViewModelList>("Удалить список", item => Rows.Remove(item)) { CommandParameter = row },
                ];
            if (holder.Commands != null)
            {
                commandsBase.AddRange(holder.Commands);
            }
            return Add(row, commandsBase);
        }
        else
        {
            return Add(row, holder.Commands != null ? holder.Commands : []);
        }
    }


    public RowViewModel? FindRowToName(string name)
    {
        foreach(var row in Rows)
        {
            var find = FindRowToName(row, name);
            if (find != null)
                return find;
        }
        return null;
    }

    private RowViewModel FindRowToName(RowViewModel row, string name)
    {
        if (row.Title == name) return row;
        if(row is RowViewModelList list)
        {
            foreach(var child in list.Children)
            {
                var find = FindRowToName(child, name);
                if (find != null) return find;
            }
        }
        return null;
    }

    public RowViewModelList AddRow(RowViewModelList list)
    {
        Rows.Add(list);
        list.TopParent = list;
        list.RaiseRowListHolder = RaiseRowListHolder;
        list.RaiseRowItemHolder = RaiseRowItemHolder;
        OnPropertyChanged(nameof(RowsCount));
        return list;
    }

    public RowViewModelList AddRow(RowViewModelList list, int index)
    {
        Rows.Insert(index,list);
        list.TopParent = list;
        list.RaiseRowListHolder = RaiseRowListHolder;
        list.RaiseRowItemHolder = RaiseRowItemHolder;
        OnPropertyChanged(nameof(RowsCount));
        return list;
    }

    private RowViewModelList Add(RowViewModelList row, IEnumerable<RadTreeCommand> commands)
    {
        Rows.Add(row);
        row.TopParent = row;
        row.Commands = commands.ToList();
        row.RaiseRowListHolder = RaiseRowListHolder;
        row.RaiseRowItemHolder = RaiseRowItemHolder;
        OnPropertyChanged(nameof(RowsCount));
        return row;
    }


    public void AddColumn(List<ColumnHolder> columnNames)
    {
        if (Columns.Count != 0)
        {
            throw new InvalidOperationException("Columns уже инициализирован!");
        }

        Init(columnNames);
    }

    public bool IsInit => Columns.Count != 0;

    public RadTreeViewModel()
    {
        Columns = new();
    }

    private void Init(List<ColumnHolder> columnNames)
    {
        ColumnViewModel[] models = new ColumnViewModel[columnNames.Count];

        for (var i = 0; i < columnNames.Count; i++)
        {
            var it = columnNames[i];
            models[i] = new ColumnViewModel(it.Title, columnNames.Count - 1 == i)
            {
                Commands = it.Commands,
                ColumnIndex = i
            };
        }

        for (var i = 0; i < models.Length; i++)
        {
            Columns.Add(models[i]);
        }
    }
}
