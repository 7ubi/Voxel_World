using System.Collections;
using UnityEngine;

namespace TerrainGeneration
{
    public class WorldGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject chunk;

        private const int ChunksToGenerate = 64;
        private int _numChunks = 0;
        private int _x = 0;
        private int _z = 0;
        
        private void Start()
        {
            StartCoroutine(GenerateChunks());
        }

        private void Update()
        {
            //if (!Input.GetKeyDown(KeyCode.Space)) return;
        }

        private IEnumerator GenerateChunks()
        {
            var generatedChunk = Instantiate(chunk, new Vector3(_x * 16, 0, _z * 16), Quaternion.identity);
            yield return new WaitUntil(() => generatedChunk.GetComponent<TerrainGeneration>().IsGenerated);
            _x++;
            if (_x == 8)
            {
                _z = (_z + 1) % 8;
                _x = 0;
            }
            _numChunks++;
            if(_numChunks < ChunksToGenerate)
                StartCoroutine(GenerateChunks());
        }
    }
}
