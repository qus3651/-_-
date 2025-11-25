using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using DG.Tweening;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField userIdInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    public UnityEngine.UI.Button loginButton;
    public UnityEngine.UI.Button signUpButton;

    [Header("SignUp Popup")]
    public GameObject signUpPopup;
    public CanvasGroup signUpCanvasGroup;
    public Transform signUpWindow;
    
    [Header("SignUp Input Fields")]
    public TMP_InputField signUpUserIdInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpPasswordConfirmInput;
    public TMP_InputField signUpNameInput;
    public TMP_InputField signUpWalletInput;
    public TMP_InputField signUpPhoneInput;
    
    [Header("SignUp Buttons & Messages")]
    public UnityEngine.UI.Button checkDuplicateButton;
    public UnityEngine.UI.Button confirmSignUpButton;
    public UnityEngine.UI.Button cancelSignUpButton;
    public TMP_Text signUpMessageText;
    public TMP_Text duplicateCheckText;
    
    private string loginUrl = "https://nodam.world/api/login.php";
    private string signUpUrl = "https://nodam.world/api/signup.php";
    private string checkDuplicateUrl = "https://nodam.world/api/check_duplicate.php";
    
    private bool isUserIdAvailable = false;
    
    void Start()
    {
        SetupLoginInputNavigation();
        SetupButtonListeners();
        
        // 초기 상태 설정
        if (signUpPopup != null)
            signUpPopup.SetActive(false);
        
        if (messageText != null)
            messageText.text = "";
        
        if (signUpMessageText != null)
            signUpMessageText.text = "";
    }
    
    void SetupLoginInputNavigation()
    {
        // ID 입력 필드에서 Tab 키 처리
        if (userIdInput != null)
        {
            userIdInput.onSubmit.AddListener((text) => {
                passwordInput.Select();
                passwordInput.ActivateInputField();
            });
            
            // Tab 키 네비게이션 설정
            var navigation = userIdInput.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            navigation.selectOnDown = passwordInput;
            navigation.selectOnRight = passwordInput;
            userIdInput.navigation = navigation;
        }
        
        // 패스워드 입력 필드에서 Enter 키로 로그인
        if (passwordInput != null)
        {
            passwordInput.onSubmit.AddListener((text) => {
                OnLoginButton();
            });
            
            var navigation = passwordInput.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            navigation.selectOnUp = userIdInput;
            navigation.selectOnLeft = userIdInput;
            passwordInput.navigation = navigation;
        }
    }
    
    void SetupButtonListeners()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginButton);
        
        if (signUpButton != null)
            signUpButton.onClick.AddListener(ShowSignUpPopup);
        
        if (checkDuplicateButton != null)
            checkDuplicateButton.onClick.AddListener(OnCheckDuplicate);
        
        if (confirmSignUpButton != null)
            confirmSignUpButton.onClick.AddListener(OnConfirmSignUp);
        
        if (cancelSignUpButton != null)
            cancelSignUpButton.onClick.AddListener(HideSignUpPopup);
        
        // 회원가입 입력 필드 변경 시 중복 체크 초기화
        if (signUpUserIdInput != null)
        {
            signUpUserIdInput.onValueChanged.AddListener((text) => {
                isUserIdAvailable = false;
                if (duplicateCheckText != null)
                {
                    duplicateCheckText.text = "";
                    duplicateCheckText.color = Color.white;
                }
            });
        }
    }
    
    public void OnLoginButton()
    {
        if (string.IsNullOrEmpty(userIdInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageText.text = "아이디와 비밀번호를 입력해주세요.";
            messageText.color = Color.red;
            return;
        }
        
        StartCoroutine(LoginCoroutine());
    }
    
    private IEnumerator LoginCoroutine()
    {
        messageText.text = "로그인 중...";
        messageText.color = Color.white;
        
        WWWForm form = new WWWForm();
        form.AddField("userid", userIdInput.text);
        form.AddField("password", passwordInput.text);

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                messageText.text = "서버 오류: " + www.error;
                messageText.color = Color.red;
            }
            else
            {
                string json = www.downloadHandler.text;
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);

                if (response.ok)
                {
                    UserData.Instance.SetUserData(response.userid, response.name, response.wallet, response.gamemoney);
                    SceneManager.LoadScene("Choice");
                }
                else
                {
                    messageText.text = "로그인 실패: " + response.error;
                    messageText.color = Color.red;
                }
            }
        }
    }
    
    void ShowSignUpPopup()
    {
        if (signUpPopup == null) return;
        
        // 입력 필드 초기화
        ClearSignUpFields();
        
        // 팝업 표시 애니메이션
        signUpPopup.SetActive(true);
        
        if (signUpCanvasGroup != null)
        {
            signUpCanvasGroup.alpha = 0;
            signUpCanvasGroup.DOFade(1, 0.3f);
        }
        
        if (signUpWindow != null)
        {
            signUpWindow.localScale = Vector3.zero;
            signUpWindow.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        }
        
        // 첫 번째 입력 필드에 포커스
        if (signUpUserIdInput != null)
        {
            signUpUserIdInput.Select();
            signUpUserIdInput.ActivateInputField();
        }
    }
    
    void HideSignUpPopup()
    {
        if (signUpPopup == null) return;
        
        if (signUpCanvasGroup != null)
        {
            signUpCanvasGroup.DOFade(0, 0.3f);
        }
        
        if (signUpWindow != null)
        {
            signUpWindow.DOScale(0, 0.3f).OnComplete(() => {
                signUpPopup.SetActive(false);
            });
        }
        else
        {
            signUpPopup.SetActive(false);
        }
    }
    
    void ClearSignUpFields()
    {
        if (signUpUserIdInput != null) signUpUserIdInput.text = "";
        if (signUpPasswordInput != null) signUpPasswordInput.text = "";
        if (signUpPasswordConfirmInput != null) signUpPasswordConfirmInput.text = "";
        if (signUpNameInput != null) signUpNameInput.text = "";
        if (signUpWalletInput != null) signUpWalletInput.text = "";
        if (signUpPhoneInput != null) signUpPhoneInput.text = "";
        if (signUpMessageText != null) signUpMessageText.text = "";
        if (duplicateCheckText != null) duplicateCheckText.text = "";
        
        isUserIdAvailable = false;
    }
    
    public void OnCheckDuplicate()
    {
        if (string.IsNullOrEmpty(signUpUserIdInput.text))
        {
            duplicateCheckText.text = "아이디를 입력해주세요.";
            duplicateCheckText.color = Color.red;
            return;
        }
        
        if (signUpUserIdInput.text.Length < 4)
        {
            duplicateCheckText.text = "아이디는 4자 이상이어야 합니다.";
            duplicateCheckText.color = Color.red;
            return;
        }
        
        StartCoroutine(CheckDuplicateCoroutine());
    }
    
    private IEnumerator CheckDuplicateCoroutine()
    {
        duplicateCheckText.text = "확인 중...";
        duplicateCheckText.color = Color.white;
        
        WWWForm form = new WWWForm();
        form.AddField("userid", signUpUserIdInput.text);
        
        using (UnityWebRequest www = UnityWebRequest.Post(checkDuplicateUrl, form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                duplicateCheckText.text = "서버 오류";
                duplicateCheckText.color = Color.red;
            }
            else
            {
                DuplicateCheckResponse response = JsonUtility.FromJson<DuplicateCheckResponse>(www.downloadHandler.text);
                
                if (response.available)
                {
                    isUserIdAvailable = true;
                    duplicateCheckText.text = "사용 가능한 아이디입니다.";
                    duplicateCheckText.color = Color.green;
                }
                else
                {
                    isUserIdAvailable = false;
                    duplicateCheckText.text = "이미 사용 중인 아이디입니다.";
                    duplicateCheckText.color = Color.red;
                }
            }
        }
    }
    
    public void OnConfirmSignUp()
    {
        // 유효성 검사
        if (!ValidateSignUpForm())
            return;
        
        StartCoroutine(SignUpCoroutine());
    }
    
    bool ValidateSignUpForm()
    {
        signUpMessageText.color = Color.red;
        
        // 중복 체크 확인
        if (!isUserIdAvailable)
        {
            signUpMessageText.text = "아이디 중복 확인을 해주세요.";
            return false;
        }
        
        // 필수 필드 확인
        if (string.IsNullOrEmpty(signUpUserIdInput.text) || 
            string.IsNullOrEmpty(signUpPasswordInput.text) ||
            string.IsNullOrEmpty(signUpNameInput.text))
        {
            signUpMessageText.text = "필수 항목을 모두 입력해주세요.";
            return false;
        }
        
        // 비밀번호 확인
        if (signUpPasswordInput.text != signUpPasswordConfirmInput.text)
        {
            signUpMessageText.text = "비밀번호가 일치하지 않습니다.";
            return false;
        }
        
        // 비밀번호 길이 체크
        if (signUpPasswordInput.text.Length < 4)
        {
            signUpMessageText.text = "비밀번호는 4자 이상이어야 합니다.";
            return false;
        }
        
        // 지갑 주소 형식 체크 (선택사항)
        if (!string.IsNullOrEmpty(signUpWalletInput.text))
        {
            if (signUpWalletInput.text.Length < 2)
            {
                signUpMessageText.text = "올바른 지갑 주소를 입력해주세요.";
                return false;
            }
        }
        
        // 전화번호 형식 체크 (선택사항)
        if (!string.IsNullOrEmpty(signUpPhoneInput.text))
        {
            string phone = signUpPhoneInput.text.Replace("-", "");
            if (phone.Length < 10)
            {
                signUpMessageText.text = "올바른 전화번호를 입력해주세요.";
                return false;
            }
        }
        
        return true;
    }
    
    private IEnumerator SignUpCoroutine()
    {
        signUpMessageText.text = "회원가입 처리 중...";
        signUpMessageText.color = Color.white;
        
        WWWForm form = new WWWForm();
        form.AddField("userid", signUpUserIdInput.text);
        form.AddField("password", signUpPasswordInput.text);
        form.AddField("name", signUpNameInput.text);
        form.AddField("wallet", signUpWalletInput.text);
        form.AddField("phone", signUpPhoneInput.text);
        
        using (UnityWebRequest www = UnityWebRequest.Post(signUpUrl, form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                signUpMessageText.text = "서버 오류: " + www.error;
                signUpMessageText.color = Color.red;
            }
            else
            {
                SignUpResponse response = JsonUtility.FromJson<SignUpResponse>(www.downloadHandler.text);
                
                if (response.ok)
                {
                    signUpMessageText.text = "회원가입 성공!";
                    signUpMessageText.color = Color.green;
                    
                    // 로그인 화면에 아이디 자동 입력
                    userIdInput.text = signUpUserIdInput.text;
                    passwordInput.text = "";
                    
                    // 1.5초 후 팝업 닫기
                    yield return new WaitForSeconds(1.5f);
                    HideSignUpPopup();
                    
                    messageText.text = "회원가입이 완료되었습니다. 로그인해주세요.";
                    messageText.color = Color.green;
                    
                    // 패스워드 필드에 포커스
                    passwordInput.Select();
                    passwordInput.ActivateInputField();
                }
                else
                {
                    signUpMessageText.text = "회원가입 실패: " + response.error;
                    signUpMessageText.color = Color.red;
                }
            }
        }
    }
    
    // Response 클래스들
    [System.Serializable]
    private class LoginResponse
    {
        public bool ok;
        public string userid;
        public string name;
        public string wallet;
        public int gamemoney;
        public string error;
    }
    
    [System.Serializable]
    private class SignUpResponse
    {
        public bool ok;
        public string error;
    }
    
    [System.Serializable]
    private class DuplicateCheckResponse
    {
        public bool available;
    }
}