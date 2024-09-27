using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Unity;
using System.Threading.Tasks;

/*
���̾�̽� �ǽð� �����ͺ��̽��� ��� ��� / ���ӿ� �¸��� �ð� / ����� �̸��� ���� / ��ŷ�� �ҷ���

*/

public class Firebase_Database : MonoBehaviour
{
    DatabaseReference reference; //Firebase Database�� ��Ʈ �����Դϴ�. Firebase�� ����ϴ� �� ���˴ϴ�.
    int maxRetryCount = 3;
    int currentRetry = 0;
    
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase���� �ҷ��� ��ŷ ������ �ӽ÷� �����ϴ� �迭�Դϴ�.

    public GameObject rankingUIpanel;

    
    //JSON ���Ϸ� ����� ���� CLASS ���� 
    public class User //������� �����͸� ����ȭ�ϱ� ����
    {
        public string username;
        public string email;
        public User(string username,string email)
        {
            this.username = username;
            this.email = email;
        }
    }

    void Start() //Firebase���� ����� �����ϴ� �ʱ� ������ ���
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                reference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    public void OnRankingUIPanelOpened()
    {

        Debug.Log("OnRankingUIPanelOpened ȣ���.");
        InitializeRankUI(); // ��ü UI �ʱ�ȭ
        DataLoad();
    }

    private void InitializeRankUI()
    {
        for(int i=0; i<Rank.Length;i++)
        {
            Rank[i].text = "";
        }
    }

    //���� �¸� �� �ð��� �̸��� ����
    public void SaveVictoryTime(float elapsedTime, string gameName)
    {
        FirebaseAuthManager authManager = FirebaseAuthManager.instance;

        // authManager Ȥ�� user�� null���� Ȯ��
        if (authManager == null || authManager.user == null)
        {
            Debug.LogError("FirebaseAuthManager �Ǵ� user�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        string userEmail = authManager.user.Email;
        string timeFormatted = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(elapsedTime / 60), Mathf.FloorToInt(elapsedTime % 60));

        string uniqueKey = Guid.NewGuid().ToString();
        // int count = (int)Time.time;

        StartCoroutine(SaveVictoryTime(userEmail, timeFormatted, gameName, uniqueKey));
    }

    private IEnumerator SaveVictoryTime(string userEmail, string timeFormatted, string gameName, string uniqueKey)
    {
        var saveTask = reference.Child("rank").Child(uniqueKey).SetValueAsync(new Dictionary<string, object>
        {
          {"email",userEmail },
            {"game",gameName },
          {"score",timeFormatted }

        });

        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.IsFaulted)
        {

            Debug.LogError("�¸� �ð� ���� ����: " + saveTask.Exception);
        }
        else
        {
            Debug.Log("�¸� �ð��� ���������� ����Ǿ����ϴ�.");

        }
    }

    //��ŷ ������ �ҷ�����
    void DataLoad() //Database���� ��ŷ �����͸� �񵿱�� �ҷ����� �޼���
    {
        if(currentRetry>=maxRetryCount)
        {
            Debug.LogError("Failed to load data after multiple attempts.");
            return;
        }

        StartCoroutine(LoadDataCoroutine()); // �ڷ�ƾ�� ���� �񵿱� �۾� ó��
    }

    IEnumerator LoadDataCoroutine()
    {

        var task = reference.Child("rank").GetValueAsync(); // Firebase �񵿱� �۾� ����
        yield return new WaitUntil(() => task.IsCompleted); // �񵿱� �۾��� �Ϸ�� ������ ���

            if (task.IsFaulted)
            {
                Debug.LogError("Data load failed: " + task.Exception);
                currentRetry++;
                DataLoad();
            }
            else
            {
                DataSnapshot snapshot = task.Result;

                    // ������ �ε� Ȯ�� �α�
                    Debug.Log("Firebase ������ �ε� �Ϸ�. ������ ����: " + snapshot.ChildrenCount);

                    if (snapshot.ChildrenCount == 0)
                    {
                        Debug.LogWarning("��ŷ �����Ͱ� �����ϴ�.");
                yield break; // ��ŷ �����Ͱ� ������ ����
            }

                    List<string> validRanks = new List<string>(); // ��ȿ�� �����͸� ������ ����Ʈ

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        IDictionary rankInfo = (IDictionary)data.Value;

                        if (rankInfo.Contains("email") && rankInfo.Contains("game") && rankInfo.Contains("score"))
                {
                    string gameName = rankInfo["game"].ToString();

                    // "LEVEL MAP_01" �Ǵ� "LEVEL MAP_02"�� ��츸 ����Ʈ�� �߰�
                    if (gameName == "LEVEL MAP_01" || gameName == "LEVEL MAP_02")
                    {
                        string rankData = rankInfo["email"].ToString() + "  |  "
                                        + gameName + "  |  "
                                        + rankInfo["score"].ToString();

                        validRanks.Add(rankData); // ��ȿ�� �����͸� �߰�
                        Debug.Log("�ҷ��� ������: " + rankData);
                    }
                    else
                    {
                        Debug.Log($"���� '{gameName}' �����ʹ� ���õ�.");
                    }
                }

                else
                {
                    Debug.Log("��ŷ �����Ͱ� �̻��ؿ�~");
                }
            }
                    strRank = validRanks.ToArray(); // ��ȿ�� �����͸� �迭�� ��ȯ
                    TextLoad();
            }
    }

    void TextLoad() //�ҷ��� �����͸� UI�� ǥ���ϴ� �޼���
    {
        Debug.Log("TextLoad() ȣ���. �ҷ��� �����͸� UI�� ǥ���մϴ�.");
        Debug.Log($"Rank �迭�� ũ��: {Rank.Length}");

        if (strRank == null || strRank.Length == 0)
        {
            Debug.LogError("strRank �迭�� ����ְų� null�Դϴ�.");
            return;
        }

        Debug.Log($"strRank �迭�� ũ��: {strRank.Length}");

        Array.Sort(strRank, (x,y)=>
        {
            try
            {
                string[] xParts = x.Split('|');
                string[] yParts = y.Split('|');

                string[] xTimeParts = xParts[2].Trim().Split(':');
                string[] yTimeParts = yParts[2].Trim().Split(':');

                int xMinutes = int.Parse(xTimeParts[0]);
                int xSeconds = int.Parse(xTimeParts[1]);

                int yMinutes = int.Parse(yTimeParts[0]);
                int ySeconds = int.Parse(yTimeParts[1]);

                //�а� �ʸ� ���Ͽ� ����
                int xTotalSeconds = xMinutes * 60 + xSeconds;
                int yTotalSeconds = yMinutes * 60 + ySeconds;

                return xTotalSeconds.CompareTo(yTotalSeconds);
            }
            catch(Exception e)
            {
                Debug.LogError("Error parsing rank data: " + e.Message);
                return 0;
            }
        }); //�ð��� ����

        try
        {

            for (int i = 0; i < Rank.Length; i++)
            {
                Debug.Log($"���� Rank[{i}] ó�� ��");

                if (i < strRank.Length)
                {
                    Debug.Log($"Rank[{i}]�� �Ҵ�� �ؽ�Ʈ: {strRank[i]}");
                    Rank[i].text = strRank[i];
                }
                else
                {
                    Debug.Log($"Rank[{i}]�� ��� �ֽ��ϴ�.");
                    Rank[i].text = "";
                }
                Rank[i].fontSize = 60;

            }
        }
        catch(Exception e)
        {
            Debug.LogError($"���� �߻�: {e.Message}");
        }
    }
}
