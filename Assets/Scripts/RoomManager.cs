using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{

   public class User
    {
        public int id;
        public string PlayerName;
        public bool[] values;

        public User(int id, string playerName)
        {
            this.id = id;
            this.PlayerName = playerName;
            values = new bool[3];
        }

        public int GetTrueAnswer()
        {
            int sum = 0;

            for(int i = 0;i < 3; i++)
            {
                sum += System.Convert.ToInt32(values[i]);
            }

            return sum;
        }
    }


    public static RoomManager instance;
    
    private int numberOfPlayers;
    [SerializeField]
    private PhotonView _photonView;

    private readonly int gameScene = 1;

    public List<User> players = new List<User>();

    private void Awake()
    {
        //If RoomManager instance is created, destroy it

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
                Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        numberOfPlayers = PhotonNetwork.PlayerList.Length;

        CheckPlayer();
    }


    // CHECK NUMBER OF PLAYER, IF GREATER THAN 2, START GAME
    private void CheckPlayer()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (numberOfPlayers == 2)
        {
            StartGame();
        }
    }


    private void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        //Game level
        SceneManager.LoadScene(gameScene);

        _photonView.RPC("LoadPlayers", RpcTarget.All);

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == gameScene)
        {
            RPC_CreatePlayer();
        }
    }

    [PunRPC]
    public void RPC_CreatePlayer()  //CREATE PLAYER CONTROLLER ON CLIENT
    {
        GameObject obj = PhotonNetwork.Instantiate("Prefabs/PlayerController", Vector3.zero, Quaternion.identity);
    }

    [PunRPC]
    public void LoadPlayers()   //ADD PLAYER INFO TO PLAYERS LIST ON CLIENT
    {
        Player[] _players = PhotonNetwork.PlayerList;

        for (int i = 0; i < _players.Length; i++)
        {
            players.Add(new User(i, _players[i].NickName));
        }
       
    }
    //IF PLAYER LEFT THE GAME, FINISH
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        GameManager.instance.FinishGame(0);
    }

    private void OnApplicationQuit()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1 && GameManager.instance.isInGame)
            GameManager.instance.FinishGame(0);
    }
}
