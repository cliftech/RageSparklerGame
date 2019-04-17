using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossArena : MonoBehaviour
{
    private Action bossTriggerAction;
    [Header("an animator with a trigger 'Close' and 'Open'")]
    public Animator gate;
    public Animator gate2;

    public void Set(Action bossTriggerAction)
    {
        this.bossTriggerAction = bossTriggerAction;
    }

    public void OpenGate1()
    {
        if (gate != null)
        {
            gate.SetTrigger("Open");
        }
    }
    public void OpenGate2()
    {
        if (gate2 != null)
        {
            gate2.SetTrigger("Open");
        }
    }
    public void CloseGate1()
    {
        if (gate != null)
        {
            gate.SetTrigger("Close");
        }
    }
    public void CloseGate2()
    {
        if (gate2 != null)
        {
            gate2.SetTrigger("Close");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossTriggerAction.Invoke();
            gameObject.SetActive(false);
            CloseGate1();
            CloseGate2();
        }
    }
}
