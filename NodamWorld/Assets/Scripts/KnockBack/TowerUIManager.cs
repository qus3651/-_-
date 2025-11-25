using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager Instance { get; private set; }

    [Header("Nickname Input Panel")]
    public GameObject nicknameInputPanel;
    public TMP_InputField nicknameInputField;
    public Button btnNicknameConfirm;
    public TMP_Text nicknameErrorText;

    [Header("Lobby Panel")]
    public GameObject lobbyPanel;
    public Button btnLobbyStart;
    public Button btnLobbyQuit;
    public TMP_Text lobbyCoinText;

    [Header("Guide Popup")]
    public GameObject guidePopup;
    public GameObject guideText1;
    public GameObject guideText2;
    public GameObject guideText3;  // ← 추가
    public Button btnGuideNext;
    public TMP_Text btnGuideNextLabel;

    [Header("Start Button")]
    public GameObject startButtonObject;
    public Button btnStartGame;

    [Header("HUD")]
    public GameObject hudPanel;
    public TMP_Text playerNameText;
    public TMP_Text floorText;
    public TMP_Text stageText;
    public TMP_Text distanceText;
    public TMP_Text playTimeText;
    public TMP_Text coinText;
    public Image currentDirectionImage;
    public Sprite leftDirectionSprite;
    public Sprite rightDirectionSprite;
    public Button nextDirButton;
    public TMP_Text nextDirButtonLabel;
    public Button btnSettings;  // ← 추가 (Settings 버튼)

    [Header("Settings Popup")]  // ← 추가
    public GameObject settingsPopup;
    public Toggle toggleBGM;
    public Toggle toggleCoinSound;
    public Button btnCloseSettings;

    [Header("GameOver Panel")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverFloorText;
    public TMP_Text gameOverPlayTimeText;
    public TMP_Text gameOverEarnedCoinText;
    public TMP_Text gameOverTotalCoinText;
    public Button btnRestart;
    public Button btnQuit;

    [Header("Error Panel")]
    public GameObject errorPanel;
    public TMP_Text errorText;
    public Button btnErrorConfirm;

    [Header("BGM")]
    public AudioSource audioSource;
    public AudioClip lobbyBGM;
    public AudioClip mainBGM;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    [Header("Audio Settings")]  // ← 추가
    public bool isBGMEnabled = true;
    public bool isCoinSoundEnabled = true;

    private int guideStep = 0;
    private int lastFloor;
    private float lastDistance;
    private float lastPlayTime;
    private int lastCoins;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 닉네임 입력
        if (btnNicknameConfirm != null) btnNicknameConfirm.onClick.AddListener(OnClickNicknameConfirm);

        // 로비 버튼
        if (btnLobbyStart != null) btnLobbyStart.onClick.AddListener(OnClickLobbyStart);
        if (btnLobbyQuit != null) btnLobbyQuit.onClick.AddListener(OnClickLobbyQuit);

        // 가이드 팝업
        if (btnGuideNext != null) btnGuideNext.onClick.AddListener(OnClickGuideNext);
        
        // START 버튼
        if (btnStartGame != null) btnStartGame.onClick.AddListener(OnClickStartGame);

        // HUD 버튼
        if (nextDirButton != null) nextDirButton.onClick.AddListener(OnClickToggleDirection);
        if (btnSettings != null) btnSettings.onClick.AddListener(OnClickSettings);  // ← 추가

        // Settings 팝업
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(OnClickCloseSettings);  // ← 추가
        if (toggleBGM != null) toggleBGM.onValueChanged.AddListener(OnToggleBGM);  // ← 추가
        if (toggleCoinSound != null) toggleCoinSound.onValueChanged.AddListener(OnToggleCoinSound);  // ← 추가

        // 게임오버 버튼
        if (btnRestart != null) btnRestart.onClick.AddListener(OnClickRestart);
        if (btnQuit != null) btnQuit.onClick.AddListener(OnClickQuitToLobby);

        // 에러 확인
        if (btnErrorConfirm != null) btnErrorConfirm.onClick.AddListener(OnClickErrorConfirm);

        // 패널 초기화
        if (nicknameInputPanel) nicknameInputPanel.SetActive(false);
        if (lobbyPanel) lobbyPanel.SetActive(false);
        if (guidePopup) guidePopup.SetActive(false);
        if (startButtonObject) startButtonObject.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (errorPanel) errorPanel.SetActive(false);
        if (settingsPopup) settingsPopup.SetActive(false);  // ← 추가

        // AudioSource 설정
        if (audioSource != null)
        {
            audioSource.volume = bgmVolume;
        }

        // 설정 초기화
        LoadAudioSettings();
    }

    // ===== 오디오 설정 =====

    private void LoadAudioSettings()
    {
        isBGMEnabled = PlayerPrefs.GetInt("BGM_Enabled", 1) == 1;
        isCoinSoundEnabled = PlayerPrefs.GetInt("CoinSound_Enabled", 1) == 1;

        if (toggleBGM != null) toggleBGM.isOn = isBGMEnabled;
        if (toggleCoinSound != null) toggleCoinSound.isOn = isCoinSoundEnabled;

        ApplyAudioSettings();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("BGM_Enabled", isBGMEnabled ? 1 : 0);
        PlayerPrefs.SetInt("CoinSound_Enabled", isCoinSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyAudioSettings()
    {
        if (audioSource != null)
        {
            audioSource.mute = !isBGMEnabled;
        }
    }

    private void OnToggleBGM(bool value)
    {
        isBGMEnabled = value;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    private void OnToggleCoinSound(bool value)
    {
        isCoinSoundEnabled = value;
        SaveAudioSettings();
    }

    public bool IsCoinSoundEnabled()
    {
        return isCoinSoundEnabled;
    }

    // ===== Settings 팝업 =====

    private void OnClickSettings()
    {
        if (settingsPopup) settingsPopup.SetActive(true);
        Time.timeScale = 0f;  // 게임 일시정지
    }

    private void OnClickCloseSettings()
    {
        if (settingsPopup) settingsPopup.SetActive(false);
        Time.timeScale = 1f;  // 게임 재개
    }

    // ===== 닉네임 입력 =====
    
    public void ShowNicknameInput()
    {
        if (nicknameInputPanel) nicknameInputPanel.SetActive(true);
        if (nicknameInputField) nicknameInputField.text = "";
        if (nicknameErrorText) nicknameErrorText.text = "";
        
        PlayBGM(lobbyBGM);
    }

    private void OnClickNicknameConfirm()
    {
        if (nicknameInputField == null) return;

        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            if (nicknameErrorText) nicknameErrorText.text = "닉네임을 입력해주세요.";
            return;
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            if (nicknameErrorText) nicknameErrorText.text = "닉네임은 2~10자여야 합니다.";
            return;
        }

        if (TossGameLoginManager.Instance != null)
        {
            TossGameLoginManager.Instance.RegisterNickname(nickname);
        }

        if (nicknameInputPanel) nicknameInputPanel.SetActive(false);
    }

    // ===== 로그인 완료 =====

    public void OnLoginComplete(string nickname, int totalCoins)
    {
        Debug.Log($"[TowerUI] Login complete - Nickname: {nickname}, Coins: {totalCoins}");
        
        if (playerNameText != null)
        {
            playerNameText.text = nickname;
        }

        ShowLobby();
        UpdateLobbyCoinDisplay(totalCoins);
    }

    private void UpdateLobbyCoinDisplay(int totalCoins)
    {
        if (lobbyCoinText != null)
        {
            lobbyCoinText.text = $"코인: {totalCoins}";
        }
    }

    // ===== 로비 =====

    public void ShowLobby()
    {
        if (lobbyPanel) lobbyPanel.SetActive(true);
        if (nicknameInputPanel) nicknameInputPanel.SetActive(false);
        if (guidePopup) guidePopup.SetActive(false);
        if (startButtonObject) startButtonObject.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (settingsPopup) settingsPopup.SetActive(false);

        Time.timeScale = 1f;  // 게임 속도 복원

        if (TossGameLoginManager.Instance != null)
        {
            UpdateLobbyCoinDisplay(TossGameLoginManager.Instance.totalCoins);
        }

        PlayBGM(lobbyBGM);
    }

    private void OnClickLobbyStart()
    {
        if (lobbyPanel) lobbyPanel.SetActive(false);
        ShowGuidePopup();
    }

    private void OnClickLobbyQuit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ===== 가이드 (3개) =====

    private void ShowGuidePopup()
    {
        guideStep = 0;
        if (guidePopup) guidePopup.SetActive(true);
        if (startButtonObject) startButtonObject.SetActive(false);
        UpdateGuideText();
    }

    private void UpdateGuideText()
    {
        if (guideStep == 0)
        {
            if (guideText1) guideText1.SetActive(true);
            if (guideText2) guideText2.SetActive(false);
            if (guideText3) guideText3.SetActive(false);
            if (btnGuideNextLabel) btnGuideNextLabel.text = "다음";
        }
        else if (guideStep == 1)
        {
            if (guideText1) guideText1.SetActive(false);
            if (guideText2) guideText2.SetActive(true);
            if (guideText3) guideText3.SetActive(false);
            if (btnGuideNextLabel) btnGuideNextLabel.text = "다음";
        }
        else if (guideStep == 2)
        {
            if (guideText1) guideText1.SetActive(false);
            if (guideText2) guideText2.SetActive(false);
            if (guideText3) guideText3.SetActive(true);
            if (btnGuideNextLabel) btnGuideNextLabel.text = "다음";
        }
    }

    private void OnClickGuideNext()
    {
        guideStep++;
        
        if (guideStep <= 2)
        {
            UpdateGuideText();
        }
        else if (guideStep == 3)
        {
            if (guidePopup) guidePopup.SetActive(false);
            PrepareGameStart();
        }
    }

    // ===== 게임 준비 (맵/공 생성, START 버튼 표시) =====

    private void PrepareGameStart()
    {
        if (hudPanel) hudPanel.SetActive(true);
        if (startButtonObject) startButtonObject.SetActive(true);

        if (nextDirButton != null)
        {
            nextDirButton.interactable = false;
        }

        PlayBGM(mainBGM);

        var gm = Object.FindFirstObjectByType<TowerGameManager>();
        if (gm != null)
        {
            gm.PrepareGame();
        }
    }

    // ===== START 버튼 =====

    private void OnClickStartGame()
    {
        if (startButtonObject) startButtonObject.SetActive(false);

        if (nextDirButton != null)
        {
            nextDirButton.interactable = true;
        }

        var gm = Object.FindFirstObjectByType<TowerGameManager>();
        if (gm != null)
        {
            gm.StartGame();
        }
    }

    // ===== HUD =====

    public void UpdateHUD(int floor, int stage, float distance, float playTime, int coins, TowerSquareController.Direction nextDir)
    {
        if (floorText) floorText.text = $"{floor}M";
        if (stageText) stageText.text = $"{stage} 단계";
        if (distanceText) distanceText.text = $"{distance:0.0}m";
        if (playTimeText) playTimeText.text = FormatTime(playTime);
        if (coinText) coinText.text = $"{coins}";
        UpdateNextDirLabel(nextDir);
        UpdateCurrentDirectionImage(nextDir);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes}:{seconds:00}";
    }

    public void UpdateNextDirOnly(TowerSquareController.Direction nextDir)
    {
        UpdateNextDirLabel(nextDir);
        UpdateCurrentDirectionImage(nextDir);
    }

    private void UpdateNextDirLabel(TowerSquareController.Direction nextDir)
    {
        if (nextDirButtonLabel)
            nextDirButtonLabel.text = (nextDir == TowerSquareController.Direction.Left) ? "←" : "→";
    }

    private void UpdateCurrentDirectionImage(TowerSquareController.Direction nextDir)
    {
        if (currentDirectionImage != null)
        {
            if (nextDir == TowerSquareController.Direction.Left && leftDirectionSprite != null)
            {
                currentDirectionImage.sprite = leftDirectionSprite;
            }
            else if (nextDir == TowerSquareController.Direction.Right && rightDirectionSprite != null)
            {
                currentDirectionImage.sprite = rightDirectionSprite;
            }
        }
    }

    private void OnClickToggleDirection()
    {
        var square = Object.FindFirstObjectByType<TowerSquareController>();
        if (square != null && !square.IsGameOver)
        {
            square.ToggleNextDirection();
        }
    }

    // ===== 게임오버 =====

    public void ShowGameOver(int floor, float distance, float playTime, int earnedCoins)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        
        if (gameOverFloorText)
        {
            gameOverFloorText.text = $"{floor}";
        }

        if (gameOverPlayTimeText)
        {
            gameOverPlayTimeText.text = FormatTime(playTime);
        }

        if (gameOverEarnedCoinText)
        {
            gameOverEarnedCoinText.text = $"+{earnedCoins}";
        }

        if (gameOverTotalCoinText && TossGameLoginManager.Instance != null)
        {
            int totalCoins = TossGameLoginManager.Instance.totalCoins;
            gameOverTotalCoinText.text = $"{totalCoins}";
        }

        lastFloor = floor;
        lastDistance = distance;
        lastPlayTime = playTime;
        lastCoins = earnedCoins;
    }

    private void OnClickRestart()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<TowerGameManager>();
        if (gm != null)
        {
            gm.ResetGame();
        }
        
        // 로비 대신 게임 준비 상태로 이동
        PrepareGameStart();
    }

    private void OnClickQuitToLobby()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<TowerGameManager>();
        if (gm != null)
        {
            gm.ResetGame();
        }

        ShowLobby();
    }

    // ===== 에러 =====

    public void ShowError(string message)
    {
        if (errorPanel) errorPanel.SetActive(true);
        if (errorText) errorText.text = message;
    }

    private void OnClickErrorConfirm()
    {
        if (errorPanel) errorPanel.SetActive(false);
    }

    // ===== BGM =====

    private void PlayBGM(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying) return;

        audioSource.clip = clip;
        audioSource.volume = bgmVolume;
        audioSource.loop = true;
        audioSource.Play();

        ApplyAudioSettings();  // BGM 설정 적용
    }
}