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
사용자가 클릭 이벤트를 통해 데이터를 Firebase Realtime Database에 저장할 수 있도록
사용자의 username과 email을 JSON 형식으로 변환한 후, 이를 Firebase 데이터베이스에 업로드
*/

public class Firebase_Database : MonoBehaviour
{
    //JSON 파일로 만들기 위해 CLASS 정의 
    public class User //사용자의 데이터를 구조화하기 위함
    {
        public string username;
        public string email;
        public User(string username,string email)
        {
            this.username = username;
            this.email = email;
        }
    }
    DatabaseReference reference; //Firebase Database의 루트 참조입니다. Firebase와 통신하는 데 사용됩니다.
    int count = 1;
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase에서 불러온 랭킹 정보를 임시로 저장하는 배열입니다.
    private long strLen; //Firebase에서 불러온 데이터의 총 개수입니다.

    private bool textLoadBool = false;//데이터를 다 불러온 후에 텍스트를 업데이트할지를 결정하는 플래그입니다.

    void Start() //Firebase와의 통신을 시작하는 초기 설정을 담당
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

    //게임 승리 시 시간과 이메일 저장
    public void SaveVictoryTime(float elapsedTime, string gameName)
    {
        FirebaseAuthManager authManager = FirebaseAuthManager.instance;

        // authManager 혹은 user가 null인지 확인
        if (authManager == null || authManager.user == null)
        {
            Debug.LogError("FirebaseAuthManager 또는 user가 초기화되지 않았습니다.");
            return;
        }

        string userEmail = authManager.user.Email;
        string timeFormatted = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(elapsedTime / 60), Mathf.FloorToInt(elapsedTime % 60));

        int count = (int)Time.time;

        //firebase에 저장
        reference.Child("rank").Child("num" + count.ToString()).SetValueAsync(new Dictionary<string, object>
        { {"email",userEmail },
            {"game",gameName },
          {"score",timeFormatted }

        });
    }

    //랭킹 데이터 불러오기
    void DataLoad() //Database에서 랭킹 데이터를 비동기로 불러오는 메서드
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

    void TextLoad() //불러온 데이터를 UI에 표시하는 메서드
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

            //분과 초를 비교하여 정렬
            int xTotalSeconds = xMinutes * 60 + xSeconds;
            int yTotalSeconds = yMinutes * 60 + ySeconds;

            return xTotalSeconds.CompareTo(yTotalSeconds);

        }); //시간순 정렬

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
   // //로드 버튼 클릭 시
   // public void LoadBtn()
   // {
   //     readUser("personal information");
   // }
   //
   //private void writeNewUser(string userId, string name,string email,int count)
   // {
   //     //클래스 user 변수를 만들고 받아온 name,email 을 대입
   //     User user = new User(name, email);
   //     //대입 시킨 클래스 변수 user를 json 파일로 변경
   //     string json = JsonUtility.ToJson(user);
   //     //DatabaseReference 변수에 uderId를 자식으로 변환되 json 파일을 업로드 
   //     reference.Child(userId).Child("num"+count.ToString()).SetRawJsonValueAsync(json);
   // }
   //
   // private void readUser(string userId)
   // {
   //     //reference의 자식 userId를 task로 받음
   //     reference.Child(userId).GetValueAsync().ContinueWith(task =>
   //     {
   //         if (task.IsFaulted)
   //         {
   //             Debug.Log("Error");
   //         }
   //         else if (task.IsCompleted)
   //         {
   //             //task의 결과를 받는 변수
   //             DataSnapshot snapshot = task.Result;
   //             //snapshot의 자식 개수를 확인
   //             Debug.Log(snapshot.ChildrenCount);
   //
   //             //각 데이터를 IDictionary로 변환해 각 이름에 맞게 변수 초기화
   //             foreach (DataSnapshot data in snapshot.Children)
   //             {
   //                 IDictionary personInfo = (IDictionary)data.Value;
   //                 Debug.Log("email: " + personInfo["email"] + ", username: " + personInfo["username"] + ", num : " + personInfo["num"]);
   //             }
   //         }
   //     });
   // }
}
