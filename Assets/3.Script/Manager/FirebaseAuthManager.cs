using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using System.Threading.Tasks;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager instance = null;
    public FirebaseAuth auth;  //로그인 회원가입 등에 사용
    public FirebaseUser user; //인증이 완료된 유저 정보

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TextMeshProUGUI stateText;
    public Toggle remeberMeToggle;

    public Button loginButton;
    public Button loginStateButton;
    public Button registerButton;
    public Button logoutButton;
    public Sprite loggedInSprite;
    private Sprite originalSprite;
    public GameObject loginUIPanel;

   private bool isFirebaseInitialized = false;
   private bool isLoggedIn = false;

    void Awake()
    {
        if(instance==null)
        {
            instance=this;
            DontDestroyOnLoad(gameObject);

            if(loginStateButton !=null)
            {
                originalSprite = loginStateButton.GetComponent<Image>().sprite;
            }

        // Firebase 초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                // Firebase 초기화가 완료되면 auth 객체 사용 가능
              //  FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                //  // Firebase 초기화 완료 표시
                isFirebaseInitialized = true;

                // Firebase가 초기화된 후 현재 사용자 체크는 메인 스레드에서 실행되어야 함
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // Firebase가 초기화된 후 현재 사용자 체크
                    CheckCurrentUser();
                });
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        LoadRememberEmail();
    }

    public bool IsInitialized()
    {
        return isFirebaseInitialized;
    }

    private void OnEnable()
    {
        if (isFirebaseInitialized)
        {
            CheckCurrentUser();
        }
        else
        {
            Debug.Log("Firebase가 아직 초기화되지 않았습니다. 인증 상태를 확인할 수 없습니다.");
        }
    }

    public void CheckCurrentUser()
    {
        if (!isFirebaseInitialized)
        {
            Debug.Log("Firebase가 아직 초기화되지 않았습니다. 인증 상태를 확인할 수 없습니다.");
            return;
        }

        if (auth==null)
        {
            Debug.Log("파이어베이스인증 초기화되지않음");
            return;
        }

        Debug.Log("상태를 표시할게요");
        user = auth.CurrentUser;
        if(user !=null)
        {
            if (!isLoggedIn)
            {
                isLoggedIn = true;
                UpdateLoggedInUI();
            }
            else
            {
                SetUIForLoggedIn();
            }

            }
        else
        {
            if (isLoggedIn)
            {
                isLoggedIn = false;
                UpdateLoggedOutUI();
            }
            else
            {
                SetUIForLoggedOut();
            }
            }
    }

    void UpdateLoggedInUI()
    {
            stateText.text = "Logged in as: " + user.Email;
            SetUIForLoggedIn();

    }

    void UpdateLoggedOutUI()
    {
            stateText.text = "Please log in.";
            SetUIForLoggedOut();

    }

    public async void Create()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if(string.IsNullOrEmpty(email)||!email.Contains("@"))
        {
            stateText.text = "Invalid email format.";
            return;
        }
        if(string.IsNullOrEmpty(password)||password.Length<6)
        {
            stateText.text = "Password must be at least 6 characters long.";
            return;
        }

        try
        {
            // Firebase의 사용자 생성 요청을 비동기로 보냅니다.
            var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            // 비동기 작업이 완료된 후 FirebaseUser를 가져옵니다.
            FirebaseUser newUser = authResult.User;
            stateText.text= "Registration successful: " + newUser.Email;
            RememberEmail();
            isLoggedIn = true;
            SetUIForLoggedIn();
        }
        catch(FirebaseException e)
        {
            stateText.text= "Failed: " + e.Message;
        }

    }
    
    public async void LogIn()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        // 이메일 및 비밀번호 유효성 검사
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            stateText.text = "Invalid email format.";
            return;
        }
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            stateText.text = "Password must be at least 6 characters long.";
            return;
        }

        try
        {
            var authResult =await auth.SignInWithEmailAndPasswordAsync(email,password);
            FirebaseUser newUser = authResult.User;
            stateText.text= "Login successful: " + newUser.Email;
            RememberEmail();
            isLoggedIn = true;
            SetUIForLoggedIn();
        }
        catch (FirebaseException e)
        {
            Debug.LogError("Login failed: " + e.Message);
            stateText.text= "Login failed" + e.Message;
        }
    }

    public void LogOut()
    {
        auth.SignOut();
        stateText.text= "You have logged out.";
        isLoggedIn = false;
        SetUIForLoggedOut();
    }

    void SetUIForLoggedIn()
    {
        loginButton.gameObject.SetActive(false);  // 로그인 버튼 감춤
        logoutButton.gameObject.SetActive(true);  // 로그아웃 버튼 보임

        // 로그인 상태 버튼 스프라이트 변경
        if (loginStateButton != null && loggedInSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = loggedInSprite;
        }
        }

    void SetUIForLoggedOut()
    {
        loginButton.gameObject.SetActive(true);   // 로그인 버튼 보임
        logoutButton.gameObject.SetActive(false); // 로그아웃 버튼 감춤

        if (loginStateButton != null && originalSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = originalSprite;
        }
    }

    void RememberEmail()
    {
        if(remeberMeToggle.isOn)
        {
            PlayerPrefs.SetString("rememberedEmail", emailInputField.text);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.DeleteKey("rememberedEmail");
        }
    }

    void LoadRememberEmail()
    {
        if(PlayerPrefs.HasKey("rememberedEmail"))
        {
            emailInputField.text = PlayerPrefs.GetString("rememberedEmail");
            remeberMeToggle.isOn = true;
        }
        else
        {
            emailInputField.text = "";
            passwordInputField.text = "";
            remeberMeToggle.isOn = false;
        }
    }
}
