using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
	// Difficulty levels
    public enum Levels
    {
        BASIC = 0,
        MEDIUM,
        HARD,
    }

	// Numbers of rounds and questions per round
	public const int NUM_ROUNDS = 2;
	public const int QUESTIONS_PER_ROUND = 6;
	// When level check will be performed for the dynamic level system
	public const int NUM_QUESTION_CHECK_LEVEL = 3;

	// Game Events
	public const string ON_CURRENT_DATA_RECEIVED = "OnCurrentDataReceived";
	public const string ON_ANSWER_CLICK_DONE = "OnAnswerClickDone";
	public const string IS_LAST_ROUND = "IsLastRound";
	public const string LAST_ROUND_ANSWER = "LastRoundAnswer";
	public const string ON_REMOTE_DATA_RECEIVED = "OnRemoteDataReceived";
	public const string ON_SEND_DATA_TO_DATABASE = "OnSendDataToDatabase";

	// JSON with quizz data
	public const string JSON_FILE = "quiz.json";
}
