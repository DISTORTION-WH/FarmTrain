using UnityEngine;
using UnityEngine.EventSystems;

// ���� ������ ����� �������� �� ����� UI-������� � ����������� Graphic (Image, Button, Text),
// ����� �� ��� ����������� ������� ��������� ����.
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    public string tooltipMessage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(tooltipMessage) && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Show(tooltipMessage, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

    public void SetTooltip(string message)
    {
        this.tooltipMessage = message;
    }
}