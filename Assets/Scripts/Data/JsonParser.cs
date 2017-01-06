using UnityEngine;
using System;
using System.Collections.Generic;
using MiniJSON;

public class JsonParser
{
    private string dataFile;
    private Dictionary<string, object> json;
    private string levelNode;
    private List<object> levelNodeProcessed;
    private string questionData;
    private Dictionary<string, object> questionDataProcessed;

	// List with all questions => from 0,QUESTIONS_PER_ROUND => BASIC...
	private List<QuestionData> questionResult = new List<QuestionData>();

    public JsonParser(string jsonData)
    {
		dataFile = jsonData;
    }

	public List<QuestionData> Parse()
    {
        json = Json.Deserialize(dataFile) as Dictionary<string, object>;

        var levels = EnumUtil.GetNames<Constants.Levels>();

        // All levels ...
        for (int i = 0; i < levels.Length; i++)
        {
            ParseLevel(levels[i]);
        }

		return questionResult;
    }

    private void ParseLevel(string level)
    {
        levelNode = Json.Serialize(json[level.ToLower()]);
        levelNodeProcessed = Json.Deserialize(levelNode) as List<object>;

        // All questions ...
        foreach (object element in levelNodeProcessed)
        {
            questionData = Json.Serialize(element);
            ParseQuestion(questionData);
        }
    }

    private void ParseQuestion(string questionData)
    {
        questionDataProcessed = Json.Deserialize(questionData) as Dictionary<string, object>;
		QuestionData question = new QuestionData ();
		foreach (KeyValuePair<string, object> par in questionDataProcessed)
        {
            // Question data parsing goes here
            // Load question container
            switch (par.Key.ToString())
            {
                case "id":
                    break;
				case "question":
					question.QuestionText = par.Value.ToString();
                    break;
				case "answers":
					question.Answers = new List<AnswerData> ();
					foreach (object val in (List<object>)par.Value)
					{
						AnswerData answer = new AnswerData ();
						answer.AnswerText = val.ToString ();
						question.Answers.Add(answer);
					}
                    break;
				case "correct":
					int i = 0;
					foreach (object val in (List<object>)par.Value)
					{
						question.Answers [i].IsCorrect = bool.Parse(val.ToString());
						i++;
					}					
                    break;
				case "timeLimit":
					question.TimeLimit = float.Parse (par.Value.ToString());
					break;
				case "points":
					question.PointsAdded = int.Parse (par.Value.ToString());
					break;
            }
        }

		questionResult.Add (question);
    }
   
}
