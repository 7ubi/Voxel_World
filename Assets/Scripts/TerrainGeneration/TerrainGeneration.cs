using UnityEngine;

namespace TerrainGeneration
{
    public class TerrainGeneration : MonoBehaviour
    {
        private ComputeBuffer verteciesBuffer;
        private ComputeBuffer triangleBuffer;

        private Vertex[] vertices = new Vertex[210];
        private int[] triangles = new int[210];
        
        private const int SOURCE_VERT_STRIDE = sizeof(float) * (3 + 3 + 2);

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
            var vertices = new Vector3[this.vertices.Length];
            var normals = new Vector3[this.vertices.Length];
            var uvs = new Vector2[this.vertices.Length];
            for(var i = 0; i < this.vertices.Length; i++) {
                var vertex = this.vertices[i];
                vertices[i] = vertex.position;
                normals[i] = vertex.normal;
                uvs[i] = vertex.uv;
            }
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.triangles = this.triangles;
            mesh.Optimize();
            return mesh;
        }

        private void GenerateTerrain()
        {
            var kernelId = terrainGenerationCompute.FindKernel("CreateBlock");

            verteciesBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Append);
            triangleBuffer = new ComputeBuffer(triangles.Length, sizeof(int), ComputeBufferType.Append);

            verteciesBuffer.SetData(vertices);
            triangleBuffer.SetData(triangles);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_vertices", verteciesBuffer);
            terrainGenerationCompute.SetBuffer(kernelId, "generated_triangles", triangleBuffer);

            terrainGenerationCompute.Dispatch(kernelId, 8, 1, 1);
            
            verteciesBuffer.GetData(vertices);
            triangleBuffer.GetData(triangles);
            
            _meshFilter.mesh = GenerateMesh();
        }

        private void OnDestroy()
        {
            verteciesBuffer.Dispose();
            triangleBuffer.Dispose();
        }
    }
}