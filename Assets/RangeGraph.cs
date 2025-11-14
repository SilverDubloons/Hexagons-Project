using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ItemRangedWeapon;

public class RangeGraph : MonoBehaviour
{
	public UIData uiData;
	public RectTransform graphRectTransform;
	public RectTransform graphDot;
	public RectTransform gridArea;
	public Transform gridObjectsParent;
	public RectTransform coordinatesRectTransform;
	public TMP_Text coordinatesText;
	public ItemRangedWeapon weapon;
	private Vector2 graphBottomLeft;
	private Vector2 graphTopRight;
	private Vector2 graphLabelXRange;
	private float graphLabelXVerticalPosition;
	private Vector2 graphLabelYRange;
	private float graphLabelYHorizontalPosition;
	private Vector2 graphSize;
	private float graphLineWidth;
	private int minXLabel;
	private int maxXLabel;
	private int minYLabel;
	private int maxYLabel;
	private int xRange;
	private int yRange;
	private int minX;
	private int maxX;
	private int minY;
	private int maxY;
	public RangeMod[] rangeMods;

	public void ResetGraphProperties()
	{
		graphBottomLeft = new Vector2(82,52);
		graphTopRight = new Vector2(758,378);
		graphLabelXRange = new Vector2(40,716);
		graphLabelXVerticalPosition = -5f;
		graphLabelYRange = new Vector2(29,355);
		graphLabelYHorizontalPosition = -12f;
		graphLineWidth = 4;
		graphSize = new Vector2(graphTopRight.x - graphBottomLeft.x, graphTopRight.y - graphBottomLeft.y);
	}
	
	public int GetRangeMod(int range)
	{
		if(range > rangeMods[rangeMods.Length-1].distance)
		{
			return -200;
		}
		if(range == 0)
		{
			return rangeMods[0].modifier;
		}
		for(int i = 1; i < rangeMods.Length; i++)
		{
			if(range <= rangeMods[i].distance)
			{
				float distanceDifference = rangeMods[i].distance - rangeMods[i-1].distance;
				float distanceFromStart =  range - rangeMods[i-1].distance;
				float distanceT = distanceFromStart / distanceDifference;
				float rangeMod = Mathf.Lerp(rangeMods[i-1].modifier, rangeMods[i].modifier, distanceT);
				return Mathf.RoundToInt(rangeMod);
			}
		}
		return -200;
	}
	
	public int CountDigits(int num)
	{
		return (int)Mathf.Floor(Mathf.Log10(num) +1);
	}
	
	public int FindClosestIndexToNumber(List<int> numbers, int targetValue)
	{
		int closestIndex = 0;
		int minDifference = int.MaxValue;
		
		for(int i = 0; i < numbers.Count; i++)
		{
			int difference = Mathf.Abs(numbers[i] - targetValue);
			if(difference < minDifference)
			{
				minDifference = difference;
				closestIndex = i;
			}
		}
		return closestIndex;
	}
	
