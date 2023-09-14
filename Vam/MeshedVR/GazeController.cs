using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    public class GazeController
    {
        public GazeController(Script script, string personID)
        {
            this.script = script;
            person = Utils.GetAtom(personID);
            if (person == null)
            {
                script.LogErrorFormat("[GazeController] Person '{0}' not found.", personID);
                return;
            }

            personHeadControl = Utils.GetStorable<FreeControllerV3>(person, "headControl");
            if (personHeadControl  == null)
            {
                script.LogErrorFormat("[GazeController] HeadControl for person '{0}' not found.", personID);
                return;
            }

            head = personHeadControl.transform;
        }

        // Set a reference object to determine where "forward" is.
        public void SetReference(Transform transform)
        {
            reference = transform;
        }

        // Set a reference object to determine where "forward" is.
        public void SetReference(string atomID, string controlID)
        {
            reference = null;
            Atom atom = Utils.GetAtom(atomID);
            if (atom == null)
            {
                script.LogErrorFormat("[GazeController] Atom '{0}' not found.", atomID);
                return;
            }

            JSONStorable storable = Utils.GetStorable(atom, controlID);
            if (storable == null)
            {
                script.LogErrorFormat("[GazeController] Control '{0}/{1}' not found.", atomID, controlID);
                return;
            }

            reference = storable.transform;
        }

        // Set player as target to look at.
        public void SetLookAtPlayer()
        {
            lookAtTarget = CameraTarget.centerTarget.transform;
            lookAtOffset = Vector3.zero;
        }

        // Set player as target to look at.
        public void SetLookAtPlayer(Vector3 offset)
        {
            lookAtTarget = CameraTarget.centerTarget.transform;
            lookAtOffset = offset;
        }

        // Set transform as target to look at.
        public void SetLookAt(Transform transform)
        {
            lookAtTarget = transform;
            lookAtOffset = Vector3.zero;
        }

        // Set transform as target to look at.
        public void SetLookAt(Transform transform, Vector3 offset)
        {
            lookAtTarget = transform;
            lookAtOffset = offset;
        }

        // Set maximum offset angle (in degrees) from directly looking at the target during idle animations. Values closer to 0 mean the character will be more focused on the target.
        public void SetFocusAngles(float angleH, float angleV)
        {
            focusAngleH = angleH * Mathf.Deg2Rad;
            focusAngleV = angleV * Mathf.Deg2Rad;
        }

        // Set min/max duration between random focus point changes.
        public void SetFocusChangeDuration(float min, float max)
        {
            focusChangeDurationMin = Mathf.Clamp(min, 0.01f, max);
            focusChangeDurationMax = Mathf.Max(max, focusChangeDurationMin);
        }

        // Set how fast the gaze adapts to a new target position.
        public void SetGazeDuration(float duration)
        {
            gazeDuration = Mathf.Max(duration, 0.001f);
        }
    
        // Current angle (in degrees) between look direction and target direction.
        public float GetCurrentAngle()
        {
            return currentAngle;
        }

        // Call during OnFixedUpdate of your script.
        public void OnFixedUpdate()
        {
            if (lookAtTarget == null || head == null || reference == null)
                return;

            // compute horizontal and vertical angles
            Vector3 lookAtPosition = lookAtTarget.TransformPoint(lookAtOffset);
            Vector3 actualDir = reference.InverseTransformDirection(head.forward);
            Vector3 targetDir = lookAtPosition - head.position;
            targetDir.Normalize();
            targetDir = reference.InverseTransformDirection(targetDir);
            Vector2 actualDirH = new Vector2(actualDir.x, actualDir.z);
            Vector2 targetDirH = new Vector2(targetDir.x, targetDir.z);
            Vector2 actualDirV = new Vector2(actualDirH.magnitude, actualDir.y);
            Vector2 targetDirV = new Vector2(targetDirH.magnitude, targetDir.y);
            actualDirH.Normalize();
            targetDirH.Normalize();
            actualDirV.Normalize();
            targetDirV.Normalize();
            float actualH = Mathf.Atan2(actualDirH.x, actualDirH.y);
            float targetH = Mathf.Atan2(targetDirH.x, targetDirH.y);
            float actualV = Mathf.Atan2(actualDirV.y, actualDirV.x);
            float targetV = Mathf.Atan2(targetDirV.y, targetDirV.x);

            // apply focus
            focusChangeClock += focusChangeSpeed * Time.fixedDeltaTime;
            if (focusChangeClock >= 1.0f)
            {
                focusChangeSpeed = 1.0f / Random.Range(focusChangeDurationMin, focusChangeDurationMax);
                focusChangeClock = 0.0f;
                focusPrev = focusNext;
                focusNext = Random.insideUnitCircle;
            }
            float t = Mathf.SmoothStep(0.0f, 1.0f, focusChangeClock);
            targetH += Mathf.Lerp(focusPrev.x, focusNext.x, t) * focusAngleH;
            targetV += Mathf.Lerp(focusPrev.y, focusNext.y, t) * focusAngleV;

            // adjust angles
            targetH = Mathf.Clamp(targetH, -maxAngleH, maxAngleH);
            targetV = Mathf.Clamp(targetV, -maxAngleV, maxAngleV);
            actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, gazeDuration, Mathf.Infinity, Time.fixedDeltaTime);
            actualV = Mathf.SmoothDamp(actualV, targetV, ref velocityV, gazeDuration, Mathf.Infinity, Time.fixedDeltaTime);

            // recombine
            actualDir = RecombineDirection(actualH, actualV);
            targetDir = RecombineDirection(targetH, targetV);
            actualDir = reference.TransformDirection(actualDir);
            head.transform.LookAt(head.transform.position + actualDir);

            // apply roll
            rollChangeClock += rollChangeSpeed * Time.fixedDeltaTime;
            if (rollChangeClock >= 1.0f)
            {
                rollChangeSpeed = 1.0f / Random.Range(rollChangeDurationMin, rollChangeDurationMax);
                rollChangeClock = 0.0f;
                rollPrev = rollNext;
                rollNext = Random.Range(-rollAngleMax, rollAngleMax);
            }
            t = Mathf.SmoothStep(0.0f, 1.0f, rollChangeClock);
            float roll = Mathf.Lerp(rollPrev, rollNext, t);
            Vector3 eulerAngles = head.transform.localEulerAngles;
            eulerAngles.z = roll * Mathf.Rad2Deg;
            head.transform.localEulerAngles = eulerAngles;

            // compute angle
            currentAngle = Vector3.Angle(actualDir, targetDir);
        }

        private Vector3 RecombineDirection(float angleH, float angleV)
        {
            float cosV = Mathf.Cos(angleV);
            return new Vector3(
                Mathf.Sin(angleH) * cosV,
                Mathf.Sin(angleV),
                Mathf.Cos(angleH) * cosV
            );
        }

        private readonly Script script;
        private readonly Atom person;
        private readonly FreeControllerV3 personHeadControl;
        private Transform lookAtTarget;
        private Vector3 lookAtOffset;
        private readonly Transform head;
        private Transform reference;

        // tweak parameters
        private float focusAngleH = 6.0f * Mathf.Deg2Rad;
        private float focusAngleV = 4.0f * Mathf.Deg2Rad;
        private float focusChangeDurationMin = 1.0f;
        private float focusChangeDurationMax = 4.0f;
        private float gazeDuration = 0.7f;
        private float rollChangeDurationMin = 2.0f;
        private float rollChangeDurationMax = 6.0f;
        private float rollAngleMax = 6.0f * Mathf.Deg2Rad;

        // runtime data
        private float velocityH = 0.0f;
        private float velocityV = 0.0f;
        private float focusChangeClock = 1.0f;
        private float focusChangeSpeed = 1.0f;
        private Vector2 focusNext = Vector2.zero;
        private Vector2 focusPrev = Vector2.zero;
        private float rollNext = 0.0f;
        private float rollPrev = 0.0f;        
        private float rollChangeClock = 1.0f;
        private float rollChangeSpeed = 1.0f;
        private float currentAngle = 0.0f;

        private const float maxAngleH = 90.0f * Mathf.Deg2Rad;
        private const float maxAngleV = 45.0f * Mathf.Deg2Rad;
    }
}