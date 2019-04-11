using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LethalWater : MonoBehaviour
{
    float t;
    public float timeToTop;
    Vector3 startPosition;
    Vector3 target;
    float timeToReachTarget;
    public GameObject destination;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = target = transform.position;
        SetDestination(destination.transform.position, timeToTop);
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, target, t);
    }
    public void SetDestination(Vector3 destination, float time)
    {
        t = 0;
        startPosition = transform.position;
        timeToReachTarget = time;
        target = destination;
    }
}
