using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SpawnerManager : MonoBehaviour
{
    public GameObject[] allShapes; // ShapeManager yerine GameObject tutuyoruz
    
    [Header("Preview UI References")]
    [SerializeField] private Image nextShapeImage; // Next shape preview image
    [SerializeField] private Image nextNextShapeImage; // Next+1 shape preview image
    [SerializeField] private Image replaceShapeImage; // Replace shape preview image
    
    [Header("Shape Queue System")]
    private Queue<int> shapeQueue = new Queue<int>(); // Shape indekslerini tutar
    private const int QUEUE_SIZE = 7; // Bag of 7 sistemi için
    
    [Header("Replace Shape")]
    private int replaceShapeIndex = -1; // Replace shape'in indeksi
    private int currentActiveShapeIndex = -1; // Şu anda aktif olan shape'in indeksi
    
    private void Start()
    {
        InitializeQueue();
    }
    
    // Queue'yu başlat (Bag of 7 sistemi)
    private void InitializeQueue()
    {
        FillQueue();
        InitializeReplaceShape();
        UpdatePreviewUI();
    }
    
    // Replace shape'i başlat
    private void InitializeReplaceShape()
    {
        replaceShapeIndex = Random.Range(0, allShapes.Length);
        UpdateReplaceShapeUI();
    }
    
    // Queue'yu doldur (her shape tipinden bir tane garantili)
    private void FillQueue()
    {
        List<int> bag = new List<int>();
        
        // Her shape tipinden bir tane ekle
        for (int i = 0; i < allShapes.Length; i++)
        {
            bag.Add(i);
        }
        
        // Listeyi karıştır
        for (int i = 0; i < bag.Count; i++)
        {
            int temp = bag[i];
            int randomIndex = Random.Range(i, bag.Count);
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }
        
        // Queue'ya ekle
        foreach (int shapeIndex in bag)
        {
            shapeQueue.Enqueue(shapeIndex);
        }
    }
    
    // Sıradaki shape'i al ve queue'yu güncelle
    public ShapeManager GetNextShape()
    {
        // Prefab listesi boşsa uyarı ver
        if (allShapes == null || allShapes.Length == 0)
        {
            Debug.LogError("SpawnerManager: Prefab listesi boş!");
            return null;
        }
        
        // Queue boşalırsa yeniden doldur
        if (shapeQueue.Count == 0)
        {
            FillQueue();
        }
        
        // Sıradaki shape indeksini al
        int shapeIndex = shapeQueue.Dequeue();
        
        // Aktif shape indeksini güncelle
        currentActiveShapeIndex = shapeIndex;
        
        // Yeni rastgele shape ekle (queue'nun sonuna)
        if (shapeQueue.Count < QUEUE_SIZE)
        {
            AddRandomShapeToQueue();
        }
        
        // UI'ı güncelle
        UpdatePreviewUI();

        if (allShapes[shapeIndex] == null)
        {
            Debug.LogError($"SpawnerManager: Prefab {shapeIndex} null!");
            return null;
        }

        // Board'ın dışında (üstte), ortada spawn et - 2 blok daha yukarıda
        Vector3 spawnPosition = new Vector3(4f, 26f, 0f);
        GameObject obj = Instantiate(allShapes[shapeIndex], spawnPosition, Quaternion.identity);

        // ShapeManager script'ini root veya child'lardan bul
        ShapeManager shape = obj.GetComponent<ShapeManager>();
        if (shape == null)
        {
            shape = obj.GetComponentInChildren<ShapeManager>();
        }

        // Hâlâ bulunamadıysa hata ver
        if (shape == null)
        {
            Debug.LogError($"SpawnerManager: {allShapes[shapeIndex].name} prefabında ShapeManager script'i bulunamadı!");
            return null;
        }

        return shape;
    }
    
    // Queue'ya rastgele shape ekle
    private void AddRandomShapeToQueue()
    {
        int randomIndex = Random.Range(0, allShapes.Length);
        shapeQueue.Enqueue(randomIndex);
    }
    
    // Preview UI'ını güncelle
    private void UpdatePreviewUI()
    {
        if (shapeQueue.Count >= 2)
        {
            int[] queueArray = shapeQueue.ToArray();
            Sprite nextSprite = GetShapePreviewSprite(queueArray[0]);
            Sprite nextNextSprite = GetShapePreviewSprite(queueArray[1]);
            
            // Direkt Image update
            UpdateShapeImages(nextSprite, nextNextSprite);
        }
    }
    
    // Shape'in preview sprite'ını al
    private Sprite GetShapePreviewSprite(int shapeIndex)
    {
        if (shapeIndex >= 0 && shapeIndex < allShapes.Length && allShapes[shapeIndex] != null)
        {
            ShapeManager shapeManager = allShapes[shapeIndex].GetComponent<ShapeManager>();
            if (shapeManager == null)
            {
                shapeManager = allShapes[shapeIndex].GetComponentInChildren<ShapeManager>();
            }
            
            if (shapeManager != null)
            {
                return shapeManager.PreviewSprite;
            }
        }
        return null;
    }
    
    // Image'ları direkt güncelle
    private void UpdateShapeImages(Sprite nextSprite, Sprite nextNextSprite)
    {
        if (nextShapeImage != null)
        {
            nextShapeImage.sprite = nextSprite;
            nextShapeImage.enabled = nextSprite != null; // Sprite yoksa gizle
        }
        
        if (nextNextShapeImage != null)
        {
            nextNextShapeImage.sprite = nextNextSprite;
            nextNextShapeImage.enabled = nextNextSprite != null; // Sprite yoksa gizle
        }
    }
    
    // Replace shape UI'ını güncelle
    private void UpdateReplaceShapeUI()
    {
        if (replaceShapeImage != null && replaceShapeIndex >= 0)
        {
            Sprite replaceSprite = GetShapePreviewSprite(replaceShapeIndex);
            replaceShapeImage.sprite = replaceSprite;
            replaceShapeImage.enabled = replaceSprite != null;
        }
    }

    // Eski metod - geriye uyumluluk için (deprecated)
    public ShapeManager GetRandomShape()
    {
        Debug.LogWarning("GetRandomShape deprecated. Use GetNextShape instead.");
        return GetNextShape();
    }
    public ShapeManager CreateReplaceShape()
    {
        // Eğer replace shape index belirlenmemişse, rastgele bir tane seç (aktif shape'den farklı)
        if (replaceShapeIndex < 0)
        {
            replaceShapeIndex = GetDifferentShapeIndex(currentActiveShapeIndex);
            UpdateReplaceShapeUI();
        }
        
        // Replace shape'i oluştur
        GameObject replaceShapeObj = Instantiate(allShapes[replaceShapeIndex], transform.position, Quaternion.identity);
        ShapeManager replaceShape = replaceShapeObj.GetComponent<ShapeManager>();
        if (replaceShape == null)
        {
            replaceShape = replaceShapeObj.GetComponentInChildren<ShapeManager>();
        }
        
        // Yeni replace shape seç ve UI'ı güncelle (aktif shape'den farklı)
        replaceShapeIndex = GetDifferentShapeIndex(currentActiveShapeIndex);
        UpdateReplaceShapeUI();
        
        return replaceShape;
    }
    
    // Verilen shape indeksinden farklı bir shape indeksi döndür
    private int GetDifferentShapeIndex(int excludeIndex)
    {
        if (allShapes.Length <= 1)
        {
            return 0; // Sadece bir shape varsa o shape'i döndür
        }
        
        int newIndex;
        int attempts = 0;
        do
        {
            newIndex = Random.Range(0, allShapes.Length);
            attempts++;
        }
        while (newIndex == excludeIndex && attempts < 10); // Sonsuz döngüyü önlemek için 10 deneme limiti
        
        return newIndex;
    }
    
}
