using UnityEngine;

public class WindowScript : MonoBehaviour
{

    private GameObject _switchWindow = null;

    public void OnClick_Close()
    {
        this.transform.gameObject.SetActive(false);
    }

    public void OnClick_Switchindow(GameObject window) {

        _switchWindow = window;
        _switchWindow.SetActive(true);
        this.transform.gameObject.SetActive(false);
    }


    private void OnEnable() {
        // TODO:  chowanie podswietlenia npc i napisu
           
    }
}
