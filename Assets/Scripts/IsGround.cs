using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGround : MonoBehaviour
{
    MoveTest _parent = null;

    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.parent.gameObject.GetComponent<MoveTest>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            _parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player")
        {
            _parent.IsGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
        {
            _parent.IsGround(false);
        }
    }
}
