using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryColliderBehaviour : MonoBehaviour
{
    public Collider otherCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        otherCollider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == otherCollider)
        {
            otherCollider = null;
        }
    }
}
