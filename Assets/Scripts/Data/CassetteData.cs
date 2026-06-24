using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCassette", menuName = "Library/Cassette Data")]
public class CassetteData : ScriptableObject
{
    public string title;
    public string genre;
    public string position;
    public DateTime ReleaseDate;
    public string description;
    public Sprite coverArt;
}
