using System.Collections.ObjectModel;

namespace RadTreeView;

public class RadTreeViewModel : BaseViewModel, IDisposable
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


    public event Action<RowViewModel> AddItem;
    public event Action<RowViewModel> ChangeSelectedItem;

    public event Action<RowViewModel, string> RenameItemAction;

    internal void Rename(RowViewModel rowViewModel, string oldName)
    {
        RenameItemAction?.Invoke(rowViewModel, oldName);
    }

    public RowViewModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetValue(ref _selectedItem, value))
                ChangeSelectedItem?.Invoke(value);
        }
    }

    public void RaiseAddItem(RowViewModel item) => AddItem?.Invoke(item);

    public void AddRows(List<RowHolderList> holder)
    {
        foreach (RowHolderList rowHolder in holder)
        {
            var row = new RowViewModelList(Columns.Count, Rows)
            {
                Image = rowHolder.Image,
                Title = rowHolder.Title,
            };
            AddRows(rowHolder.Rows, row);
            Add(row);
        }
    }

    private void AddRows(IEnumerable<RowHolder> holder, RowViewModelList parent = null)
    {
        foreach(RowHolder rowHolder in holder)
        {
            if(rowHolder is RowHolderList rowHolderList)
            {
                var result = parent.AddChildrenList(rowHolder);
                if (rowHolderList.Rows.Count > 0)
                {
                    AddRows(rowHolderList.Rows, result);
                }
            }
            if(rowHolder is RowHolderItem rowHolderItem)
            {
                parent.AddChildrenItem(rowHolderItem);
            }
        }
    }

    public RowViewModelList AddRow(RowHolder holder)
    {
        var row = new RowViewModelList(Columns.Count, Rows)
        {
            Image = holder.Image,
        };
        return Add(row);
    }


    public RowViewModel? FindRowToName(string name)
    {
        foreach (var row in Rows)
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
        if (row is RowViewModelList list)
        {
            foreach (var child in list.Children)
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
        list.Owner = this;
        OnPropertyChanged(nameof(RowsCount));
        return list;
    }

    public RowViewModelList AddRow(RowViewModelList list, int index)
    {
        Rows.Insert(index, list);
        list.TopParent = list;
        list.Owner = this;
        OnPropertyChanged(nameof(RowsCount));
        return list;
    }

    private RowViewModelList Add(RowViewModelList row)
    {
        Rows.Add(row);
        row.TopParent = row;
        row.Owner = this;
        OnPropertyChanged(nameof(RowsCount));
        return row;
    }


    public void AddColumn(List<ColumnHolder> columnNames)
    {
        Init(columnNames);
    }

    public bool IsInit => Columns.Count != 0;

    public RadTreeViewModel()
    {
        Columns = [];
    }

    private void Init(List<ColumnHolder> columnNames)
    {
        ColumnViewModel[] models = new ColumnViewModel[columnNames.Count];

        for (var i = 0; i < columnNames.Count; i++)
        {
            var it = columnNames[i];
            models[i] = new ColumnViewModel(it.Title, columnNames.Count - 1 == i)
            {
                ColumnIndex = i,
                CommandHolder = [..it.Commands]
            };
        }

        for (var i = 0; i < models.Length; i++)
        {
            Columns.Add(models[i]);
        }
    }

    public void Dispose()
    {
        _selectedItem.Dispose();
        _selectedItem = null;
        foreach (var model in Columns)
            model.Dispose();
        Columns.Clear();
        Columns = null;
        foreach (var model in Rows)
            model.Dispose();
        Rows.Clear();
        Rows = null;
    }
}
