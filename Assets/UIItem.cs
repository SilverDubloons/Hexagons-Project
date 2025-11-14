using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public RectTransform borderRect;
	public RectTransform imageRect;
	public TMP_Text quantityText;
	public Image borderImage;
	public Image itemImage;
	public Item item;
	public ResizableWindow resizableWindow;
	
	public void OnBeginDrag(PointerEventData pointerEventData)
    {
		if(pointerEventData.button == PointerEventData.InputButton.Left)
		{
			resizableWindow.uiData.containersHandler.ClearSelectionInOtherWindows(resizableWindow);
			resizableWindow.uiData.containersHandler.currentlyDragging = true;

			if(resizableWindow.currentSelection.Contains(this))
			{
				if(resizableWindow.currentSelection.Count == 1 && item.stackable)
				{
					resizableWindow.uiData.containersHandler.singularItemBeingDragged = this;
				}
			}
			else
			{
				resizableWindow.ClearCurrentSelection();
				resizableWindow.currentSelection.Add(this);
				if(item.stackable)
				{
					resizableWindow.uiData.containersHandler.singularItemBeingDragged = this;
				}
				borderImage.color = Color.white;
				resizableWindow.lastSelectedUIItem = this;
			}
			resizableWindow.uiData.mouseCursor.ChangeCursor(6);
		}
	}
	
	public void OnDrag(PointerEventData pointerEventData)
    {
		if(pointerEventData.button == PointerEventData.InputButton.Left)
		{
			resizableWindow.uiData.containersHandler.ResetBorderColors();
			int indexMouseIsOver = resizableWindow.uiData.containersHandler.WhichIndexIsMouseOver();
			if(indexMouseIsOver >= 0)
			{
				if(resizableWindow.uiData.containersHandler.resizableWindows[indexMouseIsOver] != resizableWindow)
				{
					resizableWindow.uiData.containersHandler.resizableWindows[indexMouseIsOver].windowBorderRect.GetComponent<UnityEngine.UI.Image>().color = Color.green;
				}
			}
		}
	}
	
	public void OnEndDrag(PointerEventData pointerEventData)
    {
		if(pointerEventData.button == PointerEventData.InputButton.Left)
		{
			int indexMouseIsOver = resizableWindow.uiData.containersHandler.WhichIndexIsMouseOver();
			if(indexMouseIsOver >= 0)
			{
				if(resizableWindow.uiData.containersHandler.resizableWindows[indexMouseIsOver] != resizableWindow)
				{
					ResizableWindow draggingToWindow = resizableWindow.uiData.containersHandler.resizableWindows[indexMouseIsOver];
					ResizableWindow oldWindow = resizableWindow;
					foreach(UIItem uiItem in resizableWindow.currentSelection)
					{
						uiItem.MoveItem(draggingToWindow);
					}
					draggingToWindow.RebuildItemTypeToggles();
					draggingToWindow.WindowResized();
					draggingToWindow.UpdateWeight();
					if(draggingToWindow.container.sortingType >= 0)
					{
						draggingToWindow.SortInventory(draggingToWindow.container.sortingType, draggingToWindow.container.reverseSort, true);
					}
					oldWindow.RebuildItemTypeToggles();
					oldWindow.WindowResized();
					oldWindow.UpdateWeight();
					oldWindow.ClearCurrentSelection();
				}
				else
				{
					PointerEventData ped = new PointerEventData(EventSystem.current);
					ped.position = Input.mousePosition;

					List<RaycastResult> results = new List<RaycastResult>();
					EventSystem.current.RaycastAll(ped, results);
					foreach(RaycastResult result in results)
					{
						UIItem dropTarget = result.gameObject.GetComponent<UIItem>();
						if(dropTarget != null)
						{
							if(dropTarget != this && dropTarget.item.itemName == item.itemName)
							{
								dropTarget.item.quantity += item.quantity;
								dropTarget.quantityText.gameObject.SetActive(true);
								dropTarget.quantityText.text = "" + dropTarget.item.quantity;
								dropTarget.borderImage.color = Color.white;
								resizableWindow.uiItems.Remove(this);
								item.transform.SetParent(resizableWindow.uiData.destroyedObjectsTempStorage);
								transform.SetParent(resizableWindow.uiData.destroyedObjectsTempStorage);
								resizableWindow.ClearCurrentSelection();
								if(resizableWindow.container.sortingType >= 0)
								{
									resizableWindow.SortInventory(resizableWindow.container.sortingType, resizableWindow.container.reverseSort, true);
								}
								else
								{
									resizableWindow.RearangeContainer();
								}
								Destroy(item.gameObject);
								Destroy(this.gameObject);
							}
							break;
						}
					}	
				}
			}
			resizableWindow.uiData.containersHandler.currentlyDragging = false;
			resizableWindow.uiData.containersHandler.singularItemBeingDragged = null;
			resizableWindow.uiData.containersHandler.ResetBorderColors();
			resizableWindow.uiData.mouseCursor.ChangeCursor(0);
		}
	}
	
	public void MoveItem(ResizableWindow destinationWindow)
	{
		resizableWindow.uiItems.Remove(this);
		//print("Moving " + item.itemName + " from " + resizableWindow.container.containerName + " to " +destinationWindow.container.containerName);
		if(item.stackable)
		{
			resizableWindow.container.currentWeight -= item.quantity * item.weight;
			destinationWindow.container.currentWeight += item.quantity * item.weight;
			foreach(Transform child in destinationWindow.container.transform)
			{
				Item curItem = child.GetComponent<Item>();
				if(curItem.itemName == item.itemName)
				{
					curItem.quantity += item.quantity;
					curItem.uiItem.quantityText.gameObject.SetActive(true);
					curItem.uiItem.quantityText.text = "" + curItem.quantity;
					curItem.recency = destinationWindow.container.itemsAdded;
					destinationWindow.container.itemsAdded++;
					item.transform.SetParent(resizableWindow.uiData.destroyedObjectsTempStorage);
					transform.SetParent(resizableWindow.uiData.destroyedObjectsTempStorage);
					//print("Destroying " + itemToAdd.itemName + " as " + curItem.itemName + " has the same name and is stackable.");
					Destroy(item.gameObject);
					Destroy(this.gameObject);
					return;
				}
			}
			item.recency = destinationWindow.container.itemsAdded;
			destinationWindow.container.itemsAdded++;
			destinationWindow.uiItems.Add(this);
			resizableWindow = destinationWindow;
			item.transform.SetParent(destinationWindow.container.transform);
			transform.SetParent(destinationWindow.itemParent);
		}
		else
		{
			item.recency = destinationWindow.container.itemsAdded;
			destinationWindow.container.itemsAdded++;
			resizableWindow.container.currentWeight -= item.weight;
			destinationWindow.container.currentWeight += item.weight;
			destinationWindow.uiItems.Add(this);
			resizableWindow = destinationWindow;
			item.transform.SetParent(destinationWindow.container.transform);
			transform.SetParent(destinationWindow.itemParent);
		}
	}
	
	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		if(!resizableWindow.uiData.containersHandler.currentlyDragging)
		{
			borderImage.color = Color.white;
		}
		else
		{
			if(resizableWindow.uiData.containersHandler.singularItemBeingDragged != null)
			{
				if(resizableWindow.uiData.containersHandler.singularItemBeingDragged != this)
				{
					if(resizableWindow.uiData.containersHandler.singularItemBeingDragged.resizableWindow == resizableWindow)
					{
						if(resizableWindow.uiData.containersHandler.singularItemBeingDragged.item.itemName == item.itemName && item.stackable)
						{
							borderImage.color = Color.green;
						}
					}
				}
			}
		}
	}
	
	public void OnPointerUp(PointerEventData pointerEventData)
    {

	}
	
	public void OnPointerExit(PointerEventData pointerEventData)
	{
		if(!resizableWindow.currentSelection.Contains(this))
		{
			borderImage.color = resizableWindow.uiData.darkGray;
		}
	}
	
	public void OnPointerClick(PointerEventData pointerEventData)
	{
		resizableWindow.uiData.containersHandler.ClearSelectionInOtherWindows(resizableWindow);
		if(pointerEventData.button == PointerEventData.InputButton.Left)
		{
			if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
			{
				if(resizableWindow.lastSelectedUIItem != null)
				{
					int thisItemIndex = resizableWindow.uiItems.IndexOf(this);
					int lastSelectedItemIndex = resizableWindow.uiItems.IndexOf(resizableWindow.lastSelectedUIItem);
					if(thisItemIndex != lastSelectedItemIndex)
					{
						int firstIndex = thisItemIndex;
						int lastIndex = lastSelectedItemIndex;
						if(lastSelectedItemIndex < firstIndex)
						{
							firstIndex = lastSelectedItemIndex;
							lastIndex = thisItemIndex;
						}
						for(int i = firstIndex; i <= lastIndex; i++)
						{
							if(!resizableWindow.currentSelection.Contains(resizableWindow.uiItems[i]) && !resizableWindow.itemTypesExcluded.Contains(resizableWindow.uiItems[i].item.typeInt))
							{
								if(resizableWindow.searchInput.text == "" ||  resizableWindow.uiItems[i].item.itemName.ToLower().Contains(resizableWindow.searchInput.text.ToLower()))
								{
									resizableWindow.currentSelection.Add(resizableWindow.uiItems[i]);
									resizableWindow.uiItems[i].borderImage.color = Color.white;
								}
							}
						}
					}
				}
				else
				{
					resizableWindow.ClearCurrentSelection();
					resizableWindow.currentSelection.Add(this);
					borderImage.color = Color.white;
					resizableWindow.lastSelectedUIItem = this;
				}
			}
			else
			{
				if(Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
				{
					if(resizableWindow.currentSelection.Contains(this))
					{
						borderImage.color = resizableWindow.uiData.darkGray;
						resizableWindow.currentSelection.Remove(this);
						if(resizableWindow.currentSelection.Count == 0)
						{
							resizableWindow.ClearCurrentSelection();
						}
					}
					else
					{
						borderImage.color = Color.white;
						resizableWindow.currentSelection.Add(this);
						resizableWindow.lastSelectedUIItem = this;
					}
				}
				else
				{
					resizableWindow.ClearCurrentSelection();
					resizableWindow.currentSelection.Add(this);
					borderImage.color = Color.white;
					resizableWindow.lastSelectedUIItem = this;
				}
			}
		}
		if(pointerEventData.button == PointerEventData.InputButton.Right)
		{
			if(item.stackable && item.quantity > 1)
			{
				resizableWindow.uiData.itemSplitInterface.gameObject.SetActive(true);
				resizableWindow.uiData.itemSplitInterface.SetupSplitInterface(this);
			}
		}
	}
}