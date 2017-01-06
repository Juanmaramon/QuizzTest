using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionData 
{
	string questionText;
	float timeLimit;
	int pointsAdded;
	List<AnswerData> answers;

	public string QuestionText
	{
		get { return questionText; }
		set { questionText = value; }
	}

	public float TimeLimit
	{
		get { return timeLimit; }
		set { timeLimit = value; }
	}

	public int PointsAdded
	{
		get { return pointsAdded; }
		set { pointsAdded = value; }
	}

	public List<AnswerData> Answers
	{
		get { return answers; }
		set { answers = value; }
	}
}
