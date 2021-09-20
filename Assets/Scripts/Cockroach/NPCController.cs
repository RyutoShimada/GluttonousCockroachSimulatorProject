using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NPCController : MonoBehaviour
{
    public static Action Ded;
    [SerializeField] CockroachScriptableObject m_data = null;
    [SerializeField] CockroachMoveController m_moveController = null;
    [SerializeField] GameObject m_dedEffect = null;
    [SerializeField] AudioClip m_clip = null;
    CockroachChildPoolManager m_poolManager = null;
    float m_moveTimer;
    float m_jumpTimer;
    bool m_isDed = false;

    bool Active
    {
        get => gameObject.activeSelf;
        set
        {
            gameObject.SetActive(value);
        }
    }

    void Start()
    {
        m_poolManager = transform.GetComponentInParent<CockroachChildPoolManager>();
        m_moveController.StartSetNpc();
        m_jumpTimer = UnityEngine.Random.Range(m_data.MinJumpTime, m_data.MaxJumpTime);
        StartCoroutine(Routine());
    }

    private void OnEnable()
    {
        if (m_isDed)
        {
            m_isDed = false;
            StartCoroutine(Routine());
        }
    }

    IEnumerator Routine()
    {
        int random;

        while (!m_isDed)
        {
            random = UnityEngine.Random.Range(0, 2);

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
        m_moveTimer = UnityEngine.Random.Range(m_data.MinMoveTime, m_data.MaxMoveTime);
        while (m_moveTimer > 0)
        {
            m_jumpTimer -= Time.deltaTime;
            if (m_jumpTimer < 0)
            {
                m_jumpTimer = UnityEngine.Random.Range(m_data.MinJumpTime, m_data.MaxJumpTime);
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
        m_moveTimer = UnityEngine.Random.Range(m_data.MinMoveTime, m_data.MaxMoveTime);
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
        float waitTime = UnityEngine.Random.Range(0, m_data.AftorMoveWaitTime);
        yield return new WaitForSeconds(waitTime);
        waitTime = UnityEngine.Random.Range(m_data.LeftRotateRange, m_data.RightRotateRange);
        m_moveController.MouseMove(waitTime, waitTime);
    }

    void Rotating()
    {
        float rotate = UnityEngine.Random.Range(m_data.LeftRotateRange, m_data.RightRotateRange);
        m_moveController.MouseMove(rotate, rotate);
    }

    void Jump()
    {
        m_moveController.Jump(true);
    }

    void UnActive()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Attack")
        {
            if (m_isDed) return;

            //EventSystem.Instance.AddEnergy();
            //EventSystem.Instance.JudgeAttack(false);
            AudioSource.PlayClipAtPoint(m_clip, transform.position, 0.5f);
            Instantiate(m_dedEffect, transform.position + transform.up * 0.2f, transform.rotation);
            m_isDed = true;
            // ニンゲンに知らせて、同期させる
            Ded?.Invoke();
            m_poolManager?.DecreaseCount(true);
            Invoke(nameof(UnActive), 0.1f);
        }
    }
}
