using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public int speed;
    void Update()
    {
        transform.Rotate(0, 0, -0.1f * speed * Time.deltaTime);
    }
}