	public void DrawRangeModifierGraph()
	{
		minX = rangeMods[0].distance;					// X is Distance
		maxX = rangeMods[rangeMods.Length-1].distance;	// rangeMods should always be in order of distance
		minY = int.MaxValue;							// Y is Modifier
		maxY = int.MinValue;
		
		for(int i = 0; i < rangeMods.Length; i++)
		{
			if(rangeMods[i].modifier < minY)
			{
				minY = rangeMods[i].modifier;
			}
			if(rangeMods[i].modifier > maxY)
			{
				maxY = rangeMods[i].modifier;
			}
		}
		
		xRange = maxX - minX;
		List<int> totalLabelsList = new List<int>();
		for(int j = 5; j < 50; j+=5)
		{
			if(j > 10)
			{
				j+=5;	// 5, 10, 20, 30, 40, 50
			}
			int minXScaleTemp = minX;
			while(minXScaleTemp % 5 != 0)
			{
				minXScaleTemp -= 1;
			}
			while(minXScaleTemp % j != 0)
			{
				minXScaleTemp -= 5;
			}
			int maxXScaleTemp = maxX;
			while(maxXScaleTemp % 5 != 0)
			{
				maxXScaleTemp += 1;
			}
			while(maxXScaleTemp % j != 0)
			{
				maxXScaleTemp +=5;
			}
			int totalScaleSections = (maxXScaleTemp - minXScaleTemp) / j + 1;
			totalLabelsList.Add(totalScaleSections);
		}
		int totalLabelsIndexClosestToDesiredNumber = FindClosestIndexToNumber(totalLabelsList, 7);
		int totalLabels = totalLabelsList[totalLabelsIndexClosestToDesiredNumber];
		int labelScale;
		switch(totalLabelsIndexClosestToDesiredNumber)
		{
			case 0:
			labelScale = 5;
			break;
			case 1:
			labelScale = 10;
			break;
			case 2:
			labelScale = 20;
			break;
			case 3:
			labelScale = 30;
			break;
			case 4:
			labelScale = 40;
			break;
			case 5:
			labelScale = 50;
			break;
			default:
			labelScale = 100;
			break;
		}
		minXLabel = minX;
		while(minXLabel % 5 != 0)
		{
			minXLabel -= 1;
		}
		while(minXLabel % labelScale !=0)
		{
			minXLabel -= 5;
		}
		maxXLabel = minXLabel + labelScale * (totalLabels - 1);
		for(int k = 0; k < totalLabels; k++)
		{
			GameObject newLabel = Instantiate(uiData.graphAxisLabelXPrefab, new Vector3(0,0,0), Quaternion.identity, gridObjectsParent);
			RectTransform rt = newLabel.GetComponent<RectTransform>();
			rt.anchoredPosition = new Vector2(graphLabelXRange.x + (graphLabelXRange.y - graphLabelXRange.x) * ((float)k / (totalLabels - 1)), graphLabelXVerticalPosition);
			newLabel.GetComponent<TMP_Text>().text = "" + (minXLabel + labelScale * k);
		}
		
		yRange = maxY - minY;
		totalLabelsList.Clear();
		for(int l = 5; l < 50; l+=5)
		{
			if(l > 10)
			{
				l+=5;	// 5, 10, 20, 30, 40, 50
			}
			int minYScaleTemp = minY;
			while(minYScaleTemp % 5 != 0)
			{
				minYScaleTemp -= 1;
			}
			while(minYScaleTemp % l != 0)
			{
				minYScaleTemp -= 5;
			}
			int maxYScaleTemp = maxY;
			while(maxYScaleTemp % 5 != 0)
			{
				maxYScaleTemp += 1;
			}
			while(maxYScaleTemp % l != 0)
			{
				maxYScaleTemp +=5;
			}
			int totalScaleSections = (maxYScaleTemp - minYScaleTemp) / l + 1;
			totalLabelsList.Add(totalScaleSections);
		}
		totalLabelsIndexClosestToDesiredNumber = FindClosestIndexToNumber(totalLabelsList, 7);
		totalLabels = totalLabelsList[totalLabelsIndexClosestToDesiredNumber];
		switch(totalLabelsIndexClosestToDesiredNumber)
		{
			case 0:
			labelScale = 5;
			break;
			case 1:
			labelScale = 10;
			break;
			case 2:
			labelScale = 20;
			break;
			case 3:
			labelScale = 30;
			break;
			case 4:
			labelScale = 40;
			break;
			case 5:
			labelScale = 50;
			break;
			default:
			labelScale = 100;
			break;
		}
		minYLabel = minY;
		while(minYLabel % 5 != 0)
		{
			minYLabel -= 1;
		}
		while(minYLabel % labelScale !=0)
		{
			minYLabel -= 5;
			
		}
		for(int m = 0; m < totalLabels; m++)
		{
			GameObject newLabel = Instantiate(uiData.graphAxisLabelYPrefab, new Vector3(0,0,0), Quaternion.identity, gridObjectsParent);
			RectTransform rt = newLabel.GetComponent<RectTransform>();
			rt.anchoredPosition = new Vector2(graphLabelYHorizontalPosition, graphLabelYRange.x + (graphLabelYRange.y - graphLabelYRange.x) * ((float)m / (totalLabels - 1)));
			newLabel.GetComponent<TMP_Text>().text = "" + (minYLabel + labelScale * m);
		}
		float graphHeight = graphTopRight.y - graphBottomLeft.y;
		float graphWidth = graphTopRight.x - graphBottomLeft.x;
		maxYLabel = minYLabel + labelScale * (totalLabels - 1);
		float labelScaleSizeY = maxYLabel - minYLabel;
		float labelScaleSizeX = maxXLabel - minXLabel;
		if(minYLabel < minY)
		{
			float differenceFromBottomLabel = minY - minYLabel;
			float newBottom = graphBottomLeft.y + graphHeight * (differenceFromBottomLabel / labelScaleSizeY);
			graphBottomLeft = new Vector2(graphBottomLeft.x, newBottom);
		}
		if(maxYLabel > maxY)
		{
			float differenceFromTopLabel = maxYLabel - maxY;
			float newTop = graphTopRight.y - graphHeight * (differenceFromTopLabel / labelScaleSizeY);
			graphTopRight = new Vector2(graphTopRight.x, newTop);
		}
		if(minXLabel < minX)
		{
			float differenceFromLeftLabel = minX - minXLabel;
			float newLeft = graphBottomLeft.x + graphWidth * (differenceFromLeftLabel / labelScaleSizeX);
			graphBottomLeft = new Vector2(newLeft, graphBottomLeft.y);
		}
		if(maxXLabel > maxX)
		{
			float differenceFromRightLabel = maxXLabel - maxX;
			float newRight = graphTopRight.x - graphWidth * (differenceFromRightLabel / labelScaleSizeX);
			graphTopRight = new Vector2(newRight, graphTopRight.y);
		}
		graphSize = new Vector2(graphTopRight.x - graphBottomLeft.x, graphTopRight.y - graphBottomLeft.y);
		
		for(int a = 1; a < rangeMods.Length; a++)
		{
			GameObject newLine = Instantiate(uiData.graphLinePrefab,new Vector3(0,0,0), Quaternion.identity, gridObjectsParent);
			RectTransform rt = newLine.GetComponent<RectTransform>();
			float xPos1 = (((float)rangeMods[a-1].distance - minX) / xRange) * graphSize.x + graphBottomLeft.x;
			float yPos1 = (((float)rangeMods[a-1].modifier - minY) / yRange) * graphSize.y + graphBottomLeft.y;
			Vector2 firstPoint = new Vector2(xPos1, yPos1);
			rt.anchoredPosition = new Vector2(xPos1, yPos1 + graphLineWidth/2);
			newLine.name = "" + a;
			float xPos2 = (((float)rangeMods[a].distance - minX) / xRange) * graphSize.x + graphBottomLeft.x;
			float yPos2 = (((float)rangeMods[a].modifier - minY) / yRange) * graphSize.y + graphBottomLeft.y;
			Vector2 secondPoint = new Vector2(xPos2, yPos2);
			float angle = Mathf.Atan((secondPoint.y - firstPoint.y) / (secondPoint.x - firstPoint.x));
			rt.localEulerAngles = new Vector3(0,0,angle * Mathf.Rad2Deg - 90);
			float distance = Mathf.Sqrt(Mathf.Pow((secondPoint.x - firstPoint.x), 2) + (Mathf.Pow((secondPoint.y - firstPoint.y), 2)));
			rt.sizeDelta = new Vector2 (graphLineWidth, distance);
		}
		ResetGraphProperties();
	}
	
	
	void Update()
	{
		Vector2 mousePos = Input.mousePosition;
		Rect gridRect = GetScreenCoordinates(gridArea);
		if(gridRect.Contains(mousePos))
		{
			int gridX = Mathf.RoundToInt(((mousePos.x - gridRect.xMin) / (gridRect.xMax - gridRect.xMin)) * (maxXLabel - minXLabel));
			int gridY = GetRangeMod(gridX);
			if(gridX >= minX && gridX <= maxX)
			{
				graphDot.gameObject.SetActive(true);
				float graphDotX = 82 + (756-82) * ((float)(gridX - minXLabel) / (maxXLabel - minXLabel));
				float graphDotY = 52 + (378-52) * ((float)(gridY - minYLabel) / (maxYLabel - minYLabel));
				graphDot.anchoredPosition = new Vector2(graphDotX, graphDotY);
				coordinatesRectTransform.gameObject.SetActive(true);
				coordinatesText.text = "" + gridX + ", " + gridY;
				Vector2 normalizedMousePos = new Vector2(mousePos.x/Screen.width,mousePos.y/Screen.height);
				coordinatesRectTransform.anchoredPosition = new Vector2(normalizedMousePos.x * 2560, normalizedMousePos.y * 1440);
			}
			else
			{
				graphDot.gameObject.SetActive(false);
				coordinatesRectTransform.gameObject.SetActive(false);
			}
		}
		else
		{
			graphDot.gameObject.SetActive(false);
			coordinatesRectTransform.gameObject.SetActive(false);
		}
		/* Vector2 graphPos = graphRectTransform.anchoredPosition;
		Vector2 relativePos = mousePos - graphPos;
		if(relativePos.x >= graphBottomLeft.x && relativePos.x <= graphTopRight.x && relativePos.y >= graphBottomLeft.y && relativePos.y <= graphTopRight.y)
		{
			graphDot.gameObject.SetActive(true);
		}
		else
		{
			graphDot.gameObject.SetActive(false);
		} */
		//print("MousePos = " + mousePos + " graphPos = " + graphPos + " relativePos = " + relativePos);
		
	}
	
	public Rect GetScreenCoordinates(RectTransform uiElement)
	{
	  var worldCorners = new Vector3[4];
	  uiElement.GetWorldCorners(worldCorners);
	  var result = new Rect(
		worldCorners[0].x,
		worldCorners[0].y,
		worldCorners[2].x - worldCorners[0].x,
		worldCorners[2].y - worldCorners[0].y);
	  return result;
	}
}
