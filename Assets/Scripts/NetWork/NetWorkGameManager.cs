using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public enum OperatedCharactor
    {
        Cockroach,
        Human
    }

    public class NetWorkGameManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields
        static public NetWorkGameManager m_Instance = null;
        public string m_operateCharactor = null;
        #endregion

        #region Private Fields
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

        [Tooltip("Result の UI(GameObject) をアサインする")]
        [SerializeField] GameObject m_resultUi = null;

        [Tooltip("Result の ResultText をアサインする")]
        [SerializeField] Text m_resultText = null;

        [Tooltip("FeedBackText をアサイン")]
        [SerializeField] Text m_feedBack = null;

        [SerializeField] FoodGeneraterNetWork m_foodGeneraterNetWork = null;

        CockroachUINetWork m_cockroachUINetWork = null;

        /// <summary>操作しているキャラクター</summary>
        OperatedCharactor m_operatedByPlayer;

        /// <summary>勝利したキャラクター</summary>
        OperatedCharactor m_victoryPlayer;

        /// <summary>動けるかどうか</summary>
        IIsCanMove m_canMove = null;

        /// <summary>リザルトを表示しているかどうか</summary>
        bool m_isResualt = false;

        bool m_isGame;

        GameObject m_instance;
        #endregion

        #region Property
        public bool IsGame { get => m_isGame; }
        #endregion

        #region MonoBehaviour CallBacks
        private void Awake()
        {
            m_Instance = this;
        }

        void Start()
        {
            EventSystem.Instance.Subscribe((EventSystem.CockroachIsDed)CockroachIsDed);
            //EventSystem.Instance.Subscribe((EventSystem.FoodGenerate)FoodGenerate);

            if (!PhotonNetwork.IsConnected)
            {
                // マスターサーバーへ接続できなかったらLuncherSceneをロードする
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
                Cursor.visible = false;
                m_feedBack.text = "対戦相手を待っています...";
                GameObject operate = null;

                if (m_operateCharactor == this.m_cockroachPrefab.name)
                {
                    // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                    operate = PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos.position, Quaternion.identity, 0);
                    m_cockroachUINetWork = operate.GetComponent<CockroachUINetWork>(); // ゴキブリ用のUIを入れる
                    m_operatedByPlayer = OperatedCharactor.Cockroach;
                }
                else
                {
                    // 部屋の中で、ローカルプレーヤー用の Human を生成。PhotonNetwork.Instantiate()で同期。
                    operate = PhotonNetwork.Instantiate(this.m_humanPrefab.name, m_humanSpawnPos.position, Quaternion.identity, 0);
                    m_operatedByPlayer = OperatedCharactor.Human;
                }

                m_canMove = operate.GetComponent<IIsCanMove>();
                m_canMove.IsMove(true);

                //EventSystem.Instance.IsMove(true);

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
                LeaveRoom();
            }

            if (m_isGame)
            {
                TimeCountDown();
            }
            else
            {
                if (!m_countDownText.gameObject.activeSelf)
                {
                    m_countDownText.gameObject.SetActive(true);
                    m_countDownText.text = "そこまで！";
                    //EventSystem.Instance.Unsubscribe((EventSystem.FoodGenerate)FoodGenerate);

                    if (m_operatedByPlayer == OperatedCharactor.Cockroach && m_cockroachUINetWork)
                    {
                        m_cockroachUINetWork.UiSetActiveFalse();
                    }

                    if (m_resultUi && m_resultText)
                    {
                        StartCoroutine(Result());
                    }
                    else
                    {
                        Debug.LogError("m_result もしくは m_resultText が Null です", this);
                    }
                }

            }
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// プレイヤーが参加した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Realtime.Player newPlayer)
        {
            Debug.Log($"ActorNumber : {newPlayer.ActorNumber} がルームに参加しました");

            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false; // 部屋を閉じる
                photonView.RPC(nameof(GameStart), RpcTarget.AllViaServer);
            }
        }

        /// <summary>
        /// 同じ部屋にいたプレイヤーが退出した時に呼ばれる
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerLeftRoom(Realtime.Player player)
        {
            Debug.Log($"ActorNumber : {player.ActorNumber} がルームから退出しました");

            if (!m_isResualt)
            {
                SceneManager.sceneLoaded += LogFeedBack;
                LeaveRoom();
            }
        }

        /// <summary>
        /// ローカルプレーヤーが部屋を出たときに呼び出される。
        /// </summary>
		public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("LauncherScene");
        }

        #endregion

        #region Private Methods

        void LogFeedBack(Scene next, LoadSceneMode mode)
        {
            Launcher.m_Instance.LogFeedback("対戦相手が退出しました");
            SceneManager.sceneLoaded -= LogFeedBack;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 現在の部屋を離れてマスターサーバーに戻り、部屋に参加したり部屋を作ったりすることができます。
        /// </summary>
        public void LeaveRoom()
        {
            Cursor.visible = true;
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
            m_feedBack.text = "";
            m_canMove.IsMove(false);
            //EventSystem.Instance.IsMove(false);

            if (m_operatedByPlayer == OperatedCharactor.Cockroach)
            {
                EventSystem.Instance.Reset(m_cockroachSpawnPos.position, m_cockroachSpawnPos.rotation);
            }
            else
            {
                EventSystem.Instance.Reset(m_humanSpawnPos.position, m_humanSpawnPos.rotation);
            }

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
                    m_countDownText.text = "はじめ！";
                    yield return new WaitForSeconds(0.5f);
                }
            }

            this.m_isGame = true;
            m_countDownText.gameObject.SetActive(false);
            m_canMove.IsMove(m_isGame);
            //EventSystem.Instance.IsMove(m_isGame);

            if (PhotonNetwork.IsMasterClient)
            {
                // Foodの生成はマスタークライアントだけ行う
                m_foodGeneraterNetWork.GetComponent<IFoodGenerate>().Generate();
            }
        }

        IEnumerator Result()
        {
            m_isResualt = true;
            yield return new WaitForSeconds(1f);
            m_resultUi.SetActive(true);
            Cursor.visible = true;

            if (m_victoryPlayer == this.m_operatedByPlayer)
            {
                m_resultText.text = "あなたの勝ちです";
            }
            else
            {
                m_resultText.text = "あなたの負けです";
            }
        }

        void TimeCountDown()
        {
            if (PhotonNetwork.IsMasterClient)
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
                        m_victoryPlayer = OperatedCharactor.Cockroach;
                        m_minutes = 0;
                        m_seconds = 0;
                        m_isGame = false;
                        m_canMove.IsMove(false);
                        //EventSystem.Instance.IsMove(m_isGame);
                        Debug.Log("TimeUp!");
                    }
                }
            }

            if (m_timerText != null)
            {
                m_timerText.text = $"{m_minutes.ToString("00")} : {m_seconds.ToString("00")}";
            }
        }

        void CockroachIsDed(bool isDed)
        {
            m_victoryPlayer = OperatedCharactor.Human;
            m_isGame = !isDed;
            EventSystem.Instance.IsMove(false);
            EventSystem.Instance.Unsubscribe((EventSystem.CockroachIsDed)CockroachIsDed);
        }

        // Event をやっていた
        //void FoodGenerate()
        //{
        //    if (!PhotonNetwork.IsMasterClient) return;

        //    if (m_foodGeneraterNetWork)
        //    {
        //        StartCoroutine(m_foodGeneraterNetWork.StartGenerate());
        //    }
        //    else
        //    {
        //        Debug.LogError("m_foodGeneraterNetWork は破棄されています");
        //    }
        //}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(m_minutes);
                stream.SendNext(m_seconds);
                stream.SendNext(m_victoryPlayer);
                stream.SendNext(m_isGame);
            }
            else if (!stream.IsWriting && !PhotonNetwork.IsMasterClient)
            {
                m_minutes = (int)stream.ReceiveNext();
                m_seconds = (float)stream.ReceiveNext();
                m_victoryPlayer = (OperatedCharactor)stream.ReceiveNext();
                m_isGame = (bool)stream.ReceiveNext();
            }
        }
    }
}
