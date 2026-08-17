using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour{

    [SerializeField]
    private GameObject cubePrefab;

    [SerializeField]
    private int chunksVisible = 2;
    public Transform viewer;

    public GameObject cuboidHolder;
    public int chunkSize = 10000;
    public int spaceBetweenPoints = 5000;
    public int seed = 123456789;

    public Vector2 cubeSizeRange = new Vector2(2500, 5000);

    private Vector3Int _lastViewerChunkPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    private Dictionary<Vector3Int, Chunk> _terrainChunkDic = new Dictionary<Vector3Int, Chunk>();
    private HashSet<Vector3Int> _currentlyVisibleChunks = new HashSet<Vector3Int>();

    private List<Vector3Int> _keysToRemove = new List<Vector3Int>();

    public void Update(){
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks(){

        Vector3 viewerPos = viewer.position;   //world space

        Vector3Int viewerChunkPos = new Vector3Int(  // new "map" space
            Mathf.FloorToInt(viewerPos.x / chunkSize),
            Mathf.FloorToInt(viewerPos.y / chunkSize),
            Mathf.FloorToInt(viewerPos.z / chunkSize)
        );

        if (viewerChunkPos == _lastViewerChunkPos)
            return;
        _lastViewerChunkPos = viewerChunkPos;

        _currentlyVisibleChunks.Clear();

        for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++){
            for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++){
                for (int zOffset = -chunksVisible; zOffset <= chunksVisible; zOffset++){
                    Vector3Int chunkPos = new Vector3Int(viewerChunkPos.x + xOffset, viewerChunkPos.y + yOffset, viewerChunkPos.z + zOffset); //chunk position in map space

                    _currentlyVisibleChunks.Add(chunkPos);

                    if (!(_terrainChunkDic.ContainsKey(chunkPos))){
                        _terrainChunkDic.Add(chunkPos, GenerateChunk(cuboidHolder, cubeSizeRange, seed, chunkSize, spaceBetweenPoints, chunkPos, cubePrefab));
                    }
                }
            }

        }

        _keysToRemove.Clear();
        foreach (var chunkEntry in _terrainChunkDic){
            if (!_currentlyVisibleChunks.Contains(chunkEntry.Key)){
                chunkEntry.Value.DestroyChunk();
                _keysToRemove.Add(chunkEntry.Key);
            }
        }
        foreach (var key in _keysToRemove){
            _terrainChunkDic.Remove(key);
        }
    }


    public class Chunk{

        public List<GameObject> cuboidsInChunk = new List<GameObject>();
        public Vector3Int chunkGridPos;

        public Chunk(Vector3Int gridPos){
            chunkGridPos = gridPos;
            cuboidsInChunk = new List<GameObject>();
        }
        public void DestroyChunk(){
            foreach (GameObject go in cuboidsInChunk){
                GameObject.Destroy(go);
            }
            cuboidsInChunk.Clear();
        }

    }
    public static Chunk GenerateChunk(GameObject cuboidHolder, Vector2 cubeSizeRange, int seed, int chunkSize, int spaceBetweenPoints, Vector3Int chunkPos, GameObject cubePrefab){

        Chunk chunk = new Chunk(chunkPos);

        List<Vector3> vectorList = GenerateVectorList(chunkSize, spaceBetweenPoints, chunkPos*chunkSize);

        foreach (Vector3 positionOfCube in vectorList){
            chunk.cuboidsInChunk.Add(CuboidGenerator.GenerateCuboid(positionOfCube, cuboidHolder, cubeSizeRange, seed, cubePrefab));
        }
        return chunk;
    }

    public static List<Vector3> GenerateVectorList(int maxGridSize, int spaceInbetweenPoints, Vector3 startingPos){
        List<Vector3> vectorList = new List<Vector3>();

        int pointsPerAxis = maxGridSize / spaceInbetweenPoints;

        for (int x = 0; x < pointsPerAxis; x++){
            for (int y = 0; y < pointsPerAxis; y++){
                for (int z = 0; z < pointsPerAxis; z++){
                    Vector3 offset = new Vector3(x * spaceInbetweenPoints, y * spaceInbetweenPoints, z * spaceInbetweenPoints);
                    vectorList.Add(startingPos + offset);
                }
            }
        }

        return vectorList;
    }

}
