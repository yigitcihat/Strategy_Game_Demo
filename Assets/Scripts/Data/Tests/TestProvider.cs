using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProvider : IGridProvider
{
    private readonly List<IProductionItem> _items = new List<IProductionItem>();
    private readonly int _maximumAlowedItemCount;

    /// <summary>
    /// CTOR
    /// </summary>
    public TestProvider(GridRenderMode renderMode = GridRenderMode.Grid, int maximumAlowedItemCount = -1)
    {
        gridRenderMode = renderMode;
        _maximumAlowedItemCount = maximumAlowedItemCount;
    }

    public int gridItemCount => _items.Count;

    public GridRenderMode gridRenderMode { get; }

    public bool isGridFull
    {
        get
        {
            if (_maximumAlowedItemCount < 0) return false;
            return gridItemCount < _maximumAlowedItemCount;
        }
    }

    public bool AddProductionItem(IProductionItem item)
    {
        if (_items.Contains(item)) return false;
        _items.Add(item);
        return true;
    }

    public bool DropProductionItem(IProductionItem item) => RemoveProductionItem(item);
    public IProductionItem GetProductionItem(int index) => _items[index];
    public bool CanAddProductionItem(IProductionItem item) => true;
    public bool CanRemoveProductionItem(IProductionItem item) => true;
    public bool CanDropProductionItem(IProductionItem item) => true;
    public bool RemoveProductionItem(IProductionItem item) => _items.Remove(item);
}
