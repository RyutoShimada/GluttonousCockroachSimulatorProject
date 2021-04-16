using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachMoveController : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    Rigidbody m_rb;
    float m_h;
    float m_v;
    Vector3 m_dir;
    Vector3 m_vel;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_h = Input.GetAxisRaw("Horizontal");
        m_v = Input.GetAxisRaw("Vertical");
        if (m_h != 0 || m_v != 0)
        {
            m_dir.x = m_h;
            m_dir.z = m_v;
            transform.forward = m_dir;
        }
        
        m_vel = m_rb.velocity;
        m_vel.x = m_h;
        m_vel.y = 0;
        m_vel.z = m_v;
        m_rb.velocity = m_vel.normalized * m_speed;
    }
}
