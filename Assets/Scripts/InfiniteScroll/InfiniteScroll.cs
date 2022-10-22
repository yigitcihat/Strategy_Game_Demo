using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    [SerializeField]
    private float outOfBoundsThreshold;

    private ContentPositinier contentPositinier;

    private ScrollRect scrollRect;

    private Vector2 lastDragPosition;

    private bool positiveDrag;

    private RectTransform canvasRect;

    private float spacePower;


    private void Start()
    {
        spacePower = 2;
        canvasRect = transform.root.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        contentPositinier = GetComponentInChildren<ContentPositinier>();
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        positiveDrag = eventData.position.y > lastDragPosition.y;



        lastDragPosition = eventData.position;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (contentPositinier.Vertical)
        {
            positiveDrag = eventData.scrollDelta.y > 0;
        }
        else
        {
            positiveDrag = eventData.scrollDelta.y < 0;
        }
    }

    public void OnViewScroll()
    {
        HandleVerticalScroll();
    }

    private void HandleVerticalScroll()
    {
        int currItemIndex = positiveDrag ? scrollRect.content.childCount - 1 : 0;
        var currItem = scrollRect.content.GetChild(currItemIndex);

        if (!ReachedThreshold(currItem))
        {
            return;
        }

        int endItemIndex = positiveDrag ? 0 : scrollRect.content.childCount - 1;
        Transform endItem = scrollRect.content.GetChild(endItemIndex);
        RectTransform endItemRectTransform = endItem.GetComponent<RectTransform>();
        Vector2 newPos = endItem.GetComponent<RectTransform>().position;
        if (canvasRect.localScale.y >= .7f)
            spacePower = 2;

        else
            spacePower = 1.5f;
        if (positiveDrag)
        {
            newPos.y = endItemRectTransform.position.y - (100 * canvasRect.localScale.y) - contentPositinier.ChildHeight * spacePower + contentPositinier.ItemSpacing;

        }
        else
        {
            newPos.y = endItemRectTransform.position.y + (100 * canvasRect.localScale.y) + contentPositinier.ChildHeight * spacePower - contentPositinier.ItemSpacing;
        }
        currItem.GetComponent<RectTransform>().position = newPos;
        currItem.SetSiblingIndex(endItemIndex);
    }

    private bool ReachedThreshold(Transform item)
    {
        float heightPower = outOfBoundsThreshold;
        if (canvasRect.localScale.y > 1f)
        {
            heightPower = outOfBoundsThreshold * 10;
        }
        else
        {
            heightPower = outOfBoundsThreshold / 3;
        }
        float posYThreshold = transform.position.y + contentPositinier.Height * 0.5f + heightPower;
        float negYThreshold = transform.position.y - contentPositinier.Height * 0.5f - heightPower;
        return positiveDrag ? item.position.y - contentPositinier.ChildHeight * 0.5f > posYThreshold :
            item.position.y + contentPositinier.ChildHeight * 0.5f < negYThreshold;


    }

}
