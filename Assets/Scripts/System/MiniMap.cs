using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [HideInInspector] public GameObject m_player;

    void Update()
    {
        if (NetWorkGameManager.Instance == null) return;
        if (m_player == null) return;
        ChangePosition();
        ChangeForward();
    }

    void ChangePosition()
    {
        var pos = m_player.transform.position;
        pos.y = transform.position.y;
        transform.position = pos;
    }

    void ChangeForward()
    {
        if (m_player.transform.up != Vector3.up) return;
        transform.root.forward = m_player.transform.forward;
    }
}
