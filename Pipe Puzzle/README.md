# Pipe Puzzle
The objective of this game is to have the player match pipes so that there is a flow from the starting cell to the ending cell.
The game was broken down into these list of tasks: 
1) **Creating a path of pipes:** Used a random-search recursive algorithm to create a new design level in every game instantiation. 
<details><summary><b>Click me to see codebase</b></summary>
<p>
    
```
    /// <summary>
    /// Helper function that instantiates a path with the appropriate pipes.
    /// </summary>
    void InstantiatePath()
    {
        // 1) Initializing nodes
        for (int r = 0; r < NUM_ROWS; r++)
        {
            for (int c = 0; c < NUM_COLS; c++)
            {
                _graph[r, c] = new PipeNode(r, c);
            }
        }

        // 2) Find a start, end, and a random path in between 
        int randColStart = _rand.Next(0, NUM_COLS);
        int randColEnd = _rand.Next(0, NUM_COLS);
        PipeNode startNode = _graph[0, randColStart];
        PipeNode endNode = _graph[NUM_ROWS - 1, randColEnd];
        RandomSearch(_graph, startNode, endNode);

        // 3) Populate the path tiles with the appropriate pipe
        GameObject pipeTiles = new GameObject();
        pipeTiles.transform.name = "Pipe Tiles";
        pipeTiles.transform.parent = transform;
        for (int i = 0; i < _path.Count; i++)
        {
            // Inititalize the pipe tile and its componenets
            GameObject pipeTile = new GameObject();
            pipeTile.AddComponent<SpriteRenderer>();
            pipeTile.AddComponent<BoxCollider2D>();

            pipeTile.transform.name = "Pipe Tile " + i;
            pipeTile.GetComponent<SpriteRenderer>().sprite = DeterminePipe(i);
            pipeTile.GetComponent<BoxCollider2D>().size = new Vector2(1, 1);

            Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, _possibleRotations[_rand.Next(0, _possibleRotations.Length)]));
            Vector3 position = new Vector3(_path[i].col, -_path[i].row, 0f);
            _path[i].tile = Instantiate(pipeTile, position, rotation).transform;
            _path[i].tile.parent = pipeTiles.transform;

            Destroy(pipeTile);
        }
    }

    /// <summary>
    /// A recursive search that only returns true when the goal node has been found. 
    /// </summary>
    /// <returns><c>true</c>, if the goal node has been found, <c>false</c> otherwise.</returns>
    /// <param name="graph">Graph.</param>
    /// <param name="visitedNode">The node we are currently on.</param>
    /// <param name="goalNode">The node that we want to reach.</param>
    bool RandomSearch(PipeNode[,] graph, PipeNode visitedNode, PipeNode goalNode)
    {
        visitedNode.visited = true;
        List<PipeNode> nodesToVisit = new List<PipeNode>();

        // If we have reached the goal
        if (goalNode.row == visitedNode.row && goalNode.col == visitedNode.col)
        {
            _path.Add(visitedNode);
            visitedNode.onPath = true;
            return true;
        }

        // Check if bottom node is valid
        if (visitedNode.row + 1 < NUM_ROWS && !graph[visitedNode.row + 1, visitedNode.col].visited)
        {
            nodesToVisit.Add(graph[visitedNode.row + 1, visitedNode.col]);
        }

        // Check if left node is valid
        if (visitedNode.col - 1 >= 0 && !graph[visitedNode.row, visitedNode.col - 1].visited)
        {
            nodesToVisit.Add(graph[visitedNode.row, visitedNode.col - 1]);
        }

        // Check if right node is valid
        if (visitedNode.col + 1 < NUM_COLS && !graph[visitedNode.row, visitedNode.col + 1].visited)
        {
            nodesToVisit.Add(graph[visitedNode.row, visitedNode.col + 1]);
        }

        // Check if top node is valid
        if (visitedNode.row - 1 >= 0 && !graph[visitedNode.row - 1, visitedNode.col].visited)
        {
            nodesToVisit.Add(graph[visitedNode.row - 1, visitedNode.col]);
        }

        // Randomize the list of nodes to visit
        List<PipeNode> randomNodesToVisit = new List<PipeNode>();
        while (nodesToVisit.Count > 0)
        {
            int randomIndex = _rand.Next(0, nodesToVisit.Count);
            randomNodesToVisit.Add(nodesToVisit[randomIndex]);
            nodesToVisit.RemoveAt(randomIndex);
        }

        // Go into the next recursive iteration of each node to visit
        foreach (PipeNode node in randomNodesToVisit)
        {
            if (RandomSearch(graph, node, goalNode))
            {
                _path.Add(visitedNode);
                visitedNode.onPath = true;
                return true;
            }
        }

        return false;
    }
```

