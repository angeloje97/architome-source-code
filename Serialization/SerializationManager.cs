using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Architome
{
    public class SerializationManager
    {
        public static bool Save(string saveName, object saveData)
        {
            BinaryFormatter formatter = GetBinaryFormatter();

            if (!Directory.Exists($"{Application.persistentDataPath}/saves"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/saves");
            }

            string path = $"{Application.persistentDataPath}/saves/{saveName}.save";

            Debugger.InConsole(9693, $"path is {path}");

            FileStream file = File.Create(path);

            formatter.Serialize(file, saveData);

            file.Close();
            

            return true;
        }

        public static object Load(string saveName)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/saves"))
            {
                return null;
            }

            string path = $"{Application.persistentDataPath}/saves/{saveName}.save";


            if (!File.Exists(path))
            {
                return null;
            }

            BinaryFormatter formatter = GetBinaryFormatter();

            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                object save = formatter.Deserialize(file);
                file.Close();
                return save;
            }
            catch
            {
                Debug.LogError($"Failed to load file at {path}");
                file.Close();
                return null;
            }
        }

        public static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter;
        }
    }
}
