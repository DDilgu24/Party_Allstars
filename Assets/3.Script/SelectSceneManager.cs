using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SelectSceneManager : MonoBehaviour
{
    private int page = 0; // ������ (0: �ο� �� ���� / 1: ���� ���� / 2: ĳ���� ����)
    private int SelectTurn = 0; // [ĳ���� ���� ����] �������� �÷��̾��� ��ȣ(1~4)
    private int cursorIndex = 0; // ���� �������� �÷��̾��� Ŀ��
    private int[] isSelected = new int[16]; // ĳ���Ͱ� ���õǾ����� ����(0: �̼��� / n: n�� �÷��̾ ����)
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)
    private int selectBoard; // ������ ������� ��ȣ
    private int PlayerNum, COMNum;
    private int TotalNum => PlayerNum + COMNum; // �÷��̾� ��, COM ��

    private readonly string[] boardNames = 
    { 
        "������ �ε�", "�ѿ� Ŭ����", "��ǳ�� ����", "�η��� �����̹�" 
    };
    private readonly string[] boardExplain = 
    { 
        "���� �������� �ڽ� ������ �����ϴ� �����Դϴ�.\n���� ���� ��븦 ��� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "���� �� �ѿ� 4���� ��� ������ ȹ���ϼ���.\n10�� ���� ���� ������ ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "��ǳ���� ��� NPC���� Ȳ�ݴ�ǳ������ ��ȯ�ϼ���.\n10�� ���� Ȳ�ݴ�ǳ���� ���� ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "������� ��ǳ���� ���� ���������� ��Ƴ�������.\nHP�� 0�� �Ǹ� ���忡�� <color=#ff3333>Ż��</color>�մϴ�."
    };
    private readonly string[] Character_name = 
    {
        "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������"
    };
    public Color[] playerColor = // �÷��̾� ��ȣ �� �÷�
    { 
        Color.gray, Color.blue, Color.red, Color.green, Color.yellow 
    }; 

    [SerializeField] private GameObject[] SelectPanel;
    [SerializeField] private GameObject[] cursor; // 0. �ο��� ����(0������) / 1~4. ĳ���� ����(2������) / 5. ���� ����(1������)
    [SerializeField] private Sprite[] PNoSp; // �÷��̾� ��ȣ ��������Ʈ
    [SerializeField] private Text PlayerNumText, boardNameText, boardExplainText, charNameText;
    private bool inputBlocked = false; // ���� ������ �Է� ����

    private void Update()
    {
        if (inputBlocked) return;
        if (Input.GetKeyDown(KeyCode.UpArrow)) CursorIndexChange(0);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) CursorIndexChange(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) CursorIndexChange(2);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) CursorIndexChange(3);
        else if (Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.IsFading) NextButton();
        else if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.instance.IsFading) BackButton();
    }

    // �Է¹��� ����Ű(������ ǥ����)�� ���� Ŀ���� �ε����� �ٲٴ� �޼ҵ� 
    private void CursorIndexChange(int n)
    {
        // 0�������� ��� : �÷��� �ο��� ����
        if (page.Equals(0))
        {
            switch (n)
            {
                case 0: // Up
                    if (cursorIndex % 4 < 2) cursorIndex = (cursorIndex + 8) % 12;
                    else if (cursorIndex % 4 == 2) cursorIndex = (cursorIndex + 4) % 8;
                    break;
                case 1: // Left
                    if (cursorIndex % 4 > 0) cursorIndex--;
                    else cursorIndex += (3 - cursorIndex / 4);
                    break;
                case 2: // Down
                    if (cursorIndex % 4 < 2) cursorIndex = (cursorIndex + 4) % 12;
                    else if (cursorIndex % 4 == 2) cursorIndex = (cursorIndex + 4) % 8;
                    break;
                case 3: // Right
                    if (cursorIndex % 3 > 0 || cursorIndex.Equals(0)) cursorIndex++;
                    else cursorIndex = cursorIndex * 4 / 3 - 4;
                    break;
            }
        }
        // 1������ : ���� ����
        else if (page.Equals(1))
        {
            if (n.Equals(1)) cursorIndex += 3; // Left
            else if (n.Equals(3)) cursorIndex += 1; // Right
            cursorIndex %= 4;
        }
        // 2������ : ĳ���� ����
        else
        {
            if (SelectTurn > TotalNum) return; // ���� Ȯ�� ����(��� ĳ���� ���� �Ϸ�): ����Ű ��ȿȭ
            while (true) // ���� ����ų ���� �̹� ���õ� ĳ������ ���, �� �̵��� �ݺ�
            {
                switch (n)
                {
                    case 0: // Up
                        cursorIndex -= 8;
                        if (cursorIndex < 0) cursorIndex += 16;
                        break;
                    case 1: // Left
                        if ((cursorIndex % 8).Equals(0)) cursorIndex += 8;
                        cursorIndex -= 1;
                        break;
                    case 2: // Down
                        cursorIndex += 8;
                        if (cursorIndex > 15) cursorIndex -= 16;
                        break;
                    case 3: // Right
                        cursorIndex += 1;
                        if ((cursorIndex % 8).Equals(0)) cursorIndex -= 8;
                        break;
                }
                if (isSelected[cursorIndex].Equals(0)) break; // ���� �� �� ĳ���� ���� ��, �ݺ� ����
            }
        }
        CursorChange(); // Ŀ�� �̹����� �ε����� �°� �̵�
    }

    // �ε����� ���� Ŀ�� �̹����� �ٲٴ� �޼ҵ�
    private void CursorChange()
    {
        if (page.Equals(0))
        {
            cursor[0].SetActive(true);
            cursor[0].GetComponent<RectTransform>().anchoredPosition = 
                new Vector3(
                    (cursorIndex % 4) * 450 + (cursorIndex / 4) * 50 - 750, 
                    350 - 400 * (cursorIndex / 4),
                    0
                );
            PlayerNumText.text = $"�÷��̾� ��: {cursorIndex % 4 + 1}\nCOM�� ��: {3 - (cursorIndex % 4) - (cursorIndex / 4)}";
        }
        else if (page.Equals(1))
        {
            cursor[5].SetActive(true);
            cursor[5].GetComponent<RectTransform>().anchoredPosition = 
                new Vector3(cursorIndex * 450 - 750, 300, 0);
            boardNameText.text = boardNames[cursorIndex];
            boardExplainText.text = boardExplain[cursorIndex];
        }
        else
        {
            cursor[SelectTurn].SetActive(true);
            cursor[SelectTurn].GetComponent<RectTransform>().anchoredPosition = 
                new Vector3(
                    (cursorIndex % 8) * 300 - 1050,
                    300 - (cursorIndex / 8) * 400 - (cursorIndex % 2) * 100,
                    0
                );
            charNameText.text = Character_name[cursorIndex];
        }
    }

    public void BackButton()
    {
        // tween ���� ���̶�� �������� ����
        if (DOTween.IsTweening(SelectPanel[page])) return;

        // ĳ���� ���� ����������, ������ ĳ���Ͱ� 1 �̻�
        if(SelectTurn > 1)
        {
            if(SelectTurn < TotalNum + 1) // ���� �������� �ƴ϶�� -> ���� �÷��̾��� Ŀ�� ��Ȱ��ȭ
                cursor[SelectTurn].SetActive(false); 
            SelectTurn--;  // ���� �÷��̾�� ���ư�
            cursorIndex = selectChar[SelectTurn]; // Ŀ���� ��ġ�� ���� ������ ĳ����
            isSelected[selectChar[SelectTurn]] = 0; // �� �÷��̾ ������ ĳ���ʹ� �̼��� ���·�
            selectChar[SelectTurn] = -1; // �� �÷��̾ ������ ĳ���͸� ����(-1)

            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectCancel"); // �ִϸ��̼� �ٽ� ���
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(false); // OK ǥ�� ��Ȱ��ȭ
            CursorChange();
        }

        else
        {
            // 0������ ������, ���� �޴���
            if (page.Equals(0))
                MoveScene(0);
            else
            {
                // 1���������ٸ�, Ŀ���� �����ߴ� �ο� ���� ����
                if (page.Equals(1))
                {
                    cursorIndex = 16 - (TotalNum) * 4 + PlayerNum - 1;
                }
                // 2���������ٸ�, Ŀ���� �����ߴ� ���� ��ġ��
                else
                {
                    SelectTurn = 0;
                    cursorIndex = selectBoard;
                }

                // ������ �̵� ȿ��
                page--;
                cursor[page * 5].SetActive(false);
                SelectPanel[page].SetActive(true);
                SelectPanel[page].transform.localPosition = new Vector3(-1920, 0, 0);
                inputBlocked = true;

                Sequence seq = DOTween.Sequence()
                 .Append(SelectPanel[page].transform.DOLocalMoveX(0, 0.5f))
                 .Join(SelectPanel[page + 1].transform.DOLocalMoveX(1920, 0.5f))
                 .OnComplete(() =>
                 {
                     SelectPanel[page + 1].SetActive(false);
                     CursorChange();
                     inputBlocked = false;
                 });
            }
        }
    }

    public void NextButton()
    {
        // tween ���� ���̶�� �� �Ե���
        if (DOTween.IsTweening(SelectPanel[page])) return;
        if (page < 2)
        {
            // 0������: �ο� �� ���� �Ϸ�
            if (page.Equals(0))
            {
                PlayerNum = cursorIndex % 4 + 1;
                COMNum = 3 - cursorIndex % 4 - cursorIndex / 4;
            }
            // 1������: ���� ���� �Ϸ�
            else
            {
                selectBoard = cursorIndex;
                SelectTurn++;
                for (int i = 1; i <= 4; i++) 
                {
                    // ĳ���� ���� �� Ŀ���� P0 �Ǵ� COM����
                    int isPlayer = (i <= PlayerNum) ? i : 0;
                    cursor[i].transform.Find("PlayerNo").gameObject.GetComponent<Image>().sprite = PNoSp[isPlayer];
                    // Ŀ�� �׵θ� ���� ����
                    cursor[i].transform.Find("Center/Edge").gameObject.GetComponent<Image>().color = playerColor[isPlayer];
                }
            }
            page++;
            cursor[(page * 5) % 9].SetActive(false);
            cursorIndex = 0;
            SelectPanel[page].SetActive(true);
            SelectPanel[page].transform.localPosition = new Vector3(1920, 0, 0);
            inputBlocked = true;

            Sequence seq = DOTween.Sequence()
             .Append(SelectPanel[page].transform.DOLocalMoveX(0, 0.5f))
             .Join(SelectPanel[page - 1].transform.DOLocalMoveX(-1920, 0.5f))
             .OnComplete(() =>
             {
                 SelectPanel[page - 1].SetActive(false);
                 CursorChange();
                 inputBlocked = false;
             });

        }
        // 2������: ĳ���� ���ÿ��� Next
        else
        {
            // ���� ���� ���ο��� ���� ���� ���
            if (SelectTurn > TotalNum)
            {
                GameManager.instance.SelectInfo(PlayerNum, COMNum, selectBoard, selectChar);
                MoveScene(1);
                return;
            }

            isSelected[cursorIndex] = SelectTurn + 1; // �� �÷��̾ ������ ĳ���ʹ� ���� ���·�
            selectChar[SelectTurn] = cursorIndex; // �� �÷��̾��� ĳ���͸� ����
            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // �ִϸ��̼� ����
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK ǥ�� Ȱ��ȭ
            SelectTurn++;  // ���� �÷��̾��

            // �� �ο��� ��ŭ ĳ���� ���� �Ϸ�� ���
            if (SelectTurn > TotalNum)
            {
                charNameText.text = "���� �Ϸ�! �غ� �Ǿ�����?";
            }

            // �� ���Ϸ� ���� �Ϸ�� ���(��� ����)
            else
            {
                cursor[SelectTurn].SetActive(true); // ���� �÷��̾��� Ŀ�� Ȱ��ȭ
                cursorIndex = 0;
                while (isSelected[cursorIndex] > 0) cursorIndex++; // �⺻ Ŀ�� ��ġ ����
                CursorChange();
            }
        }
    }

    public void MoveScene(int n)
    {
        GameManager.instance.FadeOut(() =>
        {
            string s;
            switch(n)
            {
                case 1:
                    s = "BD1. Mario Road";
                    break;
                default:
                    s = "2. Main Menu";
                    break;
            }
            SceneManager.LoadScene(s);
            GameManager.instance.FadeIn();
        });
    }
}
