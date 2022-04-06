using Modding;
using System.Reflection;
using System;
namespace CustomPantheon
{
    public partial class CustomPantheon:Mod,IGlobalSettings<Setting>,ITogglableMod
    {
		public static Setting gs = new Setting();
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
			Log("Set Custom Door");
		}

        public void OnLoadGlobal(Setting s) => gs = s;
		public Setting OnSaveGlobal() => gs;
		public void Unload()
        {
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AddDoor;
			ModHooks.LanguageGetHook -= ChangeDoorText;
			ModHooks.GetPlayerVariableHook -= ChangeCustomDoorVar;
		}
	}
}
