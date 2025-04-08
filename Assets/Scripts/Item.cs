using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class Item : ScriptableObject
{
	public string itemName;

	[TextArea]
	public string description;

	public Sprite itemSprite;
	public ItemType type;
	public bool isUnique;
	public List<ItemEffect> itemEffects;
}