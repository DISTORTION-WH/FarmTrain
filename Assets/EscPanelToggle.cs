using UnityEngine;

public class EscPanelToggle : MonoBehaviour
{
    public GameObject panelToOpen; // ������, ������� ����� ��������
    public AudioClip openSound;
    public AudioSource audioSource;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            // ������������ ���������� ������
            panelToOpen.SetActive(!panelToOpen.activeSelf);
        }
    }
}
