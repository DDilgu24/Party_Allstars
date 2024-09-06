using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


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
    private CharInfo[] charinfo = new CharInfo[4];

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            charinfo[i] = new CharInfo(i + 1);
            CharUI[i].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex]; // 캐릭터 로고 설정
            CharUI[i].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[charinfo[i].rank - 1]; // 순위 이미지 설정
            CharUI[i].transform.Find("Upper_Right").GetComponent<Image>().sprite = PNoSp[charinfo[i].playerNO - 1]; // 플레이어 번호 이미지 설정
            CharUI[i].transform.Find("UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex]; // 플레이어 이름 설정
            CharacterMark.transform.Find($"{i + 1}P").GetComponent<SpriteRenderer>().sprite = CharMarks[charinfo[i].charIndex];
            ScoreSpriteSet(i, charinfo[i].score);
            RankSpriteSet(i, charinfo[i].rank);
        }
    }


    private void ScoreChanged(int pNo, int score)
    {
        charinfo[pNo].score = score;
        ScoreSpriteSet(pNo, score);

        // 순위 재계산 및 이미지 변경
        for (int i = 0; i < 4; i++)
        {
            int higherThanMe = 0;
            for (int j = 0; j < 4; j++)
            {
                if (charinfo[j].score > charinfo[i].score) higherThanMe++;
            }
            charinfo[i].rank = higherThanMe + 1;
            RankSpriteSet(i, higherThanMe + 1);
        }
    }


    private void ScoreSpriteSet(int pNo, int score)
    {
        // 점수 십의 자리 설정
        if (score < 10)
        {
            CharUI[pNo].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[10];
        }
        else
        {
            CharUI[pNo].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
        }
        // 점수 일의 자리 설정
        CharUI[pNo].transform.Find("Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
    }

    private void RankSpriteSet(int pNo, int rank)
    {
        // 순위 이미지 재설정
        CharUI[pNo].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[rank - 1];
        // 테두리, 그라데이션 색 재설정
        Color c = new Color(0.5f, 0.5f, 0.0f, 0.5f);
        if (rank.Equals(2)) c = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        else if (rank.Equals(3)) c = new Color(0.5f, 0.0f, 0.0f, 0.5f);
        else if (rank.Equals(4)) c = new Color(0.5f, 0.0f, 0.5f, 0.5f);
        CharUI[pNo].transform.Find("Upper_Left").GetComponent<Image>().color = c;
        CharUI[pNo].transform.Find("Lower_Right").GetComponent<Image>().color = c;
        CharUI[pNo].transform.Find("Center/Edge").GetComponent<Image>().color = c;
    }

    public void DebugAddScore(int pNo)
    {
        ScoreChanged(pNo, charinfo[pNo].score + 1);
    }
}
