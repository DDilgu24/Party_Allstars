using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;


public class CharInfo
{
    public int playerNO; // �÷��̾� ��ȣ (1~4: �÷��̾� / 5~8: CPU)
    public int charIndex; // ĳ������ �ε��� (0~15) 

    public int rank; // ���� ���� (1~4)
    public int[] items = new int[3]; // ������ ������ 
    public int itemCount = 0;
    public int score; // ���� [���� 4�� ��� HP]

    public CharInfo(int pNo)
    {
        playerNO = pNo;
        try { charIndex = GameManager.instance.selectChar[playerNO]; }
        catch (Exception) { charIndex = playerNO; }

        rank = 1;
        for (int i = 0; i < 3; i++) items[i] = -1;
        score = 0;
    }

    public void GetItem(int itemNo)
    {
        if (itemCount < 3) items[itemCount++] = itemNo;
    }
    public bool UseItem(int itemindex)
    {
        if (items[itemindex] < 0) return false;
        items[itemindex] = -1;
        for (int i = itemindex; i < itemCount - 1; i++)
        {
            items[i] = items[i + 1];
            items[i + 1] = -1;
        }
        itemCount--;
        return true;
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
    [SerializeField] private Sprite[] DiceEdgeSp; // �ֻ��� ��� ��������Ʈ
    [SerializeField] private Sprite[] NumSp; // ���� ǥ��� ���� ��������Ʈ
    public CharInfo[] charinfo = new CharInfo[4];
    [SerializeField] public GameObject ResultUI; // ��� UI
    public int N;

    // 0. �̱��� ����
    public static CharInfoManager instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        DOTween.Init();
        N = GameManager.instance.Total_Num;
        for (int i = 0; i < 4; i++)
        {
            if (i < N)
            {
                int pno = i + 1;
                int ifCOMthat0 = (pno > GameManager.instance.Player_Num) ? 0 : 1;
                charinfo[i] = new CharInfo(pno);
                CharUI[i].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex]; // ĳ���� �ΰ� ����
                CharUI[i].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[charinfo[i].rank - 1]; // ���� �̹��� ����
                CharUI[i].transform.Find("Upper_Right").GetComponent<Image>().sprite = PNoSp[charinfo[i].playerNO * ifCOMthat0]; // �÷��̾� ��ȣ �̹��� ����
                CharUI[i].transform.Find("UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex]; // �÷��̾� �̸� ����
                CharacterMark.transform.Find($"{i + 1}P").GetComponent<SpriteRenderer>().sprite = CharMarks[charinfo[i].charIndex];
                ScoreSpriteSet(i + 1, charinfo[i].score);
                RankSpriteSet(i + 1, charinfo[i].rank);
            }
            else
            {
                CharUI[i].transform.position = new Vector3(0, 1080, 0); // �� ���̴� ������ ���ܹ�����
            }
        }
    }


    public void ScoreAdd(int pNo, int addscore = 1)
    {
        charinfo[pNo-1].score += addscore;
        ScoreSpriteSet(pNo, charinfo[pNo-1].score);

        // ���� ���� �� �̹��� ����
        for (int i = 0; i < N; i++)
        {
            int higherThanMe = 0;
            for (int j = 0; j < N; j++)
            {
                if (charinfo[j].score > charinfo[i].score) higherThanMe++;
            }
            charinfo[i].rank = higherThanMe + 1;
            RankSpriteSet(i+1, higherThanMe + 1);
        }
    }


