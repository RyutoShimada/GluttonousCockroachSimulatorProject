using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Rigidbody必須</summary>
[RequireComponent(typeof(Rigidbody))]

/// <summary>動くものはこのクラスを継承する</summary>
public class MoveBass : MonoBehaviour
{
    /// <summary>速度</summary>
    [SerializeField] float m_speed = 5f;
    Rigidbody m_rb;
    Vector3 m_velocity;
    Vector3 m_direction;
    Vector2 m_inputDirection;
    /// <summary>落下速度</summary>
    float m_velocityY;

    private void Awake()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {

    }

    /// <summary>移動の処理</summary>
    public virtual void Move()
    {
        m_inputDirection.x = Input.GetAxisRaw("Horizontal");
        m_inputDirection.y = Input.GetAxisRaw("Vertical");
        m_direction = new Vector3(m_inputDirection.x, 0, m_inputDirection.y);//方向を変換

        if (m_inputDirection != Vector2.zero) //入力されている時
        {
            transform.forward = m_direction; //向きを保存
            m_velocity = new Vector3(m_inputDirection.x, 0, m_inputDirection.y);//方向を入力
            m_velocityY = m_rb.velocity.y;//Y軸速度を保存
            m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
            m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
        }
        else
        {
            m_velocity = new Vector3(0, m_rb.velocity.y, 0);
            m_rb.velocity = m_velocity; //ピタッと止まるようにする
        }
    }
}
