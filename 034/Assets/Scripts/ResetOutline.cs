using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetOutline : MonoBehaviour
{
    PlayerManager playerManager;

    public Material outlineMaterial;
    Material material;

    private void Awake()
    {
        AddMaterialForTriangle();
        material = this.GetComponent<MeshRenderer>().materials[1];
    }

    private void Start()
    {
        playerManager = PlayerManager.playerManager;
        ChangeZWrite();
    }

    private void Update()
    {
        HandleReset();
    }

    public void HandleReset()
    {
        if (playerManager.currentSolid!=this.gameObject)
        {
            material.SetFloat("_OpenOutline", 0);
        }
    }

    public void AddMaterialForTriangle()
    {
        if (GetComponent<SolidManager>().solid==Solid.Triangle)
        {
            Material[] currentMaterials = GetComponent<MeshRenderer>().materials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                newMaterials[i] = currentMaterials[i];
            }
            newMaterials[currentMaterials.Length] = outlineMaterial;
            GetComponent<MeshRenderer>().materials = newMaterials;
        }
    }

    public void ChangeZWrite()
    {
        this.GetComponent<MeshRenderer>().materials[0].SetInt("_ZWrite", 1);
    }
}
