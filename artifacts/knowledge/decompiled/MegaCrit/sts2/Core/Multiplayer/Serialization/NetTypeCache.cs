using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public class NetTypeCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TBase> where TBase : class
{
	private readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();

	private readonly List<Type> _idToType;

	public NetTypeCache(List<Type> types)
	{
		types.Sort((Type t1, Type t2) => string.CompareOrdinal(t1.Name, t2.Name));
		_idToType = types;
		for (int i = 0; i < types.Count; i++)
		{
			Type type = types[i];
			if (!type.GetInterfaces().Contains<Type>(typeof(TBase)))
			{
				throw new InvalidOperationException($"Type {types[i]} does not implement interface {typeof(TBase)}!");
			}
			_typeToId[type] = i;
		}
	}

	public int TypeToId<T>() where T : TBase
	{
		return TypeToId(typeof(T));
	}

	public int TypeToId(Type type)
	{
		return _typeToId[type];
	}

	public bool TryGetTypeFromId(int id, out Type? type)
	{
		if (id < 0 || id >= _idToType.Count)
		{
			type = null;
			return false;
		}
		type = _idToType[id];
		return true;
	}
}
