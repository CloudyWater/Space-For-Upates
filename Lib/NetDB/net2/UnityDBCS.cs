using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// [iBoxDB.net2.dll]
using iBoxDB.LocalServer;

public class UnityDBCS : MonoBehaviour
{

	public DB server = null;
	public DB.AutoBox db = null;

	void Start ()
	{
		if (db == null) {

			DB.Root (Application.persistentDataPath);
#if (UNITY_METRO || NETFX_CORE) && (!UNITY_EDITOR)
			// from WSDatabaseConfig.cs
			iBoxDB.WSDatabaseConfig.ResetStorage();
#endif
			Xamarin_IL2CPP_MakeSureClassesAreLinked ();

			server = new DB (3);

			// two tables(Players,Items) and their keys(ID,Name)
			server.GetConfig ().EnsureTable<Player> ("Players", "ID");
			//autoIncrementID use  (int)db.Id (1, 1,false)  or (long)db.NewId(1,1,false)

			// max length is 20 , default is 32
			server.GetConfig ().EnsureTable<Item> ("Items", "Name(20)");

			// if device has small memory & disk [option]
			server.MinConfig ();

			db = server.Open ();

		}

		if (db.SelectCount ("from Items") == 0) {

			// insert player's score to database 
			var player = new Player{
    Name = "Player_" + (int)Time.realtimeSinceStartup,
    Score = DateTime.Now.Second + (int)Time.realtimeSinceStartup + 1,
    ID = db.Id(1)
};
			db.Insert ("Players", player);


			//dynamic data, each object has different properties
			var shield = new Item () { Name = "Shield", Position = 1 };
			shield ["attributes"] = new string[] { "earth" };
			db.Insert ("Items", shield);


			var spear = new Item () { Name = "Spear", Position = 2 };
			spear ["attributes"] = new string[] { "metal", "fire" };
			spear ["attachedSkills"] = new string[] { "dragonFire" };
			db.Insert ("Items", spear);


			var composedItem = new Item () { Name = "ComposedItem", Position = 3, XP = 0 };
			composedItem ["Source1"] = "Shield";
			composedItem ["Source2"] = "Spear";
			composedItem ["level"] = 0;
			db.Insert ("Items", composedItem);

		}

		DrawToString ();
	}

	void DrawToString ()
	{
		_context = "";
		//SQL-like Query
		foreach (Item item in db.Select<Item>("from Items order by Position")) {
			string s = DB.ToString (item);
			if (item.Name == "ComposedItem") {
				s += " XP=" + item.XP;
			}
			s += "\r\n\r\n";
			_context += Format (s);
		}
		_context += "Players \r\n";
		foreach (Player player in db.Select<Player>("from Players where Score >= ? order by Score desc", 0)) {
			_context += player.Name + " Score:" + player.Score + "\r\n";
		}
	}

	private string _context;

	void OnGUI ()
	{
		if (GUI.Button (new Rect (0, 0, Screen.width / 2, 50), "NewScore")) {

			long sequence = db.NewId (0, 1);
			var player = new Player
{
    Name = "Player_" + sequence,
    Score = DateTime.Now.Second + 1,
    ID = db.Id(1)
};
			db.Insert ("Players", player);

			DrawToString ();
		}
		if (GUI.Button (new Rect (Screen.width / 2, 0, Screen.width / 2, 50), "LevelUp")) {

			// use ID to read item from db then update <level> and <experience points> 
			var composedItem = db.SelectKey<Item> ("Items", "ComposedItem");
			composedItem.XP = (long)(Time.fixedTime * 100);
			composedItem ["level"] = (int)composedItem ["level"] + 1;
			db.Update ("Items", composedItem);

			DrawToString ();
		}
		String exinfo = IntPtr.Size == 4 ? "32bit" : "64bit";
		exinfo += (" Path:\r\n" + Application.persistentDataPath);
		GUI.Box (new Rect (0, 50, Screen.width, Screen.height - 50), "\r\n" + _context +
			"\r\n" + exinfo);
	}

	//A Player, Normal class 
	public class Player
	{
		public long ID;
		public string Name;
		public int Score;
	}

	// An Item, Dynamic class  
	public class Item : Dictionary<string, object>
	{
		public string Name {
			get {
				return (string)base ["Name"];
			}
			set {
				if (value.Length > 20) {
					throw new ArgumentOutOfRangeException ();
				}
				base ["Name"] = value;
			}
		}

		public int Position {
			get {
				return (int)base ["Position"];
			}
			set {
				base ["Position"] = value;
			}
		}
		//encrypt experience points 
		public long XP {
			get {
				object ot;
				if (!base.TryGetValue ("_xp", out ot)) {
					return 0;
				}
				string t = ot as string;
				t = t.Replace ("fakeData", "");
				return Int64.Parse (t);
			}
			set {
				var t = "fakeData" + value;
				base ["_xp"] = t;
			}
		}
	}

	string Format (string s)
	{
		int pos = s.IndexOf (',', s.IndexOf (',', 0) + 1);
		if (pos > 0) {
			s = s.Substring (0, pos + 1) + "\r\n" + Format (s.Substring (pos + 1));
		}
		return s;
	}

	private static void Xamarin_IL2CPP_MakeSureClassesAreLinked ()
	{
#if !NETFX_CORE && !UNITY_METRO && !WINDOWS_PHONE && !UNITY_WP8 && !UNITY_EDITOR
		if (DateTime.Now.Ticks == 0) {
			//ensure the methods 
			new System.IO.FileStream ("", System.IO.FileMode.OpenOrCreate,
               System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 1);
			new System.IO.FileStream ("", System.IO.FileMode.OpenOrCreate,
                System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 1, false);
			//new System.IO.FileStream("", System.IO.FileMode.OpenOrCreate,
			//    System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 1, System.IO.FileOptions.None);
			System.IO.File.Delete (System.IO.FileAccess.ReadWrite.ToString ());
			System.IO.File.Exists (System.IO.FileShare.ReadWrite.ToString ());
			System.String.Intern (System.IO.FileOptions.None.ToString ());
			System.Threading.Thread.Sleep (System.IO.FileMode.OpenOrCreate.GetHashCode ());
		}
#endif
	}

}
