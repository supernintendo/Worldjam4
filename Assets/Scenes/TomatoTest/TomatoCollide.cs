using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TomatoCollide : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("splat");
    }
}
