#nullable enable
using System;
using System.Collections.Generic;
using Terraria.UI;

namespace RecipeBrowser.UIElements;

/// <summary>
/// Provides utility methods for recursively traversing, searching, and extracting elements from UIElement hierarchies.
/// Useful for locating and collecting specific UI controls in complex Terraria UI trees.
/// </summary>
internal static class UIElementHelpers
{
	/// <summary>
	/// Recursively searches all children of the given UIElement and collects every element whose runtime type matches the specified <paramref name="type"/>.
	/// This is useful for gathering all elements of a particular kind (such as headers or buttons) in complex, nested UI layouts.
	/// </summary>
	/// <param name="parent">The root UIElement from which to start searching.</param>
	/// <param name="type">The exact runtime type to match against child elements.</param>
	/// <param name="outList">The list where all matching elements will be added.</param>
	internal static void GatherElementsByType(UIElement parent, Type type, List<UIElement> outList)
	{
		foreach (var child in parent.Children)
		{
			if (child.GetType() == type)
			{
				outList.Add(child);
			}
			GatherElementsByType(child, type, outList);
		}
	}

	/// <summary>
	/// Recursively searches for the first child element of type <typeparamref name="T"/> in the UIElement hierarchy, starting from <paramref name="parent"/>.
	/// An optional predicate can be provided to filter results based on custom logic.
	/// Returns the first match found in a depth-first search, or <c>null</c> if no matching element is found.
	/// </summary>
	/// <typeparam name="T">The specific UIElement-derived type to search for.</typeparam>
	/// <param name="parent">The UIElement to begin the search from.</param>
	/// <param name="predicate">An optional filter function to further restrict matching elements.</param>
	/// <returns>The first matching child element of type <typeparamref name="T"/>, or <c>null</c> if none found.</returns>
	internal static T? FindChildOfType<T>(UIElement parent, Func<T, bool>? predicate = null)
		where T : UIElement
	{
		foreach (var child in parent.Children)
		{
			if (child is T tChild && (predicate?.Invoke(tChild) ?? true))
			{
				return tChild;
			}
			var found = FindChildOfType(child, predicate);
			if (found != null)
			{
				return found;
			}
		}

		return null;
	}
}
