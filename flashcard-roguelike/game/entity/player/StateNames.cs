using Godot;
using System;
using System.Linq;
using System.Reflection;

public static class StateNames
{
	public static String base_state{ get; set;} = "base_state";
	public static String idle{ get; set;}  = "idle";
	public static String run{ get; set;}  = "run";
	public static String sprint{ get; set;}  = "sprint";
	public static String interact{ get; set;}  = "interact";
	public static String jump{ get; set;}  = "jump";
	public static String midair{ get; set;}  = "midair";
	public static String landing{ get; set;}  = "landing";
	public static String death{ get; set;}  = "death";
	
}
