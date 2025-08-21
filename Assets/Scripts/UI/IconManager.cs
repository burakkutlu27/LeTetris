using UnityEngine;
using UnityEngine.UI;
public class IconManager : MonoBehaviour
{
    public Sprite openIcon;
    public Sprite closeIcon;

    private Image iconImage;
    public bool defaultIcon = true;

    private void Start()
    {
        iconImage = GetComponent<Image>();
        iconImage.sprite = defaultIcon ? openIcon : closeIcon;
    }

    public void ToggleIcon(bool defaultIcon)
{
    if (!iconImage || !openIcon || !closeIcon)
    {
        Debug.LogWarning("IconManager: Eksik referans var!");
        return;
    }

    iconImage.sprite = defaultIcon ? openIcon : closeIcon;
    Debug.Log($"Icon değişti → {(defaultIcon ? "Open" : "Close")}, yeni sprite: {iconImage.sprite.name}");
}

}
