using System;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private GameObject chunk;
    //private int x = 0;


    private void Start()
    {
    }

    private void Update()
    {
        
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        /*x += 8;
        Instantiate(chunk, new Vector3(x, 0, 0), Quaternion.identity);*/
        var time = DateTime.Now;

        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                Instantiate(chunk, new Vector3(x * 8, 0, z * 8), Quaternion.identity);
            }
        }
        Debug.Log("Time to generate Chunks: " + (DateTime.Now - time));
    }
}
