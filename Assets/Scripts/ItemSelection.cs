using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelection : MonoBehaviour
{
    Text _text;

    void Start()
    {
        _text = GetComponentInChildren<Text>();
        _text.text = string.Empty;

        var allControllers = GameObject.FindObjectsOfType<GridItemController>();

        foreach (var controller in allControllers)
        {
            controller.onItemHovered += HandleItemHover;
        }
    }

    private void HandleItemHover(IProductionItem item)
    {
        if (item != null)
        {
            _text.text = (item as ItemDefinition).Name;
        }
        else
        {
            _text.text = string.Empty;
        }
    }
}
