using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGround : MonoBehaviour
{
    CockroachMoveController _parent = null;

    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.parent.gameObject.GetComponent<CockroachMoveController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            _parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            _parent.IsGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            _parent.IsGround(false);
        }
    }
}
