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


    // ShopItemRow.cs

    public void Setup(ShopItem shopItem, int shopStock, int playerItemCount, bool isBuyMode, Action<ShopItem> buttonCallback)
    {
        this.currentShopItem = shopItem;
        this.onActionButtonClicked = buttonCallback;
        var itemData = shopItem.itemData;

        itemIcon.sprite = itemData.itemIcon;
        descriptionText.text = itemData.description;

        if (isBuyMode)
        {
            buttonText.text = "Buy";
            priceText.text = $"{shopItem.buyPrice} BYN";
            availableText.text = shopItem.isInfiniteStock ? "In stock" : $"{shopStock}";

            // --- ������ ����� ������ �������� ---

            // 1. ������� ��������: ������� �� ����� � ���� �� ����� �� ������.
            bool canAfford = PlayerWallet.Instance.HasEnoughMoney(shopItem.buyPrice);
            bool hasStock = shopItem.isInfiniteStock || shopStock > 0;

            // �������� � �������������, ��� ������ �����, ���� ������� ������� ���������.
            bool isPurchaseable = canAfford && hasStock;

            // 2. ���� ������� ������� ������, �������� ����� �������, ����������� ��� ���� ��������, ��������.
            if (isPurchaseable)
            {
                switch (itemData.itemType)
                {
                    // �������� ��� ���������
                    case ItemType.Upgrade:
                        // ��� ��������� ��� ������?
                        if (InventoryManager.Instance.StorageUpgradeData == itemData)
                        {
                            isPurchaseable = !TrainUpgradeManager.Instance.HasUpgrade(itemData);
                        }
                        // �����, ����� ��� ��������� ��� ������?
                        else
                        {
                            var allConfigs = AnimalPenManager.Instance.GetAllPenConfigs();
                            var animalForThisUpgrade = allConfigs.FirstOrDefault(c => c.upgradeLevels.Any(l => l.requiredUpgradeItem == itemData))?.animalData;

                            if (animalForThisUpgrade != null)
                            {
                                ItemData nextUpgrade = AnimalPenManager.Instance.GetNextAvailableUpgrade(animalForThisUpgrade);
                                isPurchaseable = (nextUpgrade == itemData);
                            }
                            else
                            {
                                isPurchaseable = false; // ����������� ���������
                                Debug.LogWarning($"�� ������� ���������� ���������� ���������: {itemData.name}");
                            }
                        }
                        break;

                    // �������� ��� ��������
                    case ItemType.Animal:
                        var animalData = itemData.associatedAnimalData;
                        if (animalData != null)
                        {
                            int currentCount = AnimalPenManager.Instance.GetAnimalCount(animalData);
                            int maxCapacity = AnimalPenManager.Instance.GetMaxCapacityForAnimal(animalData);
                            if (currentCount >= maxCapacity)
                            {
                                isPurchaseable = false; // ��� ����� � ������
                            }
                        }
                        else
                        {
                            isPurchaseable = false; // ������ � ������
                        }
                        break;

                    // �������� ��� ���� ��������� ��������� (������, �����������, ��������)
                    default:
                        // ���������, ���� �� ����� � ��������� ���� �� ��� 1 �����
                        if (!InventoryManager.Instance.CheckForSpace(itemData, 1))
                        {
                            isPurchaseable = false; // ��� ����� � ���������
                        }
                        break;
                }
            }

            actionButton.interactable = isPurchaseable;

            // --- ����� ����� ������ �������� ---
        }
        else // ����� �������
        {
            buttonText.text = "Sell";
            priceText.text = $"{shopItem.sellPrice} BYN";
            availableText.text = $"You have: {playerItemCount}";
            bool playerHasItem = playerItemCount > 0;
            actionButton.interactable = playerHasItem && shopItem.willBuy;
        }

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        onActionButtonClicked?.Invoke(currentShopItem);
    }
}