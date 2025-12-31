using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    public const float scale = 20;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }

            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        int size;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        MapGenerator mapGen;

        System.Random prng;
        EndlessTerrain endless;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            mapGen = FindObjectOfType<MapGenerator>();

            this.size = size;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            meshObject.layer = 8;
            SetVisible(false);

            prng = new System.Random(mapGen.seed + 10000 * (int)position.x + (int)position.y);
            endless = meshObject.transform.parent.GetComponent<EndlessTerrain>();

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider) collisionLODMesh = lodMeshes[i];
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }


        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                            SpawnDecorations();
                            if (meshObject.transform.position == Vector3.zero) SpawnTube();
                        }
                        else if (!collisionLODMesh.hasRequestedMesh) collisionLODMesh.RequestMesh(mapData);
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

        public void SpawnDecorations()
        {
            for (int i = 0; i < prng.Next(1, mapData.biomeMap[0,0].decorationFrequency); i++)
            {
                // Choose a random local position within the chunk bounds
                Vector3 worldPos = meshObject.transform.position;
                Vector3 localPos = new Vector3((float)prng.NextDouble() * size - size/2, 10, (float)prng.NextDouble() * size - size/2);
                worldPos = worldPos + localPos * size;

                // Raycast down to terrain surface
                if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit))
                {
                    BiomeType biome = mapData.biomeMap[(int)localPos.x + size/2, (int)localPos.z + size/2];
                    Decoration decoration = biome.spawnableDecorations[prng.Next(0, biome.spawnableDecorations.Length)];
                    GameObject decoObject = Instantiate(decoration.prefab);

                    decoObject.transform.parent = meshObject.transform;
                    decoObject.transform.position = hit.point;
                    decoObject.layer = 8;

                    for (int j = 0; j < decoObject.transform.childCount; j++)
                    {
                        GameObject child = decoObject.transform.GetChild(j).gameObject;
                        child.AddComponent<MeshCollider>();
                        child.layer = 8;
                    }

                    // Rotate object to match the slope
                    decoObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    decoObject.transform.localScale *= Mathf.Lerp((int)decoration.minMaxScale.x, (int)decoration.minMaxScale.y, (float)prng.NextDouble());
                }
            }
        }

        public void SpawnTube()
        {
            Vector3 tubePos = meshObject.transform.position;
            tubePos.y += 100;
            if (Physics.Raycast(tubePos, Vector3.down, out RaycastHit _hit))
            {
                GameObject tubeObj = Instantiate(mapGen.tubeObj);
                tubeObj.transform.parent = meshObject.transform;
                tubeObj.transform.position = _hit.point;
                tubeObj.layer = 8;

                for (int j = 0; j < tubeObj.transform.childCount; j++)
                {
                    GameObject child = tubeObj.transform.GetChild(j).gameObject;
                    child.AddComponent<MeshCollider>();
                    child.layer = 8;
                }

                // Rotate object to match the slope
                tubeObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);
                tubeObj.transform.localScale *= 4;
                tubeObj.transform.position -= new Vector3(0, 2, 0);
            }
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }

    [System.Serializable]
    public struct Decoration
    {
        public string name;
        public GameObject prefab;
        public Vector2 minMaxScale;
    }
}