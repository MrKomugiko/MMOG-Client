using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
   [SerializeField] int spingAnglePerFrame;
   [SerializeField] Transform _transform;
   Vector3 spinVector;
    public bool ReceivedMessageFromServer = false;

    private void OnEnable() 
    {
       ReceivedMessageFromServer = false;
       StartCoroutine(SpiningAnimation());
       spinVector = new Vector3(0,0,spingAnglePerFrame);
   }
    private void OnDisable() {
        StopAllCoroutines();
   }
    IEnumerator SpiningAnimation()
    {

        while(gameObject.activeSelf)
        {
            _transform.Rotate(-spinVector,Space.Self);
            yield return new WaitForFixedUpdate();
            
            if(ReceivedMessageFromServer) {
                this.gameObject.transform.parent.parent.gameObject.SetActive(false);
                yield return null;
            }
        }
    }
}
