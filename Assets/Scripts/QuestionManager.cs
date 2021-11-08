using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;

public enum Answers
{
    A,
    B,
    C,
    D
}

public class QuestionManager : Singleton<QuestionManager>
{
    // Start is called before the first frame update
    private void Start()
    {
        GenerateQuestions();

        foreach (var question in questionList)
        {
            print(question.question);
        }
    }

    public int questionCounter = -1;
    private List<Question> questionList = new List<Question>();
    [Header("UI")]
    public TMP_Text questionText;
    public TMP_Text[] answerTexts;
    public Button[] answerButtons;

    public void NextQuestion()
    {
        questionText.SetText(questionList[questionCounter].question);
        for (int i = 0, n = questionList[questionCounter].answerTexts.Length; i < n; i++)
        {
            answerTexts[i].SetText(questionList[questionCounter].answerTexts[i]);
        }
        
        AnswerButtonsEnabled(true);
    }

    public void AnswerButtonsEnabled(bool isEnabled)
    {
        foreach (var selectedButton in answerButtons)
        {
            selectedButton.interactable = isEnabled;
            if(isEnabled) selectedButton.GetComponent<Image>().color = GlobalResources.NormalButtonColor;
        }
    }

    public void ShowGivingAnswer(Button button)
    {
        button.GetComponent<Image>().color = GlobalResources.AnsweredColor;
    }
    
    public Answers GetTrueAnswer()
    {
        return questionList[questionCounter].trueAnswer;
    }

    public bool AnswerIsTrue(Answers answer)
    {
        return answer == GetTrueAnswer();
    }

    public void ShowTrueAnswer()
    {
        answerButtons[(int)GetTrueAnswer()].GetComponent<Image>().color = GlobalResources.TrueAnswerColor;
    }

    #region GENERATE QUESTIONS

    private void GenerateQuestions()
    {
        var readedString = ReadString();
        var allQuestionList = DeserializeString(readedString);

        for (int i = 0; i < 3; i++)
        {
            questionList.Add(allQuestionList[Random.Range(allQuestionList.Count / 3 * i, allQuestionList.Count / 3 * (i + 1))]);
        }
    }

    private List<Question> DeserializeString(string deserializedText)
    {
        var tempList = new List<Question>();
        var splittedText = deserializedText.Split('\n');

        for (int i = 0, n = splittedText.Length; i < n; i+= 3)
        {
            var answers = splittedText[i + 1].Split('/');
            tempList.Add(new Question(splittedText[i], answers, splittedText[i + 2][0] - 'A'));
        }

        return tempList;
    }
    
    private static string ReadString()
    {
        string path = "Assets/questions.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string text = reader.ReadToEnd();
        reader.Close();
        return text;
    }

    #endregion
}
