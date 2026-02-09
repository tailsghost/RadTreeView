using System.Collections.ObjectModel;

namespace RadTreeView.Interfaces;

public interface ICollectionCommand
{
    Collection<CommandModel> Commands { get; set; }
}
