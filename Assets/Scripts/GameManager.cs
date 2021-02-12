using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject startTimerPanel;
    public TMP_Text player1NameTxt, player2NameTxt, QuestionTxt, Answer1Txt,
        Answer2Txt, Answer3Txt, Answer4Txt, TimerTxt, StartCounterTxt;
    public Image[] P1_InfoBox;
    public Image[] P2_InfoBox;
    public Animator p1answeranim, p2answeranim;
    public Button[] answerButtons;

    [Header("End UI")]
    public GameObject endGamePanel;
    public TMP_Text endInfoTxt, endCounterTxt;

    [Header("Questions")]
    public Question[] questions;
    private List<Question> questionList;
    Player[] playerList;

    [HideInInspector]
    public bool isInGame = false;

    private readonly byte ShowAnswer = 0;
    private readonly byte RightAnswer = 1;
    private readonly byte WrongAnswer = 2;

    private readonly Color blueColor = new Color(0.4f, 0.4f, 0.6f);
    private readonly Color greenColor = new Color(0.1f, 0.8f, 0.1f);
    private readonly Color redColor = new Color(0.8f, 0.1f, 0.1f);

    private int QuestionCounter = -1;

    public static GameManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        questionList = new List<Question>(questions);

        playerList = PhotonNetwork.PlayerList;
    }

    public void PreparePlayers(Player[] players)
    {
        player1NameTxt.text = players[0].NickName;
        player2NameTxt.text = players[1].NickName;
    }

    public void Next()
    {

        QuestionCounter++;

        if(QuestionCounter == 3)
        {
            GameEnd();
            return;
        }

        QuestionTxt.SetText(questionList[QuestionCounter].question);
        Answer1Txt.SetText(questionList[QuestionCounter].answer1.answertxt);
        answerButtons[0].tag = questionList[QuestionCounter].answer1.isTrue ? "true_answer" : "wrong_answer";
        Answer2Txt.SetText(questionList[QuestionCounter].answer2.answertxt);
        answerButtons[1].tag = questionList[QuestionCounter].answer2.isTrue ? "true_answer" : "wrong_answer";
        Answer3Txt.SetText(questionList[QuestionCounter].answer3.answertxt);
        answerButtons[2].tag = questionList[QuestionCounter].answer3.isTrue ? "true_answer" : "wrong_answer";
        Answer4Txt.SetText(questionList[QuestionCounter].answer4.answertxt);
        answerButtons[3].tag = questionList[QuestionCounter].answer4.isTrue ? "true_answer" : "wrong_answer";

        AnswerButtonsSetEnabled(true);

        StopAllCoroutines();
        StartCoroutine(QuestionTimer());
    }

    public void OnAnswerClick(Button button)
    {
        button.GetComponent<Image>().color = blueColor;
        AnswerButtonsSetEnabled(false);
        RaiseAnswerEvent(button.CompareTag("true_answer") ? RightAnswer : WrongAnswer);

    }

    public void RaiseAnswerEvent(byte code)
    {
        string playerName = PlayerPrefs.GetString("PlayerName");

        byte playerID = GetPlayerID(playerName);

        object[] content = null;

        if (code == 0)
        {
            content = new object[] { playerName, playerID, RoomManager.instance.players[playerID].values[QuestionCounter] };

            UpdateScore(playerName, RoomManager.instance.players[playerID].values[QuestionCounter]);
        }
            
        else if(code != 3)
        {
            RoomManager.instance.players[playerID].values[QuestionCounter] = !Convert.ToBoolean(code - 1);

            content = new object[] { playerName, playerID };
        }
        else
        {
            content = new object[] { playerName, playerID };
        }


        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(code, content, raiseEventOptions, sendOptions);
    }

    private IEnumerator QuestionTimer()
    {
        for(int i = 10;i >= 0; i--)
        {
            TimerTxt.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        AnswerButtonsSetEnabled(false);

        RaiseAnswerEvent(ShowAnswer);

        StartCoroutine(WaitStart());
    }

    private IEnumerator WaitStart()
    {
        for (int i = 3; i >= 0; i--)
        {
           UpdateTimer(i);
            yield return new WaitForSeconds(1);
        }

        Next();
    }

    public void UpdateTimer(int time)
    {
        TimerTxt.SetText(time.ToString());
    }

    public void UpdateStartTimer(int time)
    {
        StartCounterTxt.SetText(time.ToString());
    }

    public void StartPanelActive(bool active)
    {
        startTimerPanel.SetActive(active);
        //SHOW TIMER TEXT
        TimerTxt.gameObject.SetActive(!active);
    }

    public void ShowAnswerPanel(byte id)
    {
        if (id == 0)
            p1answeranim.SetTrigger("Fade");
        else
            p2answeranim.SetTrigger("Fade");
    }

    public void UpdateScore(string playerName, bool isTrue)
    {
        if (GetPlayerID(playerName) == 0)
        {
            P1_InfoBox[QuestionCounter].color = isTrue ? greenColor : redColor;
            RoomManager.instance.players[0].values[QuestionCounter] = isTrue;
        }
        else
        {
            P2_InfoBox[QuestionCounter].color = isTrue ? greenColor : redColor;
            RoomManager.instance.players[1].values[QuestionCounter] = isTrue;
        }

        answerButtons[GetTrueAnswer()].GetComponent<Image>().color = greenColor;
      
    }

    byte GetPlayerID(string playerName)
    {
        return ((playerList[0].NickName == playerName) ? (byte)0 : (byte)1);
    }

    public void AnswerButtonsSetEnabled(bool isEnabled)
    {
        for(int i = 0;i < answerButtons.Length; i++)
        {
            answerButtons[i].enabled = isEnabled;

            if(isEnabled) answerButtons[i].GetComponent<Image>().color = new Color(1, 1, 1);
        }
    }

    public int GetTrueAnswer()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i].CompareTag("true_answer"))
            {
                return i;
            }
        }

        return 0;
    }

    public void GameEnd()
    {
        RaiseAnswerEvent(3);
    }

    public void FinishGame(byte state)
    {
        StopAllCoroutines();

        isInGame = false;

        switch (state)
        {
            case 0:
                endInfoTxt.SetText("KAZANDIN !");
                break;
            case 1:
                endInfoTxt.SetText("KAYBETTİN !");
                break;
            case 2:
                endInfoTxt.SetText("BERABERE !");
                break;
        }
        

        endGamePanel.SetActive(true);

        StartCoroutine(EndCounter());
    }

    IEnumerator EndCounter()
    {
        for (int i = 10; i >= 0; i--)
        {
            endCounterTxt.SetText("" + i);
            yield return new WaitForSeconds(1);
        }

        ReturnMainMenu();
    }

    public void ReturnMainMenu()
    {
        StopAllCoroutines();

        if (PhotonNetwork.IsMasterClient)
        {
            Destroy(GameObject.Find(RoomManager.instance.players[0].PlayerName));
            Destroy(GameObject.Find(RoomManager.instance.players[1].PlayerName));
        }

        Destroy(RoomManager.instance.gameObject);

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

}
