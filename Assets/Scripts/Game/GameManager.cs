using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    SpawnerManager spawnerManager;
    public BoardManager boardManager;
    public ShapeManager activeShape;

    [Header("Game Settings")]
    [Range(0.05f, 1f)]
    [SerializeField] private float moveInterval = 0.5f;
    [SerializeField] private float fastDropInterval = 0.1f; // Hızlı düşüş için daha kısa interval
    private float moveTimer;
    private bool isFastDropping = false; // Aşağı ok tuşuna basılı tutulup tutulmadığını kontrol et

    public bool gameOver = false;
    public bool gameStarted = false; // Oyun başlatıldı mı kontrolü

    public bool isClockWise = true;

    public GameObject gameOverPanel;

    private ScoreManager scoreManager;
    private FollowShapeManager followShape;

    private ShapeManager replaceShape;
    private int replaceShapeTypeIndex = -1; // Replace shape'in type indeksini sakla

    public Image replaceShapeImage;

    private bool canReplaceShape = true;

    public ParticleManager[] levelUpParticals = new ParticleManager[5];

    private void Awake()
    {
        spawnerManager = GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnerManager>();
        boardManager = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardManager>();
        scoreManager = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
        scoreManager.OnLevelUp += HandleLevelUp;
        followShape = GameObject.FindGameObjectWithTag("FollowShapeManager").GetComponent<FollowShapeManager>();
    }
    private void Start()
    {
        // Oyun henüz başlamadı, sadece UI'ı hazırla
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        // DelayedStart sadece oyun başlatıldığında çağrılacak
    }
    void HandleLevelUp(int newLevel)
    {
        // Level arttıkça moveInterval’ı küçült
        // Minimum hızı 0.1f ile sınırlayalım
        moveInterval = Mathf.Max(0.1f, moveInterval - 0.075f);
        StartCoroutine(LevelUpFNC());
        Debug.Log($"Level {newLevel} → Yeni hız: {moveInterval}");
    }
    private System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        SpawnNewShape();
    }
    
    // IntroManager tarafından çağrılacak metod
    public void StartGame()
    {
        Debug.Log("StartGame() called!");
        if (!gameStarted)
        {
            gameStarted = true;
            Debug.Log("Starting game for the first time...");
            
            // Oyun başladığında müziği başlat
            if (SoundManager.instance != null)
            {
                SoundManager.instance.StartGameMusic();
            }
            
            StartCoroutine(DelayedStart());
        }
        else
        {
            Debug.Log("Game already started, ignoring call.");
        }
    }

    private void Update()
    {
        if (!gameStarted || gameOver || !activeShape) return;

        // Klavye kontrolleri
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            activeShape.MoveLeft();
            if (!boardManager.IsPositionValid(activeShape))
            {
                SoundManager.instance.PlaySFX(1); // SFX for invalid move
                activeShape.MoveRight();
            }
            else
            {
                SoundManager.instance.PlaySFX(3);
                // Ghost piece'i güncelle
                if (followShape != null)
                {
                    followShape.UpdateFollowShape(activeShape, boardManager);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            activeShape.MoveRight();
            if (!boardManager.IsPositionValid(activeShape))
            {
                SoundManager.instance.PlaySFX(1); // SFX for invalid move
                activeShape.MoveLeft();
            }
            else
            {
                SoundManager.instance.PlaySFX(3);
                // Ghost piece'i güncelle
                if (followShape != null)
                {
                    followShape.UpdateFollowShape(activeShape, boardManager);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            activeShape.TurnLeft();
            if (!boardManager.IsPositionValid(activeShape))
            {
                SoundManager.instance.PlaySFX(1); // SFX for invalid move
                activeShape.TurnRight();
            }
            else
            {
                SoundManager.instance.PlaySFX(3);
                // Ghost piece'i güncelle (rotasyon sonrası)
                if (followShape != null)
                {
                    followShape.UpdateFollowShape(activeShape, boardManager);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            activeShape.TurnRight();
            if (!boardManager.IsPositionValid(activeShape))
            {
                SoundManager.instance.PlaySFX(1); // SFX for invalid move}
                activeShape.TurnLeft();
            }
            else
            {
                SoundManager.instance.PlaySFX(3);
                // Ghost piece'i güncelle (rotasyon sonrası)
                if (followShape != null)
                {
                    followShape.UpdateFollowShape(activeShape, boardManager);
                }
            }
        }

        // Aşağı ok tuşu kontrolü - basılı tutma destekli
        if (Input.GetKey(KeyCode.DownArrow))
        {
            isFastDropping = true;
        }
        else
        {
            isFastDropping = false;
        }

        // Tek basım ile manuel hareket (eski davranış korunuyor)
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveShapeDown();

        // Otomatik aşağı inme - hızlı düşüş moduna göre interval ayarla
        float currentMoveInterval = isFastDropping ? fastDropInterval : moveInterval;
        moveTimer += Time.deltaTime;
        if (moveTimer >= currentMoveInterval)
        {
            moveTimer = 0;
            MoveShapeDown();
            if (!isFastDropping) // Normal hızda ses çal, hızlı düşüşte çok fazla ses olmasın
            {
                SoundManager.instance.PlaySFX(3);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isClockWise = !isClockWise;
        }

        // Space tuşu ile shape değiştirme
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeReplaceShape();
        }
    }
    //Şekilin oturması fonskiyonu
    void MoveShapeDown()
    {
        if (activeShape == null || gameOver) return;

        activeShape.MoveDown();

        if (!boardManager.IsPositionValid(activeShape))
        {
            // Geçersizse geri al
            activeShape.MoveUp();

            // Grid'e yerleştir
            boardManager.StoreShapeInGrid(activeShape);
            SoundManager.instance.PlaySFX(5);
            
            // Ghost piece'i temizle (shape yerleştirildi)
            if (followShape != null)
            {
                followShape.ResetFollowShape();
            }

            // Replace shape'i güncelle
            if (replaceShape != null)
            {
                replaceShape = spawnerManager.CreateReplaceShape();
                replaceShapeImage.sprite = replaceShape.PreviewSprite;
                replaceShape.gameObject.SetActive(false);
            }
            
            // Replace özelliğini tekrar aktif et
            canReplaceShape = true;
            
            // Satırları temizle
            boardManager.ClearLines();

            // Shape artık aktif değil
            activeShape = null;

            // Yeni shape spawn et
            SpawnNewShape();
        }
        else
        {
            // Pozisyonu hizala
            Vector3 pos = activeShape.transform.position;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            activeShape.transform.position = pos;
        }
    }

    void SpawnNewShape()
    {
        Debug.Log("SpawnNewShape() called!");
        if (gameOver) return;

        // Önce GameOver kontrolü yap
        if (boardManager.IsGameOver())
        {
            gameOver = true;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            Debug.Log("Game Over! Board is too full to continue!");
            SoundManager.instance.PlaySFX(6); // SFX for game over
            return;
        }

        activeShape = spawnerManager.GetNextShape();

        if (activeShape != null)
        {
            // Spawn pozisyonu board'ın üstünde başlat - 2 blok daha yukarıda
            Vector3 spawnPos = new Vector3(boardManager.width / 2, boardManager.height + 3, 0);
            activeShape.transform.position = spawnPos;

            // Pozisyonu hizala
            Vector3 pos = activeShape.transform.position;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            activeShape.transform.position = pos;

            Debug.Log($"New shape spawned at position: {activeShape.transform.position}");

            if(replaceShape == null)
            {
                replaceShape = spawnerManager.CreateReplaceShape();
                replaceShapeImage.sprite = replaceShape.PreviewSprite;
                replaceShape.gameObject.SetActive(false);
            }
            // Şeklin board'a inebilir durumda olup olmadığını kontrol et
            // Birkaç step aşağı indirip çakışma var mı bak
            for (int i = 0; i < 5; i++)
            {
                activeShape.MoveDown();
                if (!boardManager.IsPositionValid(activeShape))
                {
                    activeShape.MoveUp();
                    if (!boardManager.CanPlaceNewShape(activeShape))
                    {
                        gameOver = true;
                        Debug.Log("Game Over! Cannot place new shape!");
                        Destroy(activeShape.gameObject);
                        activeShape = null;
                        return;
                    }
                    break;
                }
            }

            // Yeni shape için ghost piece oluştur
            if (followShape != null && activeShape != null)
            {
                followShape.CreateFollowShape(activeShape, boardManager);
            }
        }
    }
    public void RotateShape()
    {
        if (activeShape == null) return;

        // Rotate yönünü toggle et
        isClockWise = !isClockWise;

        // ShapeManager'a uygula
        activeShape.isTurnClockWise(isClockWise);

        // Opsiyonel: Ses efekti
        SoundManager.instance.PlaySFX(3);

        Debug.Log("Shape rotated. Current direction: " + (isClockWise ? "Clockwise" : "CounterClockwise"));
    }

    public void ChangeReplaceShape()
    {
        if(canReplaceShape && activeShape != null && replaceShape != null)
        {
            canReplaceShape = false;
            
            // Ses efekti çal
            SoundManager.instance.PlaySFX(3);
            
            // Mevcut aktif şeklin pozisyonunu kaydet
            Vector3 currentPosition = activeShape.transform.position;
            
            // Aktif şekli deaktif et
            activeShape.gameObject.SetActive(false);
            
            // Replace şeklini aktif et ve pozisyonunu ayarla
            replaceShape.gameObject.SetActive(true);
            replaceShape.transform.position = currentPosition;
            
            // Şekilleri değiştir
            activeShape = replaceShape;
            
            // Yeni replace şekli oluştur (aktif şekilden farklı olacak şekilde)
            replaceShape = spawnerManager.CreateReplaceShape();
            if (replaceShape != null)
            {
                replaceShapeImage.sprite = replaceShape.PreviewSprite;
                replaceShape.gameObject.SetActive(false);
            }
        }
        
        // Ghost piece'i güncelle
        if(followShape && activeShape != null)
        {
            followShape.ResetFollowShape();
            followShape.CreateFollowShape(activeShape, boardManager);
        }   
    }

    IEnumerator LevelUpFNC()
    {
        yield return new WaitForSeconds(.2f);
        int counter = 0;
        while (counter < levelUpParticals.Length)
        {
            levelUpParticals[counter].PlayEffect();
            counter++;
            yield return new WaitForSeconds(.2f);
        }
    }
}
