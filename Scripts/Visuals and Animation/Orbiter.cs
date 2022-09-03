using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbiter : MonoBehaviour
{
    [SerializeField] private float offset = 5.0f;
    [SerializeField] private float speed = 5;
    private float zpos = 0;
    private float ypos = 0;
    private float zdir = 1; 
    private float ydir = 1;

    void Start()
    {
        ypos = offset; //start at the top

        Vector3 startPosition = transform.position;
        startPosition.y += offset;
        transform.localPosition = startPosition;
    }

    void FixedUpdate()
    {
        if ((Mathf.Abs(zpos) + Time.deltaTime) > offset)
            zdir *= -1;
        if ((Mathf.Abs(ypos) + Time.deltaTime) > offset)
            ydir *= -1;
        Vector3 newPosition = transform.position;
        ypos += (Time.deltaTime * ydir * speed);
        zpos += (Time.deltaTime * zdir * speed);
        newPosition.y = ypos;
        newPosition.z = zpos;
        transform.localPosition = newPosition;
    }
}
