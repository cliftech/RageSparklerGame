using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public string title = "enter title";
    public List<Portal> checkPoints;
    public AudioClip backgroundMusic;

    public Transform LeftBound;
    public Transform TopBound;
    public Transform RightBound;
    public Transform BottomBound;

    public Transform spawnPoint;
    public Transform LevelEnd;
    public bool DoesAmuletFlash;
}
