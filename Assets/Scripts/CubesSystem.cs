using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubesSystem : MonoBehaviour
{
    public static CubesSystem Instance { get; private set; }

    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private LayerMask cubeLayerMask;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameStart;
    public event EventHandler OnGameWon;
    public event EventHandler OnGameLost;
    
    private bool pointerOverUI;
    private bool isBusy = false;
    private Cube previouslySelectedCube;
    private Cube selectedCube;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one CubesSystem. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.OnLeftClick += InputManager_OnLeftClick;
        InputManager.Instance.OnRightClick += InputManager_OnRightClick;
        InputManager.Instance.OnDoubleClick += InputManager_OnDoubleClick;
        InputManager.Instance.OnCubeMovement += InputManager_OnCubeMovement;

        Cube.OnAnyBombMarked += (_,_) => CheckWin();

        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        pointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }



    private void InputManager_OnLeftClick(object sender, EventArgs e)
    {
        if (isBusy) return;
        if (pointerOverUI) return;

        Cube cube = GetPointedCube();
        if (cube != null)
        {
            SetSelectedCube(cube.IsTaggedAsBombed() ? null : cube);
        }
    }

    private void InputManager_OnRightClick(object sender, EventArgs e)
    {
        if (isBusy) return;
        if (pointerOverUI) return;

        Cube cube = GetPointedCube();
        if (cube != null)
        {
            cube.NextTag();
        }
    }

    private void InputManager_OnDoubleClick(object sender, EventArgs e)
    {
        if (isBusy) return;
        if (pointerOverUI) return;
        if (previouslySelectedCube != selectedCube) return;
        if (selectedCube == null) return;

        if (selectedCube.HasBomb())
        {
            selectedCube.Explode(() => {});
            LoseGame();
            return;
        }

        if (selectedCube.HasBeenRevealed())
        {
            gridSystem.SetCubeAtGridPosition(gridSystem.GetGridPosition(selectedCube.GetPosition()), null);
            selectedCube.Explode(CheckWin);
            return;
        }

        List<Cube> closedList = new List<Cube>();
        List<Cube> openList = new List<Cube>{ selectedCube };

        while (openList.Count > 0)
        {
            Cube currentCube = openList[0];
            openList.Remove(currentCube);
            closedList.Add(currentCube);

            int bombCount = gridSystem.GetNeighbourBombCount(currentCube);
            currentCube.UpdateText(bombCount);
            currentCube.Reveal();

            if (bombCount == 0)
            {
                List<Cube> neighbourCubeList = gridSystem.GetNeighbourCubeList(currentCube);
                foreach (Cube neighbourCube in neighbourCubeList)
                {
                    if (closedList.Contains(neighbourCube)) continue;
                    else if (openList.Contains(neighbourCube)) continue;
                    else openList.Add(neighbourCube);
                }
                gridSystem.SetCubeAtGridPosition(gridSystem.GetGridPosition(currentCube.GetPosition()), null);
                currentCube.Explode(CheckWin);
            }
        }
        CheckWin();

    }

    private void InputManager_OnCubeMovement(object sender, EventArgs e)
    {
        if (isBusy) return;
        if (selectedCube == null) return;

        int cubeMovement = InputManager.Instance.GetCubeMovement();
        if (cubeMovement == 0) return;
        if (! CanMoveCube(cubeMovement)) return;

        if (selectedCube.HasBomb())
        {
            LoseGame();
            return;
        }

        SetBusy();
        selectedCube.Move(cubeMovement, () => { CheckWin(); ClearBusy(); });
    }



    private bool CanMoveCube(int cubeMovement)
    {
        Vector3 cubeDirection = selectedCube.GetDirection();

        // with GridSystem
        try
        {
            GridSystem.cubeAxis axis = gridSystem.DirectionToAxis(cubeDirection);
            Vector3Int currentGridPosition = gridSystem.GetGridPosition(selectedCube.GetPosition());
            Vector3Int targetGridPosition = currentGridPosition;
            switch (axis)
            {
                case GridSystem.cubeAxis.X:
                    targetGridPosition.x += cubeMovement;
                    break;
                case GridSystem.cubeAxis.Y:
                    targetGridPosition.y += cubeMovement;
                    break;
                case GridSystem.cubeAxis.Z:
                    targetGridPosition.z += cubeMovement;
                    break;
            }
            if (! gridSystem.IsGridPositionInsideGrid(targetGridPosition)) return true;
            return gridSystem.GetCubeAtGridPosition(targetGridPosition) == null;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            return false;
        }

        // with raycast
        /*
        Vector3 movingDirection = cubeDirection * cubeMovement;
        return ! Physics.Raycast(
            selectedCube.GetPosition(),
            movingDirection.normalized,
            1f, // max distance
            cubeLayerMask
        );
        */
    }

    private void SetBusy()
    {
        isBusy = true;
    }

    private void ClearBusy()
    {
        isBusy = false;
    }




    public int GetRemainingBombCount()
    {
        int remainingBombs = gridSystem.GetTotalBombCount();

        Cube[,,] cubeArray = gridSystem.GetCubeArray();
        if (cubeArray != null)
        {
            foreach (Cube cube in cubeArray)
            {
                if (cube == null) continue;
                if (cube.IsTaggedAsBombed()) remainingBombs--;
            }
        }

        return remainingBombs;
    }

    public int GetRemainingCubeCount()
    {
        int remainingCubes = 0;

        foreach (Cube cube in gridSystem.GetCubeArray())
        {
            if (cube != null) remainingCubes++;
        }

        return remainingCubes;
    }




    private void CheckWin()
    {
        Cube[,,] cubeArray = gridSystem.GetCubeArray();
        foreach (Cube cube in cubeArray)
        {
            if (cube == null) continue;
            if (cube.HasBomb() && cube.IsTaggedAsBombed()) continue;
            if (cube.HasBeenRevealed()) continue;
            return;
        }
        WinGame();
    }

    private void WinGame()
    {
        SetBusy();
        OnGamePaused?.Invoke(this, EventArgs.Empty);
        OnGameWon?.Invoke(this, EventArgs.Empty);
    }

    private void LoseGame()
    {
        SetBusy();
        OnGamePaused?.Invoke(this, EventArgs.Empty);
        OnGameLost?.Invoke(this, EventArgs.Empty);
    }

    public void RestartGame()
    {
        gridSystem.Restart();
        ClearBusy();
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }

    public void RestartGame(Vector3Int gridSize, int numberOfBombs)
    {
        gridSystem.Restart(gridSize, numberOfBombs);
        ClearBusy();
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }




    private Cube GetPointedCube()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        if (Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, cubeLayerMask))
        {
            if (hitInfo.transform.TryGetComponent<Cube>(out Cube cube))
            {
                return cube;
            }
        }
        return null;
    }

    private void SetSelectedCube(Cube cube)
    {
        previouslySelectedCube = selectedCube;

        if (selectedCube == cube) return;

        if (selectedCube) selectedCube.SetSelected(false);
        selectedCube = cube;
        if (selectedCube) selectedCube.SetSelected(true);
    }



    public Vector3Int GetGridSize() => gridSystem.GetGridSize();
    public int GetTotalBombCount() => gridSystem.GetTotalBombCount();

}
