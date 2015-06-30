using UnityEngine;

public class EnumFunctions {

	//Get a random value in a enum
	public T GetRandomEnumValue<T>( int startValue )
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(Random.Range(startValue,A.Length));
		return V;
	}

}
