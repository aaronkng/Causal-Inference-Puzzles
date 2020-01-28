# Sliding Puzzle
The objective of this game is for the player to match three sets of game pieces so that they align either along the rows or the columns. The construction of this game was broken down into this list of tasks: 
1) **Randomizing initial game state:** Had to make sure the game was inititalized so that tiles are randomly placed throughout the grid. 
```
    /// <summary>
    /// Randomize the tiles;
    /// </summary>
    private void RandomizeTiles()
    {
        // Selecting three random set of tiles
        List<int> tileIDList = new List<int>();
        for(int i = 0; i < 5; i++)
        {
            tileIDList.Add(i);
        }
        List<Tile> selectedTileList = new List<Tile>();
        System.Random rand = new System.Random();
        for(int i = 0; i < 3; i++)
        {
            int randNum = rand.Next(0, tileIDList.Count);
            int tileID = tileIDList[randNum];

            // We want only two tiles for the last tile set
            int maxTileCount = 3;
            if(i >= 2)
            {
                maxTileCount = 2;
            }

            for (int j = 0; j < maxTileCount; j++)
            {
                selectedTileList.Add(_tilePrefabs[tileID]);
            }
            tileIDList.Remove(tileID);
        }

        // Randomizing tile positions
        for(int c = 1; c >= -1; c--)
        {
            for(int r = -1; r <= 1; r++)
            {
                if (c == -1 && r == 1)
                {
                    // Do nothing (we don't want to fill in last tile
                }
                else
                {
                    int randNum = rand.Next(0, selectedTileList.Count);
                    Vector3 position = new Vector3(r, c, 0f);
                    Tile tile = Instantiate(selectedTileList[randNum], position, Quaternion.identity);

                    // Add tiles to corresponing list to check for win conditions later
                    if(tile.tileName.Equals("Brain"))
                    {
                        _brainTiles.Add(tile);
                    }
                    else if(tile.tileName.Equals("Heart"))
                    {
                        _heartTiles.Add(tile);
                    }
                    else if(tile.tileName.Equals("Moxy"))
                    {
                        _moxyTiles.Add(tile);
                    }
                    else if (tile.tileName.Equals("O"))
                    {
                        _oTiles.Add(tile);
                    }
                    else
                    {
                        _xTiles.Add(tile);
                    }
                    selectedTileList.RemoveAt(randNum);
                }
            }
        }
```
2) **Checking for input:** Checked if the user touched a tile and measures the amount of the force from the user's swipe to determine whether a tile should move or not in the direction of the user's swipe. 
```
    /// <summary>
    /// Checks if the user has flicked a tile hard enough. 
    /// </summary>
    void CheckMouseState()
    {
        // On mouse click
        if (Input.GetMouseButtonDown(0))
        {
            _lastTouchPosition = Input.mousePosition;
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _hitObj = Physics2D.Raycast(point, Vector2.zero);
        }
        // While user is swiping
        if (Input.GetMouseButton(0))
        {
            // If user clicked on a tile
            if (_hitObj.collider != null && _hitObj.transform.tag == "Tile")
            {
                _touchDelta = Input.mousePosition - _lastTouchPosition;

                // If user swipes hard enough
                if (_touchDelta.magnitude > DELTATHRESHOLD)
                {
                    // If swiped in horizontal direction
                    if (Mathf.Abs(_touchDelta.x) > Mathf.Abs(_touchDelta.y))
                    {
                        // If swiped right and moving into free space
                        if (_touchDelta.x > 0 && _freePosition == _hitObj.transform.position + Vector3.right)
                        {
                            StartCoroutine(WaitAndMoveTile(_hitObj, Vector3.right));
                        }
                        // If swiped left and moving into free space
                        else if (_touchDelta.x < 0 && _freePosition == _hitObj.transform.position + Vector3.left)
                        {
                            StartCoroutine(WaitAndMoveTile(_hitObj, Vector3.left));
                        }
                    }
                    // If swiped in vertical direction
                    else if (Mathf.Abs(_touchDelta.x) < Mathf.Abs(_touchDelta.y))
                    {
                        // If swiped up and moving into free space
                        if (_touchDelta.y > 0 && _freePosition == _hitObj.transform.position + Vector3.up)
                        {
                            StartCoroutine(WaitAndMoveTile(_hitObj, Vector3.up));
                        }
                        // If swiped down and moving into free space
                        else if (_touchDelta.y < 0 && _freePosition == _hitObj.transform.position + Vector3.down)
                        {
                            StartCoroutine(WaitAndMoveTile(_hitObj, Vector3.down));
                        }
                    }
                }
                _lastTouchPosition = Input.mousePosition;
            }
        }
    }
```
3) **LERPing animation for tile movement:** LERPed the movement of the tile when it goes from one position into another to allow for some visual juiciness in gameplay. 
```
    /// <summary>
    /// Moves the tile if the user has flicked hard enough.
    /// </summary>
    /// <param name="hitObj">The tile being moved.</param>
    /// <param name="direction">The direction the tile is moving towards.</param>
    IEnumerator WaitAndMoveTile(RaycastHit2D hitObj, Vector3 direction)
    {
        Vector3 startingPosition = hitObj.transform.position;
        Vector3 finalPosition = hitObj.transform.position + direction;
        yield return new WaitForSeconds(TILEMOVEDELAYTIME);
        float startTime = Time.time;
        while (Time.time - startTime <= 1)
        {
            float distCovered = (Time.time - startTime) * TILESPEED;
            Vector3 newPos = Vector3.Lerp(startingPosition, finalPosition, distCovered);

            // Clamp the lerp so it doesn't overshoot final position
            if (direction == Vector3.up)
            {
                if (startingPosition.y == 0.0f)
                {
                    newPos.y = Mathf.Clamp(newPos.y, 0.0f, 1.0f);
                }
                else
                {
                    newPos.y = Mathf.Clamp(newPos.y, -1.0f, 0.0f);
                }
            }
            else if (direction == Vector3.down)
            {
                if (startingPosition.y == 0.0f)
                {
                    newPos.y = Mathf.Clamp(newPos.y, -1.0f, 0.0f);
                }
                else
                {
                    newPos.y = Mathf.Clamp(newPos.y, 0.0f, 1.0f);
                }
            }
            else if (direction == Vector3.right)
            {
                if (startingPosition.x == 0.0f)
                {
                    newPos.x = Mathf.Clamp(newPos.x, 0.0f, 1.0f);
                }
                else
                {
                    newPos.x = Mathf.Clamp(newPos.x, -1.0f, 0.0f);
                }
            }
            else if (direction == Vector3.left)
            {
                if (startingPosition.x == 0.0f)
                {
                    newPos.x = Mathf.Clamp(newPos.x, -1.0f, 0.0f);
                }
                else
                {
                    newPos.x = Mathf.Clamp(newPos.x, 0.0f, 1.0f);
                }
            }
            hitObj.transform.position = newPos;
            yield return 1;
        }
        _freePosition = startingPosition;
    }
```
4) **Checking for victory condition (row-side and column-side):** Had to check if the player managed to win by getting all the pieces aligned column-wise or row-wise. 
```
    /// <summary>
    /// Checks if player won via columns.
    /// </summary>
    /// <returns><c>true</c>, if player won via columns, <c>false</c> otherwise.</returns>
    private bool CheckColSideVictory()
    {
        bool brainRowWin = true;
        bool heartRowWin = true;
        bool moxyRowWin = true;
        bool oRowWin = true;
        bool xRowWin = true;

        for (int i = 0; i < _brainTiles.Count - 1; i++)
        {
            float x_0 = _brainTiles[i].transform.position.x;
            float x_1 = _brainTiles[i + 1].transform.position.x;

            if (!CheckNearlyEqual(x_0, x_1))
            {
                brainRowWin = false;
                break;
            }
        }

        for (int i = 0; i < _heartTiles.Count - 1; i++)
        {
            float x_0 = _heartTiles[i].transform.position.x;
            float x_1 = _heartTiles[i + 1].transform.position.x;

            if (!CheckNearlyEqual(x_0, x_1))
            {
                heartRowWin = false;
                break;
            }
        }

        for (int i = 0; i < _moxyTiles.Count - 1; i++)
        {
            float x_0 = _moxyTiles[i].transform.position.x;
            float x_1 = _moxyTiles[i + 1].transform.position.x;

            if (!CheckNearlyEqual(x_0, x_1))
            {
                moxyRowWin = false;
                break;
            }
        }

        for (int i = 0; i < _oTiles.Count - 1; i++)
        {
            float x_0 = _oTiles[i].transform.position.x;
            float x_1 = _oTiles[i + 1].transform.position.x;

            if (!CheckNearlyEqual(x_0, x_1))
            {
                oRowWin = false;
                break;
            }
        }

        for (int i = 0; i < _xTiles.Count - 1; i++)
        {
            float x_0 = _xTiles[i].transform.position.x;
            float x_1 = _xTiles[i + 1].transform.position.x;

            if (!CheckNearlyEqual(x_0, x_1))
            {
                xRowWin = false;
                break;
            }
        }

        return brainRowWin && heartRowWin && moxyRowWin && oRowWin && xRowWin;
    }

    /// <summary>
    /// Checks if the player won by matching rows. 
    /// </summary>
    /// <returns><c>true</c>, if player won via rows, <c>false</c> otherwise.</returns>
    private bool CheckRowSideVictory()
    {
        bool brainColWin = true;
        bool heartColWin = true;
        bool moxyColWin = true;
        bool oColWin = true;
        bool xColWin = true;

        for (int i = 0; i < _brainTiles.Count - 1; i++)
        {
            float y_0 = _brainTiles[i].transform.position.y;
            float y_1 = _brainTiles[i + 1].transform.position.y;

            if (!CheckNearlyEqual(y_0, y_1))
            {
                brainColWin = false;
                break;
            }
        }

        for (int i = 0; i < _heartTiles.Count - 1; i++)
        {
            float y_0 = _heartTiles[i].transform.position.y;
            float y_1 = _heartTiles[i + 1].transform.position.y;

            if (!CheckNearlyEqual(y_0, y_1))
            {
                heartColWin = false;
                break;
            }
        }

        for (int i = 0; i < _moxyTiles.Count - 1; i++)
        {
            float y_0 = _moxyTiles[i].transform.position.y;
            float y_1 = _moxyTiles[i + 1].transform.position.y;

            if (!CheckNearlyEqual(y_0, y_1))
            {
                moxyColWin = false;
                break;
            }
        }

        for (int i = 0; i < _oTiles.Count - 1; i++)
        {
            float y_0 = _oTiles[i].transform.position.y;
            float y_1 = _oTiles[i + 1].transform.position.y;

            if (!CheckNearlyEqual(y_0, y_1))
            {
                oColWin = false;
                break;
            }
        }

        for (int i = 0; i < _xTiles.Count - 1; i++)
        {
            float y_0 = _xTiles[i].transform.position.y;
            float y_1 = _xTiles[i + 1].transform.position.y;

            if (!CheckNearlyEqual(y_0, y_1))
            {
                xColWin = false;
                break;
            }
        }

        return brainColWin && heartColWin && moxyColWin && oColWin && xColWin;
    }
```
**Demo:**  
![sliding_tile_demo](https://user-images.githubusercontent.com/34965351/73234736-4a63fa80-4140-11ea-88f2-cef0018d3c10.gif)
