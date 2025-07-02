using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using AIChara;
using System.Linq;

namespace MouseMoveManual
{
    [BepInProcess("HoneySelect2")]
    [BepInPlugin("ManualVR.MouseMoveManual", "MouseMoveManual", "1.0.0")]
    public class MouseMoveManual:BaseUnityPlugin
    {
        internal static ConfigEntry<KeyboardShortcut> middleProgress { get; set; }
        internal static ConfigEntry<float> sensitivityValue { get; private set; }
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin MouseMoveManual is loaded!");
            MouseMoveManual.sensitivityValue = base.Config.Bind<float>("MouseMoveManual > sensitivityValue", "mouse sensitivityValue", 100f, new ConfigDescription("mouse sensitivityValue.", null, Array.Empty<object>()));
            MouseMoveManual.middleProgress = base.Config.Bind<KeyboardShortcut>("MouseMoveManual > Draggers", "Show MouseMoveManual UI", new KeyboardShortcut(KeyCode.W, new KeyCode[]
            {
                KeyCode.LeftControl
            }), new ConfigDescription("Displays a MouseMoveManual UI .", null, Array.Empty<object>()));
			loadBeanList();
			Harmony.CreateAndPatchAll(typeof(MouseMoveManual), null);
        }

        private void Update()
        {
            if (MouseMoveManual.middleProgress.Value.IsDown())
            {
                MouseMoveManual.activeDraggerUI = !MouseMoveManual.activeDraggerUI;
            }
        }

        private void OnGUI()
        {
            if (MouseMoveManual.activeDraggerUI)
            {
                AnimationUI.DrawAnimationUI();
            }
        }

        private static bool activeDraggerUI;
		public static HScene hScene;
		public static HSceneFlagCtrl hFlagCtrl;
		private static bool isFirstFrame = true;
		private static Vector3 preferPosition;
		private static bool startFlag = false;
		private static float num = 0f;
		private static int istate = 0;
		private static float feel;
		public static List<AnimaBean> beanList;
		public static AnimaBean bean;
		public static string[] options ;
		public static int nowAnimationIdx = 0;
		public static string savePath = "UserData/MouseMoveManual.xml";

		[HarmonyPostfix]
		[HarmonyPatch(typeof(HScene), "SetStartAnimationInfo")]
		public static void HScene_SetStartAnimationInfo_Patch(HScene __instance, HSceneSprite ___sprite)
		{
			MouseMoveManual.hScene = __instance;
			
			if (MouseMoveManual.hScene == null)
			{
				return;
			}

			

			List<HScene.AnimationListInfo>[] lstAnimInfo =  Traverse.Create(MouseMoveManual.hScene).Field("lstAnimInfo").GetValue<List<HScene.AnimationListInfo>[]>();
			
			if (lstAnimInfo != null){
				List<HScene.AnimationListInfo> animationListInfo = lstAnimInfo[2];
				MouseMoveManual.options = animationListInfo.Select(e => e.nameAnimation).ToArray();
				//MouseMoveManual.options.ToList().ForEach(e => UnityEngine.Debug.Log("MouseMoveManual.options==========>" + e));
				
				
			}
		}

		public static void loadBeanList()
        {
			beanList = Tools.LoadListFromXml<AnimaBean>(MouseMoveManual.savePath);
			if(beanList == null)
            {
				beanList = new List<AnimaBean>();
            }
        }

		public static void initAnimaBean()
        {
			MouseMoveManual.hFlagCtrl = MouseMoveManual.hScene.ctrlFlag;
			int id = MouseMoveManual.hFlagCtrl.nowAnimationInfo.id;
			string nameAnimation = MouseMoveManual.hFlagCtrl.nowAnimationInfo.nameAnimation;
			bool isFaintness = MouseMoveManual.hFlagCtrl.isFaintness;
			float feel = MouseMoveManual.hScene.ctrlFlag.feel_f;

			//UnityEngine.Debug.Log("id==========>" + id);
			//Debug.Log("nameAnimation==========>" + nameAnimation);
			//UnityEngine.Debug.Log("isFaintness==========>" + isFaintness);

			//if (beanList.Count > 0)
			//{
			//	AnimaBean tBean = beanList.First(e => e.Id == id && e.nameAnimation == nameAnimation && e.isFaintness == isFaintness);

			//	if (tBean != null)
			//	{
			//		MouseMoveManual.bean = tBean;

			//	}
			//}
			int feelStep = 0;
            if (feel > 0.75f)
            {
				feelStep = 1;
            }
			if (MouseMoveManual.bean == null || MouseMoveManual.bean.nameAnimation != nameAnimation|| MouseMoveManual.bean.feelStep != feelStep || MouseMoveManual.bean.isFaintness != isFaintness )
			{
				if (beanList.Count > 0)
                {
					AnimaBean tBean = beanList.FirstOrDefault(e =>
					{
						if (isFaintness)
						{
							return e.Id == id && e.nameAnimation == nameAnimation && e.isFaintness == isFaintness;
						}
						else
						{
							return e.Id == id && e.nameAnimation == nameAnimation && e.feelStep == feelStep && e.isFaintness == isFaintness;
						}
					}
					);
					MouseMoveManual.bean = tBean == null ? new AnimaBean(id, nameAnimation, feelStep, isFaintness) : tBean;
                }
                else
                {
					MouseMoveManual.bean = new AnimaBean(id, nameAnimation, feelStep,isFaintness);

				}
					
				
            }
			//Debug.Log("1MouseMoveManual.bean==========>" + MouseMoveManual.bean.nameAnimation);
		}

