using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

