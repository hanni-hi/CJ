using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using System.Threading.Tasks;
using TMPro;

/*
 파이어베이스 인증 / 사용자 로그인 / 로그아웃 / 회원가입 / UI 업데이트 / 사용자 상태 관리
 
 지금 현재 이 스크립트에서 파이어베이스 초기화가 제대로 안되서 문제가 일어나는 중임
 */


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
    private bool panelwasActive = false;

    void Awake() //파이어베이스 초기화 
    {
        if(instance==null)
        {
            instance=this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseAuthManager instance 설정 완료");
            InitializeFirebase();

            if (loginStateButton !=null)
            {
                originalSprite = loginStateButton.GetComponent<Image>().sprite;
            }

        // Firebase 초기화
       // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
       // {
       //     if (task.IsCompleted && task.Result == DependencyStatus.Available)
       //     {
       //         // Firebase가 초기화된 후 현재 사용자 체크는 메인 스레드에서 실행되어야 함
       //         UnityMainThreadDispatcher.Instance().Enqueue(() =>
       //         {
       //
       //         Debug.Log("초기화 성공★★★: " + task.Result);
       //
       //         // Firebase 초기화가 완료되면 auth 객체 사용 가능
       //         FirebaseApp app = FirebaseApp.DefaultInstance;
       //         
       //             auth = FirebaseAuth.DefaultInstance;
       //
       //
       //             if (FirebaseAuthManager.instance.auth != null)
       //             {
       //                 Debug.Log("Auth 초기화 성공");
       //             }
       //             else
       //             {
       //                 Debug.LogError("Auth 초기화 실패");
       //             }
       //
       //
       //
       //
       //             //  // Firebase 초기화 완료 표시
       //             isFirebaseInitialized = true;
       //
       //             // Firebase가 초기화된 후 현재 사용자 체크
       //             CheckCurrentUser();
       //         });
       //     }
       //     else
       //     {
       //         Debug.LogError("초기화 실패함★★★: " + task.Result);
       //     }
       // });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                isFirebaseInitialized = true;

            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });

        LoadRememberEmail();
    }

    private void Update()
    {
        if(loginUIPanel.activeSelf&&!panelwasActive)
        {
            panelwasActive = true;
            if(IsInitialized())
            {
                CheckCurrentUser();
            }
            else
            {
                Debug.Log("Update : Firebase가 아직 초기화되지 않았습니다.");
            }
        }

        if(!loginUIPanel.activeSelf&&panelwasActive)
        {
            panelwasActive = false;
        }

    }

    public bool IsInitialized()
    {
        return isFirebaseInitialized;
    }

   // private void OnEnable()
   // {
   //     if (FirebaseAuthManager.instance != null && FirebaseAuthManager.instance.IsInitialized())
   //     {
   //         CheckCurrentUser();
   //     }
   //     else
   //     {
   //         Debug.Log("OnEnable : Firebase가 아직 초기화되지 않았습니다. 인증 상태를 확인할 수 없습니다.");
   //     }
   // }

    public void CheckCurrentUser()
    {
        if (!isFirebaseInitialized)
        {
            Debug.Log("CheckCurrentUser : Firebase가 아직 초기화되지 않았습니다. 인증 상태를 확인할 수 없습니다.");
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

    public void LogOut() //로그아웃
    {
        auth.SignOut();
        stateText.text= "You have logged out.";
        isLoggedIn = false;
        SetUIForLoggedOut();
    }

    void SetUIForLoggedIn() //로그인한 상태의 ui 요소 적용
    {
        loginButton.gameObject.SetActive(false);  // 로그인 버튼 감춤
        logoutButton.gameObject.SetActive(true);  // 로그아웃 버튼 보임

        // 로그인 상태 버튼 스프라이트 변경
        if (loginStateButton != null && loggedInSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = loggedInSprite;
        }
        }

    void SetUIForLoggedOut() //로그아웃한 상태의 ui 요소 적용
    {
        loginButton.gameObject.SetActive(true);   // 로그인 버튼 보임
        logoutButton.gameObject.SetActive(false); // 로그아웃 버튼 감춤

        if (loginStateButton != null && originalSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = originalSprite;
        }
    }

    void RememberEmail() //토글 활성화 여부를 확인하여 이메일 저장할지 결정
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

    void LoadRememberEmail() //저장된 이메일이 있다면 불러오고 / 없다면 이메일 필드를 비워둠
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
