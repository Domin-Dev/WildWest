using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCEyesight : MonoBehaviour
{
    NPCController controller;
    private void Start()
    {
        controller = transform.parent.GetComponent<NPCController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != null && collision.transform.parent.tag == "Player")
        {
            controller.SetTarget(collision.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.parent != null && collision.transform.parent.tag == "Player")
        {
            controller.SetTarget(null);
        }
    }
}
