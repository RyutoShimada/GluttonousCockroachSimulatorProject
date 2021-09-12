using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;


public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    [SerializeField] GameObject m_cockroachPrefab = null;
    [SerializeField] GameObject m_humanPrefab = null;
    [Tooltip("プレイヤーがキャラクターを選択するための UI Panel")]
    [SerializeField] GameObject m_controlPanel = null;
    [Tooltip("接続の進捗状況をユーザーに知らせるUIテキスト")]
    [SerializeField] Text m_feedbackText = null;
    [Tooltip("1部屋あたりの最大プレイヤー数")]
    [SerializeField] byte m_maxPlayersPerRoom = 2;
    [Tooltip("ロード中に表示させるアニメ")]
    [SerializeField] GameObject m_loaderImage = null;
    [Tooltip("サーバー接続からの反応説明")]
    [SerializeField] GameObject m_description = null;
    #endregion


    #region Private Fields
    /// <summary>接続中かどうか</summary>
    bool m_isConnecting;
    /// <summary>このクライアントのバージョン番号</summary>
    string m_gameVersion = "1";
    /// <summary>このクライアントが操作しているオブジェクトの名前</summary>
    string m_operateName = null;
    #endregion


    #region Public Fields
    static public Launcher m_Instance;
    #endregion


    #region MonoBehaviour CallBacks
    void Awake()
    {
        if (m_loaderImage == null)
        {
            Debug.LogError("m_loaderAnime がアサインされていません", this);
        }

        m_Instance = this;
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        m_loaderImage.SetActive(false);
    }
    #endregion


    #region Private Fields
    /// <summary>
    /// タイトルからキャラクターを選択した時に呼ばれる
    /// </summary>
    /// <param name="next">選択したオブジェクトの名前</param>
    /// <param name="mode"></param>
    void SetOperate(Scene next, LoadSceneMode mode)
    {
        FindObjectOfType<NetWorkGameManager>().m_operateCharactor = m_operateName;
        SceneManager.sceneLoaded -= SetOperate;
    }
    #endregion


    #region Public Methods
    /// <summary>
    /// 接続処理を開始します。
    /// すでに接続されている場合は、ランダムな部屋への参加を試みます。
    /// まだ接続されていない場合、このアプリケーションのインスタンスをPhoton Cloud Networkに接続します。
    /// </summary>
    public void Connect()
    {
        m_feedbackText.text = "";
        m_isConnecting = true;
        m_controlPanel.SetActive(false);
        m_description.SetActive(false);
        m_loaderImage.SetActive(true);

        // 接続されているかどうかをチェックし、接続されていれば参加し、そうでなければサーバーへの接続を開始します。
        if (PhotonNetwork.IsConnected)
        {
            //LogFeedback("部屋へ接続...");

            // 既にあるランダムなルームへ接続。失敗すると新しくルームを作成する。
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            //LogFeedback("マスターサーバーへ接続...");

            PhotonNetwork.ConnectUsingSettings(); // マスターサーバーへ接続する
            PhotonNetwork.GameVersion = this.m_gameVersion;
        }

        SceneManager.sceneLoaded += SetOperate;
    }

    /// <summary>
    /// 開発者用のUnity Editor内ではなく、プレイヤー用のUIビューにフィードバックを記録します。
    /// </summary>
    /// <param name="message">表示内容</param>
    public void LogFeedback(string message)
    {
        if (!m_feedbackText) return;

        // 新しいメッセージを改行して表示する。
        m_feedbackText.text += System.Environment.NewLine + message;
    }

    /// <summary>
    /// ボタンから呼ぶ
    /// </summary>
    public void SetOperateNameIsCockroach()
    {
        m_operateName = m_cockroachPrefab.name;
    }

    /// <summary>
    /// ボタンから呼ぶ
    /// </summary>
    public void SetOperateNameIsHuman()
    {
        m_operateName = m_humanPrefab.name;
    }
    #endregion


    #region MonoBehaviourPunCallbacks CallBacks

    /// <summary>
    /// マスターサーバーへの接続に成功した時に呼ばれる
    /// </summary>
    public override void OnConnectedToMaster()
    {
        if (m_isConnecting)
        {
            LogFeedback("サーバーへアクセス...");
            Debug.Log("マスターサーバーへの接続に成功 -> ランダムな部屋への接続を開始");

            Hashtable expectedCustomRoomProperties;

            if (m_operateName == m_cockroachPrefab.name)
            {
                expectedCustomRoomProperties = new Hashtable { { m_humanPrefab.name, 0 } };
                PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, m_maxPlayersPerRoom);
            }
            else
            {
                expectedCustomRoomProperties = new Hashtable { { m_cockroachPrefab.name, 0 } };
                PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, m_maxPlayersPerRoom);
            }
        }
    }

    /// <summary>
    /// ランダムな部屋の入室に失敗した時に呼ばれる
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //LogFeedback("部屋を作成します...");
        Debug.Log("ランダムな部屋への接続に失敗 -> 部屋を作成");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = m_maxPlayersPerRoom;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { m_operateName };
        roomOptions.CustomRoomProperties = new Hashtable { { m_operateName, 0 } };
        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }


    /// <summary>
    /// Photonサーバーから失敗した場合や、意図的に切断した後に呼ばれる
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        LogFeedback($"サーバーとのアクセスがきれました : {cause}");
        Debug.LogError($"サーバーとの接続が切断されました : {cause}");

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }

        if (m_loaderImage && m_loaderImage.activeSelf)
        {
            m_loaderImage.SetActive(false);
        }

        m_isConnecting = false;

        if (m_controlPanel && !m_controlPanel.activeSelf)
        {
            m_controlPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 部屋の入室に成功したときに呼ばれる
    /// </summary>
    /// <remarks>
    /// このメソッドは、プレイヤーキャラクターをインスタンス化するためによく使われます。
    /// 試合を「能動的」に開始する必要がある場合は、ユーザーのボタン操作やタイマーで起動する[PunRPC]を呼び出すことができます。
    /// これが呼び出されたとき、通常はすでにPhotonNetwork.PlayerListを介して部屋にいる既存のプレーヤーにアクセスできます。
    /// また、すべてのカスタムプロパティは、Room.customProperties としてすでに利用可能なはずです。Room.PlayerCountを確認すると、以下のことがわかります。
    /// 充分な数のプレーヤーが部屋にいる状態でプレイを開始します。
    /// </remarks>
    public override void OnJoinedRoom()
    {
        Debug.Log($"部屋への接続に成功 : 現在の部屋の人数は { PhotonNetwork.CurrentRoom.PlayerCount} 人");

        // 最初のプレーヤーである場合のみロードし、そうでない場合はPhotonNetwork.AutomaticallySyncSceneに頼ってインスタンスシーンを同期させます。
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("NetWorkGameScene をロードします");
            PhotonNetwork.LoadLevel("NetWorkGameScene");
        }
    }

    #endregion
}