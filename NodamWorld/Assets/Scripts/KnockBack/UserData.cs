using UnityEngine;

public class UserData : MonoBehaviour
{
    public static UserData Instance { get; private set; }

    public string UserId { get; private set; }
    public string Name { get; private set; }
    public string Wallet { get; private set; }
    public int GameMoney { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUserData(string userId, string name, string wallet, int gameMoney)
    {
        UserId = userId;
        Name = name;
        Wallet = wallet;
        GameMoney = gameMoney;
    }

    // ★ 게임머니 업데이트 메서드
    public void UpdateGameMoney(int newAmount)
    {
        GameMoney = Mathf.Max(0, newAmount); // 음수 방지
        Debug.Log($"[UserData] GameMoney updated: {GameMoney}");
    }
}
