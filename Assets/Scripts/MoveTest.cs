using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveTest : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    [SerializeField] float m_jumpPower = 1f;
    [SerializeField] float m_turnSpeed = 5;
    [SerializeField] float m_gravityPower = 1f;
    Rigidbody m_rb;
    Vector3 m_gravityDir;
    Vector3 m_velocity;
    Vector3 m_direction;
    RaycastHit m_hit;
    /// <summary>Rayを飛ばす距離</summary>
    const float m_rayDis = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        m_rb = this.gameObject.GetComponent<Rigidbody>();
        m_gravityDir = Vector3.down;
        m_hit = new RaycastHit();
    }

    // Update is called once per frame
    void Update()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Physics.Raycast(transform.position, m_direction, out m_hit);
        Jump(m_jumpPower);
    }

    void Move(float h, float v)
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);//重力

        m_direction = transform.forward;

        if (v > 0)
        {
            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                m_velocity = m_direction.normalized * m_speed;
                m_velocity.y = m_rb.velocity.y;
            }
            else if (m_gravityDir == Vector3.left || m_gravityDir == Vector3.right)
            {
                m_velocity = m_direction.normalized * m_speed;
            }
            else if (m_gravityDir == Vector3.forward || m_gravityDir == Vector3.back)
            {
                m_velocity = m_direction.normalized * m_speed;
            }

            m_rb.velocity = m_velocity;
        }
        else if (v <= 0)
        {
            //ピタッと止まるようにする
            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                m_velocity = new Vector3(0, m_rb.velocity.y, 0);
            }
            else
            {
                m_velocity = Vector3.zero;
            }

            m_rb.velocity = m_velocity;
        }

        if (h != 0)
        {
            transform.Rotate(new Vector3(0f, h * m_turnSpeed, 0f));
        }
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && Physics.Raycast(this.transform.position, m_gravityDir, m_rayDis))
        {
            m_rb.AddForce(-m_gravityDir.normalized * jumpPower, ForceMode.Impulse);
            Fall();
        }
    }

    void Fall()
    {
        m_gravityDir = Vector3.down;
    }

    void ChangeGravity()
    {
        m_gravityDir = -m_hit.normal;
        ChangeRotate(m_hit.normal);
    }

    void ChangeRotate(Vector3 nomal)
    {
        //https://teratail.com/questions/290578
        Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
        transform.rotation = toRotate;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!m_hit.collider) return;
        if (m_hit.collider.name == collision.collider.name)
        {
            //Debug.Log(true);
            ChangeGravity();
        }
        else
        {
            //Debug.Log(false);
            
        }

        if (collision.collider.name == "Floor")
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = toRotate;
        }

        //Debug.Log($"Enter : {collision.gameObject.name}");
        //Debug.Log($"name : { m_hit.collider.gameObject.name }");
    }

    private void OnCollisionExit(Collision collision)
    {
        m_hit = new RaycastHit();
    }
}
