using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [���� ����] 1�ܰ�: ���� ���� �������̽�
public interface IGameState
{
    void EnterState(GameContext context); // ���°� ���۵� ��
    void UpdateState(GameContext context); // ���°� �������� ��
    void ExitState(GameContext context); // ���°� ����� ��
}

// [���� ����] 2�ܰ�: ���� ���ؽ�Ʈ Ŭ����
public class GameContext : MonoBehaviour
{
    private IGameState _currentState;

    // ���� ���¸� �����ϰ� ���� ��ȯ�� ó���մϴ�.
    public void SetState(IGameState newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    // ���� ���¸� ������Ʈ�մϴ�.
    private void Update()
    {
        _currentState?.UpdateState(this);
    }
}

