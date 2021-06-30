using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PersistentValues : MonoBehaviour
{
    
    public int playerLives = 3;

    


    public void Awake()
    {
        DontDestroyOnLoad(this);
        
        
    }


}
