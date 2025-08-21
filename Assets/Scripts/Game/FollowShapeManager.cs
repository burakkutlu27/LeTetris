using UnityEngine;

public class FollowShapeManager : MonoBehaviour
{
    private ShapeManager followShape;
    private ShapeManager currentRealShape;
    
    [Header("Ghost Piece Settings")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.3f); // Transparent white
    public bool enableGhostPiece = true;

    private void Start()
    {
        // Ghost piece başlangıçta görünmez
        if (followShape != null)
        {
            followShape.gameObject.SetActive(false);
        }
    }

    public void CreateFollowShape(ShapeManager realShape, BoardManager board)
    {
        if (!enableGhostPiece || realShape == null || board == null)
            return;

        // Eğer real shape değişmişse veya follow shape yoksa yeniden oluştur
        if (currentRealShape != realShape || followShape == null)
        {
            ResetFollowShape();
            
            // Yeni ghost piece oluştur
            GameObject ghostObject = Instantiate(realShape.gameObject, realShape.transform.position, realShape.transform.rotation);
            followShape = ghostObject.GetComponent<ShapeManager>();
            followShape.name = "GhostPiece_" + realShape.name;
            
            // Ghost piece'i aktif real shape'in parent'ı altında tut
            ghostObject.transform.SetParent(this.transform);
            
            // Collider'ları kaldır (eğer varsa) - ghost piece fizik etkileşimi yapmamalı
            Collider2D[] colliders = ghostObject.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
            
            // Tüm sprite renderer'ları ghost rengine çevir
            SpriteRenderer[] allSprites = followShape.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in allSprites)
            {
                sr.color = ghostColor;
                // Ghost piece'in render layer'ını değiştir (arka planda görünsün)
                sr.sortingOrder = sr.sortingOrder - 1;
            }
            
            currentRealShape = realShape;
        }
        else
        {
            // Mevcut ghost piece'i güncelle
            followShape.transform.position = realShape.transform.position;
            followShape.transform.rotation = realShape.transform.rotation;
        }

        // Ghost piece'i tabana kadar indir
        UpdateGhostPiecePosition(board);
        
        // Ghost piece'i göster
        followShape.gameObject.SetActive(true);
    }

    private void UpdateGhostPiecePosition(BoardManager board)
    {
        if (followShape == null || board == null)
            return;

        // Ghost piece'i aşağı doğru hareket ettir ta ki geçersiz pozisyona gelene kadar
        while (board.IsPositionValid(followShape))
        {
            followShape.MoveDown();
        }
        
        // Son geçerli pozisyona geri al
        followShape.MoveUp();
    }

    public void UpdateFollowShape(ShapeManager realShape, BoardManager board)
    {
        // Bu method hareket veya rotasyon sonrası çağrılacak
        if (enableGhostPiece && realShape != null && board != null)
        {
            CreateFollowShape(realShape, board);
        }
    }

    public void ResetFollowShape()
    {
        if (followShape != null)
        {
            Destroy(followShape.gameObject);
            followShape = null;
        }
        currentRealShape = null;
    }

    public void SetGhostPieceVisibility(bool visible)
    {
        enableGhostPiece = visible;
        if (followShape != null)
        {
            followShape.gameObject.SetActive(visible);
        }
    }

    public void SetGhostPieceColor(Color newColor)
    {
        ghostColor = newColor;
        if (followShape != null)
        {
            SpriteRenderer[] allSprites = followShape.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in allSprites)
            {
                sr.color = ghostColor;
            }
        }
    }
}
