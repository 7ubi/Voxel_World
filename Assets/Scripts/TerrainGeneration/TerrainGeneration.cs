using System;
using System.Runtime.CompilerServices;
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

        private int _kernelId;
        
        private const int SourceVertStride = sizeof(float) * (3 + 3 + 2);
        
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _triangleBuffer;
        private ComputeBuffer _blockIdBuffer;

        private Vertex[] _verticesFromBuffer;
        private int[] _triangles;
        private int[] _blockIds;
        
        private Mesh _mesh;

        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _uvs;
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        private bool _isGenerated = false;

        public bool IsGenerated => _isGenerated;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _verticesFromBuffer = new Vertex[36 * chunkSize * chunkSize * maxHeight];
            _triangles = new int[36 * chunkSize * chunkSize * maxHeight];
            _blockIds = new int[(chunkSize + 3) * (chunkSize + 3) * (maxHeight + 2)];
            GenerateTerrain();
        }
        
        private void GenerateMesh()
        {
            _vertices = new Vector3[_verticesFromBuffer.Length];
            _normals = new Vector3[_verticesFromBuffer.Length];
            _uvs = new Vector2[_verticesFromBuffer.Length];
            for(var i = 0; i < _verticesFromBuffer.Length; i++) {
                var vertex = _verticesFromBuffer[i];
                _vertices[i] = vertex.position;
                _normals[i] = vertex.normal;
                _uvs[i] = vertex.uv;
            }
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetUVs(0, _uvs);
            _mesh.triangles = _triangles;
            _meshFilter.mesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
        }
        
        private void CreateBuffer()
        {
            _kernelId = terrainGenerationCompute.FindKernel("create_chunk");

            _verticesBuffer = new ComputeBuffer(_verticesFromBuffer.Length, SourceVertStride, ComputeBufferType.Append);
            _triangleBuffer = new ComputeBuffer(_triangles.Length, sizeof(int), ComputeBufferType.Append);
            _blockIdBuffer = new ComputeBuffer(_blockIds.Length, sizeof(int), ComputeBufferType.Append);

            _verticesBuffer.SetData(_verticesFromBuffer);
            _triangleBuffer.SetData(_triangles);
            _blockIdBuffer.SetData(_blockIds);
            terrainGenerationCompute.SetBuffer(_kernelId, "generated_vertices", _verticesBuffer);
            terrainGenerationCompute.SetBuffer(_kernelId, "generated_triangles", _triangleBuffer);
            terrainGenerationCompute.SetBuffer(_kernelId, "block_ids", _blockIdBuffer);
            terrainGenerationCompute.SetVector("offset", new Vector4(transform.position.x, 0, transform.position.z, 0));
            terrainGenerationCompute.SetFloat("min_height", minHeight);
            terrainGenerationCompute.SetFloat("max_height", maxHeight);
            terrainGenerationCompute.SetInt("chunk_size", chunkSize);
        }

        private void GenerateTerrain()
        {
            CreateBuffer();

            var time = DateTime.Now;
            terrainGenerationCompute.Dispatch(_kernelId, 8, 1, 1);
            Log("Time to run Shader: " + (DateTime.Now - time));
            
            time = DateTime.Now;
            _verticesBuffer.GetData(_verticesFromBuffer);
            _triangleBuffer.GetData(_triangles);
            GenerateMesh();
            _isGenerated = true;
            CleanUp();
            Log("Time to generate Mesh: " + (DateTime.Now - time));
        }

        private void CleanUp()
        {
            _verticesBuffer.Dispose();
            _triangleBuffer.Dispose();
            _blockIdBuffer.Dispose();

            _verticesFromBuffer = null;
            _vertices = null;
            _uvs = null;
            _normals = null;
            _triangles = null;
            _blockIds = null;
        }

        private void Log(string message)
        {
            if (debugMessages)
            {
                Debug.Log(message);
            }
        }
    }
}