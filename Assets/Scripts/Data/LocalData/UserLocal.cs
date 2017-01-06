using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Reflection;

public class UserLocal : MonoBehaviour 
{
	private static string fileUserLocal = "userLocal.sal";

	public static void SaveUserData(string userName, int lastScore, Constants.Levels level = Constants.Levels.BASIC)
	{
		// Avoid some serialization problems on iOS
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		} 

		// Allow serialization
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;
		UserLocalData user = new UserLocalData ();
		user.SetData (userName, lastScore, level);

		// Check user exists 
		if (File.Exists(Application.persistentDataPath + "/" + fileUserLocal)) 
		{
			// If exists, delete it ...
			File.Delete(Application.persistentDataPath + "/" + fileUserLocal);
		}

		// Create new file
		file = File.Create(Application.persistentDataPath + "/" + fileUserLocal);
		file.Close();

		// Write data
		file = File.Open(Application.persistentDataPath + "/" + fileUserLocal, FileMode.Open);
		bf.Serialize(file, user);	
		file.Close();
	}

	public static void SaveUserData(UserLocalData user)
	{
		// Avoid some serialization problems on iOS
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		} 
			
		// Allow serialization
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;

		// Check user exists 
		if (File.Exists(Application.persistentDataPath + "/" + fileUserLocal)) 
		{
			// If exists, delete it ...
			File.Delete(Application.persistentDataPath + "/" + fileUserLocal);
		}

		// Create new file
		file = File.Create(Application.persistentDataPath + "/" + fileUserLocal);
		file.Close();

		// Write data
		file = File.Open(Application.persistentDataPath + "/" + fileUserLocal, FileMode.Open);
		bf.Serialize(file, user);	
		file.Close();
	}

	public static UserLocalData ReadUserData()
	{
		// Avoid some serialization problems on iOS
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		} 

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;

		// If file exists return user
		if (File.Exists (Application.persistentDataPath + "/" + fileUserLocal)) 
		{
			file = File.Open(Application.persistentDataPath + "/" + fileUserLocal, FileMode.Open);
			UserLocalData user = (UserLocalData)bf.Deserialize(file);
			file.Close();			

			return user;
		}

		return null;
	}
}
