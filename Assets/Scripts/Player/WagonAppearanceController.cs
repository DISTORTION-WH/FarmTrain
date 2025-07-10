using UnityEngine;

// ��������, ��� �� ������� ���� SpriteRenderer, ����� �������� ������.
[RequireComponent(typeof(SpriteRenderer))]
public class WagonAppearanceController : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("������ ������ �� ��������� ������.")]
    [SerializeField] private Sprite defaultSprite;

    [Tooltip("������ ������ ����� ��������� ������.")]
    [SerializeField] private Sprite upgradedSprite;

    [Header("Upgrade Data")]
    [Tooltip("������ �� ItemData, ������� ������������ ����� ��������� ��� ������.")]
    [SerializeField] private ItemData storageUpgradeItem;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // �������� ������ �� ���������, ������� ����� ������.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // ���������, ��� ��� ������ �� �����, ����� �������� ������.
        if (storageUpgradeItem == null || defaultSprite == null || upgradedSprite == null)
        {
            Debug.LogError($"�� WagonAppearanceController ({gameObject.name}) �� ��������� ��� ����������� ���� (������� ��� ������ ���������)!", this);
            return;
        }

        // ������������� �� ������� ������� � ��������.
        // ��� �����, ���� ����� ����� ���������, �������� �� ����� � ������� (���� ��� ��������).
        // ���� ��������� ���������� ������ �� �������, ��� �������� ��������� ��� ����������� �� �����.
        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.OnItemPurchased += OnItemPurchased;
        }

        // ����� ��������� ��������� � ��������� ��� ������.
        UpdateWagonAppearance();
    }

    private void OnDestroy()
    {
        // ����� ����� ���������� �� �������, ����� �������� ������ ������ � ������.
        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.OnItemPurchased -= OnItemPurchased;
        }
    }

    /// <summary>
    /// ���� ����� ����������, ����� ����� ���-�� �������� � ��������.
    /// </summary>
    private void OnItemPurchased(ItemData purchasedItem, int quantity)
    {
        // ��� ���������� ������ ��� ������, ����� ��������� ������� - ��� ���� ���������.
        if (purchasedItem == storageUpgradeItem)
        {
            Debug.Log($"<color=cyan>[WagonAppearance]</color> �������� ������� � ������� ��������� ������! �������� ������� ���.");
            UpdateWagonAppearance();
        }
    }

    /// <summary>
    /// ������� �����, ������� ���������, ���� �� ���������, � ������ ������.
    /// </summary>
    private void UpdateWagonAppearance()
    {
        // ���������, ���������� �� �������� ���������.
        if (TrainUpgradeManager.Instance == null)
        {
            Debug.LogError("TrainUpgradeManager �� ������! ���������� ��������� ���������.", this);
            // �� ������ ������ ������ ������ �� ���������.
            spriteRenderer.sprite = defaultSprite;
            return;
        }

        // ���������� � ���������, ������� �� ���������.
        bool isUpgraded = TrainUpgradeManager.Instance.HasUpgrade(storageUpgradeItem);

        // � ����������� �� ������, ������������� ������ ������.
        if (isUpgraded)
        {
            spriteRenderer.sprite = upgradedSprite;
            Debug.Log($"[WagonAppearance] ���������� ���������� ������ ��� ������ {gameObject.name}.");
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
            Debug.Log($"[WagonAppearance] ���������� ������� ������ ��� ������ {gameObject.name}.");
        }
    }
}