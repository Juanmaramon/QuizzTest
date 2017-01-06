using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
	public Text userNameText;
	public Text questionText;
	public Text scoreText;
	public Text timeText;
	public Text roundText;
	public Text markerText;
	public Text winText;
	public Text loseText;
	public SCFade qaPannelFade;
	public SCFade roundsPannelFade;
	public SCFade overPannelFade;
	public SimpleObjectPool answerButtonObjectPool;
	public Transform answerButtonParent;
	private RoundData currentRoundData;
	private int currentRoundNumber = -1;
	private QuestionData[] questionPool;
	private bool isRoundActive;
	private float timeRemaining;
	private int questionNumber;
	private int nextQuestionIndex;
	private int playerScore;
	private List<GameObject> answerButtonGameObjects = new List<GameObject>();
	private QuestionData questionData;
	private int corrects;
	private int correctsOnChunk;
	private int errors;
	private bool win;
	private Constants.Levels currentLevel;
	private UserLocalData user;

	void Start()
	{
		// Listen to events
		EventManager.StartListening<BasicEvent> (Constants.ON_CURRENT_DATA_RECEIVED, OnCurrentDataReceived);
		EventManager.StartListening<BasicEvent> (Constants.ON_ANSWER_CLICK_DONE, OnAnswerClickDone);
		EventManager.StartListening<BasicEvent> (Constants.LAST_ROUND_ANSWER, OnLastRoundAnswer);
		playerScore = 0;
		questionNumber = 0;
		currentRoundNumber = -1;
		isRoundActive = false;
		corrects = correctsOnChunk = 0;
		errors = 0;
		markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
		user = UserLocal.ReadUserData ();
		// Show user name
		userNameText.text = user.Name;
		win = true;
		currentLevel = user.Level;

		// Show Round #0, only for first round
		if (currentRoundNumber == -1) 
		{
			StartGame ();
		}
	}

	void OnDisable()
	{
		EventManager.StopListening<BasicEvent> (Constants.ON_CURRENT_DATA_RECEIVED, OnCurrentDataReceived);
		EventManager.StopListening<BasicEvent> (Constants.ON_ANSWER_CLICK_DONE, OnAnswerClickDone);
		EventManager.StopListening<BasicEvent> (Constants.LAST_ROUND_ANSWER, OnLastRoundAnswer);
	}

	private void OnCurrentDataReceived(BasicEvent e)
	{
		currentRoundData = (RoundData)e.Data;
		questionPool = currentRoundData.Questions;

		timeRemaining = questionPool [0].TimeLimit;

		// After 2 secs show questions, start the game!
		Invoke("StartRound", 2f);
		// Show first question
		Invoke("ShowQuestion", 2f);
	}

	private void OnAnswerClickDone(BasicEvent e)
	{
		bool isCorrect = (bool)e.Data;
		
		AnswerButtonClicked (isCorrect);
	}

	private void OnLastRoundAnswer(BasicEvent e)
	{
		bool isLastRound = (bool)e.Data;

		// End game, show results
		if (isLastRound) 
		{
			timeText.text = "0";
			markerText.enabled = false;
			isRoundActive = false;
			overPannelFade.StartFadeIn (0.3f);
			qaPannelFade.StartFadeOut (0.3f);
			roundsPannelFade.StartFadeOut (0.3f);

			// Save last score
			user.LastScore = playerScore;
			UserLocal.SaveUserData (user);

			// Send score to database
			EventManager.TriggerEvent (Constants.ON_SEND_DATA_TO_DATABASE, new BasicEvent(user));

			// Show proper feedback
			if (win) 
			{
				winText.enabled = true;
				loseText.enabled = false;
			} 
			else 
			{
				winText.enabled = false;
				loseText.enabled = true;
			}
		}
	}

	private void RemoveAnswerButtons()
	{
		// Remove old answer buttons
		while (answerButtonGameObjects.Count > 0) 
		{
			answerButtonObjectPool.ReturnObject (answerButtonGameObjects[0]);
			answerButtonGameObjects.RemoveAt (0);
		}
	}

	private void ShowQuestion()
	{
		// First remove old answer buttons
		RemoveAnswerButtons ();
		// Get question from pool
		questionData = questionPool [nextQuestionIndex];
		questionText.text = questionData.QuestionText;
		timeRemaining = questionData.TimeLimit;

		// Create N answer buttons
		for (int i = 0; i < questionData.Answers.Count; i++) 
		{
			GameObject answerButtonGameObject = answerButtonObjectPool.GetObject ();
			answerButtonGameObject.transform.SetParent (answerButtonParent, false);
			answerButtonGameObjects.Add (answerButtonGameObject);
			AnswerButton answerButton = answerButtonGameObject.GetComponent<AnswerButton> ();
			answerButton.Setup (questionData.Answers [i]);
		}
	}

	public void AnswerButtonClicked(bool isCorrect)
	{
		// Was correct?
		if (isCorrect) 
		{
			corrects++;
			correctsOnChunk++;
			// Show marker
			markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
			// Add score to user
			playerScore += questionData.PointsAdded;
			scoreText.text = playerScore.ToString ();
		}
		else 
		{
			errors++;
		}

		// Level check done on 1/2 questions and last question of every round (chunk)
		if ((questionNumber != 0) && (((questionNumber + 1) % Constants.NUM_QUESTION_CHECK_LEVEL) == 0)) 
		{
			// If this chunk of question was done perfect, level up
			if (correctsOnChunk == Constants.NUM_QUESTION_CHECK_LEVEL) 
			{
				// Level up ...
				LevelUp();
			} 
			// If only one or less correct answer, level down
			else if (correctsOnChunk <= 0) 
			{
				// Level down ...
				LevelDown();
			}

			// Reset to check in next chunk
			correctsOnChunk = 0;
		}

		// Get next question or end round
		NextQuestionOrEnd ();
	}

	private void LevelUp()
	{
		switch (currentLevel) 
		{
			default:
				currentLevel = (Constants.Levels)(currentLevel + 1);
				break;
			case Constants.Levels.HARD:
				break;
		}

		// Save new level
		user.Level = currentLevel;
		UserLocal.SaveUserData (user);
	}

	private void LevelDown()
	{
		switch (currentLevel) 
		{
			default:
				currentLevel = (Constants.Levels)(currentLevel - 1);
				break;
			case Constants.Levels.BASIC:
				break;
		}

		// Save new level
		user.Level = currentLevel;
		UserLocal.SaveUserData (user);
	}

	private void NextQuestionOrEnd()
	{
		// Wasn't last question of current round?
		if (Constants.QUESTIONS_PER_ROUND > (questionNumber + 1)) 
		{
			// Will get next question
			questionNumber++;
			GetNextQuestionIndex();
			ShowQuestion ();
		}
		// Was last question
		else 
		{
			// End round
			EndRound();
		}
	}

	private void GetNextQuestionIndex()
	{
		// Get question index on question pool based on user's level
		// First 6 questions => BASIC
		// 6 on the middle => MEDIUM
		// the last 6 => HARD
		switch (currentLevel) 
		{
			case Constants.Levels.BASIC:
				nextQuestionIndex = questionNumber;
				break;
			case Constants.Levels.MEDIUM:
				nextQuestionIndex = questionNumber + Constants.QUESTIONS_PER_ROUND;
				break;
			case Constants.Levels.HARD:
				nextQuestionIndex = questionNumber + (Constants.QUESTIONS_PER_ROUND * 2);
				break;
		}
	}

	public void EndRound()
	{
		// Resets question index
		questionNumber = 0;
		GetNextQuestionIndex ();

		// If user is winning and corrects are more than 5, errors less than 3
		if (currentRoundNumber != -1) 
		{
			if (win && (corrects >= 5) && (errors < 3)) 
			{
				win = true;
			} 
			else 
			{
				win = false;
			}
		}

		// Next round
		currentRoundNumber++;

		// Show round pannel
		roundText.text = currentRoundNumber.ToString();
		isRoundActive = false;
		qaPannelFade.StartFadeOut (0f);
		roundsPannelFade.StartFadeIn (0.3f);
		overPannelFade.StartFadeOut (0f);
	
		// Is last round?
		EventManager.TriggerEvent (Constants.IS_LAST_ROUND);
	}

	private void StartGame()
	{
		EndRound ();
	}

	private void StartRound()
	{
		// Show Q&A pannel and restart marker
		corrects = errors = 0;
		markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
		roundText.text = currentRoundNumber.ToString();
		isRoundActive = true;
		qaPannelFade.StartFadeIn (0.3f);
		roundsPannelFade.StartFadeOut (0.3f);
		overPannelFade.StartFadeOut (0.3f);

	}

	public void RestartGame()
	{
		SceneManager.LoadScene ("StartTest");
	}

	private void UpdateTimeRemaing ()
	{
		timeText.text = Mathf.Round(timeRemaining).ToString();

		// If time is over, next question or end
		if (timeRemaining <= 0f) 
		{
			errors++;
			NextQuestionOrEnd ();
		}
	}

	void Update()
	{
		// Update time if there is a question
		if (isRoundActive) 
		{
			timeRemaining -= Time.deltaTime;
			UpdateTimeRemaing ();
		}
	}
}
