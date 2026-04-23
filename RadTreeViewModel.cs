using System.Collections.ObjectModel;
using System.Windows;

namespace RadTreeView;

public class RadTreeViewModel : BaseViewModel, IDisposable
{
    public bool IsDisposable { get; set; } = false;
    private int _count;
    public bool IsInitColumn { get; private set; } = false;
    public bool IsInitRow { get; private set; } = false;

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
    public event Action<string> RenameItemErrorAction;

    public event Action<RowViewModelList> Swapped;

    public void UpdateCount()
    {
        var count = 0;
        foreach (var item in Rows)
        {
            count += UpdateCount(item);
        }

        Count = count;
    }

    public void Swap(RowViewModel drag, RowViewModel swapped, Dictionary<RowViewModel, Rect> selections)
    {
        var parent = drag.Parent;

        var indexDrag = parent.Children.IndexOf(drag);
        if (indexDrag == -1)
            throw new ArgumentOutOfRangeException(nameof(drag));

        var indexSwapped = parent.Children.IndexOf(swapped);
        if (indexSwapped == -1)
            throw new ArgumentOutOfRangeException(nameof(swapped));

        if (indexDrag == indexSwapped)
            return;

        var ordered = selections
            .OrderBy(x => x.Value.Top)
            .Select(x => x.Key)
            .ToList();

        var targetIndex = ordered.IndexOf(swapped);
        if (targetIndex == -1)
            return;

        var currentIndex = indexDrag;

        if (currentIndex < targetIndex)
        {
            var temp = parent.Children[currentIndex];

            for (var i = currentIndex; i < targetIndex; i++)
            {
                parent.Children[i] = parent.Children[i + 1];
            }

            parent.Children[targetIndex] = temp;
        }
        else
        {
            var temp = parent.Children[currentIndex];

            for (var i = currentIndex; i > targetIndex; i--)
            {
                parent.Children[i] = parent.Children[i - 1];
            }

            parent.Children[targetIndex] = temp;
        }

        Swapped?.Invoke(parent);
    }

    private int UpdateCount(RowViewModel row)
    {
        var count = 0;
        if (row is RowViewModelList list)
        {
            count += list.Children.Count;
            foreach (var child in list.Children)
            {
                count += UpdateCount(child);
            }
        }

        return count;
    }

    internal bool Rename(RowViewModel rowViewModel, string oldName, string newName)
    {
        if (IsExistsName(newName))
        {
            RenameItemErrorAction?.Invoke(newName);
            return false;
        }
        RenameItemAction?.Invoke(rowViewModel, newName);
        return true;
    }

    private bool IsExistsName(string name)
    {
        foreach (var row in Rows)
        {
            if (IsExistsName(row, name))
                return true;
        }
        return false;
    }

    private bool IsExistsName(RowViewModel row, string name)
    {
        if (row is RowViewModelList list)
        {
            foreach (var child in list.Children)
            {
                if(IsExistsName(child, name)) return true;
            }
        }
        else if (row is RowViewModelItem item)
        {
            if (item.Title == name) return true;
        }

        return false;
    }

    public RowViewModel SelectedItem
    {
        get => field;
        set
        {
            if (SetValue(ref field, value))
                ChangeSelectedItem?.Invoke(value);
        }
    }

    public void RaiseAddItem(RowViewModel item) => AddItem?.Invoke(item);

    public void AddRows(List<RowHolderList> holder)
    {
        if (IsInitRow) return;
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
        IsInitRow = true;
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
        if (IsInitColumn) return;
        Init(columnNames);
        IsInitColumn = true;
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
                CommandHolder = [..it.Commands],
                ColumnWidth = it.ColumnWidth == 0 ? 200 : it.ColumnWidth
            };
        }

        for (var i = 0; i < models.Length; i++)
        {
            Columns.Add(models[i]);
        }
    }

    public void Dispose()
    {
        SelectedItem?.Dispose();
        SelectedItem = null;
        foreach (var model in Columns)
            model.Dispose();
        Columns.Clear();
        foreach (var model in Rows)
            model.Dispose();
        Rows.Clear();
    }
}
