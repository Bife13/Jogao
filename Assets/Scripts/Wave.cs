using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Game/Wave")]
public class Wave : ScriptableObject
{
	public string waveName;
	public int waveNumber;
	public List<GameObject> waveUnits;
}