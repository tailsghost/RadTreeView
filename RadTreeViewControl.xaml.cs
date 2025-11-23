using RadTreeView.Commands;
using RadTreeView.Interfaces;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RadTreeView;

public partial class RadTreeViewControl
{

    public RadTreeViewModel ViewModel { get; }

    private readonly Dictionary<ColumnViewModel, ContentPresenter> Columns = new();
    private readonly Dictionary<ColumnViewModel, ColumnDefinition> ColumnsDef = new();

    private readonly Dictionary<RowViewModel, Grid> Elements = new();
    private readonly Dictionary<RowViewModel, Border> VerticalLines = new();
    private readonly Dictionary<RowViewModel, Border> BorderButtons = new();
    private readonly Dictionary<RowViewModel, List<FrameworkElement>> OtherElements = new();
    private readonly Dictionary<RowViewModel, RowDefinition> RowDefs = new();


    private readonly Dictionary<ColumnViewModel, Popup> ColumnPopups = new();
    private readonly Dictionary<ColumnViewModel, double> PopupStartOffsets = new();
    private readonly Dictionary<ColumnViewModel, double> PopupStartVerticalOffsets = new();


    private void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);
    private void BeginInvoke(Action action) => Application.Current.Dispatcher.BeginInvoke(action);


    public event Action<RowViewModel> SelectedElement;
    public event Action<RowViewModel> ElementDoubleClick;

    public RadTreeViewControl(RadTreeViewModel model)
    {
        ViewModel = model;
        DataContext = this;
        InitializeComponent();
    }

    private void PART_RootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ResetSelected();
        e.Handled = true;
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
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    content.MouseRightButtonUp += Column_MouseRightButtonUp;

                    var columnDef = new ColumnDefinition()
                    {
                        Width = new GridLength(header.IsLast ? 1 : header.ColumnWidth, header.IsLast ? GridUnitType.Star : GridUnitType.Pixel)
                    };

                    PART_RootGrid.ColumnDefinitions.Add(columnDef);
                    Grid.SetColumn(content, PART_RootGrid.ColumnDefinitions.Count - 1);
                    Grid.SetRow(content, 0);
                    PART_RootGrid.Children.Add(content);
                    ColumnsDef[header] = columnDef;

                    if (!header.IsLast)
                    {
                        var splitter = new Border
                        {
                            BorderThickness = new Thickness(0, 0, 2, 0),
                            BorderBrush = Brushes.LightGray,
                            Cursor = Cursors.SizeWE,
                            Tag = header,
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };

                        splitter.MouseLeftButtonDown += Splitter_MouseLeftButtonDown;
                        splitter.MouseLeftButtonUp += Splitter_MouseLeftButtonUp;
                        splitter.MouseMove += Splitter_MouseMove;

                        Grid.SetColumn(splitter, PART_RootGrid.ColumnDefinitions.Count - 1);
                        Grid.SetRow(splitter, 0);
                        Grid.SetRowSpan(splitter, int.MaxValue);
                        PART_RootGrid.Children.Add(splitter);
                    }
                    Columns[header] = content;
                    header.PropertyChanged += HeaderColumn_PropertyChanged;
                    break;
                }

            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                break;
        }
    }

    private void HeaderColumn_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ColumnViewModel columnViewModel)
            return;

        if (e.PropertyName is nameof(ColumnViewModel.LastPoint))
        {
            var currentDef = ColumnsDef[columnViewModel];
            var currentColumn = Columns[columnViewModel];

            var lastColumnDef = ColumnsDef.Last();
            var lastColumn = Columns.Last();

            var delta = columnViewModel.LastPoint.X - columnViewModel.StartPoint.X;

            var indexOf = ColumnsDef
                .ToList()
                .IndexOf(new KeyValuePair<ColumnViewModel, ColumnDefinition>(columnViewModel, currentDef));

            var newValue = currentDef.Width.Value + delta;



            currentDef.Width = new GridLength(newValue > columnViewModel.MinColumnWidth ? newValue : columnViewModel.MinColumnWidth, GridUnitType.Pixel);

            columnViewModel.ColumnWidth = (int)currentDef.Width.Value;
        }
    }

    private void Splitter_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Border { Tag: ColumnViewModel column } border) return;
        if (!column.IsMoveMode) return;
        if (!ColumnPopups.TryGetValue(column, out var popup)) return;
        if (popup.Child is not FrameworkElement fe) return;

        var gridPos = Mouse.GetPosition(PART_RootGrid);

        var dx = gridPos.X - column.StartPoint.X;

        var startOffset = PopupStartOffsets.TryGetValue(column, out var so) ? so : popup.HorizontalOffset;
        var newOffset = startOffset + dx;

        var minX = 0.0;
        var maxX = Math.Max(0, PART_RootGrid.ActualWidth - fe.ActualWidth);
        newOffset = column.Clamp(newOffset, minX, maxX);

        popup.HorizontalOffset = newOffset;

        if (PopupStartVerticalOffsets.TryGetValue(column, out var vOff))
            popup.VerticalOffset = vOff;

        e.Handled = true;
    }

    private void Splitter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: ColumnViewModel column } border) return;
        column.IsMoveMode = true;
        border.CaptureMouse();
        column.StartPoint = e.GetPosition(PART_RootGrid);

        var visualCopy = new Border
        {
            Width = 1,
            Height = border.ActualHeight,
            CornerRadius = border is Border b ? b.CornerRadius : new CornerRadius(0),
            BorderThickness = new Thickness(1, 0, 0, 0),
            BorderBrush = border.BorderBrush,
            Background = new VisualBrush(border) { Stretch = Stretch.None },
            Opacity = 0.7,
            IsHitTestVisible = false
        };

        var popup = new Popup
        {
            Child = visualCopy,
            Placement = PlacementMode.Relative,
            PlacementTarget = PART_RootGrid,
            AllowsTransparency = true,
            StaysOpen = true,
            IsHitTestVisible = false,
            IsOpen = true
        };

        var initialHor = column.StartPoint.X - (visualCopy.Width / 2.0);
        var initialVert = column.StartPoint.Y - (visualCopy.Height / 2.0);

        initialHor = column.Clamp(initialHor, 0, Math.Max(0, PART_RootGrid.ActualWidth - visualCopy.Width));
        initialVert = column.Clamp(initialVert, 0, Math.Max(0, PART_RootGrid.ActualHeight - visualCopy.Height));

        popup.HorizontalOffset = initialHor;
        popup.VerticalOffset = initialVert;

        ColumnPopups[column] = popup;
        PopupStartOffsets[column] = popup.HorizontalOffset;
        PopupStartVerticalOffsets[column] = popup.VerticalOffset;

        e.Handled = true;
    }

    private void Splitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: ColumnViewModel column } border) return;

        column.IsMoveMode = false;
        border.ReleaseMouseCapture();

        if (ColumnPopups.TryGetValue(column, out var popup))
        {
            column.LastPoint = new Point(popup.HorizontalOffset, popup.VerticalOffset);

            popup.IsOpen = false;
            popup.Child = null;
            ColumnPopups.Remove(column);
        }

        PopupStartOffsets.Remove(column);
        PopupStartVerticalOffsets.Remove(column);
        e.Handled = true;
    }

    private void Column_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ITree vm } content) return;
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


                    if (row is RowViewModelList rowViewModel)
                    {
                        rowViewModel.Children.CollectionChanged += Rows_CollectionChanged;
                    }

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
                    ViewModel.Count++;
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems[0] is not RowViewModel row) return;
                    RemoveChild(row);
                    Task.Run(() => RebuildVisibleRows());
                    break;
                }
        }
    }

    private void RemoveChild(RowViewModel row)
    {
        ViewModel.Count--;
        if (RowDefs.TryGetValue(row, out var rowDef))
        {
            PART_RootGrid.RowDefinitions.Remove(rowDef);
            RowDefs.Remove(row);
        }
        if (Elements.TryGetValue(row, out var grid))
        {
            for (var i = 0; i < grid.Children.Count; i++)
            {
                if (grid.Children[i] is Border { Name: "PART_newBorder" } border)
                {
                    border.MouseRightButtonUp -= Column_MouseRightButtonUp;
                }
            }
            PART_RootGrid.Children.Remove(grid);
            Elements.Remove(row);
        }
        if (OtherElements.TryGetValue(row, out var listother))
        {
            foreach (var el in listother)
            {
                PART_RootGrid.Children.Remove(el);
                if(el is Border border)
                {
                    border.MouseLeftButtonUp -= NewBorder_MouseLeftButtonUp;
                }
            }
            OtherElements.Remove(row);
        }
        if (VerticalLines.TryGetValue(row, out var line))
        {
            PART_RootGrid.Children.Remove(line);
            VerticalLines.Remove(row);
        }
        if (BorderButtons.TryGetValue(row, out var button))
        {
            button.MouseLeftButtonUp -= BorderButton_MouseLeftButtonUp;
            BorderButtons.Remove(row);
        }
        if (row is RowViewModelList list)
        {
            list.Children.CollectionChanged -= Rows_CollectionChanged;
            foreach (var child in list.Children)
            {
                RemoveChild(child);
            }
        }

        if (row.Parent is RowViewModelList parentList)
        {
            if (parentList.Children.Count == 0)
            {
                if (BorderButtons.TryGetValue(parentList, out var buttons))
                {
                    buttons.Visibility = Visibility.Collapsed;
                }
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

        if (row.Parent != null)
        {
            if (BorderButtons.TryGetValue(row.Parent, out var parentBorder))
            {
                if (row.Parent is RowViewModelList parentRowList)
                {
                    if (parentRowList.Children.Count == 0)
                    {
                        parentBorder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        parentBorder.Visibility = Visibility.Visible;
                    }
                }
            }
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
                BorderThickness = new Thickness(0, 0.5, 0, 0.5),
                BorderBrush = Brushes.LightGray,
                Child = value,
                Margin = new Thickness(i == 0 ? row.RowOffset : 0, 0, 0, 0),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                Tag = row
            };

            newBorder.MouseLeftButtonUp += NewBorder_MouseLeftButtonUp;

            OtherElements[row].Add(newBorder);

            PART_RootGrid.Children.Add(newBorder);
            Grid.SetColumn(newBorder, i);
            if (index == -1)
            {
                throw new IndexOutOfRangeException("Не найден индекс элемента!");
            }

            Grid.SetRow(newBorder, index + 1);
        }

        row.UpdateRowsPosition = true;
    }

    private void ResetSelected()
    {
        foreach(var element in Elements)
        {
            var grid = element.Value;
            for (var i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child is not Border { Name: "PART_newBorder" } borderGrid) continue;
                borderGrid.BorderBrush = Brushes.LightGray;
            }
        }

        foreach (var item in OtherElements)
        {
            foreach (var element in item.Value)
            {
                if (element is Border borderOther)
                {
                    borderOther.BorderBrush = Brushes.LightGray;
                }
            }
        }
    }

    private void NewBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: RowViewModel model } border) return;
        ResetSelected();
        if (Elements.TryGetValue(model, out var grid))
        {
            for(var i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child is not Border { Name: "PART_newBorder" } borderGrid) continue;
                borderGrid.BorderBrush = Brushes.Blue;
            }
        }
        if (OtherElements.TryGetValue(model, out var list))
        {
            foreach (var element in list)
            {
                if (element is Border borderOther)
                {
                    borderOther.BorderBrush = Brushes.Blue;
                }
            }
        }

        e.Handled = true;
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
            IsHitTestVisible = true,
            Background = Brushes.Transparent
        };

        currentGrid.RowDefinitions.Add(new RowDefinition()
        {
            Height = new GridLength(row.RowHeight, GridUnitType.Pixel)
        });

        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Pixel) });
        currentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        var rowOffset = RowViewModel.RowOffsetImmutable / 5;

        RowViewModelList rowIsList = null;

        if (row is RowViewModelList rowViewModelList)
        {
            var textButton = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = rowViewModelList.IsOpenChildren ? "➖" : "➕",
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
                Cursor = Cursors.Hand,
                Tag = row
            };

            borderButton.MouseLeftButtonUp += BorderButton_MouseLeftButtonUp;

            Panel.SetZIndex(borderButton, 1000);

            BorderButtons[row] = borderButton;

            if (rowViewModelList.Children.Count == 0)
            {
                borderButton.Visibility = Visibility.Collapsed;
            }
            rowIsList = rowViewModelList;
        }

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

        if (rowIsList != null)
        {
            currentGrid.Children.Add(BorderButtons[rowIsList]);
            Grid.SetColumn(BorderButtons[rowIsList], 0);
        }
        currentGrid.Children.Add(lineBorder);
        Grid.SetColumn(lineBorder, 0);

        Elements[row] = currentGrid;

        if (row.Parent != null)
        {
            PART_RootGrid.Children.Add(lineBorderDown);

            Grid.SetRow(lineBorderDown, index + 1);
            Grid.SetColumn(lineBorderDown, 0);

            VerticalLines[row] = lineBorderDown;

            if (rowIsList != null && rowIsList.Parent is RowViewModelList parent && parent.IsOpenChildren)
            {
                lineBorderDown.Visibility = Visibility.Collapsed;
            }

            if (row.DepthChildren > 0)
            {
                Task.Run(() => ChangeHeightLine(row));
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
            Cursor = Cursors.Hand,
            Tag = row,
            Name = "PART_newBorder"
        };
        newBorder.MouseRightButtonUp += Column_MouseRightButtonUp;
        currentGrid.Children.Add(newBorder);

        newBorder.MouseLeftButtonDown += Border_MouseLeftButtonDown;

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
        if (row.UpdateRowsPosition)
        {
            Task.Run(RebuildVisibleRows);
        }


        UpdateBorderThickness(row);

        var rows = row.GetTopRows();
        foreach (var it in rows)
        {
            UpdateBorderThickness(it);
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: RowViewModel row }) return;
        if (row is RowViewModel)
        {
            if (e.ClickCount == 2)
            {
                ElementDoubleClick?.Invoke(row);
                row.SelectedRow();
            }
            else
            {
                SelectedElement?.Invoke(row);
                row.SelectedRow();
                NewBorder_MouseLeftButtonUp(sender, e);
            }

            if (row is RowViewModelList rowList)
            {
                if (rowList.Children.Count > 0)
                {
                    rowList.IsOpenChildren = !rowList.IsOpenChildren;
                }
            }
        }
    }

    private void BorderButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: RowViewModelList rowList }) return;
        rowList.IsOpenChildren = !rowList.IsOpenChildren;
    }

    private List<RowViewModel> GetVisibleRows()
    {
        var result = new List<RowViewModel>();

        void AddVisible(RowViewModel node)
        {
            result.Add(node);
            if (node is RowViewModelList list)
            {
                if (list.IsOpenChildren)
                {
                    foreach (var c in list.Children)
                        AddVisible(c);
                }
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
        var newRowDefs = new Dictionary<RowViewModel, RowDefinition>();
        Invoke(() =>
        {
            PART_RootGrid.RowDefinitions.Clear();
            if (headerDef != null)
                PART_RootGrid.RowDefinitions.Add(headerDef);
            foreach (var r in visibleRows)
            {
                var rd = new RowDefinition { Height = new GridLength(r.RowHeight, GridUnitType.Pixel) };
                PART_RootGrid.RowDefinitions.Add(rd);
                newRowDefs[r] = rd;
            }

            PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        });

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
                Invoke(() =>
                {
                    grid.Visibility = Visibility.Collapsed;
                    PART_RootGrid.Children.Remove(grid);
                });
            }
        }
        foreach (var kv in OtherElements.ToList())
        {
            var row = kv.Key;
            foreach (var el in kv.Value)
            {
                Invoke(() =>
                {
                    el.Visibility = Visibility.Collapsed;
                    PART_RootGrid.Children.Remove(el);
                });
            }
        }
        foreach (var kv in VerticalLines.ToList())
        {
            var row = kv.Key;
            var el = kv.Value;
            Invoke(() =>
            {
                el.Visibility = Visibility.Collapsed;
                PART_RootGrid.Children.Remove(el);
            });
        }

        foreach (var kv in VerticalLines.ToList())
        {
            UpdateHeightLine(kv.Key);
        }

        foreach (var row in visibleRows)
        {

            if (OtherElements.TryGetValue(row, out var list))
            {
                foreach (var it in list)
                {
                    Invoke(() =>
                    {
                        if (!PART_RootGrid.Children.Contains(it))
                            PART_RootGrid.Children.Add(it);
                        it.Visibility = Visibility.Visible;
                        Grid.SetRow(it, rowIndex);
                    });
                }
            }

            if (VerticalLines.TryGetValue(row, out var line))
            {
                Invoke(() =>
                {
                    if (!PART_RootGrid.Children.Contains(line))
                        PART_RootGrid.Children.Add(line);
                    Grid.SetRow(line, rowIndex);
                    line.Visibility = Visibility.Visible;
                    Panel.SetZIndex(line, 0);
                });
            }

            if (BorderButtons.TryGetValue(row, out var border))
            {
                if (border.Child is TextBlock tb)
                {
                    if (row is RowViewModelList rowlist)
                    {
                        Invoke(() =>
                        {
                            tb.Text = rowlist.IsOpenChildren ? "➖" : "➕";
                        });
                    }
                }
            }

            if (Elements.TryGetValue(row, out var grid))
            {
                Invoke(() =>
                {
                    if (!PART_RootGrid.Children.Contains(grid))
                        PART_RootGrid.Children.Add(grid);
                    grid.Visibility = Visibility.Visible;
                    Grid.SetRow(grid, rowIndex);
                    Panel.SetZIndex(grid, 1000);
                });
            }

            UpdateBorderThickness(row);

            rowIndex++;
        }
    }

    private void Row_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not RowViewModel item) return;
        if (e.PropertyName is nameof(RowViewModelList.IsOpenChildren))
        {
            if (item is RowViewModelList rowlist)
            {
                if (BorderButtons.TryGetValue(item, out var border))
                {
                    if (border.Child is TextBlock borderBlock)
                        borderBlock.Text = rowlist.IsOpenChildren ? "➖" : "➕";
                }
            }

            if (item.UpdateRowsPosition)
                Task.Run(() => RebuildVisibleRows());
        }
    }


    private void UpdateBorderThickness(RowViewModel item)
    {
        const double left = 1.0;
        const double right = 0.0;
        var thickness = new Thickness(0, 0, 0, 0);
        if (item.Parent is RowViewModelList rowList)
        {
            var isFirst = ReferenceEquals(rowList.Children[0], item);
            var isLast = ReferenceEquals(rowList.Children[^1], item);
            var isLastSection = rowList.TopParent.RowListEqualsLast();
            var isLastItemSection = isLastSection && ReferenceEquals(rowList.TopParent.Children.Last(), item);

            var top = isFirst ? 0.0 : 1.0;
            double bottom;

            if (item is RowViewModelList it)
            {
                if(it.Children.Count > 0)
                    bottom = it.IsOpenChildren ? 1.0 : 0.0;
                else
                {
                    bottom = (isLastSection && isLast && rowList.IsOpenChildren && isLastItemSection) ? 1.0 : 0.0;
                }
            }
            else
            {
                bottom = (isLastSection && isLast && rowList.IsOpenChildren && isLastItemSection) ? 1.0 : 0.0;
            }

            thickness = new Thickness(left, top, right, bottom);
        }
        else
        {
            if (item is RowViewModelList rl)
            {
                var isFirst = rl.IsFirstTopRow();
                var isLast = rl.RowListEqualsLast();
                var top = isFirst ? 0.0 : 1.0;
                double bottom = 0.0;
                if (rl.Children.Count > 0)
                {
                    if (rl.Children.Count > 0)
                        if (rl.IsOpenChildren)
                        {
                            bottom = 1.0;
                        }
                        else
                        {
                            if (isLast)
                            {
                                bottom = 1.0;
                            }
                            else
                            {
                                bottom = 0.0;
                            }
                        }
                    else
                        {
                            bottom = 1.0;
                        }
                }
                else
                {
                    if (isLast && isFirst)
                    {
                        if (rl.IsOpenChildren)
                        {
                            bottom = rl.IsOpenChildren ? 1.0 : 0.0;
                        }
                        else
                        {
                            bottom = 1.0;
                        }
                    }
                    else
                    {
                        bottom = (isLast && !rl.IsOpenChildren) ? 1.0 : 0.0;
                    }
                }
                thickness = new Thickness(left, top, right, bottom);
            }
            else
            {
                thickness = new Thickness(left, 0.0, right, 0.0);
            }
        }

        if (Elements.TryGetValue(item, out var grid))
        {
            Invoke(() =>
            {
                foreach (var child in grid.Children)
                {
                    if (child is Border border && border.Name == "PART_newBorder")
                    {
                        border.BorderThickness = thickness;
                    }
                }
            });
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
        if (item is RowViewModelList rowlist)
        {
            if (rowlist.IsOpenChildren)
            {
                foreach (var c in rowlist.Children)
                    count += CountVisibleRows(c);
            }
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
        RowViewModelList parent = null;
        if (item.Parent is RowViewModelList list && list.Children.Count > 1)
        {
            if (list.Children.First() == item)
            {
                var indexOf = list.Children.IndexOf(item);
                if (indexOf != -1)
                {
                    rows = CountVisibleRows(item) + list.Children.Count - 1 - indexOf;
                    if (rows <= 0) rows = 1;
                }
            }
            else
            {
                if (list.Children.Last() != item)
                {
                    var indexOf = list.Children.IndexOf(item);
                    if (indexOf != -1)
                    {
                        rows = CountVisibleRows(list.Children.ElementAt(indexOf)) + list.Children.Count - 1 - indexOf;
                        if (rows <= 0) rows = 1;
                    }
                }
                else
                    rows = 1;
            }
            parent = list;
        }
        else
        {
            rows = 1;
        }

        Invoke(() =>
        {
            Grid.SetRow(border, index + 1);
            Grid.SetRowSpan(border, rows);
            border.Height = Math.Max(1, rows * 25 - RowViewModel.RowOffsetImmutable / 2);
            if (parent != null && !parent.IsOpenChildren)
                border.Visibility = Visibility.Collapsed;
            else
                border.Visibility = Visibility.Visible;
        });
    }

}
