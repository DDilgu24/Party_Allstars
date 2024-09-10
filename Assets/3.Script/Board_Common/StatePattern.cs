using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// [���� ����] 3�ܰ�: ���º� ����
// 3-1. ���� �˸� ����
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
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        Board_Manager.instance.SetCameraTarget(b.order[b.orderPointer] - 1);
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        Vector3 v = new Vector3(1125, -25, 0);
        b.TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        // �׵θ�, �׶��̼� ���� + pNo + ĳ���� �ΰ� 4�� �ٲٱ�
        TurnAlertEdge.color = b.playerColor[b.order[b.orderPointer]];
        TurnAlertGra.color = b.playerColor[b.order[b.orderPointer]];
        TurnAlertpNo.sprite = b.playerNum[b.order[b.orderPointer]];
        TurnAlertChar.sprite = b.CharUI[b.order[b.orderPointer] - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        v = new Vector3(-125, -25, 0);
        b.TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
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
        b.TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
    }
}
// 3-2. ������ ���� �ޱ� ���� - �ϴ��� �н�...
// 3-3. �ֻ��� ���� �� �ε帮�� ����
public class RollDiceState : Board_Data, IGameState
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
