using UnityEngine;
using UnityEditor;
using System.IO;

public class PreviewSpriteAssigner : EditorWindow
{
    [MenuItem("Tools/Assign Preview Sprites")]
    public static void ShowWindow()
    {
        GetWindow<PreviewSpriteAssigner>("Preview Sprite Assigner");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Preview Sprite Auto Assigner", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Bu tool tüm shape prefablarına otomatik preview sprite atar.");
        GUILayout.Label("Her shape'in SpriteRenderer'ındaki sprite'ı preview olarak kullanır.");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Assign Preview Sprites"))
        {
            AssignPreviewSprites();
        }
    }
    
    private void AssignPreviewSprites()
    {
        // Scripts klasöründeki tüm prefabları bul
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        
        int assigned = 0;
        
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
                    // SpriteRenderer'ı bul
                    SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                        spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
                    
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        // SerializedObject kullanarak preview sprite'ı ata
                        SerializedObject serializedObject = new SerializedObject(shapeManager);
                        SerializedProperty previewSpriteProperty = serializedObject.FindProperty("previewSprite");
                        
                        if (previewSpriteProperty != null)
                        {
                            previewSpriteProperty.objectReferenceValue = spriteRenderer.sprite;
                            serializedObject.ApplyModifiedProperties();
                            assigned++;
                            
                            Debug.Log($"Preview sprite assigned to {prefab.name}");
                        }
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Complete", $"Preview sprites assigned to {assigned} shapes!", "OK");
    }
}
