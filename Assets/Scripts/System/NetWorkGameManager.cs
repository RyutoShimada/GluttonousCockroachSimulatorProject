using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public enum Charactor
{
    Cockroach,
    Human
}

enum GameSatate
{
    Stay,
    InGame,
    GameOver
}

public class NetWorkGameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields
    static public NetWorkGameManager Instance = null;
    [HideInInspector] public string m_operateCharactor = null;
    #endregion

    #region Private Fields
    [Tooltip("CockroachNetWork の Prefab")]
    [SerializeField] GameObject m_cockroachPrefab = null;

    [Tooltip("Cockroach の 生成場所")]
    [SerializeField] Transform[] m_cockroachSpawnPos = null;

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

    [SerializeField] FoodGenerater m_foodGeneraterNetWork = null;

    [SerializeField] GameObject m_menu = null;

    [SerializeField] Button m_resualtSelectButton = null;

    [SerializeField] GameObject[] m_countDown = null;

    CockroachUI m_cockroachUINetWork = null;

    Cockroach m_cockroach = null;

    Human m_human = null;

    /// <summary>操作しているキャラクター</summary>
    Charactor m_operatedByPlayer;

    /// <summary>勝利したキャラクター</summary>
    Charactor m_victoryPlayer;

    /// <summary>動けるかどうか</summary>
    IIsCanMove m_canMove = null;

    GameSatate m_gameState = default;

    /// <summary>リザルトを表示しているかどうか</summary>
    public bool m_isResualt = false;

    public bool m_counting = false;

    bool m_isGame;

    #endregion

    #region Property
    public bool IsGame { get => m_isGame; }
    #endregion

    #region MonoBehaviour CallBacks
    private void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
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
            m_feedBack.text = "マッチングちゅう...";
            GameObject operate = null;
            m_gameState = GameSatate.Stay;

            if (m_operateCharactor == this.m_cockroachPrefab.name)
            {
                // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                int random = Random.Range(0, m_cockroachSpawnPos.Length);
                operate = PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos[random].position, m_cockroachSpawnPos[random].rotation, 0);
                m_cockroach = operate.GetComponent<Cockroach>();
                m_cockroachUINetWork = operate.GetComponent<CockroachUI>(); // ゴキブリ用のUIを入れる
                m_operatedByPlayer = Charactor.Cockroach;
            }
            else
            {
                // 部屋の中で、ローカルプレーヤー用の Human を生成。PhotonNetwork.Instantiate()で同期。
                operate = PhotonNetwork.Instantiate(this.m_humanPrefab.name, m_humanSpawnPos.position, Quaternion.identity, 0);
                m_human = operate.transform.GetComponentInChildren<Human>();
                m_operatedByPlayer = Charactor.Human;
            }

            Cockroach.IsDed += CockroachIsDed;
            m_canMove = operate.GetComponent<IIsCanMove>();
            m_canMove.IsMove(true);

            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Debug.Log("あなたはマスタークライアントです");
            }
        }
    }


    //void Update()
    //{
    //    if (m_isGame)
    //    {
    //        TimeCountDown();
    //    }
    //    else
    //    {
    //        if (m_gameState == GameSatate.InGame)
    //        {
    //            photonView.RPC(nameof(GameOver), RpcTarget.AllViaServer);
    //        }
    //    }
    //}

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cockroach.IsDed -= CockroachIsDed;
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
            PhotonNetwork.CurrentRoom.IsOpen = false; // 部屋を閉じる
            photonView.RPC(nameof(GameStart), RpcTarget.AllViaServer);
        }
    }

    /// <summary>
    /// 同じ部屋にいたプレイヤーが退出した時に呼ばれる
    /// </summary>
    /// <param name="other">Other.</param>
    public override void OnPlayerLeftRoom(Player player)
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

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting && PhotonNetwork.IsMasterClient)
    //    {
    //        stream.SendNext(m_minutes);
    //        stream.SendNext(m_seconds);
    //    }
    //    else if (!stream.IsWriting && !PhotonNetwork.IsMasterClient)
    //    {
    //        m_minutes = (int)stream.ReceiveNext();
    //        m_seconds = (float)stream.ReceiveNext();
    //    }
    //}

    #endregion

    #region Private Methods

    void LogFeedBack(Scene next, LoadSceneMode mode)
    {
        Launcher.m_Instance.LogFeedback("テキ が にげました");
        SceneManager.sceneLoaded -= LogFeedBack;
    }

    [PunRPC]
    void GameStart()
    {
        if (m_operatedByPlayer == Charactor.Cockroach)
        {
            m_feedBack.text = "エサ を たべて にげろ";
        }
        else
        {
            m_feedBack.text = "ヤツ を たおせ";
        }

        if (m_operatedByPlayer == Charactor.Cockroach)
        {
            int random = Random.Range(0, m_cockroachSpawnPos.Length);
            EventSystem.Instance.ResetTransform(m_cockroachSpawnPos[random].position, m_cockroachSpawnPos[random].rotation);
        }
        else
        {
            EventSystem.Instance.ResetTransform(m_humanSpawnPos.position, m_humanSpawnPos.rotation);
        }

        m_canMove.IsMove(false);
        //EventSystem.Instance.IsMove(false);

        StartCoroutine(CoroutineGameStart(m_waitForSeconds));
    }

    [PunRPC]
    void GameOver()
    {
        if (m_gameState == GameSatate.Stay) { LeaveRoom(); }

        if (m_countDownText.gameObject.activeSelf) return;
        
        if (m_menu.activeSelf) { m_menu.SetActive(false); }

        m_isGame = false;
        m_gameState = GameSatate.GameOver;
        m_canMove.IsMove(false);
        m_countDownText.gameObject.SetActive(true);
        m_countDownText.text = "そこまで";
        Cockroach.IsDed -= CockroachIsDed;

        if (m_operatedByPlayer == Charactor.Cockroach && m_cockroachUINetWork)
        {
            m_cockroachUINetWork.UiSetActiveFalse();
        }

        if (m_operatedByPlayer == Charactor.Human && m_human)
        {
            m_human.UiSetActiveFalse();
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

    [PunRPC]
    void VictoryCharactor(Charactor victory) => m_victoryPlayer = victory;

    IEnumerator CoroutineGameStart(int waitSeconds)
    {
        m_counting = true;
        for (int i = waitSeconds; i >= 0; i--)
        {
            yield return new WaitForSeconds(1f);

            if (i != 0)
            {
                if (i < m_countDown.Length)
                {
                    m_countDown[i].SetActive(false);
                }

                m_countDown[i - 1].SetActive(true);
            }
            else
            {
                m_countDown[0].SetActive(false);
                m_countDownText.text = "はじめ";
                yield return new WaitForSeconds(0.5f);
                m_feedBack.text = "";
                m_cockroach?.CallGenerate(10); //最初に10匹生成
            }
        }

        m_isGame = true;
        m_counting = false;
        m_gameState = GameSatate.InGame;
        m_countDownText.gameObject.SetActive(false);
        m_canMove.IsMove(true);

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
        Cursor.lockState = CursorLockMode.None;
        m_resualtSelectButton?.Select();

        if (m_victoryPlayer == this.m_operatedByPlayer)
        {
            m_resultText.text = "きみの かち だよ";
        }
        else
        {
            m_resultText.text = "きみの まけ だよ";
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
                    photonView.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Cockroach);
                    m_minutes = 0;
                    m_seconds = 0;
                    m_isGame = false;
                }
            }
        }

        if (m_timerText != null)
        {
            m_timerText.text = $"{m_minutes.ToString("00")} : {m_seconds.ToString("00")}";
        }
    }

    void CockroachIsDed()
    {
        if (photonView != null)
        {
            photonView.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Human);
            photonView.RPC(nameof(GameOver), RpcTarget.AllViaServer);
        }
        else
        {
            PhotonView view = GetComponent<PhotonView>();
            view.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Human);
            view.RPC(nameof(GameOver), RpcTarget.AllViaServer);
        }
        
        m_isGame = false;
    }

    #endregion

    #region Public Methods

    public void CockroachProliferationComplete()
    {
        photonView.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Cockroach);
        m_isGame = false;
        photonView.RPC(nameof(GameOver), RpcTarget.AllViaServer);
    }

    /// <summary>
    /// 降参する時ボタンから呼ぶ
    /// </summary>
    public void GameOverCallBack()
    {
        if (m_operatedByPlayer == Charactor.Cockroach)
        {
            photonView.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Human);
        }
        else
        {
            photonView.RPC(nameof(VictoryCharactor), RpcTarget.All, Charactor.Cockroach);
        }

        photonView.RPC(nameof(GameOver), RpcTarget.AllViaServer);
    }

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
}