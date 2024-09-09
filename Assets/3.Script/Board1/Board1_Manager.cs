using UnityEngine;
using DG.Tweening;

public class Board1_Manager : Board_Manager
{

    public void DebugAddScore(int pNo)
    {
        playerMarks[pNo].transform.DOMove(playerMarks[pNo].transform.position - Vector3.right * 7, 0.5f);
    }
}
