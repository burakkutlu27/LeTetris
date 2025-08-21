using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class IntroManager : MonoBehaviour
{
    public GameObject[] numbers;
    public GameObject numbersTransform;
    
    [Header("Objects to activate after intro")]
    public GameObject gameCanvas;  // Oyun UI Canvas'ı

    private void Start()
    {
        // Intro başlangıcında sadece Canvas'ı deaktif et
        if (gameCanvas != null) gameCanvas.SetActive(false);
        
        StartCoroutine(AnimateNumbers());
    }
    IEnumerator AnimateNumbers()
    {
        yield return new WaitForSeconds(.1f);
        numbersTransform.GetComponent<RectTransform>().DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
        numbersTransform.GetComponent<CanvasGroup>().DOFade(1f, 0.3f);
        yield return new WaitForSeconds(0.2f);
        int counter = 0;
        while (counter < numbers.Length)
        {
            numbers[counter].GetComponent<RectTransform>().DOLocalMoveY(0, 0.5f);
            numbers[counter].GetComponent<CanvasGroup>().DOFade(1f, .5f);
            numbers[counter].GetComponent<RectTransform>().DOScale(2f, .3f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                numbers[counter].GetComponent<RectTransform>().DOScale(1f, 0.2f).SetEase(Ease.InBack);
            });
            yield return new WaitForSeconds(1f);
            counter++;
            numbers[counter - 1].GetComponent<RectTransform>().DOLocalMoveY(150f, 0.5f);
            yield return new WaitForSeconds(0.2f);
        }
        numbersTransform.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            numbersTransform.transform.parent.gameObject.SetActive(false);
            
            // Intro bittikten sonra Canvas'ı aktif et
            if (gameCanvas != null) gameCanvas.SetActive(true);
            
            // GameManager'ı bul ve oyunu başlat
            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StartGame();
                Debug.Log("Intro completed! Game Canvas activated and GameManager started.");
            }
            else
            {
                Debug.LogError("GameManager not found!");
            }
        });
    }
}
