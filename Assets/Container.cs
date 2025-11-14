using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
	//	items have a 'recency' property, which is set to itemsAdded when added to this inventory.
	//	itemsAdded is then incremented. Remember to adjust recency not only for new items but
	//	for stacks of items that get incremented.
	public string containerName;
	public int itemsAdded;
	public float currentWeight;
	public float maxWeight; // kg
	public UIData uiData;
	public int sortingType = -1; // -1 = None, 0 = Recent, 1 = Type, 2 = Weight, 3 = Value
	public bool reverseSort;
	
	void Start()
	{
		uiData = GameObject.FindWithTag("UIData").GetComponent<UIData>();
		UpdateCurrentWeight();
		DrawContainer();
	}
	
	void UpdateCurrentWeight()
	{
		float totalWeight = 0;
		foreach(Transform child in transform)
		{
			Item curItem = child.GetComponent<Item>();
			if(curItem.stackable)
			{
				totalWeight += curItem.quantity * curItem.weight;
			}
			else
			{
				totalWeight += curItem.weight;
			}
		}
		currentWeight = totalWeight;
	}
	
    void DrawContainer()
	{
		GameObject newWindow = Instantiate(uiData.containerWindowPrefab, new Vector3(Random.Range(0,500),Random.Range(0,350),0), Quaternion.identity, uiData.containersHandler.transform);
		newWindow.GetComponent<RectTransform>().sizeDelta = new Vector3(273+72*3,342+72*2);
		newWindow.name = containerName;
		ResizableWindow resizableWindow = newWindow.GetComponent<ResizableWindow>();
		uiData.containersHandler.resizableWindows.Add(resizableWindow);
		if(containerName == "Bookcase")
		{
			resizableWindow.hasATakeAllButton = true;
		}
		resizableWindow.titleText.text = containerName;
		resizableWindow.uiData = uiData;
		resizableWindow.CreateIconsForContainer(this);
		resizableWindow.WindowResized();
	}
	public void AddItem(Item itemToAdd)
	{
		print("Adding item named " + itemToAdd.itemName + " to container named " + containerName);
		if(itemToAdd.stackable)
		{
			currentWeight += itemToAdd.quantity * itemToAdd.weight;
			foreach(Transform child in transform)
			{
				Item curItem = child.GetComponent<Item>();
				if(curItem.itemName == itemToAdd.itemName)
				{
					curItem.quantity += itemToAdd.quantity;
					itemToAdd.transform.SetParent(uiData.destroyedObjectsTempStorage);
					print("Destroying " + itemToAdd.itemName + " as " + curItem.itemName + " has the same name and is stackable.");
					Destroy(itemToAdd.gameObject);
					return;
				}
			}
			itemToAdd.transform.SetParent(this.transform);
		}
		else
		{
			currentWeight += itemToAdd.weight;
			itemToAdd.transform.SetParent(this.transform);
		}
	}
}
