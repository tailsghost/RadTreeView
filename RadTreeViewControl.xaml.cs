using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RadTreeView;

public partial class RadTreeViewControl
{

    private int _index = 0;

    public RadTreeViewModel ViewModel { get; }

    private Dictionary<RowViewModel, Grid> Elements = [];

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

                    PART_RootGrid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });

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
            if (row.Parent == null)
            {
                AddGrid(row, index);
            }
            else
            {
                var elements = Elements.ToDictionary();
                foreach (var item in elements)
                {
                    if (row.Parent != item.Key) continue;
                    AddGrid(row, index, item.Value);
                    break;
                }
            }
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
        var content = row.MainContent;
        var value = content.Value;

        var currentGrid = new Grid()
        {
            Margin = new Thickness(parentGrid == null ? 10 : -RowViewModel.RowOffsetImmutable / 2, 0, parentGrid == null ? 0 : 0, 0),
            Tag = row,
            HorizontalAlignment = HorizontalAlignment.Stretch
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
            Text = "➕",
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
            Margin = new Thickness(row.RowOffset - rowOffset, 0, -row.RowOffset + rowOffset, 0),
            Background = Brushes.Azure,
            Cursor = Cursors.Hand
        };
        Panel.SetZIndex(borderButton, 10);

        var lineBorder = new Border
        {
            Margin = new Thickness(row.RowOffset - rowOffset / 2, 0, -row.RowOffset + rowOffset, 0),
            Width = 20 + rowOffset,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0.5, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        var lineBorderDown = new Border
        {
            Margin = new Thickness(row.RowOffset - rowOffset / 2, 0, -row.RowOffset + rowOffset, 0),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Bottom
        };

        currentGrid.Children.Add(borderButton);
        currentGrid.Children.Add(lineBorder);
        currentGrid.Children.Add(lineBorderDown);
        Grid.SetColumn(lineBorder, 0);
        Grid.SetColumn(borderButton, 0);
        Grid.SetColumn(lineBorderDown, 0);

        if (row.Parent != null)
        {
            if (!row.Parent.IsOpenChildren)
            {
                lineBorderDown.Visibility = Visibility.Collapsed;
            }
            var count = row.Parent.Children.Count;

            if (count == 1)
            {

                lineBorderDown.Height = 25 / 2;
                row.Parent.FirstCompleted = true;
            }
            else
            {
                if (!row.Parent.FirstCompleted)
                {
                    lineBorderDown.Height = 25 / 2;
                    var margin = lineBorderDown.Margin;

                    lineBorderDown.Margin = new Thickness(margin.Left, margin.Top - lineBorderDown.Height, margin.Right, margin.Bottom + lineBorderDown.Height);
                    row.Parent.FirstCompleted = true;
                }
                else
                {
                    lineBorderDown.Height = 25;
                    var margin = lineBorderDown.Margin;

                    lineBorderDown.Margin = new Thickness(margin.Left, margin.Top - lineBorderDown.Height / 2, margin.Right, margin.Bottom + lineBorderDown.Height / 2);
                }
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

        if (parentGrid != null)
        {
            parentGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(row.RowHeight, GridUnitType.Pixel)
            });

            var newRowIndex = parentGrid.RowDefinitions.Count - 1;
            var border = new Border();
            parentGrid.Children.Add(currentGrid);

            Grid.SetRowSpan(parentGrid, parentGrid.RowDefinitions.Count);

            Grid.SetColumn(currentGrid, 2);
            Grid.SetRow(currentGrid, newRowIndex);
            Grid.SetColumnSpan(currentGrid, Math.Max(1, parentGrid.ColumnDefinitions.Count - 2));
        }
        else
        {
            PART_RootGrid.Children.Add(currentGrid);
            Grid.SetColumn(currentGrid, 0);
            if (index == -1) throw new IndexOutOfRangeException("Не найден индекс элемента!");
            Grid.SetRow(currentGrid, index + 1);
        }

        Elements.Add(row, currentGrid);
    }
}
