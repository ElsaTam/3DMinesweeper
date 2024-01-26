using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static event EventHandler OnAnyGridSystemStarted;

    [SerializeField] private Vector3Int size = new(5, 5, 5);
    [SerializeField] private float cubeSize = 1f;
    [SerializeField] private int numberOfBombs = 20;
    [SerializeField] private Transform cubeXPrefab;
    [SerializeField] private Transform cubeYPrefab;
    [SerializeField] private Transform cubeZPrefab;

    public enum cubeAxis
    {
        X,
        Y,
        Z
    }

    private Cube[,,] cubeArray;

    private void Start()
    {
        Init();
        Cube.OnAnyCubeMoved += Cube_OnAnyCubeMoved;
    }

    public void Restart(Vector3Int gridSize, int numberOfBombs)
    {
        this.size = gridSize;
        this.numberOfBombs = numberOfBombs;
        Restart();
    }

    public void Restart()
    {
        Clean();
        Init();
    }

    private void Clean()
    {
        foreach(Cube cube in cubeArray)
        {
            if (cube == null) continue;
            Destroy(cube.gameObject);
        }
        Array.Clear(cubeArray, 0, cubeArray.Length);
    }

    private void Init()
    {
        int remainingNumberOfBombs = numberOfBombs;
        int remainingNumberOfCubes = size.x * size.y * size.z;

        Array axisValues = Enum.GetValues(typeof(cubeAxis));
        cubeArray = new Cube[size.x, size.y, size.z];
        for (int x = 0; x < size.x; ++x)
        {
            for (int y = 0; y < size.y; ++y)
            {
                for (int z = 0; z < size.z; ++z)
                {
                    cubeAxis axis = (cubeAxis)axisValues.GetValue(UnityEngine.Random.Range(0, axisValues.Length));
                    Transform cubePrefab = null;
                    switch (axis)
                    {
                        case cubeAxis.X:
                            cubePrefab = cubeXPrefab;
                            break;
                        case cubeAxis.Y:
                            cubePrefab = cubeYPrefab;
                            break;
                        case cubeAxis.Z:
                            cubePrefab = cubeZPrefab;
                            break;
                    }
                    Vector3 cubePosition = new(transform.position.x + x, transform.position.y + y, transform.position.z + z);
                    Transform cubeTransform = Instantiate(cubePrefab, cubePosition, Quaternion.identity);
                    cubeTransform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
                    cubeArray[x, y, z] = cubeTransform.GetComponent<Cube>();

                    float bombProbability = (float)remainingNumberOfBombs / (float)remainingNumberOfCubes;
                    bool hasBomb = false;
                    if (UnityEngine.Random.Range(0f, 1f) < bombProbability)
                    {
                        hasBomb = true;
                        remainingNumberOfBombs--;
                    }
                    cubeArray[x, y, z].Setup(AxisToDirection(axis), hasBomb);

                    remainingNumberOfCubes--;
                }
            }
        }

        OnAnyGridSystemStarted?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 AxisToDirection(cubeAxis axis)
    {
        switch (axis)
        {
            case cubeAxis.X:
                return Vector3.right;
            case cubeAxis.Y:
                return Vector3.up;
            case cubeAxis.Z:
                return Vector3.forward;
        }
        return Vector3.zero;
    }

    public cubeAxis DirectionToAxis(Vector3 direction)
    {
        if (direction == Vector3.right) return cubeAxis.X;
        if (direction == Vector3.up) return cubeAxis.Y;
        if (direction == Vector3.forward) return cubeAxis.Z;
        throw new ArgumentException($"Direction can not be {direction}, it must be one of: {Vector3.right}, {Vector3.up} or {Vector3.forward}", nameof(direction));
    }



    public List<Cube> GetNeighbourCubeList(Cube currentCube)
    {
        List<Cube> neighbourList = new List<Cube>();

        Vector3Int gridPosition = GetGridPosition(currentCube.GetPosition());

        if (gridPosition.x - 1 >= 0)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x - 1, gridPosition.y,     gridPosition.z    ));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }
        if (gridPosition.y - 1 >= 0)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x,     gridPosition.y - 1, gridPosition.z    ));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }
        if (gridPosition.z - 1 >= 0)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x,     gridPosition.y,     gridPosition.z - 1));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }
        if (gridPosition.x + 1 < size.x)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x + 1, gridPosition.y,     gridPosition.z    ));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }
        if (gridPosition.y + 1 < size.y)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x,     gridPosition.y + 1, gridPosition.z    ));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }
        if (gridPosition.z + 1 < size.z)
        {
            Cube neighbourCube = GetCubeAtGridPosition(new Vector3Int(gridPosition.x,     gridPosition.y,     gridPosition.z + 1));
            if (neighbourCube != null) neighbourList.Add(neighbourCube);
        }

        return neighbourList;
    }

    public int GetNeighbourBombCount(Cube cube)
    {
        int neighbourBombCount = 0;
        List<Cube> neighbourList = GetNeighbourCubeList(cube);
        foreach (Cube neighbourCube in neighbourList)
        {
            if (neighbourCube.HasBomb()) neighbourBombCount++;
        }
        return neighbourBombCount;
    }



    public Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(Mathf.RoundToInt(worldPosition.x / cubeSize),
                              Mathf.RoundToInt(worldPosition.y / cubeSize),
                              Mathf.RoundToInt(worldPosition.z / cubeSize));
    }

    public Cube GetCubeAtGridPosition(Vector3Int gridPosition)
    {
        return cubeArray[gridPosition.x, gridPosition.y, gridPosition.z];
    }

    public void SetCubeAtGridPosition(Vector3Int gridPosition, Cube cube)
    {
        cubeArray[gridPosition.x, gridPosition.y, gridPosition.z] = cube;
    }

    public Cube GetCubeAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int gridPosition = GetGridPosition(worldPosition);
        return GetCubeAtGridPosition(gridPosition);
    }

    public bool IsGridPositionInsideGrid(Vector3Int gridPosition)
    {
        return    gridPosition.x < size.x && gridPosition.x >= 0
               && gridPosition.y < size.y && gridPosition.y >= 0
               && gridPosition.z < size.z && gridPosition.z >= 0;
    }




    private void Cube_OnAnyCubeMoved(object sender, EventArgs e)
    {
        Cube cube = sender as Cube;

        Vector3Int oldCubePosition = GetGridPosition(cube.GetOldPosition());
        Vector3Int newCubePosition = GetGridPosition(cube.GetPosition());

        SetCubeAtGridPosition(oldCubePosition, null);

        // check if outside of the grid
        if (!IsGridPositionInsideGrid(newCubePosition))
        {
            cube.Explode(() => {});
        }
        else
        {
            SetCubeAtGridPosition(newCubePosition, cube);
            cube.UpdateText(GetNeighbourBombCount(cube));
        }
    }

    public Vector3Int GetGridSize() => size;
    public Vector3 GetSize() => size.ConvertTo<Vector3>() * cubeSize;
    public Vector3 GetCenter()
    {
        Vector3 origin = transform.position;
        Vector3 halfSize = size.ConvertTo<Vector3>() / 2f;
        Vector3 halfCubeOffset = new(cubeSize / 2f, cubeSize / 2f, cubeSize / 2f);
        return origin + halfSize - halfCubeOffset;
    }
    public Cube[,,] GetCubeArray() => cubeArray;
    public int GetTotalBombCount() => numberOfBombs;

}
