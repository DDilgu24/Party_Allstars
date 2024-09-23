using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectSceneManager : MonoBehaviour
{
    private int page = 0; // ������ (0: �ο� �� ���� / 1: ���� ���� / 2: ĳ���� ����)
    private int SelectTurn = 0; // [ĳ���� ���� ����] �������� �÷��̾��� ��ȣ(1~4)
    private int cursorIndex = 0; // ���� �������� �÷��̾��� Ŀ��
    private int[] isSelected = new int[16]; // ĳ���Ͱ� ���õǾ����� ����(0: �̼��� / n: n�� �÷��̾ ����)
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)
    private int selectBoard; // ������ ������� ��ȣ
    private int PlayerNum, COMNum; // �÷��̾� ��, COM ��

    private string[] Board_name = new string[4] { "������ �ε�", "�ѿ� Ŭ����", "��ǳ�� ����", "�η��� �����̹�" };
    private string[] Board_explain = new string[4] { 
        "���� �������� �ڽ� ������ �����ϴ� �����Դϴ�.\n���� ���� ��븦 ��� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "���� �� �ѿ� 4���� ��� ������ ȹ���ϼ���.\n10�� ���� ���� ������ ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "��ǳ���� ��� NPC���� Ȳ�ݴ�ǳ������ ��ȯ�ϼ���.\n10�� ���� Ȳ�ݴ�ǳ���� ���� ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "������� ��ǳ���� ���� ���������� ��Ƴ�������.\nHP�� 0�� �Ǹ� ���忡�� <color=#ff3333>Ż��</color>�մϴ�."};
    private string[] Character_name = new string[16]
    { "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������" };
    [SerializeField] private GameObject[] SelectPanel;
    [SerializeField] private GameObject[] cursor;
    [SerializeField] private Text PlayerNumText, boardNameText, boardExplainText, charNameText;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) CursorIndexChange(0);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) CursorIndexChange(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) CursorIndexChange(2);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) CursorIndexChange(3);
        else if(Input.GetKeyDown(KeyCode.Return)) NextButton();
        else if (Input.GetKeyDown(KeyCode.Escape)) BackButton();
    }

    // �Է¹��� ����Ű(������ ǥ����)�� ���� Ŀ���� �ε����� �ٲٴ� �޼ҵ� 
    private void CursorIndexChange(int n)
    {
        // 0������ : �÷��� �ο��� ����
        if (page.Equals(0))
        {
            switch (n)
            {
                case 0:
                    if (cursorIndex % 4 < 2) cursorIndex = (cursorIndex + 8) % 12;
                    else if (cursorIndex % 4 == 2) cursorIndex = (cursorIndex + 4) % 8;
                    break;
                case 1:
                    if (cursorIndex % 4 > 0) cursorIndex--;
                    else cursorIndex += (3 - cursorIndex / 4);
                    break;
                case 2:
                    if (cursorIndex % 4 < 2) cursorIndex = (cursorIndex + 4) % 12;
                    else if (cursorIndex % 4 == 2) cursorIndex = (cursorIndex + 4) % 8;
                    break;
                case 3:
                    if (cursorIndex % 3 > 0 || cursorIndex.Equals(0)) cursorIndex++;
                    else cursorIndex = cursorIndex * 4 / 3 - 4;
                    break;
            }
        }
        // 1������ : ���� ����
        else if (page.Equals(1))
        {
            if (n.Equals(1)) cursorIndex += 3;
            else if (n.Equals(3)) cursorIndex += 1;
            cursorIndex %= 4;
        }
        // 2������ : ĳ���� ����
        else
        {
            if (SelectTurn > 4) return; // 4 ĳ���� ���� �Ϸ� -> ���� Ȯ�� : ����Ű ��ȿȭ
            while (true) // ���� ����ų ���� �̹� ���õ� ĳ������ ���, �̵��� �ݺ�
            {
                switch (n)
                {
                    case 0:
                        cursorIndex -= 8;
                        if (cursorIndex < 0) cursorIndex += 16;
                        break;
                    case 1:
                        if ((cursorIndex % 8).Equals(0)) cursorIndex += 8;
                        cursorIndex -= 1;
                        break;
                    case 2:
                        cursorIndex += 8;
                        if (cursorIndex > 15) cursorIndex -= 16;
                        break;
                    case 3:
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
            cursor[0].GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 4) * 450 + (cursorIndex / 4) * 50 - 750, 350 - 400 * (cursorIndex / 4), 0);
            PlayerNumText.text = $"�÷��̾� ��: {cursorIndex % 4 + 1}\nCOM�� ��: {3 - (cursorIndex % 4) - (cursorIndex / 4)}";
        }
        else if (page.Equals(1))
        {
            cursor[5].GetComponent<RectTransform>().anchoredPosition = new Vector3(cursorIndex * 450 - 750, 300, 0);
            boardNameText.text = Board_name[cursorIndex];
            boardExplainText.text = Board_explain[cursorIndex];
        }
        else
        {
            cursor[SelectTurn].GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 8) * 300 - 1050, 300 - (cursorIndex / 8) * 400 - (cursorIndex % 2) * 100, 0);
            charNameText.text = Character_name[cursorIndex];
        }
    }

    public void BackButton()
    {
        // ĳ���� ���� ����������, ������ ĳ���Ͱ� 1 �̻�
        if(SelectTurn > 1)
        {
            if(SelectTurn < 5) cursor[SelectTurn].SetActive(false); // ���� �÷��̾��� Ŀ�� ��Ȱ��ȭ
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
            if (page.Equals(0)) MoveScene(0);
            else
            {
                if (page.Equals(1))
                {
                    cursorIndex = 16 - (PlayerNum + COMNum) * 4 + PlayerNum - 1;
                }
                else
                {
                    SelectTurn = 0;
                    cursorIndex = selectBoard;
                }
                page--;
                SelectPanel[page].SetActive(true);
                SelectPanel[page + 1].SetActive(false);
                CursorChange();
            }
        }
    }

    public void NextButton()
    {
        // �ο� �� ���� �Ǵ� ���� ���ÿ��� Next
        if (page < 2)
        {
            if (page.Equals(0))
            {
                PlayerNum = cursorIndex % 4 + 1;
                COMNum = 3 - cursorIndex % 4 - cursorIndex / 4;
            }
            else
            {
                selectBoard = cursorIndex;
                SelectTurn++;
            }
            page++;
            cursorIndex = 0;
            SelectPanel[page - 1].SetActive(false);
            SelectPanel[page].SetActive(true);
            CursorChange();
        }
        // ĳ���� ���ÿ��� Next
        else
        {
            // ���� ���� �Ϸ��� ���
            if (SelectTurn > 4)
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

            // 4ĳ���� ���� �Ϸ�� ���
            if (SelectTurn > 4)
            {
                charNameText.text = "���� �Ϸ�! �غ� �Ǿ�����?";
                /*
                Debug.Log($"��: {Board_name[selectBoard]}");
                for (int i = 1; i <= 4; i++)
                {
                    Debug.Log($"{i}P : {Character_name[selectChar[i]]}");
                }
                */
            }

            // 1~3 ĳ���͸� ���� �Ϸ�� ���
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
