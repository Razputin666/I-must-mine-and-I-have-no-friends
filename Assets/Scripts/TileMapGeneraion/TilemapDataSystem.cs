using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class TilemapDataSystem
{
    public static void Save(string saveName, string folderName, List<WorldTile> saveTiles)
    {
        string path = Path.Combine(Application.persistentDataPath, "Saves");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path, folderName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path, saveName + ".save");
        Debug.Log(path);
        //string saveJson = JsonHelper.ToJson(saveTiles.ToArray(), true);

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, saveTiles.ToArray());
        stream.Close();

        //File.WriteAllText(path, saveJson);
    }

    public static List<WorldTile> Load(string saveName, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, "Saves");
        path = Path.Combine(path, folderName);
        path = Path.Combine(path, saveName + ".save");

        if (File.Exists(path))
        {
            //List<WorldTile> gameTiles = new List<WorldTile>();

            //string loadJson = File.ReadAllText(path);

            //List<WorldTile> loadTiles = JsonHelper.FromJson<WorldTile>(loadJson).ToList<WorldTile>();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            List<WorldTile> loadTiles = ((WorldTile[])formatter.Deserialize(stream)).ToList<WorldTile>();
            stream.Close();

            return loadTiles;
        }
        else
        {
            Debug.LogError("Save file not found.");
            return null;
        }
    }

    public static void Delete(string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, "Saves");
        path = Path.Combine(path, folderName);

        DirectoryInfo directory = new DirectoryInfo(path);
        directory.Delete(true);
    }
}