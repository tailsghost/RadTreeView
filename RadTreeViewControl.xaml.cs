using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RadTreeView;

public partial class RadTreeViewControl
{
    public RadTreeViewModel ViewModel { get; }

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
                    if(PART_RootGrid.RowDefinitions.Count == 0)
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
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(header.IsLast ? 1 : header.ColumnWidth, header.IsLast ? GridUnitType.Star : GridUnitType.Pixel)
                    });
                    Grid.SetColumn(content, PART_RootGrid.ColumnDefinitions.Count - 1);
                    Grid.SetRow(content, 0);
                    PART_RootGrid.Children.Add(content);

                    if(!header.IsLast)
                    {
                        var splitter = new Border
                        {
                            BorderThickness = new Thickness(0,0,2,0),
                            BorderBrush = Brushes.LightGray,
                            Cursor = Cursors.SizeWE,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        };

                        Grid.SetColumn(splitter, PART_RootGrid.ColumnDefinitions.Count-1);
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

    private void Rows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems[0] is not RowViewModel row) return;

                    PART_RootGrid.RowDefinitions.Last().Height = new GridLength(row.RowHeight, GridUnitType.Pixel);

                    for (var i =0; i < row.RowContents.Count; i++)
                    {
                        var it = row.RowContents[i];
                        var content = new ContentControl()
                        {
                            Content = it.Value,
                            Margin = new Thickness(5,0,0,0)
                        };
                        var border = new Border()
                        {
                            BorderThickness = new Thickness(1,1,0,1),
                            BorderBrush = Brushes.LightGray,
                            Child = content,
                            Margin = new Thickness(i == 0 ? row.RowOffset : -1.5, 0, 1.5, 0)
                        };
                        Grid.SetRow(border, PART_RootGrid.RowDefinitions.Count-1);
                        Grid.SetColumn(border, i);
                        PART_RootGrid.Children.Add(border);
                    }

                    PART_RootGrid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });

                    break;
                }
        }
    }
}
