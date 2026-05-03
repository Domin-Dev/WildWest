using NUnit.Framework;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{


    public void Update()
    {
        Vector2 position = Input.mousePosition;
        transform.position = position;
    }
}
