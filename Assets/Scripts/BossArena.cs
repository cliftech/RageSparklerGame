using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossArena : MonoBehaviour
{
    private Action bossTriggerAction;

    public void Set(Action bossTriggerAction)
    {
        this.bossTriggerAction = bossTriggerAction;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossTriggerAction.Invoke();
            gameObject.SetActive(false);
        }
    }
}
