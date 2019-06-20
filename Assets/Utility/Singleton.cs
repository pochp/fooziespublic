using UnityEngine;


// Taken from http://wiki.unity3d.com/index.php/Singleton 
// This singleton was modified to be destroyed on load.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T _instance;

    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong." +
                                       " An instance of type " + typeof(T).ToString() + " already exists." +
                                       " - there should never be more than 1 singleton!" +
                                       " Reopenning the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                    }
                }

                return _instance;
            }
        }
    }
}