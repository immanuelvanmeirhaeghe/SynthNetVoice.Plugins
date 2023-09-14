using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.Events;

/* 95% written by MeshedVR
Doc'0c adapted it to a plugin
*/
namespace ReVAMped
{
    public class WindControl : MVRScript
    {
        Atom windDirection;
        public static Vector3 globalWind = Vector3.zero;
        protected string _missingReceiverStoreId = string.Empty;
        public bool isGlobal;
        protected JSONStorableBool isGlobalJSON;
        protected Atom receivingAtom;
        protected JSONStorableStringChooser atomJSON;
        protected JSONStorable receiver;
        protected JSONStorableStringChooser receiverJSON;
        protected string _receiverTargetName;
        protected JSONStorableVector3 receiverTarget;
        protected JSONStorableStringChooser receiverTargetJSON;
        protected JSONStorableFloat currentMagnitudeJSON;
        protected JSONStorableBool autoJSON;
        protected JSONStorableFloat periodJSON;
        protected JSONStorableFloat quicknessJSON;
        protected JSONStorableFloat targetMagnitudeJSON;
        protected JSONStorableFloat lowerMagnitudeJSON;
        protected JSONStorableFloat upperMagnitudeJSON;
        protected float timer;

        protected void SyncIsGlobal(bool b)
        {
            isGlobal = b;
        }

        protected void SyncAtomChoices()
        {
            List<string> stringList = new List<string>();
            stringList.Add("None");
            foreach (string atomUiD in SuperController.singleton.GetAtomUIDs())
                stringList.Add(atomUiD);
            atomJSON.choices = stringList;
        }

        protected void OnAtomRename(string oldid, string newid)
        {
            SyncAtomChoices();
            if (atomJSON == null || !(receivingAtom != null))
                return;
            atomJSON.valNoCallback = receivingAtom.uid;
        }

        protected void SyncAtom(string atomUID)
        {
            List<string> stringList = new List<string>();
            stringList.Add("None");
            if (atomUID != null)
            {
                receivingAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (receivingAtom != null)
                {
                    foreach (string storableId in receivingAtom.GetStorableIDs())
                    {
                        stringList.Add(storableId);
                    }
                }
            }
            else
                receivingAtom = (Atom)null;

            receiverJSON.choices = stringList;
            receiverJSON.val = "None";
        }

        protected void CheckMissingReceiver()
        {
            if (!(_missingReceiverStoreId != string.Empty)
                || !(receivingAtom != null)
                || !(receivingAtom.GetStorableByID(_missingReceiverStoreId) != null))
                return;

            string receiverTargetName = _receiverTargetName;
            SyncReceiver(_missingReceiverStoreId);
            _missingReceiverStoreId = string.Empty;
            insideRestore = true;
            receiverTargetJSON.val = receiverTargetName;
            insideRestore = false;
        }

        protected void SyncReceiver(string receiverID)
        {
            List<string> stringList = new List<string>();
            stringList.Add("None");
            if (receivingAtom != null && receiverID != null)
            {
                receiver = receivingAtom.GetStorableByID(receiverID);
                if (receiver != null)
                {
                    foreach (string vector3ParamName in receiver.GetVector3ParamNames())
                    {
                        stringList.Add(vector3ParamName);
                    }
                }
                else if (receiverID != "None")
                    _missingReceiverStoreId = receiverID;
            }
            else
            {
                receiver = null;
            }

            receiverTargetJSON.choices = stringList;
            receiverTargetJSON.val = "None";
        }

        protected void SyncReceiverTarget(string receiverTargetName)
        {
            _receiverTargetName = receiverTargetName;
            receiverTarget = (JSONStorableVector3)null;
            if (receiver != null && receiverTargetName != null)
                receiverTarget = receiver.GetVector3JSONParam(receiverTargetName);
        }

