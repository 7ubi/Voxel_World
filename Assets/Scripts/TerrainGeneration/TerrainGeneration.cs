using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGeneration
{
    public class TerrainGeneration : MonoBehaviour
    {
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _triangleBuffer;

        private Vertex[] _vertices;
        private int[] _triangles;
        
        private const int SourceVertStride = sizeof(float) * (3 + 3 + 2);

        private MeshFilter _meshFilter;
        
        public ComputeShader terrainGenerationCompute;

        private Mesh _mesh;

        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector2[] uvs;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = new Mesh();
            _vertices = new Vertex[36 * 16 * 16 * 16];
            _triangles = new int[36 * 16 * 16 * 16];
            GenerateTerrain();
        }
        
        private void GenerateMesh()
        {
            
            var time = DateTime.Now;
            vertices = new Vector3[_vertices.Length];
            normals = new Vector3[_vertices.Length];
            uvs = new Vector2[_vertices.Length];
            for(var i = 0; i < _vertices.Length; i++) {
                var vertex = _vertices[i];
                vertices[i] = vertex.position;
                normals[i] = vertex.normal;
                uvs[i] = vertex.uv;
            }
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.triangles = _triangles;
            _meshFilter.mesh = _mesh;
        }

        private void GenerateTerrain()
        {
            var kernelId = terrainGenerationCompute.FindKernel("CreateBlock");

            _verticesBuffer = new ComputeBuffer(_vertices.Length, SourceVertStride, ComputeBufferType.Append);
            _triangleBuffer = new ComputeBuffer(_triangles.Length, sizeof(int), ComputeBufferType.Append);

            _verticesBuffer.SetData(_vertices);
            _triangleBuffer.SetData(_triangles);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_vertices", _verticesBuffer);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_triangles", _triangleBuffer);
            terrainGenerationCompute.SetVector("offset", new Vector4(transform.position.x, 0, transform.position.z, 0));

            var time = DateTime.Now;
            terrainGenerationCompute.Dispatch(kernelId, 8, 1, 1);
            Debug.Log("Time to run Shader: " + (DateTime.Now - time));
            
            time = DateTime.Now;
            _verticesBuffer.GetData(_vertices);
            _triangleBuffer.GetData(_triangles);
            GenerateMesh();
            Debug.Log("Time to generate Mesh: " + (DateTime.Now - time));

        }

        private void OnDestroy()
        {
            _verticesBuffer.Dispose();
            _triangleBuffer.Dispose();
        }
    }
}