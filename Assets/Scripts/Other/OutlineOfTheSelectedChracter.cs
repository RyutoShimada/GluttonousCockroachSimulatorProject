using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

enum SelectState
{
    Up,
    Down,
    Left,
    Right
}

public class OutlineOfTheSelectedChracter : MonoBehaviour
{
    [SerializeField] CockroachOutline m_cockroachOutline = null;
    [SerializeField] HumanOutline m_humanOutline = null;

    [SerializeField] GameObject m_returnButtonImage = null;
    [SerializeField] GameObject m_quitButtonImage = null;

    [SerializeField] AudioClip m_cursolSE = null;
    [SerializeField] AudioClip m_clickSE = null;

    Vector3 m_cursorPosition;
    Vector3 m_cursorPosition3d;
    RaycastHit m_hit;

    AudioSource m_audio;

    SelectState m_selectState = SelectState.Left;

    private void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    void ChangeState()
    {
        switch (m_selectState)
        {
            case SelectState.Up:
                Debug.Log("UP");
                // 自分をフォーカス
                if (!m_returnButtonImage.activeSelf) m_returnButtonImage.SetActive(true);
                m_audio.PlayOneShot(m_cursolSE);
                // 自分以外のフォーカスを外す
                if (m_cockroachOutline.enabled) m_cockroachOutline.enabled = false;
                if (m_humanOutline.enabled) m_humanOutline.enabled = false;
                if (m_quitButtonImage.activeSelf) m_quitButtonImage.SetActive(false);
                break;

            case SelectState.Down:
                Debug.Log("DOWN");
                if (!m_quitButtonImage.activeSelf) m_quitButtonImage.SetActive(true);
                m_audio.PlayOneShot(m_cursolSE);
                if (m_cockroachOutline.enabled) m_cockroachOutline.enabled = false;
                if (m_humanOutline.enabled) m_humanOutline.enabled = false;
                if (m_returnButtonImage.activeSelf) m_returnButtonImage.SetActive(false);
                break;

            case SelectState.Left:
                Debug.Log("LEFT");
                if (!m_cockroachOutline.enabled) m_cockroachOutline.enabled = true;
                m_audio.PlayOneShot(m_cursolSE);
                if (m_humanOutline.enabled) m_humanOutline.enabled = false;
                if (m_returnButtonImage.activeSelf) m_returnButtonImage.SetActive(false);
                if (m_quitButtonImage.activeSelf) m_quitButtonImage.SetActive(false);
                break;

            case SelectState.Right:
                Debug.Log("RIGHT");
                if (!m_humanOutline.enabled) m_humanOutline.enabled = true;
                m_audio.PlayOneShot(m_cursolSE);
                if (m_cockroachOutline.enabled) m_cockroachOutline.enabled = false;
                if (m_returnButtonImage.activeSelf) m_returnButtonImage.SetActive(false);
                if (m_quitButtonImage.activeSelf) m_quitButtonImage.SetActive(false);
                break;

            default:
                break;
        }
    }

    void OnRay(InputAction.CallbackContext context)
    {
        m_cursorPosition = context.ReadValue<Vector2>();
        m_cursorPosition.z = 10.0f; // z座標に適当な値を入れる
        m_cursorPosition3d = Camera.main.ScreenToWorldPoint(m_cursorPosition); // 3Dの座標になおす

        // カメラから cursorPosition3d の方向へレイを飛ばす
        if (Physics.Raycast(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position), out m_hit, Mathf.Infinity))
        {
            Debug.DrawRay(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position) * m_hit.distance, Color.red);

            if (m_hit.collider.gameObject.tag == "Cockroach")
            {
                if (!m_cockroachOutline.enabled)
                {
                    m_cockroachOutline.enabled = true;
                    m_audio.PlayOneShot(m_cursolSE);
                }
            }
            else if (m_hit.collider.gameObject.tag == "Human")
            {
                if (!m_humanOutline.enabled)
                {
                    m_humanOutline.enabled = true;
                    m_audio.PlayOneShot(m_cursolSE);
                }
            }
            else
            {
                UnEnabled();
            }
        }
        else
        {
            UnEnabled();
        }
    }

    void UnEnabled()
    {
        if (m_cockroachOutline.enabled)
        {
            m_cockroachOutline.enabled = false;
        }

        if (m_humanOutline.enabled)
        {
            m_humanOutline.enabled = false;
        }
    }

    public void Click()
    {
        m_audio.PlayOneShot(m_clickSE);
    }

    public void LeftSelect(InputAction.CallbackContext context)
    {
        if (Gamepad.current == null) return;

        if (context.ReadValue<float>() != 0)
        {
            m_selectState = SelectState.Left;
            ChangeState();
        }
    }

    public void RightSelect(InputAction.CallbackContext context)
    {
        if (Gamepad.current == null) return;

        if (context.ReadValue<float>() != 0)
        {
            m_selectState = SelectState.Right;
            ChangeState();
        }
    }

    public void UpSelect(InputAction.CallbackContext context)
    {
        if (Gamepad.current == null) return;

        if (context.ReadValue<float>() != 0)
        {
            if (m_selectState == SelectState.Left || m_selectState == SelectState.Right)
            {
                m_selectState = SelectState.Up;
            }
            else if (m_selectState == SelectState.Down)
            {
                m_selectState = SelectState.Right;
            }

            ChangeState();
        }
    }

    public void DownSelect(InputAction.CallbackContext context)
    {
        if (Gamepad.current == null) return;

        if (context.ReadValue<float>() != 0)
        {
            if (m_selectState == SelectState.Left || m_selectState == SelectState.Right)
            {
                m_selectState = SelectState.Down;
            }
            else if (m_selectState == SelectState.Up)
            {
                m_selectState = SelectState.Left;
            }

            ChangeState();
        }
    }

    public void MouseSelect(InputAction.CallbackContext context)
    {
        if (Mouse.current == null) return;
        OnRay(context);
    }

    public void Decision(InputAction.CallbackContext context)
    {

    }
}
