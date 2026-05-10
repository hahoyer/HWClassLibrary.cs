using System.Diagnostics;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Lazily computes a value the first time it is requested and caches the result. Recursive
///     evaluation is detected and turned into an assertion. Validity can be invalidated via
///     <see cref="IsValid" /> so the producer runs again on the next access.
/// </summary>
/// <typeparam name="TValueType"> Type of the cached value. </typeparam>
[PublicAPI]
[Serializable]
[DebuggerDisplay("{D(),nq}")]
public sealed class ValueCache<TValueType>
{
	/// <summary><c>true</c> while the producer is currently running; used to detect recursion.</summary>
	[EnableDumpExcept(false)]
	public bool IsBusy { get; private set; }

	readonly Func<TValueType> CreateValue;
	TValueType? Data;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ValidityState;

	/// <summary>The cached value; computed on first access and on every access after invalidation.</summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public TValueType Value
	{
		get
		{
			Ensure();
			return Data!;
		}
	}

	/// <summary>
	///     Whether the cache currently holds a computed value. Setting to <c>false</c> drops the cache;
	///     setting to <c>true</c> forces immediate evaluation.
	/// </summary>
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

	/// <param name="createValue"> Producer invoked on first access to compute the cached value. </param>
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

/// <summary>
///     Heterogeneous bag of cached items keyed by an arbitrary token (typically the producing
///     delegate). Backs <see cref="ValueCacheExtension" />, which adds typed access on top.
/// </summary>
[PublicAPI]
public sealed class ValueCache : Dictionary<object, object>
{
	/// <summary>Marker interface for objects that own a <see cref="ValueCache" /> instance.</summary>
	public interface IContainer
	{
		/// <summary>The cache associated with this container.</summary>
		ValueCache Cache { get; }
	}
}