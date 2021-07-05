using System.Collections;
using System.Linq;
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

    public void OnClick_OpenCloseWindow(GameObject window)
    {
        window.transform.gameObject.SetActive(!window.activeSelf);
    }
   public void OnClick_ConnectToServer()
   {
       Client.instance.ConnectToServer();
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

            case "CONNECTION-FAILED":
                  responsMessage.SetText($"<color=orange>Błąd łączenia z serwerem, spróbuj ponownie.</color>"); 
            break;
            
            case "CONNECTION-SUCCES":
                  responsMessage.SetText($"<color=green>Połączyłeś się z serwerem.</color>"); 
            break;
            
            default:
            // wyswietlenie dowolnej wioadomosci
             responsMessage.SetText($"<color=white>{_response}</color>"); 
            break;

        }
            UIManager.instance.ClearMessage(time: 4f, this.responsMessage);  
    }
}
