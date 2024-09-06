using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class CharInfo
{
    public int playerNO; // �÷��̾� ��ȣ (1~4: �÷��̾� / 5~8: CPU)
    public int charIndex; // ĳ������ �ε��� (0~15) 

    public int rank; // ���� ���� (1~4)
    public int[] items = new int[3]; // ������ ������ 
    public int score; // ���� [���� 4�� ��� HP]

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
    [SerializeField] private GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] private GameObject CharacterMark; // ĳ���� ��ġ ǥ�� ������Ʈ
    [SerializeField] private Sprite[] CharLogo; // ĳ���� �ΰ� ��������Ʈ
    [SerializeField] private Sprite[] CharMarks; // ĳ���� ��ġ ǥ�� ��������Ʈ
    [SerializeField] private Sprite[] RankSp; // ���� ��������Ʈ
    [SerializeField] private Sprite[] PNoSp; // �÷��̾� ��ȣ ��������Ʈ
    [SerializeField] private Sprite[] ItemSp; // ������ ��������Ʈ
    [SerializeField] private Sprite[] NumSp; // ���� ǥ��� ���� ��������Ʈ
    private CharInfo[] charinfo = new CharInfo[4];

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            charinfo[i] = new CharInfo(i + 1);
            CharUI[i].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex]; // ĳ���� �ΰ� ����
            CharUI[i].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[charinfo[i].rank - 1]; // ���� �̹��� ����
            CharUI[i].transform.Find("Upper_Right").GetComponent<Image>().sprite = PNoSp[charinfo[i].playerNO - 1]; // �÷��̾� ��ȣ �̹��� ����
            CharUI[i].transform.Find("UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex]; // �÷��̾� �̸� ����
            CharacterMark.transform.Find($"{i + 1}P").GetComponent<SpriteRenderer>().sprite = CharMarks[charinfo[i].charIndex];
            ScoreSpriteSet(i, charinfo[i].score);
            RankSpriteSet(i, charinfo[i].rank);
        }
    }


    private void ScoreChanged(int pNo, int score)
    {
        charinfo[pNo].score = score;
        ScoreSpriteSet(pNo, score);

        // ���� ���� �� �̹��� ����
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
        // ���� ���� �ڸ� ����
        if (score < 10)
        {
            CharUI[pNo].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[10];
        }
        else
        {
            CharUI[pNo].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
        }
        // ���� ���� �ڸ� ����
        CharUI[pNo].transform.Find("Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
    }

    private void RankSpriteSet(int pNo, int rank)
    {
        // ���� �̹��� �缳��
        CharUI[pNo].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[rank - 1];
        // �׵θ�, �׶��̼� �� �缳��
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
