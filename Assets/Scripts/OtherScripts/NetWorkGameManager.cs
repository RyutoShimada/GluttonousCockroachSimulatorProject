using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class NetWorkGameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        static public GameManager m_Instance;

        #endregion

        #region Private Fields

        private GameObject m_instance;

        [Tooltip("CockroachNetWorkのPrefab")]
        [SerializeField]
        private GameObject m_cockroachPrefab;

        [Tooltip("HumanNetWorkのPrefab")]
        [SerializeField]
        private GameObject m_humanPrefab;

        #endregion

        #region MonoBehaviour CallBacks

        void Start()
        {

        }

        
        void Update()
        {

        }

        #endregion
    }
}
