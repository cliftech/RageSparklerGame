using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerablePistonCollisionEventSender : MonoBehaviour
{
    private TriggerablePiston piston;

    void Awake()
    {
        piston = GetComponentInParent<TriggerablePiston>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        piston.OnCollisionStay2D_Triggered(collision);
    }
}
