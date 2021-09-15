using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] CockroachScriptableObject m_data = null;
    [SerializeField] CockroachMoveController m_moveController = null;

    float m_moveTimer;
    float m_jumpTimer;
    void Start()
    {
        m_moveController.StartSet();
        m_jumpTimer = Random.Range(m_data.MinJumpTime, m_data.MaxJumpTime);
        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        int random;

        while (!m_moveController.IsDed && m_moveController.m_canMove)
        {
            random = Random.Range(0, 2);

            if (random > 0)
            {
                yield return Move();
            }
            else
            {
                yield return MeandelingMove();
            }

            yield return Rotate();
        }
    }

    IEnumerator Move()
    {
        m_moveTimer = Random.Range(m_data.MinMoveTime, m_data.MaxMoveTime);
        while (m_moveTimer > 0)
        {
            m_jumpTimer -= Time.deltaTime;
            if (m_jumpTimer < 0)
            {
                m_jumpTimer = Random.Range(m_data.MinJumpTime, m_data.MaxJumpTime);
                yield return Rotate();
                m_moveController.Move(1f);
                Jump();
            }
            m_moveController.Move(1f);
            m_moveTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator MeandelingMove()
    {
        m_moveTimer = Random.Range(m_data.MinMoveTime, m_data.MaxMoveTime);
        m_jumpTimer = 0.3f;

        while (m_moveTimer > 0)
        {
            m_jumpTimer -= Time.deltaTime;

            if (m_jumpTimer < 0)
            {
                m_jumpTimer = 0.3f;
                Rotating();
            }

            m_moveController.Move(1f);
            m_moveTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator Rotate()
    {
        float waitTime = Random.Range(0, m_data.AftorMoveWaitTime);
        yield return new WaitForSeconds(waitTime);
        waitTime = Random.Range(m_data.LeftRotateRange, m_data.RightRotateRange);
        m_moveController.MouseMove(waitTime, waitTime);
    }

    void Rotating()
    {
        float rotate = Random.Range(m_data.LeftRotateRange, m_data.RightRotateRange);
        m_moveController.MouseMove(rotate, rotate);
    }

    void Jump()
    {
        m_moveController.Jump(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Attack")
        {
            gameObject.SetActive(false);
        }
    }
}
