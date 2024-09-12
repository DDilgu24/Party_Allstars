using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectSceneManager : MonoBehaviour
{
    private int page = 0; // ������(0: ���� ���� / 1: ĳ���� ����)
    private int SelectTurn = 0; // �������� �÷��̾��� ��ȣ(1~4)
    private int cursorIndex = 0; // ���� �������� �÷��̾��� Ŀ��
    private int[] isSelected = new int[16]; // ĳ���Ͱ� ���õǾ����� ����(0: �̼��� / n: n�� �÷��̾ ����)
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)
    private int selectBoard; // ������ ������� ��ȣ

    private string[] Board_name = new string[4] { "������ �ε�", "�ѿ� Ŭ����", "��ǳ�� ����", "�η��� �����̹�" };
    private string[] Board_explain = new string[4] { 
        "���� �������� �ڽ� ������ �����ϴ� �����Դϴ�.\n���� ���� ��븦 ��� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "���� �� �ѿ� 4���� ��� ������ ȹ���ϼ���.\n10�� ���� ���� ������ ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "��ǳ���� ��� NPC���� Ȳ�ݴ�ǳ������ ��ȯ�ϼ���.\n10�� ���� Ȳ�ݴ�ǳ���� ���� ���� ���� �÷��̾ <color=#999900>���</color>�մϴ�.",
        "������� ��ǳ���� ���� ���������� ��Ƴ�������.\nHP�� 0�� �Ǹ� ���忡�� <color=#ff3333>Ż��</color>�մϴ�."};
    private string[] Character_name = new string[16]
    { "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������" };
    [SerializeField] private GameObject MapSelectPanel;
    [SerializeField] private GameObject CharSelectPanel;
    [SerializeField] private GameObject[] cursor;
    [SerializeField] private Text boardNameText, boardExplainText, charNameText;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CursorIndexChange(0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CursorIndexChange(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CursorIndexChange(2);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CursorIndexChange(3);
        }
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            NextButton();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackButton();
        }
    }

    private void CursorIndexChange(int n)
    {
        if(page.Equals(0))
        {
            switch (n)
            {
                case 0:
                    break;
                case 1:
                    cursorIndex = (cursorIndex + 3) % 4;
                    break;
                case 2:
                    break;
                case 3:
                    cursorIndex = (cursorIndex + 1) % 4;
                    break;
            }
        }
        else
        {
            if (SelectTurn > 4) return; // 4 ĳ���� ���� �Ϸ� -> ���� Ȯ�� : ����Ű ��ȿȭ
            while (true)
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
                if (isSelected[cursorIndex].Equals(0)) break;
            }
        }
        CursorChange();
    }
    private void CursorChange()
    {
        if (page.Equals(0))
        {
            cursor[0].GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 4) * 450 - 750, 300, 0);
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
            if (page.Equals(0)) SceneManager.LoadScene("2. Main Menu");
            else
            {
                page--;
                SelectTurn = 0;
                cursorIndex = selectBoard;
                MapSelectPanel.SetActive(true);
                CharSelectPanel.SetActive(false);
                CursorChange();
            }
        }
    }

    public void NextButton()
    {
        if (page.Equals(0))
        {
            page++;
            SelectTurn++;
            selectBoard = cursorIndex;
            cursorIndex = 0;
            MapSelectPanel.SetActive(false);
            CharSelectPanel.SetActive(true);
            CursorChange();
        }
        else
        {
            if (SelectTurn > 4)
            {
                GameManager.instance.selectChar = selectChar;
                SceneManager.LoadScene("BD1. Mario Road");
                return;
            }
            isSelected[cursorIndex] = SelectTurn + 1; // �� �÷��̾ ������ ĳ���ʹ� ���� ���·�
            selectChar[SelectTurn] = cursorIndex; // �� �÷��̾��� ĳ���͸� ����
            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // �ִϸ��̼� ����
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK ǥ�� Ȱ��ȭ
            SelectTurn++;  // ���� �÷��̾��
            if (SelectTurn > 4)
            {
                Debug.Log("���� �Ϸ�");
                /*
                Debug.Log($"��: {Board_name[selectBoard]}");
                for (int i = 1; i <= 4; i++)
                {
                    Debug.Log($"{i}P : {Character_name[selectChar[i]]}");
                }
                */
            }
            else
            {
                cursor[SelectTurn].SetActive(true); // ���� �÷��̾��� Ŀ�� Ȱ��ȭ
                cursorIndex = 0;
                while (isSelected[cursorIndex] > 0) cursorIndex++; // �⺻ Ŀ�� ��ġ ����
                CursorChange();
            }
        }
    }
}
