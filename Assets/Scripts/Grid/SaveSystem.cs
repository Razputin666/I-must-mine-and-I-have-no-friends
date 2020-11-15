using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem
{
    private const string SAVE_EXTENSION = "txt";

    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    private static bool isInit = false;

    public static void Init()
    {
        if(!isInit)
        {
            isInit = true;
            //Test if Save folder exists
            if(!Directory.Exists(SAVE_FOLDER))
            {
                //Create Save Folder
                Directory.CreateDirectory(SAVE_FOLDER);
            }
        }
    }
}
