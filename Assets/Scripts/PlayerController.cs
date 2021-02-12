using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviour, IOnEventCallback
{
    [SerializeField]
    private string _playerName;

    public int PlayerID { get; set; }

    public string PlayerName
    {
        get
        {
            return _playerName;
        }

        set
        {
            _playerName = value;
            gameObject.name = _playerName;
        }
    }

    [SerializeField]
    private PhotonView _photonView;

    GameManager manager;

    private void Start()
    {
        manager = FindObjectOfType<GameManager>();

        if (_photonView.IsMine)
            _photonView.RPC("SetPlayer", RpcTarget.All, PlayerPrefs.GetString("PlayerName"));

        if (PhotonNetwork.IsMasterClient)
        {
            PreparePlayers();
            _photonView.RPC("PreparePlayers", RpcTarget.Others);
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    [PunRPC]
    public void SetPlayer(string name)
    {
        PlayerName = name;
    }

    [PunRPC]
    public void PreparePlayers()
    {
        manager.PreparePlayers(PhotonNetwork.PlayerList);

        if(_photonView.IsMine)
            StartCoroutine(WaitStart());
    }

    private IEnumerator WaitStart()
    {
        for(int i = 3;i > 0; i--)
        {
            manager.UpdateStartTimer(i);
            yield return new WaitForSeconds(1);
        }
        //CLOSE START PANEL
        manager.StartPanelActive(false);
        //SHOW ANOTHER QUESTION
        manager.Next();
    }


    public void OnEvent(EventData photonEvent)
    {
        if (!_photonView.IsMine)
            return;

        if (photonEvent.Code > 3)
            return;

        string playerName = ((object[])photonEvent.CustomData)[0] as string;

        switch (photonEvent.Code)
        {
            case 0:
                manager.UpdateScore(playerName, (bool)((object[])photonEvent.CustomData)[2]);

                break;
            case 1:
                manager.ShowAnswerPanel((byte)((object[])photonEvent.CustomData)[1]);

                break;
            case 2:
                manager.ShowAnswerPanel((byte)((object[])photonEvent.CustomData)[1]);
                
                break;
            case 3:
                ShowEndScreen(playerName);
                break;
        }

    }

    public void ShowEndScreen(string playerName)
    {
        Debug.Log(_photonView.IsMine + "   " + playerName);
        if (!_photonView.IsMine)
            return;

        
        //ADD POINT
        RoomManager.User user1 = RoomManager.instance.players[0];
        RoomManager.User user2 = RoomManager.instance.players[1];

        if (PhotonNetwork.IsMasterClient)
        {
            if(user1.GetTrueAnswer() > user2.GetTrueAnswer())
            {
                int value = PlayerPrefs.GetInt("Point") + 1;
                PlayerPrefs.SetInt("Point", value);
                manager.FinishGame(0);
            }
            else if(user1.GetTrueAnswer() == 0)
            {
                manager.FinishGame(1);
            }
            else if(user1.GetTrueAnswer() == user2.GetTrueAnswer())
            {
                int value = PlayerPrefs.GetInt("Point") + 1;
                Debug.Log("Master : " + value);
                manager.FinishGame(2);
            }
            else
            {
                manager.FinishGame(1);
            }
        }
        else
        {
            if (user1.GetTrueAnswer() < user2.GetTrueAnswer())
            {
                int value = PlayerPrefs.GetInt("Point") + 1;
                PlayerPrefs.SetInt("Point", value);
                manager.FinishGame(0);
            }
            else if (user2.GetTrueAnswer() == 0)
            {
                manager.FinishGame(1);
            }
            else if (user1.GetTrueAnswer() == user2.GetTrueAnswer())
            {
                int value = PlayerPrefs.GetInt("Point") + 1;
                PlayerPrefs.SetInt("Point", value);
                manager.FinishGame(2);
            }
            else
            {
                manager.FinishGame(1);
            }
        }
    }
}
