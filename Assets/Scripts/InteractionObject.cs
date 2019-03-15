using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionObject : MonoBehaviour
{
    //Class for setting objects type/settings

    public bool collectable; //if true, can be added to inventory
    public bool talks;
    public bool openable;
    public string itemType;

    public void Talk()
    {
        print("Hi!");
    }
}
