using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScript : MonoBehaviour
{
    static GameObject SlotPrefab;
    static GameObject EmptySlotPrefab;
    static GameObject Content_SlotsHolder;
    public Inventory InventoryDATA;
    public List<GameObject> ListSlotObjects;
    public static InventoryScript instance;

    private void Awake() {

        Content_SlotsHolder = this.transform.Find("Content").Find("SLOTS").gameObject;
       // print("DEBUG: "+Content_SlotsHolder.name);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
        this.gameObject.SetActive(false);
    }
    public void SetupInventory()
    {
        SlotPrefab = (Resources.Load ("SlotPrefabs/SLOT")) as GameObject;
        Console.WriteLine(SlotPrefab.name);
        EmptySlotPrefab = (Resources.Load ("SlotPrefabs/EMPTY_SLOT")) as GameObject;
        Console.WriteLine(EmptySlotPrefab.name);
        ListSlotObjects = new List<GameObject>();
        InventoryDATA = new Inventory(10);
    }

  [ContextMenu("Dodaj item / nonstackable / Item")]
    public void AddItem0()
    {
        InventoryDATA.DEBUG_ADDITEMBYID(0);
      
    }

 [ContextMenu("Dodaj item / stackable / Health potion")]
    public void AddItem1()
    {
        InventoryDATA.DEBUG_ADDITEMBYID(1);

    }
      [ContextMenu("Dodaj item / stackable / Silver coin")]
    public void AddItem2()
    {
        InventoryDATA.DEBUG_ADDITEMBYID(2);
        
    }
    public void NewSlotAdded()
    {
    //    print("dodano slot");
        var slot = Instantiate
        (
            InventoryScript.EmptySlotPrefab,
            Vector3.zero,
            Quaternion.identity,
            Content_SlotsHolder.transform
        );
        slot.transform.localPosition = Vector3.zero;
        ListSlotObjects.Add(slot);
    }

    public void NewItemAdded(int slotIndex, int itemId, bool isStackable)
    {
        // usuniecie starego holdera dla pustego pola
        int siblingIndex = ListSlotObjects[slotIndex].transform.GetSiblingIndex();
        Destroy(ListSlotObjects[slotIndex]);
        ListSlotObjects.RemoveAt(slotIndex);
        ListSlotObjects.Insert(slotIndex,null);
        // dodanie nowego
        print($"dodano 1 item {itemId} do slotu nr. {slotIndex}");
        var itemSlot = Instantiate
        (
            SlotPrefab,
            Vector3.zero,
            Quaternion.identity,
            Content_SlotsHolder.transform
        );

        itemSlot.transform.localPosition = Vector3.zero;
        itemSlot.transform.SetSiblingIndex(siblingIndex);
        // dodanie w jego miejsce hodlera z inormacjami dot. itemka
        ListSlotObjects[slotIndex] = itemSlot;
        // konfiguracja wstepna, obrazek, liczba sztuk 
        var image =  ListSlotObjects[slotIndex].transform.Find("ItemImage").GetComponent<Image>();
        image.sprite = ListOfItemDATA.Where(item=>item.id == itemId).First().image;
        var counter = ListSlotObjects[slotIndex].transform.Find("ItemCounter").GetComponent<TextMeshProUGUI>();
        counter.text = isStackable?"1":"";
    }
    // TODO: zamienic na dict'a danych itemkow wszytsko statystyki, opis itp, bedzie trzymanie u klienta, a serwer bedzie wydawac pozwolenie na drop izalozenie ?
    [SerializeField] public List<ITEM_UI_DATA> ListOfItemDATA; 
  
    

    public void StackExistItem(int slotIndex, int itemId)
    {
        print($"dodano kolejny item [{itemId}]{Inventory.Items_LIST.Where(item=>item.id == itemId).First().name} do slotu nr. {slotIndex}");
        var counter = ListSlotObjects[slotIndex].transform.Find("ItemCounter").GetComponent<TextMeshProUGUI>();
        int countValue;
        Int32.TryParse(counter.text,out countValue);
        counter.SetText((countValue + 1).ToString());
    }
}

[Serializable]
public class ITEM_UI_DATA
{
    public string name;
    public int id;
    public Sprite image;

}
