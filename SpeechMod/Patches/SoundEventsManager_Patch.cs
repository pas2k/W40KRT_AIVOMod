using HarmonyLib;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Sound.Base;
using UnityEngine;
using System.Reflection;
using Kingmaker.UI.Sound;

namespace AiVoiceoverMod.Patches;

[HarmonyPatch]
public class SoundEventsManager_Patch
{
    //[HarmonyPatch(typeof(SoundEventsManager), "PostEventInternal", typeof(string), typeof(GameObject), typeof(bool))]
    //[HarmonyPrefix]
    //public static void PostEventInternal(string eventName, GameObject gameObject, bool canBeStopped = false) 
    //{
    //    //UnityEngine.Debug.LogWarning($"Playing {eventName}!");
    //}

    static Type akSoundEngine;

    static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("Kingmaker.Sound.Base.SoundEventsManager");
        var methods = type.GetMethods();
        UnityEngine.Debug.LogWarning($"Initializing ZLKHSKDHAHODI!");

        foreach (Type t in AccessTools.AllTypes())
        {
            if (t.FullName == "AkSoundEngine")
            {
                akSoundEngine = t;
                UnityEngine.Debug.LogWarning($"Found AkSoundEngine!");
                var getIdFromString = akSoundEngine.GetMethod("GetIDFromString");
                if (getIdFromString != null)
                {
                    UnityEngine.Debug.LogWarning($"Found GetIDFromString!");
                    UnityEngine.Debug.LogWarning($"Found {getIdFromString.Invoke(null, new object[] { "BNTRS_Chapter3_Heinrix_128" })}!"); 
                }
                break;
            }
        }
        foreach ( var m in methods )
        {
            UnityEngine.Debug.LogWarning($"Meth: {m}!");
            if (m.Name == "PostEvent" && m.GetParameters().Length > 3)
            {
                return m;
            }
        }
        return null;

    }

    static void Prefix(object[] __args)
    {
        uint id = 0;
        try {
            var getIdFromString = akSoundEngine.GetMethod("GetIDFromString");
            id = (uint)getIdFromString.Invoke(null, new object[] { __args[0] });
        } catch (Exception e) {
            UnityEngine.Debug.LogWarning($"Err {akSoundEngine} {e}!");
        }
        //UnityEngine.Debug.LogWarning($"Playing2 {__args[0]} (id is {id})!");

        //VoiceOverPlayer.PlayVoiceOver
    }


    //[HarmonyPatch(typeof(SoundEventsManager), "PostEventInternal", typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object))]
    //[HarmonyPrefix]
    //public static void PostEventInternal(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
    //{
    //    UnityEngine.Debug.LogWarning($"Playing2 {eventName}!");
    //}

    // There is also private uint PostEventInternal(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
}
