using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageContainer : MonoBehaviour
{
    private Func<float> damageFunc;

    public void SetDamageCall(Func<float> damageFunc)
    {
        this.damageFunc = damageFunc;
    }

    public float GetDamage()
    {
        return damageFunc.Invoke();
    }
}
