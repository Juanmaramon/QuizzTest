using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour 
{
	public Text answerText;
	private AnswerData answerData;

	void Start () 
	{
				
	}
	
	public void Setup(AnswerData data)
	{
		answerData = data;
		answerText.text = answerData.AnswerText;
	}

	public void HandleClick()
	{
		// Send event to game controller with answer
		EventManager.TriggerEvent (Constants.ON_ANSWER_CLICK_DONE, new BasicEvent(answerData.IsCorrect));
	}
}