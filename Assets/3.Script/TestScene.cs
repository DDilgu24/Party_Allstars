using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    public bool DieisLooping = true;
    [SerializeField] private GameObject CharSp;
    [SerializeField] private GameObject Dice6;
    [SerializeField] private Sprite[] DiceNum;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && DieisLooping)
        {
            DieisLooping = false;
            CharSp.GetComponent<Animator>().SetTrigger("HasJump");
            Invoke("HitDice", 0.25f);
        }
    }

    private void HitDice()
    {
        Dice6.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        int r = Random.Range(1, 7);
        print(r);
        Dice6.GetComponent<Animator>().SetTrigger("WasHit");
        Dice6.GetComponent<Animator>().enabled = false;
        Dice6.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
    }
}
