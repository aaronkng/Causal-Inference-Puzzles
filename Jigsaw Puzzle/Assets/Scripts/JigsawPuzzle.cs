using System.Collections.Generic;
using UnityEngine;

// Because we can't serialize double arrays onto the Unity editor, 
// we are using this method instead.
// tileSprites size is 3 (for each type of image)
// spriteRow size is 9 (for each jigsaw puzzle) 
[System.Serializable]
public struct SpriteRow
{
    [SerializeField] public Sprite[] spriteRow;
}

public class JigsawPuzzle : MonoBehaviour
{
    private enum MouseState { MOUSEUP, MOUSEDOWN }

    public JigsawPiece tilePrefab;
    public int tileSetID = 0;

    public SpriteRow[] tileSprites;
    private List<JigsawPiece> _tileList = new List<JigsawPiece>();
    private List<Vector3> _tilePositionList = new List<Vector3>();
    private MouseState _mouseState;
    private RaycastHit2D _hitObj;
    private Vector3 _movingTilePos = Vector3.zero;

    void Start()
    {
        CreateListOfPos();
        RandomizeJigsawPieces();

        _mouseState = MouseState.MOUSEUP;
    }

    void Update()
    {
        CheckMouseState();
        if (CheckWinCondition())
        {
            Debug.Log("WIN!");
        }
    }

    /// <summary>
    /// Checks mouse state of player. If the mouse has not been clicked, 
    /// then the player can click on a tile; else, the player is currently
    /// dragging a tile.
    /// </summary>
    void CheckMouseState()
    {
        switch (_mouseState)
        {
            case MouseState.MOUSEUP:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _hitObj = Physics2D.Raycast(point, Vector2.zero);
                    if (_hitObj.collider != null && _hitObj.transform.tag == "Tile")
                    {
                        _movingTilePos = _hitObj.transform.position;
                        _mouseState = MouseState.MOUSEDOWN;
                    }
                }
                break;
            case MouseState.MOUSEDOWN:
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // If the player is dragging the tile
                if (Input.GetMouseButton(0))
                {
                    _hitObj.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
                    _hitObj.transform.GetComponent<JigsawPiece>().IsMoving = true;
                }
                // If the player has released the tile
                else if (Input.GetMouseButtonUp(0))
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector3.forward);
                    // If the player has released the tile in a random spot
                    if (hits.Length == 1)
                    {
                        hits[0].transform.position = _movingTilePos;
                    }
                    // If the player has released the tile on top of another tile
                    else if (hits.Length == 2)
                    {
                        for (int i = 0; i < hits.Length; i++)
                        {
                            // Since hits has non-determined order of what objects are
                            // hit by the ray, we need to check each object in hits
                            // to prevent oob error
                            if (hits[i].transform.GetComponent<JigsawPiece>().IsMoving)
                            {
                                if (i == 0)
                                {
                                    hits[i].transform.position = hits[i + 1].transform.position;
                                    hits[i + 1].transform.position = _movingTilePos;
                                }
                                else if (i == 1)
                                {
                                    hits[i].transform.position = hits[i - 1].transform.position;
                                    hits[i - 1].transform.position = _movingTilePos;
                                }
                                hits[i].transform.GetComponent<JigsawPiece>().IsMoving = false;
                            }
                        }
                    }
                    _mouseState = MouseState.MOUSEUP;
                }
                break;
        }
    }

    /// <summary>
    /// Creates a list of vector3 positions the jigsaw pieces can fall into. 
    /// </summary>
    private void CreateListOfPos()
    {
        for (int row = -1; row <= 1; row++)
        {
            for (int col = 1; col >= -1; col--)
            {
                Vector3 pos = new Vector3(row, col, 0f);
                _tilePositionList.Add(pos);
            }
        }
    }

    /// <summary>
    /// Randomizes the jigsaw pieces.
    /// </summary>
    private void RandomizeJigsawPieces()
    {
        System.Random rand = new System.Random();
        int tileCount = 0;
        for (int col = 1; col >= -1; col--)
        {
            for (int row = -1; row <= 1; row++)
            {
                Vector3 origPos = new Vector3(row, col, 0f);
                int ranPosID = rand.Next(0, _tilePositionList.Count - 1);
                Vector3 ranPos = _tilePositionList[ranPosID];

                JigsawPiece tile = Instantiate(tilePrefab, ranPos, Quaternion.identity);
                tile.SetSprite(tileSprites[tileSetID].spriteRow[tileCount]);
                tile.TilePos = new Vector3(row, col, 0f);

                _tilePositionList.RemoveAt(ranPosID);
                _tileList.Add(tile);
                tileCount++;
            }
        }
    }

    /// <summary>
    /// Checks if the player won
    /// </summary>
    /// <returns><c>true</c>, if all pieces are in the right place, <c>false</c> otherwise.</returns>
    private bool CheckWinCondition()
    {
        foreach(JigsawPiece tile in _tileList)
        {
            if(!tile.TilePos.Equals(tile.transform.position))
            {
                return false;
            }
        }
        return true;
    }
}
