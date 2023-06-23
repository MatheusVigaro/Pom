namespace Pom.Eff;

public sealed record EffectDefinition(DevInterface.RoomSettingsPage.DevEffectsCategories? category)
{
	internal bool _sealed;
	internal Dictionary<string, EffectField> _fields { get; } = new();
	private System.Collections.ObjectModel.ReadOnlyDictionary<string, EffectField> _c_fieldReadOnly;
	public System.Collections.ObjectModel.ReadOnlyDictionary<string, EffectField> Fields
	{
		get {
			_c_fieldReadOnly ??= new(_fields);
			return _c_fieldReadOnly;
		}
	}
	public static EffectDefinition @default = new(category: null);

	private static void __ValidateDefaultValue(DataType t, object? value)
	{
		Type? needed = t switch
		{
			DataType.Int => typeof(int),
			DataType.Float => typeof(float),
			DataType.Bool => typeof(bool),
			DataType.String => typeof(string),
			_ => null
		};
		if (needed != value?.GetType())
		{
			throw new ArgumentException($"Invalid default argument value {value} for {t}");
		}
	}

	public EffectDefinition AddField(EffectField field)
	{
		if (_sealed) return this;
		_fields[field.Name] = field;
		return this;
	}
	public EffectDefinition Seal() {
		this._sealed = true;
		return this;
	}
}

#pragma warning restore 1591