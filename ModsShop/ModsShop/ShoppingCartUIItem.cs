using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModsShop;

public class ShoppingCartUIItem : MonoBehaviour
{
    public ShoppingCartUI ui;
    public Button lessBtn;
    public Button moreBtn;
    public Button removeBtn;
    public Text itemText;

#if !Mini
    internal void SetupElement(ShoppingCartUI scui, KeyValuePair<ItemDetails, int> item)
    {
        ui = scui;
        if (!item.Key.MultiplePurchases)
            itemText.text = $"{item.Key.ItemName}";
        else
            itemText.text = $"{item.Key.ItemName} <color=yellow>x{item.Value}</color>";

        removeBtn.onClick.AddListener(() => ui.RemoveItem(item.Key));
        if (item.Key.MultiplePurchases)
        {
            lessBtn.onClick.AddListener(() => ui.MoreLess(item.Key, false));
            moreBtn.onClick.AddListener(() => ui.MoreLess(item.Key, true));
        }
        else
        {
            lessBtn.gameObject.SetActive(false);
            moreBtn.gameObject.SetActive(false);
        }
    }
#endif
}