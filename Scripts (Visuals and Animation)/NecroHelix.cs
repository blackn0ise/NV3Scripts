using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroHelix : MonoBehaviour
{
    private Transform parent;
    private Vector3 offset;
    [SerializeField] private float spinrate = 40.0f;
    [SerializeField] private float spindir = -1;


    private void Start()
    {
        Initialise();
    }

    void Update()
    {
        Rotate();
        UpdateRelative();
    }

    private void Rotate()
    {
        transform.Rotate(0, spindir * spinrate * Time.deltaTime, 0, Space.Self);
        Vector3 rotation = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(rotation);
    }

    private void UpdateRelative()
    {
        if (parent)
        {
            transform.position = parent.position + offset;
        }
        else
            Destroy(gameObject);
    }

    private void Initialise()
    {
        parent = transform.parent;
        offset = transform.position - parent.position;
        offset.x = 0;
        offset.z = 0;
        transform.parent = transform.parent.parent;
    }
}
