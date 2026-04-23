using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RadTreeView.Adorner;

public sealed class DragAdorner: System.Windows.Documents.Adorner
{
    private readonly ContentPresenter _presenter;
    private double _left;
    private double _top;
    private readonly VisualCollection _visualChildren;
    public DragAdorner(UIElement adornedElement, object data,DataTemplate template, double width, double height) : base(adornedElement)
    {
        IsHitTestVisible = false;

        _presenter = new ContentPresenter
        {
            Content = data,
            ContentTemplate = template,
            Width = width,
            Height = height,
            Opacity = 0.85,
            IsHitTestVisible = false
        };

        _visualChildren = new VisualCollection(this) { _presenter };
    }

    public void SetPosition(double left, double top)
    {
        _left = left;
        _top = top;
        InvalidateArrange();
    }


    protected override int VisualChildrenCount => _visualChildren.Count;

    protected override Visual GetVisualChild(int index)
    {
        return _visualChildren[index];
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _presenter.Measure(constraint);
        return _presenter.RenderSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var width = finalSize.Width;
        var height = finalSize.Height;

        if(double.IsNaN(width) || width <= 0)
            width = _presenter.DesiredSize.Width;

        if(double.IsNaN(height) || height <= 0)
            height = _presenter.DesiredSize.Height;

        _presenter.Arrange(new Rect(new Point(_left, _top), new Size(width, height)));
        return finalSize;
    }
}

