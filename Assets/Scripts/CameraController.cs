using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject PlayerCharacter;
    //Bounds are to be set by the maker of the area.
    public GameObject LeftBound;
    public GameObject TopBound;
    public GameObject RightBound;
    public GameObject BottomBound;

    private Vector3 Offset;


    // Start is called before the first frame update
    void Start()
    {
        Offset = transform.position - PlayerCharacter.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var position = PlayerCharacter.transform.position + Offset;
        var Camera = GetComponent<Camera>();
        position = new Vector3
        (
            Mathf.Clamp(position.x, LeftBound.transform.position.x + Camera.orthographicSize * Camera.aspect, RightBound.transform.position.x - Camera.orthographicSize * Camera.aspect),
            Mathf.Clamp(position.y, BottomBound.transform.position.y + Camera.orthographicSize, TopBound.transform.position.y - Camera.orthographicSize),
            position.z);
        transform.position = position;
    }
}
