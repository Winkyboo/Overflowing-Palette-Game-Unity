using UnityEngine;
using System.Collections.Generic;
using TMPro;

[System.Serializable] 
public struct LevelLayout
{
    public TileColor[] row;
}
public class GridManager : MonoBehaviour
{
    [Header("UI References")]
    [Header("Game State & UI")]
    public GameObject highlighter;
    public GameObject movableHighlightCircle;
    public Transform[] colorButtonTargets;
    public LevelLayout[] levelData;
    public int gridWidth = 10;
    public int gridHeight = 8;

    [SerializeField] private Tile tilePrefab;

    public int movesRemaining;
    private int maxMoves;
    public TextMeshProUGUI movesText;
    private Dictionary<TileColor, Color> colorMap;
    private Tile[,] allTiles;
    public TileColor selectedColor;
    private TileColor[,] initialTileColors;
    public int resetCharges = 3;
    public TextMeshProUGUI resetText;
    public Color resetTextColorNormal = Color.yellow;
    public Color resetTextColorZero = Color.red;

    void Awake()
    {
        allTiles = new Tile[gridWidth, gridHeight];
        initialTileColors = new TileColor[gridWidth, gridHeight];
        InitializeColorMap();
    }

    void Start()
    {
        UpdateSelectionUI();
        maxMoves = movesRemaining;
        UpdateMovesText();
        GenerateGrid();
        FocusCameraOnGrid();
        UpdateResetText();
    }

    void UpdateMovesText()
    {
        if (movesText != null) { movesText.text = $"{movesRemaining}"; }
    }

    void InitializeColorMap()
    {
        colorMap = new Dictionary<TileColor, Color>();
        colorMap.Add(TileColor.Blue, new Color(0.25f, 0.5f, 1f));
        colorMap.Add(TileColor.Red, new Color(0.9f, 0.2f, 0.2f));
        colorMap.Add(TileColor.Yellow, new Color(0.95f, 0.9f, 0.3f));
        colorMap.Add(TileColor.Green, new Color(0.3f, 0.8f, 0.6f));
        colorMap.Add(TileColor.Black, new Color(0.2f, 0.2f, 0.2f));
    }

    void GenerateGrid()
    {
        if (tilePrefab == null) { return; }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile newTile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity, this.transform);
                newTile.name = $"Tile {x},{y}";
                newTile.gridManager = this;
                allTiles[x, y] = newTile;

                TileColor colorFromLevel = levelData[gridHeight - 1 - y].row[x];

                initialTileColors[x, y] = colorFromLevel;

                newTile.color = colorFromLevel;
                newTile.SetColor(colorMap[colorFromLevel]);

                if (colorFromLevel == TileColor.Black)
                {
                    newTile.isLocked = true;
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { SelectColorBlue(); }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { SelectColorRed(); }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) { SelectColorYellow(); }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) { SelectColorGreen(); }
        if (Input.GetKeyDown(KeyCode.R)) ResetGrid();
    }

    void UpdateSelectionUI()
    {
        bool colorIsSelected = selectedColor != TileColor.None;

        if (highlighter != null) highlighter.SetActive(colorIsSelected);
        if (movableHighlightCircle != null) movableHighlightCircle.SetActive(colorIsSelected);

        if (colorIsSelected)
        {
            int colorIndex = (int)selectedColor - 1;
            if (colorIndex >= 0 && colorIndex < colorButtonTargets.Length)
            {
                Transform targetButton = colorButtonTargets[colorIndex];
                if (targetButton == null) return;

                if (highlighter != null)
                {
                    highlighter.transform.position = targetButton.position + new Vector3(150f, 0, 0);
                }

                if (movableHighlightCircle != null)
                {
                    movableHighlightCircle.transform.position = targetButton.position;
                }
            }
        }
    }

    public void SelectColorBlue() { selectedColor = TileColor.Blue; Debug.Log("Warna dipilih: BIRU"); UpdateSelectionUI(); }
    public void SelectColorRed() { selectedColor = TileColor.Red; Debug.Log("Warna dipilih: MERAH"); UpdateSelectionUI(); }
    public void SelectColorYellow() { selectedColor = TileColor.Yellow; Debug.Log("Warna dipilih: KUNING"); UpdateSelectionUI(); }
    public void SelectColorGreen() { selectedColor = TileColor.Green; Debug.Log("Warna dipilih: HIJAU"); UpdateSelectionUI(); }

    public void OnTileClicked(Tile clickedTile)
    {
        if (selectedColor == TileColor.None)
        {
            Debug.Log("Pilih warna dulu!");
            return;
        }
        if (clickedTile.color == selectedColor) return;
        movesRemaining--;
        UpdateMovesText();
        FloodFill(clickedTile, selectedColor);
        CheckGameState();
    }

    void CheckGameState()
    {
        if (movesRemaining <= 0)
        {
            Debug.Log("Langkah Habis! Mereset puzzle...");
            ResetGrid();
        }
    }

    void FloodFill(Tile startTile, TileColor newColor)
    {
        TileColor originalColor = startTile.color;
        if (originalColor == newColor) return;
        Queue<Tile> processQueue = new Queue<Tile>();
        processQueue.Enqueue(startTile);

        while (processQueue.Count > 0)
        {
            Tile currentTile = processQueue.Dequeue();
            currentTile.color = newColor;
            currentTile.SetColor(colorMap[newColor]);
            List<Tile> neighbors = GetNeighbors(currentTile);
            foreach (Tile neighbor in neighbors)
            {
                if (neighbor.color == originalColor)
                {
                    processQueue.Enqueue(neighbor);
                }
            }
        }
    }

    List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int tilePos = FindTilePosition(tile);
        if (tilePos.y + 1 < gridHeight) neighbors.Add(allTiles[tilePos.x, tilePos.y + 1]);
        if (tilePos.y - 1 >= 0) neighbors.Add(allTiles[tilePos.x, tilePos.y - 1]);
        if (tilePos.x + 1 < gridWidth) neighbors.Add(allTiles[tilePos.x + 1, tilePos.y]);
        if (tilePos.x - 1 >= 0) neighbors.Add(allTiles[tilePos.x - 1, tilePos.y]);
        return neighbors;
    }

    Vector2Int FindTilePosition(Tile tileToFind)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (allTiles[x, y] == tileToFind)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    void FocusCameraOnGrid()
    {
        float centerX = (gridWidth - 1) / 2.0f;
        float centerY = (gridHeight - 1) / 2.0f;
        Camera.main.transform.position = new Vector3(centerX, centerY, Camera.main.transform.position.z);
        float padding = 1.5f;
        float requiredSizeY = (gridHeight / 2.0f) + padding;
        float requiredSizeX = (gridWidth / (2.0f * Camera.main.aspect)) + padding;
        Camera.main.orthographicSize = Mathf.Max(requiredSizeY, requiredSizeX);
    }
    public void ResetGrid()
    {
        if (resetCharges <= 0) return; 
        resetCharges--;
        UpdateResetText();
        Debug.Log("Meresset grid ke kondisi awal...");
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                TileColor originalColor = initialTileColors[x, y];
                Tile tile = allTiles[x, y];
                tile.color = originalColor;
                tile.SetColor(colorMap[originalColor]);
            }
        }
        selectedColor = TileColor.None;
        UpdateSelectionUI();
        movesRemaining = maxMoves;
        UpdateMovesText();
    }

    void UpdateResetText()
    {
        if (resetText != null)
        {
            resetText.text = resetCharges.ToString(); 
        
            if (resetCharges <= 0) resetText.color = resetTextColorZero; 
            else resetText.color = resetTextColorNormal; 
        }
    }
}