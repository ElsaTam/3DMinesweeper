using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Cube : MonoBehaviour
{

    public static event EventHandler OnAnyCubeMoved;
    public static event EventHandler OnAnyBombMarked;
    public static event EventHandler OnAnyBombUnmarked;
    
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material revealedCubeMaterial;
    [SerializeField] private Material bombCubeMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    private List<TextMeshPro> textElementList;
    private List<MeshRenderer> flagElementList;
    private List<MeshRenderer> bombElementList;

    private enum HintTag
    {
        Empty,
        Flag,
        Bomb,
        Revealed
    }
    private HintTag hintTag;

    private bool isSelected;
    private bool isMoving;
    private Vector3 oldPosition;
    private Vector3 direction;
    private Vector3 targetPosition;
    private Action onMoveComplete;
    private bool hasBomb;

    private void Awake()
    {
        hintTag = HintTag.Empty;

        textElementList = new List<TextMeshPro>();
        flagElementList = new List<MeshRenderer>();
        bombElementList = new List<MeshRenderer>();

        foreach (TextMeshPro textMeshPro in GetComponentsInChildren<TextMeshPro>())
        {
            if (textMeshPro.gameObject.CompareTag("BombCountText"))
            {
                textElementList.Add(textMeshPro);
            }
        }
        foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
        {
            if (meshRenderer.gameObject.CompareTag("FlagQuad"))
            {
                flagElementList.Add(meshRenderer);
            }
            if (meshRenderer.gameObject.CompareTag("BombQuad"))
            {
                bombElementList.Add(meshRenderer);
            }
        }

        SetTextElementsActive(false);
        SetFlagElementsActive(false);
        SetBombElementsActive(false);
    }

    public void Setup(Vector3 direction, bool hasBomb)
    {
        this.direction = direction.normalized;
        this.hasBomb = hasBomb;
        //if (hasBomb) defaultMaterial = bombCubeMaterial;
        isMoving = false;
        SetSelected(false);
    }

    private void Update()
    {
        if (!isMoving) return;

        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float epsilonDistance = .1f;
        if (Vector3.Distance(targetPosition, transform.position) > epsilonDistance)
        {
            float moveSpeed = 4f;
            transform.position += Time.deltaTime * moveSpeed * moveDirection;
        }
        else
        {
            transform.position = new Vector3(Mathf.Round(transform.position.x),
                                             Mathf.Round(transform.position.y),
                                             Mathf.Round(transform.position.z));
            isMoving = false;
            OnAnyCubeMoved?.Invoke(this, EventArgs.Empty);
            onMoveComplete();
        }
    }

    public void Move(int movement, Action onMoveComplete)
    {
        oldPosition = transform.position;
        targetPosition = transform.position + movement * direction;
        isMoving = true;
        this.onMoveComplete = onMoveComplete;
    }

    public void Explode(Action onExplosionComplete)
    {
        SetSelected(false);
        if (hasBomb) meshRenderer.material = bombCubeMaterial;
        GetComponent<ExplodeCube>().Run();
        onExplosionComplete();
    }

    public void NextTag()
    {
        switch (hintTag)
        {
            case HintTag.Empty:
                SetFlagElementsActive(true);
                SetBombElementsActive(false);
                hintTag = HintTag.Flag;
                break;
            case HintTag.Flag:
                SetFlagElementsActive(false);
                SetBombElementsActive(true);
                hintTag = HintTag.Bomb;
                OnAnyBombMarked?.Invoke(this, EventArgs.Empty);
                meshRenderer.material = bombCubeMaterial;
                break;
            case HintTag.Bomb:
                SetFlagElementsActive(false);
                SetBombElementsActive(false);
                hintTag = HintTag.Empty;
                OnAnyBombUnmarked?.Invoke(this, EventArgs.Empty);
                meshRenderer.material = isSelected ? selectedMaterial : defaultMaterial;
                break;
        }
    }

    public void Reveal()
    {
        hintTag = HintTag.Revealed;
        SetFlagElementsActive(false);
        SetBombElementsActive(false);
        SetTextElementsActive(true);
        if (!isSelected)
        {
            meshRenderer.material = revealedCubeMaterial;
        }
        defaultMaterial = revealedCubeMaterial;
    }

    public void UpdateText(int numberOfNeighboringBombs)
    {
        foreach(TextMeshPro textMeshPro in textElementList)
        {
            textMeshPro.text = numberOfNeighboringBombs.ToString();
        }
    }

    private void SetTextElementsActive(bool isActive)
    {
        foreach(TextMeshPro textMeshPro in textElementList)
        {
            textMeshPro.gameObject.SetActive(isActive);
        }
    }
    private void SetFlagElementsActive(bool isActive)
    {
        foreach(MeshRenderer meshRenderer in flagElementList)
        {
            meshRenderer.gameObject.SetActive(isActive);
        }
    }
    private void SetBombElementsActive(bool isActive)
    {
        foreach(MeshRenderer meshRenderer in bombElementList)
        {
            meshRenderer.gameObject.SetActive(isActive);
        }
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        meshRenderer.material = isSelected ? selectedMaterial : defaultMaterial;
    }

    public MeshRenderer GetMeshRenderer() => meshRenderer;



    public Vector3 GetOldPosition() => oldPosition;
    public Vector3 GetPosition() => transform.position;
    public Vector3 GetDirection() => direction;
    public bool HasBomb() => hasBomb;
    public bool IsTaggedAsBombed() => hintTag == HintTag.Bomb;
    public bool HasBeenRevealed() => hintTag == HintTag.Revealed;

    public override string ToString()
    {
        return $"[Cube] at {transform.position} / moving {direction} / bomb ? {hasBomb}.";
    }
}
