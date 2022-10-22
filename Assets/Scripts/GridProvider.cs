using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridProvider : IGridProvider
{
    private List<IProductionItem> _items = new List<IProductionItem>();
    private int _maximumAlowedItemCount;
    ItemType _allowedItem;

    /// <summary>
    /// CTOR
    /// </summary>
    public GridProvider(GridRenderMode renderMode, int maximumAlowedItemCount = -1, ItemType allowedItem = ItemType.Any)
    {
        gridRenderMode = renderMode;
        _maximumAlowedItemCount = maximumAlowedItemCount;
        _allowedItem = allowedItem;
    }

    public int gridItemCount => _items.Count;

    public GridRenderMode gridRenderMode { get; private set; }

    public bool isGridFull
    {
        get
        {
            if (_maximumAlowedItemCount < 0) return false;
            return gridItemCount >= _maximumAlowedItemCount;
        }
    }

    public bool AddProductionItem(IProductionItem item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
            return true;
        }
        return false;
    }

    public bool DropProductionItem(IProductionItem item)
    {
        return RemoveProductionItem(item);
    }

    public IProductionItem GetProductionItem(int index)
    {
        return _items[index];
    }

    public bool CanAddProductionItem(IProductionItem item)
    {
        if (_allowedItem == ItemType.Any) return true;
        return (item as ItemDefinition).Type == _allowedItem;
    }

    public bool CanRemoveProductionItem(IProductionItem item)
    {
        return true;
    }

    public bool CanDropProductionItem(IProductionItem item)
    {
        return true;
    }

    public bool RemoveProductionItem(IProductionItem item)
    {
        return _items.Remove(item);
    }
}
