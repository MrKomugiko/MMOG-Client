using System.Net.WebSockets;
using System.Xml.Schema;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

public class CounterScript : MonoBehaviour
{
    [SerializeField]  TextMeshProUGUI description;
    [SerializeField]  List<Image> counterCircles = new List<Image>();
    [SerializeField] int _timeDelay;
    public delegate void MethodToExecute(int roomIdToLeaveFrom);
    private Coroutine routineInProgress = null;

    public void SetCounter(Action<int> _action, int _actionParam, string shortDescription, int timeCountdown)
    {
        if(routineInProgress != null) print("counter juz działa");

        this.gameObject.SetActive(true);

        description.SetText(shortDescription);

        _timeDelay = timeCountdown;

        routineInProgress = StartCoroutine(StartCounter(_action, _timeDelay, _actionParam));
    }
 
    
    public IEnumerator CancelCounter()
    {
        StopCoroutine(routineInProgress);
        
        foreach(var circle in counterCircles.Where(c=>c.color == Color.red))
        {
            circle.color = new Color32( 254 , 161 , 0, 255 );
        }
        yield return new WaitForSeconds(0.5f);

        ResetCounter();
        print("routine zatrzymana");
        yield return null;
    }
    public IEnumerator StartCounter(Action<int> _actionToExecuteAfterCountEnd,int time,int _actionParam)
    {
        float interval = time*1.0f / counterCircles.Count *1.0f;

        foreach(var circle in counterCircles)
        {
            yield return new WaitForSecondsRealtime(interval);
            circle.color = Color.red;
        }

        _actionToExecuteAfterCountEnd(_actionParam);
        
        ResetCounter();
        yield return null;
    }

    public void OnClick_CancelCounter()
    {
        print("Anulowałeś odliczanie - cancel counter");
  
        // wysłanie do serwera info ze anulujesz odliczanie 
        // serwer ma rozesłac to do osob z twojego teamu
        ClientSend.SendCancellationCounting(GameManager.GetLocalPlayer().dungeonRoom.LobbyID);
  
        StartCoroutine(CancelCounter());
    }
     public void FromServer_CancelCounter(Packet packet)
    {
        print("serwer anulował odliczanier");
        StartCoroutine(CancelCounter());
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

        // wyzerowanie czasu
        _timeDelay = 0;

        // zamkniecie okna
        this.gameObject.SetActive(false);

        // reset
        routineInProgress = null;
    }
}
