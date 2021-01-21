using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : PersistableObject
{
    [SerializeField] private SpawnArea spawnArea;
    
    public static GameLevel CurrentLevel { get; private set; }

    [SerializeField] private PersistableObject[] persistentObjects;
    
    void OnEnable () 
    {
        CurrentLevel = this;
        
        if (persistentObjects == null) 
        {
            persistentObjects = new PersistableObject[0];
        }
    }

    public void ConfigureSpawn(Shape shape) {
        spawnArea.ConfigureSpawn(shape);
    }
    
    public override void Save(GameDataWriter writer)
    {
        writer.Write(persistentObjects.Length);
        for (int i = 0; i < persistentObjects.Length; i++) 
        {
            persistentObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for (int i = 0; i < savedCount; i++)
        {
            persistentObjects[i].Load(reader);
        }
    }
}
