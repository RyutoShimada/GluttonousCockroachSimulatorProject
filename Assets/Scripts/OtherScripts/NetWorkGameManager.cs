using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public enum SelecteCharacter
{
    Cockroach,
    Human
}

public class NetWorkGameManager : MonoBehaviourPunCallbacks
{
    /// <summary>プレイヤーのプレハブ</summary>
    [SerializeField] string m_cockroachPrefabName = "Cockroach";
    [SerializeField] string m_humanPrefabName = "Human";
    [SerializeField] string m_humanVcamPrefabName = "HumanVcam";
    GameObject m_cockoroach = null;
    GameObject m_human = null;
    //[SerializeField] Transform[] m_spawnPositions;
    [SerializeField] Transform m_cockroachSpawnPos;
    [SerializeField] Transform m_humanSpawnPos;
    [SerializeField] GameObject m_cockroachCanvas = null;
    [SerializeField] Transform m_humanVcamPos;
    GameObject m_cockoroachCamera = null;
    GameObject m_humanCamera = null;
    const byte m_maxPlayers = 2;
    SelecteCharacter m_selectCharacter;
    CinemachineVirtualCameraBase m_humanVcam;

    public SelecteCharacter SelecteCharacter
    {
        get => m_selectCharacter;
        set
        {
            m_selectCharacter = value;
        }
    }

    private void Awake()
    {
        // シーンの自動同期は無効にする（シーン切り替えがない時は意味はない）
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    private void Start()
    {
        // Photon に接続する
        Connect("1.0"); // 1.0 はバージョン番号（同じバージョンを指定したクライアント同士が接続できる）
    }

    /// <summary>
    /// Photonに接続する
    /// </summary>
    private void Connect(string gameVersion)
    {
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.GameVersion = gameVersion;    // 同じバージョンを指定したもの同士が接続できる
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// ニックネームを付ける
    /// </summary>
    private void SetMyNickName(string nickName)
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("nickName: " + nickName);
            PhotonNetwork.LocalPlayer.NickName = nickName;
        }
    }

