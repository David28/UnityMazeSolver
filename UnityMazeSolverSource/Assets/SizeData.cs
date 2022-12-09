using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// fileName is the default name when creating a new Instance
// menuName is where to find it in the context menu of Create
[CreateAssetMenu(fileName = "Data", menuName = "Data/SizeData")]


public class SizeData : ScriptableObject
{
    public string someStringValue = "";
    public int width = 20;
    public int height = 20;

}
