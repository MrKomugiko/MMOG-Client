using System.Net.WebSockets;
using System.Xml.Schema;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CounterScript : MonoBehaviour
{
    [SerializeField]  TextMeshProUGUI description;
    [SerializeField]  List<Image> counterCircles = new List<Image>();
    [SerializeField] int timeDelay;
    public delegate void MethodToExecute(int roomIdToLeaveFrom);
    MethodToExecute MTExe;

    private bool IsCounterRunning = false;
    public void SetCounterForLeavingDungeon(MethodToExecute method, string shortDescription, int timeCountdown, int roomID)
    {
        if(IsCounterRunning) 
        {
            print("counter juz dziala");
            return;
        }

        this.gameObject.SetActive(true);
        MTExe = method;
        description.SetText(shortDescription);
        timeDelay = timeCountdown;

        StartCoroutine(StartCounter(timeDelay, roomID));
    }

    public IEnumerator StartCounter(int time,int roomID)
    {
        IsCounterRunning = true;
        // 10 kulek / time 
        // updating interfal 
        float interval = time*1.0f / counterCircles.Count *1.0f;
        print(interval.ToString());

        foreach(var circle in counterCircles)
        {
            yield return new WaitForSecondsRealtime(interval);
            circle.color = Color.red;
        }

        MTExe(roomID);
        
        ResetCounter();
        yield return null;

    }

    private void ResetCounter()
    {
        // wyzerowanie kulek na biało
        foreach(var circle in counterCircles)
        {
            circle.color = Color.white;
        }
        
        // wyczyszczenie opisu
        description.SetText("");

        // wyczyszczenie delegaty
        MTExe = null;

        // wyzerowanie czasu
        timeDelay = 0;

        // włączenie do kolejnego dzialania
        IsCounterRunning = false;

        // zamkniecie okna
        this.gameObject.SetActive(false);
    }



    // [ContextMenu("test Delegate")] public void TestDelegate() => SetCounter(DebugWrite, "test...", 3);
    // public void DebugWrite() => print("ELOOOOOOOOOOOOOOOOOOOO");
}
