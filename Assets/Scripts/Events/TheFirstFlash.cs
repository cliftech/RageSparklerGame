using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheFirstFlash : MonoBehaviour
{
    private SpriteRenderer AmuletFlash;
    public Transform LevelEnd;
    private bool IsFlashing;
    private float MaxDistance;

    private void Awake()
    {
        AmuletFlash = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!IsFlashing)
            return;
        Color c = AmuletFlash.color;
        float Distance = Vector2.Distance(transform.position, LevelEnd.position);
        c.a = Mathf.MoveTowards(c.a, (MaxDistance/Distance)/10, Time.deltaTime);
        AmuletFlash.color = c;
    }

    public void StartFlashing()
    {
        IsFlashing = true;
        MaxDistance = Vector2.Distance(transform.position, LevelEnd.position);
    }

    public void SetAmuletFlash(bool doesFlash, Transform levelEnd)
    {
        IsFlashing = false;
        this.LevelEnd = levelEnd;
        Color c = AmuletFlash.color;
        c.a = 0;
        AmuletFlash.color = c;
    }
}
