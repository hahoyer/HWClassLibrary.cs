using System.Diagnostics;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
[Serializable]
[DebuggerDisplay("{D(),nq}")]
public sealed class ValueCache<TValueType>
{
	[EnableDumpExcept(false)]
	public bool IsBusy { get; private set; }

	readonly Func<TValueType> CreateValue;
	TValueType? Data;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ValidityState;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public TValueType Value
	{
		get
		{
			Ensure();
			return Data!;
		}
	}

	public bool IsValid
	{
		get => ValidityState;
		set
		{
			if(value)
				Ensure();
			else
				Reset();
		}
	}

	public ValueCache(Func<TValueType> createValue) => CreateValue = createValue;

	string D() => IsBusy? "Evaluation pending" :
		IsValid? $"{Value}" : "Unknown";

	void Ensure()
	{
		if(ValidityState)
			return;

		(!IsBusy).Assert("Recursive attempt to get value.");
		IsBusy = true;
		try
		{
			Data = CreateValue();
			ValidityState = true;
		}
		finally
		{
			IsBusy = false;
		}
	}

	void Reset()
	{
		if(!ValidityState)
			return;

		(!IsBusy).Assert("Attempt to reset value during getting value.");
		IsBusy = true;
		Data = default;
		ValidityState = false;
		IsBusy = false;
	}
}

[PublicAPI]
public sealed class ValueCache : Dictionary<object, object>
{
	public interface IContainer
	{
		ValueCache Cache { get; }
	}
}