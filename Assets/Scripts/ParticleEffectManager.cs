using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    private static ParticleEffectManager instance;
    public List<ParticleEffect> particleEffects;

    public void Start()
    {
        instance = this;
        foreach (var p in particleEffects)
        {
            p.particleSystem = Instantiate(p.prefab, transform).GetComponent<ParticleSystem>();
            p.particleSystem.Stop();
        }
    }

    public static void PlayEffect(ParticleEffect.Type type, Vector3 position, Vector3 direction)
    {
        instance.Play(type, position, direction);
    }

    public void Play(ParticleEffect.Type type, Vector3 position, Vector3 direction)
    {
        foreach (var p in particleEffects)
            if (p.type == type)
                Play(p.particleSystem, position, direction);
    }

    private void Play(ParticleSystem particleSystem, Vector3 position, Vector3 direction)
    {
        particleSystem.transform.position = position;
        if (direction == Vector3.zero)
            direction = Vector3.back;
        particleSystem.transform.forward = direction;
        particleSystem.Play();

    }
}

[System.Serializable]
public class ParticleEffect
{
    public GameObject prefab;
    [HideInInspector] public ParticleSystem particleSystem;
    public enum Type { blood }
    public Type type;
}
