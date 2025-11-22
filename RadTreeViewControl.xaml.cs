using RadTreeView.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RadTreeView;

public partial class RadTreeViewControl
{

    private int _index = 0;

    public RadTreeViewModel ViewModel { get; }

    private readonly Dictionary<RowViewModel, Grid> Elements = new();
    private readonly Dictionary<RowViewModel, Border> VerticalLines = new();
    private readonly Dictionary<RowViewModel, Border> BorderButtons = new();
    private readonly Dictionary<RowViewModel, List<FrameworkElement>> OtherElements = new();
    private readonly Dictionary<RowViewModel, RowDefinition> RowDefs = new();

    public RadTreeViewControl(RadTreeViewModel model)
    {
        ViewModel = model;
        DataContext = this;
        InitializeComponent();
    }

    public RadTreeViewControl()
    {
        ViewModel = new RadTreeViewModel([]);
        DataContext = this;
        InitializeComponent();

        PART_RootGrid.RowDefinitions.Clear();

        ViewModel.Columns.CollectionChanged += Columns_CollectionChanged;
        ViewModel.Rows.CollectionChanged += Rows_CollectionChanged;
    }

    private void PART_RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsInit) return;
        var mainGrid = PART_RootGrid;
        mainGrid.ColumnDefinitions.Clear();
        mainGrid.RowDefinitions.Clear();

        foreach (var header in ViewModel.Columns)
        {
            mainGrid.Children.Add(new ContentPresenter()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = header,
                Tag = header
            });
        }
    }

    private void Columns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems[0] is not ColumnViewModel header) return;
                    if (PART_RootGrid.RowDefinitions.Count == 0)
                    {
                        PART_RootGrid.RowDefinitions.Add(new RowDefinition()
                        {
                            Height = new GridLength(header.ColumnHeight, GridUnitType.Pixel)
                        });

                        PART_RootGrid.RowDefinitions.Add(new RowDefinition()
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        });
                    }
                    var content = new ContentPresenter()
                    {
                        Content = header,
                        Tag = header,
                        Height = header.ColumnHeight,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    content.MouseRightButtonUp += Column_MouseRightButtonUp;

                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(header.IsLast ? 1 : header.ColumnWidth, header.IsLast ? GridUnitType.Star : GridUnitType.Pixel)
                    });
                    Grid.SetColumn(content, PART_RootGrid.ColumnDefinitions.Count - 1);
                    Grid.SetRow(content, 0);
                    PART_RootGrid.Children.Add(content);

                    if (!header.IsLast)
                    {
                        var splitter = new Border
                        {
                            BorderThickness = new Thickness(0, 0, 2, 0),
                            BorderBrush = Brushes.LightGray,
                            Cursor = Cursors.SizeWE,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        };

                        Grid.SetColumn(splitter, PART_RootGrid.ColumnDefinitions.Count - 1);
                        Grid.SetRow(splitter, 0);
                        Grid.SetRowSpan(splitter, int.MaxValue);
                        PART_RootGrid.Children.Add(splitter);
                    }

                    break;
                }

            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                break;
        }
    }

    private void Column_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ContentPresenter { Tag: ColumnViewModel vm } content) return;
        OpenContextMenu(vm.Commands, content);
    }


    private void OpenContextMenu(IEnumerable<CommandBase> commands, FrameworkElement element)
    {
        if (!commands.Any()) return;
        ContextMenu menu = null;
        if (element.ContextMenu == null)
            menu = new ContextMenu();
        else
        {
            menu = element.ContextMenu;
            menu.Items.Clear();
        }

        foreach (var command in commands)
        {
            menu.Items.Add(new MenuItem()
            {
                Header = command.CommandName,
                Command = command.Command,
                CommandParameter = command.CommandParameter,
                Cursor = Cursors.Hand
            });
        }

        element.ContextMenu = menu;
    }

    private void Rows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems[0] is not RowViewModel row) return;


                    row.Children.CollectionChanged += Rows_CollectionChanged;

                    PART_RootGrid.RowDefinitions.Last().Height = new GridLength(row.RowHeight, GridUnitType.Pixel);

                    var content = new ContentControl()
                    {
                        Content = row,
                        Margin = new Thickness(5, 0, 0, 0)
                    };

                    Grid.SetRow(content, PART_RootGrid.RowDefinitions.Count - 1);
                    PART_RootGrid.Children.Add(content);

                    var rowDef = new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    };

                    PART_RootGrid.RowDefinitions.Add(rowDef);

                    RowDefs[row] = rowDef;

                    break;
                }
        }
    }

    private void Border_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Border { DataContext: RowViewModel row } border) return;
        var index = row.GetIndexRowItem();

        if (row.RowContents.Count > 0)
        {
            AddGrid(row, index);
        }

        if (row.RowContents.Count > 0)
        {
            OtherElements[row] = new List<FrameworkElement>(row.RowContents.Count);
        }

        for (var i = 1; i < row.RowContents.Count; i++)
        {
            var content = row.RowContents[i];
            var value = content.Value;

            var newBorder = new Border
            {
                BorderThickness = new Thickness(0, 1, 0, 1),
                BorderBrush = Brushes.LightGray,
                Child = value,
                Margin = new Thickness(i == 0 ? row.RowOffset : 0, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            OtherElements[row].Add(newBorder);

            PART_RootGrid.Children.Add(newBorder);
            Grid.SetColumn(newBorder, i);
            if (index == -1)
            {
                throw new IndexOutOfRangeException("Не найден индекс элемента!");
            }

            Grid.SetRow(newBorder, index + 1);
        }
    }

    private void AddGrid(RowViewModel row, int index, Grid? parentGrid = null)
    {

        if (index == -1) throw new IndexOutOfRangeException("Не найден индекс элемента!");
        var content = row.MainContent;
        var value = content.Value;

        var currentGrid = new Grid()
        {
            Margin = new Thickness(parentGrid == null ? 10 : -RowViewModel.RowOffsetImmutable / 2, 0, parentGrid == null ? 0 : 0, 0),
            Tag = row,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SnapsToDevicePixels = true
        };

        currentGrid.RowDefinitions.Add(new RowDefinition()
        {
            Height = new GridLength(row.RowHeight, GridUnitType.Pixel)
        });

        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Pixel) });
        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        var rowOffset = RowViewModel.RowOffsetImmutable / 5;

        var textButton = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = row.IsOpenChildren ? "➖" : "➕",
            FontSize = 5,
            Foreground = Brushes.Black
        };

        var borderButton = new Border
        {
            Width = 10,
            Height = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Child = textButton,
            Margin = new Thickness(row.RowOffset - RowViewModel.RowOffsetImmutable / 3, 0, -row.RowOffset + RowViewModel.RowOffsetImmutable / 3, 0),
            Background = Brushes.Azure,
            Cursor = Cursors.Hand
        };
        Panel.SetZIndex(borderButton, 1000);

        BorderButtons[row] = borderButton;

        var lineBorder = new Border
        {
            Margin = new Thickness(row.RowOffset - RowViewModel.RowOffsetImmutable / 6, 0, -row.RowOffset + RowViewModel.RowOffsetImmutable / 6, 0),
            Width = 20 + rowOffset,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0.5, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            IsHitTestVisible = false
        };

        var lineBorderDown = new Border
        {
            Margin = new Thickness(row.RowOffset + RowViewModel.RowOffsetImmutable / 4, 0, -row.RowOffset - RowViewModel.RowOffsetImmutable / 4, 0),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1, 0, 0, 0),
            Tag = row,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false
        };

        if (row.Children.Count == 0)
        {
            borderButton.Visibility = Visibility.Collapsed;
        }

        currentGrid.Children.Add(borderButton);
        currentGrid.Children.Add(lineBorder);
        Grid.SetColumn(lineBorder, 0);
        Grid.SetColumn(borderButton, 0);

        Elements[row] = currentGrid;

        if (row.Parent != null)
        {
            PART_RootGrid.Children.Add(lineBorderDown);

            Grid.SetRow(lineBorderDown, index + 1);
            Grid.SetColumn(lineBorderDown, 0);

            VerticalLines[row] = lineBorderDown;

            if (!row.Parent.IsOpenChildren)
            {
                lineBorderDown.Visibility = Visibility.Collapsed;
            }

            if (row.DepthChildren > 0)
            {
                ChangeHeightLine(row);
            }
        }
        else
        {
            lineBorderDown.Visibility = Visibility.Collapsed;
        }

        var newBorder = new Border
        {
            BorderThickness = new Thickness(1, 1, 0, 1),
            BorderBrush = Brushes.LightGray,
            Child = value,
            Margin = new Thickness(row.RowOffset, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        currentGrid.Children.Add(newBorder);

        if (row.Image != null)
        {
            Grid.SetColumn(newBorder, 2);
            var image = new Image()
            {
                Source = row.Image,
                Width = 15,
                Height = 15,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(row.RowOffset - rowOffset, 0, -row.RowOffset + rowOffset, 0)
            };
            currentGrid.Children.Add(image);
            Grid.SetColumn(image, 1);
        }
        else
        {
            Grid.SetColumn(newBorder, 2);
        }

        PART_RootGrid.Children.Add(currentGrid);
        Grid.SetColumn(currentGrid, 0);
        Grid.SetRow(currentGrid, index + 1);

        row.PropertyChanged += Row_PropertyChanged;
    }

    private List<RowViewModel> GetVisibleRows()
    {
        var result = new List<RowViewModel>();

        void AddVisible(RowViewModel node)
        {
            result.Add(node);
            if (node.IsOpenChildren)
            {
                foreach (var c in node.Children)
                    AddVisible(c);
            }
        }

        foreach (var top in ViewModel.Rows)
            AddVisible(top);

        return result;
    }

    private void RebuildVisibleRows()
    {
        RowDefinition headerDef = null;
        if (PART_RootGrid.RowDefinitions.Count > 0)
            headerDef = PART_RootGrid.RowDefinitions[0];

        var visibleRows = GetVisibleRows();

        PART_RootGrid.RowDefinitions.Clear();
        if (headerDef != null)
            PART_RootGrid.RowDefinitions.Add(headerDef);

        var newRowDefs = new Dictionary<RowViewModel, RowDefinition>();
        foreach (var r in visibleRows)
        {
            var rd = new RowDefinition { Height = new GridLength(r.RowHeight, GridUnitType.Pixel) };
            PART_RootGrid.RowDefinitions.Add(rd);
            newRowDefs[r] = rd;
        }

        PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        foreach (var kv in newRowDefs)
        {
            RowDefs[kv.Key] = kv.Value;
        }

        int rowIndex = 1;
        var visibleSet = new HashSet<RowViewModel>(visibleRows);

        foreach (var kv in Elements.ToList())
        {
            var row = kv.Key;
            var grid = kv.Value;
            if (!visibleSet.Contains(row))
            {
                grid.Visibility = Visibility.Collapsed;
                PART_RootGrid.Children.Remove(grid);
            }
        }
        foreach (var kv in OtherElements.ToList())
        {
            var row = kv.Key;
            foreach (var el in kv.Value)
            {
                el.Visibility = Visibility.Collapsed;
                PART_RootGrid.Children.Remove(el);
            }
        }
        foreach (var kv in VerticalLines.ToList())
        {
            var row = kv.Key;
            var el = kv.Value;
            el.Visibility = Visibility.Collapsed;
            PART_RootGrid.Children.Remove(el);
        }

        foreach (var kv in VerticalLines)
        {
            UpdateHeightLine(kv.Key);
        }

        foreach (var row in visibleRows)
        {

            if (OtherElements.TryGetValue(row, out var list))
            {
                foreach (var it in list)
                {
                    if (!PART_RootGrid.Children.Contains(it))
                        PART_RootGrid.Children.Add(it);
                    it.Visibility = Visibility.Visible;
                    Grid.SetRow(it, rowIndex);
                }
            }

            if (VerticalLines.TryGetValue(row, out var line))
            {
                if (!PART_RootGrid.Children.Contains(line))
                    PART_RootGrid.Children.Add(line);
                Grid.SetRow(line, rowIndex);
                line.Visibility = Visibility.Visible;
                Panel.SetZIndex(line, 0);
            }

            if (BorderButtons.TryGetValue(row, out var border))
            {
                if (border.Child is TextBlock tb)
                    tb.Text = row.IsOpenChildren ? "➖" : "➕";
            }

            if (Elements.TryGetValue(row, out var grid))
            {
                if (!PART_RootGrid.Children.Contains(grid))
                    PART_RootGrid.Children.Add(grid);
                grid.Visibility = Visibility.Visible;
                Grid.SetRow(grid, rowIndex);
                Panel.SetZIndex(grid, 1000);
            }

            rowIndex++;
        }
    }

    private void BringToFront(UIElement element)
    {
        Panel.SetZIndex(element, 1000);

        if (PART_RootGrid.Children.Contains(element))
        {
            PART_RootGrid.Children.Remove(element);
            PART_RootGrid.Children.Add(element);
        }
    }

    private void Row_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not RowViewModel item) return;
        if (e.PropertyName == nameof(RowViewModel.IsOpenChildren))
        {
            if (BorderButtons.TryGetValue(item, out var border))
            {
                if (border.Child is TextBlock borderBlock)
                    borderBlock.Text = item.IsOpenChildren ? "➖" : "➕";
            }

            RebuildVisibleRows();
        }
    }

    private void ChangeHeightLine(RowViewModel item)
    {
        var current = item;

        while (current != null)
        {
            UpdateHeightLine(current);
            current = current.Parent;
        }
    }

    private int CountVisibleRows(RowViewModel item)
    {
        int count = 1;

        if (item.IsOpenChildren)
        {
            foreach (var c in item.Children)
                count += CountVisibleRows(c);
        }

        return count;
    }

    private void UpdateHeightLine(RowViewModel item)
    {
        if (!VerticalLines.TryGetValue(item, out var border))
            return;

        var index = item.GetIndexRowItem();
        if (index == -1) return;

        var rows = 0;

        if (item.Parent.Children.Count > 1)
        {
            if (item.Parent.Children.First() == item)
            {
                var indexOf = item.Parent.Children.IndexOf(item);
                if (indexOf != -1)
                {
                    rows = CountVisibleRows(item) + item.Parent.Children.Count - 1 - indexOf;
                    if (rows <= 0) rows = 1;
                }
            }
            else
            {
                rows = 1;
            }
        }
        else
        {
            rows = 1;
        }

        Grid.SetRow(border, index + 1);

        Grid.SetRowSpan(border, rows);

        border.Height = Math.Max(1, rows * 25 - RowViewModel.RowOffsetImmutable / 2);
        if (item.Parent != null && !item.Parent.IsOpenChildren)
            border.Visibility = Visibility.Collapsed;
        else
            border.Visibility = Visibility.Visible;
    }

}
