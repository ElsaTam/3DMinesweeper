using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_InputField sizeXInput;
    [SerializeField] private TMP_InputField sizeYInput;
    [SerializeField] private TMP_InputField sizeZInput;
    [SerializeField] private TMP_InputField numberOfBombsInput;
    [SerializeField] private Toggle explosionToggle;

    private Vector3Int gridSize;
    private int numberOfBombs;
    private bool useExplosion;

    private float fadeDuration = 1f;

    private void Awake()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        restartButton.onClick.AddListener(OnStartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        sizeXInput.onEndEdit.AddListener(delegate{ValidateSizeInput(sizeXInput, 0);});
        sizeYInput.onEndEdit.AddListener(delegate{ValidateSizeInput(sizeYInput, 1);});
        sizeZInput.onEndEdit.AddListener(delegate{ValidateSizeInput(sizeZInput, 2);});
        numberOfBombsInput.onEndEdit.AddListener(delegate{ValidateNumberOfBombsInput();});
        explosionToggle.onValueChanged.AddListener((bool useExplosion) => {this.useExplosion = useExplosion;});
    }

    private void Start()
    {
        InputManager.Instance.OnKeyTabPressed += InputManager_OnKeyTabPressed;
        CubesSystem.Instance.OnGameWon += CubesSystem_OnGameWon;
        CubesSystem.Instance.OnGameLost += CubesSystem_OnGameLost;
        CubesSystem.Instance.OnGamePaused += CubesSystem_OnGamePaused;
        CubesSystem.Instance.OnGameResumed += (_,_) => Disappear();
        CubesSystem.Instance.OnGameStarted += (_,_) => Disappear();

        finalText.text = "MENU";

        SetInput();
    }

    private void ValidateSizeInput(TMP_InputField input, int idx)
    {
        try
        {
            gridSize[idx] = Mathf.Clamp(int.Parse(input.text), 3, 10);
        }
        catch (FormatException) { }

        input.text = gridSize[idx].ToString();
        ValidateNumberOfBombsInput();
    }

    private void ValidateNumberOfBombsInput()
    {
        try
        {
            numberOfBombs = Mathf.Clamp(int.Parse(numberOfBombsInput.text), 1, gridSize.x * gridSize.y * gridSize.z - 1);
        }
        catch { }

        numberOfBombsInput.text = numberOfBombs.ToString();
    }


    private void InputManager_OnKeyTabPressed(object sender, EventArgs e)
    {
        if (sizeXInput.isFocused) sizeYInput.Select();
        if (sizeYInput.isFocused) sizeZInput.Select();
        if (sizeZInput.isFocused) numberOfBombsInput.Select();
        if (numberOfBombsInput.isFocused) sizeXInput.Select();
    }

    private void CubesSystem_OnGamePaused(object sender, EventArgs e)
    {
        finalText.text = "MENU";
        finalText.color = Color.white;
        resumeButton.gameObject.SetActive(true);
        SetInput();
        Appear();
    }

    private void CubesSystem_OnGameWon(object sender, EventArgs e)
    {
        finalText.text = "IT'S A WIN!";
        finalText.color = restartButton.image.color;
        resumeButton.gameObject.SetActive(false);
        SetInput();
        Appear();
    }

    private void CubesSystem_OnGameLost(object sender, EventArgs e)
    {
        Debug.Log("Lost");
        finalText.text = "GAME OVER";
        finalText.color = quitButton.image.color;
        resumeButton.gameObject.SetActive(false);
        SetInput();
        Appear();
    }


    private void SetInput()
    {
        gridSize = CubesSystem.Instance.GetGridSize();
        sizeXInput.text = gridSize.x.ToString();
        sizeYInput.text = gridSize.y.ToString();
        sizeZInput.text = gridSize.z.ToString();

        numberOfBombs = CubesSystem.Instance.GetTotalBombCount();
        numberOfBombsInput.text = numberOfBombs.ToString();

        useExplosion = CubesSystem.Instance.UseExplosion();
        explosionToggle.isOn = useExplosion;
    }

    private void Appear()
    {
        if (! gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(Fade(GetComponent<CanvasGroup>(), 0, 1, FadeInCompleted));
        }
    }

    private void Disappear()
    {
        if (  gameObject.activeSelf)
        {
            SetInteractable(false);
            StartCoroutine(Fade(GetComponent<CanvasGroup>(), 1, 0, FadeOutCompleted));
        }
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float start, float end, Action onFadeComplete)
    {
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
        onFadeComplete();
    }




    private void FadeInCompleted()
    {
        SetInteractable(true);
    }

    private void FadeOutCompleted()
    {
        gameObject.SetActive(false);
    }

    private void SetInteractable(bool interactable)
    {
        foreach (Button button in GetComponents<Button>())
        {
            button.interactable = interactable;
        }
    }


    private void OnResumeClicked()
    {
        CubesSystem.Instance.ResumeGame();
    }

    private void OnStartClicked()
    {
        CubesSystem.Instance.RestartGame(gridSize, numberOfBombs, useExplosion);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
