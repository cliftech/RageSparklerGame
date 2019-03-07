using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageContainer : MonoBehaviour
{
    private Func<float> damageFunc;
    private Func<bool> doKnockbackFunc;
    public void SetDamageCall(Func<float> damageFunc)
    {
        this.damageFunc = damageFunc;
    }

    public void SetDoKnockbackCall(Func<bool> doKnockbackFunc)
    {
        this.doKnockbackFunc = doKnockbackFunc;
    }

    public float GetDamage()
    {
        return damageFunc.Invoke();
    }

    public bool doKnockback()
    {
        return doKnockbackFunc.Invoke();
    }
}
