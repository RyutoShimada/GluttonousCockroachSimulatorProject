﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
    public class HumanSprayAttackControllerNetWork : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject m_humanUi = null;
        [HideInInspector] public Image m_crossHair = null;
        GameObject m_ui = null;

        /// <summary>ゴキブリに当たっているかどうか</summary>
        [HideInInspector] public bool m_sprayHit;

        private void Awake()
        {
            if (!photonView.IsMine) return;
            m_sprayHit = false;
            Transform t = GameObject.Find("Canvas").transform;
            m_ui = Instantiate(m_humanUi, t);
            m_crossHair = m_ui.GetComponent<Image>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Cockroach")
            {
                m_sprayHit = true;
                m_crossHair.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Cockroach")
            {
                m_sprayHit = false;
                m_crossHair.color = Color.white;
            }
        }

        /// <summary>
        /// Cockroach の UI を非表示にします
        /// </summary>
        public void UiSetActiveFalse() => m_ui.SetActive(false);
    }
}