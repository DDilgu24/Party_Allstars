using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

// [���� ����] 3�ܰ�: ���º� ����
// 3-1. ���� �˸� ����
public class OrderAlertState : Board_Data, IGameState
{
    public void EnterState(GameContext context)
    {
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        Board_Manager.instance.SetCameraTarget(order[orderPointer] - 1);
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        /* �ӽ� */print(CharUI[order[orderPointer] - 1].transform.Find("UserName").GetComponent<Text>().text);
        // 3-2. ������ ���� �ޱ� ���� - �ϴ��� �н�...
        // 3-3. �ֻ��� ���� �� �ε帮�� ����
        // 3-4. ĳ���� �̵� ����
        // 3-5. ���� ĭ�� �̺�Ʈ �ߵ� ����

    }

    public void UpdateState(GameContext context)
    {
        // ���� ������Ʈ ���� (��: �÷��̾� �Է� ó��, ���� �߰� ��)

        // ���� ���, ���� ���·� ��ȯ�ϴ� ����
        if (false)
        {
            // context.SetState(new CombatState());
        }
    }

    public void ExitState(GameContext context)
    {
        // �˸� ���� ���� �� �۾� ����
    }
}

public class StatePattern : MonoBehaviour
{
    // �̱��� ����
    public static StatePattern instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }
}
