using System.Collections;
using TMPro;
using UnityEngine;

public class WindowScript : MonoBehaviour
{

    private GameObject _switchWindow = null;
    [SerializeField] TextMeshProUGUI responsMessage;
    public void OnClick_Close()
    {
        this.transform.gameObject.SetActive(false);
    }

    public void OnClick_Switchindow(GameObject window) {

        _switchWindow = window;
        _switchWindow.SetActive(true);
        this.transform.gameObject.SetActive(false);
    }

   
        
    
    public void ShowServerMessage(string _response)
    {
        switch(_response)
        {
            case "SUCCES":
                responsMessage.SetText($"<color=green>Pomyślnie utworzono konto.</color>"); 
             
            break;

            case "FAILED":
                responsMessage.SetText($"<color=red>Nazwa użytkownika jest już zajęta.</color>"); 
            break;

            default:
            // wyswietlenie dowolnej wioadomosci
             responsMessage.SetText($"<color=white>{_response}</color>"); 
            break;

        }
            UIManager.instance.ClearMessage(time: 4f, this.responsMessage);  
    }
}
