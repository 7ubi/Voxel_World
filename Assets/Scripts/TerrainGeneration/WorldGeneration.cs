using System;
using System.Collections;
using UnityEngine;

namespace TerrainGeneration
{
    public class WorldGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject chunk;

        private const int ChunksToGenerate = 32 * 32;
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
            if (_x == (int)Math.Sqrt(ChunksToGenerate))
            {
                _z = (_z + 1) % (int)Math.Sqrt(ChunksToGenerate);
                _x = 0;
            }
            _numChunks++;
            if(_numChunks < ChunksToGenerate)
                StartCoroutine(GenerateChunks());
        }
    }
}