        public override void Init()
        {
            try
            {

                if ((bool)(SuperController.singleton))
                    SuperController.singleton.onAtomUIDRenameHandlers += new SuperController.OnAtomUIDRename(OnAtomRename);

                isGlobalJSON = new JSONStorableBool("global", false, new JSONStorableBool.SetBoolCallback(SyncIsGlobal));

                atomJSON = new JSONStorableStringChooser(
                  "atom", SuperController.singleton.GetAtomUIDs(), (string)null,
                  "Atom", new JSONStorableStringChooser.SetStringCallback(SyncAtom));

                atomJSON.popupOpenCallback = new JSONStorableStringChooser.PopupOpenCallback(SyncAtomChoices);
                receiverJSON = new JSONStorableStringChooser(
                  "receiver", (List<string>)null, (string)null,
                  "Receiver", new JSONStorableStringChooser.SetStringCallback(SyncReceiver));


                receiverTargetJSON = new JSONStorableStringChooser(
                  "receiverTarget", (List<string>)null, (string)null,
                  "Target", new JSONStorableStringChooser.SetStringCallback(SyncReceiverTarget));

                currentMagnitudeJSON = new JSONStorableFloat("currentMagnitude", 0.0f, -50f, 50f, false, true);
                autoJSON = new JSONStorableBool("auto", false);
                periodJSON = new JSONStorableFloat("period", 0.5f, 0.0f, 10f, false, true);
                quicknessJSON = new JSONStorableFloat("quickness", 10f, 0.0f, 100f, true, true);
                lowerMagnitudeJSON = new JSONStorableFloat("lowerMagnitude", 0.0f, -50f, 50f, false, true);
                upperMagnitudeJSON = new JSONStorableFloat("upperMagnitude", 0.0f, -50f, 50f, false, true);
                targetMagnitudeJSON = new JSONStorableFloat("targetMagnitude", 0.0f, -50f, 50f, false, false);

                RegisterBool(isGlobalJSON);
                RegisterStringChooser(atomJSON);
                RegisterStringChooser(receiverJSON);
                RegisterStringChooser(receiverTargetJSON);

                RegisterBool(autoJSON);
                RegisterFloat(periodJSON);
                RegisterFloat(quicknessJSON);
                RegisterFloat(upperMagnitudeJSON);
                RegisterFloat(lowerMagnitudeJSON);
                RegisterFloat(currentMagnitudeJSON);

                CreateToggle(isGlobalJSON);
                CreateScrollablePopup(atomJSON, true);
                CreateScrollablePopup(receiverJSON, true).popupPanelHeight = 1100f;
                CreateScrollablePopup(receiverTargetJSON, true);

                CreateSlider(currentMagnitudeJSON);
                CreateToggle(autoJSON);
                CreateSlider(periodJSON);
                CreateSlider(quicknessJSON);
                CreateSlider(lowerMagnitudeJSON);
                CreateSlider(upperMagnitudeJSON);
                CreateSlider(targetMagnitudeJSON);
            }
            catch (System.Exception e)
            {
                SuperController.LogError("" + e);
            }
        }


        protected void Update()
        {
            if (currentMagnitudeJSON == null)
                return;
            if (autoJSON != null && autoJSON.val)
            {
                timer -= Time.deltaTime;
                if ((double)timer < 0.0)
                {
                    timer = periodJSON.val;
                    targetMagnitudeJSON.val = Random.Range(lowerMagnitudeJSON.val, upperMagnitudeJSON.val);
                }
                currentMagnitudeJSON.val = Mathf.Lerp(currentMagnitudeJSON.val, targetMagnitudeJSON.val, Time.deltaTime * quicknessJSON.val);
            }
            Vector3 vector3 = currentMagnitudeJSON.val * transform.forward;
            if (isGlobal)
            {
                WindControl.globalWind = vector3;
            }
            else
            {
                CheckMissingReceiver();
                if (receiverTarget == null)
                    return;
                receiverTarget.val = currentMagnitudeJSON.val * transform.forward;
            }
        }


        protected void OnDestroy()
        {
            if (SuperController.singleton)
                SuperController.singleton.onAtomUIDRenameHandlers -= new SuperController.OnAtomUIDRename(OnAtomRename);
        }
    }

}
