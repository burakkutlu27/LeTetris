using UnityEngine;
using UnityEngine.UI;

public class RotateManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button rotateButton;

    private void Start()
    {
        if (rotateButton != null && gameManager != null)
        {
            rotateButton.onClick.AddListener(OnRotateButtonPressed);
        }
    }

    private void OnRotateButtonPressed()
    {
        gameManager.RotateShape();
    }
}
