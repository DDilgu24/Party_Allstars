using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/*
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
public class OrderAlertState : Board_Manager, IGameState
{
    public void EnterState(GameContext context)
    {
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        // SetCameraTarget(order[orderPointer] - 1);
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        Vector3 v = new Vector3(1125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        // �׵θ�, �׶��̼� ���� + pNo + ĳ���� �ΰ� 4�� �ٲٱ�
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[order[orderPointer]];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[order[orderPointer]];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[order[orderPointer]];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[order[orderPointer] - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }

    public void UpdateState(GameContext context)
    {
        // �����̽��� ������ ���� ������Ʈ �ǰ�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            context.SetState(new RollDiceState());
        }
    }

    public void ExitState(GameContext context)
    {
        // �� �˸� UI �������� ���鼭 �������
        Vector3 v = new Vector3(-1125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }
}
// 3-2. ������ ���� �ޱ� ���� - �ϴ��� �н�...
// 3-3. �ֻ��� ���� �� �ε帮�� ����
public class RollDiceState : Board_Manager, IGameState
{
    public void EnterState(GameContext context)
    {

    }

    public void UpdateState(GameContext context)
    {
        // �����̽��� ������ ���� ������Ʈ �ǰ�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("��������� ����");
            // context.SetState(new CombatState());
        }
    }

    public void ExitState(GameContext context)
    {
        print("��������� ����");
    }
}
// 3-4. ĳ���� �̵� ����
// 3-5. ���� ĭ�� �̺�Ʈ �ߵ� ����
*/