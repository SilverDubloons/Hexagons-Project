using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public string itemName;
    public float weight; // kg
	public Sprite icon;
	public int cost;
	public string type;
	public int typeInt;	// sorting	1 = Weapon		2 = Consumable Weapon	3 = Armor	4 = Ammo
						//			5 = Meds		6 = Tools				7 = Mats	8 = Mods
						//			9 = Implants	10= Quest				11= Junk
	public bool stackable;
	public int quantity;
	public int recency; // higher is more recent
	public UIItem uiItem;
	
	void Awake()
	{
		if(string.IsNullOrEmpty(type) && typeInt == 0)
		{
			print(this.name + " item has no type inititialization");
		}
		if(typeInt == 0)
		{
			string typeLowerCase = type.ToLower();
			switch(typeLowerCase)
			{
				case "weapon":
				typeInt = 1;
				break;
				case "cw":
				typeInt = 2;
				break;
				case "armor":
				typeInt = 3;
				break;
				case "ammo":
				typeInt = 4;
				break;
				case "med":
				typeInt = 5;
				break;
				case "tool":
				typeInt = 6;
				break;
				case "mat":
				typeInt = 7;
				break;
				case "mod":
				typeInt = 8;
				break;
				case "implant":
				typeInt = 9;
				break;
				case "quest":
				typeInt = 10;
				break;
				case "junk":
				typeInt = 11;
				break;
				default:
				print(this.name + " item has bad formatted type string");
				break;
			}
		}
	}
}