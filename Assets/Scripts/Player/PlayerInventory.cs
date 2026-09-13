using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
   
    [SerializeField]private static string currentKey;

    public static void AddKey(KeyType key) 
    {
        Debug.Log("Key collected: " + key);
        currentKey = key.ToString();
    }
    public static void RemoveKey()
    {
        currentKey = "none";
    }
    public bool HasKey(string key) 
    {
        return currentKey.Equals(key);
    }
}
