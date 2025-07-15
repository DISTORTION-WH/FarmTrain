// QuestLogEntryUI.cs (������������ ������)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class QuestLogEntryUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image checkmarkImage;
    [SerializeField] private Button pinButton;
    [SerializeField] private Image pinIcon;
    [SerializeField] private Button mainButton;

    [Header("Pin Sprites")]
    [SerializeField] private Sprite pinnedSprite;
    [SerializeField] private Sprite unpinnedSprite;
    [SerializeField] private Sprite pinHighlightedSprite;

    [Header("Button Sprites")]
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite buttonSelectedSprite;
    [SerializeField] private Sprite buttonOnSprite;

    [Header("Text Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material disabledMaterial;

    public Quest assignedQuest { get; private set; }
    private Action<Quest> onSelectCallback;

    // <<< ��������� 1: ����� ���������� ��� ������� ���������� ���������
    private bool isSelected = false;    // ��� ������ �������? ����������� �����.
    private bool isPointerInside = false; // ��������� �� ������ ��� ���� �������?
    private bool isPointerDown = false;   // ������ �� ��� �� ���� ������?

    private void Awake()
    {
        if (titleText == null) titleText = GetComponentInChildren<TextMeshProUGUI>();
        if (titleText != null) titleText.raycastTarget = false;
    }

    public void Setup(Quest quest, Action<Quest> selectCallback)
    {
        assignedQuest = quest;
        onSelectCallback = selectCallback;
        isSelected = false; // ��� ����� ��������� ���������� ���������

        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(HandleSelection);
        pinButton.onClick.RemoveAllListeners();
        pinButton.onClick.AddListener(HandlePinning);

        UpdateVisuals(); // ��������� ������� ���
    }

    // <<< ��������� 2: ������� �����, ������� ������������� ��������� ������ �����
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals(); // ����� ��������� ���������, ��������� ������� ���
    }

    // <<< ��������� 3: ��� ������ ������� ������ ������ ������ ����� � �������� ���� ����� ��� ���������� ����
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!mainButton.interactable) return;
        isPointerDown = true;
        UpdateVisuals();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mainButton.interactable) return;
        isPointerDown = false;
        UpdateVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        isPointerDown = false; // ���� ������ ����, �� � ������� ����������
        UpdateVisuals();
    }

    private void HandleSelection()
    {
        onSelectCallback?.Invoke(assignedQuest);
    }

    private void HandlePinning()
    {
        QuestManager.Instance.PinQuest(assignedQuest);
        // QuestLogUI ������� ��� ��������, ������� ����
    }

    // <<< ��������� 4: ������ �����, ������� ������, ��� ������ ��������� ������
    private void UpdateVisuals()
    {
        if (assignedQuest == null) return;

        titleText.text = assignedQuest.title;

        var buttonImage = mainButton.GetComponent<Image>();

        // ������� ����������� �� �������� ������
        if (assignedQuest.status == QuestStatus.Completed)
        {
            titleText.fontMaterial = disabledMaterial;
            checkmarkImage.gameObject.SetActive(true);
            pinButton.gameObject.SetActive(false);
            mainButton.interactable = false;
            if (buttonImage != null) buttonImage.sprite = buttonSprite;
            return; // �������, ������ ��������� �� �����
        }

        // ���� ����� �� ��������
        mainButton.interactable = true;
        checkmarkImage.gameObject.SetActive(false);
        pinButton.gameObject.SetActive(true);

        // ������ ���������� ������� ��� �� ������ �����������:
        // 1. ������� (����� ������� ���������)
        // 2. ������
        // 3. ��������
        // 4. ������� ��������� (����������/�� ����������)
        if (isSelected)
        {
            if (buttonImage != null) buttonImage.sprite = buttonSelectedSprite;
            titleText.fontMaterial = selectedMaterial;
            pinIcon.sprite = pinnedSprite; // � ��������� ��������� ������� ������ "�������"
        }
        else if (isPointerDown)
        {
            if (buttonImage != null) buttonImage.sprite = buttonSelectedSprite; // ����� ������������ ��� �� ������, ��� � ��� ������
            titleText.fontMaterial = selectedMaterial;
            pinIcon.sprite = pinnedSprite;
        }
        else if (isPointerInside)
        {
            if (buttonImage != null) buttonImage.sprite = buttonOnSprite;
            titleText.fontMaterial = highlightedMaterial;
            pinIcon.sprite = pinHighlightedSprite; // ����������� ������ ��� ������������ �������
        }
        else // ������� ���������
        {
            if (buttonImage != null) buttonImage.sprite = buttonSprite;
            titleText.fontMaterial = normalMaterial;
            pinIcon.sprite = assignedQuest.isPinned ? pinnedSprite : unpinnedSprite;
        }
    }
}