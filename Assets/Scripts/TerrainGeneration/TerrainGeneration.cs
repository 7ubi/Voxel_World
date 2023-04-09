using UnityEngine;

namespace TerrainGeneration
{
    public class TerrainGeneration : MonoBehaviour
    {
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _triangleBuffer;

        private readonly Vertex[] _vertices = new Vertex[210];
        private readonly int[] _triangles = new int[210];
        
        private const int SourceVertStride = sizeof(float) * (3 + 3 + 2);

        private MeshFilter _meshFilter;
        
        public ComputeShader terrainGenerationCompute;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            GenerateTerrain();
        }
        
        private Mesh GenerateMesh()
        {
            var mesh = new Mesh();
            var vertices = new Vector3[_vertices.Length];
            var normals = new Vector3[_vertices.Length];
            var uvs = new Vector2[_vertices.Length];
            for(var i = 0; i < _vertices.Length; i++) {
                var vertex = _vertices[i];
                vertices[i] = vertex.position;
                normals[i] = vertex.normal;
                uvs[i] = vertex.uv;
            }
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.triangles = _triangles;
            mesh.Optimize();
            return mesh;
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

            terrainGenerationCompute.Dispatch(kernelId, 8, 1, 1);
            
            _verticesBuffer.GetData(_vertices);
            _triangleBuffer.GetData(_triangles);
            
            _meshFilter.mesh = GenerateMesh();
        }

        private void OnDestroy()
        {
            _verticesBuffer.Dispose();
            _triangleBuffer.Dispose();
        }
    }
}