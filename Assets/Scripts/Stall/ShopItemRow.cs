// ShopItemRow.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class ShopItemRow : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI availableText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private Action<ShopItem> onActionButtonClicked;
    private ShopItem currentShopItem;

    // ���������� TooltipTrigger ���������, �� �� �������������� � Awake
    private TooltipTrigger actionButtonTooltip;

    // --- ������� ����� AWAKE() ��������� ---
    /*
    private void Awake()
    {
       // ����� ������ ������ ���
    }
    */

    public void Setup(ShopItem shopItem, int shopStock, int playerItemCount, bool isBuyMode, Action<ShopItem> buttonCallback)
    {
        // --- ������ ��������� ---
        // �������������� TooltipTrigger ����� �����, � ������ Setup.
        if (actionButtonTooltip == null) // ���������, ����� �� ������ ��� ������ ���
        {
            actionButtonTooltip = actionButton.GetComponent<TooltipTrigger>();
            if (actionButtonTooltip == null)
            {
                Debug.LogError($"�� ������ '{actionButton.name}' � ������� ShopItemRow ����������� ��������� TooltipTrigger!", gameObject);
                // ���� ���������� ���, �� �� ����� ����������, ����� ����� ������.
                // ����� ������ ����� �� ������.
                return;
            }
        }
        // --- ����� ��������� ---

        this.currentShopItem = shopItem;
        this.onActionButtonClicked = buttonCallback;
        var itemData = shopItem.itemData;

        itemIcon.sprite = itemData.itemIcon;
        descriptionText.text = itemData.description;

        if (isBuyMode)
        {
            buttonText.text = "Buy";
            priceText.text = $"{shopItem.buyPrice} BYN";
            availableText.text = shopItem.isInfiniteStock ? "Available" : $"{shopStock}";

            bool isPurchaseable = ShopUIManager.Instance.IsItemPurchaseable(shopItem, out string reason);

            actionButton.interactable = isPurchaseable;
            actionButtonTooltip.SetTooltip(isPurchaseable ? "" : reason); // ������ actionButtonTooltip �� ������ ���� null
        }
        else // ����� �������
        {
            buttonText.text = "Sell";
            priceText.text = $"{shopItem.sellPrice} BYN";
            availableText.text = $"You have: {playerItemCount}";

            bool canSell = false;
            string reason = "";

            if (shopItem.itemData.itemType == ItemType.Animal)
            {
                canSell = playerItemCount > 1;
                if (!canSell && playerItemCount == 1) reason = "You can't sell the last one";
            }
            else
            {
                canSell = playerItemCount > 0;
            }

            actionButton.interactable = canSell && shopItem.willBuy;
            actionButtonTooltip.SetTooltip(actionButton.interactable ? "" : reason);
        }

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        onActionButtonClicked?.Invoke(currentShopItem);
    }
}