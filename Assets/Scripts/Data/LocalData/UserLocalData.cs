using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class UserLocalData 
{
	private string name;
	private int lastScore;
	private Constants.Levels level;

	public UserLocalData()
	{
		this.name = "";
		this.lastScore = 0;
		this.level = Constants.Levels.BASIC;
	}

	public void SetData(string userName, int userLastScore, Constants.Levels userLevel)
	{
		this.name = userName;
		this.lastScore = userLastScore;
		this.level = userLevel;
	}

	public string Name
	{
		get { return name; }
	}

	public int LastScore
	{
		get { return lastScore; }
		set { lastScore = value; }
	}

	public Constants.Levels Level
	{
		get { return level; }
		set { level = value; }
	}
}
