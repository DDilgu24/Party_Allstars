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
    [SerializeField] public Transform[] playerMarks; // 플레이어 말 오브젝트 배열
    [SerializeField] public Transform[] Spaces; // 발판 위치 배열

    [SerializeField] public GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] public GameObject TurnAlert, Intro;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] playerNum;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // 순서를 정할 주사위 눈금
    public int[] order = new int[4] { 0, 0, 0, 0 }; // 순서. [3, 2, 1, 4] 이라면 3p > 2p > 1p > 4p 순서 임을 의미
    public bool[] isStun = new bool[5] { true, false, false, false, false }; // 캐릭터의 스턴 여부. 플레이어 번호를 기준으로 함(인덱스 0 미사용)

    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // 플레이어 번호 별 컬러
    private Vector3[] playerOffset = new Vector3[5] { Vector3.zero, new Vector3(1f, 0, 0), new Vector3(-1f, 0, 0), new Vector3(0.5f, 0, 1f), new Vector3(-0.5f, 0, 1f) }; // 플레이어 번호 별 위치 오프셋

    private int turns = 0; // 턴 수
    private int phase; // 현재 차례(0: 턴 초기 / 1~4: n번째 플레이어 / 5: 턴 종료(미니게임) / 6: 미니게임 결과(현재 상황))
    private bool EndGame = false;
    int N;

    private IEnumerator Start()
    {
        DOTween.Init();
        N = GameManager.instance.Total_Num;
        for (int i = N; i < 4; i++)
        {
            // 인원 수를 초과하는 캐릭터 마크 비활성화
            CharUI[i].SetActive(false);
            playerMarks[i].gameObject.SetActive(false);
        }
        // 1단계 : 인트로
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").GetComponent<RectTransform>().DOSizeDelta(new Vector2(1192, 464), 3.0f).SetEase(Ease.InQuad);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2단계 : 게임 준비
        // 2-1. 게임 시작을 알리는 문구
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 플레이할 순서를 정하겠습니다!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. 순서 정하기
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 0; i < 4; i++)
        {
            if (i >= N)
            {
                // 인원 수 이상의 인덱스의 순서 주사위는 -1로 처리
                orderDecideNum[i] = -1;
                continue;
            }
            yield return new WaitForSeconds(0.2f);
            playerMarks[i].Find($"{i + 1}P_Dice").gameObject.SetActive(true); // 주사위 활성화 
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // 모든 인원 주사위 결과 나올 때까지 대기
        // 2-3. 순서 결정 및 UI 재배치
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. 진짜 게임 시작 전
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("순서가 정해졌어요!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 렛츠 파티!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("게임 시작");
        for (int i = 0; i < N ; i++)
        {
            // 모든 주사위 비활성화
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
        int playerNo = order[phase - 1];
        int ifComThat0 = (playerNo > GameManager.instance.Player_Num)? 0 : 1;
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        SetCameraTarget(playerNo);
        playerMarks[playerNo - 1].transform.position += 0.1f * Vector3.forward; // 현재 캐릭터를 앞으로 보이게
        // 2. 현재 차례가 누군지 알려주는 UI
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo * ifComThat0];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        yield return null;
        v = new Vector3(-125, -25, 0);
        moveTween = TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        yield return moveTween.WaitForCompletion();
        // 3. 스페이스 바 누르면 계속 진행(COM 차례에는 0.5초)
        if(playerNo > GameManager.instance.Player_Num)
            yield return new WaitForSeconds(0.5f);
        else 
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 스턴 상태가 아니어야 4~7 진행
        if (!isStun[playerNo]) 
        {
            // 4. [일단 패스] 아이템 선택 받기
            // 5. 주사위 두드리기
            yield return new WaitForSeconds(0.25f);
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(true); // 주사위 활성화 
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().enabled = true; // 주사위 애니도 활성화
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().SetBool("DiceStop", false); // 주사위를 다시 돌아가게
            if (playerNo > GameManager.instance.Player_Num)
                yield return new WaitForSeconds(1.0f);
            else
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            int move = HitDice(playerNo);
            // 6. 캐릭터 이동
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(1).gameObject.SetActive(false); // 주사위 테두리 비활성화. 숫자만 보이게
            yield return new WaitForSeconds(0.5f);
            while (move > 0)
            {
                yield return new WaitForSeconds(0.25f);
                /*
                Vector3 oldpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // 현재 칸을 시작 좌표로
                CharInfoManager.instance.ScoreAdd(playerNo); // 점수를 1 더하기
                Vector3 newpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // 다음 칸을 도착 좌표로
                Vector3 midpos = (oldpos + newpos) * 0.5f; // 중간 좌표를 계산
                midpos.y = Mathf.Max(oldpos.y, newpos.y) + 2; // 중간 좌표의 y값을 보정
                Vector3[] movepath = new Vector3[] { oldpos, midpos, newpos };
                moveTween = playerMarks[playerNo - 1].transform.DOPath(movepath, 0.3f, PathType.CatmullRom).SetEase(Ease.Linear); // 캐릭터 말을 포물선으로 이동
                */

                CharInfoManager.instance.ScoreAdd(playerNo); // 점수를 1 더하기
                int newscore = CharInfoManager.instance.charinfo[playerNo - 1].score; // 바뀐 점수를 캐싱
                Vector3 newpos = Spaces[newscore].position + Vector3.forward * 0.1f; // 다음 칸을 도착 좌표로

                bool moveIsJump = false; // 다음 칸에 따라 캐릭터 이동 방식이 결정 (직선 OR 점프)
                if (newscore >= 23 && newscore != 30) moveIsJump = true;
                else if (4 <= newscore && newscore <= 11) moveIsJump = true;

                if (moveIsJump) moveTween = playerMarks[playerNo - 1].transform.DOLocalJump(newpos, 2f, 1, 0.3f); // 캐릭터 말 이동 - 점프
                else moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 0.3f).SetEase(Ease.Linear); // 캐릭터 말 이동 - 직선

                yield return moveTween.WaitForCompletion(); // 말 이동이 끝날 때까지 대기
                move--; // 남은 눈금 1 감소
                playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // 남은 눈금으로 이미지 변경
                if (CharInfoManager.instance.charinfo[playerNo - 1].score > 33) // 골대 도착하면
                {
                    BD1SoundManager.instance.BGMPlayer.pitch = 1f;
                    move = 0;
                    EndGame = true;
                    newpos += Vector3.down * 9.5f;
                    BD1SoundManager.instance.StopBGM();
                    BD1SoundManager.instance.PlaySFX("FlagDown");
                    CharUI[0].transform.parent.GetComponent<RectTransform>().DOMoveY(1000, 0.5f); // 순위표 UI 치우기
                    moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 1.5f).SetEase(Ease.Linear); // 깃대 아래로 이동
                    yield return moveTween.WaitForCompletion(); // 말 이동이 끝날 때까지 대기
                    playerMarks[playerNo - 1].transform.DOMove(Spaces[35].position + Vector3.forward * 2, 0.5f).SetEase(Ease.Linear);
                    BD1SoundManager.instance.PlaySFX("Victory");
                }
                else if (CharInfoManager.instance.charinfo[playerNo - 1].score >= 25) // 25점 도달하면
                    BD1SoundManager.instance.BGMPlayer.pitch = 1.1f;
            }
            yield return new WaitForSeconds(0.1f);
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(1).gameObject.SetActive(true); // 주사위 테두리 다시 활성화하고
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(false); // 주사위를 비활성화 
            // 7. 멈춘 칸에 맞는 이벤트 - 메소드로 처리
            yield return StartCoroutine(SpaceEvent_co(playerNo, CharInfoManager.instance.charinfo[playerNo - 1].score));
        }
        else // 스턴 상태 였다면
        {
            yield return new WaitForSeconds(1f);
            isStun[playerNo] = false; // 스턴 상태 해제
            playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(false); // 파티클도 해제
            CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(false);
        }

        // 8. 다음 차례로
        yield return new WaitForSeconds(0.5f);
        if (EndGame) // 게임 끝났으면
        {
            yield return StartCoroutine(CharInfoManager.instance.ResultUISetting());
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
        else // 아니면
        {
            phase++;
            if (phase > GameManager.instance.Total_Num) { turns++; phase = 1; } // 모든 차례 끝나면 바로 1번째로. 미니게임 페이즈는 일단 없는 걸로
            StartCoroutine(PlayerTurn_co()); // 다음 플레이어로
        }
    }

    // 준비 2-2(순서 정하기) + 턴 진행 5(주사위 두드리기) 관련 메소드
    private int HitDice(int p)
    {
        Transform tf = playerMarks[p - 1].Find($"{p}P_Dice"); // 주사위를 캐시
        playerMarks[p - 1].transform.DOLocalMoveY(playerMarks[p - 1].transform.position.y + 1.8f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // 캐릭터 점프 효과
        tf.transform.DOLocalMoveY(3 - 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // 주사위는 가만히 있도록 보이게
        tf.GetChild(2).GetComponent<ParticleSystem>().Play(); // 파티클 재생
        int r = UnityEngine.Random.Range(1, 7); // 주사위 범위: 1~6

        // 순서 정하기 단계인 경우
        if(turns.Equals(0))
        {
            while (orderDecideNum.Any(value => value.Equals(r))) // 순서 정하기 이므로 중복을 배제
                r = UnityEngine.Random.Range(1, 7);
            orderDecideNum[p - 1] = r;
            if (p.Equals(1))
            {
                for (int i = 2; i <= N; i++) StartCoroutine(HitDice_co(i));
            }
        }

        tf.GetComponent<Animator>().SetBool("DiceStop",true); // 주사위: 애니메이션을 정지하고 이미지를 결과값으로 변경 
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        return r;
    }

    private IEnumerator HitDice_co(int p)
    {
        float delayTime = 0.01f;
        if (p > GameManager.instance.Player_Num) 
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

    // 턴 진행 1(턴에 맞는 카메라 이동)
    public void SetCameraTarget(int playerIndex)
    {
        // 플레이어 말 오브젝트의 Transform을 카메라의 타겟으로 설정
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    // 턴 진행 7(도착한 칸에 대한 이벤트)
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
                // 굼바 효과
                // 1. 굼바가 캐릭터에게 몸통 박치기
                t = Spaces[n].GetChild(0).transform.DOLocalMoveZ(-1f, 0.3f).SetEase(Ease.OutQuad);
                yield return t.WaitForCompletion();
                // 2. 캐릭터 위에 스턴 파티클 
                playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(true);
                CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(true);
                // 3. 굼바 원위치
                t = Spaces[n].GetChild(0).transform.DOLocalMoveZ(-3, 0.75f);
                yield return t.WaitForCompletion();
                // 4. 캐릭터에게 스턴 효과 반영
                isStun[playerNo] = true;
                break;
            case 2:
            case 11:
            case 18:
            case 19:
                // 아이템 효과
                // 0. 3개 꽉 찼으면 받을 수 없게
                if(CharInfoManager.instance.charinfo[playerNo - 1].itemCount.Equals(3))
                {
                    break;
                }
                // 1. 랜덤으로 주사위 선정
                int r = UnityEngine.Random.Range(0, 100);
                int itemNo = 5;
                int h = Mathf.Max(CharInfoManager.instance.Score1st() - n, 15); // 가중치
                int[] itemRange = new int[4] {h + 5, 2*h + 10, 3*h + 20, 4*h + 30}; // 아이템 확률표
                for (int i = 0; i < 4; i++)
                {
                    if(r < itemRange[i])
                    {
                        itemNo = i;
                        break;
                    }
                }
                if (itemNo.Equals(5)) itemNo -= (r % 2); // 꽝에 해당하는 경우 4,5 번중 하나
                // 2. 점프하는 애니매이션
                playerMarks[playerNo - 1].transform
                    .DOLocalMoveY(playerMarks[playerNo - 1].transform.position.y + 1.8f, 0.2f)
                    .SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                // 3. 아이템이 나오는 효과
                seq = DOTween.Sequence().SetAutoKill(false)
                 .Append(Spaces[n].GetChild(0).transform.DOLocalMoveY(4.7f, 0.75f)) // 위로 올라오면서
                 .Join(Spaces[n].GetChild(0).transform.DOScale(new Vector3(-3f, 3f, 3f), 0.75f)); // 동시에 크기도 조금 커지게
                yield return seq.WaitForCompletion();
                // 4. 나온 아이템이 사라지면서, UI에 등장하는 효과;
                // 4-1. 상단 UI의 아이템 들어갈 곳을 캐싱, scale을 0으로
                Transform itemUItf = CharUI[playerNo - 1].transform.Find($"Lower_Left/Item ({ CharInfoManager.instance.charinfo[playerNo - 1].itemCount + 1 })");
                itemUItf.localScale = Vector3.zero;
                // 4-2. 아이템 들어갈 곳에 얻은 아이템 이미지를 저장
                // itemUItf.GetComponent<Image>().sprite = ;
                // 4-2. 나온 아이템은 작아지면서, 동시에 크기는 커지게
                seq = DOTween.Sequence().SetAutoKill(false)
                 .Append(Spaces[n].GetChild(0).transform.DOScale(0, 0.75f))
                 .Join(itemUItf.DOScale(2.5f, 0.75f));
                // 4-3. 시퀀스 끝날때까지 대기
                yield return seq.WaitForCompletion();
                // 5. 아이템을 캐릭터 정보에 직접 반영
                CharInfoManager.instance.charinfo[playerNo - 1].GetItem(itemNo);
                // 6. 아이템 
                break;
            case 12:
            case 15:
                // 추락 효과
                // 1. 시네머신의 추적 효과를 임시로 끔
                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
                // 2. 캐릭터가 아래로 떨어짐
                t = playerMarks[playerNo - 1].transform.DOMoveY(-5, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                // 3. 캐릭터를 11번 칸으로 이동
                CharInfoManager.instance.ScoreAdd(playerNo, 11 - CharInfoManager.instance.charinfo[playerNo - 1].score); // 점수를 11로 감소
                playerMarks[playerNo - 1].transform.position = Spaces[11].position + Vector3.up * 5f + Vector3.forward * 0.1f; // 캐릭터 마크를 11번 칸 위로 이동
                SetCameraTarget(playerNo); // 시네머신 추적 다시 활성화
                t = playerMarks[playerNo - 1].transform.DOMoveY(2, 0.5f).SetEase(Ease.InQuad); // y 값을 11번 칸과 동일하게
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                break;
            case 31:
                // 뻐끔 효과
                // 1. 뻐끔플라워가 위로 올라옴
                t = Spaces[31].GetChild(0).transform.DOLocalMoveY(0.5f, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                // 2. 캐릭터와 함께 아래로 내려감
                seq = DOTween.Sequence().SetAutoKill(false)
                .Append(Spaces[31].GetChild(0).transform.DOLocalMoveY(-5f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(0, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 3. 뻐끔과 캐릭터를 28번 칸 아래로 이동
                CharInfoManager.instance.ScoreAdd(playerNo, 28 - CharInfoManager.instance.charinfo[playerNo - 1].score); // 점수를 28로 감소
                Spaces[31].GetChild(0).transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // 뻐끔을 28번 칸의 3.5 아래로 이동 + 앞에 보이게 z 조절
                playerMarks[playerNo - 1].transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.1f; // 캐릭터 마크를 28번 칸의 3.5 아래로 이동
                // 4. 28번 칸 파이프 위로 올라오는 효과
                seq = DOTween.Sequence().SetAutoKill(false)
                .Append(Spaces[31].GetChild(0).transform.DOMoveY(4f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(4f, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 5. 캐릭터에 스턴 효과 부여
                playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(true);
                CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(true);
                isStun[playerNo] = true;
                yield return new WaitForSeconds(0.5f);
                // 6. 뻐끔플라워는 다시 아래로 + 원위치(31번 아래)
                t = Spaces[31].GetChild(0).transform.DOMoveY(0.5f, 0.5f);
                yield return t.WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                Spaces[31].GetChild(0).transform.position = Spaces[31].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // 뻐끔을 31번 칸 아래로 이동
                break;
        }
    }
}