</p>
</details>

2) **Determine pipe sprite:** Determined the sprite of a pipe by counting how many active neighbors it has (active neighbors are those with pipe sprtes). 
<details><summary><b>Click me to see codebase</b></summary>
<p>
    
```
    /// <summary>
    /// Determines which pipe sprite to use based on the number of neighbors
    /// and their orientation. 
    /// </summary>
    /// <returns>The sprite based on the number of neighbors.</returns>
    /// <param name="index">Index of the current node on the path.</param>
    Sprite DeterminePipe(int index)
    {
        PipeNode currNode = _path[index];
        int numNeighbors = 0;

        // Check top node
        if (currNode.row - 1 >= 0 && _graph[currNode.row - 1, currNode.col].onPath)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.TOP);
        }

        // Check bottom node
        if (currNode.row + 1 < NUM_ROWS && _graph[currNode.row + 1, currNode.col].onPath)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.BOTTOM);
        }

        // Check left node
        if (currNode.col - 1 >= 0 && _graph[currNode.row, currNode.col - 1].onPath)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.LEFT);
        }

        // Check right node
        if (currNode.col + 1 < NUM_COLS && _graph[currNode.row, currNode.col + 1].onPath)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.RIGHT);
        }

        // Add a neighbor on the bottom for the last tile
        if (index == 0)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.BOTTOM);
        }
        // Add a neighbor on the top for the first tile
        else if (index == _path.Count - 1)
        {
            numNeighbors++;
            currNode.neighbors.Add(PipeNode.NodePosition.TOP);
        }

        // Determine which sprite to use 
        if (numNeighbors == 4)
        {
            currNode.pipeType = PipeNode.PipeType.CROSS;
            return crossPipeSprite;
        }
        else if (numNeighbors == 3)
        {
            currNode.pipeType = PipeNode.PipeType.T;
            return tPipeSprite;
        }
        else
        {
            if (index == 0 && currNode.col == _path[index + 1].col)
            {
                currNode.pipeType = PipeNode.PipeType.I;
                return iPipeSprite;
            }
            else if (index == _path.Count - 1 && currNode.col == _path[index - 1].col)
            {
                currNode.pipeType = PipeNode.PipeType.I;
                return iPipeSprite;
            }
            else if (index != 0 && index != _path.Count - 1 &&
                (_path[index + 1].col == _path[index - 1].col ||
                _path[index + 1].row == _path[index - 1].row))
            {
                currNode.pipeType = PipeNode.PipeType.I;
                return iPipeSprite;
            }
            else
            {
                currNode.pipeType = PipeNode.PipeType.C;
                return cPipeSprite;
            }
        }
    }
```

</p>
</details>

3) **Checked if user clicked on a tile:**
<details><summary><b>Click me to see codebase</b></summary>
<p>
    
```
    /// <summary>
    /// Checks if the user has clicked on the tile. If so, prevent any
    /// input until the rotation animation has finished. 
    /// </summary>
    void CheckForClick()
    {
        if(Input.GetMouseButtonDown(0) && _canInput)
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitObj = Physics2D.Raycast(point, Vector2.zero);
            if(hitObj.collider != null)
            {
                _canInput = false;
                StartCoroutine(WaitAndRotateTile(hitObj));
            }
        }
    }
```

</p>
</details>

4) **LERPed the rotation of a pipe tile:**
<details><summary><b>Click me to see codebase</b></summary>
<p>
    
