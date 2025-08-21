using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    private int lines;
    private int level = 1;

    public int linesInLevel = 5;

    private int minline = 1;
    private int maxline = 4;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI linesText;
    public TextMeshProUGUI levelText;
    public bool isLevelPassed = false;
    public event System.Action<int> OnLevelUp; // yeni level değerini gönderecek
    
    private void Start()
    {
        // Lines değerini başlangıç değeri ile initialize et
        lines = linesInLevel;
        UpdateText();
    }

    public void LineScore(int n)
    {
        isLevelPassed = false;
        n = Mathf.Clamp(n, minline, maxline);

        switch (n)
        {
            case 1:
                score += 30 * level;
                break;
            case 2:
                score += 50 * level;
                break;
            case 3:
                score += 150 * level;
                break;
            case 4:
                score += 500 * level;
                break;

        }
        lines -= n;
        if (lines <= 0)
        {
            LevelUp();
        }
        // UI güncelle
        if (scoreText != null) scoreText.text = score.ToString();
        if (linesText != null) linesText.text = lines.ToString();
        if (levelText != null) levelText.text = level.ToString();
    }

    public void LevelUp()
    {
        level++;
        lines = linesInLevel * level;
        isLevelPassed = true;
        SoundManager.instance.PlaySFX(2);
        OnLevelUp?.Invoke(level);
    }

    public void UpdateText()
    {
        if (scoreText != null) scoreText.text = score.ToString();
        if (linesText != null) linesText.text = lines.ToString();
        if (levelText != null) levelText.text = level.ToString();
    }
}