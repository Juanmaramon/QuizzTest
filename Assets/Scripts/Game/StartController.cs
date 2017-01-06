using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour {

	public Button startGameButton;
	public Button playWithUserButton;
	public Text playWithUserText;
	public Text orText;
	public InputField nameInput;
	private string userName;

	void Start()
	{
		startGameButton.interactable = false;

		UserLocalData user = UserLocal.ReadUserData ();
		// If user exists, is possible to play again with this user
		if (user != null) 
		{
			playWithUserText.text = "Play with " + user.Name;
			playWithUserButton.gameObject.SetActive (true);
			orText.enabled = true;
		}
		else
		{
			playWithUserButton.gameObject.SetActive (false);
			orText.enabled = false;
		}
	}

	public void EndEditName()
	{
		if (!string.IsNullOrEmpty (nameInput.text)) 
		{
			startGameButton.interactable = true;	
		} 
		else 
		{
			startGameButton.interactable = false;
		}
	}

	public void PushedPlayButton()
	{
		userName = nameInput.text;
		UserLocal.SaveUserData (userName, 0);
		SceneManager.LoadScene ("GameTest");
	}

	public void PushedPlayWithButton()
	{
		SceneManager.LoadScene ("GameTest");
	}
}