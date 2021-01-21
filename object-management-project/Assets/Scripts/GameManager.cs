using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : PersistableObject
{
    private const int saveVersion = 4;
    
    [SerializeField] ShapeManager ShapeManager;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode destroyKey = KeyCode.D;

    private List<Shape> shapes;

    public PersistentStorage storage;
    
    public float CreationSpeed { get; set; }
    private float creationProgress;
    public float DestructionSpeed { get; set; }
    private float destructionProgress;
    
    private int loadedLevelBuildIndex;

    private Random.State mainRandomState;
    [SerializeField] private bool reseedOnLoad;
    
    [SerializeField] Slider creationSpeedSlider;
    [SerializeField] Slider destructionSpeedSlider;
    
    private void Awake()
    {
        CreationSpeed = 5f;
        DestructionSpeed = 1f;
    }

    void Start()
    {
        mainRandomState = Random.state;
        
        shapes = new List<Shape>();

        if (Application.isEditor)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Level "))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }
        
        NewGame();
        StartCoroutine(LoadLevel(1));
    }

    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKeyDown(newKey))
        {
            NewGame();   
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            NewGame();
            storage.Load(this);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                NewGame();
                StartCoroutine(LoadLevel(1));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                NewGame();
                StartCoroutine(LoadLevel(2));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                NewGame();
                StartCoroutine(LoadLevel(3));
            }
        }
    }

    private void FixedUpdate()
    {
        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
        
        for (int i = 0; i < shapes.Count; i++) {
            shapes[i].GameUpdate();
        }
    }

    void CreateShape()
    {
        Shape instance = ShapeManager.GetRandom();
        GameLevel.CurrentLevel.ConfigureSpawn(instance);
        shapes.Add(instance);
    }

    void DestroyShape()
    {
        if (shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            ShapeManager.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        }
    }

    void NewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int) Time.unscaledTime;
        mainRandomState = Random.state;
        Random.InitState(seed);
        
        creationSpeedSlider.value = CreationSpeed = 0f;
        destructionSpeedSlider.value = DestructionSpeed = 0f;

        for (int i = 0; i < shapes.Count; i++)
        {
            ShapeManager.Reclaim(shapes[i]);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.CurrentLevel.Save(writer);
        foreach (var instance in shapes)
        {
            writer.Write(instance.ShapeId);
            writer.Write(instance.MaterialId);
            instance.Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;

        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
    }


    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();

        if (version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad)
            {
                Random.state = state;
            }

            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
        }
        
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        if (version >= 3) {
            GameLevel.CurrentLevel.Load(reader);
        }
        
        for (int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = ShapeManager.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }
    
    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        if (loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }
}
