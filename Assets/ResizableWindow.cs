using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResizableWindow : MonoBehaviour
{
	public RectTransform rt;
    public RectTransform topResize;
	public RectTransform rightResize;
	public RectTransform leftResize;
	public RectTransform bottomResize;
	public RectTransform titleMovement;
	public RectTransform scrollView;
	public RectTransform contentRect;
	public RectTransform closeButtonRect;
	public RectTransform itemTypeToggleElipsis;
	public RectTransform[] menuBars;
	public RectTransform centerFill;
	public RectTransform windowShadowRect;
	public RectTransform windowBorderRect;
	public TMP_InputField searchInput;
	public Scrollbar scrollbar;
	public TMP_Text titleText;
	public TMP_Text weightText;
	public TMP_Text takeAllButtonText;
	public Vector2 minimumSize;
	public UIData uiData;
	public Transform sortingOptionsParent;
	public Transform itemParent;
	public Transform itemTypeToggleParent;
	public bool scrollbarToTop;
	public Container container;
	public Toggle alwaysToggle;
	public Toggle inverseToggle;
	public bool hasACloseButton;
	public bool hasATakeAllButton;
	
	public List<UIItem> uiItems;
	public List<RectTransform> itemTypeToggleRects;
	public List<int> itemTypesExcluded;
	public List<UIItem> currentSelection;
	public UIItem lastSelectedUIItem;
	
	public void ClearCurrentSelection()
	{
		for(int i = 0; i < currentSelection.Count; i++)
		{
			currentSelection[i].borderImage.color = uiData.darkGray;
		}
		currentSelection.Clear();
		lastSelectedUIItem = null;
	}
	
	public void ResizingFinished()
	{
		uiData.mouseCursor.ChangeCursor(0);
		scrollbarToTop = false;
	}
	
	public void WindowResized()
	{
		//print(container.containerName + " was resized.");
		windowShadowRect.sizeDelta = rt.sizeDelta;
		windowBorderRect.sizeDelta = new Vector2(rt.sizeDelta.x - uiData.edgeSize, rt.sizeDelta.y - uiData.edgeSize);
		for(int i = 0; i < menuBars.Length; i++)
		{
			menuBars[i].sizeDelta = new Vector2(rt.sizeDelta.x - uiData.edgeSize * 2, menuBars[i].sizeDelta.y);
		}
		centerFill.sizeDelta = new Vector2(rt.sizeDelta.x - uiData.edgeSize * 2, rt.sizeDelta.y - uiData.edgeSize * 2 - uiData.menuBarSize*3 - uiData.generalPadding*2);
		topResize.sizeDelta = new Vector2(rt.sizeDelta.x - uiData.cornerResizerSize * 2, topResize.sizeDelta.y);
		bottomResize.sizeDelta = new Vector2(rt.sizeDelta.x - uiData.cornerResizerSize * 2, bottomResize.sizeDelta.y);
		leftResize.sizeDelta = new Vector2(leftResize.sizeDelta.x, rt.sizeDelta.y - uiData.cornerResizerSize * 2);
		rightResize.sizeDelta = new Vector2(rightResize.sizeDelta.x, rt.sizeDelta.y - uiData.cornerResizerSize * 2);
		float titleMovementSizeX = rt.sizeDelta.x - uiData.cornerResizerSize * 2;
		float textRectSizeX = rt.sizeDelta.x - uiData.cornerResizerSize * 2 - uiData.generalPadding * 2;
		if(hasACloseButton)
		{
			titleMovementSizeX -= uiData.menuBarSize;
			textRectSizeX -= uiData.menuBarSize - uiData.generalPadding;
		}
		titleMovement.sizeDelta = new Vector2(titleMovementSizeX, titleMovement.sizeDelta.y);
		RectTransform textRectTransform = titleText.GetComponent<RectTransform>();
		textRectTransform.sizeDelta = new Vector2(textRectSizeX, textRectTransform.sizeDelta.y);
		scrollView.sizeDelta = new Vector2(rt.sizeDelta.x - uiData.cornerResizerSize - uiData.edgeSize - uiData.generalPadding * 3, rt.sizeDelta.y - uiData.cornerResizerSize - uiData.edgeSize - uiData.menuBarSize * 3 - uiData.generalPadding * 4);
		RearangeContainer();
	}
	
	public void RebuildItemTypeToggles()
	{
		List<int> uniqueItemTypes = new List<int>();
		foreach(Transform child in itemParent.transform)
		{
			UIItem curUIITem = child.GetComponent<UIItem>();
			if(!uniqueItemTypes.Contains(curUIITem.item.typeInt))
			{
				if(curUIITem.item.typeInt == 0)
				{
					print(curUIITem.item.itemName + " has type 0");
				}
				uniqueItemTypes.Add(curUIITem.item.typeInt);
			}
		}
		foreach(Transform oldItemType in itemTypeToggleParent)
		{
			Destroy(oldItemType.gameObject);
		}
		itemTypeToggleRects.Clear();
		uniqueItemTypes.Sort();
		for(int i = 0; i < uniqueItemTypes.Count; i++)
		{
			GameObject newItemType = Instantiate(uiData.itemTypeTogglePrefab, new Vector3(0,0,0), Quaternion.identity, itemTypeToggleParent);
			ItemTypeToggle itemTypeToggle = newItemType.GetComponent<ItemTypeToggle>();
			itemTypeToggle.itemType = uniqueItemTypes[i];
			itemTypeToggle.resizableWindow = this;
			itemTypeToggle.representativeImage.sprite = uiData.itemTypeRepresentativeSprites[uniqueItemTypes[i]];
			itemTypeToggle.rt.anchoredPosition = new Vector2(uiData.edgeSize + uiData.generalPadding*2 + uiData.sortByButtonSize + (uiData.itemTypeToggleSize + uiData.itemTypeTogglePadding)*i, -64);
			itemTypeToggleRects.Add(newItemType.GetComponent<RectTransform>());
			if(itemTypesExcluded.Contains(uniqueItemTypes[i]))
			{
				itemTypeToggle.ChangeColorsToDisabled();
			}
		}
		if(CountVisibleItems() == 0)
		{
			ResetItemTypeTogglesAndSearch();
		}
	}
	
	public void CreateIconsForContainer(Container cont)
	{
		//print("Rebuilding container named " + cont.containerName);
		uiItems.Clear();
		container = cont;
		if(!hasACloseButton)
		{
			closeButtonRect.gameObject.SetActive(false);
		}
		if(!hasATakeAllButton)
		{
			takeAllButtonText.transform.parent.gameObject.SetActive(false);
		}
		List<int> uniqueItemTypes = new List<int>();
		//float totalWeight = 0f;
		foreach(Transform child in itemParent.transform)
		{
			Destroy(child.gameObject);
		}
		foreach(Transform child in cont.transform)
		{
			GameObject newItem = Instantiate(uiData.itemUIPrefab, new Vector3(0,0,0), Quaternion.identity, itemParent);
			UIItem newUIItem = newItem.GetComponent<UIItem>();
			uiItems.Add(newUIItem);
			newUIItem.resizableWindow = this;
			newUIItem.borderRect.sizeDelta = new Vector2(uiData.itemSize, uiData.itemSize);
			Item curItem = child.GetComponent<Item>();
			curItem.uiItem = newUIItem;
			newUIItem.item = curItem;
			newItem.name = curItem.itemName;
			if(!uniqueItemTypes.Contains(newUIItem.item.typeInt))
			{
				if(newUIItem.item.typeInt == 0)
				{
					print(newUIItem.item.itemName + " has type 0");
				}
				uniqueItemTypes.Add(newUIItem.item.typeInt);
			}
			Sprite itemSprite = curItem.icon;
			if(curItem.stackable && curItem.quantity > 1)
			{
				newUIItem.quantityText.gameObject.SetActive(true);
				newUIItem.quantityText.text = ""+curItem.quantity;
			}
			newUIItem.itemImage.sprite = itemSprite;
			Vector2 spriteSize = itemSprite.rect.size;
			Vector2 sizeRatio = new Vector2();
			if(spriteSize.x >= spriteSize.y)
			{
				sizeRatio.x = 1;
				sizeRatio.y = spriteSize.y / spriteSize.x;
			}
			else
			{
				sizeRatio.x = spriteSize.x / spriteSize.y;
				sizeRatio.y = 1;
			}
			newUIItem.imageRect.sizeDelta = new Vector2((uiData.itemSize - uiData.itemBorderPadding*2)*sizeRatio.x, (uiData.itemSize - uiData.itemBorderPadding*2)*sizeRatio.y);
			newUIItem.imageRect.anchoredPosition = new Vector2(uiData.itemBorderPadding + (uiData.itemSize - uiData.itemBorderPadding*2)*((1-sizeRatio.x)/2), uiData.itemBorderPadding + (uiData.itemSize - uiData.itemBorderPadding*2)*((1-sizeRatio.y)/2));
			//totalWeight += curItem.weight;
		}
		uniqueItemTypes.Sort();
		for(int i = 0; i < uniqueItemTypes.Count; i++)
		{
			GameObject newItemType = Instantiate(uiData.itemTypeTogglePrefab, new Vector3(0,0,0), Quaternion.identity, itemTypeToggleParent);
			ItemTypeToggle itemTypeToggle = newItemType.GetComponent<ItemTypeToggle>();
			
			itemTypeToggle.itemType = uniqueItemTypes[i];
			itemTypeToggle.resizableWindow = this;
			itemTypeToggle.representativeImage.sprite = uiData.itemTypeRepresentativeSprites[uniqueItemTypes[i]];
			itemTypeToggle.rt.anchoredPosition = new Vector2(uiData.edgeSize + uiData.generalPadding*2 + uiData.sortByButtonSize + (uiData.itemTypeToggleSize + uiData.itemTypeTogglePadding)*i, -64);
			itemTypeToggleRects.Add(newItemType.GetComponent<RectTransform>());
		}
		UpdateWeight();
	}
	
	public void UpdateWeight()
	{
		weightText.text = "" + Mathf.Round(container.currentWeight*10)/10 + "/" + Mathf.Round(container.maxWeight);
	}
	
	public void SearchUpdated()
	{
		ClearCurrentSelection();
		RearangeContainer();
	}
	
	public int CountVisibleItems()
	{
		int visibleItems = uiItems.Count;
		if(itemTypesExcluded.Count > 0 || searchInput.text != "")
		{
			for(int i = 0; i < uiItems.Count; i++)
			{
				if(itemTypesExcluded.Contains(uiItems[i].item.typeInt) || (searchInput.text != "" &&  !uiItems[i].item.itemName.ToLower().Contains(searchInput.text.ToLower())))
				{
					visibleItems--;
				}
			}
		}
		return visibleItems;
	}
	
	public void ResetItemTypeTogglesAndSearch()
	{
		for(int i = 0; i < itemTypeToggleRects.Count; i++)
		{
			ItemTypeToggle curToggle = itemTypeToggleRects[i].GetComponent<ItemTypeToggle>();
			curToggle.ResetToggleColors();
		}
		itemTypesExcluded.Clear();
		searchInput.text = "";
	}
	
	public void RearangeContainer()
	{
		int itemsToDisplay = uiItems.Count;
		takeAllButtonText.text = "Take all";
		if(itemTypesExcluded.Count > 0 || searchInput.text != "")
		{
			takeAllButtonText.text = "Take filtered";
			for(int i = 0; i < uiItems.Count; i++)
			{
				if(itemTypesExcluded.Contains(uiItems[i].item.typeInt) || (searchInput.text != "" &&  !uiItems[i].item.itemName.ToLower().Contains(searchInput.text.ToLower())))
				{
					itemsToDisplay--;
				}
			}
		}
		if(scrollbar.size >= 0.99f)
		{
			scrollbarToTop = true;
		}
		int itemsWide = Mathf.FloorToInt((scrollView.sizeDelta.x - uiData.scrollbarWidth - uiData.itemPadding) / (uiData.itemSize + uiData.itemPadding));
		int itemsTall = Mathf.CeilToInt((float)itemsToDisplay / itemsWide);
		contentRect.sizeDelta = new Vector2(scrollView.sizeDelta.x, itemsTall * (uiData.itemPadding + uiData.itemSize)+uiData.itemPadding);
		if(contentRect.sizeDelta.y < scrollView.sizeDelta.y)
		{
			contentRect.sizeDelta = new Vector2(scrollView.sizeDelta.x,  scrollView.sizeDelta.y);
		}
		int curItem = 0;
		for(int i = 0; i < uiItems.Count; i++)
		{
			if(itemTypesExcluded.Contains(uiItems[i].item.typeInt) || (searchInput.text != "" &&  !uiItems[i].item.itemName.ToLower().Contains(searchInput.text.ToLower())))
			{
				uiItems[i].gameObject.SetActive(false);
			}
			else
			{
				uiItems[i].gameObject.SetActive(true);
				uiItems[i].borderRect.anchoredPosition = new Vector2(uiData.itemPadding * (curItem % itemsWide + 1) + uiData.itemSize * (curItem % itemsWide), -(uiData.itemPadding + uiData.itemSize) * (curItem / itemsWide + 1));
				curItem++;
			}
		}
		if(scrollbarToTop)
		{	// This is what causes the jittering when resizing,
			// but without it the scrollbar keeps going down.
			// This seems better than the alternative.
			// I tried adjusting script execution timing to no avail.
			scrollbar.value = 1;	
		}
		bool firstOffWindowFound = false;
		for(int j = 0 ; j < itemTypeToggleRects.Count; j++)
		{
			if(itemTypeToggleRects[j].anchoredPosition.x + itemTypeToggleRects[j].sizeDelta.x + uiData.generalPadding + uiData.edgeSize > rt.sizeDelta.x)
			{
				itemTypeToggleRects[j].gameObject.SetActive(false);
				if(!firstOffWindowFound)
				{
					itemTypeToggleElipsis.gameObject.SetActive(true);
					itemTypeToggleElipsis.anchoredPosition = new Vector2(itemTypeToggleRects[j-1].anchoredPosition.x, itemTypeToggleElipsis.anchoredPosition.y);
					itemTypeToggleRects[j-1].gameObject.SetActive(false);
				}
				firstOffWindowFound = true;
			}
			else
			{
				itemTypeToggleRects[j].gameObject.SetActive(true);
			}
		}
		if(!firstOffWindowFound)
		{
			itemTypeToggleElipsis.gameObject.SetActive(false);
		}
	}
	
	public void TakeAllClicked()
	{
		// Save this for when party system is implemented, maybe have it generate a button for each
		// party member that spawns upward like the way the sort by buttons go downward. If there is
		// only one party member then have it go straight to them
		print("Remember to implement this once party management is in the game!");
	}
	
	public void ToggleSortingOptions()
	{
		foreach(Transform child in sortingOptionsParent)
		{
			child.gameObject.SetActive(!child.gameObject.activeSelf);
		}
	}
	
	public class ItemComparerRecency : IComparer<UIItem>
	{
		public int Compare(UIItem a, UIItem b)
		{
			int aRecency = a.item.recency;
			int bRecency = b.item.recency;
			int recencyComparison = a.item.recency.CompareTo(b.item.recency);
			if(recencyComparison == 0) // this should never happen
			{
				print("Recency comparison returned 0 for comparing " + a.item.itemName + " to " + b.item.itemName);
			}
			return recencyComparison;
		}
	}
	
	public class ItemComparerType : IComparer<UIItem>
	{
		public int Compare(UIItem a, UIItem b)
		{
			int aType = a.item.typeInt;
			int bType = b.item.typeInt;
			int typeComparison = aType.CompareTo(bType);
			if(typeComparison == 0)
			{
				return a.item.recency.CompareTo(b.item.recency);
			}
			return typeComparison;
		}
	}
	
	public class ItemComparerWeight : IComparer<UIItem>
	{
		public int Compare(UIItem a, UIItem b)
		{
			float aWeight = a.item.weight;
			float bWeight = b.item.weight;
			if(a.item.stackable)
			{
				aWeight *= a.item.quantity;
			}
			if(b.item.stackable)
			{
				bWeight *= b.item.quantity;
			}
			int weightComparison = aWeight.CompareTo(bWeight);
			if(weightComparison == 0)
			{
				return a.item.recency.CompareTo(b.item.recency);
			}
			return weightComparison;
		}
	}
	public class ItemComparerValue : IComparer<UIItem>
	{
		public int Compare(UIItem a, UIItem b)
		{
			float aValue = a.item.cost;
			float bValue = b.item.cost;
			if(a.item.stackable)
			{
				aValue *= a.item.quantity;
			}
			if(b.item.stackable)
			{
				bValue *= b.item.quantity;
			}
			int valueComparison = aValue.CompareTo(bValue);
			if(valueComparison == 0)
			{
				return a.item.recency.CompareTo(b.item.recency);
			}
			return valueComparison;
		}
	}
	
	public void SortToggleUpdated()
	{
		container.reverseSort = inverseToggle.isOn;
		if(!alwaysToggle.isOn)
		{
			container.sortingType = -1;
		}
	}
	
	public void SortClicked(int sortType)
	{
		bool inverse = !inverseToggle.isOn; // Default sorting is low to high, so reverse it
		if(sortType == 1)
		{
			inverse = !inverse;	// except for type, which we want to be low to high
		}
		SortInventory(sortType, inverse, alwaysToggle.isOn);
	}
	
	public void SortInventory(int sortType, bool reverse, bool always)
	{	// 0 = Recent, 1 = Type, 2 = Weight, 3 = Value
		switch(sortType)
		{
			case 0:
			ItemComparerRecency recencyComparer = new ItemComparerRecency();
			uiItems.Sort(recencyComparer);
			break;
			case 1:
			ItemComparerType typeComparer = new ItemComparerType();
			uiItems.Sort(typeComparer);
			break;
			case 2:
			ItemComparerWeight weightComparer = new ItemComparerWeight();
			uiItems.Sort(weightComparer);
			break;
			case 3:
			ItemComparerValue valueComparer = new ItemComparerValue();
			uiItems.Sort(valueComparer);
			break;
		}
		if(reverse)
		{
			uiItems.Reverse();
		}
		RearangeContainer();
		
		container.reverseSort = reverse;
		if(always)
		{
			container.sortingType = sortType;
			for(int i = 0; i < uiItems.Count; i++)
			{
				uiItems[i].item.transform.SetSiblingIndex(i);
			}
		}
		else
		{
			container.sortingType = -1;
		}
	}
}