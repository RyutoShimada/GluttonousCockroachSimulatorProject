using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class NetWorkGameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        static public NetWorkGameManager m_Instance = null;

        #endregion

        #region Private Fields

        GameObject m_instance;

        [Tooltip("CockroachNetWork の Prefab")]
        [SerializeField] GameObject m_cockroachPrefab = null;

        [Tooltip("Cockroach の 生成場所")]
        [SerializeField] Transform m_cockroachSpawnPos = null;

        [Tooltip("HumanNetWork の Prefab")]
        [SerializeField] GameObject m_humanPrefab = null;

        [Tooltip("Human の 生成場所")]
        [SerializeField] Transform m_humanSpawnPos = null;

        byte m_maxPlayerCount = 2;

        #endregion

        #region MonoBehaviour CallBacks

        void Start()
        {
            m_Instance = this;

            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("LauncherScene");
                return;
            }

            if (!m_cockroachPrefab)
            {
                Debug.LogError("CockroachPrefab が NULL です。NetWorkManager にアサインしてください。", this);
            }
            else if (!m_humanPrefab)
            {
                Debug.LogError("HumanPrefab が NULL です。NetWorkManager にアサインしてください。", this);
            }
            else
            {
                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                Debug.Log($"ActorNumber : {actorNumber} がルームに参加しました");

                if (actorNumber == 1)
                {
                    // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                    PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos.position, Quaternion.identity, 0);
                }
                else
                {
                    // 部屋の中で、ローカルプレーヤー用の Human を生成。PhotonNetwork.Instantiate()で同期。
                    PhotonNetwork.Instantiate(this.m_humanPrefab.name, m_humanSpawnPos.position, Quaternion.identity, 0);
                }

                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    Debug.Log("あなたはマスタークライアントです");
                }
            }
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// 同じ部屋にいたプレイヤーが参加した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"ActorNumber : {newPlayer.ActorNumber} がルームに参加しました");
        }

        /// <summary>
        /// 同じ部屋にいたプレイヤーが退出した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerLeftRoom(Player player)
        {
            Debug.Log($"ActorNumber : {player.ActorNumber} がルームに退出しました");
        }

        /// <summary>
        /// ローカルプレーヤーが部屋を出たときに呼び出される。ランチャーシーンをロードする必要があります。
        /// </summary>
		public override void OnLeftRoom()
        {
            SceneManager.LoadScene("LauncherScene");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 現在の部屋を離れてマスターサーバーに戻り、部屋に参加したり部屋を作ったりすることができます。
        /// </summary>
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// プレーヤーアプリケーションを終了します。
        /// </summary>
        public void QuitApplication()
        {
            Application.Quit();
        }

        #endregion
    }
}
