using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectSceneManager : MonoBehaviour
{
    private int page = 0; // 페이지 (0: 인원 수 선택 / 1: 보드 선택 / 2: 캐릭터 선택)
    private int SelectTurn = 0; // [캐릭터 선택 한정] 선택중인 플레이어의 번호(1~4)
    private int cursorIndex = 0; // 현재 선택중인 플레이어의 커서
    private int[] isSelected = new int[16]; // 캐릭터가 선택되었는지 여부(0: 미선택 / n: n번 플레이어가 선택)
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // 플레이어별 선택한 캐릭터의 인덱스(-1: 미선택)
    private int selectBoard; // 선택한 보드맵의 번호
    private int PlayerNum, COMNum; // 플레이어 수, COM 수

    private string[] Board_name = new string[4] { "마리오 로드", "뿌요 클리너", "단풍잎 축제", "팡랜드 서바이벌" };
    private string[] Board_explain = new string[4] { 
        "슈퍼 마리오의 코스 위에서 진행하는 보드입니다.\n가장 먼저 깃대를 잡는 플레이어가 <color=#999900>우승</color>합니다.",
        "같은 색 뿌요 4개를 모아 점수를 획득하세요.\n10턴 동안 얻은 점수가 가장 높은 플레이어가 <color=#999900>우승</color>합니다.",
        "단풍잎을 모아 NPC에게 황금단풍잎으로 교환하세요.\n10턴 동안 황금단풍잎을 가장 많이 모은 플레이어가 <color=#999900>우승</color>합니다.",
        "쏟아지는 물풍선을 피해 마지막까지 살아남으세요.\nHP가 0이 되면 보드에서 <color=#ff3333>탈락</color>합니다."};
    private string[] Character_name = new string[16]
    { "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스",
        "팬텀", "메르세데스", "호영", "라라", "다오", "배찌", "디지니", "마리드" };
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

    // 입력받은 방향키(정수로 표현됨)에 따라 커서의 인덱스를 바꾸는 메소드 
    private void CursorIndexChange(int n)
    {
        // 0페이지 : 플레이 인원수 선택
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
        // 1페이지 : 보드 선택
        else if (page.Equals(1))
        {
            if (n.Equals(1)) cursorIndex += 3;
            else if (n.Equals(3)) cursorIndex += 1;
            cursorIndex %= 4;
        }
        // 2페이지 : 캐릭터 선택
        else
        {
            if (SelectTurn > 4) return; // 4 캐릭터 선택 완료 -> 최종 확인 : 방향키 무효화
            while (true) // 새로 가리킬 곳이 이미 선택된 캐릭터인 경우, 이동을 반복
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
                if (isSelected[cursorIndex].Equals(0)) break; // 선택 안 된 캐릭터 도달 시, 반복 종료
            }
        }
        CursorChange(); // 커서 이미지를 인덱스에 맞게 이동
    }

    // 인덱스에 따라 커서 이미지를 바꾸는 메소드
    private void CursorChange()
    {
        if (page.Equals(0))
        {
            cursor[0].GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 4) * 450 + (cursorIndex / 4) * 50 - 750, 350 - 400 * (cursorIndex / 4), 0);
            PlayerNumText.text = $"플레이어 수: {cursorIndex % 4 + 1}\nCOM의 수: {3 - (cursorIndex % 4) - (cursorIndex / 4)}";
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
        // 캐릭터 선택 페이지에서, 선택한 캐릭터가 1 이상
        if(SelectTurn > 1)
        {
            if(SelectTurn < 5) cursor[SelectTurn].SetActive(false); // 현재 플레이어의 커서 비활성화
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
            // 0페이지 였으면, 메인 메뉴로
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
        // 인원 수 선택 또는 보드 선택에서 Next
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
        // 캐릭터 선택에서 Next
        else
        {
            // 최종 선택 완료한 경우
            if (SelectTurn > 4)
            {
                GameManager.instance.SelectInfo(PlayerNum, COMNum, selectBoard, selectChar);
                MoveScene(1);
                return;
            }

            isSelected[cursorIndex] = SelectTurn + 1; // 그 플레이어가 선택한 캐릭터는 선택 상태로
            selectChar[SelectTurn] = cursorIndex; // 그 플레이어의 캐릭터를 지정
            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // 애니매이션 종료
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK 표시 활성화
            SelectTurn++;  // 다음 플레이어로

            // 4캐릭터 선택 완료된 경우
            if (SelectTurn > 4)
            {
                charNameText.text = "선택 완료! 준비 되었나요?";
                /*
                Debug.Log($"맵: {Board_name[selectBoard]}");
                for (int i = 1; i <= 4; i++)
                {
                    Debug.Log($"{i}P : {Character_name[selectChar[i]]}");
                }
                */
            }

            // 1~3 캐릭터만 선택 완료된 경우
            else
            {
                cursor[SelectTurn].SetActive(true); // 다음 플레이어의 커서 활성화
                cursorIndex = 0;
                while (isSelected[cursorIndex] > 0) cursorIndex++; // 기본 커서 위치 설정
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
