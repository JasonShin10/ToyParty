using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility 
{
    public static int GetGameLayers(string[] layerNames)
    {
        return LayerMask.GetMask(layerNames);
    }
}
