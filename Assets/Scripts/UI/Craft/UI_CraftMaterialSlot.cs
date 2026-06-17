using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftMaterialSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameValue;

    public void SetUpMaterialSlot(ItemData_SO itemData, int avaliableAmount, int requireAmount)
    {
        this.icon.sprite = itemData.itemIcon;

        this.nameValue.text = itemData.itemName + " - " + avaliableAmount + "/" + requireAmount;
    }
}
