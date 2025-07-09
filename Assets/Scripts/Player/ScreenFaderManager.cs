// ScreenFaderManager.cs

using UnityEngine;
using System.Collections;
using UnityEngine.UI; // <<< �������� ���

public class ScreenFaderManager : MonoBehaviour
{
    public static ScreenFaderManager Instance { get; private set; }

    [SerializeField] private CanvasGroup faderCanvasGroup; // <<< ���������: ������ ������ �� CanvasGroup
    [SerializeField] private float fadeDuration = 1.0f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // � ���������� ���������� ��� FaderImage � ����������� CanvasGroup � ��� ����
        if (faderCanvasGroup == null)
        {
            Debug.LogError("Fader Canvas Group �� �������� � ScreenFaderManager!");
            return;
        }

        // ����������, ��� �� ��������� ��� ������
        faderCanvasGroup.alpha = 0f;
    }

    // ScreenFaderManager.cs
    public IEnumerator FadeOutAndInCoroutine(System.Action onMiddleOfFade = null)
    {
        // ��� �������� ��� ��, ������ �������� �������� ������
        yield return StartCoroutine(Fade(1f));
        onMiddleOfFade?.Invoke();
        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator FadeCoroutine(System.Action onMiddleOfFade)
    {
        // 1. ������� ���������� (Fade Out)
        yield return StartCoroutine(Fade(1f));

        // 2. �������� � ��������, ���� ����� ������
        onMiddleOfFade?.Invoke();

        // 3. ������� ���������� (Fade In)
        yield return StartCoroutine(Fade(0f));
    }

    // ��������������� �������� ��� ������ �������� ��������� �����
    private IEnumerator Fade(float targetAlpha)
    {
        float time = 0;
        float startAlpha = faderCanvasGroup.alpha;

        while (time < fadeDuration)
        {
            // Lerp - �������� ������������ �� startAlpha � targetAlpha �� �����
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null; // ���� ���������� �����
        }

        // �����������, ��� � ����� ����� ����� ����� ����� �������� ��������
        faderCanvasGroup.alpha = targetAlpha;
    }
}