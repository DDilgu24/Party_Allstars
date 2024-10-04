using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SelectSceneManager : MonoBehaviour
{
    private int page = 0; // 페이지 (0: 인원 수 선택 / 1: 보드 선택 / 2: 캐릭터 선택)
    private int SelectTurn = 0; // [캐릭터 선택 한정] 선택중인 플레이어의 번호(1~4)
    private int cursorIndex = 0; // 현재 선택중인 플레이어의 커서
    private int[] isSelected = new int[16]; // 캐릭터가 선택되었는지 여부(0: 미선택 / n: n번 플레이어가 선택)
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // 플레이어별 선택한 캐릭터의 인덱스(-1: 미선택)
    private int selectBoard; // 선택한 보드맵의 번호
    private int PlayerNum, COMNum;
    private int TotalNum => PlayerNum + COMNum; // 플레이어 수, COM 수

    private readonly string[] boardNames = 
    { 
        "마리오 로드", "뿌요 클리너", "단풍잎 축제", "팡랜드 서바이벌" 
    };
    private readonly string[] boardExplain = 
    { 
        "슈퍼 마리오의 코스 위에서 진행하는 보드입니다.\n가장 먼저 깃대를 잡는 플레이어가 <color=#999900>우승</color>합니다.",
        "같은 색 뿌요 4개를 모아 점수를 획득하세요.\n10턴 동안 얻은 점수가 가장 높은 플레이어가 <color=#999900>우승</color>합니다.",
        "단풍잎을 모아 NPC에게 황금단풍잎으로 교환하세요.\n10턴 동안 황금단풍잎을 가장 많이 모은 플레이어가 <color=#999900>우승</color>합니다.",
        "쏟아지는 물풍선을 피해 마지막까지 살아남으세요.\nHP가 0이 되면 보드에서 <color=#ff3333>탈락</color>합니다."
    };
    private readonly string[] Character_name = 
    {
        "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스",
        "팬텀", "메르세데스", "호영", "라라", "다오", "배찌", "디지니", "마리드"
    };
    public Color[] playerColor = // 플레이어 번호 별 컬러
    { 
        Color.gray, Color.blue, Color.red, Color.green, Color.yellow 
    }; 

    [SerializeField] private GameObject[] SelectPanel;
    [SerializeField] private GameObject[] cursor; // 0. 인원수 선택(0페이지) / 1~4. 캐릭터 선택(2페이지) / 5. 보드 선택(1페이지)
    [SerializeField] private Sprite[] PNoSp; // 플레이어 번호 스프라이트
    [SerializeField] private Text PlayerNumText, boardNameText, boardExplainText, charNameText;
    private bool inputBlocked = false; // 버그 방지용 입력 방지

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

    // 입력받은 방향키(정수로 표현됨)에 따라 커서의 인덱스를 바꾸는 메소드 
    private void CursorIndexChange(int n)
    {
        // 0페이지인 경우 : 플레이 인원수 선택
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
        // 1페이지 : 보드 선택
        else if (page.Equals(1))
        {
            if (n.Equals(1)) cursorIndex += 3; // Left
            else if (n.Equals(3)) cursorIndex += 1; // Right
            cursorIndex %= 4;
        }
        // 2페이지 : 캐릭터 선택
        else
        {
            if (SelectTurn > TotalNum) return; // 최종 확인 상태(모든 캐릭터 선택 완료): 방향키 무효화
            while (true) // 새로 가리킬 곳이 이미 선택된 캐릭터인 경우, 그 이동을 반복
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
            cursor[0].SetActive(true);
            cursor[0].GetComponent<RectTransform>().anchoredPosition = 
                new Vector3(
                    (cursorIndex % 4) * 450 + (cursorIndex / 4) * 50 - 750, 
                    350 - 400 * (cursorIndex / 4),
                    0
                );
            PlayerNumText.text = $"플레이어 수: {cursorIndex % 4 + 1}\nCOM의 수: {3 - (cursorIndex % 4) - (cursorIndex / 4)}";
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
        // tween 동작 중이라면 실행하지 않음
        if (DOTween.IsTweening(SelectPanel[page])) return;

        // 캐릭터 선택 페이지에서, 선택한 캐릭터가 1 이상
        if(SelectTurn > 1)
        {
            if(SelectTurn < TotalNum + 1) // 최종 결정여부 아니라면 -> 현재 플레이어의 커서 비활성화
                cursor[SelectTurn].SetActive(false); 
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
            if (page.Equals(0))
                MoveScene(0);
            else
            {
                // 1페이지였다면, 커서를 선택했던 인원 수에 따라서
                if (page.Equals(1))
                {
                    cursorIndex = 16 - (TotalNum) * 4 + PlayerNum - 1;
                }
                // 2페이지였다면, 커서를 선택했던 보드 위치로
                else
                {
                    SelectTurn = 0;
                    cursorIndex = selectBoard;
                }

                // 페이지 이동 효과
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
        // tween 동작 중이라면 안 먹도록
        if (DOTween.IsTweening(SelectPanel[page])) return;
        if (page < 2)
        {
            // 0페이지: 인원 수 선택 완료
            if (page.Equals(0))
            {
                PlayerNum = cursorIndex % 4 + 1;
                COMNum = 3 - cursorIndex % 4 - cursorIndex / 4;
            }
            // 1페이지: 보드 선택 완료
            else
            {
                selectBoard = cursorIndex;
                SelectTurn++;
                for (int i = 1; i <= 4; i++) 
                {
                    // 캐릭터 선택 시 커서를 P0 또는 COM으로
                    int isPlayer = (i <= PlayerNum) ? i : 0;
                    cursor[i].transform.Find("PlayerNo").gameObject.GetComponent<Image>().sprite = PNoSp[isPlayer];
                    // 커서 테두리 색깔도 변경
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
        // 2페이지: 캐릭터 선택에서 Next
        else
        {
            // 최종 결정 여부에서 다음 누른 경우
            if (SelectTurn > TotalNum)
            {
                GameManager.instance.SelectInfo(PlayerNum, COMNum, selectBoard, selectChar);
                MoveScene(1);
                return;
            }

            isSelected[cursorIndex] = SelectTurn + 1; // 그 플레이어가 선택한 캐릭터는 선택 상태로
            selectChar[SelectTurn] = cursorIndex; // 그 플레이어의 캐릭터를 지정
            cursor[SelectTurn].GetComponent<Animator>().SetTrigger("SelectOK"); // 애니메이션 종료
            cursor[SelectTurn].transform.Find("SelectOK").gameObject.SetActive(true); // OK 표시 활성화
            SelectTurn++;  // 다음 플레이어로

            // 총 인원수 만큼 캐릭터 선택 완료된 경우
            if (SelectTurn > TotalNum)
            {
                charNameText.text = "선택 완료! 준비 되었나요?";
            }

            // 그 이하로 선택 완료된 경우(계속 선택)
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
