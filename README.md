# Warhammer 40K: Rogue Trader - AI Voiceover Mod
By [pas2k](https://github.com/pas2k)
Based on [SpeechMod](https://github.com/Osmodium/W40KRogueTraderSpeechMod) by [Osmodium](https://github.com/Osmodium)


## Acknowledgements

This mod would not be possible without massive work put into its foundation, SpeechMod by Osmodium.

This mod is intended to be merged back into Osmodium's  when the following major issues are resolved:

 - Spatial WWise playback instead of NAudio
 - Direct UUID resolution for the strings instead of 
 - Cross-platform support

## This mod allows playing pre-rendered voiceover sound clips, and uses built-in SpeechMod as a fallback.

It only works in English version of the game. Anything other than dialogues will lack voiceover for languages other than English.


## Known issues for the mod:

 - Only Windows is supported as of now
 - Due to the game's usage of Wwise as a sound engine, it uses NAudio for a playback, leading to following issues
 - Voiceover doesn't decrease the volume of the background music like it does for original voice lines
 - Voiceover is not affected by the game volume controls
 - Conflicts with original SpeechMod


## Known issues for the voiceover pack
 - Shorter cues might have artifacts/repeats
 - The "computer diagogues" are funky
 - The pronounciation of some 40k-specific terms can be wrong/uneven
 - Due to difficulty of statically resolving dialogues, some voices might be wrong
 - Voices have a limited roster of emotions/archetypes/voice actors (only WH40KRT references were used)

---

### How to install 📝

 1. Download the W40K_AIVOMod file and unzip.
 2. Please note that the game comes with its own built in Unity Mod Manager so you do not need to install another one.
 3. Navigate to *%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\\*
 4. Copy the *W40K_AIVOMod* folder into the *UnityModManager* folder
 5. Unpack the voiceover pack into *W40K_AIVOMod\tts* folder
 6. Use the included CheckInstallation.exe to confirm that everything is in the correct place.
 7. Launch Warhammer 40K: Rogue Trader, you may need to hit **ctrl+F10** to see the mod manager window.

---