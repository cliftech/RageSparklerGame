using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public string title = "enter title";
    public List<Portal> checkPoints;

    public Transform LeftBound;
    public Transform TopBound;
    public Transform RightBound;
    public Transform BottomBound;

    public Transform spawnPoint;
}
