// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// Performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class RemoteDatabase : MonoBehaviour {

	ArrayList leaderBoard;

	// For this example only allow to store 30 highscores
	private const int MaxScores = 30;
	private new string name = "";
	private int score = 100;

	const int kMaxLogSize = 16382;
	DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

	// When the app starts, check to make sure that we have
	// the required dependencies to use Firebase, and if not,
	// add them if possible.
	void Start() {
		dependencyStatus = FirebaseApp.CheckDependencies();
		if (dependencyStatus != DependencyStatus.Available) {
			FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
				dependencyStatus = FirebaseApp.CheckDependencies();
				if (dependencyStatus == DependencyStatus.Available) {
					InitializeFirebase();
				} else {
					// This should never happen if we're only using Firebase Analytics.
					// It does not rely on any external dependencies.
					Debug.LogError(
						"Could not resolve all Firebase dependencies: " + dependencyStatus);
				}
			});
		} else {
			InitializeFirebase();
		}

		EventManager.StartListening<BasicEvent> (Constants.ON_SEND_DATA_TO_DATABASE, OnSendDataToDatabase);
	}

	void OnDisable()
	{
		EventManager.StopListening<BasicEvent> (Constants.ON_SEND_DATA_TO_DATABASE, OnSendDataToDatabase);
	}

	// Initialize the Firebase database:
	void InitializeFirebase() {
		FirebaseApp app = FirebaseApp.DefaultInstance;
		app.SetEditorDatabaseUrl("https://quizz-6ffb1.firebaseio.com/");

		leaderBoard = new ArrayList();
		leaderBoard.Add("Firebase Top " + MaxScores.ToString() + " Scores");
		FirebaseDatabase.DefaultInstance
			.GetReference("Leaders").OrderByChild("score")
			.ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
			if (e2.DatabaseError != null) {
				Debug.LogError(e2.DatabaseError.Message);
				return;
			}
			string title = leaderBoard[0].ToString();
			leaderBoard.Clear();
			leaderBoard.Add(title);
			if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0) {
				foreach (var childSnapshot in e2.Snapshot.Children) {
					leaderBoard.Insert(1, childSnapshot.Child("score").Value.ToString()
						+ "  " + childSnapshot.Child("name").Value.ToString());
				}
			}
		};
	}

	// Output text to the debug log text field, as well as the console.
	public void DebugLog(string s) {
		Debug.Log(s);
	}

	// A realtime database transaction receives MutableData which can be modified
	// and returns a TransactionResult which is either TransactionResult.Success(data) with
	// modified data or TransactionResult.Abort() which stops the transaction with no changes.
	TransactionResult AddScoreTransaction(MutableData mutableData) {
		List<object> leaders = mutableData.Value as List<object>;

		if (leaders == null) {
			leaders = new List<object>();
		} else if (mutableData.ChildrenCount >= MaxScores) {
			// If the current list of scores is greater or equal to our maximum allowed number,
			// we see if the new score should be added and remove the lowest existing score.
			long minScore = long.MaxValue;
			object minVal = null;
			foreach (var child in leaders) {
				if (!(child is Dictionary<string, object>))
					continue;
				long childScore = (long)((Dictionary<string, object>)child)["score"];
				if (childScore < minScore) {
					minScore = childScore;
					minVal = child;
				}
			}
			// If the new score is lower than the current minimum, we abort.
			if (minScore > score) {
				//return TransactionResult.Abort();
			}
			// Otherwise, we remove the current lowest to be replaced with the new score.
			leaders.Remove(minVal);
		}

		// Now we add the new score as a new entry that contains the name and score.
		Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
		newScoreMap["score"] = score;
		newScoreMap["name"] = name;
		leaders.Add(newScoreMap);

		// You must set the Value to indicate data at that location has changed.
		mutableData.Value = leaders;
		return TransactionResult.Success(mutableData);
	}

	private void OnSendDataToDatabase(BasicEvent e)
	{
		UserLocalData user = (UserLocalData)e.Data;

		AddScore (user.Name, user.LastScore);
	}

	public void AddScore(string name, int score) {
		if (score == 0 || string.IsNullOrEmpty(name)) {
			DebugLog("invalid score or name.");
			return;
		}
		//DebugLog(String.Format("Attempting to add score {0} {1}",
		//	name, score.ToString()));

		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("HighScores");

		this.name = name;
		this.score = score;

		//DebugLog("Running Transaction...");
		// Use a transaction to ensure that we do not encounter issues with
		// simultaneous updates that otherwise might create more than MaxScores top scores.
		reference.RunTransaction(AddScoreTransaction)
			.ContinueWith(task => {
				if (task.Exception != null) {
					DebugLog(task.Exception.ToString());
				} else if (task.IsCompleted) {
					DebugLog("Transaction complete.");
				}
			});
	}

}
