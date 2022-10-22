using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : IGridManager
{
    private Vector2Int _size = Vector2Int.one;
    private IGridProvider _provider;
    private Rect _fullRect;

    public GridManager(IGridProvider provider, int width, int height)
    {
        _provider = provider;
        Rebuild();
        Resize(width, height);
    }

    /// <inheritdoc />
    public int width => _size.x;

    /// <inheritdoc />
    public int height => _size.y;

    /// <inheritdoc />
    public void Resize(int newWidth, int newHeight)
    {
        _size.x = newWidth;
        _size.y = newHeight;
        RebuildRect();
    }

    private void RebuildRect()
    {
        _fullRect = new Rect(0, 0, _size.x, _size.y);
        HandleSizeChanged();
        onResized?.Invoke();
    }

    private void HandleSizeChanged()
    {
        // Drop all items that no longer fit the inventory
        for (int i = 0; i < allItems.Length;)
        {
            var item = allItems[i];
            var shouldBeDropped = false;
            var padding = Vector2.one * 0.01f;

            if (!_fullRect.Contains(item.GetMinPoint() + padding) || !_fullRect.Contains(item.GetMaxPoint() - padding))
            {
                shouldBeDropped = true;
            }

            if (shouldBeDropped)
            {
                TryDrop(item);
            }
            else
            {
                i++;
            }
        }
    }

    /// <inheritdoc />
    public void Rebuild()
    {
        Rebuild(false);
    }

    private void Rebuild(bool silent)
    {
        allItems = new IProductionItem[_provider.gridItemCount];
        for (var i = 0; i < _provider.gridItemCount; i++)
        {
            allItems[i] = _provider.GetProductionItem(i);
        }
        if (!silent) onRebuilt?.Invoke();
    }

    public void Dispose()
    {
        _provider = null;
        allItems = null;
    }

    /// <inheritdoc />
    public bool isFull
    {
        get
        {
            if (_provider.isGridFull) return true;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (GetAtPoint(new Vector2Int(x, y)) == null) { return false; }
                }
            }
            return true;
        }
    }

    /// <inheritdoc />
    public IProductionItem[] allItems { get; private set; }

    /// <inheritdoc />
    public Action onRebuilt { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemDropped { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemDroppedFailed { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemAdded { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemAddedFailed { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemRemoved { get; set; }

    /// <inheritdoc />
    public Action onResized { get; set; }

    /// <inheritdoc />
    public IProductionItem GetAtPoint(Vector2Int point)
    {
        // Single item override
        if (_provider.gridRenderMode == GridRenderMode.Single && _provider.isGridFull && allItems.Length > 0)
        {
            return allItems[0];
        }

        foreach (var item in allItems)
        {
            if (item.Contains(point)) { return item; }
        }
        return null;
    }

    /// <inheritdoc />
    public IProductionItem[] GetAtPoint(Vector2Int point, Vector2Int size)
    {
        var posibleItems = new IProductionItem[size.x * size.y];
        var c = 0;
        for (var x = 0; x < size.x; x++)
        {
            for (var y = 0; y < size.y; y++)
            {
                posibleItems[c] = GetAtPoint(point + new Vector2Int(x, y));
                c++;
            }
        }
        return posibleItems.Distinct().Where(x => x != null).ToArray();
    }

    /// <inheritdoc />
    public bool TryRemove(IProductionItem item)
    {
        if (!CanRemove(item)) return false;
        if (!_provider.RemoveProductionItem(item)) return false;
        Rebuild(true);
        onItemRemoved?.Invoke(item);
        return true;
    }

    /// <inheritdoc />
    public bool TryDrop(IProductionItem item)
    {
        if (!CanDrop(item) || !_provider.DropProductionItem(item))
        {
            onItemDroppedFailed?.Invoke(item);
            return false;
        }
        Rebuild(true);
        onItemDropped?.Invoke(item);
        return true;
    }

    internal bool TryForceDrop(IProductionItem item)
    {
        if (!item.canDrop)
        {
            onItemDroppedFailed?.Invoke(item);
            return false;
        }
        onItemDropped?.Invoke(item);
        return true;
    }

    /// <inheritdoc />
    public bool CanAddAt(IProductionItem item, Vector2Int point)
    {
        if (!_provider.CanAddProductionItem(item) || _provider.isGridFull)
        {
            return false;
        }

        if (_provider.gridRenderMode == GridRenderMode.Single)
        {
            return true;
        }

        var previousPoint = item.position;
        item.position = point;
        var padding = Vector2.one * 0.01f;

        // Check if item is outside of inventory
        if (!_fullRect.Contains(item.GetMinPoint() + padding) || !_fullRect.Contains(item.GetMaxPoint() - padding))
        {
            item.position = previousPoint;
            return false;
        }

        // Check if item overlaps another item already in the inventory
        if (!allItems.Any(otherItem => item.Overlaps(otherItem))) return true; // Item can be added
        item.position = previousPoint;
        return false;

    }

    /// <inheritdoc />
    public bool TryAddAt(IProductionItem item, Vector2Int point)
    {
        if (!CanAddAt(item, point) || !_provider.AddProductionItem(item))
        {
            onItemAddedFailed?.Invoke(item);
            return false;
        }
        switch (_provider.gridRenderMode)
        {
            case GridRenderMode.Single:
                item.position = GetCenterPosition(item);
                break;
            case GridRenderMode.Grid:
                item.position = point;
                break;
            default:
                throw new NotImplementedException($"InventoryRenderMode.{_provider.gridRenderMode.ToString()} have not yet been implemented");
        }
        Rebuild(true);
        onItemAdded?.Invoke(item);
        return true;
    }

    /// <inheritdoc />
    public bool CanAdd(IProductionItem item)
    {
        Vector2Int point;
        if (!Contains(item) && GetFirstPointThatFitsItem(item, out point))
        {
            return CanAddAt(item, point);
        }
        return false;
    }

    /// <inheritdoc />
    public bool TryAdd(IProductionItem item)
    {
        if (!CanAdd(item)) return false;
        Vector2Int point;
        return GetFirstPointThatFitsItem(item, out point) && TryAddAt(item, point);
    }

    /// <inheritdoc />
    public bool CanSwap(IProductionItem item)
    {
        return _provider.gridRenderMode == GridRenderMode.Single &&
            DoesItemFit(item) &&
            _provider.CanAddProductionItem(item);
    }

    /// <inheritdoc />
    public void DropAll()
    {
        var itemsToDrop = allItems.ToArray();
        foreach (var item in itemsToDrop)
        {
            TryDrop(item);
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var item in allItems)
        {
            TryRemove(item);
        }
    }

    /// <inheritdoc />
    public bool Contains(IProductionItem item) => allItems.Contains(item);


    /// <inheritdoc />
    public bool CanRemove(IProductionItem item) => Contains(item) && _provider.CanRemoveProductionItem(item);

    /// <inheritdoc />
    public bool CanDrop(IProductionItem item) => Contains(item) && _provider.CanDropProductionItem(item) && item.canDrop;

    /*
     * Get first free point that will fit the given item
     */
    private bool GetFirstPointThatFitsItem(IProductionItem item, out Vector2Int point)
    {
        if (DoesItemFit(item))
        {
            for (var x = 0; x < width - (item.width - 1); x++)
            {
                for (var y = 0; y < height - (item.height - 1); y++)
                {
                    point = new Vector2Int(x, y);
                    if (CanAddAt(item, point)) return true;
                }
            }
        }
        point = Vector2Int.zero;
        return false;
    }

    /* 
     * Returns true if given items physically fits within this inventory
     */
    private bool DoesItemFit(IProductionItem item) => item.width <= width && item.height <= height;

    /*
     * Returns the center post position for a given item within this inventory
     */
    private Vector2Int GetCenterPosition(IProductionItem item)
    {
        return new Vector2Int(
            (_size.x - item.width) / 2,
            (_size.y - item.height) / 2
        );
    }
}
