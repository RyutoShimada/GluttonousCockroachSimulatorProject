using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGround : MonoBehaviour
{
    [Tooltip("CockroachMoveController がアタッチされているオブジェクトをアサインする")]
    [SerializeField] CockroachMoveControllerNetWork m_parent = null;

    // Start is called before the first frame update
    void Start()
    {
        m_parent = transform.parent.gameObject.GetComponent<CockroachMoveControllerNetWork>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(false);
        }
    }
}
