using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private string _username;

    public int Id { get => _id; set => _id = value; }
    public string Username { get => _username; set => _username = value; }

}
