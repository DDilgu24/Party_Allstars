using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;


public class Board_Manager : MonoBehaviour
{
    [SerializeField] public CinemachineVirtualCamera virtualCamera;
    [SerializeField] public Transform[] playerMarks; // �÷��̾� �� ������Ʈ �迭

    [SerializeField] public GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] public GameObject TurnAlert, Intro, characterMark;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] playerNum;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // ������ ���� �ֻ��� ����
    public int[] order = new int[4] { 0, 0, 0, 0 }; // ����. [3, 2, 1, 4] �̶�� 3p > 2p > 1p > 4p ���� ���� �ǹ�


    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // �÷��̾� ��ȣ �� �÷�

    private int turns = 0; // �� ��
    private int phase; // ���� ����(0: �� �ʱ� / 1~4: n��° �÷��̾� / 5: �� ����(�̴ϰ���) / 6: �̴ϰ��� ���(���� ��Ȳ))
    private bool co_in_update; // ������Ʈ ���� �ڷ�ƾ�� ���� ���ΰ�?

    private IEnumerator Start()
    {
        // 1�ܰ� : ��Ʈ��
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.one, 3f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2�ܰ� : ���� �غ�
        // 2-1. ���� ������ �˸��� ����
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� �÷����� ������ ���ϰڽ��ϴ�!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. ���� ���ϱ�
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 1; i <= 4; i++)
        {
            yield return new WaitForSeconds(0.2f);
            characterMark.transform.Find($"{i}P_Dice").gameObject.SetActive(true);
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // 4���� �ֻ��� �� ���������� ���
        // 2-3. ���� ���� �� UI ���ġ
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. ��¥ ���� ���� ��
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("������ ���������!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� ���� ��Ƽ!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("���� ����");
        for (int i = 1; i <= 4; i++)
        {
            characterMark.transform.Find($"{i}P_Dice").gameObject.SetActive(false);
        }
        virtualCamera.Priority = 11;
        turns = 1;
        phase = 1;
        // co_in_update = false;
        StartCoroutine(PlayerTurn_co());
    }
    /*
    private void Update()
    {
        if (turns < 1) return;
        // 0 : �� �ʱ�
        if(phase.Equals(0))
        {
            // ���߿� �̺�Ʈ �߰�(������ 5�� �̺�Ʈ ���� ��...)�ϸ� �ǵ帮��
            // ������ �ϴ� �н�
            phase++;
        }
        // 1~4 : �÷��̾� ��
        else if(phase <= 4)
        {
            if(!co_in_update)
            {
                co_in_update = true;
                StartCoroutine(PlayerTurn_co());
            }
        }
        // 5 : �̴ϰ���
        else if(phase.Equals(5))
        {
            // ������ �ϴ� �н�
            phase++;
        }
        // 6 : �߰� ���
        else
        {
            // ������ �ϴ� �н� - ���� ������
            turns++;
            phase = 0;
        }
    }
    */
    private IEnumerator PlayerTurn_co()
    {
        // co_in_update = false;
        int playerNo = order[phase - 1];
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        SetCameraTarget(playerNo);
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        yield return null;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 3. �����̽� �� ������ �˸� UI �����
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        print("�ϴ��� �������");
        // 4. [�ϴ� �н�] ������ ���� �ޱ�
        // 5. �ֻ��� �ε帮��
        characterMark.transform.Find($"{playerNo}P_Dice").gameObject.SetActive(true);
        // 6. ĳ���� �̵�
        // 7. [�ϴ� �н�] ���� ĭ�� �´� �̺�Ʈ
    }



    private void HitDice(int p = 1)
    {
        characterMark.transform.Find($"{p}P").transform.DOLocalMoveY(3.5f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        Transform tf = characterMark.transform.Find($"{p}P_Dice");
        tf.GetChild(0).GetComponent<ParticleSystem>().Play();
        int r = Random.Range(1, 7);
        while(orderDecideNum.Any(value => value.Equals(r))) // ���� ���ϱ� �̹Ƿ� �ߺ��� ����
        {
            r = Random.Range(1, 7);
        }
        orderDecideNum[p - 1] = r;
        tf.GetComponent<Animator>().SetTrigger("WasHit");
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        if (p.Equals(1))
        {
            StartCoroutine(ComHitDice(2));
            StartCoroutine(ComHitDice(3));
            StartCoroutine(ComHitDice(4));
        }
    }

    private IEnumerator ComHitDice(int p)
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 1.5f));
        HitDice(p);
    }

    private void DecisionOrder()
    {
        int index = 0;
        for (int i = 6; i > 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (orderDecideNum[j].Equals(i)) 
                { 
                    order[index] = j + 1;
                    Vector3 newPos = new Vector3(300 * index - 800, 300 - 25 * (index % 2), 0);
                    CharUI[j].GetComponent<RectTransform>().DOAnchorPos(newPos, 1f).SetEase(Ease.OutQuad);
                    index++;
                    break; 
                }
            }
            if (index > 3) break;
        }
    }

    public void SetCameraTarget(int playerIndex)
    {
        // �÷��̾� �� ������Ʈ�� Transform�� ī�޶��� Ÿ������ ����
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    public void DebugAddScore(int pNo)
    {
        playerMarks[pNo].transform.DOMove(playerMarks[pNo].transform.position - Vector3.right * 7, 0.5f);
    }


}
