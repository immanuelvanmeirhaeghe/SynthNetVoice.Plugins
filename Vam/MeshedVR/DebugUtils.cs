using UnityEngine;
using System.Text;
using System.Collections.Generic;

namespace MacGruber
{
    static class DebugUtils
	{
        // Explore Storables and child GameObjects of an Atom
        public static void DebugAtom(Atom atom)
		{
            if (atom == null)
            {
                SuperController.LogMessage("DebugAtom: null");
                return;
            }

            StringBuilder builder = new StringBuilder();
			builder.Append("DebugAtom: '").Append(atom.name).Append("' [\n");
			builder.Append("    Category: '").Append(atom.category).Append("'\n");
			builder.Append("    Storables:\n");
			List<string> storables = atom.GetStorableIDs();
			for (int i=0; i<storables.Count; ++i)
			{
				builder.Append("        '").Append(storables[i]).Append("'\n");
			}			
			builder.Append("    Children:\n");
			foreach (Transform child in atom.transform) {
				builder.Append("        '").Append(child.name).Append("'\n");
			}			
			builder.Append("]");
			SuperController.LogMessage(builder.ToString());
		}

        // Explore childs of an Transform
        public static void DebugTransform(Transform transform)
		{
            if (transform == null)
            {
                SuperController.LogMessage("DebugTransform: null");
                return;
            }

			StringBuilder builder = new StringBuilder();
			builder.Append("DebugTransform: '").Append(transform.gameObject.name).Append("' [\n");
            builder.Append("    Components:\n");
            Component[] components = transform.GetComponents(typeof(Component));
            foreach (Component component in components)
            {
                builder.Append("        '").Append(component.GetType().ToString()).Append("'\n");
            }
            builder.Append("    Children:\n");
			foreach (Transform child in transform) {
				builder.Append("        '").Append(child.name).Append("'\n");
			}			
			builder.Append("]");
            SuperController.LogMessage(builder.ToString());
		}

        // Explore childs of an Transform
        public static void DebugTransformRecursive(Transform transform)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("DebugTransform: '").Append(transform.gameObject.name).Append("' [\n");
            DebugTransformRecursive(transform, builder, 2);
            builder.Append("]");
            SuperController.LogMessage(builder.ToString());
        }

        private static void DebugTransformRecursive(Transform transform, StringBuilder builder, int indent)
        {
            string indentstr = new string(' ', indent);
            foreach (Transform child in transform)
            {
                builder.Append(indentstr).Append(child.name).Append("\n");
                DebugTransformRecursive(child, builder, indent+2);
            }
        }
    }
}
