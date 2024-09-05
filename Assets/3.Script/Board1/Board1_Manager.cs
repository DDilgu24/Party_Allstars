using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board1_Manager : MonoBehaviour
{
    [SerializeField] private GameObject Intro; 
    private void Awake()
    {

    }

    private IEnumerator Start()
    {
        BD1SoundManager.instance.PlayBGM("Intro");
        yield return new WaitForSeconds(8.65f);
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").gameObject.SetActive(false);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "그럼 플레이할 순서를 정하겠습니다!";
    }


}
