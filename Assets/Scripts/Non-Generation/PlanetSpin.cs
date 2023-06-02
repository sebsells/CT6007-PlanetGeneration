using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
    [SerializeField] float spinSpeed = 2.0f;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0.0f, spinSpeed * Time.time, 0.0f);
    }
}
