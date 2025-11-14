using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Might need things like crit chance/chance to hit/graze for specific skills.

public class Statistics : MonoBehaviour
{
    //	Attributes
    int strength;
	int dexterity;
	int constitution;
	int perception;
	int intelligence;
	int charisma;
	int luck;
	
	// Derived Statistics
	int maxHP;
	int curHP;
	int healingRate;
	
	int accuracyBase;
	int rangeBasedAccuracy;
	
	int criticalChance;
	int criticalChanceCombat;
	int criticalChanceSkill;
	int criticalChanceDialog;
	int criticalChanceStealth;
	
	int criticalFailureChance;
	int criticalFailureChanceCombat;
	int criticalFailureChanceSkill;
	int criticalFailureChanceDialog;
	int criticalFailureChanceStealth;
	
	int experienceBonus;
	int maxCompanions;
	
	int maxCarryWeight;		// kg
	int implantTollerance;
	
	int armorHandling;
	int initiative;
	int scrounge;
	
	int resistance;
	int radiationResistance;
	int poisonResistance;
	int injuryResistance;
	int psychicResistance;
	int sonicResistance;
	
	int dodge;
	
	int[] threshold = new int[4];				// Need these for torso, arms, legs, and head
	int protection;
	int bladedThreshold;
	int bladedProtection;
	int bluntThreshold;
	int bluntProtection;
	int piercingThreshold;
	int piercingProtection;
	int explosiveThreshold;
	int explosiveProtection;
	int empThreshold;
	int empProtection;
	int acidThreshold;
	int acidProtection;
	int laserThreshold;
	int laserProtection;
	int plasmaThreshold;
	int plasmaProtection;
}
