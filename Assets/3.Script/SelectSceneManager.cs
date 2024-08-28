using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectSceneManager : MonoBehaviour
{
    private int SelectTurn = 0; // �������� �÷��̾��� ��ȣ
    private int cursorIndex = 0; // ���� �������� �÷��̾��� Ŀ��
    private int[] isSelected = new int[16]; // ĳ���Ͱ� ���õǾ����� ����(0: �̼��� / n: n�� �÷��̾ ����)
    private int[] selectChar = new int[4] { -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)

    private string[] Character_name = new string[16] 
    { "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������", 
        "����", "�޸�������", "ȣ��", "���", "����", "�ٿ�", "������", "������" };
    [SerializeField] private GameObject[] cursor;
    [SerializeField] private Text char_name;


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
        if (SelectTurn > 3) return;
        while(true)
        {
            switch(n)
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
        CursorChange();
    }
    private void CursorChange()
    {
        cursor[SelectTurn].GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 8) * 300 - 1050, 300 - (cursorIndex / 8) * 400 - (cursorIndex % 2) * 100, 0);
        char_name.text = Character_name[cursorIndex];
    }

    public void BackButton()
    {
        if(SelectTurn > 0)
        {
            if(SelectTurn < 4) cursor[SelectTurn].SetActive(false); // ���� �÷��̾��� Ŀ�� ��Ȱ��ȭ
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
            Debug.Log("�ڷ� ����(���� �� ����)");
        }
    }

    public void NextButton()
    {
        if (SelectTurn > 3)
        {
            Debug.Log("Ȯ�� �Ϸ�(���� �� ����)");
            return;
        }
        isSelected[cursorIndex] = SelectTurn + 1; // �� �÷��̾ ������ ĳ���ʹ� ���� ���·�
        selectChar[SelectTurn] = cursorIndex; // �� �÷��̾��� ĳ���͸� ����
        cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // �ִϸ��̼� ����
        cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK ǥ�� Ȱ��ȭ
        SelectTurn++;  // ���� �÷��̾��
        if(SelectTurn > 3)
        {
            Debug.Log("���� �Ϸ�");
            for (int i = 0; i < 4; i++)
            {
                Debug.Log(Character_name[selectChar[i]]);
            }
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
