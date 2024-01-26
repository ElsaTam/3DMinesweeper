using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bombCountText;

    private void Start()
    {
        Cube.OnAnyBombMarked += (_, _) => UpdateBombCount();;
        Cube.OnAnyBombUnmarked += (_, _) => UpdateBombCount();
        CubesSystem.Instance.OnGamePaused += (_, _) => gameObject.SetActive(false);
        CubesSystem.Instance.OnGameStart += CubesSystem_OnGameStart;
        UpdateBombCount();
        gameObject.SetActive(false);
    }


    private void UpdateBombCount()
    {
        bombCountText.text = CubesSystem.Instance.GetRemainingBombCount().ToString();
    }

    private void CubesSystem_OnGameStart(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        UpdateBombCount();
    }

}
