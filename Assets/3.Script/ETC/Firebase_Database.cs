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

public class Firebase_Database : MonoBehaviour
{
    DatabaseReference reference; //Firebase Database�� ��Ʈ �����Դϴ�. Firebase�� ����ϴ� �� ���˴ϴ�.
    int maxRetryCount = 3;
    int currentRetry = 0;
    
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase���� �ҷ��� ��ŷ ������ �ӽ÷� �����ϴ� �迭�Դϴ�.
    private long strLen; //Firebase���� �ҷ��� �������� �� �����Դϴ�.

   // private bool textLoadBool = false;//�����͸� �� �ҷ��� �Ŀ� �ؽ�Ʈ�� ������Ʈ������ �����ϴ� �÷����Դϴ�.

    string uniqueKey = Guid.NewGuid().ToString();

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

   // void LateUpdate()
   // {
   //     if(textLoadBool)
   //     {
   //         TextLoad();
   //     }
   //   //  if (Time.timeScale != 0.0f) Time.timeScale = 0.0f;
   // }

    public void OnRankingUIPanelOpened()
    {
        if(rankingUIpanel.activeSelf)
        {
            DataLoad();
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

       // int count = (int)Time.time;

        //firebase�� ����
        reference.Child("rank").Child(uniqueKey).SetValueAsync(new Dictionary<string, object>
        { {"email",userEmail },
            {"game",gameName },
          {"score",timeFormatted }

        });
    }

    //��ŷ ������ �ҷ�����
    void DataLoad() //Database���� ��ŷ �����͸� �񵿱�� �ҷ����� �޼���
    {
        if(currentRetry>=maxRetryCount)
        {
            Debug.LogError("Failed to load data after multiple attempts.");
            return;
        }

        reference.Child("rank").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                currentRetry++;
                DataLoad();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int count = 0;
                strLen = snapshot.ChildrenCount;
                strRank = new string[strLen];

                foreach (DataSnapshot data in snapshot.Children)
                {
                    IDictionary rankInfo = (IDictionary)data.Value;
                    strRank[count] = rankInfo["email"].ToString() + "  |  "
                                    +rankInfo["game"].ToString()+ "  |  "
                                    + rankInfo["score"].ToString();
                    count++;
                }
                TextLoad();
            }
        });
    }

    void TextLoad() //�ҷ��� �����͸� UI�� ǥ���ϴ� �޼���
    {
        Array.Sort(strRank, (x,y)=>
        {
            string[] xParts = x.Split('|')[1].Split(':');
            string[] yParts = y.Split('|')[1].Split(':');

            int xMinutes = int.Parse(xParts[0]);
            int xSeconds = int.Parse(xParts[1]);

            int yMinutes = int.Parse(yParts[0]);
            int ySeconds = int.Parse(yParts[1]);

            //�а� �ʸ� ���Ͽ� ����
            int xTotalSeconds = xMinutes * 60 + xSeconds;
            int yTotalSeconds = yMinutes * 60 + ySeconds;

            return xTotalSeconds.CompareTo(yTotalSeconds);

        }); //�ð��� ����

        for(int i=0; i<Rank.Length;i++)
        {
            if (i>=strLen) return;
            Rank[i].text = strRank[i];
        }
    }
}
