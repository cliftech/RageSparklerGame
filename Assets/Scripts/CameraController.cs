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
    private Vector3 ExtraOffset;
    private float MaxExtra = 5f;


    // Start is called before the first frame update
    void Start()
    {
        Offset = transform.position - PlayerCharacter.transform.position;
        ExtraOffset = new Vector3(0, 0, 0);
    }

    void LateUpdate()
    {
        
        if (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") > 0)
        {
            if (ExtraOffset.x < MaxExtra)
                ExtraOffset.x += 0.1f;
        }
        else if (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") < 0)
        {
            if (ExtraOffset.x > -MaxExtra)
                ExtraOffset.x -= 0.1f;
        }
        else if (ExtraOffset.x > 0.002f || ExtraOffset.x < -0.002f )
        {
                ExtraOffset.x += ExtraOffset.x > 0f? -0.1f:0.1f;
        }
        var position = PlayerCharacter.transform.position + Offset;
        var Camera = GetComponent<Camera>();
        position += ExtraOffset;
        position = new Vector3
        (
            Mathf.Clamp(position.x, LeftBound.transform.position.x + Camera.orthographicSize * Camera.aspect, RightBound.transform.position.x - Camera.orthographicSize * Camera.aspect),
            Mathf.Clamp(position.y, BottomBound.transform.position.y + Camera.orthographicSize, TopBound.transform.position.y - Camera.orthographicSize),
            position.z);
        transform.position = position;
    }
}
