using Newtonsoft.Json;
namespace Pom.Eff;

/// <summary>
/// Functionality for extending effects with additional settings.
/// </summary>
public static partial class Eff
{
	static Eff()
	{
		try
		{
			__ImplInit();
		}
		catch (Exception ex)
		{
			plog.LogFatal(ex);
		}
	}

	private static void __ImplInit()
	{
		__AddHooks();
	}
	public const float DEVUI_TITLE_WIDTH = 110f;
	public const float V_SPACING = 5f;
	public const float H_SPACING = 5f;
	public const float ROW_HEIGHT = 18f;
	public const float INT_BUTTON_WIDTH = 20f;
	public const float INT_VALUELABEL_WIDTH = 60f;
	public const float NAMELABEL_WIDTH = DEVUI_TITLE_WIDTH;
	public readonly static Dictionary<int, EffectExtraData> attachedData = new();
	public readonly static Dictionary<string, EffectDefinition> effectDefinitions = new();
	internal readonly static List<KeyValuePair<string, string>> __escapeSequences = new()
	{
		new("-", "%1"),
		new( ",","%2" ),
		new (":", "%3"),
		new("%","%0"), // this goes last, very important
	};
	internal static string __EscapeString(string s)
		=> __escapeSequences.Aggregate(s, (s, kvp) => s.Replace(kvp.Key, kvp.Value));
	internal static string __UnescapeString(string s)
		=> System.Linq.Enumerable.Reverse(__escapeSequences).Aggregate(s, (s, kvp) => s.Replace(kvp.Value, kvp.Key));
	#region API
	public static void RegisterEffectDefinition(string name, EffectDefinition definition)
	{
		if (!definition._sealed) throw new ArgumentException("Effect definition not sealed! Make sure to call Seal() after you are done adding fields");
		effectDefinitions[new RoomSettings.RoomEffect.Type(name, true).ToString()] = definition;
	}
	public static void RemoveEffectDefinition(string name)
	{
		effectDefinitions.Remove(name);
	}
	#endregion
	private static Dictionary<string, string> __ExtractRawExtraData(this RoomSettings.RoomEffect effect)
	{
		//List<int> popIndices = new();
		Dictionary<string, string> result = new();
		for (int i = 0; i < effect.unrecognizedAttributes.Length; i++)
		{
			ref string? attr = ref effect.unrecognizedAttributes[i];
			int splitindex = attr.IndexOf(':');
			if (splitindex < 0)
			{
				plog.LogError($"Eff: Unrecognized attribute needs a name! {attr} skipping");
				continue;
			}
			var name = attr.Substring(0, splitindex);
			var value = __UnescapeString(attr.Substring(splitindex + 1));
			plog.LogWarning($"Deserialized named property {name} : {value}");
			result[name] = value;
			attr = null; //remove from array
		}
		effect.unrecognizedAttributes = effect.unrecognizedAttributes.SkipWhile(x => x == null).ToArray();

		return result;
	}

}
