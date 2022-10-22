
/// <summary>
/// A provider for a single grid
/// </summary>
public interface IGridProvider
{
    /// <summary>
    /// Returns the render mode of this grid
    /// </summary>
    GridRenderMode gridRenderMode { get; }

    /// <summary>
    /// Returns the total amount of grid items in 
    /// this grid
    /// </summary>
    int gridItemCount { get; }

    /// <summary>
    /// Returns true if the grid is full
    /// </summary>
    bool isGridFull { get; }

    /// <summary>
    /// Returns the grid item at given index
    /// </summary>
    IProductionItem GetProductionItem(int index);

    /// <summary>
    /// Returns true if given grid item is allowed inside 
    /// this grid
    /// </summary>
    bool CanAddProductionItem(IProductionItem item);

    /// <summary>
    /// Returns true if given Production item is allowed to 
    /// be removed from this Production
    /// </summary>
    bool CanRemoveProductionItem(IProductionItem item);

    /// <summary>
    /// Returns true if given Production item is allowed to 
    /// be dropped on the ground
    /// </summary>
    bool CanDropProductionItem(IProductionItem item);

    /// <summary>
    /// Invoked when an invProductionentory item is added to the 
    /// grid. Returns true if successful.
    /// </summary>
    bool AddProductionItem(IProductionItem item);

    /// <summary>
    /// Invoked when an Production item is removed to the 
    /// grid. Returns true if successful.
    /// </summary>
    bool RemoveProductionItem(IProductionItem item);

    /// <summary>
    /// Invoked when an Production item is removed from the 
    /// grid and should be placed on the ground.
    /// Returns true if successful.
    /// </summary>
    bool DropProductionItem(IProductionItem item);
}
