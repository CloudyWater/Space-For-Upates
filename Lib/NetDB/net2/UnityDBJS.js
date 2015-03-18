#pragma strict

import System;

// [iBoxDB.net2.dll]
import iBoxDB.LocalServer;

public var server : DB; 
public var db : DB.AutoBox; 

function Start () {
    if (db == null) { 
        // load from Assets/Resources/db2.bytes , readonly
        //var tx:TextAsset = UnityEngine.Resources.Load( "db2" );
        // var bx1 = DB.Create(tx.bytes ).Open();
     
        DB.Root( Application.persistentDataPath );
        server = DB.Create( 2 );
		
        // two tables(Players,Items) and their keys(ID,Name)
        server.GetConfig().EnsureTable("Players", { "ID":0 });
        //autoIncrementID use  (int)db.Id (1, 1,false)  or (long)db.NewId(1,1,false)
  
        // max length is 20 , default is 32
        server.GetConfig().EnsureTable("Items", {"Name(20)":"" });
				
        // if device has small memory & disk [option]
        //server.MinConfig(); 			
		
        db = server.Open (0); 
    }
    if (db.SelectCount ("from Items") == 0) {
			    
        // insert player's score to database 
        db.Insert ("Players", DB.Convert( { 
            "Name" : "Player_" + parseInt( Time.realtimeSinceStartup ) ,
            "Score": DateTime.Now.Second + parseInt(Time.realtimeSinceStartup) + 1 ,
            "ID" : db.Id(1,1,false)
        }));
			

        var shield = { "Name": "Shield", "Position":1 };
        shield["attributes"] = [ "earth" ];				
        db .Insert ("Items", DB.Convert( shield));
			
			
        var spear = { "Name": "Spear" , "Position":2  };
        spear["attributes"] = ["metal" , "fire" ];
        spear["attachedSkills"] = [  "dragonFire"  ];
        db .Insert ("Items", DB.Convert( spear));
			
			
        var composedItem = { "Name": "ComposedItem" , "Position":3 , "XP":0 };
        composedItem["Source1"] = "Shield";
        composedItem["Source2"] = "Spear";
        composedItem["level"] = 0;
        db .Insert ("Items", DB.Convert( composedItem));							 
    }
    DrawToString ();	
}

function DrawToString ()
{
    _context = "";
    //SQL-like Query
    for (var item in db.Select( "from Items order by Position")) { 
        var s = DB.ToString (item); 
        s += "\r\n\r\n";			
        _context += Format (s);
    }
    _context += "Players \r\n";
    for (var player in db.Select( "from Players where Score >= ? order by Score desc" , 0 )) {
        _context += player["Name"] + " Score:" + player["Score"] + "\r\n";
    }
}
private var _context : String;


function OnGUI ()
{
    if (GUI.Button (new Rect (0, 0, Screen.width / 2, 50), "NewScore")) {
		
        var sequence = db.Id (0,1,false); 
        var player =  {  "Name" : "Player_" + sequence, 
            "Score": DateTime.Now.Second + 1 , "ID" : db.Id(1,1,false) };
        db.Insert ("Players", DB.Convert( player )); 
		
        DrawToString ();
    }
    if (GUI.Button (new Rect (Screen.width / 2, 0, Screen.width / 2, 50), "LevelUp")) {
		
        // use ID to read item from db then update <level> and <experience points> 
        var composedItem = db.SelectKey("Items", "ComposedItem").Clone();
        composedItem["XP"] = parseInt(Time.fixedTime * 100);
        composedItem["level"] = parseInt(composedItem ["level"].ToString()) + 1; 
        db.Update ("Items", composedItem); 
		
        DrawToString ();
    }
    GUI.Box (new Rect (0, 50, Screen.width, Screen.height - 50), "\r\n" + _context +
		"\r\n DBFilePath=" + Application.persistentDataPath); 
}


function Format ( s : String) : String
    {	
        var pos = s.IndexOf (','[0], s.IndexOf (','[0], 0) + 1);
        if (pos > 0) {
            s = s.Substring (0, pos + 1) + "\r\n" + Format (s.Substring (pos + 1));
        }
        return s;
    }