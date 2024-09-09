using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
public class OrderAlertState : Board_Data, IGameState
{
    public void EnterState(GameContext context)
    {
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        Board_Manager.instance.SetCameraTarget(order[orderPointer] - 1);
        // 2. 현재 차례가 누군지 알려주는 UI
        /* 임시 */print(CharUI[order[orderPointer] - 1].transform.Find("UserName").GetComponent<Text>().text);
        // 3-2. 아이템 선택 받기 상태 - 일단은 패스...
        // 3-3. 주사위 등장 및 두드리기 상태
        // 3-4. 캐릭터 이동 상태
        // 3-5. 멈춤 칸의 이벤트 발동 상태

    }

    public void UpdateState(GameContext context)
    {
        // 상태 업데이트 로직 (예: 플레이어 입력 처리, 몬스터 발견 등)

        // 예를 들어, 전투 상태로 전환하는 조건
        if (false)
        {
            // context.SetState(new CombatState());
        }
    }

    public void ExitState(GameContext context)
    {
        // 알림 상태 종료 시 작업 수행
    }
}

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
