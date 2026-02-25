using RadTreeView.Commands;
using RadTreeView.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;

namespace RadTreeView;

public partial class RadTreeViewControl
{
    public RadTreeViewModel ViewModel { get; private set; }

    private readonly Dictionary<ColumnViewModel, ContentPresenter> Columns = [];
    private readonly Dictionary<RowViewModel, List<ContentPresenter>> Rows = [];
    private readonly Dictionary<ColumnViewModel, ColumnDefinition> ColumnsDef = [];

    private readonly Dictionary<RowViewModel, Grid> Elements = [];
    private readonly List<RowViewModel> ElementsIndex = [];
    private readonly Dictionary<RowViewModel, Border> VerticalLines = [];
    private readonly Dictionary<RowViewModel, Border> BorderButtons = [];
    private readonly Dictionary<RowViewModel, List<FrameworkElement>> OtherElements = [];
    private readonly Dictionary<RowViewModel, RowDefinition> RowDefs = [];


    private readonly Dictionary<ColumnViewModel, Popup> ColumnPopups = [];
    private readonly Dictionary<ColumnViewModel, double> PopupStartOffsets = [];
    private readonly Dictionary<ColumnViewModel, double> PopupStartVerticalOffsets = [];


    private void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);
    private void BeginInvoke(Action action) => Application.Current.Dispatcher.BeginInvoke(action);


    public event Action<RowViewModel> SelectedElement;
    public event Action<RowViewModel> ElementDoubleClick;
    public event Func<object, RowViewModel, MouseButtonEventArgs, bool> MouseLeftItemDown;
    public event Func<object, RowViewModel, MouseButtonEventArgs, bool> MouseLeftItemUp;
    public event Func<object, RowViewModel, MouseEventArgs, bool> MouseItemMove;


    public static readonly DependencyProperty LayoutItemTemplateSelectorProperty = DependencyProperty.Register(nameof(LayoutItemTemplateSelector), typeof(DataTemplateSelector), typeof(RadTreeViewControl),
    new FrameworkPropertyMetadata(null));


    public static readonly DependencyProperty ColumnsCollectionProperty =
    DependencyProperty.Register(
        nameof(ColumnsCollection),
        typeof(Collection<ColumnHolder>),
        typeof(RibbonControl),
        new FrameworkPropertyMetadata(null));

    public Collection<ColumnHolder> ColumnsCollection
    {
        get
        {
            var col = (Collection<ColumnHolder>?)GetValue(ColumnsCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(ColumnsCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(ColumnsCollectionProperty, value);
    }

    public static readonly DependencyProperty RowsCollectionProperty =
    DependencyProperty.Register(
        nameof(RowsCollection),
        typeof(Collection<RowHolderList>),
        typeof(RibbonControl),
        new FrameworkPropertyMetadata(null));

    public Collection<RowHolderList> RowsCollection
    {
        get
        {
            var col = (Collection<RowHolderList>?)GetValue(RowsCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(RowsCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(RowsCollectionProperty, value);
    }

    public static readonly DependencyProperty RowListCommandCollectionProperty =
        DependencyProperty.Register(
        nameof(RowListCommandCollection),
        typeof(Collection<CommandHolder>),
        typeof(RibbonControl),
        new FrameworkPropertyMetadata(null));

    public Collection<CommandHolder> RowListCommandCollection
    {
        get
        {
            var col = (Collection<CommandHolder>?)GetValue(RowListCommandCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(RowListCommandCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(RowListCommandCollectionProperty, value);
    }

    public static readonly DependencyProperty ColumnCommandCollectionProperty =
        DependencyProperty.Register(
        nameof(ColumnCommandCollection),
        typeof(Collection<CommandHolder>),
        typeof(RibbonControl),
        new FrameworkPropertyMetadata(null));

    public Collection<CommandHolder> ColumnCommandCollection
    {
        get
        {
            var col = (Collection<CommandHolder>?)GetValue(ColumnCommandCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(ColumnCommandCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(ColumnCommandCollectionProperty, value);
    }

    public static readonly DependencyProperty RowItemCommandCollectionProperty =
            DependencyProperty.Register(
            nameof(RowItemCommandCollection),
            typeof(Collection<CommandHolder>),
            typeof(RibbonControl),
            new FrameworkPropertyMetadata(null));

    public Collection<CommandHolder> RowItemCommandCollection
    {
        get
        {
            var col = (Collection<CommandHolder>?)GetValue(RowItemCommandCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(RowItemCommandCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(RowItemCommandCollectionProperty, value);
    }

    public static readonly DependencyProperty AllCommandsCollectionProperty =
        DependencyProperty.Register(
        nameof(AllCommandsCollection),
        typeof(Collection<CommandModel>),
        typeof(RibbonControl),
        new FrameworkPropertyMetadata(null));

    public Collection<CommandModel> AllCommandsCollection
    {
        get
        {
            var col = (Collection<CommandModel>?)GetValue(AllCommandsCollectionProperty);
            if (col == null)
            {
                col = [];
                SetValue(AllCommandsCollectionProperty, col);
            }
            return col;
        }
        set => SetValue(AllCommandsCollectionProperty, value);
    }

    public IRadTreeCommandStrategy RadTreeCommandStrategy { get; set; }
    public IRadTreeLayoutUpdateStrategy RadTreeLayoutUpdateStrategy { get; set; }

    public DataTemplateSelector LayoutItemTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(LayoutItemTemplateSelectorProperty);
        set => SetValue(LayoutItemTemplateSelectorProperty, value);
    }

    public RadTreeViewControl(RadTreeViewModel model)
    {
        ViewModel = model;
        DataContext = this;
        InitializeComponent();
        PART_RootGrid.RowDefinitions.Clear();
    }

    private void PART_RootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ResetSelected();
        e.Handled = true;
    }

    public RadTreeViewControl()
    {
        InitializeComponent();
        PART_RootGrid.RowDefinitions.Clear();
        DataContextChanged += RadTreeViewControl_DataContextChanged;
    }

    private void RadTreeViewControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not RadTreeViewControl { DataContext: RadTreeViewModel model }) return;
        ViewModel = model;
        DataContextChanged -= RadTreeViewControl_DataContextChanged;
        DataContext = this;
    }

    public void InitialMenu()
    {
        InitialColumns();
        InitialRows();
    }

    private void Columns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems[0] is not ColumnViewModel header) return;
                    InitialColumn(header);
                    break;
                }

            case NotifyCollectionChangedAction.Remove:

                break;
        }
    }

    private int GetInsertIndex(RowViewModel list)
    {
        var index = 0;
        if (list is RowViewModelList parentList)
        {
            foreach (var child in parentList.Children)
            {
                index += GetInsertIndex(child);
            }
        }
        index++;
        return index;
    }

    private void InsertRow(RowViewModel row)
    {
        if (row.Parent is not RowViewModelList list)
        {
            ElementsIndex.Add(row);
            return;
        }

        int parentIndex = -1;

        for (int i = 0; i < ElementsIndex.Count; i++)
        {
            if (ReferenceEquals(ElementsIndex[i], list))
            {
                parentIndex = i;
                break;
            }
        }

        if (parentIndex < 0)
        {
            ElementsIndex.Add(row);
            return;
        }

        int childOffset = GetInsertIndex(list);

        int insertIndex = parentIndex + childOffset - 1;

        if (insertIndex >= ElementsIndex.Count)
            ElementsIndex.Add(row);
        else
            ElementsIndex.Insert(insertIndex, row);
    }

    private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems?.Count != 1) return;
            if (e.NewItems[0] is not RowViewModel row) return;

            InsertRow(row);
            InitialRow(row);
            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems?.Count != 1) return;
            if (e.OldItems[0] is not RowViewModel row) return;

            RemoveChild(row);
        }
    }

    private void PART_RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsInit && !ViewModel.IsDisposable) return;
        ViewModel.Count = 0;
        var mainGrid = PART_RootGrid;
        mainGrid.ColumnDefinitions.Clear();
        mainGrid.RowDefinitions.Clear();
        InitialMenu();
        ViewModel.IsDisposable = false;
    }

    public void InitialRows()
    {

        ViewModel.AddRows([.. RowsCollection]);
        foreach (var row in ViewModel.Rows)
        {
            InitialRows(row);
        }
        ViewModel.Rows.CollectionChanged -= Rows_CollectionChanged;
        ViewModel.Rows.CollectionChanged += Rows_CollectionChanged;
    }

    private void InitialRows(RowViewModel row)
    {
        if (row is RowViewModelList list)
        {
            InitialRow(row);
            ElementsIndex.Add(row);
            foreach (var it in list.Children)
            {
                InitialRows(it);
            }
        }
        else
        {
            ElementsIndex.Add(row);
            InitialRow(row);
        }
    }

    private void InitialRow(RowViewModel row)
    {
        if (row is RowViewModelList rowViewModel)
        {
            RadTreeCommandStrategy?.AddRowListCommand(row, AllCommandsCollection, RowListCommandCollection, rowViewModel);
            rowViewModel.Children.CollectionChanged += Rows_CollectionChanged;
        }
        else if (row is RowViewModelItem item)
        {
            RadTreeCommandStrategy?.AddRowItemCommand(row, AllCommandsCollection, RowItemCommandCollection, item);
        }

        PART_RootGrid.RowDefinitions.Last().Height = new GridLength(row.RowHeight, GridUnitType.Pixel);

        var column = ViewModel.Columns[0];
        var columnUI = Columns[column];
        var template = LayoutItemTemplateSelector.SelectTemplate(row, columnUI);

        var content = new ContentPresenter()
        {
            Content = row,
            DataContext = row,
            Margin = new Thickness(5, 0, 0, 0),
            ContentTemplate = template
        };

        var rowDef = new RowDefinition()
        {
            Height = new GridLength(1, GridUnitType.Star)
        };

        PART_RootGrid.RowDefinitions.Add(rowDef);

        RowDefs[row] = rowDef;
        ViewModel.Count++;
        ViewModel.RaiseAddItem(row);
        Rows[row] = [content];
        AddBorder(row, content);
    }

    public void InitialColumns()
    {
        if (ColumnsCollection.Count > 0)
        {
            ViewModel.AddColumn([.. ColumnsCollection]);
        }
        foreach (var header in ViewModel.Columns)
        {
            InitialColumn(header);
        }
        ViewModel.Columns.CollectionChanged += Columns_CollectionChanged;
    }

    private void InitialColumn(ColumnViewModel header)
    {
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
        RadTreeCommandStrategy?.AddColumnCommand(header, AllCommandsCollection, ColumnCommandCollection, ViewModel.Rows);

        var content = new ContentPresenter()
        {
            Content = header,
            Tag = header,
            Height = header.ColumnHeight,
            VerticalAlignment = VerticalAlignment.Stretch,
            ContentTemplateSelector = LayoutItemTemplateSelector
        };

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
            PART_RootGrid.Children.Remove(grid);
            Elements.Remove(row);
            ElementsIndex.Remove(row);
        }
        if (OtherElements.TryGetValue(row, out var listother))
        {
            foreach (var el in listother)
            {
                PART_RootGrid.Children.Remove(el);
                if (el is Border border)
                {
                    border.MouseLeftButtonUp -= NewBorder_MouseLeftButtonUp;
                    border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;
                    border.MouseMove -= NewBorder_MouseMove;
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

        if (row.Parent is RowViewModelList parent)
        {
            UpdateElements(parent);
        }

        PART_RootGrid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
    }

    private void AddBorder(RowViewModel row, ContentPresenter content)
    {
        var index = row.GetIndexRowItem();

        if (Rows[row] != null)
        {
            AddGrid(row, index, content);
        }

        var depth = 0;

        if (row.Parent != null)
        {
            depth = row.Parent.DepthChildren;
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

        var contents = Rows[row];

        if (ViewModel.Columns.Count > 0)
        {
            OtherElements[row] = new List<FrameworkElement>(ViewModel.Columns.Count);
        }

        Visibility visibility = Visibility.Collapsed;

        if (row.Parent == null)
        {
            visibility = Visibility.Visible;
        }
        else
        {
            visibility = row.Parent is RowViewModelList { IsOpenChildren: true } ? Visibility.Visible : Visibility.Collapsed;
        }

        for (var i = 1; i < ViewModel.Columns.Count; i++)
        {
            var column = ViewModel.Columns[i];
            var columnUI = Columns[column];
            var template = LayoutItemTemplateSelector.SelectTemplate(row, columnUI);

            var presenter = new ContentPresenter
            {
                Content = row,
                ContentTemplate = template,
            };

            var newBorder = new Border
            {
                BorderThickness = new Thickness(0, 0.5, 0, 0.5),
                BorderBrush = Brushes.LightGray,
                Child = presenter,
                Margin = new Thickness(i == 0 ? row.RowOffset : 0, 0, 0, 0),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                Tag = row,
                DataContext = row
            };

            Rows[row].Add(presenter);

            newBorder.MouseLeftButtonUp += NewBorder_MouseLeftButtonUp;
            newBorder.Visibility = visibility;
            OtherElements[row].Add(newBorder);

            PART_RootGrid.Children.Add(newBorder);
            Grid.SetColumn(newBorder, i);
            if (index == -1)
            {
                throw new IndexOutOfRangeException("Не найден индекс элемента!");
            }

            Grid.SetRow(newBorder, index == 1 ? 1 : index + depth + 1);
        }

        row.UpdateRowsPosition = true;
        if (row.Parent is RowViewModelList parentList)
        {
            UpdateElements(parentList);
        }
    }

    private void ResetSelected()
    {
        foreach (var element in Elements)
        {
            var grid = element.Value;
            for (var i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child is not Border { Name: "PART_newBorder", Child: Border borderChild } borderGrid) continue;
                borderChild.BorderBrush = Brushes.Transparent;
                break;
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
            for (var i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child is not Border { Name: "PART_newBorder", Child: Border borderChild } borderGrid) continue;
                borderChild.BorderBrush = Brushes.Blue;
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
    }

    private void AddGrid(RowViewModel row, int index, ContentPresenter content, Grid? parentGrid = null)
    {
        if (index == -1) throw new IndexOutOfRangeException("Не найден индекс элемента!");

        var borderValue = new Border()
        {
            Child = content,
            BorderThickness = new Thickness(1),
            Background = row.IsEnable ? Brushes.Transparent : Brushes.Gray
        };

        var currentGrid = new Grid()
        {
            Margin = new Thickness(parentGrid == null ? 10 : -RowViewModel.RowOffsetImmutable / 2, 0, parentGrid == null ? 0 : 0, 0),
            Tag = row,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsHitTestVisible = true,
            Background = Brushes.Transparent,
            DataContext = row
        };

        if (row.Parent is RowViewModelList list)
        {
            if (!list.IsOpenChildren)
            {
                currentGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                currentGrid.Visibility = Visibility.Visible;
            }
        }

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


        if (row.Parent != null || row is RowViewModelList { IsFolder: true })
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
            Child = borderValue,
            Margin = new Thickness(row.RowOffset, 0, 0, 0),
            Cursor = Cursors.Hand,
            Tag = row,
            Name = "PART_newBorder"
        };
        currentGrid.Children.Add(newBorder);

        newBorder.MouseLeftButtonDown += Border_MouseLeftButtonDown;
        newBorder.MouseLeftButtonUp += Border_MouseLeftButtonUp;
        newBorder.MouseMove += NewBorder_MouseMove;

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

        if (row.Parent != null)
        {
            if (row.Parent is RowViewModelList parentList)
            {
                if (parentList.IsOpenChildren)
                {
                    PART_RootGrid.Children.Add(currentGrid);
                    Grid.SetColumn(currentGrid, 0);
                    Grid.SetRow(currentGrid, index + 1);
                }
                else
                {

                }
            }
        }
        else
        {
            PART_RootGrid.Children.Add(currentGrid);
            Grid.SetColumn(currentGrid, 0);
            Grid.SetRow(currentGrid, index);
        }

        row.PropertyChanged += Row_PropertyChanged;
    }

    private void NewBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Border { Tag: RowViewModel row }) return;
        MouseItemMove?.Invoke(sender, row, e);
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: RowViewModel row }) return;
        if (row is RowViewModel)
        {
            SelectedElement?.Invoke(row);
            row.SelectedRow();
            NewBorder_MouseLeftButtonUp(sender, e);

            if (MouseLeftItemUp == null || MouseLeftItemUp(sender, row, e))
            {
                if (row is RowViewModelList rowList)
                {
                    if (rowList.Children.Count > 0)
                    {
                        rowList.UpdateRowsPosition = true;
                        rowList.IsOpenChildren = !rowList.IsOpenChildren;
                        rowList.UpdateRowsPosition = false;
                    }
                }
            }
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: RowViewModel row }) return;
        MouseLeftItemDown?.Invoke(sender, row, e);
    }

    private void BorderButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: RowViewModelList rowList }) return;

        rowList.IsOpenChildren = !rowList.IsOpenChildren;
    }

    private void UpdateElements(RowViewModelList rowlist)
    {
        UpdateVisibility(rowlist);
        ChangeRowPlace();

        RecalculateAllLines();
        UpdateBorderThickness(rowlist);
        foreach (var i in rowlist.Children)
        {
            UpdateBorderThickness(i);
        }
    }


    private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

                UpdateElements(rowlist);
            }


        }
        else if (e.PropertyName is nameof(RowViewModel.IsEnable))
        {
            if (Elements.TryGetValue(item, out var grid))
            {
                foreach (var child in grid.Children)
                {
                    if (child is not Border border) continue;
                    if (border.Child is Border borderChild)
                    {
                        borderChild.Background = item.IsEnable ? Brushes.Transparent : Brushes.Gray;
                        break;
                    }
                }
            }
        }
    }

    private bool IsChainOpen(RowViewModel row)
    {
        var parent = row.Parent as RowViewModelList;

        while (parent != null)
        {
            if (!parent.IsOpenChildren)
                return false;

            parent = parent.Parent as RowViewModelList;
        }

        return true;
    }

    private void UpdateVisibility(RowViewModel row)
    {
        var visibility = IsChainOpen(row)
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (Elements.TryGetValue(row, out var grid))
            grid.Visibility = visibility;

        if (OtherElements.TryGetValue(row, out var others))
            foreach (var o in others)
                o.Visibility = visibility;

        if (row is RowViewModelList list)
            foreach (var child in list.Children)
                UpdateVisibility(child);
    }

    private void ChangeRowPlace()
    {
        var invisibleIndex = 0;

        for (var i = 0; i < ElementsIndex.Count; i++)
        {
            var element = Elements[ElementsIndex[i]];
            var otherElements = OtherElements[ElementsIndex[i]];
            if (element.Visibility == Visibility.Visible)
            {
                if (!PART_RootGrid.Children.Contains(element))
                {
                    PART_RootGrid.Children.Add(element);
                }

                Grid.SetColumn(element, 0);
                Grid.SetRow(element, i + 1 - invisibleIndex);
            }
            else
            {
                PART_RootGrid.Children.Remove(element);
                invisibleIndex++;
            }
            var column = 1;
            foreach (var other in otherElements)
            {
                if (other.Visibility == Visibility.Visible)
                {
                    if (!PART_RootGrid.Children.Contains(other))
                    {
                        PART_RootGrid.Children.Add(other);
                    }

                    Grid.SetColumn(other, column++);
                    Grid.SetRow(other, i + 1 - invisibleIndex);
                }
                else
                {
                    PART_RootGrid.Children.Remove(other);
                }
            }
        }
    }

    private bool IsLastVisibleElement(RowViewModel row)
    {
        var searchMode = false;
        foreach (var i in Elements)
        {
            if (i.Key == row)
            {
                searchMode = true;
                continue;
            }
            if (searchMode)
            {
                if (i.Value.Visibility == Visibility.Visible)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void UpdateBorderThickness(RowViewModel item)
    {
        const double left = 1.0;
        const double right = 0.0;
        Thickness thickness = new Thickness(0, 0, 0, 0);

        if (item.Parent is RowViewModelList rowList)
        {
            bool isFirst = ReferenceEquals(rowList.Children.FirstOrDefault(), item);
            bool isLast = ReferenceEquals(rowList.Children.LastOrDefault(), item);
            bool isLastSection = rowList.TopParent.RowListEqualsLast();
            bool isLastItemSection = isLastSection && rowList.TopParent.Children.Count > 0 ? ReferenceEquals(rowList.TopParent.Children.Last(), item) : false;
            bool isLastChildSection = isLastSection && IsLastVisibleElement(item);

            double top = isFirst ? 0.0 : 1.0;
            double bottom;

            if (item is RowViewModelList it)
            {
                if (it.Children.Count > 0)
                {
                    bottom = it.IsOpenChildren ? 1.0 : isLast && isLastChildSection ? 1.0 : 0.0;
                }
                else
                {
                    bottom = (isLastSection && isLast && rowList.IsOpenChildren && isLastItemSection) ? 1.0 : 0.0;
                }
            }
            else
            {
                bottom = (isLastSection && isLast && rowList.IsOpenChildren && isLastItemSection) || (isLastChildSection) ? 1.0 : 0.0;
            }


            thickness = new Thickness(left, top, right, bottom);
        }
        else
        {
            if (item is RowViewModelList rl)
            {
                bool isFirst = rl.IsFirstTopRow();
                bool isLast = rl.RowListEqualsLast();
                double top = isFirst ? 0.0 : 1.0;
                double bottom = 0.0;

                if (rl.Children.Count > 0)
                {
                    if (rl.IsOpenChildren)
                    {
                        bottom = 1.0;
                    }
                    else
                    {
                        bottom = isLast ? 1.0 : 0.0;
                    }
                }
                else
                {
                    if (isLast && isFirst)
                    {
                        bottom = rl.IsOpenChildren ? (rl.IsOpenChildren ? 1.0 : 0.0) : 1.0;
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
                        border.BorderThickness = thickness;
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

    private bool IsRowVisible(RowViewModel item)
    {
        var current = item.Parent;

        while (current != null)
        {
            if (current is RowViewModelList list &&
                !list.IsOpenChildren)
                return false;

            current = current.Parent;
        }

        return true;
    }

    private void RecalculateAllLines()
    {
        foreach (var item in VerticalLines.Keys)
            UpdateHeightLine(item);
    }

    private void UpdateHeightLine(RowViewModel item)
    {
        if (!VerticalLines.TryGetValue(item, out var border))
            return;

        var index = item.GetIndexRowItem();
        if (index == -1) return;

        var rows = 0;
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
            border.Visibility = IsRowVisible(item) ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    public void Dispose()
    {
        try
        {
            DataContextChanged -= RadTreeViewControl_DataContextChanged;

            Invoke(() =>
            {
                if (PART_RootGrid != null)
                {
                    PART_RootGrid.Loaded -= PART_RootGrid_Loaded;
                    PART_RootGrid.MouseLeftButtonDown -= PART_RootGrid_MouseLeftButtonDown;
                }
            });

            if (ViewModel != null)
            {
                if (ViewModel.Rows != null)
                    ViewModel.Rows.CollectionChanged -= Rows_CollectionChanged;

                if (ViewModel.Columns != null)
                    ViewModel.Columns.CollectionChanged -= Columns_CollectionChanged;
            }

            foreach (var kv in ColumnPopups.ToList())
            {
                var col = kv.Key;
                var popup = kv.Value;
                try
                {
                    Invoke(() =>
                    {
                        popup.IsOpen = false;
                        popup.Child = null;
                    });
                }
                catch { /* ignore */ }

                ColumnPopups.Remove(col);
                PopupStartOffsets.Remove(col);
                PopupStartVerticalOffsets.Remove(col);
            }

            foreach (var kv in Columns.ToList())
            {
                var header = kv.Key;
                var contentPresenter = kv.Value;

                try { header.PropertyChanged -= HeaderColumn_PropertyChanged; } catch { }

                Invoke(() =>
                {
                    if (contentPresenter != null)
                        PART_RootGrid.Children.Remove(contentPresenter);
                });

                Columns.Remove(header);

                if (ColumnsDef.TryGetValue(header, out var def))
                {
                    Invoke(() => PART_RootGrid.ColumnDefinitions.Remove(def));
                    ColumnsDef.Remove(header);
                }
            }
            Columns.Clear();
            ColumnsDef.Clear();

            Invoke(() =>
            {
                var splitters = PART_RootGrid.Children.OfType<Border>().Where(b => b.Tag is ColumnViewModel).ToList();
                foreach (var splitter in splitters)
                {
                    splitter.MouseLeftButtonDown -= Splitter_MouseLeftButtonDown;
                    splitter.MouseLeftButtonUp -= Splitter_MouseLeftButtonUp;
                    splitter.MouseMove -= Splitter_MouseMove;
                    PART_RootGrid.Children.Remove(splitter);
                }
            });

            foreach (var kv in RowDefs.ToList())
            {
                var row = kv.Key;
                var rowDef = kv.Value;
                Invoke(() => PART_RootGrid.RowDefinitions.Remove(rowDef));
                RowDefs.Remove(row);
            }
            RowDefs.Clear();

            foreach (var kv in Elements.ToList())
            {
                var row = kv.Key;
                var grid = kv.Value;

                try
                {
                    Invoke(() =>
                    {
                        foreach (var child in grid.Children.OfType<UIElement>().ToList())
                        {
                            if (child is Border b)
                            {
                                b.MouseLeftButtonDown -= Border_MouseLeftButtonDown;
                                b.MouseLeftButtonUp -= Border_MouseLeftButtonUp;
                                b.MouseMove -= NewBorder_MouseMove;
                            }
                        }

                        PART_RootGrid.Children.Remove(grid);
                    });
                }
                catch { /* ignore */ }

                Elements.Remove(row);
            }
            Elements.Clear();

            foreach (var kv in OtherElements.ToList())
            {
                var row = kv.Key;
                var list = kv.Value;
                foreach (var el in list.ToList())
                {
                    if (el is Border border)
                    {
                        border.MouseLeftButtonUp -= NewBorder_MouseLeftButtonUp;
                        border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;
                        border.MouseMove -= NewBorder_MouseMove;
                    }

                    Invoke(() => PART_RootGrid.Children.Remove(el));
                }
                OtherElements.Remove(row);
            }
            OtherElements.Clear();

            foreach (var kv in VerticalLines.ToList())
            {
                var row = kv.Key;
                var border = kv.Value;
                Invoke(() => PART_RootGrid.Children.Remove(border));
                VerticalLines.Remove(row);
            }
            VerticalLines.Clear();

            foreach (var kv in BorderButtons.ToList())
            {
                var row = kv.Key;
                var button = kv.Value;
                button.MouseLeftButtonUp -= BorderButton_MouseLeftButtonUp;
                Invoke(() => PART_RootGrid.Children.Remove(button));
                BorderButtons.Remove(row);
            }
            BorderButtons.Clear();

            foreach (var kv in Rows.ToList())
            {
                var row = kv.Key;
                var presenters = kv.Value;
                foreach (var p in presenters.ToList())
                {
                    Invoke(() => PART_RootGrid.Children.Remove(p));
                }
                Rows.Remove(row);
            }
            Rows.Clear();

            foreach (var row in ElementsIndex.ToList())
            {
                try { row.PropertyChanged -= Row_PropertyChanged; } catch { }

                if (row is RowViewModelList list)
                {
                    try { list.Children.CollectionChanged -= Rows_CollectionChanged; } catch { }
                }
            }
            ElementsIndex.Clear();

            try { RowListCommandCollection?.Clear(); } catch { }
            try { ColumnCommandCollection?.Clear(); } catch { }
            try { RowItemCommandCollection?.Clear(); } catch { }
            try { AllCommandsCollection?.Clear(); } catch { }
            try { ColumnsCollection?.Clear(); } catch { }
            try { RowsCollection?.Clear(); } catch { }

            ColumnPopups.Clear();
            PopupStartOffsets.Clear();
            PopupStartVerticalOffsets.Clear();

            ViewModel = null;
            DataContext = null;

            Invoke(() =>
            {
                try
                {
                    PART_RootGrid.Children.Clear();
                    PART_RootGrid.RowDefinitions.Clear();
                    PART_RootGrid.ColumnDefinitions.Clear();
                }
                catch { }
            });

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RadTreeViewControl.Dispose error: {ex}");
        }
    }

    private void PART_RootGrid_Unloaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsDisposable)
            Dispose();
    }
}
