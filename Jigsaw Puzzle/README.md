# Jigsaw Puzzle
The objective of this game is to have the player to match 9 jigsaw puzzle pieces so that they form an image if they win. 
The construction of this game was broken down into these list of tasks: 
1) **Checking Mouse State:** Checks if pointer has clicked on a jigsaw piece, and when user releases mouse pointer, checks if pointer holding a jigsaw piece is above another jigsaw  piece. 
<details><summary><b>Click me to see codebase</b></summary>
<p>
  
```
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
```
  
</p>
</details>

2) **Randomizing Jigsaw Pieces:** Randomizes the position of each slice of the main image in order to create varied initialized game states. 
<details><summary><b>Click me to see codebase</b></summary>
<p>
  
```
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
```
  
</p>
</details>

3) **Checking for Win Condition:** Checks if the pieces are all in their correct positions in every update loop. 
<details><summary><b>Click me to see codebase</b></summary>
<p>
  
```
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
```
  
</p>
</details>

# Demo
![jigsaw_puzzle](https://user-images.githubusercontent.com/34965351/73582411-e1cb9500-4441-11ea-8cec-df8a4b4fd523.gif)
