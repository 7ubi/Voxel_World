using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerrainGeneration
{
    public class TerrainGeneration : MonoBehaviour
    {
        [SerializeField] private ComputeShader terrainGenerationCompute;
        
        [SerializeField] private int minHeight;
        [SerializeField] private int maxHeight;
        [SerializeField] private int chunkSize;

        [SerializeField] private bool debugMessages;
        
        private const int SourceVertStride = sizeof(float) * (3 + 3 + 2);
        
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _triangleBuffer;
        private ComputeBuffer _blockIdBuffer;

        private Vertex[] _vertices;
        private int[] _triangles;
        private int[] _blockIds;
        
        private Mesh _mesh;

        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector2[] uvs;
        
        private MeshFilter _meshFilter;
        
        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = new Mesh();
            _vertices = new Vertex[36 * chunkSize * chunkSize * maxHeight];
            _triangles = new int[36 * chunkSize * chunkSize * maxHeight];
            _blockIds = new int[(chunkSize + 3) * (chunkSize + 3) * (maxHeight + 2)];
            GenerateTerrain();
        }
        
        private void GenerateMesh()
        {
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
            var kernelId = terrainGenerationCompute.FindKernel("create_chunk");

            _verticesBuffer = new ComputeBuffer(_vertices.Length, SourceVertStride, ComputeBufferType.Append);
            _triangleBuffer = new ComputeBuffer(_triangles.Length, sizeof(int), ComputeBufferType.Append);
            _blockIdBuffer = new ComputeBuffer(_blockIds.Length, sizeof(int), ComputeBufferType.Append);

            _verticesBuffer.SetData(_vertices);
            _triangleBuffer.SetData(_triangles);
            _blockIdBuffer.SetData(_blockIds);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_vertices", _verticesBuffer);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_triangles", _triangleBuffer);
            terrainGenerationCompute.SetBuffer(kernelId, "block_ids", _blockIdBuffer);
            terrainGenerationCompute.SetVector("offset", new Vector4(transform.position.x, 0, transform.position.z, 0));
            terrainGenerationCompute.SetFloat("min_height", minHeight);
            terrainGenerationCompute.SetFloat("max_height", maxHeight);
            terrainGenerationCompute.SetInt("chunk_size", chunkSize);

            var time = DateTime.Now;
            terrainGenerationCompute.Dispatch(kernelId, 8, 1, 1);
            Log("Time to run Shader: " + (DateTime.Now - time));
            
            time = DateTime.Now;
            _verticesBuffer.GetData(_vertices);
            _triangleBuffer.GetData(_triangles);
            GenerateMesh();
            Log("Time to generate Mesh: " + (DateTime.Now - time));

        }

        private void OnDestroy()
        {
            _verticesBuffer.Dispose();
            _triangleBuffer.Dispose();
            _blockIdBuffer.Dispose();
        }

        private void Log(String message)
        {
            if (debugMessages)
            {
                Debug.Log(message);
            }
        }
    }
}