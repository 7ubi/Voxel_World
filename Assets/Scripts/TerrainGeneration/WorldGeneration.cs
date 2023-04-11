using UnityEngine;

namespace TerrainGeneration
{
    public class WorldGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject chunk;
        
        private void Start()
        {
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    Instantiate(chunk, new Vector3(x * 8, 0, z * 8), Quaternion.identity);
                }
            }
        }
    }
}
