using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelection : MonoBehaviour
{
    public Text _nameText;
    public Text _definationText;
    public Image _definationImage;
    public GameObject _productionFrame;
    Image _productionImage;
    void Start()
    {
        _nameText.text = string.Empty;
        _definationImage.enabled = false;
        _productionImage = _productionFrame.transform.GetChild(0).GetComponent<Image>();
        var allControllers = GameObject.FindObjectsOfType<GridItemController>();

        foreach (var controller in allControllers)
        {
            controller.onItemPicked += HandleItemPicked;
            controller.onItemHovered += HandleItemHover;
        }
    }

    private void HandleItemHover(IProductionItem item)
    {
        if (item != null && item.canDrop)
        {
            _nameText.text = (item as ItemDefinition).Name;
            _definationText.text = (item as ItemDefinition).Defination;
            _definationImage.enabled = true;
            _definationImage.sprite = (item as ItemDefinition).sprite;
            _productionFrame.SetActive(false);
            _productionImage.sprite = null;
        }
    }
    private void HandleItemPicked(IProductionItem item)
    {
        if (item != null)
        {
            Debug.Log("Picked");
            _nameText.text = (item as ItemDefinition).Name;
            _definationText.text = (item as ItemDefinition).Defination;
            _definationImage.enabled = true;
            _definationImage.sprite = (item as ItemDefinition).sprite;
            if ((item as ItemDefinition).canProduce)
            {
                _productionFrame.SetActive(true);
                _productionImage.sprite = (item as ItemDefinition).productionSprite;
            }
            else
            {
                _productionFrame.SetActive(false);
                _productionImage.sprite = null;
            }
        }
        else
        {
            Debug.Log(" no Picked");
            _nameText.text = string.Empty;
            _definationText.text = string.Empty;
            _definationImage.sprite = null;
            _definationImage.enabled = false;
            _productionFrame.SetActive(false);
            _productionImage.sprite = null;
        }
    }
}
