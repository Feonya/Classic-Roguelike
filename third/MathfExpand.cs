using Godot;
using System;

public static class MathfExpand  
{
	public static bool IsEqualApprox(Vector2 a,Vector2 b)
	{
		return Mathf.IsEqualApprox(a.X,b.X) && Mathf.IsEqualApprox(a.Y,b.Y);

	} 


}
