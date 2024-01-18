using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
[HideInInspector]
public class GoogleData
{
    public string order, result, msg, value;
}

//참고 유튜브 : https://www.youtube.com/watch?v=3LxaTtLsC-w&ab_channel=%EA%B3%A0%EB%9D%BC%EB%8B%88TV-%EA%B2%8C%EC%9E%84%EA%B0%9C%EB%B0%9C%EC%B1%84%EB%84%90
public class GoogleSheetManager : MonoBehaviour
{
    // [1. 단순히 나 혼자 구글스프레드시트에서 엑셀데이터를 가져오고싶을때]
    // 아래보면 /export전까지 내 구글시트 링크의 /edit전까지를 복사해서 붙여넣는다.
    // /export?format=tsv (뒤에 이 tsv부분은 탭과 엔터로 이루어진 파일로 불러오겠다는 뜻)
    // &range=A2:A는 가져오고 싶은 범위(A2부터 A전체)
    // 2번째 sheet부터는 그 시트의 gid정보를 가져와서 ex) &gid=1778935828 를 뒤에 붙여준다.
    const string sheet1URL = "https://docs.google.com/spreadsheets/d/1PLnYfbYxz44NYJYaiOwZ2pWIHVbMDtbWAhOmxpxBXiM/export?format=tsv&range=A2:A";
    const string sheet2URL = "https://docs.google.com/spreadsheets/d/1PLnYfbYxz44NYJYaiOwZ2pWIHVbMDtbWAhOmxpxBXiM/export?format=tsv&gid=1778935828&range=A1:B1";

    IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        //sheet1테스트
        UnityWebRequest sheet1www = UnityWebRequest.Get(sheet1URL); //URL에 적혀있는대로 데이터를 가져온다.
        yield return sheet1www.SendWebRequest(); //잘모르겠지만 통신요청?
        string data1 = sheet1www.downloadHandler.text; // string형식으로 받아온뒤
        //print(data1); // sheet1 A2:A 출력

        //sheet2테스트
        UnityWebRequest sheet2www = UnityWebRequest.Get(sheet2URL);
        yield return sheet2www.SendWebRequest();
        string data2 = sheet2www.downloadHandler.text;
        //print(data2);// sheet2 A1:B1 출력
    }


    // [2. 여러 사람이 통신 가능하게]
    // 스프레드시트에서 배포를 누르고 나오는 URL링크를 아래에 대입. 수정할때마다 배포를 다시해서 URL도 다시 대입해줘야함
    const string URL = "https://script.google.com/macros/s/AKfycbz3qLPu9CiOtXkLxSNUUElLx_pA1ESSPfxKiBtPXQ_Yb3cgBdzOGiHzrq5LWUKyhHzvbg/exec";
    public GoogleData GD;
    public TMP_InputField NicknameInput;
    string nickname;

    // 닉네임 형식 검사 및 nickname string변수에 받아오기
    bool SetNicknamePass()
    {
        nickname = NicknameInput.text.Trim(); // 앞뒤 공백 제거
        if (nickname == "") return false; // 입력필드가 비어있으면 실행안함
        else return true;
    }

    /*// 닉네임 로그인해주기
    public void NicknameLogin()
    {
        if (!SetNicknamePass())
        {
            print("아이디 또는 비밀번호가 비어있습니다");
            return;
        }

        // form에 뭐할지를 쌓아둔후
        WWWForm form = new WWWForm();
        form.AddField("order", "nicknamelogin"); // p.order에 nicknamelogin 보내주기
        form.AddField("id", nickname); // p.id에 nickname보내주기
        StartCoroutine(NicknamePost(form)); // 여기서 form에 쌓아놨던것들 실행해준다.
    }*/

    // 코루틴형태로 바뀐 버전
    public IEnumerator NicknameLogin()
    {
        if (!SetNicknamePass())
        {
            print("아이디 또는 비밀번호가 비어있습니다");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "nicknamelogin");
        form.AddField("id", nickname);
        yield return StartCoroutine(NicknamePost(form));
    }

    // 게임 종료시 호출 메서드
    void OnApplicationQuit()
    {
        Logout();
    }

    // 스프레드시트의 닉네임을 삭제(빈 문자열로 교체)해주는 메서드
    public void Logout()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "logout");
        StartCoroutine(Post(form));
    }

    // 웹에 통신 보내주는 함수
    IEnumerator Post(WWWForm form) 
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone) Response(www.downloadHandler.text);
            else print("웹의 응답이 없습니다.");
        }
    }

    IEnumerator NicknamePost(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone) Response(www.downloadHandler.text);
            else print("웹의 응답이 없습니다.");
            //이 부분에 뭔가 통신이 종료되고 난 후 호출하고 싶은 메서드를 작성 @@
        }
    }

    //json파일 형식을 콘솔창에 이쁘게 파싱해서 보여주는 메서드
    void Response(string json) 
    {
        if (string.IsNullOrEmpty(json)) return;

        GD = JsonUtility.FromJson<GoogleData>(json);

        if (GD.result == "ERROR")
        {
            print(GD.order + "을 실행할 수 없습니다.\nERROR : " + GD.msg);
            return;
        }

        print(GD.order + "을 실행했습니다.\n메시지 : " + GD.msg);

        if (GD.order == "getValue")
        {
            //ValueInput.text = GD.value;
        }
    }

    /*//유니티에서 구글로 Set(쓰기)
    public void SetValue() 
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "setValue");
        //form.AddField("value", ValueInput.text);

        StartCoroutine(Post(form));
    }

    //구글에서 유니티로 Get(읽기)
    public void GetValue() 
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "getValue");

        StartCoroutine(Post(form));
    }*/
}