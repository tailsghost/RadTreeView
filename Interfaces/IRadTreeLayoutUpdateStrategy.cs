namespace RadTreeView.Interfaces;

public interface IRadTreeLayoutUpdateStrategy
{
    bool AddColumn(RowViewModel item);
    bool AddRow(RowViewModel item);
}
