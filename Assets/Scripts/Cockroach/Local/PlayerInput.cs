using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] CockroachMoveController m_moveController = null;

    // Start is called before the first frame update
    void Start()
    {
        m_moveController.StartSet();
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (!m_moveController.IsDed && m_moveController.IsCanMove)
        {
            m_moveController.Move(Input.GetAxisRaw("Vertical"));
            m_moveController.Jump(Input.GetButtonDown("Jump"));
            m_moveController.MouseMove(Input.GetAxis("Look X"));
            yield return new WaitForEndOfFrame();
        }
    }
}
