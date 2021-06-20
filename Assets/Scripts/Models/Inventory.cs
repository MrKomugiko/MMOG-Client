using System.Runtime.CompilerServices;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

    [Serializable]
    public class Inventory  
    {
        public Inventory(int capacity)
        {
            Debug.Log("Utworzono personalny magazyn gracza xD");
            this.capacity = capacity;

            Slots = new List<Slot>();
            for(int i = 0; i <capacity; i++)
            {
                Slots.Add(new Slot(i));
            }
        }
        public int capacity;
        public List<Slot> Slots;
        public void AddItemToInventory(Item _item)
        {
            // 1. sprawdz czy item sie stackuje
            if(_item.stackable)
            {
                // poszukaj innych wystapien tego typu itemka w inventory
                foreach(var slot in Slots.Where(slot=>!slot.Empty && !slot.Full))
                {
                    Debug.Log("slot index check "+slot.index);
                    if(slot.items.First().id == _item.id)
                    {
                    Debug.Log("slot index choosen one "+slot.index);

                        Slots[slot.index].items.Add(_item);
                        
                        Debug.Log($"Item może sie stackowac (domyślnie pojemnośc slota to 5 jednostek)\nAktualnie znajduje sie w slocie {Slots[slot.index].items.Count} sztuk tego itemka");
                        InventoryScript.instance.StackExistItem(slot.index, _item.id);
                        Debug.Log($"stack => slot index = "+slot.index);
                        return;
                    }
                }
            }
            // w przeciwnym razie wybieramy następny pusty slocik i pakujemy do niego item
            if(Slots.Where(slot=>slot.Empty).Any())
            {
                var avaiableSlot = Slots.Where(slot=>slot.Empty).First();
                Debug.Log("Dodano item do nowego slotu w pamieci.");
                avaiableSlot.items.Add(_item);

                Debug.Log("Dodano item do nowego slotu w UI.");
                InventoryScript.instance.NewItemAdded(avaiableSlot.index, _item.id, isStackable:_item.stackable);
            }
        }
        public void PrintFullInventoryList()
        {

            foreach(var slot in Slots.Where(items=>items.items.Count>0))
            {
         
                Debug.Log($"[{slot.items.Count} szt] [{slot.items.First().name}]");
            }

            /* test graficzna reprezentacja
                10 slotow wiec 4/4/2 *x* oznacza pełny slot z nie stackowalnym itemkiem

                    [ *1* ][ 3/5 ][ 2/3 ][  -  ]
                    [  -  ][  -  ][  -  ][  -  ]
                    [  -  ][  -  ]

            */
            string inventoryGraphic = "";
            int columnNumber = 4;
            foreach(var slot in Slots)
            {
                if(columnNumber ==0) {
                    inventoryGraphic += "\n";
                    columnNumber = 4;
                }
                if(slot.Empty)
                {
                    inventoryGraphic += ($"[ --- ]");
                    columnNumber--;
                    continue;
                }
                 if(slot.Full)
                {
                    inventoryGraphic += ($"[ *{slot.items.Count}* ]");
                }
                 if(!slot.Full && slot.items.Count>0)
                {
                    inventoryGraphic += ($"[ {slot.items.Count}/{slot.items.First().stackSize} ]");
                }
                columnNumber--;
            }

            Debug.Log(inventoryGraphic);
        }
        public void DEBUG_ADDITEMBYID(int itemId)
        {
            switch(itemId)
            {
                case 0:
                    AddItemToInventory(GetItem(0));
                    break;

                case 1:
                    AddItemToInventory(GetItem(1));
                    break;
                case 2:
                    AddItemToInventory(GetItem(2));
                    break;
            }

            PrintFullInventoryList();
        }
       
        public static Item GetItem(string itemName) => Items_LIST.Where(item=>item.name == itemName).First();
        public static Item GetItem(int itemId) => Items_LIST.Where(item=>item.id == itemId).First();
        public static List<Item> Items_LIST = new List<Item>();
    }

    [Serializable]
    public class Slot 
    {   
        public int index;
        public List<Item> items;
        public Slot(int index)
        {
            Debug.Log("dodano element slot");
            this.index = index;
            this.items = new List<Item>();

            InventoryScript.instance.NewSlotAdded();
        }
        public bool Full 
        { 
            get 
            {
                if (items.Any()) {
                    if (items.First().stackable == false) return true;

                    if (items.Count >= items.First().stackSize) {
                        return true;
                    }
                }

                return false;
            } 
        }

        public bool Empty 
        { 
            get
            {
                if (items.Count != 0) 
                    {
                    return false;
                   }
                return true;
            } 
        }
    }

    [Serializable]
    public class Item 
    {
        public int id;
        public string name;
        public int value;
        public int level;
        public bool stackable;
        public int stackSize;
        public string description;

        public Item(int id, string name, int value, int level, bool stackable,int stackSize = 1, string description = null)
        {
            this.id = id;
            this.name = name;
            this.value = value;
            this.level = level;
            this.stackable = stackable;
            this.stackSize = stackSize;
            this.description = description;
        }

        public override string ToString()
        {
            string item_String = "";

            item_String += this.name+"\n";
            item_String += $"* {this.level}lvl \n";
            item_String += $"* {this.value}$ \n";
            item_String += this.stackSize>0?$"stackuje się do {this.stackSize}szt\n":"";
            item_String += $"\n{this.description}\n";
            
            return item_String;
        }
    }