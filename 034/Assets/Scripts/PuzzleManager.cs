using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Á¬½Ó")]
    public List<GameObject> connectSolids1;
    public List<GameObject> connectSolids2;

    private void Start()
    {
        PlayerManager.playerManager.currentPuzzle = this;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < connectSolids1.Count; i++)
        {
            if (i < connectSolids2.Count)
            {
                if (connectSolids1[i] != null && connectSolids2[i] != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(connectSolids1[i].transform.position, connectSolids2[i].transform.position);
                }
            }
        }
    }
}
