using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactJudgment : MonoBehaviour
{
    [SerializeField] MoveTest m_moveTest = null;

    private void OnTriggerEnter(Collider other)
    {
        m_moveTest.Toucing(other.name);
    }
}
