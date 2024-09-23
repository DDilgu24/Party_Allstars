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
    [SerializeField] public GameObject TurnAlert, Intro;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] playerNum;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // ������ ���� �ֻ��� ����
    public int[] order = new int[4] { 0, 0, 0, 0 }; // ����. [3, 2, 1, 4] �̶�� 3p > 2p > 1p > 4p ���� ���� �ǹ�
    public bool[] isStun = new bool[5] { true, false, false, false, false }; // ĳ������ ���� ����. �÷��̾� ��ȣ�� �������� ��(�ε��� 0 �̻��)

    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // �÷��̾� ��ȣ �� �÷�
    private Vector3[] playerOffset = new Vector3[5] { Vector3.zero, new Vector3(1f, 0, 0), new Vector3(-1f, 0, 0), new Vector3(0.5f, 0, 1f), new Vector3(-0.5f, 0, 1f) }; // �÷��̾� ��ȣ �� ��ġ ������

    private int turns = 0; // �� ��
    private int phase; // ���� ����(0: �� �ʱ� / 1~4: n��° �÷��̾� / 5: �� ����(�̴ϰ���) / 6: �̴ϰ��� ���(���� ��Ȳ))
    private bool EndGame = false;

    private IEnumerator Start()
    {
        DOTween.Init();
        // 1�ܰ� : ��Ʈ��
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.one, 3f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2�ܰ� : ���� �غ�
        // 2-1. ���� ������ �˸��� ����
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� �÷����� ������ ���ϰڽ��ϴ�!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. ���� ���ϱ�
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 1; i <= 4; i++)
        {
            yield return new WaitForSeconds(0.2f);
            playerMarks[i - 1].GetChild(0).gameObject.SetActive(true); // �ֻ��� Ȱ��ȭ 
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // 4���� �ֻ��� �� ���������� ���
        // 2-3. ���� ���� �� UI ���ġ
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. ��¥ ���� ���� ��
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("������ ���������!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("�׷� ���� ��Ƽ!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("���� ����");
        for (int i = 1; i <= 4; i++)
        {
            playerMarks[i - 1].GetChild(0).gameObject.SetActive(false);
        }
        virtualCamera.Priority = 11;
        turns = 1;
        phase = 1;
        // co_in_update = false;
        StartCoroutine(PlayerTurn_co());
    }

    private IEnumerator PlayerTurn_co()
    {
        Tween moveTween;
        int playerNo = order[phase - 1];
        int ifComThat0 = (playerNo > GameManager.instance.Player_Num)? 0 : 1;
        // 1. ���� ������ ĳ���͸� �߽����� ī�޶� �̵�
        SetCameraTarget(playerNo);
        playerMarks[playerNo - 1].transform.position += 0.1f * Vector3.forward; // ���� ĳ���͸� ������ ���̰�?
        // 2. ���� ���ʰ� ������ �˷��ִ� UI
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo * ifComThat0];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        yield return null;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 3. �����̽� �� ������ �˸� UI �����
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // ���� ���°� �ƴϾ�� 4~7 ����
        if (!isStun[playerNo]) 
        {
            // 4. [�ϴ� �н�] ������ ���� �ޱ�
            // 5. �ֻ��� �ε帮��
            yield return new WaitForSeconds(0.25f);
            playerMarks[playerNo - 1].GetChild(0).gameObject.SetActive(true); // �ֻ��� Ȱ��ȭ 
            playerMarks[playerNo - 1].GetChild(0).gameObject.GetComponent<Animator>().enabled = true; // �ֻ��� �ִϵ� Ȱ��ȭ
            playerMarks[playerNo - 1].GetChild(0).gameObject.GetComponent<Animator>().SetBool("DiceStop", false); // �ֻ����� �ٽ� ���ư���
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            int move = HitDice(playerNo);
            // 6. ĳ���� �̵�
            playerMarks[playerNo - 1].GetChild(0).GetChild(2).gameObject.SetActive(false); // �ֻ��� �׵θ� ��Ȱ��ȭ. ���ڸ� ���̰�
            yield return new WaitForSeconds(0.5f);
            while (move > 0)
            {
                yield return new WaitForSeconds(0.25f);
                /*
                Vector3 oldpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // ���� ĭ�� ���� ��ǥ��
                CharInfoManager.instance.ScoreAdd(playerNo); // ������ 1 ���ϱ�
                Vector3 newpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // ���� ĭ�� ���� ��ǥ��
                Vector3 midpos = (oldpos + newpos) * 0.5f; // �߰� ��ǥ�� ���
                midpos.y = Mathf.Max(oldpos.y, newpos.y) + 2; // �߰� ��ǥ�� y���� ����
                Vector3[] movepath = new Vector3[] { oldpos, midpos, newpos };
                moveTween = playerMarks[playerNo - 1].transform.DOPath(movepath, 0.3f, PathType.CatmullRom).SetEase(Ease.Linear); // ĳ���� ���� ���������� �̵�
                */

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
                playerMarks[playerNo - 1].GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // ���� �������� �̹��� ����
                if (CharInfoManager.instance.charinfo[playerNo - 1].score > 33) // ��� �����ϸ�
                {
                    BD1SoundManager.instance.BGMPlayer.pitch = 1f;
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
            playerMarks[playerNo - 1].GetChild(0).GetChild(2).gameObject.SetActive(true); // �ֻ��� �׵θ� �ٽ� Ȱ��ȭ�ϰ�
            playerMarks[playerNo - 1].GetChild(0).gameObject.SetActive(false); // �ֻ����� ��Ȱ��ȭ 
            // 7. ���� ĭ�� �´� �̺�Ʈ - �޼ҵ�� ó��
            yield return StartCoroutine(SpaceEvent_co(playerNo, CharInfoManager.instance.charinfo[playerNo - 1].score));
        }
        else // ���� ���� ���ٸ�
        {
            yield return new WaitForSeconds(1f);
            isStun[playerNo] = false; // ���� ���� ����
            playerMarks[playerNo - 1].GetChild(1).gameObject.SetActive(false); // ��ƼŬ�� ����
            CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(false);
        }

        // 8. ���� ���ʷ�
        yield return new WaitForSeconds(0.5f);
        if (EndGame) // ���� ��������
        {
            yield return StartCoroutine(CharInfoManager.instance.ResultUISetting());
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
        else // �ƴϸ�
        {
            phase++;
            if (phase > 4) { turns++; phase = 1; } // �̰� �ӽ�. 4��° ���� ������ �ٷ� 1��°��
            StartCoroutine(PlayerTurn_co()); // ���� �÷��̾��
        }
    }

    // �غ� 2-2(���� ���ϱ�) + �� ���� 5(�ֻ��� �ε帮��) ���� �޼ҵ�
    private int HitDice(int p)
    {
        Transform tf = playerMarks[p - 1].GetChild(0); // �ֻ����� ĳ��
        playerMarks[p - 1].transform.DOLocalMoveY(playerMarks[p - 1].transform.position.y + 1.8f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // ĳ���� ���� ȿ��
        tf.transform.DOLocalMoveY(3 - 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // �ֻ����� ������ �ֵ��� ���̰�
        tf.GetChild(0).GetComponent<ParticleSystem>().Play(); // ��ƼŬ ���
        int r = UnityEngine.Random.Range(1, 7); // �ֻ��� ����: 1~6

        // ���� ���ϱ� �ܰ��� ���
        if(turns.Equals(0))
        {
            while (orderDecideNum.Any(value => value.Equals(r))) // ���� ���ϱ� �̹Ƿ� �ߺ��� ����
            {
                r = UnityEngine.Random.Range(1, 7);
            }
            orderDecideNum[p - 1] = r;
            if (p.Equals(1))
            {
                StartCoroutine(ComHitDice(2));
                StartCoroutine(ComHitDice(3));
                StartCoroutine(ComHitDice(4));
            }
        }

        tf.GetComponent<Animator>().SetBool("DiceStop",true); // �ֻ���: �ִϸ��̼��� �����ϰ� �̹����� ��������� ���� 
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        return r;
    }

    private IEnumerator ComHitDice(int p)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 1.5f));
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

    // �� ���� 1(�Ͽ� �´� ī�޶� �̵�)
    public void SetCameraTarget(int playerIndex)
    {
        // �÷��̾� �� ������Ʈ�� Transform�� ī�޶��� Ÿ������ ����
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    // �� ���� 7(������ ĭ�� ���� �̺�Ʈ)
    private IEnumerator SpaceEvent_co(int playerNo, int n)
    {
        Tween t;
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
                playerMarks[playerNo - 1].GetChild(1).gameObject.SetActive(true);
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
                // Debug.Log("������ ȹ���ε� ���� �� �������");
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
                Sequence seq = DOTween.Sequence().SetAutoKill(false)
                .Append(Spaces[31].GetChild(0).transform.DOLocalMoveY(-3f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(2f, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 3. ������ ĳ���͸� 28�� ĭ �Ʒ��� �̵�
                CharInfoManager.instance.ScoreAdd(playerNo, 28 - CharInfoManager.instance.charinfo[playerNo - 1].score); // ������ 28�� ����
                Spaces[31].GetChild(0).transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // ������ 28�� ĭ�� 3.5 �Ʒ��� �̵� + �տ� ���̰� z ����
                playerMarks[playerNo - 1].transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.1f; // ĳ���� ��ũ�� 28�� ĭ�� 3.5 �Ʒ��� �̵�
                // 4. 28�� ĭ ������ ���� �ö���� ȿ��
                seq = DOTween.Sequence().SetAutoKill(false)
                .Append(Spaces[31].GetChild(0).transform.DOMoveY(4f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(4f, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 5. ĳ���Ϳ� ���� ȿ�� �ο�
                playerMarks[playerNo - 1].GetChild(1).gameObject.SetActive(true);
                CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(true);
                isStun[playerNo] = true;
                yield return new WaitForSeconds(0.5f);
                // 6. �����ö���� �ٽ� �Ʒ��� + ����ġ(31�� �Ʒ�)
                t = Spaces[31].GetChild(0).transform.DOMoveY(0.5f, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                Spaces[31].GetChild(0).transform.position = Spaces[31].position + Vector3.up * -3.5f; // ������ 31�� ĭ �Ʒ��� �̵�
                break;
        }
    }
}
