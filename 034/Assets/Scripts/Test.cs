using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Material material;
    // Start is called before the first frame update

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = new Material(meshRenderer.sharedMaterial);
    }
    void Start()
    {
        material = meshRenderer.materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        Color newColor = meshRenderer.materials[0].color;
        newColor.a = Mathf.Lerp(newColor.a, 0, Time.deltaTime);
        Debug.Log(meshRenderer.materials[0].color.a);
        meshRenderer.materials[0].color = newColor;
    }
}