		public static void saveBean(AnimaBean bean)
        {
			AnimaBean tBean = null;
			if (beanList.Count > 0)
			{
				tBean = beanList.FirstOrDefault(e =>{
					if (bean.isFaintness)
					{
						return e.Id == bean.Id && e.nameAnimation == bean.nameAnimation && e.isFaintness == bean.isFaintness;
					}
					else
					{
						return e.Id == bean.Id && e.nameAnimation == bean.nameAnimation && e.feelStep == bean.feelStep && e.isFaintness == bean.isFaintness;
					}
				});
				if (tBean != null)
				{
					beanList.Remove(tBean);
				}
			}
			beanList.Add(bean);
			Debug.Log("beanList.Count==========>" + beanList.Count);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Sonyu), "setAnimationParamater")]
		public static void ctrlManual(Sonyu __instance)
		{
			initAnimaBean();
			changeSpeed(__instance, 0f);
			
			Vector3 currentMousePos = Input.mousePosition;
			//UnityEngine.Debug.Log("currentNum==========>"+num);

			AnimatorStateInfo femaleAi = Traverse.Create(__instance).Field("FemaleAi").GetValue<AnimatorStateInfo>();
			//UnityEngine.Debug.Log("isFaintness==========>" + MouseMoveManual.hScene.ctrlFlag.isFaintness);
			//UnityEngine.Debug.Log("preferPosition==========>"+preferPosition);
			//UnityEngine.Debug.Log("currentMousePos==========>"+(currentMousePos.y - preferPosition.y));
			if (isFirstFrame)
			{//第一个点
				preferPosition = currentMousePos;
				isFirstFrame = false;
				return;
			}

			if (Mathf.Pow((currentMousePos.y - preferPosition.y), 2) > 100f)
			{
				startFlag = true;
				if ((currentMousePos.y - preferPosition.y) > 0f)
				{
					num = -1f; //回来
					istate = 22;
				}
				else if ((currentMousePos.y - preferPosition.y) < 0f)
				{
					num = 1f; //进去
							  //__instance.ctrlFlag.feel_f += Random.Range(0f, 0.008f);
							  //__instance.ctrlFlag.isGaugeHit = true;
					istate = 11;
					feel = MouseMoveManual.hScene.ctrlFlag.feel_f;
					if (feel > 100f)
					{
						return;
					}
				}
				
				if (femaleAi.IsName("Idle") || femaleAi.IsName("D_Idle"))
				{
					//__instance.StartProcTrigger(num);
					Traverse.Create(__instance).Method("StartProcTrigger", new object[] { num }).GetValue();
					//__instance.StartProc(false, 0, 1, num);
					Traverse.Create(__instance).Method("StartProc", new object[] { false, 0, 1, num }).GetValue();
					//__instance.voice.HouchiTime += Time.unscaledDeltaTime;
					HVoiceCtrl voice = Traverse.Create(__instance).Field("voice").GetValue<HVoiceCtrl>();
					voice.HouchiTime += Time.unscaledDeltaTime;
					UnityEngine.Debug.Log("currentNum==========>" + num);
				}
				if (femaleAi.IsName("Orgasm_IN_A"))
				{
					//__instance.AfterTheInsideWaitingProc(0, 1);
					Traverse.Create(__instance).Method("AfterTheInsideWaitingProc", new object[] {  0, 1 }).GetValue();
					UnityEngine.Debug.Log("Orgasm_IN_A==========>" + num);
				}
				if (femaleAi.IsName("D_Orgasm_IN_A"))
				{
					//__instance.AfterTheInsideWaitingProc(1, 1);
					Traverse.Create(__instance).Method("AfterTheInsideWaitingProc", new object[] { 1, 1 }).GetValue();
					UnityEngine.Debug.Log("D_Orgasm_IN_A==========>" + num);
				}
			}
			if (bean.TProgress > bean.MidProgress)
			{
				startFlag = false;
				if (num < 0)
				{
					startFlag = true;
				}
			}
			if (bean.TProgress > bean.ForwardProgress)
			{
				startFlag = false;
				if (num > 0)
				{
					bean.TProgress = bean.StartProgress;
					startFlag = true;
				}
			}

			if (startFlag)
			{
				float progress = 0f;
				if (num > 0)
				{
					bean.TProgress += Time.deltaTime * bean.Multiple;
					//UnityEngine.Debug.Log("TProgress11==========>" + progress);
				}
				else
				{
					bean.TProgress += Time.deltaTime;
					//UnityEngine.Debug.Log("TProgress22==========>" + bean.TProgress);
				}
			}
			//UnityEngine.Debug.Log("feel==========>"+feel);
			//UnityEngine.Debug.Log("__instance==========>"+__instance.ctrlFlag.selectAnimationListInfo);
			if (feel < 0.75f)
			{
				if (femaleAi.IsName("WLoop"))
				{
					playAnima(__instance, "SLoop", bean.TProgress);
				}
				if (femaleAi.IsName("SLoop"))
				{
					playAnima(__instance, "SLoop", bean.TProgress);
				}
				if (femaleAi.IsName("D_WLoop") && MouseMoveManual.hScene.ctrlFlag.isFaintness)
				{
					playAnima(__instance, "D_SLoop", bean.TProgress);
				}
				else if (femaleAi.IsName("D_WLoop") && !MouseMoveManual.hScene.ctrlFlag.isFaintness)
				{
					playAnima(__instance, "SLoop", bean.TProgress);
				}
				if (femaleAi.IsName("D_SLoop") && MouseMoveManual.hScene.ctrlFlag.isFaintness)
				{
					playAnima(__instance, "D_SLoop", bean.TProgress);
				}
				else if (femaleAi.IsName("D_SLoop") && !MouseMoveManual.hScene.ctrlFlag.isFaintness)
				{
					playAnima(__instance, "SLoop", bean.TProgress);
				}
				if (femaleAi.IsName("OLoop"))
				{
					playAnima(__instance, "OrgasmF_IN", bean.TProgress * 0.4f);
					bean.Multiple = 1f;
				}
				if (femaleAi.IsName("D_OLoop"))
				{
					playAnima(__instance, "D_OrgasmF_IN", bean.TProgress * 0.3f);
					bean.Multiple = 1f;
				}
			}
			else
			{

				//UnityEngine.Debug.Log("femaleAi==========>"+femaleAi);
				if (femaleAi.IsName("OLoop"))
				{
					playAnima(__instance, "OLoop", bean.TProgress * 0.4f);
					bean.Multiple = 1f;
				}
				if (femaleAi.IsName("D_OLoop"))
				{
					playAnima(__instance, "D_OLoop", bean.TProgress * 0.3f);
					bean.Multiple = 1f;
				}
			}

			//float num = Input.GetAxis("Mouse ScrollWheel");
			preferPosition = currentMousePos;
			
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FeelHit), "isHit")]
		public static bool submitHit(ref bool __result)
		{
			if (istate == 11)
			{
				//UnityEngine.Debug.Log("__result--->"+__result);
				__result = true;
			}
			else
			{
				//UnityEngine.Debug.Log("__result--->"+__result);
				__result = false;
			}
			return false;
		}

		public static void playAnima(Sonyu sonyu, string ahash, float progress)
		{
			ChaControl[] chaFemales = Traverse.Create(sonyu).Field("chaFemales").GetValue<ChaControl[]>();
			chaFemales[0].animBody.Play(ahash, -1, progress);
			ChaControl[] chaMales = Traverse.Create(sonyu).Field("chaMales").GetValue<ChaControl[]>();
			chaMales[0].animBody.Play(ahash, -1, progress);
			//sonyu.chaMales[0].animBody.SetFloat("offset",1.8f);

		}

		public static void changeSpeed(Sonyu sonyu, float speedNum)
		{
			ChaControl[] chaFemales = Traverse.Create(sonyu).Field("chaFemales").GetValue<ChaControl[]>();
			chaFemales[0].setAnimatorParamFloat("speed", speedNum);
			ChaControl[] chaMales = Traverse.Create(sonyu).Field("chaMales").GetValue<ChaControl[]>();
			chaMales[0].setAnimatorParamFloat("speed", speedNum);
		}
	}
}