```
    /// <summary>
    /// LERPs the rotation animation of the tile. 
    /// </summary>
    /// <param name="hitObj">Hit object transform we are rotating.</param>
    IEnumerator WaitAndRotateTile(RaycastHit2D hitObj)
    {
        Vector3 currentEuler = hitObj.transform.rotation.eulerAngles;
        Vector3 targetEuler = new Vector3(0f, 0f, currentEuler.z + 90f);
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        float startTime = Time.time;

        // This while loop occurs for [0,0.5] seconds
        while(Time.time - startTime <= 0.5)
        {
            float rotationCovered = (Time.time - startTime) * ROTATION_SPEED;
            hitObj.transform.rotation = Quaternion.Lerp(hitObj.transform.rotation, targetRotation, rotationCovered);
            yield return 1;
        }
        _canInput = true;
    }
```

</p>
</details>

5) **Check win condition:** The game manager checks if the player won by checking the orientation of every tile. 
<details><summary><b>Click me to see codebase</b></summary>
<p>
    
```
    /// <summary>
    /// Checks if the player has won the game. 
    /// </summary>
    bool CheckWinCondition()
    {
        for (int i = 0; i < _path.Count; i++)
        {
            if (!CheckOrientation(i))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks the orientation.
    /// </summary>
    /// <returns><c>true</c>, if the pipe is in the correct orientation, <c>false</c> otherwise.</returns>
    /// <param name="index">Index of the current pipe on path.</param>
    bool CheckOrientation(int index)
    {
        PipeNode currNode = _path[index];
        float zRotation = currNode.tile.rotation.eulerAngles.z;

        // These booleans are determined when the path is constructed (but before the tiles are randomized) 
        bool hasTopNeighbor = currNode.neighbors.Contains(PipeNode.NodePosition.TOP);
        bool hasBottomNeighbor = currNode.neighbors.Contains(PipeNode.NodePosition.BOTTOM);
        bool hasLeftNeighbor = currNode.neighbors.Contains(PipeNode.NodePosition.LEFT);
        bool hasRightNeighbor = currNode.neighbors.Contains(PipeNode.NodePosition.RIGHT);

        if (currNode.pipeType == PipeNode.PipeType.I)
        {
            if (Mathf.Abs(zRotation) <= EPSILON || Mathf.Abs(zRotation - 180f) <= EPSILON)
            {
                if (hasTopNeighbor && hasBottomNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 90f) <= EPSILON || Mathf.Abs(zRotation - 270f) <= EPSILON)
            {
                if (hasLeftNeighbor && hasRightNeighbor)
                {
                    return true;
                }
            }
        }
        else if (currNode.pipeType == PipeNode.PipeType.C)
        {
            if (Mathf.Abs(zRotation) <= EPSILON)
            {
                if (hasTopNeighbor && hasLeftNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 90f) <= EPSILON)
            {
                if (hasLeftNeighbor && hasBottomNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 180f) <= EPSILON)
            {
                if (hasBottomNeighbor && hasRightNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 270f) <= EPSILON)
            {
                if (hasRightNeighbor && hasTopNeighbor)
                {
                    return true;
                }
            }
        }
        else if (currNode.pipeType == PipeNode.PipeType.T)
        {
            if (Mathf.Abs(zRotation) <= EPSILON)
            {
                if (hasLeftNeighbor && hasTopNeighbor && hasRightNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 90f) <= EPSILON)
            {
                if (hasTopNeighbor && hasLeftNeighbor && hasBottomNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 180f) <= EPSILON)
            {
                if (hasRightNeighbor && hasBottomNeighbor && hasLeftNeighbor)
                {
                    return true;
                }
            }
            else if (Mathf.Abs(zRotation - 270f) <= EPSILON)
            {
                if (hasBottomNeighbor && hasRightNeighbor && hasTopNeighbor)
                {
                    return true;
                }
            }
        }
        else if (currNode.pipeType == PipeNode.PipeType.CROSS)
        {
            return true;
        }

        return false;
    }
```

</p>
</details>

# Demo
![pipe_puzzle](https://user-images.githubusercontent.com/34965351/73584042-923d9700-444a-11ea-9afa-4252d18fef87.gif)
