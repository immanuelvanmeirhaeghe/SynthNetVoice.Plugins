using UnityEngine;
using VAM_ScriptEngine;
using System.Collections.Generic;

namespace MacGruber
{
    public class BlendQueue
    {
        public bool IsEmpty()
        {
            return activeEntry == null && queue.Count == 0;
        }

        public void OnUpdate()
        {
            if (activeEntry == null)
            {
                if (queue.Count == 0)
                    return;
                activeEntry = queue.Dequeue();
                activeEntry.Start();
            }

            if (activeEntry.Update())
                activeEntry = null;
        }

        #region Queue Functions
        public void QueueEnd(BlendSet set, float duration)
        {
            queue.Enqueue(new BlendEntry(set, duration));
        }

        public void QueueEnd(OutTriggerFloat trigger, float endValue, float duration)
        {
            QueueEnd(new BlendSet(trigger, endValue), duration);
        }

        public void QueueEnd(OutTriggerColor trigger, HSVColor endValue, float duration)
        {
            QueueEnd(new BlendSet(trigger, endValue), duration);
        }

        public void QueueEnd(OutTriggerColor trigger, Color endValue, float duration)
        {
            QueueEnd(new BlendSet(trigger, endValue), duration);
        }

        public void QueueNext(BlendSet set, float duration)
        {
            queue.Clear();
            queue.Enqueue(new BlendEntry(set, duration));
        }

        public void QueueNext(OutTriggerFloat trigger, float endValue, float duration)
        {
            QueueNext(new BlendSet(trigger, endValue), duration);
        }

        public void QueueNext(OutTriggerColor trigger, HSVColor endValue, float duration)
        {
            QueueNext(new BlendSet(trigger, endValue), duration);
        }

        public void QueueNext(OutTriggerColor trigger, Color endValue, float duration)
        {
            QueueNext(new BlendSet(trigger, endValue), duration);
        }

        public void QueueInstant(BlendSet set, float duration)
        {
            QueueNext(set, duration);
            activeEntry = null;
        }

        public void QueueInstant(OutTriggerFloat trigger, float endValue, float duration)
        {
            QueueInstant(new BlendSet(trigger, endValue), duration);
        }

        public void QueueInstant(OutTriggerColor trigger, HSVColor endValue, float duration)
        {
            QueueInstant(new BlendSet(trigger, endValue), duration);
        }

        public void QueueInstant(OutTriggerColor trigger, Color endValue, float duration)
        {
            QueueInstant(new BlendSet(trigger, endValue), duration);
        }
        #endregion

        private Queue<BlendEntry> queue = new Queue<BlendEntry>();
        private BlendEntry activeEntry = null;
    }

    public class BlendSet
    {
        public BlendSet()
        { }

        public BlendSet(OutTriggerFloat trigger, float endValue)
        {
            AddValue(trigger, endValue);
        }

        public BlendSet(OutTriggerColor trigger, HSVColor endValue)
        {
            AddValue(trigger, endValue);
        }

        public BlendSet(OutTriggerColor trigger, Color endValue)
        {
            AddValue(trigger, endValue);
        }

        public void AddValue(OutTriggerFloat trigger, float endValue)
        {
            values.Add(new BlendValueFloat(trigger, endValue));
        }

        public void AddValue(OutTriggerFloat trigger, float startValue, float endValue)
        {
            values.Add(new BlendValueFloat(trigger, startValue, endValue));
        }

        public void AddValue(OutTriggerColor trigger, HSVColor endValue)
        {
            values.Add(new BlendValueColor(trigger, endValue));
        }

        public void AddValue(OutTriggerColor trigger, HSVColor startValue, HSVColor endValue)
        {
            values.Add(new BlendValueColor(trigger, startValue, endValue));
        }

        public void AddValue(OutTriggerColor trigger, Color endValue)
        {
            HSVColor end = HSVColorPicker.RGBToHSV(endValue.r, endValue.g, endValue.b);
            values.Add(new BlendValueColor(trigger, end));
        }

        public void AddValue(OutTriggerColor trigger, Color startValue, Color endValue)
        {
            HSVColor start = HSVColorPicker.RGBToHSV(startValue.r, startValue.g, startValue.b);
            HSVColor end = HSVColorPicker.RGBToHSV(endValue.r, endValue.g, endValue.b);
            values.Add(new BlendValueColor(trigger, start, end));
        }

        public void Start()
        {
            for (int i = 0; i < values.Count; ++i)
                values[i].Start();
        }

        public void Update(float t)
        {
            for (int i = 0; i < values.Count; ++i)
                values[i].Update(t);
        }

        private List<BlendValue> values = new List<BlendValue>();
    }

    internal class BlendEntry
    {
        public BlendEntry(BlendSet set, float duration)
        {
            this.set = set;
            this.duration = duration;
        }

        public void Start()
        {
            timestamp = Utils.GetTimestamp();
            set.Start();
        }

        // return true when entry is done
        public bool Update()
        {
            float clock = Utils.TimeSince(timestamp);
            float t = Mathf.InverseLerp(0, duration, clock);
            t = Mathf.Clamp01(t);
            set.Update(t);
            return clock >= duration;
        }

        private readonly BlendSet set;
        private readonly float duration;
        private long timestamp;
    }

    internal abstract class BlendValue
    {
        public abstract void Start();
        public abstract void Update(float t);
    }

    internal class BlendValueFloat : BlendValue
    {
        public BlendValueFloat(OutTriggerFloat trigger, float endValue)
        {
            this.trigger = trigger;
            this.start = float.MinValue;
            this.end = endValue;
        }

        public BlendValueFloat(OutTriggerFloat trigger, float startValue, float endValue)
        {
            this.trigger = trigger;
            this.start = startValue;
            this.end = endValue;
        }

        public override void Start()
        {
            if (start == float.MinValue)
                start = trigger.ReadValue();
        }

        public override void Update(float t)
        {
            float value = Mathf.Lerp(start, end, t);
            trigger.Trigger(value);
        }

        private OutTriggerFloat trigger;
        private float start;
        private float end;
    }

    internal class BlendValueColor : BlendValue
    {
        public BlendValueColor(OutTriggerColor trigger, HSVColor endValue)
        {
            this.trigger = trigger;
            this.startWithCurrent = true;
            this.end = endValue;
        }

        public BlendValueColor(OutTriggerColor trigger, HSVColor startValue, HSVColor endValue)
        {
            this.trigger = trigger;
            this.startWithCurrent = false;
            this.start = startValue;
            this.end = endValue;
        }

        public override void Start()
        {
            if (startWithCurrent)
                start = trigger.ReadValueHSV();
        }

        public override void Update(float t)
        {
            HSVColor value;
            value.H = Mathf.Lerp(start.H, end.H, t);
            value.S = Mathf.Lerp(start.S, end.S, t);
            value.V = Mathf.Lerp(start.V, end.V, t);
            trigger.Trigger(value);
        }

        private OutTriggerColor trigger;
        private bool startWithCurrent;
        private HSVColor start;
        private HSVColor end;
    }
}
