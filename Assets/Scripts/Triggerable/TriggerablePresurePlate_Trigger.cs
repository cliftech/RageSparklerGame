using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerablePresurePlate_Trigger : MonoBehaviour
{
    private TriggerablePresurePlate plate;

    private void Awake()
    {
        plate = transform.parent.GetComponent<TriggerablePresurePlate>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        plate.TriggerAnimation(other);
    }
}
