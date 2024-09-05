using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    public bool DieisLooping = true;
    [SerializeField] private GameObject[] CharSp;
    [SerializeField] private GameObject[] Dice6;
    [SerializeField] private Sprite[] DiceNum;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && DieisLooping)
        {
            DieisLooping = false;
            CharSp[0].GetComponent<Animator>().SetTrigger("HasJump");
            HitDice(0);
        }
    }

    private void HitDice(int p = 0)
    {
        Dice6[p].transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        int r = Random.Range(1, 7);
        Dice6[p].GetComponent<Animator>().SetTrigger("WasHit");
        Dice6[p].GetComponent<Animator>().enabled = false;
        Dice6[p].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        if(p.Equals(0))
        {
            StartCoroutine(ComHitDice(1));
            StartCoroutine(ComHitDice(2));
            StartCoroutine(ComHitDice(3));
        }
    }

    private IEnumerator ComHitDice(int p)
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));
        CharSp[p].GetComponent<Animator>().SetTrigger("HasJump");
        HitDice(p);
    }
}
