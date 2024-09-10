using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// [상태 패턴] 3단계: 상태별 구현
// 3-1. 순서 알림 상태
public class OrderAlertState : Board_Data, IGameState
{
    public Image TurnAlertEdge, TurnAlertGra, TurnAlertpNo, TurnAlertChar;
    Board_Data b = Board_Manager.instance.GetComponent<Board_Data>();
    private void Awake()
    {
        TurnAlertEdge = b.TurnAlert.transform.Find("Center/Edge").GetComponent<Image>();
        TurnAlertGra = b.TurnAlert.transform.Find("Right").GetComponent<Image>();
        TurnAlertpNo = b.TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>();
        TurnAlertChar = b.TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>();
    }
    public void EnterState(GameContext context)
    {
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        Board_Manager.instance.SetCameraTarget(b.order[b.orderPointer] - 1);
        // 2. 현재 차례가 누군지 알려주는 UI
        Vector3 v = new Vector3(1125, -25, 0);
        b.TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        // 테두리, 그라데이션 색깔 + pNo + 캐릭터 로고 4개 바꾸기
        TurnAlertEdge.color = b.playerColor[b.order[b.orderPointer]];
        TurnAlertGra.color = b.playerColor[b.order[b.orderPointer]];
        TurnAlertpNo.sprite = b.playerNum[b.order[b.orderPointer]];
        TurnAlertChar.sprite = b.CharUI[b.order[b.orderPointer] - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        v = new Vector3(-125, -25, 0);
        b.TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }

    public void UpdateState(GameContext context)
    {
        // 스페이스바 누르면 상태 업데이트 되게
        if (Input.GetKeyDown(KeyCode.Space))
        {
            context.SetState(new RollDiceState());
        }
    }

    public void ExitState(GameContext context)
    {
        // 턴 알림 UI 왼쪽으로 가면서 사라지게
        Vector3 v = new Vector3(-1125, -25, 0);
        b.TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }
}
// 3-2. 아이템 선택 받기 상태 - 일단은 패스...
// 3-3. 주사위 등장 및 두드리기 상태
public class RollDiceState : Board_Data, IGameState
{
    public void EnterState(GameContext context)
    {

    }

    public void UpdateState(GameContext context)
    {
        // 스페이스바 누르면 상태 업데이트 되게
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("여기까지만 만듦");
            // context.SetState(new CombatState());
        }
    }

    public void ExitState(GameContext context)
    {
        print("여기까지만 만듦");
    }
}
// 3-4. 캐릭터 이동 상태
// 3-5. 멈춤 칸의 이벤트 발동 상태

public class StatePattern : MonoBehaviour
{
    // 싱글톤 적용
    public static StatePattern instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }
}
