using System.Collections.ObjectModel;

namespace RadTreeView;

public class RadTreeViewModel : BaseViewModel
{
    public ObservableCollection<RowViewModel> Rows = [];
    public ObservableCollection<ColumnViewModel> Columns;

    public int ColumnCount
    {
        get => Columns.Count;
    }
    public int RowsCount
    {
        get => Rows.Count;
    }


    public void AddRow(IEnumerable<Content> contents)
    {
        Rows.Add(new RowViewModel(Columns.Count, Rows, contents));
        OnPropertyChanged(nameof(RowsCount));
    }

    public void AddColumn(List<string> columnNames)
    {
        if(Columns.Count!= 0)
        {
            throw new InvalidOperationException("Columns уже инициализирован!");
        }

        Init(columnNames);
    }

    public bool IsInit => Columns.Count != 0;

    public RadTreeViewModel(List<string> columnNames)
    {
        Columns = new();
        if (columnNames.Count ==0) return;
        Init(columnNames);
    }


    private void Init(List<string> columnNames)
    {
        ColumnViewModel[] models = new ColumnViewModel[columnNames.Count];

        for (var i = 0; i < columnNames.Count; i++)
        {
            var it = columnNames[i];
            models[i] = new ColumnViewModel(it, columnNames.Count - 1 == i);
        }

        for (var i = 0; i < models.Length; i++)
        {
            Columns.Add(models[i]);
        }
    }
}
