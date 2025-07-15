using UnityEngine;
using UnityEngine.UI;

// ��������� ��������� IUIManageable ��� ���������� � ����� �������� ���������� UI
public class PauseMenu : MonoBehaviour, IUIManageable
{
    [Header("������")]
    [SerializeField] private GameObject pauseMenuPanel; // ������� ������ ���� �����
    [SerializeField] private GameObject settingsPanel;  // ������ ��������

    [Header("������ �������� ����")]
    [SerializeField] private Button continueButton;     // ������ "����������"
    [SerializeField] private Button settingsButton;     // ������ "���������"
    [SerializeField] private Button exitButton;         // ������ "�����"
    [SerializeField] private Button closeButton;        // ������ "X" ��� ��������

    [Header("������ ���� ��������")]
    [SerializeField] private Button backButton;         // ������ "�����" �� ��������

    private void Start()
    {
        // ������������ ��� ���� � ������� ������������� UI
        if (ExclusiveUIManager.Instance != null)
        {
            ExclusiveUIManager.Instance.Register(this);
        }

        // ����������, ��� ��� ������ ������ ��� �������
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // ��������� �������� ��� ������
        continueButton.onClick.AddListener(CloseMenu);
        closeButton.onClick.AddListener(CloseMenu);
        settingsButton.onClick.AddListener(ShowSettings);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(HideSettings);
    }

    private void OnDestroy()
    {
        // ����� ���������� �� ��������� ��� ����������� �������
        if (ExclusiveUIManager.Instance != null)
        {
            ExclusiveUIManager.Instance.Deregister(this);
        }
    }

    private void Update()
    {
        // ��������/��������� ���� �� ������� �� ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// ����������� ��������� ���� (�������/�������).
    /// </summary>
    private void ToggleMenu()
    {
        // ���� ������� ���� ������� ����, ���� ���������, �� ��������� ��.
        // ����� - ���������.
        if (pauseMenuPanel.activeSelf || settingsPanel.activeSelf)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    /// <summary>
    /// ��������� �������� ���� �����.
    /// </summary>
    private void OpenMenu()
    {
        // �������� ���������, ��� �� ����������� (����� �� ������ ������ ����)
        ExclusiveUIManager.Instance.NotifyPanelOpening(this);

        // ������ ���� �� �����
        GameStateManager.Instance.RequestPause(this);

        // ���������� ������� ������ � �������� ���������
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    /// <summary>
    /// ��������� ��������� ������� ���� �����.
    /// </summary>
    private void CloseMenu()
    {
        // �������� ��� ������
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // ������������ ����
        GameStateManager.Instance.RequestResume(this);
    }

    /// <summary>
    /// ���������� ������ ��������.
    /// </summary>
    private void ShowSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// �������� ������ �������� � ���������� �� ������� ���� �����.
    /// </summary>
    private void HideSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// ������� �� ���� (� ������ ������ ���������).
    /// </summary>
    private void ExitGame()
    {
        Debug.Log("����� �� ����...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region IUIManageable Implementation

    /// <summary>
    /// �����, ���������� ExclusiveUIManager ��� ��������������� ��������.
    /// </summary>
    public void CloseUI()
    {
        CloseMenu();
    }

    /// <summary>
    /// ����� ��� ExclusiveUIManager, ����� ���������, ������� �� ����.
    /// </summary>
    public bool IsOpen()
    {
        return pauseMenuPanel.activeSelf || settingsPanel.activeSelf;
    }

    #endregion
}