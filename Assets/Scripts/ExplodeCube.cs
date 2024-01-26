using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeCube : MonoBehaviour
{
    private int cubePerAxis = 4;
    private float destroyTime = 1f;
    private float force = 300f;
    private float radius = 2f;

    public void Run()
    {
        for (int x = 0; x < cubePerAxis; ++x)
        {
            for (int y = 0; y < cubePerAxis; ++y)
            {
                for (int z = 0; z < cubePerAxis; ++z)
                {
                    CreateSmallCube(new Vector3(x, y, z));
                }
            }
        }
        Destroy(gameObject);
    }

    private void CreateSmallCube(Vector3 position)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        Renderer rd = cube.GetComponent<Renderer>();
        rd.material = gameObject.GetComponentInChildren<Renderer>().material;

        cube.transform.localScale = transform.localScale / cubePerAxis;

        Vector3 firstCubePosition = transform.position - transform.localScale / 2f + cube.transform.localScale / 2f;
        cube.transform.position = firstCubePosition + Vector3.Scale(position, cube.transform.localScale);

        Rigidbody rigidbody = cube.AddComponent<Rigidbody>();
        rigidbody.AddExplosionForce(force, transform.position, radius);

        Destroy(cube, destroyTime);
    }
}
