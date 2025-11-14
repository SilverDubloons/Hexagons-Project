using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceWheel : MonoBehaviour
{
	public UnityEngine.UI.Image critSuccessImage;
	public UnityEngine.UI.Image successImage;
	public UnityEngine.UI.Image grazeImage;
	public UnityEngine.UI.Image failImage;
	//public UnityEngine.UI.Image critFailImage;
	
	void Start()
	{
		SetChanceWheel(9,31,40,19,1);
	}
	
    void SetChanceWheel(int critSuccess, int success, int graze, int fail, int critFail)
	{
		critSuccessImage.fillAmount = critSuccess / 100f;
		successImage.fillAmount = success / 100f + critSuccess / 100f;
		grazeImage.fillAmount = graze / 100f + success / 100f + critSuccess / 100f;
		failImage.fillAmount = fail / 100f + graze / 100f + success / 100f + critSuccess / 100f;
		//critSuccessImage.fillAmount = critSuccess / 100f;
	}
}
