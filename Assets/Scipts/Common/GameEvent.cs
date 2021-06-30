
using System;
using UnityEngine;

[Serializable]
public class GameEvent
{
    public GameObject sender;
    public int eventId;
    public System.Object payload;
}