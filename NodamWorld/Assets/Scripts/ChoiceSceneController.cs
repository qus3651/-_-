using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChoiceSceneController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text gameMoneyText;

    private void Start()
    {
        if (UserData.Instance != null)
        {
            nameText.text = UserData.Instance.Name;

            // 3자리마다 , 넣기 ("N0" 포맷)
            gameMoneyText.text = UserData.Instance.GameMoney.ToString("N0");
        }
        else
        {
            nameText.text = "로그인 정보 없음";
            gameMoneyText.text = "";
        }
    }

    // 게임 버튼들 → 씬 이동 함수
    public void OnCigaretteGame()
    {
        SceneManager.LoadScene("Cigarette");
    }

    public void OnTowerGame()
    {
        SceneManager.LoadScene("Tower");
    }

    public void OnOtherGame2()
    {
        SceneManager.LoadScene("OddEven"); // 실제 씬 이름으로 교체
    }
}
