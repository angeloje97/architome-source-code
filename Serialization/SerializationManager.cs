using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using Architome.Enums;

namespace Architome
{
    public class SerializationManager
    {
        public static (bool, string) SaveGame(string saveName, object saveData)
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
            

            return (true, path);
        }

        public static object LoadGame(string saveName)
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


        public static List<object> LoadSaves()
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/saves"))
            {
                return null;
            }


            string[] fileEntries = Directory.GetFiles($"{Application.persistentDataPath}/saves");

            Debugger.InConsole(9185, $"{fileEntries.Length}");


            List<object> objects = new();

            BinaryFormatter formatter = GetBinaryFormatter();

            foreach (var entry in fileEntries)
            {
                if (!entry.Contains(".save")) continue;

                FileStream file = File.Open(entry, FileMode.Open);
                try
                {
                    object save = formatter.Deserialize(file);
                    file.Close();


                    objects.Add(save);
                }
                catch
                {
                    file.Close();
                }
            }

            return objects;
        }


        public static bool SaveConfig(string saveName, object saveData)
        {
            BinaryFormatter formatter = GetBinaryFormatter();

            if (!Directory.Exists($"{Application.persistentDataPath}/configuration"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/configuration");
            }

            string path = $"{Application.persistentDataPath}/configuration/{saveName}.config";

            

            Debugger.InConsole(9693, $"path is {path}");

            FileStream file = File.Create(path);

            formatter.Serialize(file, saveData);

            file.Close();


            return true;
        }

        public static object LoadConfig(string configName)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/configuration"))
            {
                return null;
            }

            string path = $"{Application.persistentDataPath}/configuration/{configName}.config";


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

            SurrogateSelector selector = new();

            var v3Surrogate = new Vector3SerializationSurrogate();
            var quatSurrogate = new QuaternionSerializationSurrogate();
            var v2Surrogate = new Vector2SerializationSurrogate();

            selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3Surrogate);
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quatSurrogate);
            selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), v2Surrogate);

            formatter.SurrogateSelector = selector;

            return formatter;
        }
    }
}
