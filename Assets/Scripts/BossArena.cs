using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossArena : MonoBehaviour
{
    private Action bossTriggerAction;
    [Header("an animator with a trigger 'Close' and 'Open'")]
    public Animator gate;

    public void Set(Action bossTriggerAction)
    {
        this.bossTriggerAction = bossTriggerAction;
    }

    public void OpenGate()
    {
        if (gate != null)
        {
            gate.SetTrigger("Open");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossTriggerAction.Invoke();
            gameObject.SetActive(false);
            if (gate != null)
            {
                gate.SetTrigger("Close");
            }
        }
    }
}
