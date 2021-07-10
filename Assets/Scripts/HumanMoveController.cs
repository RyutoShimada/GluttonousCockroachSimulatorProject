using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class HumanMoveController : MonoBehaviour
{
    [SerializeField] float m_moveSpeed = 1f;
    [SerializeField] float m_turnSpeed = 1f;
    Rigidbody m_rb;
    Vector2 m_input;
    Vector3 m_dir;
    Vector3 m_vel;
    Animator m_anim;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");

        // とりあえずのアニメーション
        m_anim.SetFloat("Speed", Mathf.Abs(m_rb.velocity.x));
        m_anim.SetFloat("Speed", Mathf.Abs(m_rb.velocity.z));

        this.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, this.transform.rotation, m_turnSpeed * Time.deltaTime); //Playerの向きをカメラの向いている方向にする
        this.transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);　//Playerが倒れないようにする
    }

    void Move()
    {
        m_dir = Vector3.forward * m_input.y + Vector3.right * m_input.x;

        if (m_input != Vector2.zero)
        {
            //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
            m_dir = Camera.main.transform.TransformDirection(m_dir);
            m_dir.y = 0;//y軸方向はゼロにして水平方向のベクトルにする

            m_vel = m_dir.normalized * m_moveSpeed;
            m_vel.y = m_rb.velocity.y;
            m_rb.velocity = m_vel;
        }
        else
        {
            m_rb.velocity = Vector3.zero;
        }
    }
}
