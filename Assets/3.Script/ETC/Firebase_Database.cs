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
파이어베이스 실시간 데이터베이스와 통신 담당 / 게임에 승리한 시간 / 사용자 이메일 저장 / 랭킹을 불러옴

*/

public class Firebase_Database : MonoBehaviour
{
    DatabaseReference reference; //Firebase Database의 루트 참조입니다. Firebase와 통신하는 데 사용됩니다.
    int maxRetryCount = 3;
    int currentRetry = 0;
    
    public TextMeshProUGUI[] Rank = new TextMeshProUGUI[7];
    private string[] strRank; //Firebase에서 불러온 랭킹 정보를 임시로 저장하는 배열입니다.

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

    public void OnRankingUIPanelOpened()
    {

        Debug.Log("OnRankingUIPanelOpened 호출됨.");
        InitializeRankUI(); // 전체 UI 초기화
        DataLoad();
    }

    private void InitializeRankUI()
    {
        for(int i=0; i<Rank.Length;i++)
        {
            Rank[i].text = "";
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

            Debug.LogError("승리 시간 저장 실패: " + saveTask.Exception);
        }
        else
        {
            Debug.Log("승리 시간이 성공적으로 저장되었습니다.");

        }
    }

    //랭킹 데이터 불러오기
    void DataLoad() //Database에서 랭킹 데이터를 비동기로 불러오는 메서드
    {
        if(currentRetry>=maxRetryCount)
        {
            Debug.LogError("Failed to load data after multiple attempts.");
            return;
        }

        StartCoroutine(LoadDataCoroutine()); // 코루틴을 통해 비동기 작업 처리
    }

    IEnumerator LoadDataCoroutine()
    {

        var task = reference.Child("rank").GetValueAsync(); // Firebase 비동기 작업 시작
        yield return new WaitUntil(() => task.IsCompleted); // 비동기 작업이 완료될 때까지 대기

            if (task.IsFaulted)
            {
                Debug.LogError("Data load failed: " + task.Exception);
                currentRetry++;
                DataLoad();
            }
            else
            {
                DataSnapshot snapshot = task.Result;

                    // 데이터 로드 확인 로그
                    Debug.Log("Firebase 데이터 로드 완료. 데이터 개수: " + snapshot.ChildrenCount);

                    if (snapshot.ChildrenCount == 0)
                    {
                        Debug.LogWarning("랭킹 데이터가 없습니다.");
                yield break; // 랭킹 데이터가 없으면 종료
            }

                    List<string> validRanks = new List<string>(); // 유효한 데이터만 저장할 리스트

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        IDictionary rankInfo = (IDictionary)data.Value;

                        if (rankInfo.Contains("email") && rankInfo.Contains("game") && rankInfo.Contains("score"))
                {
                    string gameName = rankInfo["game"].ToString();

                    // "LEVEL MAP_01" 또는 "LEVEL MAP_02"인 경우만 리스트에 추가
                    if (gameName == "LEVEL MAP_01" || gameName == "LEVEL MAP_02")
                    {
                        string rankData = rankInfo["email"].ToString() + "  |  "
                                        + gameName + "  |  "
                                        + rankInfo["score"].ToString();

                        validRanks.Add(rankData); // 유효한 데이터만 추가
                        Debug.Log("불러온 데이터: " + rankData);
                    }
                    else
                    {
                        Debug.Log($"게임 '{gameName}' 데이터는 무시됨.");
                    }
                }

                else
                {
                    Debug.Log("랭킹 데이터가 이상해요~");
                }
            }
                    strRank = validRanks.ToArray(); // 유효한 데이터를 배열로 변환
                    TextLoad();
            }
    }

    void TextLoad() //불러온 데이터를 UI에 표시하는 메서드
    {
        Debug.Log("TextLoad() 호출됨. 불러온 데이터를 UI에 표시합니다.");
        Debug.Log($"Rank 배열의 크기: {Rank.Length}");

        if (strRank == null || strRank.Length == 0)
        {
            Debug.LogError("strRank 배열이 비어있거나 null입니다.");
            return;
        }

        Debug.Log($"strRank 배열의 크기: {strRank.Length}");

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

        try
        {

            for (int i = 0; i < Rank.Length; i++)
            {
                Debug.Log($"현재 Rank[{i}] 처리 중");

                if (i < strRank.Length)
                {
                    Debug.Log($"Rank[{i}]에 할당될 텍스트: {strRank[i]}");
                    Rank[i].text = strRank[i];
                }
                else
                {
                    Debug.Log($"Rank[{i}]는 비어 있습니다.");
                    Rank[i].text = "";
                }
                Rank[i].fontSize = 60;

            }
        }
        catch(Exception e)
        {
            Debug.LogError($"예외 발생: {e.Message}");
        }
    }
}
