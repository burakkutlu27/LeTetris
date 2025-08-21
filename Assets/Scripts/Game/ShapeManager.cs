using System.Collections;
using System.Data.Common;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    [SerializeField] private bool isTurn = true;
    
    [Header("Preview Settings")]
    [SerializeField] private Sprite previewSprite; // UI'da gösterilecek preview sprite
    
    public Sprite PreviewSprite => previewSprite;

    private void Start()
    {
        // MoveRoutine GameManager tarafından kontrol edilecek, burada başlatmıyoruz
    }
    public void MoveLeft()
    {
        transform.Translate(Vector3.left, Space.World);
    }
    public void MoveRight()
    {
        transform.Translate(Vector3.right, Space.World);
    }
    public void MoveDown()
    {
        transform.Translate(Vector3.down, Space.World);
    }
    public void MoveUp()
    {
        transform.Translate(Vector3.up, Space.World);
    }

    public void TurnRight()
    {
        if (isTurn)
        {
            transform.Rotate(0, 0, -90);
        }
    }
    public void TurnLeft()
    {
        if (isTurn)
        {
            transform.Rotate(0, 0, 90);
        }
    }

    public void isTurnClockWise(bool CloksWise)
    {
        if (CloksWise)
        {
            TurnRight();
        }
        else
        {
            TurnLeft();
        }
    }
}
