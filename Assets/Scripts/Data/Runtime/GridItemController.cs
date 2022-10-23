using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IProductionController
{
    Action<IProductionItem> onItemHovered { get; set; }
    Action<IProductionItem> onItemPicked { get; set; }
    Action<IProductionItem> onItemPickedUp { get; set; }
    Action<IProductionItem> onItemAdded { get; set; }
    Action<IProductionItem> onItemSwapped { get; set; }
    Action<IProductionItem> onItemReturned { get; set; }
    Action<IProductionItem> onItemDropped { get; set; }
}

/// <summary>
/// Enables human interaction with an inventory renderer using Unity's event systems
/// </summary>
[RequireComponent(typeof(GridRenderer))]
public class GridItemController : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler,
    IEndDragHandler, IPointerExitHandler, IPointerEnterHandler,
    IProductionController
{
    private bool _isReturnOrSwap;
    // The dragged item is static and shared by all controllers
    // This way items can be moved between controllers easily
    private static ProductionDraggedItem _draggedItem;

    /// <inheritdoc />
    public Action<IProductionItem> onItemHovered { get; set; }

    public Action<IProductionItem> onItemPicked { get; set; }
    /// <inheritdoc />
    public Action<IProductionItem> onItemPickedUp { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemAdded { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemSwapped { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemReturned { get; set; }

    /// <inheritdoc />
    public Action<IProductionItem> onItemDropped { get; set; }

    private Canvas _canvas;
    internal GridRenderer inventoryRenderer;
    internal GridManager inventory => (GridManager)inventoryRenderer.inventory;

    private IProductionItem _itemToDrag;
    private PointerEventData _currentEventData;
    private IProductionItem _lastHoveredItem;
    private IProductionItem _simulationHoveredItem;
    /*
     * Setup
     */
    void Awake()
    {
        inventoryRenderer = GetComponent<GridRenderer>();
        if (inventoryRenderer == null) { throw new NullReferenceException("Could not find a renderer. This is not allowed!"); }

        // Find the canvas
        var canvases = GetComponentsInParent<Canvas>();
        if (canvases.Length == 0) { throw new NullReferenceException("Could not find a canvas."); }
        _canvas = canvases[canvases.Length - 1];
    }

    /*
     * Grid was clicked (IPointerDownHandler)
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_draggedItem != null) return;
        // Get which item to drag (item will be null of none were found)
        var grid = ScreenToGrid(eventData.position);
        _itemToDrag = inventory.GetAtPoint(grid);
        inventory.TryAdd(inventory.GetAtPoint(grid));
        onItemPicked?.Invoke(_itemToDrag);


}

    /*
     * Dragging started (IBeginDragHandler)
     */
    public void OnBeginDrag(PointerEventData eventData)
    {
        inventoryRenderer.ClearSelection();

        if (_itemToDrag == null || _draggedItem != null || !_itemToDrag.canDrop) return;

        var localPosition = ScreenToLocalPositionInRenderer(eventData.position);
        var itemOffest = inventoryRenderer.GetItemOffset(_itemToDrag);
        var offset = itemOffest - localPosition;

        // Create a dragged item 
        _draggedItem = new ProductionDraggedItem(
            _canvas,
            this,
            _itemToDrag.position,
            _itemToDrag,
            offset
        );
        // Remove the item from inventory
       
        
        inventory.TryRemove(_itemToDrag);
       
        onItemPickedUp?.Invoke(_itemToDrag);
    }

    /*
     * Dragging is continuing (IDragHandler)
     */
    public void OnDrag(PointerEventData eventData)
    {
        _currentEventData = eventData;
        if (_draggedItem != null)
        {
            // Update the items position
            //_draggedItem.Position = eventData.position;
        }
    }

    /*
     * Dragging stopped (IEndDragHandler)
     */
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggedItem == null) return;

        var mode = _draggedItem.Drop(eventData.position);

        switch (mode)
        {
            case ProductionDraggedItem.DropMode.Added:
                onItemAdded?.Invoke(_itemToDrag);
                Debug.Log("New Production created");
                GetComponent<SizeGridCreator>().CreateGridAndItems();
                _isReturnOrSwap = false;
                break;
            case ProductionDraggedItem.DropMode.Selected:
                onItemPicked?.Invoke(_itemToDrag);
                _isReturnOrSwap = false;
                break;
            case ProductionDraggedItem.DropMode.Swapped:
                onItemSwapped?.Invoke(_itemToDrag);
                _isReturnOrSwap = true;
                break;
            case ProductionDraggedItem.DropMode.Returned:
                onItemReturned?.Invoke(_itemToDrag);
                _isReturnOrSwap = true;
                break;
            case ProductionDraggedItem.DropMode.Dropped:
                onItemDropped?.Invoke(_itemToDrag);
               
                _draggedItem.item.canDrop = false;
                _isReturnOrSwap = false;
                ClearHoveredItem();
                break;
        }
        if (!_isReturnOrSwap)
        {
            if (_draggedItem != null)
            {
                _draggedItem.item.canDrop = false;
            }
            
           
        }
        else
        {
            if (_draggedItem != null)
            {
                _draggedItem.item.canDrop = true;
            }
            _isReturnOrSwap = false;
        }
        _draggedItem = null;
    }

    /*
     * Pointer left the inventory (IPointerExitHandler)
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_draggedItem != null)
        {
            // Clear the item as it leaves its current controller
            
            _draggedItem.currentController = null;
            inventoryRenderer.ClearSelection();
        }
        else { ClearHoveredItem(); }
        _currentEventData = null;
    }

    /*
     * Pointer entered the inventory (IPointerEnterHandler)
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_draggedItem != null)
        {
            // Change which controller is in control of the dragged item
            _draggedItem.currentController = this;
        }
        _currentEventData = eventData;
    }

    /*
     * Update loop
     */
    void Update()
    {
        if (_currentEventData == null) return;

        if (_draggedItem == null)
        {
            // Detect hover
            var grid = ScreenToGrid(_currentEventData.position);
            var item = inventory.GetAtPoint(grid);
            if (item == _lastHoveredItem) return;
            onItemHovered?.Invoke(item);
            _lastHoveredItem = item;
        }
        else
        {
            // Update position while dragging
            _draggedItem.position = _currentEventData.position;
        }
    }

    /* 
     * 
     */
    private void ClearHoveredItem()
    {
        if (_lastHoveredItem != null)
        {
            onItemHovered?.Invoke(null);
        }
        _lastHoveredItem = null;
    }

    /*
     * Get a point on the grid from a given screen point
     */
    internal Vector2Int ScreenToGrid(Vector2 screenPoint)
    {
        var pos = ScreenToLocalPositionInRenderer(screenPoint);
        var sizeDelta = inventoryRenderer.rectTransform.sizeDelta;
        pos.x += sizeDelta.x / 2;
        pos.y += sizeDelta.y / 2;
        return new Vector2Int(Mathf.FloorToInt(pos.x / inventoryRenderer.cellSize.x), Mathf.FloorToInt(pos.y / inventoryRenderer.cellSize.y));
    }

    private Vector2 ScreenToLocalPositionInRenderer(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryRenderer.rectTransform,
            screenPosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out var localPosition
        );
        return localPosition;
    }
}
