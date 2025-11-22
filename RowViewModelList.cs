
using System.Windows.Media.Imaging;

namespace RadTreeView;

public class RowViewModelList : RowViewModel
{
    public RowViewModelList(int rows, IList<RowViewModelList> toprows, IEnumerable<Content> contents, RowViewModel? parent = null) : base(rows, toprows, contents, parent)
    {
    }

    public void ChangeState()
    {
        IsOpenChildren = !IsOpenChildren;
    }


    public RowViewModelList AddChildrenList(IEnumerable<Content> contents)
    {
        return AddChidlren(new RowViewModelList(_rowCount, _topRows, contents, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = new BitmapImage(
            new Uri("pack://application:,,,/RadTreeViewTest;component/Assets/Project_Property_Icon.png")),
        }) as RowViewModelList;
    }

    public RowViewModelItem AddChildrenItem(IEnumerable<Content> contents)
    {
        return AddChidlren(new RowViewModelItem(_rowCount, _topRows, contents, this)
        {
            RowOffset = RowOffset + RowOffsetImmutable,
            TopParent = TopParent,
            Image = new BitmapImage(
     new Uri("pack://application:,,,/RadTreeViewTest;component/Assets/TagItem.png")),
        }) as RowViewModelItem;
    }


    public void CloseAllNodes(RowViewModelList row)
    {
        foreach(var child in row.Children)
        {
            if (child is not RowViewModelList rowList) continue;
            rowList.IsOpenChildren = false;
            CloseAllNodes(rowList);
        }
    }

    public void OpenAllNodes(RowViewModelList row)
    {
        foreach (var child in row.Children)
        {
            if (child is not RowViewModelList rowList) continue;
            rowList.IsOpenChildren = true;
            OpenAllNodes(rowList);
        }
    }

    private RowViewModel AddChidlren(RowViewModel item)
    {
        Children.Add(item);
        item.DepthChildren = DepthChildren + 1;
        IsOpenChildren = true;
        return item;
    }
}
