using RadTreeView.Commands;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace RadTreeView;

public class RadTreeViewModel : BaseViewModel
{
    public ObservableCollection<RowViewModelList> Rows = [];
    public ObservableCollection<ColumnViewModel> Columns;

    public int ColumnCount
    {
        get => Columns.Count;
    }
    public int RowsCount
    {
        get => Rows.Count;
    }


    public RowViewModelList AddRow(IEnumerable<Content> contents)
    {
        var row = new RowViewModelList(Columns.Count, Rows, contents)
        {
            Image = new BitmapImage(
            new Uri("pack://application:,,,/RadTreeViewTest;component/Assets/Project_Property_Icon.png")),
        };
        Rows.Add(row);
        row.TopParent = row;
        OnPropertyChanged(nameof(RowsCount));

        return row;
    }

    public void AddColumn(List<ColumnHolder> columnNames)
    {
        if(Columns.Count!= 0)
        {
            throw new InvalidOperationException("Columns уже инициализирован!");
        }

        Init(columnNames);
    }

    public bool IsInit => Columns.Count != 0;

    public RadTreeViewModel(List<ColumnHolder> columnNames)
    {
        Columns = new();
        if (columnNames.Count ==0) return;
        Init(columnNames);
    }

    private void Init(List<ColumnHolder> columnNames)
    {
        ColumnViewModel[] models = new ColumnViewModel[columnNames.Count];

        for (var i = 0; i < columnNames.Count; i++)
        {
            var it = columnNames[i];
            models[i] = new ColumnViewModel(it.Title, columnNames.Count - 1 == i)
            {
                Commands = it.Commands
            };
        }

        for (var i = 0; i < models.Length; i++)
        {
            Columns.Add(models[i]);
        }
    }
}
