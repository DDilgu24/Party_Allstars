using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class Board_Manager : MonoBehaviour
{
    [SerializeField] public CinemachineVirtualCamera virtualCamera;
    [SerializeField] public Transform[] playerMarks; // �÷��̾� �� ������Ʈ �迭
    [SerializeField] public Transform[] Spaces; // ���� ��ġ �迭

    [SerializeField] public GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] public Transform KeyExplain; // ���� Ű�� �˷��ִ� UI ������Ʈ
    [SerializeField] public Transform ViewModeCamera; // ���� Ű�� �˷��ִ� UI ������Ʈ
    [SerializeField] public GameObject TurnAlert, Intro;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] DiceEdge;
    [SerializeField] public Sprite[] playerNum;
    [SerializeField] public Animator dice_ani;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // ������ ���� �ֻ��� ����
    public int[] order = new int[4] { 0, 0, 0, 0 }; // ����. [3, 2, 1, 4] �̶�� 3p > 2p > 1p > 4p ���� ���� �ǹ�
    public bool[] isStun = new bool[5] { true, false, false, false, false }; // ĳ������ ���� ����. �÷��̾� ��ȣ�� �������� ��(�ε��� 0 �̻��)

    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // �÷��̾� ��ȣ �� �÷�
    private Vector3[] playerOffset = new Vector3[5] { Vector3.zero, new Vector3(1f, 0, 0), new Vector3(-1f, 0, 0), new Vector3(0.5f, 0, 1f), new Vector3(-0.5f, 0, 1f) }; // �÷��̾� ��ȣ �� ��ġ ������
    string[] dice_Name = new string[7] { "����" , "����" , "10����" , "456" , "¦��" , "Ȧ��" , "�׳�" };

    private int turns = 0; // �� ��
    private int phase; // ���� ����(0: �� �ʱ� / 1~4: n��° �÷��̾� / 5: �� ����(�̴ϰ���) / 6: �̴ϰ��� ���(���� ��Ȳ))
    private bool EndGame = false;
    int N;
    private int applyItemNo = 6; // ����� �ֻ��� �ε��� (6 = �⺻)
    private int DebugMove = 0; // ����� ��

    private IEnumerator Start()
    {
        DOTween.Init();
        N = GameManager.instance.TotalNum;
        Tween t;
        for (int i = N; i < 4; i++)
        {
            // �ο� ���� �ʰ��ϴ� ĳ���� ��ũ ��Ȱ��ȭ
            CharUI[i].SetActive(false);
            playerMarks[i].gameObject.SetActive(false);
        }
        // 1�ܰ� : ��Ʈ��
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").GetComponent<RectTransform>().DOSizeDelta(new Vector2(1192, 464), 3.0f).SetEase(Ease.InQuad);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2�ܰ� : ���� �غ�
        // 2-1. ���� ������ �˸��� ����
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� �÷����� ������ ���ϰڽ��ϴ�!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. ���� ���ϱ�
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 0; i < 4; i++)
        {
            if (i >= N)
            {
                // �ο� �� �̻��� �ε����� ���� �ֻ����� -1�� ó��
                orderDecideNum[i] = -1;
                continue;
            }
            yield return new WaitForSeconds(0.2f);
            playerMarks[i].Find($"{i + 1}P_Dice").gameObject.SetActive(true); // �ֻ��� Ȱ��ȭ 
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // ��� �ο� �ֻ��� ��� ���� ������ ���
        // 2-3. ���� ���� �� UI ���ġ
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. ��¥ ���� ���� ��
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Space").gameObject.SetActive(false);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("������ ���������!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Space").gameObject.SetActive(false);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� ���� ��Ƽ!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("���� ����");
        for (int i = 0; i < N ; i++)
        {
            // ��� �ֻ��� ��Ȱ��ȭ
            playerMarks[i].Find($"{i + 1}P_Dice").gameObject.SetActive(false);
        }
        virtualCamera.Priority = 11;
        turns = 1;
        phase = 1;
        StartCoroutine(PlayerTurn_co());
    }

    private IEnumerator PlayerTurn_co()
    {
        Tween moveTween;
        Sequence seq;
        int playerNo = order[phase - 1];
        int ifComThat0 = (playerNo > GameManager.instance.PlayerNum)? 0 : 1;
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        SetCameraTarget(playerNo);
        playerMarks[playerNo - 1].transform.position += 0.1f * Vector3.forward; // ���� ĳ���͸� ������ ���̰�
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        // 2-1. ����� ���� �������� ���� ������Ʈ ����
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo * ifComThat0];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        TurnAlert.transform.Find("Space").gameObject.SetActive(false);
        yield return null;
        // 2-2. ���� �˸��� Ű ���� ȭ�� ������
        v = new Vector3(-125, -25, 0);
        moveTween = TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        yield return moveTween.WaitForCompletion();
        TurnAlert.transform.Find("Space").gameObject.SetActive(playerNo <= GameManager.instance.PlayerNum); // �÷��̾� ���϶��� Space �̹��� Ȱ��ȭ

        // 2-3. �����̽� �� �Է�(Player) �Ǵ� 0.5�� ������ ��(COM) �Ѿ
        if (playerNo > GameManager.instance.PlayerNum)
            yield return new WaitForSeconds(0.5f);
        else 
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // ���� ���°� �ƴϾ�� 3~5 ����
        if (!isStun[playerNo]) 
        {
            // 3. ���� ������ ������ �ϳ��� �ڷ�ƾ���� ó��
            applyItemNo = 6;
            yield return StartCoroutine(SelectAction(playerNo));
            int move = HitDice(playerNo);
            if (applyItemNo.Equals(0))
            {
                move *= 2; // ���ӽ� : ���� �ֻ����� ��� �� 2��
                yield return new WaitForSeconds(0.5f);
                playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // ���� �������� �̹��� ����
            }
            // 4. ĳ���� �̵�
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(1).gameObject.SetActive(false); // �ֻ��� �׵θ� ��Ȱ��ȭ. ���ڸ� ���̰�
            yield return new WaitForSeconds(0.5f);
            while (move > 0)
            {
                yield return new WaitForSeconds(0.25f);

                CharInfoManager.instance.ScoreAdd(playerNo); // ������ 1 ���ϱ�
                int newscore = CharInfoManager.instance.charinfo[playerNo - 1].score; // �ٲ� ������ ĳ��
                Vector3 newpos = Spaces[newscore].position + Vector3.forward * 0.1f; // ���� ĭ�� ���� ��ǥ��

                bool moveIsJump = false; // ���� ĭ�� ���� ĳ���� �̵� ����� ���� (���� OR ����)
                if (newscore >= 23 && newscore != 30) moveIsJump = true;
                else if (4 <= newscore && newscore <= 11) moveIsJump = true;

                if (moveIsJump) moveTween = playerMarks[playerNo - 1].transform.DOLocalJump(newpos, 2f, 1, 0.3f); // ĳ���� �� �̵� - ����
                else moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 0.3f).SetEase(Ease.Linear); // ĳ���� �� �̵� - ����

                yield return moveTween.WaitForCompletion(); // �� �̵��� ���� ������ ���
                move--; // ���� ���� 1 ����
                playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // ���� �������� �̹��� ����
                if (CharInfoManager.instance.charinfo[playerNo - 1].score > 33) // ��� �����ϸ�
                {
                    BD1SoundManager.instance.BGMPlayer.pitch = 1f;
                    playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(false); // �ֻ����� ��Ȱ��ȭ 
                    move = 0;
                    EndGame = true;
                    newpos += Vector3.down * 9.5f;
                    BD1SoundManager.instance.StopBGM();
                    BD1SoundManager.instance.PlaySFX("FlagDown");
                    CharUI[0].transform.parent.GetComponent<RectTransform>().DOMoveY(1000, 0.5f); // ����ǥ UI ġ���
                    moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 1.5f).SetEase(Ease.Linear); // ��� �Ʒ��� �̵�
                    yield return moveTween.WaitForCompletion(); // �� �̵��� ���� ������ ���
                    playerMarks[playerNo - 1].transform.DOMove(Spaces[35].position + Vector3.forward * 2, 0.5f).SetEase(Ease.Linear);
                    BD1SoundManager.instance.PlaySFX("Victory");
                }
                else if (CharInfoManager.instance.charinfo[playerNo - 1].score >= 25) // 25�� �����ϸ�
                    BD1SoundManager.instance.BGMPlayer.pitch = 1.1f;
            }
            yield return new WaitForSeconds(0.1f);
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(1).gameObject.SetActive(true); // �ֻ��� �׵θ� �ٽ� Ȱ��ȭ�ϰ�
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(false); // �ֻ����� ��Ȱ��ȭ 
            // 5. ���� ĭ�� �´� �̺�Ʈ - �ڷ�ƾ���� ó��
            yield return StartCoroutine(SpaceEvent_co(playerNo, CharInfoManager.instance.charinfo[playerNo - 1].score));
        }
        else // ���� ���� ���ٸ�
        {
            yield return new WaitForSeconds(1f);
            isStun[playerNo] = false; // ���� ���� ����
            playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(false); // ��ƼŬ�� ����
            CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(false);
        }

        // 6. �� ���� ��
        yield return new WaitForSeconds(0.5f);
        if (EndGame) // ���� �������� ����
        {
            yield return StartCoroutine(CharInfoManager.instance.ResultUISetting());
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
        else // �ƴϸ� ���� ���ʷ�
        {
            phase++;
            if (phase > GameManager.instance.TotalNum) { turns++; phase = 1; } // ��� ���� ������ �ٷ� 1��°��. �̴ϰ��� ������� �ϴ� ���� �ɷ�
            StartCoroutine(PlayerTurn_co()); // ���� �÷��̾��
        }
    }

    // �غ� 2-2(���� ���ϱ�) + �� ���� 5(�ֻ��� �ε帮��) ���� �޼ҵ�
    private int HitDice(int p)
    {
        Transform tf = playerMarks[p - 1].Find($"{p}P_Dice"); // �ֻ����� ĳ��
        playerMarks[p - 1].transform.DOLocalMoveY(playerMarks[p - 1].transform.position.y + 1.8f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // ĳ���� ���� ȿ��
        tf.transform.DOLocalMoveY(3 - 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // �ֻ����� ������ �ֵ��� ���̰�
        tf.GetChild(2).GetComponent<ParticleSystem>().Play(); // ��ƼŬ ���
        int r = applyItemNo switch
        {
            0 => UnityEngine.Random.Range(1, 7),// (�ӽ�) ���� �ֻ��� : �ϴ� 1~6
            1 => tf.GetChild(0).GetComponent<SpriteRenderer>().sprite.name.Last() - 48,// ���� �ֻ���
            2 => UnityEngine.Random.Range(1, 11),// 10���� �ֻ���
            3 => UnityEngine.Random.Range(4, 7),// 456 �ֻ���
            4 => UnityEngine.Random.Range(1, 4) * 2,// ¦�� �ֻ���
            5 => UnityEngine.Random.Range(1, 4) * 2 - 1,// Ȧ�� �ֻ���
            _ => UnityEngine.Random.Range(1, 7),
        };

        if (DebugMove > 0)
        {
            r = DebugMove;
            DebugMove = 0;
        }

        // ���� ���ϱ� �ܰ��� ���
        if (turns.Equals(0))
        {
            while (orderDecideNum.Any(value => value.Equals(r))) // ���� ���ϱ� �̹Ƿ� �ߺ��� ����
                r = UnityEngine.Random.Range(1, 7);
            orderDecideNum[p - 1] = r;
            if (p.Equals(1))
            {
                for (int i = 2; i <= N; i++) StartCoroutine(HitDice_co(i));
            }
        }

        tf.GetComponent<Animator>().SetBool("DiceStop",true); // �ֻ���: �ִϸ��̼��� �����ϰ� �̹����� ��������� ���� 
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        return r;
    }

    private IEnumerator HitDice_co(int p)
    {
        float delayTime = 0.01f;
        if (p > GameManager.instance.PlayerNum) 
            delayTime = UnityEngine.Random.Range(0.75f, 1.00f);
        yield return new WaitForSeconds(delayTime);
        HitDice(p);
    }

    private void DecisionOrder()
    {
        int index = 0;
        for (int i = 6; i > 0; i--)
        {
            for (int j = 0; j < N; j++)
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
            if (N < index) break;
        }
    }

    // �� ���� 1(�Ͽ� �´� ī�޶� �̵�)
    public void SetCameraTarget(int playerIndex)
    {
        // �÷��̾� �� ������Ʈ�� Transform�� ī�޶��� Ÿ������ ����
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    // �� ���� 3(�ൿ�� �����ϴ� �̺�Ʈ)
    private IEnumerator SelectAction(int playerNo)
    {
        bool thisCoAgain = true;
        int itemCount = CharInfoManager.instance.charinfo[playerNo - 1].itemCount;
        // �ڷ�ƾ ��ü�� while�� ���� �ش� �ڷ�ƾ�� Ư�� ��Ȳ�� �ٽ� ���ư����� ��
        while (thisCoAgain)
        {
            // 0. ����� �ֻ����� �°� �׵θ� ����
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice/SMP_DiceEdge").GetComponent<SpriteRenderer>().sprite = DiceEdge[applyItemNo % 6];
            // 1. �ϴ� �ֻ����� ���ư���
            yield return new WaitForSeconds(0.25f);
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(true); // �ֻ��� Ȱ��ȭ 
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().enabled = true; // �ֻ��� �ִϵ� Ȱ��ȭ
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().SetBool("DiceStop", false); // �ֻ����� �ٽ� ���ư���
            int keyInput = -1;
            // 2. ��ȿ�� Ű(Ctrl, Shift, Spacebar)�� �ԷµǸ� ���� �۾�
            // COM�� �������� ������ �ڵ����� 1��° �� ��� �� �ڵ����� Spacebar �Է��� �ɷ� ó�� 
            if (playerNo > GameManager.instance.PlayerNum)
            {
                yield return new WaitForSeconds(0.5f);
                if (itemCount > 0)
                {
                    Used_item(playerNo, 1);
                    itemCount--;
                    continue;
                }
                yield return new WaitForSeconds(0.5f);
                keyInput = 13;
                // if (CharInfoManager.instance.charinfo[playerNo - 1].score < 1) keyInput = 2;
            }
            // �÷��̾�� �������� �߰� �ϸ鼭, ��ȿ�� Ű�� �Է¹ޱ�
            else
            {
                KeyExplain.Find("SelectMode").gameObject.SetActive(true);
                yield return new WaitUntil(() => VaildKeyinput() > 0);
                keyInput = VaildKeyinput();
            }
            KeyExplain.Find("SelectMode").gameObject.SetActive(false);
            // 3. �Է��� Ű�� ���� �۾� ����
            // 3-1. �ѷ����� ���
            if (keyInput.Equals(11))
            {
                KeyExplain.Find("ViewMode").gameObject.SetActive(true);
                ViewModeCamera.position = virtualCamera.Follow.position + Vector3.forward * 1.9f; // �� ���� transform�� ���� ī�޶� ��ġ��
                virtualCamera.Follow = ViewModeCamera; // ���� ī�޶� �� ���� transform ����
                virtualCamera.LookAt = ViewModeCamera;

                while (true)
                {
                    // Esc : 3-1�� ���� ����, ��������� SelectAction�� �ٽ� ���ư�
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        virtualCamera.Follow = playerMarks[playerNo - 1]; // ���� ī�޶� �ٽ� ĳ���� ����
                        virtualCamera.LookAt = playerMarks[playerNo - 1];
                        break;
                    }
                    // LeftArrow : ������ ���� ���������� ī�޶� ��������
                    else if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        ViewModeCamera.position += Vector3.right * 0.1f;
                        if (ViewModeCamera.position.x > 63.5f)
                            ViewModeCamera.position = new Vector3(63.5f, 2f, 6f);
                    }
                    // RightArrow 
                    else if (Input.GetKey(KeyCode.RightArrow))
                    {
                        ViewModeCamera.position += Vector3.left * 0.1f;
                        if (ViewModeCamera.position.x < -146.5f)
                            ViewModeCamera.position = new Vector3(-146.5f, 2f, 6f);
                    }
                    yield return null; // �� �ٿ��� ��� �� ���� ���������� �Ѿ
                }

                KeyExplain.Find("ViewMode").gameObject.SetActive(false);
            }
            // 3-2. ������ ���� ���
            else if (keyInput.Equals(12))
            {
                KeyExplain.Find("ItemMode").gameObject.SetActive(true);
                // ������ ���� �ҷ�����
                int cursor = 0;
                // Ŀ�� �ʱ�ȭ
                KeyExplain.Find($"ItemMode/Items/Arrow").GetComponent<RectTransform>().anchoredPosition = new Vector3(-288, 560, 0);
                for (int i = 0; i <= itemCount; i++)
                {
                    float alpha = (i.Equals(cursor)) ? 1 : 0.4f;
                    KeyExplain.Find($"ItemMode/Items/Item_{i}").GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                }
                // �ؽ�Ʈ ����
                KeyExplain.Find($"ItemMode/Items/Text").GetComponent<Text>().text = "�׳� �ֻ���";

                // ������ �̹��� �ҷ�����
                for (int i=0; i<3; i++)
                {
                    KeyExplain.Find($"ItemMode/Items/Item_{i+1}").GetComponent<Image>().sprite 
                        =CharUI[playerNo-1].transform.Find($"Lower_Left/Item ({i+1})").GetComponent<Image>().sprite;
                }
                while (true)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        break;
                    }

                    else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        bool isRightKey = Input.GetKeyDown(KeyCode.RightArrow);
                        // Ŀ�� �ε��� ����
                        cursor += ((isRightKey)? 1 : itemCount);
                        cursor %= (itemCount + 1);
                        // ȭ��ǥ �̵�
                        KeyExplain.Find($"ItemMode/Items/Arrow").GetComponent<RectTransform>().anchoredPosition = new Vector3(192 * cursor - 288, 560, 0);
                        // ���� ����
                        for (int i = 0; i <= itemCount; i++)
                        {
                            float alpha = (i.Equals(cursor)) ? 1 : 0.4f;
                            KeyExplain.Find($"ItemMode/Items/Item_{i}").GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                        }
                        // �ؽ�Ʈ ����
                        string s = "�׳� �ֻ���";
                        if(cursor > 0) 
                            s = dice_Name[CharInfoManager.instance.charinfo[playerNo - 1].items[cursor - 1]] + " �ֻ���";
                        KeyExplain.Find($"ItemMode/Items/Text").GetComponent<Text>().text = s;
                    }

                    else if (Input.GetKeyDown(KeyCode.Space))
                    {
                        if (cursor > 0)
                        {
                            Used_item(playerNo, cursor);
                        }
                        else
                        {
                            applyItemNo = 6;
                        }
                        break;
                    }

                    yield return null; // �� �ٿ��� ��� �� ���� ���������� �Ѿ
                }
                KeyExplain.Find("ItemMode").gameObject.SetActive(false);
            }
            // 3-3. ���� �ڷ�ƾ ����(�ֻ��� ġ��)
            else
            {
                if (keyInput < 11) DebugMove = keyInput;
                thisCoAgain = false;
            }
        }
    }
    private void Used_item(int playerNo, int cursor)
    {
        // ĳ���� �������� ������ ����
        applyItemNo = CharInfoManager.instance.charinfo[playerNo - 1].UseItem(cursor - 1);
        int itemCount = CharInfoManager.instance.charinfo[playerNo - 1].itemCount;
        // �ֻ��� �ִϸ��̼� �ٲٱ�
        dice_ani = playerMarks[playerNo - 1].GetChild(0).GetComponent<Animator>();
        string s = applyItemNo switch
        {
            1 => "DiceSlow",
            2 => "Dice1to10",
            3 => "Dice456",
            4 => "Dice246",
            5 => "Dice135",
            _ => "Dice6_roulette"
        };
        dice_ani.Play(s);
        // UI ����
        Transform itemSlotTf = CharUI[playerNo - 1].transform.Find("Lower_Left");
        for (int i = 0; i < 3; i++)
        {
            if (i < itemCount) itemSlotTf.Find($"Item ({i + 1})").GetComponent<Image>().sprite
                    = CharInfoManager.instance.ItemSp[CharInfoManager.instance.charinfo[playerNo - 1].items[i]];
            else itemSlotTf.Find($"Item ({i + 1})").GetComponent<Image>().sprite
                    = CharInfoManager.instance.ItemSp[6];
        }
    }


    // �� ���� 7(������ ĭ�� ���� �̺�Ʈ)
    private IEnumerator SpaceEvent_co(int playerNo, int n)
    {
        Tween t;
        Sequence seq;
        yield return new WaitForSeconds(0.5f);
        switch (n)
        {
            case 3:
            case 9:
            case 17:
            case 22:
            case 30:
                // ���� ȿ��
                // 1. ���ٰ� ĳ���Ϳ��� ���� ��ġ��
                t = Spaces[n].GetChild(0).transform.DOLocalMoveZ(-1f, 0.3f).SetEase(Ease.OutQuad);
                yield return t.WaitForCompletion();
                // 2. ĳ���� ���� ���� ��ƼŬ 
                playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(true);
                CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(true);
                // 3. ���� ����ġ
                t = Spaces[n].GetChild(0).transform.DOLocalMoveZ(-3, 0.75f);
                yield return t.WaitForCompletion();
                // 4. ĳ���Ϳ��� ���� ȿ�� �ݿ�
                isStun[playerNo] = true;
                break;
            case 2:
            case 11:
            case 18:
            case 19:
                // ������ ȿ��
                // 0. 3�� �� á���� ���� �� ����
                if(CharInfoManager.instance.charinfo[playerNo - 1].itemCount.Equals(3))
                {
                    break;
                }
                // 1. �������� �ֻ��� ����
                int r = UnityEngine.Random.Range(0, 100);
                int itemNo = 5;
                int h = Mathf.Min(CharInfoManager.instance.Score1st() - n, 15); // ����ġ
                int[] itemRange = new int[4] {h + 5, 2*h + 10, 3*h + 20, 4*h + 30}; // ������ Ȯ��ǥ
                for (int i = 0; i < 4; i++)
                {
                    if(r < itemRange[i])
                    {
                        itemNo = i;
                        break;
                    }
                }
                if (itemNo.Equals(5)) itemNo -= (r % 2); // �ο� �ش��ϴ� ��� 4,5 ���� �ϳ�
                Spaces[n].GetChild(0).GetComponent<SpriteRenderer>().sprite = CharInfoManager.instance.ItemSp[itemNo]; // ���� ������ sprite�� �̸� ����
                // 2. �����ϴ� �ִϸ��̼�
                playerMarks[playerNo - 1].transform
                    .DOLocalMoveY(playerMarks[playerNo - 1].transform.position.y + 1.8f, 0.2f)
                    .SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                // 3. �������� ������ ȿ��
                seq = DOTween.Sequence()
                 .Append(Spaces[n].GetChild(0).transform.DOLocalMoveY(4.7f, 0.75f)) // ���� �ö���鼭
                 .Join(Spaces[n].GetChild(0).transform.DOScale(new Vector3(-3f, 3f, 3f), 0.75f)); // ���ÿ� ũ�⵵ ���� Ŀ����
                yield return seq.WaitForCompletion();
                // 4. ���� �������� ������鼭, UI�� �����ϴ� ȿ��
                // 4-1. ��� UI�� ������ �� ���� ĳ��, scale�� 0����
                Transform itemUItf = CharUI[playerNo - 1].transform.Find($"Lower_Left/Item ({ CharInfoManager.instance.charinfo[playerNo - 1].itemCount + 1 })");
                itemUItf.localScale = Vector3.zero;
                // 4-2. ������ �� ���� ���� ������ �̹����� ����
                itemUItf.GetComponent<Image>().sprite = CharInfoManager.instance.ItemSp[itemNo];
                // 4-3. ���� �������� �۾����鼭, ���ÿ� UI �������� ũ��� Ŀ����
                seq = DOTween.Sequence()
                 .Append(Spaces[n].GetChild(0).transform.DOScale(0, 0.5f))
                 .Join(itemUItf.DOScale(3.5f, 0.5f))
                 .Append(itemUItf.DOScale(2.5f, 0.25f));
                // 4-4. ������ ���������� ���
                yield return seq.WaitForCompletion();
                // 5. �������� ĳ���� ������ ���� �ݿ�
                CharInfoManager.instance.charinfo[playerNo - 1].GetItem(itemNo);
                // 6. ȭ�� �� �������� �ٽ� �����
                Spaces[n].GetChild(0).transform.localPosition = Vector3.up * 3.5f;
                Spaces[n].GetChild(0).transform.localScale = Vector3.zero;
                break;
            case 12:
            case 15:
                // �߶� ȿ��
                // 1. �ó׸ӽ��� ���� ȿ���� �ӽ÷� ��
                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
                // 2. ĳ���Ͱ� �Ʒ��� ������
                t = playerMarks[playerNo - 1].transform.DOMoveY(-5, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                // 3. ĳ���͸� 11�� ĭ���� �̵�
                CharInfoManager.instance.ScoreAdd(playerNo, 11 - CharInfoManager.instance.charinfo[playerNo - 1].score); // ������ 11�� ����
                playerMarks[playerNo - 1].transform.position = Spaces[11].position + Vector3.up * 5f + Vector3.forward * 0.1f; // ĳ���� ��ũ�� 11�� ĭ ���� �̵�
                SetCameraTarget(playerNo); // �ó׸ӽ� ���� �ٽ� Ȱ��ȭ
                t = playerMarks[playerNo - 1].transform.DOMoveY(2, 0.5f).SetEase(Ease.InQuad); // y ���� 11�� ĭ�� �����ϰ�
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                break;
            case 31:
                // ���� ȿ��
                // 1. �����ö���� ���� �ö��
                t = Spaces[31].GetChild(0).transform.DOLocalMoveY(0.5f, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                // 2. ĳ���Ϳ� �Բ� �Ʒ��� ������
                seq = DOTween.Sequence()
                .Append(Spaces[31].GetChild(0).transform.DOLocalMoveY(-5f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(0, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 3. ������ ĳ���͸� 28�� ĭ �Ʒ��� �̵�
                CharInfoManager.instance.ScoreAdd(playerNo, 28 - CharInfoManager.instance.charinfo[playerNo - 1].score); // ������ 28�� ����
                Spaces[31].GetChild(0).transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // ������ 28�� ĭ�� 3.5 �Ʒ��� �̵� + �տ� ���̰� z ����
                playerMarks[playerNo - 1].transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.1f; // ĳ���� ��ũ�� 28�� ĭ�� 3.5 �Ʒ��� �̵�
                // 4. 28�� ĭ ������ ���� �ö���� ȿ��
                seq = DOTween.Sequence()
                .Append(Spaces[31].GetChild(0).transform.DOMoveY(4f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(4f, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 5. ĳ���Ϳ� ���� ȿ�� �ο�
                playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(true);
                CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(true);
                isStun[playerNo] = true;
                yield return new WaitForSeconds(0.5f);
                // 6. �����ö���� �ٽ� �Ʒ��� + ����ġ(31�� �Ʒ�)
                t = Spaces[31].GetChild(0).transform.DOMoveY(0.5f, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                Spaces[31].GetChild(0).transform.position = Spaces[31].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // ������ 31�� ĭ �Ʒ��� �̵�
                break;
        }
    }



    private int VaildKeyinput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) return 11;
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) return 12;
        if (Input.GetKeyDown(KeyCode.Space)) return 13;
        // ���⼭���� ����׿�
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha6)) return 6;
        if (Input.GetKeyDown(KeyCode.Alpha7)) return 7;
        if (Input.GetKeyDown(KeyCode.Alpha8)) return 8;
        if (Input.GetKeyDown(KeyCode.Alpha9)) return 9;
        if (Input.GetKeyDown(KeyCode.Alpha0)) return 10;
        // ������� ����׿�
        return 0;
    }
}
