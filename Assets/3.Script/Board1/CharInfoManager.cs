using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;


public class CharInfo
{
    public int playerNO; // 플레이어 번호 (1~4: 플레이어 / 5~8: CPU)
    public int charIndex; // 캐릭터의 인덱스 (0~15) 

    public int rank; // 현재 순위 (1~4)
    public int[] items = new int[3]; // 보유한 아이템 
    public int score; // 점수 [보드 4의 경우 HP]

    public CharInfo(int pNo)
    {
        playerNO = pNo;
        try { charIndex = GameManager.instance.selectChar[playerNO]; }
        catch (Exception) { charIndex = playerNO; }

        rank = 1;
        for (int i = 0; i < 3; i++) items[i] = 0;
        score = 0;
    }

}

public class CharInfoManager : MonoBehaviour
{
    [SerializeField] private GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] private GameObject CharacterMark; // 캐릭터 위치 표시 오브젝트
    [SerializeField] private Sprite[] CharLogo; // 캐릭터 로고 스프라이트
    [SerializeField] private Sprite[] CharMarks; // 캐릭터 위치 표시 스프라이트
    [SerializeField] private Sprite[] RankSp; // 순위 스프라이트
    [SerializeField] private Sprite[] PNoSp; // 플레이어 번호 스프라이트
    [SerializeField] private Sprite[] ItemSp; // 아이템 스프라이트
    [SerializeField] private Sprite[] NumSp; // 점수 표기용 숫자 스프라이트
    public CharInfo[] charinfo = new CharInfo[4];
    [SerializeField] public GameObject ResultUI; // 결과 UI

    // 0. 싱글톤 적용
    public static CharInfoManager instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        DOTween.Init();
        for (int i = 0; i < 4; i++)
        {
            charinfo[i] = new CharInfo(i + 1);
            CharUI[i].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex]; // 캐릭터 로고 설정
            CharUI[i].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[charinfo[i].rank - 1]; // 순위 이미지 설정
            CharUI[i].transform.Find("Upper_Right").GetComponent<Image>().sprite = PNoSp[charinfo[i].playerNO - 1]; // 플레이어 번호 이미지 설정
            CharUI[i].transform.Find("UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex]; // 플레이어 이름 설정
            CharacterMark.transform.Find($"{i + 1}P").GetComponent<SpriteRenderer>().sprite = CharMarks[charinfo[i].charIndex];
            ScoreSpriteSet(i+1, charinfo[i].score);
            RankSpriteSet(i+1, charinfo[i].rank);
        }
    }


    public void ScoreAdd(int pNo, int addscore = 1)
    {
        charinfo[pNo-1].score += addscore;
        ScoreSpriteSet(pNo, charinfo[pNo-1].score);

        // 순위 재계산 및 이미지 변경
        for (int i = 0; i < 4; i++)
        {
            int higherThanMe = 0;
            for (int j = 0; j < 4; j++)
            {
                if (charinfo[j].score > charinfo[i].score) higherThanMe++;
            }
            charinfo[i].rank = higherThanMe + 1;
            RankSpriteSet(i+1, higherThanMe + 1);
        }
    }


    private void ScoreSpriteSet(int pNo, int score)
    {
        // 점수 십의 자리 설정
        if (score < 10)
        {
            CharUI[pNo-1].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[10];
        }
        else
        {
            CharUI[pNo-1].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
        }
        // 점수 일의 자리 설정
        CharUI[pNo-1].transform.Find("Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
    }

    private void RankSpriteSet(int pNo, int rank)
    {
        // 순위 이미지 재설정
        CharUI[pNo-1].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[rank - 1];
        // 테두리, 그라데이션 색 재설정
        Color c = new Color(0.5f, 0.5f, 0.0f, 0.5f);
        if (rank.Equals(2)) c = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        else if (rank.Equals(3)) c = new Color(0.5f, 0.0f, 0.0f, 0.5f);
        else if (rank.Equals(4)) c = new Color(0.5f, 0.0f, 0.5f, 0.5f);
        CharUI[pNo-1].transform.Find("Upper_Left").GetComponent<Image>().color = c;
        CharUI[pNo-1].transform.Find("Lower_Right").GetComponent<Image>().color = c;
        CharUI[pNo-1].transform.Find("Center/Edge").GetComponent<Image>().color = c;
    }

    public void DebugAddScore(int n)
    {
        ScoreAdd(n+1);
    }

    public IEnumerator ResultUISetting()
    {
        Tween moveTween = ResultUI.transform.Find("Superstar").DOScale(Vector3.one * 2, 1f).SetEase(Ease.OutBack); // 슈퍼스타 로고 뜨게
        yield return new WaitForSeconds(8f); // 임시: 승리 효과음 끝날때 까지 대기
        ResultUI.transform.Find("Text").gameObject.SetActive(true); // 안내 문구 활성화
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space)); // 스페이스바 누르면
        ResultUI.transform.Find("Text").gameObject.SetActive(false); // 안내 문구 비활성화
        ResultUI.transform.Find("Superstar").gameObject.SetActive(false); // 슈퍼스타 로고 비활성화

        bool[] RankSlotUsed = new bool[5] { false, false, false, false, false }; // 해당 순위에 캐릭터가 들어가 있는지? - 공동 순위때 사용
        for (int i = 0; i < 4; i++) // 1P 부터 순위를 찾아 거기에 집어넣기
        {
            int rankslot = charinfo[i].rank; // 들어갈 슬롯을 지정할 순위 불러오기
            while (RankSlotUsed[rankslot]) rankslot++; // 이미 사용한 슬롯이라면(즉, 공동 순위) 다음 슬롯으로
            RankSlotUsed[rankslot] = true;
            // 1. 플레이어 번호
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Upper_Right").GetComponent<Image>().sprite = PNoSp[i];
            // 2. 캐릭터 이미지
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex];
            // 3. 플레이어 이름
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex];
            // 4. (1등) 승자 이미지 / (2~4등) 최종 점수
            if(charinfo[i].rank > 1)
            {
                int score = charinfo[i].score;
                if (score < 10) score += 100; // 10의 자리 빈 칸 표시를 위한 땜방 조치
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
            }
            else
            {
                // ResultUI.transform.Find("Panel/WinnerChar").GetComponent<Image>().sprite = CharMarks[charinfo[i].charIndex]; 일단 미사용
            }
            // 5. (공동 순위 인경우) 테두리 색, 그라데이션 2개 색, 순위 이미지
            // 2,3등만 공동순위 가능
            if(charinfo[i].rank != rankslot)
            {
                Color c = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                if (charinfo[i].rank.Equals(3)) c = new Color(0.5f, 0.0f, 0.0f, 0.5f);
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Upper_Left").GetComponent<Image>().color = c;
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Lower_Right").GetComponent<Image>().color = c;
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Center/Edge").GetComponent<Image>().color = c;
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[charinfo[i].rank - 1];
            }
        }

        BD1SoundManager.instance.PlayBGM("Result");
        moveTween = ResultUI.transform.Find("Panel").GetComponent<RectTransform>().DOMoveY(540, 0.5f).SetEase(Ease.Linear); // 결과 패널 내려오게
        yield return moveTween.WaitForCompletion();
        yield return new WaitForSeconds(1.5f);
        ResultUI.transform.Find("Panel/Text").gameObject.SetActive(true);
    }
}
