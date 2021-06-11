using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UPDATE_NOTES
{
    [SerializeField] public DATA _Data;
}
[Serializable]
public class DATA
{
    public Locations this[LOCATIONS location] {
        get => _Locations[(int)location];
        set => _Locations.Insert((int)location, value);
    }
    [SerializeField] public List<Locations> _Locations ;

    public Items this[ITEMS item] {
        get => _Items[(int)item];
        set => _Items.Insert((int)item, value);
    }
    [SerializeField] public List<Items> _Items;

}
[Serializable]
public class Locations
{
    public Maptypes this[MAPTYPE type] {
        get => _Type[(int)type];
        set => _Type.Insert((int)type, value);
    }
   [SerializeField]  public string _Name;    // Start_First_Floor
    [SerializeField] public int _Id ; // 0
    [SerializeField] public Vector3_json _Coordinates ; // (0,0,0)
    [SerializeField] public List<Maptypes> _Type ;
}
[Serializable]
public class Vector3_json
{
    public Vector3_json() {

    }

    public Vector3_json(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

      [SerializeField] public float x { get; set; }
      [SerializeField] public float y { get; set; }
      [SerializeField] public float z { get; set; }
}
[Serializable]
public class Maptypes : IDataElement
{
      [SerializeField] public int _Id;
      [SerializeField] public string _Name;
      [SerializeField] public string _Type; // _ObstacleMAP_Version
      [SerializeField] public int _Version;// 1000

    public void UpdateVersionNumber() {
        this._Version++;
    }
}
[Serializable]
public class Items : IDataElement
{
    public Items() {
    }

    public Items(int id, string name, string type) {
        _Id = id;
        _Name = name;
        _Type = type;

        Console.WriteLine($"dodano {_Name}, wersja {_Version}");
    }

      [SerializeField] public int _Id;
      [SerializeField] public string _Name;
      [SerializeField] public string _Type;
      [SerializeField] public int _Version = 1000;

    public void UpdateVersionNumber() {
        this._Version++;
    }
}

public interface IDataElement
{
    void UpdateVersionNumber();

}

