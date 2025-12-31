using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 15;
    public const int editorPreviewMapChunkSize = 200;
    [Range(0, 6)]
    public int editorPreviewLOD;
    public float duneNoiseScale;
    public float biomeNoiseScale;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public GameObject tubeObj;
    public TerrainType[] regions;
    public BiomeType[] biomes;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero, true);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre, bool isInEditor = false)
    {
        int chunkSize = (isInEditor) ? editorPreviewMapChunkSize : mapChunkSize;
        float[,] noiseMap = Noise.GenerateCellularNoiseMap(chunkSize + 2, chunkSize + 2, seed, duneNoiseScale, centre, normalizeMode);
        float[,] biomeMapFloat = Noise.GenerateCellularNoiseMap(chunkSize + 2, chunkSize + 2, seed, biomeNoiseScale, centre, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        BiomeType[,] biomeMap = new BiomeType[mapChunkSize, mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }

                float currentBiome = biomeMapFloat[x, y];
                for (int i = 0; i < biomes.Length; i++)
                {
                    if (currentBiome >= biomes[i].minNoiseValue)
                    {
                        biomeMap[x,y] = biomes[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        return new MapData(noiseMap, colorMap, biomeMap);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

[System.Serializable]
public struct BiomeType
{
    public string name;
    public float minNoiseValue;
    public int decorationFrequency;
    public EndlessTerrain.Decoration[] spawnableDecorations;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    public readonly BiomeType[,] biomeMap;

    public MapData(float[,] heightMap, Color[] colorMap, BiomeType[,] biomeMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
        this.biomeMap = biomeMap;
    }
}