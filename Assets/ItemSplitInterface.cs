using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSplitInterface : MonoBehaviour
{
    public RectTransform rt;
	public Vector2 totalSize;
	public RectTransform interactionBlockerRect;
	public UnityEngine.UI.Image itemImage;
	public RectTransform itemRect;
	public TMP_Text itemNameLabel;
	public TMP_Text itemMaxQuantityLabel;
	public TMP_InputField quantityInput;
	public UnityEngine.UI.Slider quantitySlider;
	public UIData uiData;
	public UIItem uiItem;
	
	public void SetupSplitInterface(UIItem uiItemToSplit)
	{
		uiItem = uiItemToSplit;
		rt.anchoredPosition = new Vector2(Screen.width/2 - totalSize.x/2, Screen.height/2 - totalSize.y/2);
		interactionBlockerRect.anchoredPosition = new Vector2(-rt.anchoredPosition.x, -rt.anchoredPosition.y);
		interactionBlockerRect.sizeDelta = new Vector2(Screen.width, Screen.height);
		
		Sprite itemSprite = uiItem.item.icon;
		itemImage.sprite = itemSprite;
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
		itemRect.sizeDelta = new Vector2((uiData.itemSize - uiData.itemBorderPadding*2)*sizeRatio.x, (uiData.itemSize - uiData.itemBorderPadding*2)*sizeRatio.y);
		itemRect.anchoredPosition = new Vector2(uiData.itemBorderPadding + (uiData.itemSize - uiData.itemBorderPadding*2)*((1-sizeRatio.x)/2), uiData.itemBorderPadding + (uiData.itemSize - uiData.itemBorderPadding*2)*((1-sizeRatio.y)/2));
		
		itemNameLabel.text = uiItem.item.itemName;
		itemMaxQuantityLabel.text = "/" + uiItem.item.quantity;
		int halfMax = uiItem.item.quantity/2;
		quantitySlider.minValue = 1;
		quantitySlider.maxValue = uiItem.item.quantity;
		quantitySlider.value = halfMax;
		quantityInput.SetTextWithoutNotify("" + halfMax);
	}
	
	public void QuantityInputUpdated()
	{
		if(quantityInput.text == "")
		{
			quantityInput.SetTextWithoutNotify("0");
			quantitySlider.SetValueWithoutNotify(0);
			StartCoroutine(MoveCaretToEnd());
			return;
		}
		int inputInt = int.Parse(quantityInput.text);
		if(inputInt < 0)
		{
			quantityInput.SetTextWithoutNotify("0");
			quantitySlider.SetValueWithoutNotify(0);
			StartCoroutine(MoveCaretToEnd());
			return;
		}
		if(inputInt > uiItem.item.quantity - 0)
		{
			quantityInput.SetTextWithoutNotify("" + (uiItem.item.quantity - 0));
			quantitySlider.SetValueWithoutNotify(uiItem.item.quantity - 0);
			return;
		}
		quantityInput.SetTextWithoutNotify("" + inputInt);
		quantitySlider.SetValueWithoutNotify(inputInt);
	}
	
	public void QuantitySliderUpdated()
	{
		quantityInput.SetTextWithoutNotify("" + quantitySlider.value);
	}
	
	public void ConfirmClicked()
	{
		int splitQuantity = int.Parse(quantityInput.text);
		if(splitQuantity <= 0 || splitQuantity >= uiItem.item.quantity)
		{
			return;
		}
		int remainingQuantity = uiItem.item.quantity - splitQuantity;
		GameObject duplicateUIItemObject = Instantiate(uiItem.gameObject, new Vector3(0,0,0), Quaternion.identity, uiItem.resizableWindow.itemParent);
		GameObject duplicateItemObject = Instantiate(uiItem.item.gameObject, new Vector3(0,0,0), Quaternion.identity, uiItem.resizableWindow.container.transform);
		UIItem duplicateUIItem = duplicateUIItemObject.GetComponent<UIItem>();
		Item duplicateItem = duplicateItemObject.GetComponent<Item>();
		duplicateUIItem.name = uiItem.name;
		duplicateItem.name = uiItem.item.name;
		duplicateUIItem.item = duplicateItem;
		duplicateItem.uiItem = duplicateUIItem;
		duplicateItem.recency = uiItem.resizableWindow.container.itemsAdded;
		uiItem.resizableWindow.container.itemsAdded++;
		duplicateItem.quantity = splitQuantity;
		duplicateUIItem.quantityText.text = "" + splitQuantity;
		if(splitQuantity == 1)
		{
			duplicateUIItem.quantityText.gameObject.SetActive(false);
		}
		uiItem.item.quantity = remainingQuantity;
		uiItem.quantityText.text = "" + remainingQuantity;
		if(remainingQuantity == 1)
		{
			uiItem.quantityText.gameObject.SetActive(false);
		}
		uiItem.resizableWindow.uiItems.Add(duplicateUIItem);
		if(uiItem.resizableWindow.container.sortingType >= 0)
		{
			uiItem.resizableWindow.SortInventory(uiItem.resizableWindow.container.sortingType, uiItem.resizableWindow.container.reverseSort, true);
		}
		else
		{
			uiItem.resizableWindow.RearangeContainer();
		}
	}
	
	public IEnumerator MoveCaretToEnd()
	{
		yield return new WaitForEndOfFrame();
		quantityInput.caretPosition = quantityInput.text.Length;
		quantityInput.ForceLabelUpdate();
	}
}