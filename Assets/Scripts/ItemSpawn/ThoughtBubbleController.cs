using UnityEngine;

public class ThoughtBubbleController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;

    void Awake()
    {
        if (iconRenderer == null)
        {
            Debug.LogError("Icon Renderer �� ������ ��� �� �������� � ThoughtBubbleController!", gameObject);
        }
        Hide();
    }

    public void Show(ItemData itemToShow)
    {
        if (iconRenderer == null || itemToShow == null)
        {
            Debug.LogError("�� ���� �������� ������� - iconRenderer ��� itemToShow �� ������!", gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (itemToShow.itemName == "Wool")
        {
            // ��������� ������� ������ ������
            iconRenderer.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        else
        {
            // ���������� ����������� ������� ��� ���� ��������� ������
            iconRenderer.transform.localScale = Vector3.one;
        }

        // ������������� ������ � �������� ������
        iconRenderer.sprite = itemToShow.itemIcon;
        gameObject.SetActive(true);
    }



    public void Hide()
    {
        gameObject.SetActive(false);
        if (iconRenderer != null)
        {
            // ���������� ������� �� ������, ���� ������� ������, ���� ��� ���� ������
            iconRenderer.transform.localScale = Vector3.one;
            iconRenderer.sprite = null;
        }
    }
}