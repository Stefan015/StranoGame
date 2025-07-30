using UnityEngine;

public class CuboidGenerator
{

    // creates a cuboid and returns a refrence to it
    public static GameObject GenerateCuboid(Vector3 vector,GameObject parent,Vector2 cubeSizeRange, int seed, int maxGridSize) {
        
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // create cube

        Texture texture = Resources.Load<Texture>("TestTexture"); // path inside Asset folder
        Material material = Resources.Load<Material>("TestMaterial1");
        material.mainTexture = texture;

        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material = material;

        Vector3 cords = GenerateCords(seed,vector,maxGridSize, cubeSizeRange);

        cube.transform.position = vector;
        cube.transform.localScale = new  Vector3(cords.x, cords.y, cords.z);
        cube.transform.parent = parent.transform;
        cube.layer = parent.layer;

        return cube;
    }

    //Generates noise values and from them extracts length, width and height of object
    private static Vector3 GenerateCords(int seed,Vector3 cubePos,int maxGridSize, Vector2 cubeSizeRange) {
        Vector3 result = Vector3.zero;
        
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        noise.SetSeed(seed);

        float noiseValue = (noise.GetNoise(cubePos.x,cubePos.y,cubePos.z) + 1) * 0.5f;

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
