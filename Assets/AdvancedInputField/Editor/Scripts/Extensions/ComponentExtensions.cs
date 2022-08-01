using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class ComponentExtensions
	{
		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if(type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach(var pinfo in pinfos)
			{
				try
				{
					if(pinfo.CanWrite)
					{
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
				}
				catch { }
			}

			FieldInfo[] finfos = type.GetFields(flags);
			foreach(var finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return comp as T;
		}
	}
}
