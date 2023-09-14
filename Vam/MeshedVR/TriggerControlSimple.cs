using System;
using UnityEngine;
using System.Collections;
using AssetBundles;

namespace MVRPlugin {

	// This class demonstrates how to create a trigger, leverage the built-in UI prefabs to manage it, and control it through a UI

	public class TriggerControlSimple : MVRScript, TriggerHandler {

		// Validate is called when storables are removed. This cleans up destroyed trigger references
		public override void Validate() {
			base.Validate();
			if (trigger != null) {
				trigger.Validate();
			}
		}

		// this function allows trigger atom references to be updated when an atom is renamed
		protected void OnAtomRename(string oldid, string newid) {
			if (trigger != null) {
				trigger.SyncAtomNames();
			}
		}

		protected Trigger trigger;
		protected TriggerActionTransition triggerActionTransition;
		protected RectTransform triggerActionTransitionPrefab;

		public void RemoveTrigger(Trigger trigger) {
			// do nothing as we only manage single trigger here
		}

		public void DuplicateTrigger(Trigger trigger) {
			// do nothing as we only manage single trigger here
		}

		public RectTransform CreateTriggerActionsUI() {
			// do nothing as we only manager single transition action here
			return null;
		}

		public RectTransform CreateTriggerActionMiniUI() {
			// do nothing as we only manager single transition action here
			return null;
		}

		public RectTransform CreateTriggerActionDiscreteUI() {
			// do nothing as we only manager single transition action here
			return null;
		}

		public RectTransform CreateTriggerActionTransitionUI() {
			RectTransform rt = null;
			if (triggerActionTransitionPrefab != null) {
				rt = (RectTransform)Instantiate(triggerActionTransitionPrefab);
			} else {
				Debug.LogError("Attempted to make TriggerActionTransitionUI when prefab was not set");
			}
			return (rt);
		}

		public void RemoveTriggerActionUI(RectTransform rt) {
			// do nothing as we only manager single transition action here
		}

		protected void SyncVariableControl(float f) {
			if (trigger != null) {
				trigger.transitionInterpValue = f;
			}
		}
		protected JSONStorableFloat variableControlJSON;

		protected void OpenTriggerDetailPanel() {
			triggerActionTransition.OpenDetailPanel();
		}
		protected JSONStorableAction OpenTriggerDetailPanelAction;

		protected IEnumerator LoadUIAssets() {
			AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionTransitionPanel", typeof(GameObject));
			if (request == null) {
				SuperController.LogError("Request for TriggerActionTransitionPanel in z_ui2 assetbundle failed");
				yield break;
			}
			yield return request;
			GameObject go = request.GetAsset<GameObject>();
			if (go == null) {
				SuperController.LogError("Failed to load TriggerActionTransitionPanel asset");
			}
			triggerActionTransitionPrefab = go.GetComponent<RectTransform>();
			if (triggerActionTransitionPrefab == null) {
				SuperController.LogError("Failed to load TriggerActionTransitionPanel asset");
			}

		}

		public override void Init() {
			try {

				trigger = new Trigger();
				trigger.handler = this;
				triggerActionTransition = trigger.CreateTransitionActionInternal();
				trigger.active = true;

				StartCoroutine(LoadUIAssets());

				// handle atom renaming - must be passed to trigger
				if (SuperController.singleton != null) {
					SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;
				}

				OpenTriggerDetailPanelAction = new JSONStorableAction("OpenTriggerDetailPanel", OpenTriggerDetailPanel);
				UIDynamicButton db = CreateButton("Trigger Detail...");
				OpenTriggerDetailPanelAction.dynamicButton = db;

				variableControlJSON = new JSONStorableFloat("variableControl", 0f, SyncVariableControl, 0f, 1f, true, true);
				CreateSlider(variableControlJSON);

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		public override void InitUI() {
			base.InitUI();
			if (UITransform != null) {
				trigger.triggerActionsParent = UITransform;
			}
		}

		protected float timer = 0f;

		protected void OnDestroy() {
			if (SuperController.singleton != null) {
				SuperController.singleton.onAtomUIDRenameHandlers -= OnAtomRename;
			}
		}

	}
}