using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace VaMJapan {
    public class DesktopCameraControl : MVRScript {
        private SuperController sc = SuperController.singleton;
        private float mult;
        private float xmult;
        private float ymult;
        private float zmult;
        private float Rxmult;
        private float Rymult;
        private float Pxmult;
        private float Pymult;

        private List<float> inputXList1 = new List<float> () { 0 };
        private List<float> inputYList1 = new List<float> () { 0 };
        private List<float> inputXList2 = new List<float> () { 0 };
        private List<float> inputYList2 = new List<float> () { 0 };
        private List<float> inputZList = new List<float> () { 0 };
        private List<float> AccelerationList = new List<float> () { 0 };

        int acceleration = 0;
        float accelerationAmount = 0;
        float currentSpeed = 1;

        float count;

        float axis;
        float axis2;
        float axis3;
        float axis4;
        float axis5;
        float released1;
        float released2;
        Vector2 lastAxis = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

        JSONStorableFloat speed;
        JSONStorableFloat rotationmult;
        JSONStorableFloat positionmult;
        JSONStorableFloat zoommult;
        JSONStorableFloat wasdzxmult;
        JSONStorableFloat highspeed;
        JSONStorableFloat smoothmult;
        JSONStorableFloat slowAcceleration;
        JSONStorableBool rdpmode;
        JSONStorableStringChooser PositionKey;
        JSONStorableStringChooser RotationKey;
        JSONStorableStringChooser ZoomInKey;
        JSONStorableStringChooser ZoomOutKey;
        JSONStorableStringChooser SpeedKey;
        JSONStorableStringChooser SustainKey;
        JSONStorableStringChooser HorizontalKey;
        JSONStorableStringChooser VerticalKey;

        UIDynamicPopup dp;

        List<string> KeyCodes = new List<string> () {
            "None",
            "space",
            "return",
            "up",
            "down",
            "left",
            "right",
            "left shift",
            "right shift",
            "left ctrl",
            "right ctrl",
            "left alt",
            "right alt",
            "tab",
            "backspace",
            "f2",
            "f3",
            "f4",
            "f5",
            "f6",
            "f7",
            "f8",
            "f9",
            "f10",
            "f11",
            "f12",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "[0]",
            "[1]",
            "[2]",
            "[3]",
            "[4]",
            "[5]",
            "[6]",
            "[7]",
            "[8]",
            "[9]",
            "b",
            "c",
            "e",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "o",
            "q",
            "v",
            "y",
            "insert",
            "delete",
            "home",
            "end",
            "page up",
            "page down",
            "numlock",
            "caps lock",
            "scroll lock",
            "pause",
        };

        public override void Init () {
            speed = new JSONStorableFloat ("Master Speed Multiplier", 1f, 0f, 5f, false);
            RegisterFloat (speed);
            CreateSlider (speed, false);
            speed.setCallbackFunction += (float val) => {
                SetSpeed ();
            };
            positionmult = new JSONStorableFloat ("Positon Speed", 1f, 0f, 5f, false);
            RegisterFloat (positionmult);
            CreateSlider (positionmult, false);
            positionmult.setCallbackFunction += (float val) => {
                SetSpeed ();
            };

            rotationmult = new JSONStorableFloat ("Rotation Speed", 1f, 0f, 5f, false);
            RegisterFloat (rotationmult);
            CreateSlider (rotationmult, false);
            rotationmult.setCallbackFunction += (float val) => {
                SetSpeed ();
            };
            zoommult = new JSONStorableFloat ("Zoom Speed", 1f, 0f, 5f, false);
            RegisterFloat (zoommult);
            CreateSlider (zoommult, false);
            zoommult.setCallbackFunction += (float val) => {
                SetSpeed ();
            };

            wasdzxmult = new JSONStorableFloat ("WASDZX Speed", .5f, 0f, 5f, false);
            RegisterFloat (wasdzxmult);
            CreateSlider (wasdzxmult, false);
            wasdzxmult.setCallbackFunction += (float val) => {
                SetSpeed ();
            };

            highspeed = new JSONStorableFloat ("Shifted Speed Multiplier", 2.5f, 0f, 10f, false);
            RegisterFloat (highspeed);
            CreateSlider (highspeed, false);

            slowAcceleration = new JSONStorableFloat ("Acceleration Time", 1f, 0f, 5f, false);
            RegisterFloat (slowAcceleration);
            CreateSlider (slowAcceleration);
            SetSlowAcceleration ();

            smoothmult = new JSONStorableFloat ("Smoothing Amount", 20f, 0f, 50f, true);
            RegisterFloat (smoothmult);
            CreateSlider (smoothmult);

            smoothmult.setCallbackFunction += (float val) => {

                inputXList1 = new List<float> () { 0 };
                inputYList1 = new List<float> () { 0 };
                inputXList2 = new List<float> () { 0 };
                inputYList2 = new List<float> () { 0 };
                inputZList = new List<float> () { 0 };

                for (float i = 1f; i <= val; i++) {
                    inputXList1.Add (0);
                    inputYList1.Add (0);
                    inputXList2.Add (0);
                    inputYList2.Add (0);
                    inputZList.Add (0);
                }

            };

            rdpmode = new JSONStorableBool ("Remote Desktop Mode", false);
            rdpmode.storeType = JSONStorableParam.StoreType.Full;
            CreateToggle (rdpmode, false);

            PositionKey = new JSONStorableStringChooser ("Position Key", KeyCodes, "q", "Position Key");
            RegisterStringChooser (PositionKey);
            dp = CreateScrollablePopup (PositionKey, true);
            dp.popupPanelHeight = 1100f;

            RotationKey = new JSONStorableStringChooser ("Rotation Key", KeyCodes, "e", "Rotation Key");
            RegisterStringChooser (RotationKey);
            dp = CreateScrollablePopup (RotationKey, true);
            dp.popupPanelHeight = 1100f;
            ZoomInKey = new JSONStorableStringChooser ("ZoomIn Key", KeyCodes, "1", "ZoomIn Key");
            RegisterStringChooser (ZoomInKey);
            dp = CreateScrollablePopup (ZoomInKey, true);
            dp.popupPanelHeight = 1100f;

            ZoomOutKey = new JSONStorableStringChooser ("ZoomOut Key", KeyCodes, "2", "ZoomOut Key");
            RegisterStringChooser (ZoomOutKey);
            dp = CreateScrollablePopup (ZoomOutKey, true);
            dp.popupPanelHeight = 1100f;

            SpeedKey = new JSONStorableStringChooser ("Speed Shift Key", KeyCodes, "left shift", "Speed Shift Key");
            RegisterStringChooser (SpeedKey);
            dp = CreateScrollablePopup (SpeedKey, true);
            dp.popupPanelHeight = 1100f;

            SustainKey = new JSONStorableStringChooser ("Sustain Key", KeyCodes, "left alt", "Sustain Key");
            RegisterStringChooser (SustainKey);
            dp = CreateScrollablePopup (SustainKey, true);
            dp.popupPanelHeight = 1100f;

            HorizontalKey = new JSONStorableStringChooser ("Horizontal Key", KeyCodes, "left ctrl", "Horizontal Key");
            RegisterStringChooser (HorizontalKey);
            dp = CreateScrollablePopup (HorizontalKey, true);
            dp.popupPanelHeight = 1100f;

            VerticalKey = new JSONStorableStringChooser ("Vertical Key", KeyCodes, "space", "Vertical Key");
            RegisterStringChooser (VerticalKey);
            dp = CreateScrollablePopup (VerticalKey, true);
            dp.popupPanelHeight = 1100f;

            LoadShortCutKey ();

            UIDynamicButton savePresetButton = CreateButton ("Save Key as Default", true);
            savePresetButton.button.onClick.AddListener (SaveShortCutKey);

            for (float i = 1f; i <= smoothmult.val; i++) {
                inputXList1.Add (0);
                inputYList1.Add (0);
                inputXList2.Add (0);
                inputYList2.Add (0);
                inputZList.Add (0);
            }

            SetSpeed ();
        }

        public void FixedUpdate () {
            count += Time.deltaTime;

            if (count >= (1f / 20f) * slowAcceleration.val) {

                if (Input.GetKey (SpeedKey.val)) {

                    // Rxmult *= (1 + (highspeed.val - 1) * 0.75f);
                    // Rymult *= (1 + (highspeed.val - 1) * 0.75f);

                    if (acceleration < 20) {
                        acceleration++;
                        accelerationAmount = AccelerationList[acceleration];
                        currentSpeed = (1 + (highspeed.val - 1) * accelerationAmount);
                        AccelerationCameraSpeed ();
                    }

                } else {
                    if (acceleration > 0) {
                        acceleration--;
                        accelerationAmount = AccelerationList[acceleration];
                        currentSpeed = (1 + (highspeed.val - 1) * accelerationAmount);
                        AccelerationCameraSpeed ();
                    }
                }

                count = 0.0f;

            }
        }

        public void Update () {

            if (axis != 0 || axis2 != 0 || Input.GetKey (RotationKey.val)) {

                Vector3 vector = sc.MonitorCenterCamera.transform.position + sc.MonitorCenterCamera.transform.forward * sc.focusDistance;

                if (Input.GetKey (RotationKey.val)) {
                    released1 = 1f;

                    float mouthInputX1;
                    float mouthInputY1;

                    if (rdpmode.val) {
                        mouthInputX1 = -(lastAxis.x - Input.mousePosition.x) * 0.2f;
                        mouthInputY1 = -(lastAxis.y - Input.mousePosition.y) * 0.2f;
                    } else {
                        mouthInputX1 = Input.GetAxis ("Mouse X");
                        mouthInputY1 = Input.GetAxis ("Mouse Y");
                    }

                    inputXList1.Add (mouthInputX1);
                    inputYList1.Add (mouthInputY1);

                } else {

                    if (Input.GetKey (SustainKey.val)) {
                        inputXList1.Add (axis);
                        inputYList1.Add (axis2);
                    } else {

                        released1 *= .95f;

                        inputXList1.Add (axis * released1);
                        inputYList1.Add (axis2 * released1);

                    }

                }

                inputXList1.RemoveAt (0);
                inputYList1.RemoveAt (0);
                axis = inputXList1.Average ();
                axis2 = inputYList1.Average ();

                if (!Input.GetKey (VerticalKey.val)) {
                    if (axis > 0.001f || axis < -0.001f) {
                        sc.navigationRig.RotateAround (vector, sc.navigationRig.up, axis * Rxmult);
                    }
                }
                if (!Input.GetKey (HorizontalKey.val)) {

                    if ((axis2 > 0.001f || axis2 < -0.001f) && sc.MonitorCenterCamera != null) {
                        Vector3 position = sc.MonitorCenterCamera.transform.position;
                        Vector3 up = sc.navigationRig.up;
                        Vector3 a = position - up * axis2 * Rymult * sc.focusDistance;
                        Vector3 a2 = a - vector;
                        a2.Normalize ();
                        a = vector + a2 * sc.focusDistance;
                        Vector3 vector2 = a - position;
                        Vector3 vector3 = sc.navigationRig.position + vector2;
                        float num = Vector3.Dot (vector2, up);
                        vector3 += up * -num;
                        sc.navigationRig.position = vector3;
                        sc.playerHeightAdjust += num;
                        if (sc.MonitorCenterCamera != null) {
                            sc.MonitorCenterCamera.transform.LookAt (vector);
                            Vector3 localEulerAngles = sc.MonitorCenterCamera.transform.localEulerAngles;
                            localEulerAngles.y = 0f;
                            localEulerAngles.z = 0f;
                            sc.MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
                        }
                    }
                }

            }

            if (axis3 != 0 || axis4 != 0 || Input.GetKey (PositionKey.val)) {

                if (Input.GetKey (PositionKey.val)) {

                    released2 = 1f;

                    float mouthInputX2;
                    float mouthInputY2;

                    if (rdpmode.val) {
                        mouthInputX2 = -(lastAxis.x - Input.mousePosition.x) * 0.2f;
                        mouthInputY2 = -(lastAxis.y - Input.mousePosition.y) * 0.2f;
                    } else {
                        mouthInputX2 = Input.GetAxis ("Mouse X");
                        mouthInputY2 = Input.GetAxis ("Mouse Y");
                    }

                    inputXList2.Add (mouthInputX2);
                    inputYList2.Add (mouthInputY2);

                } else {

                    if (Input.GetKey (SustainKey.val)) {
                        inputXList2.Add (axis3);
                        inputYList2.Add (axis4);
                    } else {
                        released2 *= .95f;
                        inputXList2.Add (axis3 * released2);
                        inputYList2.Add (axis4 * released2);

                    }
                }
                inputXList2.RemoveAt (0);
                inputYList2.RemoveAt (0);
                axis3 = inputXList2.Average ();
                axis4 = inputYList2.Average ();

                Vector3 vector4 = sc.navigationRig.position;
                if (axis3 > 0.01f || axis3 < -0.01f) {
                    vector4 += sc.MonitorCenterCamera.transform.right * -axis3 * .003f * Pxmult;
                }
                if (axis4 > 0.01f || axis4 < -0.01f) {
                    vector4 += sc.MonitorCenterCamera.transform.up * -axis4 * .003f * Pymult;
                }
                Vector3 up2 = sc.navigationRig.up;
                Vector3 lhs = vector4 - sc.navigationRig.position;
                float num2 = Vector3.Dot (lhs, up2);
                vector4 += up2 * -num2;
                sc.navigationRig.position = vector4;
                sc.playerHeightAdjust += num2;

            }

            if (Input.GetKey (ZoomInKey.val) || Input.GetKey (ZoomOutKey.val)) {

                if (inputZList.Count () <= smoothmult.val) {

                    if (Input.GetKey (ZoomInKey.val)) {

                        inputZList.Add (zmult);
                    } else {
                        inputZList.Add (-zmult);
                    }

                }

                inputZList.RemoveAt (0);
                axis5 = inputZList.Average ();

            }

            if (axis5 != 0f) {

                Vector3 forward = sc.MonitorCenterCamera.transform.forward;
                Vector3 vector5 = axis5 * forward * sc.focusDistance;
                Vector3 vector6 = sc.navigationRig.position + vector5;
                sc.focusDistance *= 1f - axis5;
                Vector3 up3 = sc.navigationRig.up;
                float num4 = Vector3.Dot (vector5, up3);
                vector6 += up3 * -num4;
                sc.navigationRig.position = vector6;
                sc.playerHeightAdjust += num4;

                if (!Input.GetKey (ZoomInKey.val) && !Input.GetKey (ZoomOutKey.val)) {
                    inputZList.Add (0f);
                    inputZList.RemoveAt (0);
                    axis5 = inputZList.Average ();
                }
            }
            if (rdpmode.val) {
                lastAxis = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
            }

        }

        private void SaveShortCutKey () {
            SimpleJSON.JSONClass Settings = new SimpleJSON.JSONClass ();
            Settings.Add ("Position Key", new SimpleJSON.JSONData (PositionKey.val));
            Settings.Add ("Rotation Key", new SimpleJSON.JSONData (RotationKey.val));
            Settings.Add ("ZoomIn Key", new SimpleJSON.JSONData (ZoomInKey.val));
            Settings.Add ("ZoomOut Key", new SimpleJSON.JSONData (ZoomOutKey.val));
            Settings.Add ("Speed Shift Key", new SimpleJSON.JSONData (SpeedKey.val));
            Settings.Add ("Sustain Key", new SimpleJSON.JSONData (SustainKey.val));
            Settings.Add ("Horizontal Key", new SimpleJSON.JSONData (HorizontalKey.val));
            Settings.Add ("Vertical Key", new SimpleJSON.JSONData (VerticalKey.val));
            sc.SaveJSON (Settings, "Custom/Scripts/VaMJapan/DCCSettings.json");
            SuperController.LogMessage ("Camera Control Keys was Saved");
        }

        private void LoadShortCutKey () {
            SimpleJSON.JSONClass LoadSettings = new SimpleJSON.JSONClass ();

            try {
                LoadSettings = (SimpleJSON.JSONClass) sc.LoadJSON ("Custom/Scripts/VaMJapan/DCCSettings.json");
            } catch (Exception) {
                return;
            }

            if (LoadSettings != null) {
                PositionKey.val = LoadSettings["Position Key"];
                RotationKey.val = LoadSettings["Rotation Key"];
                ZoomInKey.val = LoadSettings["ZoomIn Key"];
                ZoomOutKey.val = LoadSettings["ZoomOut Key"];
                SpeedKey.val = LoadSettings["Speed Shift Key"];
                SustainKey.val = LoadSettings["Sustain Key"];
                HorizontalKey.val = LoadSettings["Horizontal Key"];
                VerticalKey.val = LoadSettings["Vertical Key"];
            }
        }

        private void SetSpeed () {
            mult = speed.val;
            xmult = mult / 2;
            ymult = mult / 80;
            sc.freeMoveMultiplier = mult * wasdzxmult.val;

            Pxmult = mult * positionmult.val;
            Pymult = mult * positionmult.val;
            Rxmult = xmult * rotationmult.val;
            Rymult = ymult * rotationmult.val;

            zmult = mult / 200 * zoommult.val;

        }

        private void SetSlowAcceleration () {
            AccelerationList = new List<float> () { 0 };
            for (float i = 1f; i <= 20; i++) {

                // AccelerationList.Add (1 / (20 - i + 1));
                AccelerationList.Add ((float) (Math.Pow (i / 20, 2f)));

            }
        }

        private void AccelerationCameraSpeed () {
            sc.freeMoveMultiplier = mult * wasdzxmult.val * currentSpeed;
            Pxmult = mult * positionmult.val * currentSpeed;
            Pymult = mult * positionmult.val * currentSpeed;
            Rxmult = xmult * rotationmult.val * currentSpeed;
            Rymult = ymult * rotationmult.val * currentSpeed;
            zmult = mult / 200 * zoommult.val * currentSpeed;

            // SuperController.LogMessage ((currentSpeed).ToString ());

        }

    }
}