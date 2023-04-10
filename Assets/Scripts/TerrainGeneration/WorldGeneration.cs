using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private GameObject chunk;
    private int x = 0;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        var newChunk = Instantiate(chunk);
        x += 8;
        chunk.transform.position = new Vector3(x, 0, 0);
    }
}
