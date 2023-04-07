using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    private ComputeBuffer verteciesBuffer;

    private Vertex[] vertices = new Vertex[9];
    
    private const int SOURCE_VERT_STRIDE = sizeof(float) * (3 + 3 + 2);

    private MeshFilter _meshFilter;
    
    public ComputeShader terrainGenerationCompute;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        GenerateTerrain();
    }
    
    private Mesh GenerateMesh(Vertex[] verts, int[] indices)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[verts.Length];
        var normals = new Vector3[verts.Length];
        var uvs = new Vector2[verts.Length];
        for(var i = 0; i < verts.Length; i++) {
            var v = verts[i];
            vertices[i] = v.position;
            normals[i] = v.normal;
            uvs[i] = v.uv;
        }
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs); // TEXCOORD0
        mesh.triangles = indices;
        mesh.Optimize(); // Let Unity optimize the buffer orders
        return mesh;
    }

    private void GenerateTerrain()
    {
        var kernelId = terrainGenerationCompute.FindKernel("CreateBlock");
        
        verteciesBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Append);

        //terrainGenerationCompute.SetVector("position", new Vector4(0, 0, 0, 0));
        verteciesBuffer.SetData(vertices);
        terrainGenerationCompute.SetBuffer(kernelId, "generated_vertecies", verteciesBuffer);
        

        terrainGenerationCompute.Dispatch(kernelId, 1, 1, 1);
        
        verteciesBuffer.GetData(vertices);

        _meshFilter.mesh = GenerateMesh(vertices, new[] { 0, 2, 1, 3, 5, 4 });
    }

    private void OnDestroy()
    {
        verteciesBuffer.Dispose();
    }
}
