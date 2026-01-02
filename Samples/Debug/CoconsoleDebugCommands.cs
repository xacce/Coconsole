using Coconsole;
using Unity.Mathematics;
using UnityEngine;


[Coco]
public static class CoconsoleDebugCommands
{
	public enum Test
	{
		Value,
		Notvalue,
		Vavalue,
	}

	[Cmd]
	public static void CocoInt(int p1)
	{
		Debug.Log($"Integer: {p1}");
	}
	[Cmd]
	public static void CocoTest()
	{
		Debug.Log($"test");
	}

	[Cmd]
	public static void CocoInt2(int2 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoInt3(int3 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoFloat2(float2 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoFloat3(float3 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoVector3(Vector3 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoVector2(Vector2 p1)
	{
		Debug.Log($"Value: {p1}");
	}

	[Cmd]
	public static void CocoFloat(float p1)
	{
		Debug.Log($"Float: {p1}");
	}

	[Cmd]
	public static void CocoDouble(double p1)
	{
		Debug.Log($"Double: {p1}");
	}

	[Cmd]
	public static void CocoString(string p1)
	{
		Debug.Log($"String: {p1}");
	}

	[Cmd]
	public static void CocoBool(bool p1)
	{
		Debug.Log($"Bool: {p1}");
	}

	[Cmd]
	public static void CocoShort(short p1)
	{
		Debug.Log($"Short: {p1}");
	}

	[Cmd]
	public static void CocoUShort(ushort p1)
	{
		Debug.Log($"Ushort: {p1}");
	}

	[Cmd]
	public static void CocoULong(ulong p1)
	{
		Debug.Log($"ulong: {p1}");
	}

	[Cmd]
	public static void CocoLong(long p1)
	{
		Debug.Log($"long: {p1}");
	}

	[Cmd]
	public static void CocoByte(byte p1)
	{
		Debug.Log($"byte: {p1}");
	}

	[Cmd]
	public static void CocoEnum(Test p1)
	{
		Debug.Log($"Enum: {p1}");
	}
}