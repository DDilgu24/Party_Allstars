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
    [SerializeField] public Transform KeyExplain; // 누를 키를 알려주는 UI 오브젝트
    [SerializeField] public Transform ViewModeCamera; // 누를 키를 알려주는 UI 오브젝트
    [SerializeField] public GameObject TurnAlert, Intro;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] DiceEdge;
    [SerializeField] public Sprite[] playerNum;
    [SerializeField] public Animator dice_ani;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // 순서를 정할 주사위 눈금
    public int[] order = new int[4] { 0, 0, 0, 0 }; // 순서. [3, 2, 1, 4] 이라면 3p > 2p > 1p > 4p 순서 임을 의미
    public bool[] isStun = new bool[5] { true, false, false, false, false }; // 캐릭터의 스턴 여부. 플레이어 번호를 기준으로 함(인덱스 0 미사용)

    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // 플레이어 번호 별 컬러
    private Vector3[] playerOffset = new Vector3[5] { Vector3.zero, new Vector3(1f, 0, 0), new Vector3(-1f, 0, 0), new Vector3(0.5f, 0, 1f), new Vector3(-0.5f, 0, 1f) }; // 플레이어 번호 별 위치 오프셋
    string[] dice_Name = new string[7] { "더블" , "느릿" , "10까지" , "456" , "짝수" , "홀수" , "그냥" };

    private int turns = 0; // 턴 수
    private int phase; // 현재 차례(0: 턴 초기 / 1~4: n번째 플레이어 / 5: 턴 종료(미니게임) / 6: 미니게임 결과(현재 상황))
    private bool EndGame = false;
    int N;
    private int applyItemNo = 6; // 적용된 주사위 인덱스 (6 = 기본)
    private int DebugMove = 0; // 디버그 용

    private IEnumerator Start()
    {
        DOTween.Init();
        N = GameManager.instance.TotalNum;
        Tween t;
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
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 플레이할 순서를 정하겠습니다!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
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
        Intro.transform.Find("Toad/Space").gameObject.SetActive(false);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("순서가 정해졌어요!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Space").gameObject.SetActive(false);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        t = Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 렛츠 파티!", 0.5f);
        yield return t.WaitForCompletion();
        Intro.transform.Find("Toad/Space").gameObject.SetActive(true);
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
        Sequence seq;
        int playerNo = order[phase - 1];
        int ifComThat0 = (playerNo > GameManager.instance.PlayerNum)? 0 : 1;
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        SetCameraTarget(playerNo);
        playerMarks[playerNo - 1].transform.position += 0.1f * Vector3.forward; // 현재 캐릭터를 앞으로 보이게
        // 2. 현재 차례가 누군지 알려주는 UI
        // 2-1. 가운데에 누구 차례인지 띄우는 오브젝트 세팅
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo * ifComThat0];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        TurnAlert.transform.Find("Space").gameObject.SetActive(false);
        yield return null;
        // 2-2. 차례 알림과 키 설정 화면 안으로
        v = new Vector3(-125, -25, 0);
        moveTween = TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        yield return moveTween.WaitForCompletion();
        TurnAlert.transform.Find("Space").gameObject.SetActive(playerNo <= GameManager.instance.PlayerNum); // 플레이어 턴일때만 Space 이미지 활성화

        // 2-3. 스페이스 바 입력(Player) 또는 0.5초 딜레이 후(COM) 넘어감
        if (playerNo > GameManager.instance.PlayerNum)
            yield return new WaitForSeconds(0.5f);
        else 
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 스턴 상태가 아니어야 3~5 진행
        if (!isStun[playerNo]) 
        {
            // 3. 선택 페이즈 전반을 하나의 코루틴에서 처리
            applyItemNo = 6;
            yield return StartCoroutine(SelectAction(playerNo));
            int move = HitDice(playerNo);
            if (applyItemNo.Equals(0))
            {
                move *= 2; // ★임시 : 더블 주사위는 결과 값 2배
                yield return new WaitForSeconds(0.5f);
                playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(0).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // 남은 눈금으로 이미지 변경
            }
            // 4. 캐릭터 이동
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").GetChild(1).gameObject.SetActive(false); // 주사위 테두리 비활성화. 숫자만 보이게
            yield return new WaitForSeconds(0.5f);
            while (move > 0)
            {
                yield return new WaitForSeconds(0.25f);

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
                    playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(false); // 주사위를 비활성화 
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
            // 5. 멈춘 칸에 맞는 이벤트 - 코루틴으로 처리
            yield return StartCoroutine(SpaceEvent_co(playerNo, CharInfoManager.instance.charinfo[playerNo - 1].score));
        }
        else // 스턴 상태 였다면
        {
            yield return new WaitForSeconds(1f);
            isStun[playerNo] = false; // 스턴 상태 해제
            playerMarks[playerNo - 1].Find("Stun").gameObject.SetActive(false); // 파티클도 해제
            CharUI[playerNo - 1].transform.Find("Stun").gameObject.SetActive(false);
        }

        // 6. 턴 종료 시
        yield return new WaitForSeconds(0.5f);
        if (EndGame) // 게임 끝났으면 엔딩
        {
            yield return StartCoroutine(CharInfoManager.instance.ResultUISetting());
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
        else // 아니면 다음 차례로
        {
            phase++;
            if (phase > GameManager.instance.TotalNum) { turns++; phase = 1; } // 모든 차례 끝나면 바로 1번째로. 미니게임 페이즈는 일단 없는 걸로
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
        int r = applyItemNo switch
        {
            0 => UnityEngine.Random.Range(1, 7),// (임시) 더블 주사위 : 일단 1~6
            1 => tf.GetChild(0).GetComponent<SpriteRenderer>().sprite.name.Last() - 48,// 느릿 주사위
            2 => UnityEngine.Random.Range(1, 11),// 10까지 주사위
            3 => UnityEngine.Random.Range(4, 7),// 456 주사위
            4 => UnityEngine.Random.Range(1, 4) * 2,// 짝수 주사위
            5 => UnityEngine.Random.Range(1, 4) * 2 - 1,// 홀수 주사위
            _ => UnityEngine.Random.Range(1, 7),
        };

        if (DebugMove > 0)
        {
            r = DebugMove;
            DebugMove = 0;
        }

        // 순서 정하기 단계인 경우
        if (turns.Equals(0))
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

    // 턴 진행 1(턴에 맞는 카메라 이동)
    public void SetCameraTarget(int playerIndex)
    {
        // 플레이어 말 오브젝트의 Transform을 카메라의 타겟으로 설정
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    // 턴 진행 3(행동을 선택하는 이벤트)
    private IEnumerator SelectAction(int playerNo)
    {
        bool thisCoAgain = true;
        int itemCount = CharInfoManager.instance.charinfo[playerNo - 1].itemCount;
        // 코루틴 전체를 while로 감싸 해당 코루틴이 특정 상황에 다시 돌아가도록 함
        while (thisCoAgain)
        {
            // 0. 적용된 주사위에 맞게 테두리 변경
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice/SMP_DiceEdge").GetComponent<SpriteRenderer>().sprite = DiceEdge[applyItemNo % 6];
            // 1. 일단 주사위가 돌아가게
            yield return new WaitForSeconds(0.25f);
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.SetActive(true); // 주사위 활성화 
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().enabled = true; // 주사위 애니도 활성화
            playerMarks[playerNo - 1].Find($"{playerNo}P_Dice").gameObject.GetComponent<Animator>().SetBool("DiceStop", false); // 주사위를 다시 돌아가게
            int keyInput = -1;
            // 2. 유효한 키(Ctrl, Shift, Spacebar)가 입력되면 다음 작업
            // COM은 아이템이 있으면 자동으로 1번째 것 사용 및 자동으로 Spacebar 입력한 걸로 처리 
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
            // 플레이어는 선택지가 뜨게 하면서, 유효한 키를 입력받기
            else
            {
                KeyExplain.Find("SelectMode").gameObject.SetActive(true);
                yield return new WaitUntil(() => VaildKeyinput() > 0);
                keyInput = VaildKeyinput();
            }
            KeyExplain.Find("SelectMode").gameObject.SetActive(false);
            // 3. 입력한 키에 따라 작업 개시
            // 3-1. 둘러보기 모드
            if (keyInput.Equals(11))
            {
                KeyExplain.Find("ViewMode").gameObject.SetActive(true);
                ViewModeCamera.position = virtualCamera.Follow.position + Vector3.forward * 1.9f; // 뷰 모드용 transform을 지금 카메라 위치로
                virtualCamera.Follow = ViewModeCamera; // 가상 카메라를 뷰 모드용 transform 보게
                virtualCamera.LookAt = ViewModeCamera;

                while (true)
                {
                    // Esc : 3-1의 루프 종료, 결과적으로 SelectAction이 다시 돌아감
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        virtualCamera.Follow = playerMarks[playerNo - 1]; // 가상 카메라를 다시 캐릭터 보게
                        virtualCamera.LookAt = playerMarks[playerNo - 1];
                        break;
                    }
                    // LeftArrow : 누르는 동안 지속적으로 카메라가 왼쪽으로
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
                    yield return null; // 이 줄에서 대기 후 다음 프레임으로 넘어감
                }

                KeyExplain.Find("ViewMode").gameObject.SetActive(false);
            }
            // 3-2. 아이템 선택 모드
            else if (keyInput.Equals(12))
            {
                KeyExplain.Find("ItemMode").gameObject.SetActive(true);
                // 아이템 개수 불러오기
                int cursor = 0;
                // 커서 초기화
                KeyExplain.Find($"ItemMode/Items/Arrow").GetComponent<RectTransform>().anchoredPosition = new Vector3(-288, 560, 0);
                for (int i = 0; i <= itemCount; i++)
                {
                    float alpha = (i.Equals(cursor)) ? 1 : 0.4f;
                    KeyExplain.Find($"ItemMode/Items/Item_{i}").GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                }
                // 텍스트 변경
                KeyExplain.Find($"ItemMode/Items/Text").GetComponent<Text>().text = "그냥 주사위";

                // 아이템 이미지 불러오기
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
                        // 커서 인덱스 변경
                        cursor += ((isRightKey)? 1 : itemCount);
                        cursor %= (itemCount + 1);
                        // 화살표 이동
                        KeyExplain.Find($"ItemMode/Items/Arrow").GetComponent<RectTransform>().anchoredPosition = new Vector3(192 * cursor - 288, 560, 0);
                        // 투명도 조절
                        for (int i = 0; i <= itemCount; i++)
                        {
                            float alpha = (i.Equals(cursor)) ? 1 : 0.4f;
                            KeyExplain.Find($"ItemMode/Items/Item_{i}").GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                        }
                        // 텍스트 변경
                        string s = "그냥 주사위";
                        if(cursor > 0) 
                            s = dice_Name[CharInfoManager.instance.charinfo[playerNo - 1].items[cursor - 1]] + " 주사위";
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

                    yield return null; // 이 줄에서 대기 후 다음 프레임으로 넘어감
                }
                KeyExplain.Find("ItemMode").gameObject.SetActive(false);
            }
            // 3-3. 선택 코루틴 종료(주사위 치기)
            else
            {
                if (keyInput < 11) DebugMove = keyInput;
                thisCoAgain = false;
            }
        }
    }
    private void Used_item(int playerNo, int cursor)
    {
        // 캐릭터 정보에서 아이템 제거
        applyItemNo = CharInfoManager.instance.charinfo[playerNo - 1].UseItem(cursor - 1);
        int itemCount = CharInfoManager.instance.charinfo[playerNo - 1].itemCount;
        // 주사위 애니메이션 바꾸기
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
        // UI 갱신
        Transform itemSlotTf = CharUI[playerNo - 1].transform.Find("Lower_Left");
        for (int i = 0; i < 3; i++)
        {
            if (i < itemCount) itemSlotTf.Find($"Item ({i + 1})").GetComponent<Image>().sprite
                    = CharInfoManager.instance.ItemSp[CharInfoManager.instance.charinfo[playerNo - 1].items[i]];
            else itemSlotTf.Find($"Item ({i + 1})").GetComponent<Image>().sprite
                    = CharInfoManager.instance.ItemSp[6];
        }
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
                int h = Mathf.Min(CharInfoManager.instance.Score1st() - n, 15); // 가중치
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
                Spaces[n].GetChild(0).GetComponent<SpriteRenderer>().sprite = CharInfoManager.instance.ItemSp[itemNo]; // 나올 아이템 sprite를 미리 변경
                // 2. 점프하는 애니메이션
                playerMarks[playerNo - 1].transform
                    .DOLocalMoveY(playerMarks[playerNo - 1].transform.position.y + 1.8f, 0.2f)
                    .SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.2f);
                // 3. 아이템이 나오는 효과
                seq = DOTween.Sequence()
                 .Append(Spaces[n].GetChild(0).transform.DOLocalMoveY(4.7f, 0.75f)) // 위로 올라오면서
                 .Join(Spaces[n].GetChild(0).transform.DOScale(new Vector3(-3f, 3f, 3f), 0.75f)); // 동시에 크기도 조금 커지게
                yield return seq.WaitForCompletion();
                // 4. 나온 아이템이 사라지면서, UI에 등장하는 효과
                // 4-1. 상단 UI의 아이템 들어갈 곳을 캐싱, scale을 0으로
                Transform itemUItf = CharUI[playerNo - 1].transform.Find($"Lower_Left/Item ({ CharInfoManager.instance.charinfo[playerNo - 1].itemCount + 1 })");
                itemUItf.localScale = Vector3.zero;
                // 4-2. 아이템 들어갈 곳에 얻은 아이템 이미지를 저장
                itemUItf.GetComponent<Image>().sprite = CharInfoManager.instance.ItemSp[itemNo];
                // 4-3. 나온 아이템은 작아지면서, 동시에 UI 아이템의 크기는 커지게
                seq = DOTween.Sequence()
                 .Append(Spaces[n].GetChild(0).transform.DOScale(0, 0.5f))
                 .Join(itemUItf.DOScale(3.5f, 0.5f))
                 .Append(itemUItf.DOScale(2.5f, 0.25f));
                // 4-4. 시퀀스 끝날때까지 대기
                yield return seq.WaitForCompletion();
                // 5. 아이템을 캐릭터 정보에 직접 반영
                CharInfoManager.instance.charinfo[playerNo - 1].GetItem(itemNo);
                // 6. 화면 상 아이템을 다시 숨기기
                Spaces[n].GetChild(0).transform.localPosition = Vector3.up * 3.5f;
                Spaces[n].GetChild(0).transform.localScale = Vector3.zero;
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
                seq = DOTween.Sequence()
                .Append(Spaces[31].GetChild(0).transform.DOLocalMoveY(-5f, 0.5f))
                .Join(playerMarks[playerNo - 1].transform.DOMoveY(0, 0.5f))
                .SetDelay(0.25f);
                yield return seq.WaitForCompletion();
                // 3. 뻐끔과 캐릭터를 28번 칸 아래로 이동
                CharInfoManager.instance.ScoreAdd(playerNo, 28 - CharInfoManager.instance.charinfo[playerNo - 1].score); // 점수를 28로 감소
                Spaces[31].GetChild(0).transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.15f; // 뻐끔을 28번 칸의 3.5 아래로 이동 + 앞에 보이게 z 조절
                playerMarks[playerNo - 1].transform.position = Spaces[28].position + Vector3.up * -3.5f + Vector3.forward * 0.1f; // 캐릭터 마크를 28번 칸의 3.5 아래로 이동
                // 4. 28번 칸 파이프 위로 올라오는 효과
                seq = DOTween.Sequence()
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



    private int VaildKeyinput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) return 11;
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) return 12;
        if (Input.GetKeyDown(KeyCode.Space)) return 13;
        // 여기서부터 디버그용
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
        // 여기까지 디버그용
        return 0;
    }
}
