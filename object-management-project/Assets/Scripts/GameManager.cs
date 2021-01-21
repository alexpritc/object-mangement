using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : PersistableObject
{
    private const int saveVersion = 3;
    
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

    void CreateShape()
    {
        Shape instance = ShapeManager.GetRandom();
        Transform t = instance.transform;
        t.localPosition = GameLevel.CurrentLevel.SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        instance.SetColor(Random.ColorHSV(
            hueMin: 0f, hueMax: 1f,
            saturationMin: 0.5f, saturationMax: 1f,
            valueMin: 0.25f, valueMax: 1f,
            alphaMin: 1f, alphaMax: 1f
        ));
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
