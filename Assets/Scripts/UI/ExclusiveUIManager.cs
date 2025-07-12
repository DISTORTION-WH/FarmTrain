using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ���������� "��������", �������� ������ ��������� ��� ���� ������
public interface IUIManageable
{
    void CloseUI();
    bool IsOpen();
}

public class ExclusiveUIManager : MonoBehaviour
{
    public static ExclusiveUIManager Instance { get; private set; }

    // ������ ���� �������, ������� �����, ����� ��� ���������
    private List<IUIManageable> registeredPanels = new List<IUIManageable>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // ����� ��� ����������� ������ � ����� �������
    public void Register(IUIManageable panel)
    {
        if (!registeredPanels.Contains(panel))
        {
            registeredPanels.Add(panel);
        }
    }

    // ����� ��� �������� ������ �� ������� (�� ������ ������)
    public void Deregister(IUIManageable panel)
    {
        if (registeredPanels.Contains(panel))
        {
            registeredPanels.Remove(panel);
        }
    }

    // ������� �����! ���������� �������, ������� ����� ���������.
    public void NotifyPanelOpening(IUIManageable openingPanel)
    {
        // �������� �� ���� ������������������ �������
        foreach (var panel in registeredPanels)
        {
            // ���� ��� �� �� ������, ������� �����������, � ��� ������ �������...
            if (panel != openingPanel && panel.IsOpen())
            {
                // ...��������� ��!
                panel.CloseUI();
            }
        }
    }
}