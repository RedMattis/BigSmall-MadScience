﻿
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse.Noise;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using RimWorld.QuestGen;
using System.Text;

namespace BigAndSmall
{
    [StaticConstructorOnStartup]
    internal class Main : Mod
    {

        //public static DryadSettings settings;
        public Main(ModContentPack content) : base(content)
        {
            //settings = GetSettings<DryadSettings>();
            //ApplyHarmonyPatches();
            
        }

        public override string SettingsCategory()
        {
            return "Dryad_DryadsReworkedSettings".Translate();
        }

        //static void ApplyHarmonyPatches()
        //{
        //    var harmony = new Harmony("RedMattis.MadScienceApril");
        //    harmony.PatchAll();
        //}

        //public override void DoSettingsWindowContents(Rect inRect)
        //{
           
        //}

        //private static void CreateSettingsSlider(Listing_Standard listingStandard, string labelName, ref float value)
        //{
        //    // Define a total rect for one row of slider and label
        //    Rect fullRow = listingStandard.GetRect(Text.LineHeight);

        //    // Divide the row into segments for the label, the slider, and the value text
        //    float labelWidth = fullRow.width * 0.5f;  // 50% for label
        //    float sliderWidth = fullRow.width * 0.35f; // 35% for slider
        //    float valueWidth = fullRow.width * 0.15f;  // 15% for value display

        //    Rect labelRect = new(fullRow.x, fullRow.y, labelWidth, fullRow.height);
        //    Rect sliderRect = new(labelRect.xMax, fullRow.y, sliderWidth, fullRow.height);
        //    Rect valueRect = new(sliderRect.xMax, fullRow.y, valueWidth, fullRow.height);

        //    // Draw the label, slider, and value on the respective Rects
        //    Widgets.Label(labelRect, labelName);
        //    value = Widgets.HorizontalSlider(sliderRect, value, 0f, 4f, true);
        //    Widgets.Label(valueRect, $"{value * 100:F0}%");
        //}

        //public static void CreateSettingCheckbox(Listing_Standard listingStandard, string labelName, ref bool value)
        //{
        //    Rect fullRow = listingStandard.GetRect(Text.LineHeight);
        //    // Divide the row into two segments for the label and the checkbox
        //    float labelWidth = fullRow.width * 0.90f;
        //    float checkboxWidth = fullRow.width * 0.1f;
        //    Rect labelRect = new(fullRow.x, fullRow.y, labelWidth, fullRow.height);
        //    Rect checkboxRect = new(labelRect.xMax, fullRow.y, checkboxWidth, fullRow.height);

        //    Widgets.Label(labelRect, labelName);
        //    Widgets.Checkbox(checkboxRect.position, ref value);
        //}
    }
    
    //public class DryadSettings : ModSettings
    //{
    //    public const float defaultMechPenaltyScale = 1f;
    //    public float mechPenaltyScale = defaultMechPenaltyScale;

    //    public const float defaultTurretSpawnTime = 1;
    //    public float turretSpawnTime = defaultTurretSpawnTime;

    //    public const bool defaultNoAwakendDryads = true;
    //    public bool noAwakendDryads = defaultNoAwakendDryads;

    //    public override void ExposeData()
    //    {
    //        Scribe_Values.Look(ref mechPenaltyScale, "mechPenalty", defaultMechPenaltyScale);
    //        Scribe_Values.Look(ref turretSpawnTime, "turretSpawnTime", defaultTurretSpawnTime);
    //        Scribe_Values.Look(ref noAwakendDryads, "noAwakendDryads", defaultNoAwakendDryads);
    //        base.ExposeData();
    //    }

    //    public void ResetToDefault()
    //    {
    //        mechPenaltyScale = defaultMechPenaltyScale;
    //        turretSpawnTime = defaultTurretSpawnTime;
    //    }
    //}

}
