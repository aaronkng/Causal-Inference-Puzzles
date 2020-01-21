using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PipeNode
{
    public enum NodePosition { TOP, BOTTOM, LEFT, RIGHT };
    public enum PipeType { I, C, T, CROSS };

    public readonly int row;
    public readonly int col;
    public bool visited;    // Determines if the node has been visited during random search
    public bool onPath;     // Determines if the node is on the path from start to end
    public List<NodePosition> neighbors;
    public PipeType pipeType;
    public Transform tile;

    public PipeNode(int r, int c)
    {
        row = r;
        col = c;
        visited = false;
        onPath = false;
        neighbors = new List<NodePosition>();
        tile = null;
    }
}

public class PipePuzzle : MonoBehaviour
{
    public static float EPSILON = Mathf.Pow(2, -4);

    // Inputs needed from Unity editor
    public Sprite iPipeSprite;
    public Sprite cPipeSprite;
    public Sprite tPipeSprite;
    public Sprite crossPipeSprite;
    public Sprite exampleSprite;

    // Constants (change to private when final values are determined)
    public int NUM_ROWS;
    public int NUM_COLS;
    public float ROTATION_SPEED = 5f;

    float[] _possibleRotations = { 0f, 90f, 180f, 270f };    // Used to randomize pipe rotation
    PipeNode[,] _graph;
    List<PipeNode> _path = new List<PipeNode>(); // List of nodes that are on the path from start to end
    System.Random _rand = new System.Random();
    bool _canInput = true;   // Is false when a pipe is LERPing rotation
    bool _winCondition;      // Checks if the player has put all the pipes in the correct orientation

    void Start()
    {
        _graph = new PipeNode[NUM_ROWS, NUM_COLS];
        InstantiatePath();
        InstantiateBorder();
        InstantiateGrid();
    }

    void Update()
    {
        CheckForClick();
        _winCondition = CheckWinCondition();
    }

    public bool Finished()
    {
        return _winCondition;
    }

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

    /// <summary>
    /// Instantiates the border tiles.
    /// </summary>
    void InstantiateBorder()
    {
        // Creating a tile gameobject to instantiate
        GameObject tile = new GameObject();
        tile.AddComponent<SpriteRenderer>();

        GameObject borderTiles = new GameObject();
        borderTiles.transform.name = "Border Tiles";
        borderTiles.transform.parent = transform;

        // Instantiating border
        for (int r = 1; r > -NUM_ROWS - 1; r--)
        {
            for (int c = -1; c < NUM_COLS + 1; c++)
            {
                if ((r == 1 || r == -NUM_ROWS) || (c == -1 || c == NUM_COLS))
                {
                    Vector3 position = new Vector3(c, r, 0f);
                    GameObject borderTile = Instantiate(tile, position, Quaternion.identity);
                    borderTile.GetComponent<SpriteRenderer>().sprite = exampleSprite;
                    if (c == _path[0].col && r == -NUM_ROWS)
                    {
                        borderTile.GetComponent<SpriteRenderer>().color = Color.red;
                        borderTile.transform.name = "Ending Tile";
                        borderTile.transform.parent = transform;
                    }
                    else if (c == _path[_path.Count - 1].col && r == 1)
                    {
                        borderTile.GetComponent<SpriteRenderer>().color = Color.green;
                        borderTile.transform.name = "Starting Tile";
                        borderTile.transform.parent = transform;
                    }
                    else
                    {
                        borderTile.GetComponent<SpriteRenderer>().color = Color.black;
                        borderTile.transform.name = "Border Tile";
                        borderTile.transform.parent = borderTiles.transform;
                    }
                }
            }
        }

        Destroy(tile);
    }

    /// <summary>
    /// Instantiates the grid. 
    /// </summary>
    void InstantiateGrid()
    {
        // Create a set of horizontal grids
        float colOffset;
        if (NUM_COLS % 2 == 0)
        {
            colOffset = 0.5f;
        }
        else
        {
            colOffset = 0f;
        }
        GameObject horizontalGrids = new GameObject();
        horizontalGrids.transform.name = "Horizontal Grids";
        horizontalGrids.transform.parent = transform;

        GameObject horizontalGrid = new GameObject();
        horizontalGrid.transform.name = "Horizontal Grid";
        horizontalGrid.transform.parent = transform;

        SpriteRenderer horizSpriteRenderer = horizontalGrid.AddComponent<SpriteRenderer>();
        horizSpriteRenderer.sprite = exampleSprite;
        horizontalGrid.transform.localScale = new Vector3(NUM_COLS, 0.03f, 0f);
        for (int i = 0; i < NUM_ROWS - 1; i++)
        {
            // We offset the grid's y position by 0.5f because the tile's position is n.0f
            Vector3 position = new Vector3(NUM_COLS / 2 - colOffset, -i - 0.5f, 0f);
            GameObject grid = Instantiate(horizontalGrid, position, Quaternion.identity);
            grid.transform.parent = horizontalGrids.transform;
        }
        Destroy(horizontalGrid);

        // Create a set of vertical grids
        float rowOffset;
        if (NUM_ROWS % 2 == 0)
        {
            rowOffset = 0.5f;
        }
        else
        {
            rowOffset = 0f;
        }
        GameObject verticalGrids = new GameObject();
        verticalGrids.transform.name = "Vertical Grids";
        verticalGrids.transform.parent = transform;

        GameObject verticalGrid = new GameObject();
        verticalGrid.transform.name = "Vertical Grid";
        verticalGrid.transform.parent = transform;

        SpriteRenderer vertSpriteRenderer = verticalGrid.AddComponent<SpriteRenderer>();
        vertSpriteRenderer.sprite = exampleSprite;
        verticalGrid.transform.localScale = new Vector3(0.03f, NUM_ROWS, 0f);
        for (int i = 0; i < NUM_ROWS - 1; i++)
        {
            // We offset the grid's x position by 0.5f because the tile's position is n.0f
            Vector3 position = new Vector3(i + 0.5f, -NUM_ROWS / 2 + rowOffset, 0f);
            GameObject grid = Instantiate(verticalGrid, position, Quaternion.identity);
            grid.transform.parent = verticalGrids.transform;
        }
        Destroy(verticalGrid);
    }

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
}
