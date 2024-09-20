using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using System.Threading.Tasks;
using TMPro;

/*
 ���̾�̽� ���� / ����� �α��� / �α׾ƿ� / ȸ������ / UI ������Ʈ / ����� ���� ����
 
 ���� ���� �� ��ũ��Ʈ���� ���̾�̽� �ʱ�ȭ�� ����� �ȵǼ� ������ �Ͼ�� ����
 */


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
    public Button loginStateButton;
    public Button registerButton;
    public Button logoutButton;
    public Sprite loggedInSprite;
    private Sprite originalSprite;
    public GameObject loginUIPanel;

   private bool isFirebaseInitialized = false;
   private bool isLoggedIn = false;
    private bool panelwasActive = false;

    void Awake() //���̾�̽� �ʱ�ȭ 
    {
        if(instance==null)
        {
            instance=this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseAuthManager instance ���� �Ϸ�");
            InitializeFirebase();

            if (loginStateButton !=null)
            {
                originalSprite = loginStateButton.GetComponent<Image>().sprite;
            }

        // Firebase �ʱ�ȭ
       // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
       // {
       //     if (task.IsCompleted && task.Result == DependencyStatus.Available)
       //     {
       //         // Firebase�� �ʱ�ȭ�� �� ���� ����� üũ�� ���� �����忡�� ����Ǿ�� ��
       //         UnityMainThreadDispatcher.Instance().Enqueue(() =>
       //         {
       //
       //         Debug.Log("�ʱ�ȭ �����ڡڡ�: " + task.Result);
       //
       //         // Firebase �ʱ�ȭ�� �Ϸ�Ǹ� auth ��ü ��� ����
       //         FirebaseApp app = FirebaseApp.DefaultInstance;
       //         
       //             auth = FirebaseAuth.DefaultInstance;
       //
       //
       //             if (FirebaseAuthManager.instance.auth != null)
       //             {
       //                 Debug.Log("Auth �ʱ�ȭ ����");
       //             }
       //             else
       //             {
       //                 Debug.LogError("Auth �ʱ�ȭ ����");
       //             }
       //
       //
       //
       //
       //             //  // Firebase �ʱ�ȭ �Ϸ� ǥ��
       //             isFirebaseInitialized = true;
       //
       //             // Firebase�� �ʱ�ȭ�� �� ���� ����� üũ
       //             CheckCurrentUser();
       //         });
       //     }
       //     else
       //     {
       //         Debug.LogError("�ʱ�ȭ �����ԡڡڡ�: " + task.Result);
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
                Debug.LogError("Firebase �ʱ�ȭ ����: " + task.Result);
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
                Debug.Log("Update : Firebase�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
   //         Debug.Log("OnEnable : Firebase�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�. ���� ���¸� Ȯ���� �� �����ϴ�.");
   //     }
   // }

    public void CheckCurrentUser()
    {
        if (!isFirebaseInitialized)
        {
            Debug.Log("CheckCurrentUser : Firebase�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�. ���� ���¸� Ȯ���� �� �����ϴ�.");
            return;
        }

        if (auth==null)
        {
            Debug.Log("���̾�̽����� �ʱ�ȭ��������");
            return;
        }

        Debug.Log("���¸� ǥ���ҰԿ�");
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
            // Firebase�� ����� ���� ��û�� �񵿱�� �����ϴ�.
            var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            // �񵿱� �۾��� �Ϸ�� �� FirebaseUser�� �����ɴϴ�.
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
            isLoggedIn = true;
            SetUIForLoggedIn();
        }
        catch (FirebaseException e)
        {
            Debug.LogError("Login failed: " + e.Message);
            stateText.text= "Login failed" + e.Message;
        }
    }

    public void LogOut() //�α׾ƿ�
    {
        auth.SignOut();
        stateText.text= "You have logged out.";
        isLoggedIn = false;
        SetUIForLoggedOut();
    }

    void SetUIForLoggedIn() //�α����� ������ ui ��� ����
    {
        loginButton.gameObject.SetActive(false);  // �α��� ��ư ����
        logoutButton.gameObject.SetActive(true);  // �α׾ƿ� ��ư ����

        // �α��� ���� ��ư ��������Ʈ ����
        if (loginStateButton != null && loggedInSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = loggedInSprite;
        }
        }

    void SetUIForLoggedOut() //�α׾ƿ��� ������ ui ��� ����
    {
        loginButton.gameObject.SetActive(true);   // �α��� ��ư ����
        logoutButton.gameObject.SetActive(false); // �α׾ƿ� ��ư ����

        if (loginStateButton != null && originalSprite != null)
        {
            loginStateButton.GetComponent<Image>().sprite = originalSprite;
        }
    }

    void RememberEmail() //��� Ȱ��ȭ ���θ� Ȯ���Ͽ� �̸��� �������� ����
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

    void LoadRememberEmail() //����� �̸����� �ִٸ� �ҷ����� / ���ٸ� �̸��� �ʵ带 �����
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
