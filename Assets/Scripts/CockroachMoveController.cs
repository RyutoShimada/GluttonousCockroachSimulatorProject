using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachMoveController : MoveBass
{
    [SerializeField] float m_jumpPower = 1f;
    [SerializeField] bool m_isWall = false;

    /// <summary>Rigidbodyが無効かどうか</summary>
    bool m_unRb;


    void Start()
    {
        m_unRb = this.gameObject.GetComponent<Rigidbody>().isKinematic;
    }

    void Update()
    {
        Move();
        base.Jump(m_jumpPower);
    }

    public override void Move()
    {
        if (m_isWall)
        {

        }
        else
        {
            base.Move();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //壁や天井に接触した時
        m_unRb = true;
    }

    enum Wall
    {
        North,
        South,
        West,
        East
    }
}
