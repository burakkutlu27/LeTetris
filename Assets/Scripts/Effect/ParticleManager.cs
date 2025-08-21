using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem[] allEffects;
    private void Start()
    {
        allEffects = GetComponentsInChildren<ParticleSystem>();
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
