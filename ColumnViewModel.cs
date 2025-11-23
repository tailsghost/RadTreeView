using RadTreeView.Commands;
using RadTreeView.Interfaces;
using System.Windows;

namespace RadTreeView;

public class ColumnViewModel : BaseViewModel, ITree
{
    private Point _lastPoint;
    private int _columnIndex;
    private int _columnWidth = 200;
    private int _columnHeight = 25;

    public int ColumnIndex
    {
        get => _columnIndex;
        set => SetValue(ref _columnIndex, value);
    }

    public int ColumnWidth
    {
        get => _columnWidth;
        set => SetValue(ref _columnWidth, value);
    }

    public int ColumnHeight
    {
        get => _columnHeight;
        set => SetValue(ref _columnHeight, value);
    }

    public int MinColumnWidth { get; } = 150;

    public string Title { get; }

    public bool IsLast { get; }

    public bool IsMoveMode { get; set; }
    public Point StartPoint { get; set; }
    public Point LastPoint 
    { 
        get => _lastPoint; 
        set => SetValue(ref _lastPoint, value); 
    }

    public List<CommandBase> Commands { get; set; }

    public double Clamp(double value, double min, double max) =>
            Math.Max(min, Math.Min(max, value));

    public ColumnViewModel(string title, bool isLast)
    {
        Title = title;
        IsLast = isLast;
    }

    public void Dispose()
    {
        Commands.Clear();
    }
}
