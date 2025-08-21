using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem[] allEffects;
    private void Start()
    {
        allEffects = GetComponentsInChildren<ParticleSystem>();
        
        // Başlangıçta tüm efektleri durdur (Play On Awake durumunda bile)
        foreach (ParticleSystem effect in allEffects)
        {
            effect.Stop();
        }
    }

    public void PlayEffect()
    {
        foreach (ParticleSystem effect in allEffects)
        {
            effect.Stop();
            effect.Play();
        }
    }
}
