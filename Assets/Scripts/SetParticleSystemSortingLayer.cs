using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticleSystemSortingLayer : MonoBehaviour
{
    public string sortingLayer;
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
    }
}
