using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public StageList CurrentStage { get; private set; }

    private void Start()
    {
        SetStage(StageList.Setting);
    }

    public void SetStage(StageList stage)
    {
        CurrentStage = stage;
       
    }

    private void OnPreCull()
    {
        
    }
}
