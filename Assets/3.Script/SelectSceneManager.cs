using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectSceneManager : MonoBehaviour
{
    private int SelectTurn = 0; // 선택중인 플레이어의 번호
    private int cursorIndex = 0; // 현재 선택중인 플레이어의 커서
    private int[] isSelected = new int[16]; // 캐릭터가 선택되었는지 여부(0: 미선택 / n: n번 플레이어가 선택)
    private int[] selectChar = new int[4] { -1, -1, -1, -1 }; // 플레이어별 선택한 캐릭터의 인덱스(-1: 미선택)

    private string[] Character_name = new string[16] 
    { "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스", 
        "팬텀", "메르세데스", "호영", "라라", "배찌", "다오", "디지니", "마리드" };
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
            if(SelectTurn < 4) cursor[SelectTurn].SetActive(false); // 현재 플레이어의 커서 비활성화
            SelectTurn--;  // 이전 플레이어로 돌아감
            cursorIndex = selectChar[SelectTurn]; // 커서의 위치는 기존 선택한 캐릭터
            isSelected[selectChar[SelectTurn]] = 0; // 그 플레이어가 선택한 캐릭터는 미선택 상태로
            selectChar[SelectTurn] = -1; // 그 플레이어가 선택한 캐릭터를 미정(-1)
            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectCancel"); // 애니매이션 다시 재생
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(false); // OK 표시 비활성화
            CursorChange();
        }
        else
        {
            Debug.Log("뒤로 가기(아직 안 만듦)");
        }
    }

    public void NextButton()
    {
        if (SelectTurn > 3)
        {
            Debug.Log("확인 완료(아직 안 만듦)");
            return;
        }
        isSelected[cursorIndex] = SelectTurn + 1; // 그 플레이어가 선택한 캐릭터는 선택 상태로
        selectChar[SelectTurn] = cursorIndex; // 그 플레이어의 캐릭터를 지정
        cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // 애니매이션 종료
        cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK 표시 활성화
        SelectTurn++;  // 다음 플레이어로
        if(SelectTurn > 3)
        {
            Debug.Log("선택 완료");
            for (int i = 0; i < 4; i++)
            {
                Debug.Log(Character_name[selectChar[i]]);
            }
        }
        else
        {
            cursor[SelectTurn].SetActive(true); // 다음 플레이어의 커서 활성화
            cursorIndex = 0;
            while (isSelected[cursorIndex] > 0) cursorIndex++; // 기본 커서 위치 설정
            CursorChange();
        }
    }
}
