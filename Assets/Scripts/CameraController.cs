using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineTransposer cinemachineTransposer;

    private bool softRotation;

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        GridSystem.OnAnyGridSystemStarted += GridSystem_OnAnyGridSystemStarted;

        CubesSystem.Instance.OnGamePaused += (_, _) => softRotation = true;
        CubesSystem.Instance.OnGameStart  += (_, _) => softRotation = false;

        softRotation = false;
    }

    private void Update()
    {
        if (softRotation)
        {
            float rotationSpeed = 2f;
            transform.eulerAngles += rotationSpeed * Time.deltaTime * Vector3.down;
        }
        else
        {
            HandleRotation();
        }
    }

    private void HandleRotation()
    {
        Vector2 inputMoveDir = InputManager.Instance.GetCameraRotateVector();
        Vector3 rotationVector = new(inputMoveDir.y, inputMoveDir.x, 0);
        float rotationSpeed = 100f;
        transform.eulerAngles += rotationSpeed * Time.deltaTime * rotationVector;
    }

    private void GridSystem_OnAnyGridSystemStarted(object sender, EventArgs e)
    {
        GridSystem gridSystem = sender as GridSystem;
        Vector3 gridSize = gridSystem.GetSize();
        float maxSize = Mathf.Max(gridSize.x, gridSize.y, gridSize.z);
        transform.position = gridSystem.GetCenter();
        //transform.eulerAngles = new Vector3(45f, 45f, 0);
        cinemachineTransposer.m_FollowOffset.z = - maxSize * 1.7f;
    }

}
