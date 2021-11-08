using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string question;
    public string[] answerTexts;
    public Answers trueAnswer;

    public Question(string question, string[] answers, int trueAnswer)
    {
        this.question = question;
        this.answerTexts = answers;
        this.trueAnswer = (Answers)trueAnswer;
    }
}
