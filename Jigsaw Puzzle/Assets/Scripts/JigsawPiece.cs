using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JigsawPiece : MonoBehaviour
{
    public bool InPos { set; get; }
    public bool IsMoving { set; get; }
    public Vector3 TilePos { set; get; }
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        InPos = false;
        IsMoving = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }
}
