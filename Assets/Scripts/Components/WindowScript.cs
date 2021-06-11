using UnityEngine;

public class WindowScript : MonoBehaviour
{
    public void OnClick_Close()
    {
        this.transform.gameObject.SetActive(false);
    }

    private void OnEnable() {
        // chowanie podswietlenia npc i napisu
           
    }
}
