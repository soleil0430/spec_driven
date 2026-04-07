using System.IO;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Codenoob.Util
{
    public class Menu : ScriptableObject
    {
        [MenuItem("Codenoob/Util/Clear PlayerPrefab")]
        static void Clear_PlayerPrefab()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Codenoob/Util/Delete persistentDataPath")]
        static void Delete_PersistentDataPath()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }
    }
}