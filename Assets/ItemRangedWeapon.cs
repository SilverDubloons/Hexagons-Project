using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ItemRangedWeapon : MonoBehaviour
{
	public TMP_InputField rangeInput;
	
	[System.Serializable]
    public class RangeMod
	{
		public int distance;
		public int modifier;
		public RangeMod(int distance, int modifier)
		{
			this.distance = distance;
			this.modifier = modifier;
		}
	}
	public RangeMod[] rangeMods;
	public int rangeBasedAccuracy;
	
	public RangeMod[] CopyRangeMods(RangeMod[] source)
	{
		RangeMod[] destination = new RangeMod[source.Length];
		for(int i = 0; i < source.Length; i++)
		{
			destination[i] = new RangeMod(source[i].distance, source[i].modifier);
		}
		return destination;
	}
	
	public RangeMod[] AdjustRangeModsByRangeBasedAccuracy(RangeMod[] oldRangeMods, int rba)
	{
		RangeMod[] adjustedRangeMods = new RangeMod[oldRangeMods.Length];
		
		for(int i = 0; i < adjustedRangeMods.Length; i++)
		{
			adjustedRangeMods[i] = new RangeMod(oldRangeMods[i].distance, oldRangeMods[i].modifier);
			if(adjustedRangeMods[i].modifier < 0)
			{
				adjustedRangeMods[i].modifier = Mathf.RoundToInt((float)adjustedRangeMods[i].modifier * (1f - (float)rba / 100f));
				if(rba >= 100)
				{
					adjustedRangeMods[i].modifier = 0;
				}
			}
			else
			{
				adjustedRangeMods[i].modifier = Mathf.RoundToInt((float)adjustedRangeMods[i].modifier * (1f + (float)rba / 100f));
				if(rba <= -100)
				{
					adjustedRangeMods[i].modifier = 0;
				}
			}
			if(rba > 100)
			{
				adjustedRangeMods[i].modifier += rba - 100;
			}
			if(rba < -100)
			{
				adjustedRangeMods[i].modifier += rba + 100;
			}
		}
		
		return adjustedRangeMods;
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
	
	public void TestClicked()
	{
		//print("At range " + rangeInput.text + " modifier is " + GetRangeMod(int.Parse(rangeInput.text)));
		//DrawRangeModifierGraph();
		UIData uiData = GameObject.FindWithTag("UIData").GetComponent<UIData>();
		GameObject newGraph = Instantiate(uiData.rangeGraphPrefab,new Vector3(0,0,0), Quaternion.identity, uiData.rangeGraphParent);
		RangeGraph rangeGraph = newGraph.GetComponent<RangeGraph>();
		rangeGraph.rangeMods = CopyRangeMods(rangeMods);
		rangeGraph.rangeMods = AdjustRangeModsByRangeBasedAccuracy(rangeGraph.rangeMods, rangeBasedAccuracy);
		rangeGraph.uiData = uiData;
		rangeGraph.weapon = this;
		rangeGraph.ResetGraphProperties();
		rangeGraph.DrawRangeModifierGraph();
	}
}
