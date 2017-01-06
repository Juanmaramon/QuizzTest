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
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class RemoteStorage : MonoBehaviour {
	private string MyStorageBucket = "gs://quizz-6ffb1.appspot.com";
	private const int kMaxLogSize = 16382;

	private string firebaseStorageLocation;
	private string fileContents;

	public string FileContents
	{
		get { return fileContents; }
	}

	private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

	// When the app starts, check to make sure that we have
	// the required dependencies to use Firebase, and if not,
	// add them if possible.
	void Start() {
		dependencyStatus = FirebaseApp.CheckDependencies();
		if (dependencyStatus != DependencyStatus.Available) {
			FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
				dependencyStatus = FirebaseApp.CheckDependencies();
				if (dependencyStatus != DependencyStatus.Available) {
					// This should never happen if we're only using Firebase Analytics.
					// It does not rely on any external dependencies.
					Debug.LogError(
						"Could not resolve all Firebase dependencies: " + dependencyStatus);
				}
			});
		}

		var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
		if (!String.IsNullOrEmpty(appBucket)) {
			MyStorageBucket = String.Format("gs://{0}/", appBucket);
		}

		// Set Remote storage pathfile
		firebaseStorageLocation = MyStorageBucket + '/' + Constants.JSON_FILE;
		// Wait to get data from Remote storage (FireBase)
		Invoke ("DownloadRemoteData", .3f);
	}

	void DownloadRemoteData()
	{
		StartCoroutine (DownloadFromFirebaseStorage());
	}

	// Output text to the debug log text field, as well as the console.
	public void DebugLog(string s) {
		Debug.Log(s);
	}
		
	IEnumerator UploadToFirebaseStorage() {
		StorageReference reference = FirebaseStorage.DefaultInstance
			.GetReferenceFromUrl(firebaseStorageLocation);
		var task = reference.PutBytesAsync(Encoding.UTF8.GetBytes(fileContents));
		yield return new WaitUntil(() => task.IsCompleted);
		if (task.IsFaulted) {
			DebugLog(task.Exception.ToString());
		} else {
			fileContents = "";
			DebugLog("Finished uploading... Download Url: " + task.Result.DownloadUrl.ToString());
			DebugLog("Press the Download button to download text from Firebase Storage");
		}
	}

	IEnumerator DownloadFromFirebaseStorage() {
		StorageReference reference = FirebaseStorage.DefaultInstance
			.GetReferenceFromUrl(firebaseStorageLocation);
		var task = reference.GetBytesAsync(1024 * 1024);
		yield return new WaitUntil(() => task.IsCompleted);
		if (task.IsFaulted) {
			DebugLog(task.Exception.ToString());
		} else {
			fileContents = Encoding.UTF8.GetString(task.Result);
			//DebugLog("Finished downloading...");
			//DebugLog("Contents=" + fileContents);
			// Send quizz data to Data Controller
			EventManager.TriggerEvent (Constants.ON_REMOTE_DATA_RECEIVED, new BasicEvent (fileContents));
		}
	}
}
