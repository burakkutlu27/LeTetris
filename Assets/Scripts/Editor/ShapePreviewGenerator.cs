using UnityEngine;
using UnityEditor;
using System.IO;

public class ShapePreviewGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Shape Previews")]
    public static void ShowWindow()
    {
        GetWindow<ShapePreviewGenerator>("Shape Preview Generator");
    }
    
    private Camera previewCamera;
    private int previewSize = 128; // Preview sprite boyutu
    
    private void OnGUI()
    {
        GUILayout.Label("Shape Preview Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Bu tool tüm shape prefablarının tam görüntüsünü alır ve preview sprite oluşturur.");
        GUILayout.Space(5);
        
        previewSize = EditorGUILayout.IntSlider("Preview Size", previewSize, 64, 256);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate All Shape Previews"))
        {
            GenerateAllPreviews();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Not: Önce 'PreviewSprites' klasörü oluşturulacak.", EditorStyles.helpBox);
    }
    
    private void GenerateAllPreviews()
    {
        // PreviewSprites klasörü oluştur
        string folderPath = "Assets/PreviewSprites";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "PreviewSprites");
        }
        
        // Geçici kamera oluştur
        SetupPreviewCamera();
        
        // Tüm prefabları bul
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        
        int generated = 0;
        
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                ShapeManager shapeManager = prefab.GetComponent<ShapeManager>();
                if (shapeManager == null)
                    shapeManager = prefab.GetComponentInChildren<ShapeManager>();
                
                if (shapeManager != null)
                {
                    Sprite previewSprite = GeneratePreviewSprite(prefab);
                    if (previewSprite != null)
                    {
                        // Preview sprite'ı ShapeManager'a ata
                        SerializedObject serializedObject = new SerializedObject(shapeManager);
                        SerializedProperty previewSpriteProperty = serializedObject.FindProperty("previewSprite");
                        
                        if (previewSpriteProperty != null)
                        {
                            previewSpriteProperty.objectReferenceValue = previewSprite;
                            serializedObject.ApplyModifiedProperties();
                            generated++;
                            
                            Debug.Log($"Preview generated for {prefab.name}");
                        }
                    }
                }
            }
        }
        
        // Kamerayı temizle
        if (previewCamera != null)
        {
            DestroyImmediate(previewCamera.gameObject);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Complete", $"Generated {generated} shape previews!", "OK");
    }
    
    private void SetupPreviewCamera()
    {
        GameObject cameraObj = new GameObject("PreviewCamera");
        previewCamera = cameraObj.AddComponent<Camera>();
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 3f;
        previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Tamamen şeffaf
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.cullingMask = -1; // Tüm layer'ları render et
        previewCamera.enabled = false;
        
        // Kameranın depth ve rendering ayarları
        previewCamera.depth = -100; // En altta render et
        previewCamera.renderingPath = RenderingPath.Forward;
    }
    
    private Sprite GeneratePreviewSprite(GameObject prefab)
    {
        // Tamamen izole bir layer oluştur
        int previewLayer = 31; // Preview için ayrı layer
        
        // Geçici preview scene oluştur
        GameObject previewRoot = new GameObject("PreviewRoot");
        GameObject tempShape = Instantiate(prefab, previewRoot.transform);
        
        try
        {
            // Shape ve tüm alt objelerini preview layer'ına taşı
            SetLayerRecursively(tempShape, previewLayer);
            
            // Shape'i origin'e taşı
            tempShape.transform.position = Vector3.zero;
            
            // Shape'in tüm çocuklarını bul ve bounds hesapla
            Bounds shapeBounds = CalculateShapeBounds(tempShape);
            
            if (shapeBounds.size == Vector3.zero)
            {
                Debug.LogWarning($"Could not calculate bounds for {prefab.name}");
                return null;
            }
            
            // Shape'i tam ortala
            Vector3 offset = -shapeBounds.center;
            tempShape.transform.position = offset;
            
            // Kamerayı sadece preview layer'ını render edecek şekilde ayarla
            previewCamera.cullingMask = 1 << previewLayer; // Sadece preview layer
            previewCamera.transform.position = new Vector3(0, 0, -10f);
            
            // Orthographic size'ı shape boyutuna göre ayarla (biraz padding ile)
            float maxSize = Mathf.Max(shapeBounds.size.x, shapeBounds.size.y);
            previewCamera.orthographicSize = maxSize * 0.8f; // Biraz daha padding
            
            // Yüksek kaliteli render için daha büyük texture
            int renderSize = previewSize * 2; // 2x supersampling
            RenderTexture renderTexture = new RenderTexture(renderSize, renderSize, 0, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 8; // Daha yüksek anti-aliasing
            
            previewCamera.targetTexture = renderTexture;
            
            // Render et
            previewCamera.Render();
            
            // Texture2D'ye çevir
            RenderTexture.active = renderTexture;
            Texture2D highResTexture = new Texture2D(renderSize, renderSize, TextureFormat.RGBA32, false);
            highResTexture.ReadPixels(new Rect(0, 0, renderSize, renderSize), 0, 0);
            highResTexture.Apply();
            
            // Downscale to final size with better quality
            Texture2D finalTexture = ScaleTexture(highResTexture, previewSize, previewSize);
            
            // Alpha channel'ı kontrol et ve temizle
            CleanAlphaChannel(finalTexture);
            
            // Cleanup high res texture
            DestroyImmediate(highResTexture);
            
            // Cleanup render texture
            RenderTexture.active = null;
            previewCamera.targetTexture = null;
            renderTexture.Release();
            
            // PNG olarak kaydet
            byte[] pngData = finalTexture.EncodeToPNG();
            string fileName = $"{prefab.name}_Preview.png";
            string filePath = Path.Combine("Assets/PreviewSprites", fileName);
            File.WriteAllBytes(filePath, pngData);
            
            // Final texture'ı temizle
            DestroyImmediate(finalTexture);
            
            // Asset'i import et
            AssetDatabase.ImportAsset(filePath);
            
            // Texture import ayarlarını sprite olarak ayarla - alpha transparency ile
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed; // En yüksek kalite
                importer.SaveAndReimport();
            }
            
            // Sprite'ı yükle ve döndür
            return AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
        }
        finally
        {
            // Geçici objeleri temizle
            DestroyImmediate(previewRoot);
            
            // Kamera layer'ını sıfırla
            previewCamera.cullingMask = -1;
        }
    }
    
    // Recursive layer setting
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    // Alpha channel'ı temizle
    private void CleanAlphaChannel(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Eğer pixel tamamen şeffafsa (alpha = 0), color'ları da sıfırla
            if (pixels[i].a < 0.01f)
            {
                pixels[i] = Color.clear;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }
    
    // Texture scaling utility
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;
        
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
    
    private Bounds CalculateShapeBounds(GameObject shape)
    {
        Renderer[] renderers = shape.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
            return new Bounds();
        
        Bounds bounds = renderers[0].bounds;
        
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        
        return bounds;
    }
}
