using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using DG.Tweening;
using TMPro;

public class OddEvenGame : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI gameMoneyText;
    [SerializeField] private Button exitButton;
    
    [Header("Betting UI")]
    [SerializeField] private GameObject currentBetPanel;
    [SerializeField] private TextMeshProUGUI currentBetText;
    
    [Header("Betting Buttons")]
    [SerializeField] private Button bet1000Button;
    [SerializeField] private Button bet10000Button;
    [SerializeField] private Button bet100000Button;
    [SerializeField] private Button betAllButton;
    [SerializeField] private TextMeshProUGUI bet1000Text;
    [SerializeField] private TextMeshProUGUI bet10000Text;
    [SerializeField] private TextMeshProUGUI bet100000Text;
    [SerializeField] private TextMeshProUGUI betAllText;
    
    [Header("Choice Buttons")]
    [SerializeField] private Button oddButton;
    [SerializeField] private Button evenButton;
    [SerializeField] private TextMeshProUGUI oddButtonText;
    [SerializeField] private TextMeshProUGUI evenButtonText;
    
    [Header("Dice")]
    [SerializeField] private Image diceImage;
    [SerializeField] private Sprite[] diceSprites; // 1~6 주사위 이미지
    [SerializeField] private Transform diceContainer; // 주사위 애니메이션용 컨테이너
    
    [Header("Result Popup")]
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private CanvasGroup resultPopupCanvasGroup;
    [SerializeField] private Transform resultPopupWindow;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI resultAmountText;
    [SerializeField] private Button retryButton;
    [SerializeField] private TextMeshProUGUI retryButtonText;
    
    [Header("Visual Effects")]
    [SerializeField] private Image flashImage; // 화면 플래시 효과용
    [SerializeField] private ParticleSystem winParticles; // 승리 파티클 (선택사항)
    
    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip betSound;
    [SerializeField] private AudioClip choiceSound;
    [SerializeField] private AudioClip diceRollSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    
    [Header("Animation Settings")]
    [SerializeField] private float diceRollDuration = 2f;
    [SerializeField] private float diceRollSpeed = 0.1f;
    
    private int currentBet = 0;
    private bool isOddChoice = false;
    private bool isGameInProgress = false;
    private int currentGameMoney = 0;
    private Sequence currentSequence; // DOTween 시퀀스 관리
    
    void Start()
    {
        InitializeGame();
        SetupButtonListeners();
        SetupButtonTexts();
        DOTween.Init();
    }
    
    void InitializeGame()
    {
        // UserData에서 정보 가져오기
        if (UserData.Instance != null)
        {
            nameText.text = UserData.Instance.Name;
            currentGameMoney = UserData.Instance.GameMoney;
            UpdateGameMoneyUI();
        }
        
        // 초기 상태 설정
        resultPopup.SetActive(false);
        diceImage.gameObject.SetActive(false);
        currentBetPanel.SetActive(false);
        
        // 플래시 이미지 초기화
        if (flashImage != null)
        {
            flashImage.color = new Color(1, 1, 1, 0);
        }
        
        ResetGameState();
    }
    
    void SetupButtonTexts()
    {
        // 버튼 텍스트 초기 설정
        bet1000Text.text = "1,000";
        bet10000Text.text = "10,000";
        bet100000Text.text = "100,000";
        betAllText.text = "전액";
        oddButtonText.text = "홀수";
        evenButtonText.text = "짝수";
        retryButtonText.text = "다시하기";
    }
    
    void SetupButtonListeners()
    {
        // Exit 버튼
        exitButton.onClick.AddListener(() => {
            // 페이드 아웃 효과와 함께 씬 전환
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, 0.3f).OnComplete(() => {
                    SceneManager.LoadScene("Choice");
                });
            }
            else
            {
                SceneManager.LoadScene("Choice");
            }
        });
        
        // 배팅 버튼들
        bet1000Button.onClick.AddListener(() => AddBet(1000));
        bet10000Button.onClick.AddListener(() => AddBet(10000));
        bet100000Button.onClick.AddListener(() => AddBet(100000));
        betAllButton.onClick.AddListener(() => BetAll());
        
        // 홀짝 선택 버튼
        oddButton.onClick.AddListener(() => MakeChoice(true));
        evenButton.onClick.AddListener(() => MakeChoice(false));
        
        // 다시하기 버튼 - Inspector에서 연결할 수 있도록 리스너 추가하지 않음
        // retryButton.onClick.AddListener는 제거됨
    }
    
    // Public 메서드로 Inspector에서 연결 가능
    public void OnRetryButtonClick()
    {
        Debug.Log("Retry 버튼 클릭 - 씬 재시작");
        SceneManager.LoadScene("OddEven");
    }
    
    void AddBet(int amount)
    {
        if (isGameInProgress) return;
        
        // 잔액 체크
        if (amount > currentGameMoney)
        {
            // 잔액 부족 시 GameMoneyText 흔들기
            gameMoneyText.transform.DOShakePosition(0.5f, 10, 20, 90, false, true);
            Debug.Log("잔액이 부족합니다!");
            return;
        }
        
        if (amount <= 0)
        {
            Debug.Log("배팅 금액이 유효하지 않습니다!");
            return;
        }
        
        // 누적 배팅
        currentBet += amount;
        int previousMoney = currentGameMoney;
        currentGameMoney -= amount;
        
        // 배팅 금액 UI 표시 및 애니메이션
        if (!currentBetPanel.activeSelf)
        {
            currentBetPanel.SetActive(true);
            currentBetPanel.transform.DOScale(0, 0);
            currentBetPanel.transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack);
        }
        
        currentBetText.text = $"배팅: {currentBet:N0} NODAM";
        
        // 배팅 금액 텍스트 펀치 효과
        currentBetText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1);
        
        // TextMeshPro 전용 카운터 애니메이션
        DOTween.To(() => previousMoney, x => {
            gameMoneyText.text = $"{x:N0} NODAM";
        }, currentGameMoney, 0.5f).SetEase(Ease.OutQuad);
        
        // 사운드 재생
        PlaySound(betSound);
        
        // 배팅 버튼 상태 업데이트 (잔액에 따라)
        UpdateBettingButtonsState();
        
        // 홀짝 버튼 활성화
        SetChoiceButtonsInteractable(true);
        
        // 홀짝 버튼 강조 (첫 배팅시에만)
        if (currentBet == amount)
        {
            AnimateChoiceButtons();
        }
        
        Debug.Log($"배팅 추가: {amount}NODAM, 총 배팅: {currentBet}NODAM");
    }
    
    void BetAll()
    {
        if (isGameInProgress || currentGameMoney <= 0) return;
        
        // 전액 배팅
        int allAmount = currentGameMoney;
        currentBet += allAmount;
        currentGameMoney = 0;
        
        // 배팅 금액 UI 표시 및 애니메이션
        if (!currentBetPanel.activeSelf)
        {
            currentBetPanel.SetActive(true);
            currentBetPanel.transform.DOScale(0, 0);
            currentBetPanel.transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack);
        }
        
        currentBetText.text = $"배팅: {currentBet:N0}NODAM";
        currentBetText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 1);
        
        // 게임머니 UI 업데이트
        gameMoneyText.text = "0 NODAM";
        
        // 사운드 재생
        PlaySound(betSound);
        
        // 배팅 버튼 비활성화 (잔액 0)
        SetBettingButtonsInteractable(false);
        
        // 홀짝 버튼 활성화
        SetChoiceButtonsInteractable(true);
        
        // 홀짝 버튼 강조
        if (currentBet == allAmount)
        {
            AnimateChoiceButtons();
        }
        
        Debug.Log($"전액 배팅: {allAmount}NODAM");
    }
    
    void UpdateBettingButtonsState()
    {
        // 잔액에 따라 버튼 활성화/비활성화
        bet1000Button.interactable = currentGameMoney >= 1000;
        bet10000Button.interactable = currentGameMoney >= 10000;
        bet100000Button.interactable = currentGameMoney >= 100000;
        betAllButton.interactable = currentGameMoney > 0;
    }
    
    void AnimateChoiceButtons()
    {
        // 홀짝 버튼 텍스트 강조
        oddButtonText.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        evenButtonText.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        
        // 버튼 자체도 애니메이션
        oddButton.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        evenButton.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
    
    void StopChoiceButtonAnimations()
    {
        oddButtonText.transform.DOKill();
        evenButtonText.transform.DOKill();
        oddButton.transform.DOKill();
        evenButton.transform.DOKill();
        
        oddButtonText.transform.localScale = Vector3.one;
        evenButtonText.transform.localScale = Vector3.one;
        oddButton.transform.localScale = Vector3.one;
        evenButton.transform.localScale = Vector3.one;
    }
    
    void MakeChoice(bool isOdd)
    {
        if (isGameInProgress || currentBet == 0) return;
        
        isGameInProgress = true;
        isOddChoice = isOdd;
        
        // 애니메이션 중지
        StopChoiceButtonAnimations();
        
        // 선택한 버튼 애니메이션
        Button selectedButton = isOdd ? oddButton : evenButton;
        selectedButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);
        
        // 사운드 재생
        PlaySound(choiceSound);
        
        // 모든 버튼 비활성화 (배팅 버튼과 홀짝 버튼 모두)
        SetBettingButtonsInteractable(false);
        SetChoiceButtonsInteractable(false);
        
        // 주사위 굴리기 시작
        StartCoroutine(RollDice());
    }
    
    IEnumerator RollDice()
    {
        diceImage.gameObject.SetActive(true);
        
        // 주사위 등장 애니메이션 (DOTween Pro의 더 부드러운 효과)
        diceContainer.localScale = Vector3.zero;
        diceContainer.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        
        // 주사위 굴리는 사운드
        PlaySound(diceRollSound);
        
        // 주사위 회전 애니메이션 시작 (더 복잡한 회전)
        currentSequence = DOTween.Sequence();
        currentSequence.Append(diceContainer.DORotate(new Vector3(360, 360, 360), 0.5f, RotateMode.FastBeyond360))
                      .SetLoops(-1, LoopType.Restart)
                      .SetEase(Ease.Linear);
        
        // 주사위 위치 살짝 흔들기
        diceContainer.DOShakePosition(diceRollDuration, 20, 10, 90, false, true);
        
        // 주사위 굴리는 애니메이션
        float elapsedTime = 0f;
        int lastDiceValue = 0;
        
        while (elapsedTime < diceRollDuration)
        {
            int randomDice = Random.Range(0, 6);
            if (randomDice != lastDiceValue)
            {
                diceImage.sprite = diceSprites[randomDice];
                lastDiceValue = randomDice;
                
                // 주사위 변경 시 효과
                diceImage.transform.DOPunchScale(Vector3.one * 0.1f, diceRollSpeed/2, 5, 1);
            }
            
            elapsedTime += diceRollSpeed;
            yield return new WaitForSeconds(diceRollSpeed);
        }
        
        // 회전 애니메이션 중지
        currentSequence.Kill();
        
        // 최종 결과 결정 (유저 승률 40%)
        int finalDiceValue = DetermineResult();
        diceImage.sprite = diceSprites[finalDiceValue - 1];
        
        // 최종 결과 표시 애니메이션
        Sequence finalSequence = DOTween.Sequence();
        finalSequence.Append(diceContainer.DORotate(Vector3.zero, 0.3f))
                     .Join(diceContainer.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 1));
        
        // 잠시 대기
        yield return new WaitForSeconds(1f);
        
        // 결과 처리
        ProcessResult(finalDiceValue);
    }
    
    int DetermineResult()
    {
        float randomValue = Random.Range(0f, 1f);
        bool userWins = randomValue < 0.4f; // 40% 승률
        
        List<int> oddNumbers = new List<int> { 1, 3, 5 };
        List<int> evenNumbers = new List<int> { 2, 4, 6 };
        
        if (userWins)
        {
            // 유저가 선택한 것과 같은 결과
            if (isOddChoice)
                return oddNumbers[Random.Range(0, oddNumbers.Count)];
            else
                return evenNumbers[Random.Range(0, evenNumbers.Count)];
        }
        else
        {
            // 유저가 선택한 것과 반대 결과 (60% 확률)
            if (isOddChoice)
                return evenNumbers[Random.Range(0, evenNumbers.Count)];
            else
                return oddNumbers[Random.Range(0, oddNumbers.Count)];
        }
    }
    
    void ProcessResult(int diceValue)
    {
        bool isOddResult = (diceValue % 2 == 1);
        bool userWon = (isOddChoice == isOddResult);
        
        if (userWon)
        {
            // 승리: 배팅금액의 2배 획득
            int winAmount = currentBet * 2;
            int previousMoney = currentGameMoney;
            currentGameMoney += winAmount;
            
            resultText.text = "승리!";
            resultAmountText.text = $"+{winAmount:N0} NODAM";
            PlaySound(winSound);
            
            // TextMeshPro 카운터 애니메이션
            DOTween.To(() => previousMoney, x => {
                gameMoneyText.text = $"{x:N0} NODAM";
            }, currentGameMoney, 1f).SetEase(Ease.OutQuad);
            
            // 승리 시 화면 플래시 효과
            if (flashImage != null)
            {
                flashImage.color = new Color(1, 1, 0, 0);
                Sequence flashSequence = DOTween.Sequence();
                flashSequence.Append(flashImage.DOFade(0.3f, 0.1f))
                            .Append(flashImage.DOFade(0, 0.2f))
                            .SetLoops(2);
            }
            
            // 파티클 효과 (있다면)
            if (winParticles != null)
            {
                winParticles.Play();
            }
            
            // 서버에 게임머니 업데이트
            StartCoroutine(UpdateGameMoneyOnServer(currentBet)); // 순수 이익만 서버에 추가
        }
        else
        {
            // 패배: 이미 차감된 상태 유지
            resultText.text = "패배!";
            resultAmountText.text = $"-{currentBet:N0} NODAM";
            PlaySound(loseSound);
            
            // 패배 시 화면 흔들기
            Camera.main.transform.DOShakePosition(0.5f, 10, 20, 90, false, true);
            
            // 서버에 게임머니 업데이트 (차감)
            StartCoroutine(UpdateGameMoneyOnServer(-currentBet));
        }
        
        // UserData 업데이트
        if (UserData.Instance != null)
        {
            UserData.Instance.UpdateGameMoney(currentGameMoney);
        }
        
        // 결과 팝업 표시 애니메이션
        ShowResultPopup();
    }
    
    void ShowResultPopup()
    {
        resultPopup.SetActive(true);
        resultPopupCanvasGroup.alpha = 0;
        resultPopupWindow.localScale = Vector3.zero;
        
        // 페이드 인
        resultPopupCanvasGroup.DOFade(1, 0.3f);
        
        // 팝업 윈도우 애니메이션
        resultPopupWindow.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        
        // 결과 텍스트 애니메이션
        resultText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        
        // 결과 금액 텍스트 펀치 효과
        resultAmountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1);
    }
    
    IEnumerator UpdateGameMoneyOnServer(int amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", UserData.Instance.UserId);
        form.AddField("amount", amount);
        
        using (UnityWebRequest www = UnityWebRequest.Post("http://your-server.com/gamemoney_add.php", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("서버 업데이트 실패: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("서버 응답: " + response);
            }
        }
    }
    
    void ResetGameState()
    {
        currentBet = 0;
        isGameInProgress = false;
        
        // 애니메이션 중지
        StopChoiceButtonAnimations();
        
        // 결과 텍스트 애니메이션 중지
        if (resultText != null)
        {
            resultText.transform.DOKill();
            resultText.transform.localScale = Vector3.one;
        }
        
        // 결과 팝업 숨기기 애니메이션
        if (resultPopup.activeSelf)
        {
            resultPopupCanvasGroup.DOFade(0, 0.3f);
            resultPopupWindow.DOScale(0, 0.3f).OnComplete(() => {
                resultPopup.SetActive(false);
            });
        }
        
        // 주사위 숨기기 애니메이션
        if (diceImage.gameObject.activeSelf)
        {
            diceContainer.DOScale(0, 0.3f).OnComplete(() => {
                diceImage.gameObject.SetActive(false);
            });
        }
        
        // 배팅 금액 UI 숨기기
        if (currentBetPanel.activeSelf)
        {
            currentBetPanel.transform.DOScale(0, 0.3f).OnComplete(() => {
                currentBetPanel.SetActive(false);
            });
        }
        
        // 버튼 상태 리셋
        SetBettingButtonsInteractable(true);
        SetChoiceButtonsInteractable(false);
        
        // 배팅 버튼 등장 애니메이션
        AnimateBettingButtons();
    }
    
    void AnimateBettingButtons()
    {
        float delay = 0.1f;
        
        // 버튼 초기화
        bet1000Button.transform.localScale = Vector3.zero;
        bet10000Button.transform.localScale = Vector3.zero;
        bet100000Button.transform.localScale = Vector3.zero;
        betAllButton.transform.localScale = Vector3.zero;
        
        // 순차적 등장 애니메이션
        Sequence buttonSequence = DOTween.Sequence();
        buttonSequence.Append(bet1000Button.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack))
                      .Insert(delay * 1, bet10000Button.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack))
                      .Insert(delay * 2, bet100000Button.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack))
                      .Insert(delay * 3, betAllButton.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        
        // 텍스트 페이드 인
        bet1000Text.DOFade(0, 0);
        bet10000Text.DOFade(0, 0);
        bet100000Text.DOFade(0, 0);
        betAllText.DOFade(0, 0);
        
        bet1000Text.DOFade(1, 0.5f).SetDelay(delay * 0);
        bet10000Text.DOFade(1, 0.5f).SetDelay(delay * 1);
        bet100000Text.DOFade(1, 0.5f).SetDelay(delay * 2);
        betAllText.DOFade(1, 0.5f).SetDelay(delay * 3);
    }
    
    void UpdateGameMoneyUI()
    {
        gameMoneyText.text = $"{currentGameMoney:N0} NODAM";
    }
    
    void SetBettingButtonsInteractable(bool interactable)
    {
        bet1000Button.interactable = interactable && currentGameMoney >= 1000;
        bet10000Button.interactable = interactable && currentGameMoney >= 10000;
        bet100000Button.interactable = interactable && currentGameMoney >= 100000;
        betAllButton.interactable = interactable && currentGameMoney > 0;
    }
    
    void SetChoiceButtonsInteractable(bool interactable)
    {
        oddButton.interactable = interactable;
        evenButton.interactable = interactable;
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void OnDestroy()
    {
        // 이 GameObject의 DOTween 애니메이션만 정리 (전체 Kill 하지 않음)
        transform.DOKill();
        if (gameMoneyText != null) gameMoneyText.transform.DOKill();
        if (currentBetText != null) currentBetText.transform.DOKill();
        if (diceContainer != null) diceContainer.DOKill();
        if (resultPopupWindow != null) resultPopupWindow.DOKill();
        if (resultPopupCanvasGroup != null) resultPopupCanvasGroup.DOKill();
        
        // 시퀀스 정리
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        
        // 버튼 리스너 제거
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        if (bet1000Button != null) bet1000Button.onClick.RemoveAllListeners();
        if (bet10000Button != null) bet10000Button.onClick.RemoveAllListeners();
        if (bet100000Button != null) bet100000Button.onClick.RemoveAllListeners();
        if (betAllButton != null) betAllButton.onClick.RemoveAllListeners();
        if (oddButton != null) oddButton.onClick.RemoveAllListeners();
        if (evenButton != null) evenButton.onClick.RemoveAllListeners();
        // retryButton은 Inspector에서 연결하므로 제거하지 않음
    }
}