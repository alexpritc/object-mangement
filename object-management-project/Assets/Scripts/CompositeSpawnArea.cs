using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CompositeSpawnArea : SpawnArea
{
    [SerializeField] private SpawnArea[] spawnAreas;

    [SerializeField] private bool isSequential;

    private int nextSequentialIndex;
    
    public override Vector3 SpawnPoint
    {
        get
        {
            int index;
            if (isSequential)
            {
                index = nextSequentialIndex++;
                
                if (nextSequentialIndex >= spawnAreas.Length) {
                    nextSequentialIndex = 0;
                }
            }
            else
            {
                 index = Random.Range(0,spawnAreas.Length);
            }
            return spawnAreas[index].SpawnPoint;
        }
    }
    
    public override void Save (GameDataWriter writer) {
        writer.Write(nextSequentialIndex);
    }

    public override void Load (GameDataReader reader) {
        nextSequentialIndex = reader.ReadInt();
    }
}
