using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;

public class Board_Data : MonoBehaviour
{
    [SerializeField] protected CinemachineVirtualCamera virtualCamera;
    [SerializeField] protected Transform[] playerMarks; // �÷��̾� �� ������Ʈ �迭

    [SerializeField] protected GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] protected GameObject Intro, characterMark;
    [SerializeField] protected Sprite[] DiceNum;
    protected int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // ������ ���� �ֻ��� ����
    protected int[] order = new int[4] { 0, 0, 0, 0 }; // ����. [3, 2, 1, 4] �̶�� 3p > 2p > 1p > 4p ���� ���� �ǹ�

    public GameContext gameContext;
    public static int orderPointer = 0; // �� ��° �÷��̾��� �����ΰ�? (0~3 -> 1~4��°)
}

public class Board_Manager : Board_Data
{
    // �̱��� ����
    public static Board_Manager instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

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
        SetCameraTarget(order[orderPointer] - 1);
        // gameContext.SetState(new OrderAlertState());
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
        yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
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
        Transform target = playerMarks[playerIndex];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

}
