using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class TossGameLoginManager : MonoBehaviour
{
    public static TossGameLoginManager Instance { get; private set; }

    [Header("User Info")]
    public string userHash = "";        // 토스에서 받은 해시값
    public string nickname = "";        // 유저 닉네임
    public int userId = 0;              // 서버에서 발급한 user_id
    public int totalCoins = 0;          // 총 코인
    
    [Header("Status")]
    public bool isLoggedIn = false;
    public bool needNickname = false;

    private bool isProcessing = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TossGetUserKeyForGame();

    [DllImport("__Internal")]
    private static extern int IsTossInAppEnvironment();
#endif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartLoginProcess();
    }

    // 로그인 프로세스 시작
    public void StartLoginProcess()
    {
        if (isProcessing) return;
        isProcessing = true;

        Debug.Log("[TossGameLogin] Starting login process...");

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 환경
        if (IsTossInAppEnvironment() == 1)
        {
            Debug.Log("[TossGameLogin] Toss InApp detected - calling getUserKeyForGame");
            TossGetUserKeyForGame();
        }
        else
        {
            Debug.Log("[TossGameLogin] Not in Toss InApp - using test hash");
            OnGetUserKeySuccess(""); // 빈 문자열 = 테스트 모드
        }
#else
        // Unity 에디터
        Debug.Log("[TossGameLogin] Unity Editor - using test hash");
        OnGetUserKeySuccess(""); // 테스트 모드
#endif
    }

    // JavaScript에서 호출 - getUserKeyForGame 성공
    public void OnGetUserKeySuccess(string hash)
    {
        userHash = hash;
        
        if (string.IsNullOrEmpty(userHash))
        {
            Debug.Log("[TossGameLogin] Empty hash - using test mode");
            userHash = "test_hash_0"; // 테스트용 해시
        }

        Debug.Log($"[TossGameLogin] Got user hash: {userHash}");

        // 서버에 해시 전달하여 유저 정보 확인
        StartCoroutine(GameServerAPI.Instance.GameLogin(userHash, OnGameLoginSuccess, OnGameLoginFailed));
    }

    // JavaScript에서 호출 - getUserKeyForGame 실패
    public void OnGetUserKeyFailed(string errorMessage)
    {
        Debug.LogError($"[TossGameLogin] Failed to get user key: {errorMessage}");
        
        // 테스트 모드로 전환
        userHash = "test_hash_0";
        StartCoroutine(GameServerAPI.Instance.GameLogin(userHash, OnGameLoginSuccess, OnGameLoginFailed));
    }

    // 서버 로그인 성공
    private void OnGameLoginSuccess(GameServerAPI.GameLoginResponse response)
    {
        if (response.needNickname)
        {
            // 닉네임 필요
            Debug.Log("[TossGameLogin] Need nickname");
            needNickname = true;
            isProcessing = false;
            
            // UI에 닉네임 입력 화면 표시
            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.ShowNicknameInput();
            }
        }
        else
        {
            // 기존 유저
            userId = response.userId;
            nickname = response.nickname;
            totalCoins = response.totalCoins;
            isLoggedIn = true;
            needNickname = false;
            isProcessing = false;

            Debug.Log($"[TossGameLogin] Login success - UserId: {userId}, Nickname: {nickname}, Coins: {totalCoins}");

            // 로비로 이동
            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.OnLoginComplete(nickname, totalCoins);
            }
        }
    }

    // 서버 로그인 실패
    private void OnGameLoginFailed(string error)
    {
        Debug.LogError($"[TossGameLogin] Server login failed: {error}");
        isProcessing = false;

        // 재시도 또는 에러 메시지 표시
        if (TowerUIManager.Instance != null)
        {
            TowerUIManager.Instance.ShowError("서버 연결에 실패했습니다.");
        }
    }

    // 닉네임 등록
    public void RegisterNickname(string newNickname)
    {
        if (isProcessing) return;
        isProcessing = true;

        Debug.Log($"[TossGameLogin] Registering nickname: {newNickname}");
        StartCoroutine(GameServerAPI.Instance.RegisterNickname(userHash, newNickname, OnNicknameRegisterSuccess, OnNicknameRegisterFailed));
    }

    // 닉네임 등록 성공
    private void OnNicknameRegisterSuccess(GameServerAPI.NicknameRegisterResponse response)
    {
        userId = response.userId;
        nickname = response.nickname;
        totalCoins = 0; // 신규 유저
        isLoggedIn = true;
        needNickname = false;
        isProcessing = false;

        Debug.Log($"[TossGameLogin] Nickname registered - UserId: {userId}, Nickname: {nickname}");

        // 로비로 이동
        if (TowerUIManager.Instance != null)
        {
            TowerUIManager.Instance.OnLoginComplete(nickname, totalCoins);
        }
    }

    // 닉네임 등록 실패
    private void OnNicknameRegisterFailed(string error)
    {
        Debug.LogError($"[TossGameLogin] Nickname registration failed: {error}");
        isProcessing = false;

        if (TowerUIManager.Instance != null)
        {
            TowerUIManager.Instance.ShowError($"닉네임 등록 실패: {error}");
        }
    }

    // 코인 저장 (게임 종료 시)
    public void SaveCoins(int earnedCoins)
    {
        if (!isLoggedIn) return;

        int newTotal = totalCoins + earnedCoins;
        
        Debug.Log($"[TossGameLogin] Saving coins - Earned: {earnedCoins}, New Total: {newTotal}");
        
        StartCoroutine(GameServerAPI.Instance.UpdateCoins(userId, earnedCoins, 
            (response) => {
                totalCoins = response.totalCoins;
                Debug.Log($"[TossGameLogin] Coins updated - Total: {totalCoins}");
            },
            (error) => {
                Debug.LogError($"[TossGameLogin] Failed to update coins: {error}");
            }
        ));
    }

    // 코인 불러오기
    public void LoadUserData()
    {
        if (!isLoggedIn) return;

        StartCoroutine(GameServerAPI.Instance.GetUserData(userId,
            (response) => {
                totalCoins = response.totalCoins;
                Debug.Log($"[TossGameLogin] User data loaded - Coins: {totalCoins}");
            },
            (error) => {
                Debug.LogError($"[TossGameLogin] Failed to load user data: {error}");
            }
        ));
    }
}