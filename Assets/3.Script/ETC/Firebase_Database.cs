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

/*
����ڰ� Ŭ�� �̺�Ʈ�� ���� �����͸� Firebase Realtime Database�� ������ �� �ֵ���
������� username�� email�� JSON �������� ��ȯ�� ��, �̸� Firebase �����ͺ��̽��� ���ε�
*/

public class Firebase_Database : MonoBehaviour
{
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
    DatabaseReference reference; //Firebase Database�� ��Ʈ �����Դϴ�. Firebase�� ����ϴ� �� ���˴ϴ�.
    int count = 1;
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase���� �ҷ��� ��ŷ ������ �ӽ÷� �����ϴ� �迭�Դϴ�.
    private long strLen; //Firebase���� �ҷ��� �������� �� �����Դϴ�.

    private bool textLoadBool = false;//�����͸� �� �ҷ��� �Ŀ� �ؽ�Ʈ�� ������Ʈ������ �����ϴ� �÷����Դϴ�.

    void Start() //Firebase���� ����� �����ϴ� �ʱ� ������ ���
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;    
    }

    void Update()
    {
        if (Rank[0].text == "ID")
        {
            DataLoad();
        }
        
    }
    void LateUpdate()
    {
        if(textLoadBool)
        {
            TextLoad();
        }
      //  if (Time.timeScale != 0.0f) Time.timeScale = 0.0f;
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

        int count = (int)Time.time;

        //firebase�� ����
        reference.Child("rank").Child("num" + count.ToString()).SetValueAsync(new Dictionary<string, object>
        { {"email",userEmail },
            {"game",gameName },
          {"score",timeFormatted }

        });
    }

    //��ŷ ������ �ҷ�����
    void DataLoad() //Database���� ��ŷ �����͸� �񵿱�� �ҷ����� �޼���
    {
        reference.Child("rank").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
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
                textLoadBool = true;
            }
        });
    }

    void TextLoad() //�ҷ��� �����͸� UI�� ǥ���ϴ� �޼���
    {
        textLoadBool = false;
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

   // public void OnClickSave()
   // {
   //                               //userId , name , email
   //     writeNewUser("personal information","Han","bittersweeety@gmail.com",count);
   //     count++;
   // }
   //
   // //�ε� ��ư Ŭ�� ��
   // public void LoadBtn()
   // {
   //     readUser("personal information");
   // }
   //
   //private void writeNewUser(string userId, string name,string email,int count)
   // {
   //     //Ŭ���� user ������ ����� �޾ƿ� name,email �� ����
   //     User user = new User(name, email);
   //     //���� ��Ų Ŭ���� ���� user�� json ���Ϸ� ����
   //     string json = JsonUtility.ToJson(user);
   //     //DatabaseReference ������ uderId�� �ڽ����� ��ȯ�� json ������ ���ε� 
   //     reference.Child(userId).Child("num"+count.ToString()).SetRawJsonValueAsync(json);
   // }
   //
   // private void readUser(string userId)
   // {
   //     //reference�� �ڽ� userId�� task�� ����
   //     reference.Child(userId).GetValueAsync().ContinueWith(task =>
   //     {
   //         if (task.IsFaulted)
   //         {
   //             Debug.Log("Error");
   //         }
   //         else if (task.IsCompleted)
   //         {
   //             //task�� ����� �޴� ����
   //             DataSnapshot snapshot = task.Result;
   //             //snapshot�� �ڽ� ������ Ȯ��
   //             Debug.Log(snapshot.ChildrenCount);
   //
   //             //�� �����͸� IDictionary�� ��ȯ�� �� �̸��� �°� ���� �ʱ�ȭ
   //             foreach (DataSnapshot data in snapshot.Children)
   //             {
   //                 IDictionary personInfo = (IDictionary)data.Value;
   //                 Debug.Log("email: " + personInfo["email"] + ", username: " + personInfo["username"] + ", num : " + personInfo["num"]);
   //             }
   //         }
   //     });
   // }
}
