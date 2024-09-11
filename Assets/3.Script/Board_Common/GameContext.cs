using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/*
// [상태 패턴] 1단계: 게임 상태 인터페이스
public interface IGameState
{
    void EnterState(GameContext context); // 상태가 시작될 때
    void UpdateState(GameContext context); // 상태가 진행중일 때
    void ExitState(GameContext context); // 상태가 종료될 때
}

// [상태 패턴] 2단계: 게임 컨텍스트 클래스
public class GameContext : MonoBehaviour
{
    private IGameState _currentState;

    // 현재 상태를 설정하고 상태 전환을 처리합니다.
    public void SetState(IGameState newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    // 현재 상태를 업데이트합니다.
    private void Update()
    {
        _currentState?.UpdateState(this);
    }
}

// [상태 패턴] 3단계: 상태별 구현
// 3-1. 순서 알림 상태
public class OrderAlertState : Board_Manager, IGameState
{
    public void EnterState(GameContext context)
    {
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        // SetCameraTarget(order[orderPointer] - 1);
        // 2. 현재 차례가 누군지 알려주는 UI
        Vector3 v = new Vector3(1125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        // 테두리, 그라데이션 색깔 + pNo + 캐릭터 로고 4개 바꾸기
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[order[orderPointer]];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[order[orderPointer]];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[order[orderPointer]];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[order[orderPointer] - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
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
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }
}
// 3-2. 아이템 선택 받기 상태 - 일단은 패스...
// 3-3. 주사위 등장 및 두드리기 상태
public class RollDiceState : Board_Manager, IGameState
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
*/