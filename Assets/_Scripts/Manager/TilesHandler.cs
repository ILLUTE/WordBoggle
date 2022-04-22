using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TilesHandler : MonoBehaviour
{
    // References
    [SerializeField]
    private LevelTile levelTilePrefab;
    // Private Variables
    private LevelTile[,] m_Tiles;

    private List<LevelTile[]> m_RowWiseColumns = new List<LevelTile[]>();

    private int rows = 0, columns = 0;

    private bool tilesGeneratedOnce = false;

    private GameModes m_Mode;

    private List<LevelTile> currentWord = new List<LevelTile>();

    private List<string> usedWords = new List<string>();

    private char[] letters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    private GridData[,] m_Data;

    private static int maxIterationForEndlessGame = -1;

    private static List<LevelTile> nodes = new List<LevelTile>();

    private void Awake()
    {
        // Fetch Matrix rows and columns
        GameManager.OnTileSelected += OnTileSelected;
        GameManager.LoadGame += SetupGrid;
        GameManager.SearchWord += CheckForWord;
    }

    private void OnTileSelected(LevelTile tile)
    {
        int length = currentWord.Count;

        if (length > 0) // If I have a word then only check if new tile is a neighbor
        {
            if (!currentWord[length - 1].IsNeighbour(tile))
            {
                return;
            }
        }


        if (currentWord.Contains(tile)) // This is so that it basically backtracks, Maybe a better way.. I'll look if I get time todo.
        {
            List<LevelTile> tiles = new List<LevelTile>();

            bool hasFoundTile = false;

            LevelTile t;

            for (int i = 0; i < length; i++)
            {
                t = currentWord[i];

                if (t == tile)
                {
                    hasFoundTile = true;
                }

                if (!hasFoundTile || t == tile)
                {
                    tiles.Add(t);

                    t.SelectTile();
                }
                else
                {
                    t.DeselectTile();
                }
            }
            if (tiles.Count == 1) // this is like if I backtrack to start.. just unselect.
            {
                tiles[0].DeselectTile();

                tiles.Clear();
            }
            currentWord = new List<LevelTile>(tiles);
        }
        else
        {
            tile.SelectTile();
            currentWord.Add(tile);
        }
    }

    private void SetupGrid(int x, int y, GameModes mode = GameModes.Endless, Objective objective = null, List<GridData> gridData = null, int bonusTileIndex = -1)
    {
        DestroyGrid();

        rows = x;
        columns = y;

        m_Tiles = new LevelTile[rows, columns];

        m_Mode = mode;

        GenerateGridData(gridData);

        GenerateTileSet(bonusTileIndex);
    }

    private void GenerateGridData(List<GridData> data)
    {
        if (data == null)
        {
            return;
        }

        m_Data = new GridData[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                m_Data[i, j] = data[i * columns + j];
            }
        }
    }

    private void DestroyGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (m_Tiles[i, j] == null)
                {
                    continue;
                }
                Destroy(m_Tiles[i, j].gameObject);
            }
        }

        ResetValues();
    }

    private void ResetValues()
    {
        rows = -1;
        columns = -1;
        usedWords.Clear();
        currentWord.Clear();
        m_RowWiseColumns.Clear();
        tilesGeneratedOnce = false;
    }

    #region Generate Word Boggle From Data or Randomly
    private void GenerateTileSet(int bonusIndex = -1)
    {
        float tileWidth = 1.1f;
        float tileHeight = 1.1f;

        float startPosX = -((columns / 2) * tileWidth - (IsEven(columns) ? tileWidth / 2 : 0));
        float startPosY = ((rows / 2) * tileWidth - (IsEven(rows) ? tileWidth / 2 : 0));

        float PosY;
        float PosX;

        int elementCount = 0;

        for (int i = 0; i < rows; i++)
        {
            PosY = startPosY - (i * (tileHeight));
            for (int j = 0; j < columns; j++)
            {
                char x = m_Mode == GameModes.Endless ? letters[UnityEngine.Random.Range(0, letters.Length)] : m_Data[i, j].letter; // If this is levels.. send grid data else random.
                int tileType = m_Mode == GameModes.Endless ? 0 : m_Data[i, j].tileType;

                if (m_Tiles[i, j] == null)
                {
                    PosX = startPosX + j * tileWidth;
                    m_Tiles[i, j] = Instantiate(levelTilePrefab);
                    m_Tiles[i, j].transform.position = new Vector3(PosX, PosY);
                }

                if (!tilesGeneratedOnce)
                {
                    if (bonusIndex >= 0)
                    {
                        elementCount++;
                    }
                    m_Tiles[i, j].SetUp(x, new Vector2Int(i, j), tileType, elementCount == bonusIndex);
                }
                else
                {
                    if (m_Tiles[i, j].GetCharacter().Equals(','))
                    {
                        m_Tiles[i, j].SetCharacter(x);
                    }
                }
            }
        }

        if (m_Mode == GameModes.Endless)
        {
            for (int i = 0; i < rows; i++)
            {
                List<LevelTile> t = new List<LevelTile>();

                for (int j = 0; j < columns; j++)
                {
                    t.Add(m_Tiles[j, i]);
                }

                m_RowWiseColumns.Add(t.ToArray());
            }

        }
        tilesGeneratedOnce = true;
    }
    #endregion

    #region EndlessBoggle
    private void MoveRowWise(Vector2Int[] tiles)
    {
        if (m_Mode == GameModes.Levels) //  Need this method only for Endless.
        {
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            m_Tiles[tiles[i].x, tiles[i].y].SetCharacter(',');
        }

        for (int i = 0; i < rows; i++)
        {
            while (MoveDown(m_RowWiseColumns[i])) ;
        }

        TryAndMakeNewWord();

        GenerateTileSet();
    }

    private void TryAndMakeNewWord()
    {
        // Just checking if I can figure out how to fix Optional Task.. In the end, we have empty slots which will be linked 
        List<LevelTile> emptyTiles = new List<LevelTile>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (m_Tiles[i, j].GetCharacter().Equals(','))
                {
                    emptyTiles.Add(m_Tiles[i, j]);
                }
            }
        }

        List<LevelTile> uniqueNeighbors = new List<LevelTile>();

        int lengthOfEmptyTiles = emptyTiles.Count;

        for (int i = 0; i < lengthOfEmptyTiles; i++)
        {
            Vector2Int indices = emptyTiles[i].m_Index;

            for (int x = indices.x - 1; x <= indices.x + 1; x++)
            {
                for (int y = indices.y - 1; y <= indices.y + 1; y++)
                {
                    if (y >= 0 && x >= 0 && y < columns && x < rows)
                    {
                        if (m_Tiles[x, y] == emptyTiles[i])
                        {
                            continue;
                        }

                        if (!uniqueNeighbors.Contains(m_Tiles[x, y]) && !emptyTiles.Contains(m_Tiles[x, y]))
                        {
                            uniqueNeighbors.Add(m_Tiles[x, y]);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < uniqueNeighbors.Count; i++)
        {
            List<LevelTile> closedTiles = new List<LevelTile>();

            TraverseThroughAllNeighbors(uniqueNeighbors[i], uniqueNeighbors[i], emptyTiles, closedTiles, 0);
        }

        // This will break .. We basically have boxes.. we need a start(Get Neighbors...) we know the iterations, we just need to place characters along that path to that many iterations. find the smallest word Possible?

        int maxWordLength = nodes.Count;
        char startingNode = nodes[0].GetCharacter();
        if (GameManager.Instance.IsPrefix(startingNode.ToString()))
        {
            List<string> allWords = GameManager.Instance.GetAListOfStrings(startingNode.ToString());
            List<string> possible = new List<string>();

            for (int i = 0; i < allWords.Count; i++)
            {
                if (allWords[i].Length <= maxWordLength)
                {
                    if (!possible.Contains(allWords[i]))
                    {
                        possible.Add(allWords[i]);
                    }
                }
            }

            if (possible.Count > 0)
            {
                int x = UnityEngine.Random.Range(0, possible.Count);

                string toMake = possible[x];

                Debug.Log(toMake);

                char[] toChar = toMake.ToCharArray();

                for (int i = 1; i < toChar.Length; i++)
                {
                    nodes[i].SetCharacter(toChar[i]);
                }
            }
        }

        nodes.Clear();
        maxIterationForEndlessGame = -1;
    }

    private void TraverseThroughAllNeighbors(LevelTile n, LevelTile startTile,List<LevelTile> empty, List<LevelTile> closeTiles, int iteration)
    {
        List<LevelTile> closedTiles = new List<LevelTile>(closeTiles);
        List<LevelTile> emptyTiles = new List<LevelTile>(empty);

        closedTiles.Add(n);


        List<LevelTile> x = GetAllNeighborsWithRestriction(n, emptyTiles, closedTiles);

        if (x.Count == 0)
        {
            if (iteration > maxIterationForEndlessGame)
            {
                maxIterationForEndlessGame = iteration;

                nodes = new List<LevelTile>(closedTiles);
            }
        }

        for (int i = 0; i < x.Count; i++)
        {
            TraverseThroughAllNeighbors(x[i], startTile, emptyTiles, closedTiles, iteration + 1);
        }
    }

    private List<LevelTile> GetAllNeighborsWithRestriction(LevelTile tile, List<LevelTile> tilesToLookFrom, List<LevelTile> closedTiles)
    {
        List<LevelTile> bors = new List<LevelTile>();

        for (int i = 0; i < tilesToLookFrom.Count; i++)
        {
            if (tilesToLookFrom[i].IsNeighbour(tile))
            {
                if (!bors.Contains(tile) && !closedTiles.Contains(tilesToLookFrom[i]))
                {
                    bors.Add(tilesToLookFrom[i]);
                }
            }
        }

        return bors;
    }
        

    private bool MoveDown(LevelTile[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            if (array[i].GetCharacter().Equals(',') && !array[i - 1].GetCharacter().Equals(','))
            {
                array[i].SetCharacter(array[i - 1].GetCharacter());

                array[i - 1].SetCharacter(',');

                return true;
            }
        }

        return false;
    }

    #endregion

    private bool IsEven(float x)
    {
        return x % 2 == 0;
    }

    private void CheckForWord()
    {
        int length = currentWord.Count;

        if (length == 0)
        {
            return;
        }

        StringBuilder word = new StringBuilder();

        for (int i = 0; i < length; i++) // This loop maybe can be merged with the 2nd Loop? but in case of not found, extra things will be processed.
        {
            word.Append(currentWord[i].GetCharacter());
            currentWord[i].DeselectTile();
        }

        if (usedWords.Contains(word.ToString()) && m_Mode == GameModes.Levels)
        {
            currentWord.Clear();

            return;
        }

        bool found = GameManager.Instance.SearchForWord(word.ToString()); // Check if is it correct

        Vector2Int[] indices = new Vector2Int[length];

        int bugsFound = 0;

        LevelTile temp; // Needed a caching variable

        for (int i = 0; i < length; i++)
        {
            temp = currentWord[i];

            if (found)
            {
                bugsFound += (temp.IsBugTile) ? 1 : 0;
                indices[i] = (temp.m_Index);
            }
        }

        if (found)
        {
            GameManager.Instance.WordFormedSuccessfully(word.ToString(), indices, length * 2 * (bugsFound > 0 ? (bugsFound + 1) : 1));

            if (m_Mode == GameModes.Levels)
            {
                usedWords.Add(word.ToString());

                List<LevelTile> neighbors = GetNeighborBlocksFromCurrentWord();

                for (int i = 0; i < neighbors.Count; i++)
                {
                    neighbors[i].UpdateTileType();
                }

                if (bugsFound > 0)
                {
                    GameManager.Instance.InventoryItemValueChange(InventoryItemType.Bug, bugsFound);
                }
            }
            else if (m_Mode == GameModes.Endless)
            {
                MoveRowWise(indices);
            }
        }

        currentWord.Clear();
    }


    private List<LevelTile> GetNeighborBlocksFromCurrentWord() // Just for level based...
    {
        List<LevelTile> neighbors = new List<LevelTile>();

        for (int i = 0; i < currentWord.Count; i++)
        {
            Vector2Int index = currentWord[i].m_Index;

            for (int x = index.x - 1; x <= index.x + 1; x++)
            {
                for (int y = index.y - 1; y <= index.y + 1; y++)
                {
                    if (y >= 0 && x >= 0 && y < columns && x < rows)
                    {
                        if (m_Tiles[x, y] == currentWord[i])
                        {
                            continue;
                        }

                        if (!neighbors.Contains(m_Tiles[x, y]) && !currentWord.Contains(m_Tiles[x, y]))
                        {
                            neighbors.Add(m_Tiles[x, y]);
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    private void OnDestroy()
    {
        GameManager.OnTileSelected -= OnTileSelected;
        GameManager.LoadGame -= SetupGrid;
        GameManager.SearchWord -= CheckForWord;
    }
}

public enum GameModes
{
    None,
    Endless,
    Levels
}
