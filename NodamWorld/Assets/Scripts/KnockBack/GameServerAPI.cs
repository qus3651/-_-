using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class GameServerAPI : MonoBehaviour
{
    public static GameServerAPI Instance { get; private set; }

    [Header("Server Settings")]
    public string serverURL = "https://knockbackgames.com/api"; // PHP API 경로

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

    // 게임 로그인
    public IEnumerator GameLogin(string hash, Action<GameLoginResponse> onSuccess, Action<string> onFailed)
    {
        WWWForm form = new WWWForm();
        form.AddField("hash", hash);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverURL}/game_login.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = www.downloadHandler.text;
                    Debug.Log($"[API] GameLogin response: {json}");
                    
                    GameLoginResponse response = JsonUtility.FromJson<GameLoginResponse>(json);
                    
                    if (response.success)
                    {
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onFailed?.Invoke(response.message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[API] Parse error: {e.Message}");
                    onFailed?.Invoke("응답 파싱 실패");
                }
            }
            else
            {
                Debug.LogError($"[API] Request error: {www.error}");
                onFailed?.Invoke(www.error);
            }
        }
    }

    // 닉네임 등록
    public IEnumerator RegisterNickname(string hash, string nickname, Action<NicknameRegisterResponse> onSuccess, Action<string> onFailed)
    {
        WWWForm form = new WWWForm();
        form.AddField("hash", hash);
        form.AddField("nickname", nickname);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverURL}/register_nickname.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = www.downloadHandler.text;
                    Debug.Log($"[API] RegisterNickname response: {json}");
                    
                    NicknameRegisterResponse response = JsonUtility.FromJson<NicknameRegisterResponse>(json);
                    
                    if (response.success)
                    {
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onFailed?.Invoke(response.message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[API] Parse error: {e.Message}");
                    onFailed?.Invoke("응답 파싱 실패");
                }
            }
            else
            {
                Debug.LogError($"[API] Request error: {www.error}");
                onFailed?.Invoke(www.error);
            }
        }
    }

    // 코인 업데이트
    public IEnumerator UpdateCoins(int userId, int earnedCoins, Action<UpdateCoinsResponse> onSuccess, Action<string> onFailed)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("earned_coins", earnedCoins);

        using (UnityWebRequest www = UnityWebRequest.Post($"{serverURL}/update_coins.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = www.downloadHandler.text;
                    Debug.Log($"[API] UpdateCoins response: {json}");
                    
                    UpdateCoinsResponse response = JsonUtility.FromJson<UpdateCoinsResponse>(json);
                    
                    if (response.success)
                    {
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onFailed?.Invoke(response.message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[API] Parse error: {e.Message}");
                    onFailed?.Invoke("응답 파싱 실패");
                }
            }
            else
            {
                Debug.LogError($"[API] Request error: {www.error}");
                onFailed?.Invoke(www.error);
            }
        }
    }

    // 유저 데이터 가져오기
    public IEnumerator GetUserData(int userId, Action<GetUserDataResponse> onSuccess, Action<string> onFailed)
    {
        string url = $"{serverURL}/get_user_data.php?user_id={userId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = www.downloadHandler.text;
                    Debug.Log($"[API] GetUserData response: {json}");
                    
                    GetUserDataResponse response = JsonUtility.FromJson<GetUserDataResponse>(json);
                    
                    if (response.success)
                    {
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onFailed?.Invoke(response.message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[API] Parse error: {e.Message}");
                    onFailed?.Invoke("응답 파싱 실패");
                }
            }
            else
            {
                Debug.LogError($"[API] Request error: {www.error}");
                onFailed?.Invoke(www.error);
            }
        }
    }

    // ===== Response 클래스 =====

    [Serializable]
    public class GameLoginResponse
    {
        public bool success;
        public string message;
        public bool needNickname;
        public int userId;
        public string nickname;
        public int totalCoins;
    }

    [Serializable]
    public class NicknameRegisterResponse
    {
        public bool success;
        public string message;
        public int userId;
        public string nickname;
    }

    [Serializable]
    public class UpdateCoinsResponse
    {
        public bool success;
        public string message;
        public int totalCoins;
    }

    [Serializable]
    public class GetUserDataResponse
    {
        public bool success;
        public string message;
        public string nickname;
        public int totalCoins;
    }
}