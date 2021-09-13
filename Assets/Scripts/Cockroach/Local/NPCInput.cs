using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInput : MonoBehaviour
{
    [SerializeField] CockroachMoveController m_moveController = null;
    [SerializeField] float m_moveTimeMin = 0.5f;
    [SerializeField] float m_moveTimeMax = 5f;
    [SerializeField] float m_waitTimeMax = 2f;
    [SerializeField] float m_rotateTimeMin = -10;
    [SerializeField] float m_rotateTimeMax = 10f;
    [SerializeField] float m_minJumpTime = 1f;
    [SerializeField] float m_maxJumpTime = 5f;
    float m_moveTimer;
    float m_jumpTimer;
    void Start()
    {
        m_moveController.StartSet();
        m_jumpTimer = Random.Range(m_minJumpTime, m_maxJumpTime);
        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        int random;

        while (!m_moveController.IsDed && m_moveController.IsCanMove)
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
        m_moveTimer = Random.Range(m_moveTimeMin, m_moveTimeMax);
        while (m_moveTimer > 0)
        {
            m_jumpTimer -= Time.deltaTime;
            if (m_jumpTimer < 0)
            {
                m_jumpTimer = Random.Range(m_minJumpTime, m_maxJumpTime);
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
        m_moveTimer = Random.Range(m_moveTimeMin, m_moveTimeMax);
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
        float waitTime = Random.Range(0, m_waitTimeMax);
        yield return new WaitForSeconds(waitTime);
        waitTime = Random.Range(m_rotateTimeMin, m_rotateTimeMax);
        m_moveController.MouseMove(waitTime);
    }

    void Rotating()
    {
        float rotate = Random.Range(m_rotateTimeMin, m_rotateTimeMax);
        m_moveController.MouseMove(rotate);
    }

    void Jump()
    {
        m_moveController.Jump(true);
    }
}
