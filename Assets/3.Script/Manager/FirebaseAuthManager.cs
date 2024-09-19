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
    public FirebaseAuth auth;  //�α��� ȸ������ � ���
    public FirebaseUser user; //������ �Ϸ�� ���� ����

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TextMeshProUGUI stateText;
    public Toggle remeberMeToggle;

    public Button loginButton;
    public Button registerButton;
    public Button logoutButton;
    public GameObject loginUIPanel;

   private bool isFirebaseInitialized = false;

    void Awake()
    {
        if(instance==null)
        {
            instance=this;
            DontDestroyOnLoad(gameObject);

        // Firebase �ʱ�ȭ
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                // Firebase �ʱ�ȭ�� �Ϸ�Ǹ� auth ��ü ��� ����
              //  FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                //  // Firebase �ʱ�ȭ �Ϸ� ǥ��
                isFirebaseInitialized = true;

                // Firebase�� �ʱ�ȭ�� �� ���� ����� üũ
                CheckCurrentUser();
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
    }

    public void CheckCurrentUser()
    {
        if(auth==null)
        {
            Debug.Log("���̾�̽����� �ʱ�ȭ��������");
            return;
        }

        Debug.Log("���¸� ǥ���ҰԿ�");
        user = auth.CurrentUser;
        if(user !=null)
        {
            UpdateLoggedInUI();
        }
        else
        {
            UpdateLoggedOutUI();
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
            // Firebase�� ����� ���� ��û�� �񵿱�� �����ϴ�.
            var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            // �񵿱� �۾��� �Ϸ�� �� FirebaseUser�� �����ɴϴ�.
            FirebaseUser newUser = authResult.User;
            stateText.text= "Registration successful: " + newUser.Email;
            RememberEmail();
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

        // �̸��� �� ��й�ȣ ��ȿ�� �˻�
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
        SetUIForLoggedOut();
    }

    void SetUIForLoggedIn()
    {
        loginButton.gameObject.SetActive(false);  // �α��� ��ư ����
        logoutButton.gameObject.SetActive(true);  // �α׾ƿ� ��ư ����
    }

    void SetUIForLoggedOut()
    {
        loginButton.gameObject.SetActive(true);   // �α��� ��ư ����
        logoutButton.gameObject.SetActive(false); // �α׾ƿ� ��ư ����
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