    /// <summary>
    /// ロビーに入る
    /// </summary>
    private void JoinLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    /// <summary>
    /// 既に存在する部屋に参加する
    /// </summary>
    private void JoinExistingRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    /// <summary>
    /// ランダムな名前のルームを作って参加する
    /// </summary>
    private void CreateRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;   // 誰でも参加できるようにする
            /* **************************************************
             * spawPositions の配列長を最大プレイ人数とする。
             * 無料版では最大20まで指定できる。
             * MaxPlayers の型は byte なのでキャストしている。
             * MaxPlayers の型が byte である理由はおそらく1ルームのプレイ人数を255人に制限したいためでしょう。
             * **************************************************/
            //roomOptions.MaxPlayers = (byte)m_spawnPositions.Length;
            roomOptions.MaxPlayers = m_maxPlayers;
            PhotonNetwork.CreateRoom(null, roomOptions); // ルーム名に null を指定するとランダムなルーム名を付ける
        }
    }

    /// <summary>
    /// プレイヤーを生成する
    /// </summary>
    private void SpawnPlayer()
    {
        // プレイヤーをどこに spawn させるか決める
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;    // 自分の ActorNumber を取得する。なお ActorNumber は「1から」入室順に振られる。
        Debug.Log("My ActorNumber: " + actorNumber);

        if (actorNumber == 1)
        {
            Transform spawnPoint = m_cockroachSpawnPos;
            // Cockroachを生成し、他のクライアントと同期する
            m_cockoroach = PhotonNetwork.Instantiate(m_cockroachPrefabName, spawnPoint.position, Quaternion.identity);

            if (!m_cockoroach) return;
            m_cockoroachCamera = m_cockoroach.transform.Find("CockroachCamera").gameObject;
            m_cockoroachCamera.SetActive(true);
            m_cockroachCanvas.SetActive(true);
        }
        else
        {
            Transform spawnPoint = m_humanSpawnPos;
            // Humanを生成し、他のクライアントと同期する
            m_human = PhotonNetwork.Instantiate(m_humanPrefabName, spawnPoint.position, Quaternion.identity);

            if (!m_human) return;
            GameObject go = PhotonNetwork.Instantiate(m_humanVcamPrefabName, m_humanVcamPos.position, Quaternion.identity);
            Debug.Log(go.name);
            m_humanVcam = go.gameObject.GetComponent<CinemachineVirtualCameraBase>();
            m_humanVcam.Follow = m_human.transform;
            m_humanCamera = m_human.transform.Find("HumanCamera").gameObject;
            m_humanCamera.SetActive(true);
            m_cockroachCanvas.SetActive(false);
        }

        /* **************************************************
         * ルームに参加している人数が最大に達したら部屋を閉じる（参加を締め切る）
         * 部屋を閉じないと、最大人数から減った時に次のユーザーが入ってきてしまう。
         * 現状のコードではユーザーが最大人数から減った際の追加入室を考慮していないため、追加入室させたい場合は実装を変更する必要がある。
         * **************************************************/
        if (actorNumber > PhotonNetwork.CurrentRoom.MaxPlayers - 1)
        {
            Debug.Log("Closing Room");
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    /* ***********************************************
     * 
     * これ以降は Photon の Callback メソッド
     * 
     * ***********************************************/

    /// <summary>Photon に接続した時</summary>
    public override void OnConnected()
    {
        Debug.Log("OnConnected");
        SetMyNickName(System.Environment.UserName + "@" + System.Environment.MachineName);
    }

    /// <summary>Photon との接続が切れた時</summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected");
    }

    /// <summary>マスターサーバーに接続した時</summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        JoinLobby();
    }

    /// <summary>ロビーに参加した時</summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        JoinExistingRoom();
    }

    /// <summary>ロビーから出た時</summary>
    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
    }

    /// <summary>部屋を作成した時</summary>
    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    /// <summary>部屋の作成に失敗した時</summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed: " + message);
    }

    /// <summary>部屋に入室した時</summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        SpawnPlayer();
    }

    /// <summary>指定した部屋への入室に失敗した時</summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed: " + message);
    }

    /// <summary>ランダムな部屋への入室に失敗した時</summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed: " + message);
        CreateRandomRoom();
    }

    /// <summary>部屋から退室した時</summary>
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom");
    }

    /// <summary>自分のいる部屋に他のプレイヤーが入室してきた時</summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom: " + newPlayer.NickName);
    }

    /// <summary>自分のいる部屋から他のプレイヤーが退室した時</summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom: " + otherPlayer.NickName);
    }

    /// <summary>マスタークライアントが変わった時</summary>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("OnMasterClientSwitched to: " + newMasterClient.NickName);
    }

    /// <summary>ロビー情報に更新があった時</summary>
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Debug.Log("OnLobbyStatisticsUpdate");
    }

    /// <summary>ルームリストに更新があった時</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
    }

    /// <summary>ルームプロパティが更新された時</summary>
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("OnRoomPropertiesUpdate");
    }

    /// <summary>プレイヤープロパティが更新された時</summary>
    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("OnPlayerPropertiesUpdate");
    }

    /// <summary>フレンドリストに更新があった時</summary>
    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("OnFriendListUpdate");
    }

    /// <summary>地域リストを受け取った時</summary>
    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("OnRegionListReceived");
    }

    /// <summary>WebRpcのレスポンスがあった時</summary>
    public override void OnWebRpcResponse(OperationResponse response)
    {
        Debug.Log("OnWebRpcResponse");
    }

    /// <summary>カスタム認証のレスポンスがあった時</summary>
    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log("OnCustomAuthenticationResponse");
    }

    /// <summary>カスタム認証が失敗した時</summary>
    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("OnCustomAuthenticationFailed");
    }
}
