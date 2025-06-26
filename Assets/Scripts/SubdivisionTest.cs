using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class SubdivisionTest : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;


    private Geometry _geometry;
    // Start is called before the first frame update
    void Start()
    {
        if (meshFilter.mesh == null)
        {
            _geometry = Geometry.GetCube();
            var mesh = _geometry.ToMesh();
            meshFilter.mesh = mesh;
        }
        else
        {
            _geometry = new Geometry(meshFilter.mesh);
        }
        
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _geometry = _geometry.CatmullClarkSubdivision();
            meshFilter.mesh = _geometry.ToMesh();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _geometry = _geometry.LoopSubdivision();
            meshFilter.mesh = _geometry.ToMesh();
        }
    }

    private void Subdivide()
    {
        
    }
}
