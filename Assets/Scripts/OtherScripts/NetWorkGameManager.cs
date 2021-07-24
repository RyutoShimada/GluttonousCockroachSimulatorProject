using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class NetWorkGameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        static public NetWorkGameManager m_Instance = null;

        public bool m_isGame { get; private set; } = false;

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

        [Tooltip("タイマーのText")]
        [SerializeField] Text m_timerText = null;

        [Tooltip("制限時間（分）")]
        [SerializeField] int m_minutes = 5;

        [Tooltip("制限時間（秒）")]
        [SerializeField] float m_seconds = 59f;

        [Tooltip("カウントダウンのText")]
        [SerializeField] Text m_countDownText = null;

        [Tooltip("ゲーム開始までの秒数")]
        [SerializeField] int m_waitForSeconds = 3;

        #endregion

        [HideInInspector]
        public bool m_cockrochIsDed = false;   

        #region MonoBehaviour CallBacks

        void Start()
        {
            m_Instance = this;

            EventSystem.Instance.Subscribe((EventSystem.CockroachIsDed)CockroachIsDed);

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

                if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                    PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos.position, Quaternion.identity, 0);
                }
                else if (!HumanMoveControllerNetWork.m_Instance)
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

            if (m_isGame)
            {
                TimeCountDown();

                if (m_countDownText.gameObject.activeSelf)
                {
                    m_countDownText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!m_countDownText.gameObject.activeSelf)
                {
                    m_countDownText.gameObject.SetActive(true);
                    m_countDownText.text = "そこまで！";
                }
                
            }
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// プレイヤーが参加した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"ActorNumber : {newPlayer.ActorNumber} がルームに参加しました");

            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                photonView.RPC(nameof(GameStart), RpcTarget.All);
            }
        }

        /// <summary>
        /// 同じ部屋にいたプレイヤーが退出した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerLeftRoom(Player player)
        {
            Debug.Log($"ActorNumber : {player.ActorNumber} がルームから退出しました");
            SceneManager.sceneLoaded += LogFeedBack;
            LeaveRoom();
        }

        /// <summary>
        /// ローカルプレーヤーが部屋を出たときに呼び出される。ランチャーシーンをロードする必要があります。
        /// </summary>
		public override void OnLeftRoom()
        {
            SceneManager.LoadScene("LauncherScene");
        }

        #endregion

        #region Private Methods

        void LogFeedBack(Scene next, LoadSceneMode mode)
        {
            Launcher.m_Instance.LogFeedback("対戦相手が退出しました");
            SceneManager.sceneLoaded -= LogFeedBack;
        }


        void GameSet()
        {
            // TODO : ここにリザルトとかを書く
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

        [PunRPC]
        void GameStart()
        {
            StartCoroutine(CoroutineGameStart(m_waitForSeconds));
        }

        IEnumerator CoroutineGameStart(int waitSeconds)
        {
            for (int i = waitSeconds; i >= 0; i--)
            {
                yield return new WaitForSeconds(1f);

                if (i != 0)
                {
                    m_countDownText.text = i.ToString();
                }
                else
                {
                    m_countDownText.text = "スタート！";
                    yield return new WaitForSeconds(0.5f);
                }
            }

            this.m_isGame = true;
        }

        void TimeCountDown()
        {
            if (m_seconds > 0)
            {
                m_seconds -= Time.deltaTime;
            }
            else
            {
                if (m_minutes > 0)
                {
                    m_minutes--;
                    m_seconds = 59f;
                }
                else
                {
                    m_minutes = 0;
                    m_seconds = 0;
                    m_isGame = false;
                    Debug.Log("TimeUp!");
                }
            }

            if (m_timerText != null)
            {
                m_timerText.text = $"{m_minutes.ToString("00")} : {m_seconds.ToString("00")}";
            }
        }

        void CockroachIsDed(bool isDed)
        {
            m_isGame = !isDed;
            EventSystem.Instance.Unsubscribe((EventSystem.CockroachIsDed)CockroachIsDed);
        }
    }
}
