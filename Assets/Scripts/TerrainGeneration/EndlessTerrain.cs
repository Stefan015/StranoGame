using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{

    
    [SerializeField]
    public int chunksVisible = 2;
    public Transform viewer;

    public GameObject cuboidHolder;
    public int chunkSize = 10000;
    public int spaceBetweenPoints = 5000;
    public int seed = 123456789;
    
    public Vector2 cubeSizeRange = new Vector2(2500, 5000);

    private static Vector3 _viewerPos;
    private Vector3Int _LastViewerChunkPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);


    Dictionary<Vector3Int, Chunk> _TerrainChunkDic = new Dictionary<Vector3Int, Chunk>();
    HashSet<Vector3Int> _CurrentlyVisibleChunks = new HashSet<Vector3Int>();

    List<Vector3Int> _KeysToRemove = new List<Vector3Int>();

    public void Update() {
        UpdateVisibleChunks();
    }
    
    void UpdateVisibleChunks() {

        _viewerPos = viewer.position; // world space

        Vector3Int viewerChunkPos = new Vector3Int(  // new "map" space
            Mathf.FloorToInt(_viewerPos.x / chunkSize),
            Mathf.FloorToInt(_viewerPos.y / chunkSize),
            Mathf.FloorToInt(_viewerPos.z / chunkSize)
        );

        if (viewerChunkPos == _LastViewerChunkPos)
            return;
        _LastViewerChunkPos = viewerChunkPos;

        _CurrentlyVisibleChunks.Clear();

        for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++) {
            for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++) {
                for (int zOffset = -chunksVisible; zOffset <= chunksVisible; zOffset++) {
                    Vector3Int chunkPos = new Vector3Int(viewerChunkPos.x + xOffset, viewerChunkPos.y + yOffset, viewerChunkPos.z + zOffset); //chunk position in map space

                    _CurrentlyVisibleChunks.Add(chunkPos);

                    if (!(_TerrainChunkDic.ContainsKey(chunkPos))) {
                        _TerrainChunkDic.Add(chunkPos, GenerateChunk(cuboidHolder, cubeSizeRange, seed, chunkSize, spaceBetweenPoints, chunkPos));
                    }
                }
            }

        }
        
        _KeysToRemove.Clear();
        foreach (var chunkEntry in _TerrainChunkDic) {
            if (!_CurrentlyVisibleChunks.Contains(chunkEntry.Key)) {
                chunkEntry.Value.DestroyChunk();
                _KeysToRemove.Add(chunkEntry.Key);
            }
        }
        foreach (var key in _KeysToRemove) {
            _TerrainChunkDic.Remove(key);
        }
    }


    public class Chunk{

        public List<GameObject> CuboidsInChunk = new List<GameObject>();
        public Vector3Int ChunkGridPos;
        
        public Chunk(Vector3Int gridPos) {
            ChunkGridPos = gridPos;
            CuboidsInChunk = new List<GameObject>();
        }
        public void DestroyChunk() {
            foreach (GameObject go in CuboidsInChunk)
            {
                GameObject.Destroy(go);
            }
            CuboidsInChunk.Clear();
        }
        public bool IsInViewDistance(Vector3Int viewerChunkPos, int viewDist) {
            int distX = Mathf.Abs(viewerChunkPos.x - ChunkGridPos.x);
            int distY = Mathf.Abs(viewerChunkPos.y - ChunkGridPos.y);
            int distZ = Mathf.Abs(viewerChunkPos.z - ChunkGridPos.z);

            return distX <= viewDist && distY <= viewDist && distZ <= viewDist;
        }

    }
    public static Chunk GenerateChunk(GameObject cuboidHolder, Vector2 cubeSizeRange, int seed, int chunkSize, int spaceBetweenPoints, Vector3Int chunkPos) {

        Chunk chunk = new Chunk(chunkPos);
        
        List<Vector3> vectorList = GenerateVectorList(chunkSize, spaceBetweenPoints, chunkPos*chunkSize);

        foreach (Vector3 v in vectorList) {
            chunk.CuboidsInChunk.Add(CuboidGenerator.GenerateCuboid(v, cuboidHolder, cubeSizeRange, seed, chunkSize));
        }
        return chunk;
    }
    
    public static List<Vector3> GenerateVectorList(int maxGridSize, int spaceInbetweenPoints, Vector3 startingPos) {
        List<Vector3> vectorList = new List<Vector3>();
    
        int pointsPerAxis = maxGridSize / spaceInbetweenPoints;
    
        for (int x = 0; x < pointsPerAxis; x++) {
            for (int y = 0; y < pointsPerAxis; y++) {
                for (int z = 0; z < pointsPerAxis; z++) {
                    Vector3 offset = new Vector3(x * spaceInbetweenPoints, y * spaceInbetweenPoints, z * spaceInbetweenPoints);
                    vectorList.Add(startingPos + offset);
                }
            }
        }

        return vectorList;
    }

}
