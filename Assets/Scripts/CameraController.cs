using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Player PlayerCharacter;
    //Bounds are to be set by the maker of the area.
    private Transform LeftBound;
    private Transform TopBound;
    private Transform RightBound;
    private Transform BottomBound;

    private Vector3 Offset;
    private Vector3 ExtraOffset;
    private float MaxExtra = 2.5f;
    private bool isDisabled;
    Camera Camera;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerCharacter = FindObjectOfType<Player>();
        Offset = transform.position - PlayerCharacter.transform.position;
        ExtraOffset = new Vector3(0, 0, 0);
        Camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!PlayerCharacter.isDead && !isDisabled)
        {
            if (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") > 0)
            {
                if (ExtraOffset.x < MaxExtra)
                    ExtraOffset.x += 10f * Time.deltaTime;
            }
            else if (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") < 0)
            {
                if (ExtraOffset.x > -MaxExtra)
                    ExtraOffset.x -= 10f * Time.deltaTime;
            }
        }
        var position = PlayerCharacter.transform.position + Offset;
        position += ExtraOffset;
        position = new Vector3
        (
            Mathf.Clamp(position.x, LeftBound.position.x + Camera.orthographicSize * Camera.aspect, RightBound.position.x - Camera.orthographicSize * Camera.aspect),
            Mathf.Clamp(position.y, BottomBound.position.y + Camera.orthographicSize, TopBound.position.y - Camera.orthographicSize),
            position.z);
        transform.position = position;
    }

    public void SetEnabled(bool enabled)
    {
        isDisabled = !enabled;
    }

    public void SetBounds(Transform LeftBound, Transform TopBound, Transform RightBound, Transform BottomBound)
    {
        this.LeftBound = LeftBound;
        this.TopBound = TopBound;
        this.RightBound = RightBound;
        this.BottomBound = BottomBound;
    }
}
