using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class CollectAndSpawnController : MonoBehaviour
{
    [Header("프리팹/스폰 설정")]
    public GameObject cigarettePrefab;
    public Transform spawnParent;

    [Header("플레이어/줍기 설정")]
    public Transform player;
    public float pickupRadius = 2.0f;

    [Header("UI 참조")]
    public TMP_Text nameText;      
    public TMP_Text countText;     
    public Button pickupButton;    
    public Button collectButton;   

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public Button donateButton;    
    public Button sendButton;      
    public Button gameMoneyButton; 
    public TMP_Text submitStatusText;

    [Header("서버 연동")]
    public string settlementUrl = "https://nodam.world/api/settlement_add.php";
    public string gamemoneyUrl = "https://nodam.world/api/gamemoney_add.php";

    private string mainSceneName = "Main";
    private float sceneLoadDelaySeconds = 3f;
    private float spawnMin = -45f;
    private float spawnMax = 45f;
    private int spawnCount = 50;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();
    private int collected = 0;
    private bool isSubmitting = false;

    private string userid;
    private string userName;  
    private string wallet;
    private int gamemoney;

    [System.Serializable]
    private class ServerResponse
    {
        public bool ok;
        public string error;
        public int gamemoney; // 최신 게임머니 값
    }

    void Start()
    {
        if (cigarettePrefab == null || player == null)
        {
            Debug.LogError("[CollectAndSpawnController] 기본 설정이 빠져있습니다.");
            enabled = false;
            return;
        }

        if (spawnParent == null)
        {
            var parentGO = new GameObject("Cigarettes");
            spawnParent = parentGO.transform;
        }

        SpawnItems();
        UpdateCountUI();

        if (popupPanel != null) popupPanel.SetActive(false);
        if (submitStatusText != null) submitStatusText.text = "";

        if (pickupButton != null)
        {
            pickupButton.onClick.RemoveAllListeners();
            pickupButton.onClick.AddListener(OnPickUpButton);
        }

        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(ShowPopup);
        }

        if (donateButton != null)
        {
            donateButton.onClick.RemoveAllListeners();
            donateButton.onClick.AddListener(OnDonate);
        }

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendSettlement);
        }

        if (gameMoneyButton != null)
        {
            gameMoneyButton.onClick.RemoveAllListeners();
            gameMoneyButton.onClick.AddListener(OnGameMoney);
        }
        
        if (UserData.Instance != null)
        {
            userid = UserData.Instance.UserId;
            userName = UserData.Instance.Name;
            wallet = UserData.Instance.Wallet;
            gamemoney = UserData.Instance.GameMoney;

            if (nameText != null)
                nameText.text = "이름: " + userName;
        }
    }

    private void SpawnItems()
    {
        spawnedItems.Clear();
        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(spawnMin, spawnMax);
            float z = Random.Range(spawnMin, spawnMax);
            Vector3 pos = new Vector3(x, 0f, z);

            GameObject item = Instantiate(cigarettePrefab, pos, Quaternion.identity, spawnParent);
            spawnedItems.Add(item);
        }
    }

    private void OnPickUpButton()
    {
        if (spawnedItems.Count == 0) return;

        GameObject nearest = FindNearestWithinRadius(player.position, pickupRadius);
        if (nearest == null) return;

        spawnedItems.Remove(nearest);
        Destroy(nearest);

        collected++;
        UpdateCountUI();
    }

    private GameObject FindNearestWithinRadius(Vector3 center, float radius)
    {
        GameObject nearest = null;
        float bestSqr = radius * radius;

        foreach (var item in spawnedItems)
        {
            if (item == null) continue;
            float sqr = (item.transform.position - center).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                nearest = item;
            }
        }
        return nearest;
    }

    private void UpdateCountUI()
    {
        if (countText != null)
            countText.text = "x " + collected.ToString();
    }

    private void ShowPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (submitStatusText != null)
            submitStatusText.text = "수거한 담배꽁초 처리 방식을 선택하세요.";
    }

    private void OnDonate()
    {
        if (isSubmitting) return;

        if (submitStatusText != null) submitStatusText.text = "기부 완료";
        if (popupPanel != null) popupPanel.SetActive(false);

        StartCoroutine(LoadMainAfterDelay());
    }

    private void OnSendSettlement()
    {
        if (isSubmitting) return;

        if (submitStatusText != null) submitStatusText.text = "정산 요청 전송 중...";
        if (popupPanel != null) popupPanel.SetActive(false);

        sendButton.interactable = false;
        gameMoneyButton.interactable = false;

        StartCoroutine(SendSettlementToServer());
    }

    private void OnGameMoney()
    {
        if (isSubmitting) return;

        if (submitStatusText != null) submitStatusText.text = "게임머니 요청 전송 중...";
        if (popupPanel != null) popupPanel.SetActive(false);

        sendButton.interactable = false;
        gameMoneyButton.interactable = false;

        StartCoroutine(AddGameMoneyToServer());
    }

    private IEnumerator SendSettlementToServer()
    {
        isSubmitting = true;

        WWWForm form = new WWWForm();
        form.AddField("userid", userid);
        form.AddField("name", userName);
        form.AddField("wallet", wallet);
        form.AddField("amount", collected);

        using (UnityWebRequest uwr = UnityWebRequest.Post(settlementUrl, form))
        {
            yield return uwr.SendWebRequest();
        }

        isSubmitting = false;
        yield return new WaitForSeconds(sceneLoadDelaySeconds);
        SceneManager.LoadScene(mainSceneName);
    }

    private IEnumerator AddGameMoneyToServer()
    {
        isSubmitting = true;

        WWWForm form = new WWWForm();
        form.AddField("userid", userid);
        form.AddField("amount", collected);

        using (UnityWebRequest uwr = UnityWebRequest.Post(gamemoneyUrl, form))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                ServerResponse resp = JsonUtility.FromJson<ServerResponse>(uwr.downloadHandler.text);
                if (resp != null && resp.ok)
                {
                    if (submitStatusText != null) submitStatusText.text = "게임머니 적립 성공";

                    // ★ UserData 동기화 (메서드 호출)
                    UserData.Instance.UpdateGameMoney(resp.gamemoney);
                }
            }
        }

        isSubmitting = false;
        yield return new WaitForSeconds(sceneLoadDelaySeconds);
        SceneManager.LoadScene(mainSceneName);
    }

    private IEnumerator LoadMainAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelaySeconds);
        SceneManager.LoadScene(mainSceneName);
    }
}
