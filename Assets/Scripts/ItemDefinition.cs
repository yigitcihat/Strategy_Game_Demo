using UnityEngine;

/// <summary>
/// Scriptable Object representing an Production Item
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "Production/Item", order = 1)]
public class ItemDefinition : ScriptableObject, IProductionItem
{
    [SerializeField] private Sprite _sprite = null;
    [SerializeField] private Sprite _productionSprite = null;
    [SerializeField] private GridShape _shape = null;
    [SerializeField] private ItemType _type = ItemType.Unit;  
    [SerializeField] private bool _canDrop = true;
    [SerializeField] private bool _canProduce = true;
    [SerializeField] private string _defination;
    [SerializeField] private ItemDefinition _itemDefination;
    [SerializeField, HideInInspector] private Vector2Int _position = Vector2Int.zero;

    /// <summary>
    /// The name of the item
    /// </summary>
    public string Name => this.name;

    public string Defination => _defination;
    /// <summary>
    /// The type of the item
    /// </summary>
    public ItemType Type => _type;

    /// <inheritdoc />
    public Sprite sprite => _sprite;

    public Sprite productionSprite => _productionSprite;

    /// <inheritdoc />
    public int width
    {
        get => _shape.width;
    }
    /// <inheritdoc />
    public int height
    { 
        get => _shape.height;
    }
    


    /// <inheritdoc />
    public Vector2Int position
    {
        get => _position;
        set => _position = value;
    }

    /// <inheritdoc />
    public bool IsPartOfShape(Vector2Int localPosition)
    {
        return _shape.IsPartOfShape(localPosition);
    }

    /// <inheritdoc />
    public bool canDrop { get => _canDrop; set => _canDrop = value; }
    public bool canProduce { get => _canProduce; set => _canProduce = value; }
    public ItemDefinition itemDefination { get => _itemDefination; set => _itemDefination = value; }



    /// <summary>
    /// Creates a copy if this scriptable object
    /// </summary>
    public IProductionItem CreateInstance()
    {
        var clone = ScriptableObject.Instantiate(this);
      
        clone.name = clone.name.Substring(0, clone.name.Length - 7); // Remove (Clone) from name
        return clone;
    }
}
