using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private Transform tilePrefab;

    public int width = 10;
    public int height = 22;
    private GameObject[,] grid;

    private ScoreManager scoreManager;

    private FollowShapeManager followShape;
    public BoardManager boardManager;

    public ParticleManager lineEffect;
    
    private void Awake()
    {
        // Grid'i Awake'de initialize et ki diğer scriptler Start'ta erişebilsin
        grid = new GameObject[width, height];
    }

    private void Start()
    {
        CreateEmptyBoard();
        scoreManager = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
        followShape = GameObject.FindGameObjectWithTag("FollowShapeManager").GetComponent<FollowShapeManager>();
    }

    bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsGameOver()
    {
        // Grid henüz initialize edilmemişse false döndür
        if (grid == null)
        {
            Debug.LogWarning("Grid not initialized yet, returning false for GameOver");
            return false;
        }
        
        // Board'ın üst birkaç satırında blok var mı kontrol et
        // Height-2 ve height-3 satırlarında blok varsa GameOver yakın
        for (int y = height - 3; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    Debug.Log($"GameOver: Block found at critical row ({x}, {y})");
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool CanPlaceNewShape(ShapeManager shape)
    {
        // Grid henüz initialize edilmemişse true döndür (spawn edebilir)
        if (grid == null)
        {
            Debug.LogWarning("Grid not initialized yet, allowing shape placement");
            return true;
        }
        
        // Yeni şekil board'ın görünür alanına yerleştirilebilir mi?
        foreach (Transform block in shape.transform)
        {
            int x = Mathf.RoundToInt(block.position.x);
            int y = Mathf.RoundToInt(block.position.y);
            
            // Board içinde çakışma var mı?
            if (y < height && y >= 0 && x >= 0 && x < width)
            {
                if (grid[x, y] != null)
                {
                    Debug.Log($"Cannot place new shape: collision at ({x}, {y})");
                    return false;
                }
            }
        }
        return true;
    }

    public bool CanSpawnShape(ShapeManager shape)
    {
        // Yeni şekil spawn edilebilir mi kontrol et
        foreach (Transform block in shape.transform)
        {
            int x = Mathf.RoundToInt(block.position.x);
            int y = Mathf.RoundToInt(block.position.y);
            
            // Board içinde ve o pozisyonda başka blok yoksa OK
            if (IsInsideBoard(x, y) && grid[x, y] != null)
            {
                // Debug.Log($"Cannot spawn: Position ({x}, {y}) is occupied");
                return false;
            }
        }
        return true;
    }

    public bool IsPositionValid(ShapeManager shape)
    {
        // Grid henüz initialize edilmemişse true döndür
        if (grid == null)
        {
            return true;
        }

        foreach (Transform block in shape.transform)
        {
            int x = Mathf.RoundToInt(block.position.x);
            int y = Mathf.RoundToInt(block.position.y);

            // Board'ın altına düşmüş mü kontrol et
            if (y < 0)
                return false;

            // Board'ın yanlarından çıkmış mı kontrol et
            if (x < 0 || x >= width)
                return false;

            // Board'ın üstünde ise OK (spawn alanı)
            if (y >= height)
                continue;

            // Board içindeyse çakışma kontrolü yap
            if (grid[x, y] != null && grid[x, y].transform.parent != shape.transform)
                return false;
        }
        return true;
    }

    public void StoreShapeInGrid(ShapeManager shape)
    {
        // Önce tüm blokları bir listede topla
        List<Transform> blocksToStore = new List<Transform>();
        foreach (Transform block in shape.transform)
        {
            blocksToStore.Add(block);
        }
        
        // Şimdi blokları güvenli şekilde işle
        foreach (Transform block in blocksToStore)
        {
            // Pozisyonu tam sayıya hizala
            Vector3 pos = block.position;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            block.position = pos;
            
            int x = Mathf.RoundToInt(block.position.x);
            int y = Mathf.RoundToInt(block.position.y);

            if (IsInsideBoard(x, y))
            {
                // Önce parent'ı değiştir, sonra grid'e kaydet
                block.transform.SetParent(transform);
                grid[x, y] = block.gameObject;
            }
            else
            {
                Debug.LogWarning($"Block position out of bounds: ({x}, {y}) - Board size: {width}x{height}");
                // Sınır dışındaki bloku yok et
                Destroy(block.gameObject);
            }
        }
        
        // Shape container'ını temizle (artık boş olmalı)
        Destroy(shape.gameObject);
    }

    public void ClearLines()
    {
        // Oyun başlamadıysa satır temizleme işlemini yapma
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null || !gameManager.gameStarted)
        {
            return;
        }
        
        bool lineCleared = false;
        bool boardCompletelyCleared = false;
        int linesCleared = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                MoveAllLinesDown(y + 1);
                y++; // Aynı satırı tekrar kontrol et
                lineCleared = true;
                linesCleared++;
            }
        }
        if (lineCleared)
            {
                scoreManager.LineScore(linesCleared);
                SoundManager.instance.PlayRandomVocal();
            }
        // Board tamamen temizlenmiş mi kontrol et
        boardCompletelyCleared = IsBoardEmpty();
        if (boardCompletelyCleared)
            {
                // Burada sen index belirleyeceksin → mesela 2. SFX
                SoundManager.instance.PlaySFX(4);
            }
    }
    bool IsBoardEmpty()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                    return false;
            }
        }
        return true;
    }


    bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    void ClearLine(int y)
    {
        // Line effect'i çal
        PlayLineEffect(y);
        
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y]);
                grid[x, y] = null;
            }
        }
    }

    void MoveAllLinesDown(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].transform.position = new Vector3(x, y - 1, 0);
                }
            }
        }
    }

    void CreateEmptyBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab is not assigned!");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Transform tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                tile.name = $"Tile {x},{y}";
                tile.SetParent(transform, false);
            }
        }
    }

    void PlayLineEffect(int y)
    {
        if(lineEffect)
        {
            lineEffect.transform.position = new Vector3(0, y, 0);
            lineEffect.PlayEffect();
        }
    }
}
