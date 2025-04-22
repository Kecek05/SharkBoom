using Sortify;
using System;
using UnityEngine;

public class PlayerFlipGfx : MonoBehaviour
{
    public void HandleOnRotationChanged(bool isRight)
    {
        if(isRight)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, -90f, transform.eulerAngles.z); //still need to rotate
        else
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 90f, transform.eulerAngles.z);
    }
}
