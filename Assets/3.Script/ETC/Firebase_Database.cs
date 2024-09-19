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
    DatabaseReference reference; //Firebase Database의 루트 참조입니다. Firebase와 통신하는 데 사용됩니다.
    int maxRetryCount = 3;
    int currentRetry = 0;
    
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase에서 불러온 랭킹 정보를 임시로 저장하는 배열입니다.
    private long strLen; //Firebase에서 불러온 데이터의 총 개수입니다.

   // private bool textLoadBool = false;//데이터를 다 불러온 후에 텍스트를 업데이트할지를 결정하는 플래그입니다.

    string uniqueKey = Guid.NewGuid().ToString();

    public GameObject rankingUIpanel;

    
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

    void Start() //Firebase와의 통신을 시작하는 초기 설정을 담당
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

       // int count = (int)Time.time;

        //firebase에 저장
        reference.Child("rank").Child(uniqueKey).SetValueAsync(new Dictionary<string, object>
        { {"email",userEmail },
            {"game",gameName },
          {"score",timeFormatted }

        });
    }

    //랭킹 데이터 불러오기
    void DataLoad() //Database에서 랭킹 데이터를 비동기로 불러오는 메서드
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
            Debug.LogError("Data load failed: "+task.Exception);
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

                    if (rankInfo.Contains("email") && rankInfo.Contains("game") && rankInfo.Contains("score"))
                    {
                        strRank[count] = rankInfo["email"].ToString() + "  |  "
                                    + rankInfo["game"].ToString() + "  |  "
                                    + rankInfo["score"].ToString();
                        count++;
                    }
                    else
                    {
                        Debug.Log("랭킹 데이터가 이상해요~");
                    }
                }

                if (UnityMainThreadDispatcher.instance != null)
                {
                    // UI 업데이트를 메인 스레드에서 수행
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        TextLoad();
                    });
                }
                else
                {
                    Debug.LogError("UnityMainThreadDispatcher 가 없어요...");
                }
            }
        });
    }

    void TextLoad() //불러온 데이터를 UI에 표시하는 메서드
    {
        Array.Sort(strRank, (x,y)=>
        {
            try
            {
                string[] xParts = x.Split('|');
                string[] yParts = y.Split('|');

                if(xParts.Length<3||yParts.Length<3)
                {
                    Debug.LogError("랭킹데이터포멧이 이상하네요...");
                    return 0;
                }

                string[] xTimeParts = xParts[2].Trim().Split(':');
                string[] yTimeParts = yParts[2].Trim().Split(':');

                int xMinutes = int.Parse(xTimeParts[0]);
                int xSeconds = int.Parse(xTimeParts[1]);

                int yMinutes = int.Parse(yTimeParts[0]);
                int ySeconds = int.Parse(yTimeParts[1]);

                //분과 초를 비교하여 정렬
                int xTotalSeconds = xMinutes * 60 + xSeconds;
                int yTotalSeconds = yMinutes * 60 + ySeconds;

                return xTotalSeconds.CompareTo(yTotalSeconds);
            }
            catch(Exception e)
            {
                Debug.LogError("Error parsing rank data: " + e.Message);
                return 0;
            }
        }); //시간순 정렬

        for(int i=0; i<Mathf.Min(Rank.Length,strRank.Length);i++)
        {
            Rank[i].text = strRank[i];
        }
    }
}
