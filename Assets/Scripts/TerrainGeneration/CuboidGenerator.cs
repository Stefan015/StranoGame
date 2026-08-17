using UnityEngine;

public class CuboidGenerator{

    private static FastNoiseLite _cachedNoise;

    private static FastNoiseLite GetNoiseGenerator(int seed){
        if (_cachedNoise == null){
            _cachedNoise = new FastNoiseLite();
            _cachedNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            _cachedNoise.SetSeed(seed);
        }
        return _cachedNoise;
    }

    // creates a cuboid and returns a refrence to it
    public static GameObject GenerateCuboid(Vector3 positionOfCube,GameObject parent,Vector2 cubeSizeRange, int seed, GameObject cubePrefab){

        GameObject cube = GameObject.Instantiate(cubePrefab, positionOfCube, Quaternion.identity, parent.transform);
        Vector3 cords = GenerateCords(seed, positionOfCube, cubeSizeRange);
        cube.transform.localScale = new Vector3(cords.x, cords.y, cords.z);
        cube.layer = parent.layer;

        return cube;
    }

    //Generates _cachedNoise values and from them extracts length, width and height of object
    private static Vector3 GenerateCords(int seed,Vector3 cubePos, Vector2 cubeSizeRange){
        Vector3 result = Vector3.zero;

        float noiseValue = (GetNoiseGenerator(seed).GetNoise(cubePos.x,cubePos.y,cubePos.z) + 1) * 0.5f;

        result.x = noiseValue;
        result.y = ((noiseValue * 1000) % 10)/10;
        result.z = ((noiseValue * 100) % 10)/10;

        result.x = result.x * cubeSizeRange.y;
        result.y = result.y * cubeSizeRange.y;
        result.z = result.z * cubeSizeRange.y;

        if (result.x < cubeSizeRange.x) result.x = cubeSizeRange.x;
        if (result.y < cubeSizeRange.x) result.y = cubeSizeRange.x;
        if (result.z < cubeSizeRange.x) result.z = cubeSizeRange.x;

        return result;
    }

}
