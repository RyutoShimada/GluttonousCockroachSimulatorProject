using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveTest : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    [SerializeField] float m_turnSpeed = 5;
    [SerializeField] float m_gravityPower = 1f;
    [SerializeField] float m_jumpPower = 1f;
    [SerializeField] Transform m_rayOrigin = null;
    [SerializeField] CinemachineVirtualCamera m_vcam = null;
    Rigidbody m_rb;
    Vector3 m_gravityDir;
    Vector3 m_velocity;
    Vector3 m_direction;
    /// <summary>Rayを飛ばす距離</summary>
    const float m_rayDis = 0.19f;

    Ray m_ray;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        m_rb = this.gameObject.GetComponent<Rigidbody>();
        m_gravityDir = Vector3.down;
        m_ray = new Ray(m_rayOrigin.position, transform.forward + Vector3.down);
    }

    // Update is called once per frame
    void Update()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void Move(float h, float v)
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);//重力

        m_direction = transform.forward;

        if (v > 0)
        {
            m_velocity = m_direction.normalized * m_speed;
            m_velocity.y = m_rb.velocity.y;
            m_rb.velocity = m_velocity;
        }
        else if (v <= 0)
        {
            m_velocity = new Vector3(0, m_rb.velocity.y, 0);//ピタッと止まるようにする
            m_rb.velocity = m_velocity;
        }

        if (h != 0)
        {
            transform.Rotate(new Vector3(0f, h * m_turnSpeed, 0f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }
}
