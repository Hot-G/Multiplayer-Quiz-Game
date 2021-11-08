using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;


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

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    
    private int numberOfPlayers;
    [SerializeField]
    private PhotonView _photonView;

    private readonly int gameScene = 1;

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
        //ADD EVENTS
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(this);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckPlayer();
    }


    // CHECK NUMBER OF PLAYER, IF EQUALS 2, START GAME
    private void CheckPlayer()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.PlayerList.Length == 2)
        {
            StartGame();
        }
    }


    private void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        //Game level
        PhotonNetwork.LoadLevel(gameScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == gameScene)
        {
            RPC_CreatePlayer();
        }
    }

    [PunRPC]
    private void RPC_CreatePlayer()  //CREATE PLAYER CONTROLLER ON CLIENT
    {
        PhotonNetwork.Instantiate("Prefabs/PlayerController", Vector3.zero, Quaternion.identity);
    }
    
    //IN GAME PROPERTIES
    public static int GetPlayerID(string playerName)
    {
        return ((PhotonNetwork.PlayerList[0].NickName == playerName) ? 0 : 1);
    }

    //IF PLAYER LEFT THE GAME, FINISH
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.instance.FinishGame(0);
    }

    private void OnApplicationQuit()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1 && GameManager.instance.isInGame)
            GameManager.instance.FinishGame(0);
    }
}
