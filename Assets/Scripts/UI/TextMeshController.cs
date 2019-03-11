using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshController : MonoBehaviour
{
    private TextMesh mesh;
    new private Renderer renderer;

    public string sortingLayer = "Background";
    public int sortingOrder = 1;

    void Awake()
    {
        mesh = GetComponent<TextMesh>();
        renderer = mesh.GetComponent<Renderer>();
    }

    void Start()
    {
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = sortingOrder;
    }

    public void SetText(string text)
    {
        mesh.text = text;
    }
}
