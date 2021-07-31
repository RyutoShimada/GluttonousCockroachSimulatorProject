using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Photon.Pun.Demo.PunBasics
{
    public class PhotonTestPlayerMove : MonoBehaviourPunCallbacks
    {
        static public GameObject m_localPlayerInstance;

        [SerializeField] GameObject m_camera = null;
        [SerializeField] GameObject m_vcamPrefab = null;

        float m_h = 0;
        float m_v = 0;
        Rigidbody m_rb;
        Vector3 m_velo;
        Vector3 m_dir;

        private void Awake()
        {
            if (photonView.IsMine)
            {
                m_localPlayerInstance = gameObject;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (photonView.IsMine)
            {
                m_rb = GetComponent<Rigidbody>();

                if (m_rb)
                {
                    m_velo = m_rb.velocity;
                }

                m_camera.SetActive(true);
                GameObject go = Instantiate(m_vcamPrefab, m_camera.transform.position, m_camera.transform.rotation);
                CinemachineVirtualCamera vcam = go.GetComponent<CinemachineVirtualCamera>();
                vcam.Follow = transform;
                vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = m_camera.transform.localPosition;
            }
            else
            {
                m_camera.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;
            Move();
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.IsMine) return;

            m_h = Input.GetAxisRaw("Horizontal");
            m_v = Input.GetAxisRaw("Vertical");
        }

        private void Move()
        {
            m_dir = transform.forward * m_v;

            if (m_v != 0) // 前後
            {
                m_velo = m_dir.normalized * 10f;
                m_velo.y = m_rb.velocity.y;
            }
            else
            {
                m_velo = new Vector3(0, m_rb.velocity.y, 0);
            }

            m_rb.velocity = m_velo;

            if (m_h != 0)
            {
                transform.Rotate(new Vector3(0, m_h, 0), 3f);
            }
        }
    }
}
