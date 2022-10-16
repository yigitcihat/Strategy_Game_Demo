using UnityEngine;

public class ContentPositinier : MonoBehaviour
{
    #region Public Properties
    public float ItemSpacing { get { return itemSpacing; } }
    public float VerticalMargin { get { return verticalMargin; } }
    public bool Vertical { get { return vertical; } }
    public float Height { get { return height; } }
    public float ChildHeight { get { return childHeight; } }
    #endregion
    #region Private Members
    private RectTransform rectTransform;

    private RectTransform[] rtChildren;

    private float height;

    private float  childHeight;

    [SerializeField]
    private float itemSpacing;

    [SerializeField]
    private float verticalMargin;

    [SerializeField]
    private bool  vertical;
    #endregion
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rtChildren = new RectTransform[rectTransform.childCount];

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            rtChildren[i] = rectTransform.GetChild(i) as RectTransform;
        }

        // Subtract the margin from the top and bottom.
        height = rectTransform.rect.height - (2f * verticalMargin);

        childHeight = rtChildren[0].rect.height;

        InitializeContentVertical();

    }

    // Initializes the scroll content if the scroll view is oriented horizontally.
    private void InitializeContentVertical()
    {
        float originY = 0 - (height * 0.5f);
        float posOffset = childHeight * 0.5f;
        for (int i = 0; i < rtChildren.Length; i++)
        {
            Vector2 childPos = rtChildren[i].localPosition;
            childPos.y = originY + posOffset + i * (childHeight + itemSpacing);
            rtChildren[i].localPosition = childPos;
        }
    }
}