    private void ScoreSpriteSet(int pNo, int score)
    {
        // ���� ���� �ڸ� ����
        if (score < 10)
        {
            CharUI[pNo-1].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[10];
        }
        else
        {
            CharUI[pNo-1].transform.Find("Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
        }
        // ���� ���� �ڸ� ����
        CharUI[pNo-1].transform.Find("Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
    }

    private void RankSpriteSet(int pNo, int rank)
    {
        // ���� �̹��� �缳��
        CharUI[pNo-1].transform.Find("Upper_Left/Rank").GetComponent<Image>().sprite = RankSp[rank - 1];
        // �׵θ�, �׶��̼� �� �缳��
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
        Tween moveTween = ResultUI.transform.Find("Superstar").DOScale(Vector3.one * 2, 1f).SetEase(Ease.OutBack); // ���۽�Ÿ �ΰ� �߰�
        yield return new WaitForSeconds(8f); // �ӽ�: �¸� ȿ���� ������ ���� ���
        ResultUI.transform.Find("Text").gameObject.SetActive(true); // �ȳ� ���� Ȱ��ȭ
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space)); // �����̽��� ������
        ResultUI.transform.Find("Text").gameObject.SetActive(false); // �ȳ� ���� ��Ȱ��ȭ
        ResultUI.transform.Find("Superstar").gameObject.SetActive(false); // ���۽�Ÿ �ΰ� ��Ȱ��ȭ

        bool[] RankSlotUsed = new bool[5] { false, false, false, false, false }; // �ش� ������ ĳ���Ͱ� �� �ִ���? - ���� ������ ���
        for (int i = 0; i < 4; i++) // 1P ���� ������ ã�� �ű⿡ ����ֱ�
        {
            if(i >= N)
            {
                // �ο� ������ ū ���� ���� ������ �� ���̴� ������ ����������
                // ResultUI.transform.Find($"Panel/Rank{i + 1}_info").transform.position = new Vector3(0, 2160, 0);
                ResultUI.transform.Find($"Panel/Rank{i + 1}_info").gameObject.SetActive(false);
                continue;
            }
            int rankslot = charinfo[i].rank; // �� ������ ������ ���� �ҷ�����
            while (RankSlotUsed[rankslot]) rankslot++; // �̹� ����� �����̶��(��, ���� ����) ���� ��������
            RankSlotUsed[rankslot] = true;
            // 1. �÷��̾� ��ȣ
            int ifCOMthat0 = (i + 1 > GameManager.instance.Player_Num) ? 0 : 1;
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Upper_Right").GetComponent<Image>().sprite = PNoSp[charinfo[i].playerNO * ifCOMthat0];
            // 2. ĳ���� �̹���
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[charinfo[i].charIndex];
            // 3. �÷��̾� �̸�
            ResultUI.transform.Find($"Panel/Rank{rankslot}_info/UserName").GetComponent<Text>().text = GameManager.instance.Character_name[charinfo[i].charIndex];
            // 4. (1��) ���� �̹��� / (2~4��) ���� ����
            if(charinfo[i].rank > 1)
            {
                int score = charinfo[i].score;
                if (score < 10) score += 100; // 10�� �ڸ� �� ĭ ǥ�ø� ���� ���� ��ġ
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Lower_Right/Score_10_Digit").GetComponent<Image>().sprite = NumSp[score / 10];
                ResultUI.transform.Find($"Panel/Rank{rankslot}_info/Lower_Right/Score_1_Digit").GetComponent<Image>().sprite = NumSp[score % 10];
            }
            else
            {
                // ResultUI.transform.Find("Panel/WinnerChar").GetComponent<Image>().sprite = CharMarks[charinfo[i].charIndex]; �ϴ� �̻��
            }
            // 5. (���� ���� �ΰ��) �׵θ� ��, �׶��̼� 2�� ��, ���� �̹���
            // 2,3� �������� ����
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
        moveTween = ResultUI.transform.Find("Panel").GetComponent<RectTransform>().DOMoveY(540, 0.5f).SetEase(Ease.Linear); // ��� �г� ��������
        yield return moveTween.WaitForCompletion();
        yield return new WaitForSeconds(1.5f);
        ResultUI.transform.Find("Panel/Text").gameObject.SetActive(true);
    }

    // ���� 1���� ������ ã�� �޼ҵ�
    public int Score1st()
    {
        int max = 0;
        for (int i = 0; i < 4; i++) max = Mathf.Max(max, charinfo[i].score);
        return max;
    }
}
