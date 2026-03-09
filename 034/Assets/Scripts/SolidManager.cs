using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SolidManager : MonoBehaviour
{
    public Solid solid;

    public List<GameObject> connectedSolids;//和这个体连接的其他体

    private void Awake()
    {
        connectedSolids = new List<GameObject>();
    }
    private void Start()
    {
        PlayerManager.playerManager.solidsInGame.Add(this.gameObject);
    }
}
