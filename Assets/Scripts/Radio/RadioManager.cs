using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class RadioManager : MonoBehaviour, IUIManageable
{
    public static RadioManager Instance;

    [System.Serializable]
    public class RadioStation
    {
        public string name; 
        public AudioClip[] tracks;
        public bool isUnlocked;
    }
    public AudioClip closeSound;           // ���� ��������
    [System.Serializable]
    public class StationSprites
    {
        public Sprite normalSprite;
        public Sprite selectedSprite;
    }

    public StationSprites[] stationSprites;
    public RadioStation[] stations;
    public Button[] stationButtons;
    public AudioSource audioSource;
    public TextMeshProUGUI stationText;
    public TextMeshProUGUI songText;
    public Slider progressSlider;
    public GameObject radioPanel;
    public Button openButton;
    public Button closeButton;
    public Button repeatButton;
    public Sprite repeatOffSprite;
    public Sprite repeatOnSprite;
    public Button shuffleButton;
    public Sprite shuffleOffSprite;
    public Sprite shuffleOnSprite;
    private Image[] stationButtonImages;
    private float trackLength;
    private float currentPlayTime;
    private float targetPlayPosition;

    private int currentStationIndex = 0;
    private int currentTrackIndex = 0;
    public static int CurrentStationIndex { get; set; }
    public static int CurrentTrackIndex { get; set; }
    public static bool IsPlaying { get; set; }
    private bool isDragging = false;
    private bool isRepeatOn = false;
    private bool isShuffleOn = false;
    private List<int> shuffledTracks = new List<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (ExclusiveUIManager.Instance != null)
            {
                ExclusiveUIManager.Instance.Register(this);
            }

            if (radioPanel == null)
            {
                radioPanel = transform.Find("RadioPanel")?.gameObject;
                if (radioPanel == null) Debug.LogError("RadioPanel �� ������!");
            }

            radioPanel.SetActive(false);
        }   
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        openButton.onClick.AddListener(OpenRadioPanel);
        closeButton.onClick.AddListener(CloseRadioPanel);
        radioPanel.SetActive(false);

        stationButtonImages = new Image[stationButtons.Length];
        for (int i = 0; i < stationButtons.Length; i++)
        {
            stationButtonImages[i] = stationButtons[i].GetComponent<Image>();
        }

        HighlightCurrentStation();
    }

    private void HighlightCurrentStation()
    {
        for (int i = 0; i < stationButtons.Length; i++)
        {
            if (i >= stationSprites.Length || stationSprites[i] == null)
            {
                Debug.LogWarning($"�� ������� �������� ��� ������� {i}");
                continue;
            }

            var button = stationButtons[i].GetComponent<Button>();
            var image = stationButtonImages[i];

            if (i == currentStationIndex)
            {
                image.sprite = stationSprites[i].selectedSprite;
            }
            else
            {
                image.sprite = stationSprites[i].normalSprite;
            }

            button.interactable = stations[i].isUnlocked;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (radioPanel != null)
        {
            radioPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (stationButtons.Length != stations.Length || stationButtons.Length != stationSprites.Length)
        {
            Debug.LogError($"�������������� ��������: ������={stationButtons.Length}, �������={stations.Length}, ��������={stationSprites.Length}");
        }

        for (int i = 0; i < stations.Length; i++)
        {
            stations[i].isUnlocked = (i == 0);

            if (i < stationButtons.Length)
            {
                int stationIndex = i;
                stationButtons[i].onClick.AddListener(() => SwitchStation(stationIndex));

                var buttonText = stationButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = stations[i].name;
            }
        }

        HighlightCurrentStation();

        UpdateText();

        radioPanel?.SetActive(false);

        if (stations[0].tracks.Length > 0)
        {
            Debug.Log($"RadioManager: ��������������� ������� ����� �� ������� {stations[0].name}");
            PlayTrack();
        }
        else
        {
            Debug.LogWarning("RadioManager: �� ������� ������ ��� ������ �������");
        }
        UpdateRepeatButtonVisual();
        UpdateShuffleButtonVisual();
    }

    void Update()
    {
        if (IsPlaying && !isDragging)
        {
            currentPlayTime = audioSource.time;
            progressSlider.value = currentPlayTime;

            if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.1f)
            {
                if (isRepeatOn)
                {
                    Debug.Log("RadioManager: ������ �����");
                    audioSource.time = 0;
                    audioSource.Play();
                }
                else
                {
                    Debug.Log("RadioManager: ���� ����������, ������ ���������");
                    PlayNextTrack();
                }
            }
        }
    }

    public void OpenRadioPanel()
    {
        ExclusiveUIManager.Instance.NotifyPanelOpening(this);
        GameStateManager.Instance.RequestPause(this);
        radioPanel.SetActive(true);
        HighlightCurrentStation();
    }

    public void CloseRadioPanel()
    {
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        updated_sound sound = closeButton.GetComponentInChildren<updated_sound>();
        if (sound != null)
        {
            sound.PlaySound();
        }
        HideRadioPanel();
    }

    public void HideRadioPanel()
    {
        if (radioPanel != null && radioPanel.activeSelf)
        {
            GameStateManager.Instance.RequestResume(this);
            radioPanel.SetActive(false);
            HighlightCurrentStation();
        }
    }

    public void PlayTrack()
    {
        Debug.Log($"Trying to play track: Station={currentStationIndex}, Track={currentTrackIndex}, TotalTracks={stations[currentStationIndex].tracks.Length}");

        if (stations[currentStationIndex].tracks.Length == 0)
        {
            Debug.LogError("No tracks available for current station!");
            return;
        }

        if (audioSource.isPlaying && audioSource.clip == stations[currentStationIndex].tracks[currentTrackIndex]) return;

        CurrentStationIndex = currentStationIndex;
        CurrentTrackIndex = currentTrackIndex;
        IsPlaying = true;

        if (stations[currentStationIndex].tracks.Length == 0)
        {
            Debug.LogWarning($"RadioManager: �� ������� ��� ��������� ������ {stations[currentStationIndex].name}");
            return;
        }

        Debug.Log($"RadioManager: ��������������� ����� {currentTrackIndex} �� �������{stations[currentStationIndex].name}");

        audioSource.clip = stations[currentStationIndex].tracks[currentTrackIndex];
        audioSource.Play();
        IsPlaying = true;

        trackLength = audioSource.clip.length;
        progressSlider.maxValue = trackLength;
        progressSlider.value = 0;

        UpdateText();
    }

    public void PlayNextTrack()
    {
        Debug.Log("RadioManager: ������������ �� ��������� ����");

        if (isShuffleOn)
        {
            if (shuffledTracks.Count == 0)
            {
                Debug.Log("RadioManager: �������������� ������ ������������ ������");
                ShuffledTracks();
            }

            currentTrackIndex = shuffledTracks[0];
            shuffledTracks.RemoveAt(0);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % stations[currentStationIndex].tracks.Length;
        }

        PlayTrack();
    }

    public void PlayPreviousTrack()
    {
        Debug.Log("RadioManager: ������������ �� ���������� ����");

        if (isShuffleOn)
        {
            if (shuffledTracks.Count == 0)
            {
                Debug.Log("RadioManager: �������������� ������ ������������ ������");
                ShuffledTracks();
            }

            currentTrackIndex = shuffledTracks[shuffledTracks.Count - 1];
            shuffledTracks.RemoveAt(shuffledTracks.Count - 1);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex - 1 + stations[currentStationIndex].tracks.Length) % stations[currentStationIndex].tracks.Length;
        }

        PlayTrack();
    }

    public void TogglePlayPause()
    {
        if (IsPlaying)
        {
            Debug.Log("RadioManager: �����");
            audioSource.Pause();
            IsPlaying = false;
        }
        else
        {
            Debug.Log("RadioManager: �������������");
            audioSource.Play();
            IsPlaying = true;
        }
        UpdateText();
    }

    public void ToggleRepeat()
    {
        isRepeatOn = !isRepeatOn;
        Debug.Log($"RadioManager: ����� ������� {(isRepeatOn ? "�������" : "��������")}");
        UpdateRepeatButtonVisual();
        UpdateText();
    }

    private void UpdateRepeatButtonVisual()
    {
        if (repeatButton != null)
        {
            repeatButton.image.sprite = isRepeatOn ? repeatOnSprite : repeatOffSprite;
        }
    }

    public void ToggleShuffle()
    {
        isShuffleOn = !isShuffleOn;
        Debug.Log($"RadioManager: ����� ������������� {(isShuffleOn ? "�������" : "��������")}");
        if (isShuffleOn)
            ShuffledTracks();
        UpdateShuffleButtonVisual();
        UpdateText();
    }

    private void UpdateShuffleButtonVisual()
    {
        if (shuffleButton != null)
        {
            shuffleButton.image.sprite = isShuffleOn ? shuffleOnSprite : shuffleOffSprite;
        }
    }

    public void SwitchStation(int stationIndex)
    {
        if (stationIndex < 0 || stationIndex >= stations.Length)
        {
            Debug.LogError($"RadioManager: �������� ������ ������� {stationIndex}");
            return;
        }

        if (!stations[stationIndex].isUnlocked)
        {
            Debug.LogWarning($"RadioManager: ������� {stations[stationIndex].name} ��� �� ��������������");
            return;
        }

        Debug.Log($"RadioManager: ������������ �� ������� {stations[stationIndex].name}");
        currentStationIndex = stationIndex;
        currentTrackIndex = 0;
        HighlightCurrentStation();
        PlayTrack();
    }

    public void UnlockStation(int stationIndex)
    {
        if (stationIndex < 0 || stationIndex >= stations.Length)
        {
            Debug.LogError($"�������� ������ �������: {stationIndex}");
            return;
        }

        stations[stationIndex].isUnlocked = true;

        if (stationIndex < stationButtons.Length)
        {
            stationButtons[stationIndex].interactable = true;
        }
        Debug.Log($"RadioManager: ������� {stations[stationIndex].name} ��������������");
        UpdateText();
    }

    private void ShuffledTracks()
    {
        Debug.Log("RadioManager: ������������� ������");
        shuffledTracks = Enumerable.Range(0, stations[currentStationIndex].tracks.Length).ToList();
        shuffledTracks = shuffledTracks.OrderBy(x => Random.value).ToList();
    }

    private void UpdateText()
    {
        stationText.text = stations[currentStationIndex].name;
        songText.text = stations[currentStationIndex].tracks[currentTrackIndex].name;
    }

    public void OnSliderBeginDrag()
    {
        isDragging = true;
        Debug.Log("RadioManager: ������ ���������");
    }

    public void OnSliderEndDrag()
    {
        isDragging = false;
        audioSource.time = progressSlider.value;
        Debug.Log($"RadioManager: ��������� ���������. ����� �����: {progressSlider.value}");
    }

    public void SliderValueChanged()
    {
        if (isDragging)
        {
            targetPlayPosition = progressSlider.value;
        }
    }

    public void UpdateRadioByLevel(int level)
    {
        Debug.Log($"���������� ����� ��� ������ {level}");

        for (int i = 0; i < stations.Length; i++)
        {
            bool isUnlocked = (i < level);
            stations[i].isUnlocked = isUnlocked;

            if (i < stationButtons.Length)
            {
                stationButtons[i].interactable = isUnlocked;
            }
        }

        int targetStationIndex = Mathf.Clamp(level - 1, 0, stations.Length - 1);
        if (currentStationIndex != targetStationIndex || audioSource.clip != stations[targetStationIndex].tracks[0])
        {
            currentStationIndex = targetStationIndex;
            currentTrackIndex = 0;
            HighlightCurrentStation();
            PlayTrack();
        }
    }

    public void CloseUI()
    {
        CloseRadioPanel();
    }

    public bool IsOpen()
    {
        return radioPanel != null && radioPanel.activeSelf;
    }


    private void OnDestroy()
    {
        if (ExclusiveUIManager.Instance != null)
        {
            ExclusiveUIManager.Instance.Deregister(this);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}