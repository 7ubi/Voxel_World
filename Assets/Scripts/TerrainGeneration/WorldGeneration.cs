using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private GameObject chunk;
    private int x = 0;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        x += 8;
        Instantiate(chunk, new Vector3(x, 0, 0), Quaternion.identity);
        
    }
}
