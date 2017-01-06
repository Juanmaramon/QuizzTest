using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerData 
{
	string answerText;
	bool isCorrect;

	public string AnswerText
	{
		get { return answerText; }
		set { answerText = value; }
	}

	public bool IsCorrect
	{
		get { return isCorrect; }
		set { isCorrect = value; }
	}
}
