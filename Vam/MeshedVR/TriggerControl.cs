using System;
using UnityEngine;
using System.Collections;
using AssetBundles;

namespace MVRPlugin {

	// This class demonstrates how to create a trigger, leverage the built-in UI prefabs to manage it, and control it through a UI

	public class TriggerControl : MVRScript, TriggerHandler {

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
		protected RectTransform triggerActionsPrefab;
		protected RectTransform triggerActionMiniPrefab;
		protected RectTransform triggerActionDiscretePrefab;
		protected RectTransform triggerActionTransitionPrefab;

		public void RemoveTrigger(Trigger trigger) {
			// do nothing as we only manage single trigger here
		}

		public void DuplicateTrigger(Trigger trigger) {
			// do nothing as we only manage single trigger here
		}

		public RectTransform CreateTriggerActionsUI() {
			RectTransform rt = null;
			if (triggerActionsPrefab != null) {
				rt = (RectTransform)Instantiate(triggerActionsPrefab);
			} else {
				Debug.LogError("Attempted to make TriggerActionsUI when prefab was not set");
			}
			return (rt);
		}

		public RectTransform CreateTriggerActionMiniUI() {
			RectTransform rt = null;
			if (triggerActionMiniPrefab != null) {
				rt = (RectTransform)Instantiate(triggerActionMiniPrefab);
			} else {
				Debug.LogError("Attempted to make TriggerActionMiniUI when prefab was not set");
			}
			return (rt);
		}

		public RectTransform CreateTriggerActionDiscreteUI() {
			RectTransform rt = null;
			if (triggerActionDiscretePrefab != null) {
				rt = (RectTransform)Instantiate(triggerActionDiscretePrefab);
			} else {
				Debug.LogError("Attempted to make TriggerActionDiscreteUI when prefab was not set");
			}
			return (rt);
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
			if (rt != null) {
				Destroy(rt.gameObject);
			}
		}

		protected void SyncTriggerActive(bool b) {
			if (trigger != null) {
				trigger.active = b;
			}
		}
		protected JSONStorableBool triggerActiveJSON;
		protected void SyncVariableControl(float f) {
			if (trigger != null) {
				trigger.transitionInterpValue = f;
			}
		}
		protected JSONStorableFloat variableControlJSON;


		protected void OpenTriggerActionsPanel() {
			trigger.OpenTriggerActionsPanel();
		}
		protected JSONStorableAction OpenTriggerActionsPanelAction;

		protected IEnumerator LoadUIAssets() {
			AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionsPanel", typeof(GameObject));
			if (request == null) {
				SuperController.LogError("Request for TriggerActionsPanel in z_ui2 assetbundle failed");
				yield break;
			}
			yield return request;
			GameObject go = request.GetAsset<GameObject>();
			if (go == null) {
				SuperController.LogError("Failed to load TriggerActionsPanel asset");
			}
			triggerActionsPrefab = go.GetComponent<RectTransform>();
			if (triggerActionsPrefab == null) {
				SuperController.LogError("Failed to load TriggerActionsPanel asset");
			}

			request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionMiniPanel", typeof(GameObject));
			if (request == null) {
				SuperController.LogError("Request for TriggerActionMiniPanel in z_ui2 assetbundle failed");
				yield break;
			}
			yield return request;
			go = request.GetAsset<GameObject>();
			if (go == null) {
				SuperController.LogError("Failed to load TriggerActionMiniPanel asset");
			}
			triggerActionMiniPrefab = go.GetComponent<RectTransform>();
			if (triggerActionMiniPrefab == null) {
				SuperController.LogError("Failed to load TriggerActionMiniPanel asset");
			}

			request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionDiscretePanel", typeof(GameObject));
			if (request == null) {
				SuperController.LogError("Request for TriggerActionDiscretePanel in z_ui2 assetbundle failed");
				yield break;
			}
			yield return request;
			go = request.GetAsset<GameObject>();
			if (go == null) {
				SuperController.LogError("Failed to load TriggerActionDiscretePanel asset");
			}
			triggerActionDiscretePrefab = go.GetComponent<RectTransform>();
			if (triggerActionDiscretePrefab == null) {
				SuperController.LogError("Failed to load TriggerActionDiscretePanel asset");
			}

			request = AssetBundleManager.LoadAssetAsync("z_ui2", "TriggerActionTransitionPanel", typeof(GameObject));
			if (request == null) {
				SuperController.LogError("Request for TriggerActionTransitionPanel in z_ui2 assetbundle failed");
				yield break;
			}
			yield return request;
			go = request.GetAsset<GameObject>();
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

				StartCoroutine(LoadUIAssets());

				// handle atom renaming - must be passed to trigger
				if (SuperController.singleton != null) {
					SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;
				}

				OpenTriggerActionsPanelAction = new JSONStorableAction("OpenTriggerActionsPanel", OpenTriggerActionsPanel);
				UIDynamicButton db = CreateButton("Trigger Actions...");
				OpenTriggerActionsPanelAction.dynamicButton = db;

				triggerActiveJSON = new JSONStorableBool("triggerActive", false, SyncTriggerActive);
				CreateToggle(triggerActiveJSON);

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