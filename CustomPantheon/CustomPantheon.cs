using Modding;
using System.Reflection;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
namespace CustomPantheon
{
    public partial class CustomPantheon:Mod,IGlobalSettings<Setting>,ITogglableMod
    {
		private List<string> scenes = new List<string> { "GG_Hollow_Knight", "GG_Radiance", "GG_Grimm_Nightmare" };
		public static Setting gs = new Setting();
		public bool isCustom = false;
        public override string GetVersion()
        {
			return string.Concat(new string[]
			{
				Assembly.GetAssembly(typeof(CustomPantheon)).GetName().Version.ToString(),
				": ",
				gs.Super,
				" ",
				gs.Title,
				" ",
				gs.Desc,
				"_1.5"
			});
		}
        public override void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AddDoor;
            ModHooks.LanguageGetHook += ChangeDoorText;
            ModHooks.GetPlayerVariableHook += ChangeCustomDoorVar;
            On.PlayMakerFSM.Start += ModifyRadiance;
            On.BossSequenceController.SetupNewSequence += BossSequenceController_SetupNewSequence;
            On.BossSceneController.Start += CheckHUD;
        }

        private IEnumerator CheckHUD(On.BossSceneController.orig_Start orig, BossSceneController self)
        {
			//from HUDInChecker
			yield return orig(self);
			if(BossSequenceController.IsInSequence&&!scenes.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
				yield return new WaitUntil(() => GameManager.instance.gameState == GameState.PLAYING);
				GameCameras.instance.hudCanvas.LocateMyFSM("Slide Out").SendEvent("IN");
			}
        }

        private void BossSequenceController_SetupNewSequence(On.BossSequenceController.orig_SetupNewSequence orig, BossSequence sequence, BossSequenceController.ChallengeBindings bindings, string playerData)
        {
            orig(sequence, bindings, playerData);
			if (sequence.achievementKey == "")
			{
				isCustom = true;
			}
			else
			{
				isCustom = false;
			}
		}

        private void ModifyRadiance(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
			orig(self);
			if(self is
                {
					name:"Absolute Radiance",
					FsmName:"Control"
                }&&isCustom)
            {
				self.GetAction<SetStaticVariable>("Ending Scene", 1).setValue.boolValue = false;
            }
        }

        private object ChangeCustomDoorVar(Type type, string name, object value)
        {
            if(name== "CustomBossDoor")
            {
				return new BossSequenceDoor.Completion
				{
					allBindings=true,
					boundCharms=true,
					boundNail=true,
					boundShell=true,
					boundSoul=true,
					canUnlock=true,
					completed=true,
					noHits=true,
					unlocked=true,
					viewedBossSceneCompletions=gs.PantheonRooms
				};
            }
			return value;
        }

		private string ChangeDoorText(string key, string sheetTitle, string orig) => key switch
		{
			"CustomBossDoorDesc"=>gs.Desc,
			"CustomBossDoorTitle"=>gs.Title,
			"CustomBossDoorSuper"=>gs.Super,
			_=>orig

		};

        private void AddDoor(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
			bool flag = arg1.name == "GG_Atrium";
			if (flag)
			{
				GameManager.instance.StartCoroutine(SetPantheon());
			}
			if(arg0.name== "GG_End_Sequence")
            {
				HeroController.instance.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
			Log("Set Custom Door");
		}

        public void OnLoadGlobal(Setting s) => gs = s;
		public Setting OnSaveGlobal() => gs;
		public void Unload()
        {
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AddDoor;
			ModHooks.LanguageGetHook -= ChangeDoorText;
			ModHooks.GetPlayerVariableHook -= ChangeCustomDoorVar;
			On.PlayMakerFSM.Start -= ModifyRadiance;
			On.BossSequenceController.SetupNewSequence -= BossSequenceController_SetupNewSequence;
			On.BossSceneController.Start -= CheckHUD;
		}
	
	}
}
