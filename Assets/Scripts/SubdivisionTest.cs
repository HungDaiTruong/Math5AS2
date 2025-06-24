using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class SubdivisionTest : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        var cube = Geometry.GetCube();
        var mesh = cube.ToMesh();
        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
