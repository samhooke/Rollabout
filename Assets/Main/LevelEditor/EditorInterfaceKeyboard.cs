using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Super important note:
// This script (EditorInterfaceKeyboard.cs) must execute after EditorNode.cs
// So if it stops working (i.e. cannot draw terrain), go to:
// Edit -> Project Settings -> Script Execution Order
// and ensure that EditorInterfaceKeyboard.cs is listed after EditorNode.cs

// The last item is set to __Length to so then the length can be read by doing (int)InterfaceTerrainGroundStyle.__Length
//public enum InterfaceTerrainStyle {GroundGrass, GroundSnow, GroundDesert, RollersGeneral, RollersClouds, RollersBubbles, __Length};
public enum InterfaceTerrainType {Ground, Roller};
public enum InterfaceTerrainGroundStyle {Grass, Snow, Desert, __Length};
public enum InterfaceTerrainRollerStyle {General, Clouds, Bubbles, __Length};
public enum InterfaceTerrainTool {StraightLine, CurveBezierCubic, CurveCircularArc, __Length};
public enum InterfaceTerrainRollerRotationDirection {Clockwise, AntiClockwise};
public enum InterfaceTerrainRollerRotationSpeed {Free, Stationary, VerySlow, Slow, Normal, Fast, VeryFast, __Length}

public enum TextMenu {Main, Navigation, Terrain, TerrainGround, TerrainRoller, Scenery, Objects, Options, OptionsSave, OptionsLoad};

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	MainController mainController;
	EditorController editorController;
	
	TextMenu menuCurrent = TextMenu.Main;
	List<MenuAbstract> menus;
	
	void Awake() {
		// Move text to top right corner
		transform.position = new Vector3(0.01f, 0.99f, 0);
		
		// Get references
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>() as EditorController;
		mainController = (GameObject.Find("MainController") as GameObject).GetComponent<MainController>() as MainController;
		
		// Create all objects for the various menus
		// NOTE:
		// Menu objects are accessed by casting `menuCurrent` to int for the array index
		// As a result, the following Add()s must be in the same order as enum TextMenu
		menus = new List<MenuAbstract>();
		menus.Add(new MenuMain());
		menus.Add(new MenuNavigation());
		menus.Add(new MenuTerrain());
		menus.Add(new MenuTerrainGround());
		menus.Add(new MenuTerrainRoller());
		menus.Add(new MenuScenery());
		menus.Add(new MenuObjects());
		menus.Add(new MenuOptions());
		menus.Add(new MenuOptionsSave());
		menus.Add(new MenuOptionsLoad());
		menus.Add(new MenuError());
		
		// Give each menu object required references
		foreach (MenuAbstract menu in menus) {
			menu.SetReferances(this, mainController, editorController);
		}
	}
	
	void Start() {
		SetMenu(TextMenu.Main);
	}
	
	void Update() {
		guiText.text = menus[(int)menuCurrent].Text();
	}
	
	public void SetMenu(TextMenu menu) {
		menus[(int)menuCurrent].End();
		menuCurrent = menu;
		menus[(int)menuCurrent].Begin();
	}
}

abstract class MenuAbstract {
	
	protected EditorInterfaceKeyboard editorInterfaceKeyboard;
	protected MainController mainController;
	protected EditorController editorController;
	
	abstract public void Begin();
	abstract public string Text();
	abstract public void End();
	
	public void SetReferances(EditorInterfaceKeyboard editorInterfaceKeyboard, MainController mainController, EditorController editorController) {
		this.editorInterfaceKeyboard = editorInterfaceKeyboard;
		this.mainController = mainController;
		this.editorController = editorController;
	}
	
	protected void SetMenu(TextMenu menu) {
		editorInterfaceKeyboard.SetMenu(menu);
	}
}

class MenuMain : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "?   - Help\n" +
			"N   - Navigation\n" +
			"T   - Terrain\n" +
			"S   - Scenery\n" +
			"O   - Objects\n" +
			"Esc - Options";
		
		if (Input.GetKeyDown(KeyCode.N)) {
			SetMenu(TextMenu.Navigation);
		}
		
		if (Input.GetKeyDown(KeyCode.T)) {
			SetMenu(TextMenu.Terrain);
		}
		
		if (Input.GetKeyDown(KeyCode.O)) {
			SetMenu(TextMenu.Objects);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Options);
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuNavigation : MenuAbstract {
	
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "WASD/Arrows - Move\n" +
			"Shift       - Move faster\n" +
			"Esc         - Back";
		
		// Move camera
		editorCameraSpeed = Input.GetKey(KeyCode.LeftShift) ? editorCameraSpeedShift : editorCameraSpeedNormal;
		editorController.GetCamera().gameObject.transform.position += new Vector3(Input.GetAxis("Horizontal") * editorCameraSpeed, Input.GetAxis ("Vertical") * editorCameraSpeed, 0);
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuTerrain : MenuAbstract {

	override public void Begin() {
		editorController.NodesActivate();
	}
	
	override public string Text() {
		string t;

		t = "G   - Ground\n" + 
			"R   - Rollers\n" + 
			"Esc - Back";

		if (Input.GetKeyDown(KeyCode.G)) {
			SetMenu(TextMenu.TerrainGround);
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			SetMenu(TextMenu.TerrainRoller);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}

		return t;
	}
	
	override public void End() {
		editorController.NodesDeactivate();
	}
}

class MenuTerrainBase : MenuAbstract {

	protected class InterfaceTerrainInfo {
		// What type of line or curve it is
		public InterfaceTerrainTool interfaceTerrainTool;

		// Is it ground or roller?
		public InterfaceTerrainType interfaceTerrainType;

		// Ground specific data
		public InterfaceTerrainGroundStyle interfaceTerrainGroundStyle;

		// Roller specific data
		public InterfaceTerrainRollerStyle interfaceTerrainRollerStyle;
		public InterfaceTerrainRollerRotationDirection interfaceTerrainRollerRotationDirection;
		public InterfaceTerrainRollerRotationSpeed interfaceTerrainRollerRotationSpeed;

		public InterfaceTerrainInfo(InterfaceTerrainTool interfaceTerrainTool, InterfaceTerrainGroundStyle interfaceTerrainGroundStyle) {
			this.interfaceTerrainTool = interfaceTerrainTool;

			this.interfaceTerrainType = InterfaceTerrainType.Ground;

			this.interfaceTerrainGroundStyle = interfaceTerrainGroundStyle;
		}

		public InterfaceTerrainInfo(InterfaceTerrainTool interfaceTerrainTool, InterfaceTerrainRollerStyle interfaceTerrainRollerStyle, InterfaceTerrainRollerRotationDirection interfaceTerrainRollerRotationDirection, InterfaceTerrainRollerRotationSpeed interfaceTerrainRollerRotationSpeed) {
			this.interfaceTerrainTool = interfaceTerrainTool;

			this.interfaceTerrainType = InterfaceTerrainType.Roller;

			this.interfaceTerrainRollerStyle = interfaceTerrainRollerStyle;
			this.interfaceTerrainRollerRotationDirection = interfaceTerrainRollerRotationDirection;
			this.interfaceTerrainRollerRotationSpeed = interfaceTerrainRollerRotationSpeed;
		}
	}

	protected int terrainToolMax = (int)InterfaceTerrainTool.__Length;
	protected InterfaceTerrainTool terrainTool = InterfaceTerrainTool.StraightLine;

	int drawStage = 0;
	Vector3[] drawPoints;

	override public void Begin() {
		editorController.NodesActivate();
	}

	// This is overriden by MenuTerrainGround and MenuTerrainRoller
	override public string Text() {
		return "";
	}

	override public void End() {
		editorController.NodesDeactivate();
	}

	//protected void Draw(InterfaceTerrainType interfaceTerrainType, int terrainStyle) {
	protected void Draw(InterfaceTerrainInfo interfaceTerrainInfo) {
		if (Input.GetMouseButtonDown(0)) {
			if (editorController.MouseClaim(editorInterfaceKeyboard.gameObject)) {
				drawStage = 1;
				Vector3 m = editorController.GetMousePos();
				drawPoints = new Vector3[2];
				drawPoints[0] = m;
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			if (drawStage == 1) {
				drawStage = 0;
				Vector3 m = editorController.GetMousePos();
				drawPoints[1] = m;

				TerrainInfo terrainInfo = InterfaceTerrainInfoToTerrainInfo(interfaceTerrainInfo);
				TerrainObjectMaker terrainObjectMaker = new TerrainObjectMaker(terrainInfo);

				Vector3 partPointsDiff = drawPoints[1] - drawPoints[0];
				switch (terrainInfo.terrainBlueprintType) {
				case TerrainBlueprintType.CurveBezierCubic:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.25f);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.75f);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				case TerrainBlueprintType.CurveCircularArc:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.5f);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				case TerrainBlueprintType.StraightLine:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				}

				//terrainObjectMaker.SetSegmentLength(2f);
				terrainObjectMaker.SetIsEditable(true);

				terrainObjectMaker.CreateTerrain();

				editorController.MouseReleaseNextFrame(editorInterfaceKeyboard.gameObject);
			}
		}
	}

	protected string InterfaceTerrainToolToText(InterfaceTerrainTool t) {
		string s = "";
		switch (t) {
		case InterfaceTerrainTool.StraightLine:			s = "Straight line";		break;
		case InterfaceTerrainTool.CurveBezierCubic:		s = "Curve bezier cubic";	break;
		case InterfaceTerrainTool.CurveCircularArc:		s = "Curve circular arc";	break;
		default:										s = "???";					break;
		}
		return s;
	}

	private TerrainBlueprintType InterfaceTerrainToolToTerrainBlueprintType(InterfaceTerrainTool interfaceTerrainTool) {
		
		TerrainBlueprintType terrainBlueprintType;
		
		switch (interfaceTerrainTool) {
		case InterfaceTerrainTool.StraightLine:			terrainBlueprintType = TerrainBlueprintType.StraightLine;		break;
		case InterfaceTerrainTool.CurveBezierCubic:		terrainBlueprintType = TerrainBlueprintType.CurveBezierCubic;	break;
		case InterfaceTerrainTool.CurveCircularArc:		terrainBlueprintType = TerrainBlueprintType.CurveCircularArc;	break;
		default:
			Debug.LogWarning("Unknown terrainTool [" + terrainTool + "]. Defaulting to InterfaceTerrainTool.StraightLine");
			goto case InterfaceTerrainTool.StraightLine;
		}

		return terrainBlueprintType;
	}

	private TerrainType InterfaceTerrainTypeToTerrainType(InterfaceTerrainType interfaceTerrainType) {
		
		TerrainType terrainType;

		switch (interfaceTerrainType) {
			case InterfaceTerrainType.Ground:			terrainType = TerrainType.Ground;				break;
			case InterfaceTerrainType.Roller:			terrainType = TerrainType.Roller;				break;
			default:
			Debug.LogWarning("Unknown terrainInfo.interfaceTerrainType [" + interfaceTerrainType + "]. Defaulting to InterfaceTerrainType.Ground");
			goto case InterfaceTerrainType.Ground;
		}

		return terrainType;
	}

	private TerrainGroundStyle InterfaceTerrainGroundStyleToTerrainGroundStyle(InterfaceTerrainGroundStyle interfaceTerrainGroundStyle) {
		
		TerrainGroundStyle terrainGroundStyle;

		switch (interfaceTerrainGroundStyle) {
		case InterfaceTerrainGroundStyle.Grass:			terrainGroundStyle = TerrainGroundStyle.Grass;		break;
		case InterfaceTerrainGroundStyle.Snow:			terrainGroundStyle = TerrainGroundStyle.Snow;		break;
		case InterfaceTerrainGroundStyle.Desert:		terrainGroundStyle = TerrainGroundStyle.Desert;		break;
		default:
			Debug.LogError("Cannot find a matching TerrainGroundStyle for InterfaceTerrainGroundStyle [" + interfaceTerrainGroundStyle + "]. Defaulting to InterfaceTerrainGroundStyle.Grass");
			goto case InterfaceTerrainGroundStyle.Grass;
		}

		return terrainGroundStyle;
	}

	private TerrainRollerStyle InterfaceTerrainRollerStyleToTerrainRollerStyle(InterfaceTerrainRollerStyle interfaceTerrainRollerStyle) {
		
		TerrainRollerStyle terrainRollerStyle;

		switch (interfaceTerrainRollerStyle) {
		case InterfaceTerrainRollerStyle.General:		terrainRollerStyle = TerrainRollerStyle.General;	break;
		case InterfaceTerrainRollerStyle.Clouds:		terrainRollerStyle = TerrainRollerStyle.Clouds;		break;
		case InterfaceTerrainRollerStyle.Bubbles:		terrainRollerStyle = TerrainRollerStyle.Bubbles;	break;
		default:
			Debug.LogError("Cannot find a matching TerrainRollerStyle for InterfaceTerrainRollerStyle [" + interfaceTerrainRollerStyle + "]. Defaulting to InterfaceTerrainRollerStyle.General");
			goto case InterfaceTerrainRollerStyle.General;
		}

		return terrainRollerStyle;
	}

	private bool InterfaceTerrainRollerRotationSpeedToTerrainRollerFixed(InterfaceTerrainRollerRotationSpeed interfaceTerrainRollerRotationSpeed) {
		
		bool terrainRollerFixed;

		switch (interfaceTerrainRollerRotationSpeed) {
		case InterfaceTerrainRollerRotationSpeed.Stationary:
			terrainRollerFixed = true;
			break;
		case InterfaceTerrainRollerRotationSpeed.Free:
		case InterfaceTerrainRollerRotationSpeed.VerySlow:
		case InterfaceTerrainRollerRotationSpeed.Slow:
		case InterfaceTerrainRollerRotationSpeed.Normal:
		case InterfaceTerrainRollerRotationSpeed.Fast:
		case InterfaceTerrainRollerRotationSpeed.VeryFast:
			terrainRollerFixed = false;
			break;
		default:
			Debug.LogWarning("Invalid InterfaceTerrainRollerRotationSpeed [" + interfaceTerrainRollerRotationSpeed + "]. Defaulting to InterfaceTerrainRollerRotationSpeed.Normal");
			goto case InterfaceTerrainRollerRotationSpeed.Normal;
		}

		return terrainRollerFixed;
	}

	private float InterfaceTerrainRollerRotationSpeedAndInterfaceTerrainRollerRotationToTerrainRollerSpeed(InterfaceTerrainRollerRotationSpeed interfaceTerrainRollerRotationSpeed, InterfaceTerrainRollerRotationDirection interfaceTerrainRollerRotationDirection) {

		float terrainRollerSpeed;

		switch (interfaceTerrainRollerRotationSpeed) {
		case InterfaceTerrainRollerRotationSpeed.Stationary:	terrainRollerSpeed = 0f;		break;
		case InterfaceTerrainRollerRotationSpeed.Free:			terrainRollerSpeed = 0f;		break;
		case InterfaceTerrainRollerRotationSpeed.VerySlow:		terrainRollerSpeed = 1f;		break;
		case InterfaceTerrainRollerRotationSpeed.Slow:			terrainRollerSpeed = 2f;		break;
		case InterfaceTerrainRollerRotationSpeed.Normal:		terrainRollerSpeed = 3f;		break;
		case InterfaceTerrainRollerRotationSpeed.Fast:			terrainRollerSpeed = 4f;		break;
		case InterfaceTerrainRollerRotationSpeed.VeryFast:		terrainRollerSpeed = 5f;		break;
		default:
			Debug.LogWarning("Invalid InterfaceTerrainRollerRotationSpeed [" + interfaceTerrainRollerRotationSpeed + "]. Defaulting to InterfaceTerrainRollerRotationSpeed.Normal");
			goto case InterfaceTerrainRollerRotationSpeed.Normal;
		}

		// Change the sign of the speed to flip the rotation if necessary
		if (interfaceTerrainRollerRotationDirection == InterfaceTerrainRollerRotationDirection.Clockwise)
			terrainRollerSpeed *= -1;

		return terrainRollerSpeed;
	}

	private TerrainInfo InterfaceTerrainInfoToTerrainInfo(InterfaceTerrainInfo interfaceTerrainInfo) {

		TerrainInfo terrainInfo;

		TerrainBlueprintType terrainBlueprintType = InterfaceTerrainToolToTerrainBlueprintType(interfaceTerrainInfo.interfaceTerrainTool);

		switch (interfaceTerrainInfo.interfaceTerrainType) {
		case InterfaceTerrainType.Ground:
			
			// Ground
			TerrainGroundStyle terrainGroundStyle = InterfaceTerrainGroundStyleToTerrainGroundStyle(interfaceTerrainInfo.interfaceTerrainGroundStyle);
			float terrainGroundSegmentLength = 3f;

			terrainInfo = new TerrainInfo(terrainBlueprintType, terrainGroundStyle, terrainGroundSegmentLength);
			
			break;
		case InterfaceTerrainType.Roller:
			
			//Roller
			TerrainRollerStyle terrainRollerStyle = InterfaceTerrainRollerStyleToTerrainRollerStyle(interfaceTerrainInfo.interfaceTerrainRollerStyle);
			float terrainRollerSpacing = 1.5f;
			bool terrainRollerFixed = InterfaceTerrainRollerRotationSpeedToTerrainRollerFixed(interfaceTerrainInfo.interfaceTerrainRollerRotationSpeed);
			float terrainRollerSpeed = InterfaceTerrainRollerRotationSpeedAndInterfaceTerrainRollerRotationToTerrainRollerSpeed(interfaceTerrainInfo.interfaceTerrainRollerRotationSpeed, interfaceTerrainInfo.interfaceTerrainRollerRotationDirection);

			terrainInfo = new TerrainInfo(terrainBlueprintType, terrainRollerStyle, terrainRollerSpacing, terrainRollerFixed, terrainRollerSpeed);

			break;
		default:
			Debug.LogWarning("Invalid InterfaceTerrainType [" + interfaceTerrainInfo.interfaceTerrainType + "]. Defaulting to InterfaceTerrainType.Ground");
			goto case InterfaceTerrainType.Ground;
		}

		return terrainInfo;
	}
}

class MenuTerrainGround : MenuTerrainBase {

	InterfaceTerrainGroundStyle terrainGroundStyle = InterfaceTerrainGroundStyle.Grass;
	int terrainGroundStyleMax = (int)InterfaceTerrainGroundStyle.__Length;

	override public string Text() {

		string t;

		t = "S/D - Select style\n" +
			"T/Y - Select tool\n" +
			"Z/X - Adjust segment length\n" + 
			"Del - Delete selected ground\n" +
			"Esc - Back\n" +
			"\n" +
			"Current style: " + InterfaceTerrainGroundStyleToText(terrainGroundStyle) + "\n" +
			"Current tool:  " + InterfaceTerrainToolToText(terrainTool) + "\n" +
			"\n" +
			"Use mouse to draw and modify ground.";
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Terrain);
		}
		
		if (Input.GetKeyDown(KeyCode.S)) {
			terrainGroundStyle--;
			if ((int)terrainGroundStyle < 0)
				terrainGroundStyle = (InterfaceTerrainGroundStyle)terrainGroundStyleMax - 1;
		}

		if (Input.GetKeyDown(KeyCode.D)) {
			terrainGroundStyle++;
			if ((int)terrainGroundStyle >= terrainGroundStyleMax)
				terrainGroundStyle = (InterfaceTerrainGroundStyle)0;
		}
		
		if (Input.GetKeyDown(KeyCode.T)) {
			terrainTool--;
			if ((int)terrainTool < 0)
				terrainTool = (InterfaceTerrainTool)terrainToolMax - 1;
		}

		if (Input.GetKeyDown(KeyCode.Y)) {
			terrainTool++;
			if ((int)terrainTool >= terrainToolMax)
				terrainTool = (InterfaceTerrainTool)0;
		}

		Draw(new InterfaceTerrainInfo(terrainTool, terrainGroundStyle));

		return t;
	}

	string InterfaceTerrainGroundStyleToText(InterfaceTerrainGroundStyle t) {
		string s = "";
		switch (t) {
		case InterfaceTerrainGroundStyle.Grass:			s = "Grass";				break;
		case InterfaceTerrainGroundStyle.Snow:			s = "Snow";					break;
		case InterfaceTerrainGroundStyle.Desert:		s = "Desert";				break;
		default:										s = "???";					break;
		}
		return s;
	}
}

class MenuTerrainRoller : MenuTerrainBase {

	InterfaceTerrainRollerStyle terrainRollerStyle = InterfaceTerrainRollerStyle.General;
	int terrainRollerStyleMax = (int)InterfaceTerrainRollerStyle.__Length;

	InterfaceTerrainRollerRotationDirection terrainRollerRotationDirection = InterfaceTerrainRollerRotationDirection.Clockwise;

	InterfaceTerrainRollerRotationSpeed terrainRollerRotationSpeed = InterfaceTerrainRollerRotationSpeed.Normal;
	int terrainRollerRotationSpeedMax = (int)InterfaceTerrainRollerRotationSpeed.__Length;

	override public string Text() {

		string t;

		t = "S/D - Select style\n" + 
			"T/Y - Select tool\n" + 
			"E/R - Select rotation direction\n" + 
			"Q/W - Select rotation speed\n" + 
			"Z/X - Adjust selected roller spacing\n" + 
			"Del - Delete selected roller\n" + 
			"Esc - Back\n" + 
			"\n" + 
			"Current style:      " + InterfaceTerrainRollerStyleToText(terrainRollerStyle) + "\n" +
			"Current tool:       " + InterfaceTerrainToolToText(terrainTool) + "\n" + 
			"Current rot. dir. : " + InterfaceTerrainRollerRotationDirectionToText(terrainRollerRotationDirection) + "\n" + 
			"Current rot. speed: " + InterfaceTerrainRollerRotationSpeedToText(terrainRollerRotationSpeed) + "\n" +
			"\n" + 
			"Use mouse to draw and modify rollers.";

		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Terrain);
		}
		
		if (Input.GetKeyDown(KeyCode.S)) {
			terrainRollerStyle--;
			if ((int)terrainRollerStyle < 0)
				terrainRollerStyle = (InterfaceTerrainRollerStyle)terrainRollerStyleMax - 1;
		}

		if (Input.GetKeyDown(KeyCode.D)) {
			terrainRollerStyle++;
			if ((int)terrainRollerStyle >= terrainRollerStyleMax)
				terrainRollerStyle = (InterfaceTerrainRollerStyle)0;
		}
		
		if (Input.GetKeyDown(KeyCode.T)) {
			terrainTool--;
			if ((int)terrainTool < 0)
				terrainTool = (InterfaceTerrainTool)terrainToolMax - 1;
		}
		
		if (Input.GetKeyDown(KeyCode.Y)) {
			terrainTool++;
			if ((int)terrainTool >= terrainToolMax)
				terrainTool = (InterfaceTerrainTool)0;
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			terrainRollerRotationDirection = InterfaceTerrainRollerRotationDirection.Clockwise;
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			terrainRollerRotationDirection = InterfaceTerrainRollerRotationDirection.AntiClockwise;
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			terrainRollerRotationSpeed--;
			if ((int)terrainRollerRotationSpeed < 0)
				terrainRollerRotationSpeed = (InterfaceTerrainRollerRotationSpeed)terrainRollerRotationSpeedMax - 1;
		}

		if (Input.GetKeyDown(KeyCode.W)) {
			terrainRollerRotationSpeed++;
			if ((int)terrainRollerRotationSpeed >= terrainRollerRotationSpeedMax)
				terrainRollerRotationSpeed = (InterfaceTerrainRollerRotationSpeed)0;
		}

		Draw(new InterfaceTerrainInfo(terrainTool, terrainRollerStyle, terrainRollerRotationDirection, terrainRollerRotationSpeed));

		return t;
	}

	string InterfaceTerrainRollerStyleToText(InterfaceTerrainRollerStyle t) {
		string s = "";
		switch (t) {
		case InterfaceTerrainRollerStyle.General:		s = "Rollers";				break;
		case InterfaceTerrainRollerStyle.Clouds:		s = "Cloud rollers";		break;
		case InterfaceTerrainRollerStyle.Bubbles:		s = "Bubble rollers";		break;
		default:										s = "???";					break;
		}
		return s;
	}

	string InterfaceTerrainRollerRotationDirectionToText(InterfaceTerrainRollerRotationDirection r) {
		string s = "";
		switch (r) {
		case InterfaceTerrainRollerRotationDirection.Clockwise:		s = "Clockwise";		break;
		case InterfaceTerrainRollerRotationDirection.AntiClockwise:	s = "Anti-Clockwise";	break;
		default:													s = "???";				break;
		}
		return s;
	}

	string InterfaceTerrainRollerRotationSpeedToText(InterfaceTerrainRollerRotationSpeed r) {
		string s = "";
		switch (r) {
			case InterfaceTerrainRollerRotationSpeed.Free:			s = "No speed (free)";		break;
			case InterfaceTerrainRollerRotationSpeed.Stationary:	s = "No speed (fixed)";		break;
			case InterfaceTerrainRollerRotationSpeed.VerySlow:		s = "Very slow";			break;
			case InterfaceTerrainRollerRotationSpeed.Slow:	 		s = "Slow";					break;
			case InterfaceTerrainRollerRotationSpeed.Normal:		s = "Normal";				break;
			case InterfaceTerrainRollerRotationSpeed.Fast:			s = "Fast";					break;
			case InterfaceTerrainRollerRotationSpeed.VeryFast:		s = "Very fast";			break;
			default:												s = "???";					break;
		}
		return s;
	}
}

class MenuScenery : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		return "Scenery menu is not done yet";
	}
	
	override public void End() {
	}
}

class MenuObjects : MenuAbstract {
	
	enum EditorObject {Player, Rock};
	
	EditorObject currentObject = EditorObject.Player;
	
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
	
		t = "Current object: " + EditorObjectToString(currentObject) + 
			"\n" +
			"P   - Player\n" +
			"R   - Rock\n" +
			"\n" +
			"Esc - Back\n";
		
		if (Input.GetKeyDown(KeyCode.P)) {
			SetCurrentObject(EditorObject.Player);
		}
		
		if (Input.GetKeyDown(KeyCode.R)) {
			SetCurrentObject(EditorObject.Rock);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}
		
		return t;
	}
	
	override public void End() {
	}
	
	void SetCurrentObject(EditorObject currentObject) {
		this.currentObject = currentObject;
	}
	
	string EditorObjectToString(EditorObject editorObject) {
		string t = "";
		switch (editorObject) {
		case EditorObject.Player:
			t = "Player";
			break;
		case EditorObject.Rock:
			t = "Rock";
			break;
		}
		return t;
	}
}

class MenuOptions : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "S   - Save\n" +
			"L   - Load\n" +
			"P   - Play\n" +
			"Esc - Back";
		
		if (Input.GetKeyDown(KeyCode.S)) {
			SetMenu(TextMenu.OptionsSave);
		}
		
		if (Input.GetKeyDown(KeyCode.L)) {
			SetMenu(TextMenu.OptionsLoad);
		}
		
		if (Input.GetKeyDown(KeyCode.P)) {
			editorController.LevelPlay();
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuOptionsSave : MenuAbstract {
	
	string levelSaveLevelName = "";
	
	override public void Begin() {
		levelSaveLevelName = "";
	}
	
	override public string Text() {
		string t;
		
		// Show all files in directory
		DirectoryInfo info = new DirectoryInfo(mainController.GetLevelFilePath());
		FileInfo[] fileInfo = info.GetFiles();
		List<string> files = new List<string>();
		foreach (FileInfo file in fileInfo) {
			if (file.Extension == mainController.GetLevelFileExtension())
				files.Add(Path.GetFileNameWithoutExtension(file.Name));
		}
		
		t = "";
		
		foreach (char c in Input.inputString) {
			if (c == "\b"[0]) {
				// backspace
				if (levelSaveLevelName.Length >= 1) {
					levelSaveLevelName = levelSaveLevelName.Substring(0, levelSaveLevelName.Length - 1);
				}
			} else if (c == "\n"[0] || c == "\r"[0]) {
				// return
			} else {
				levelSaveLevelName += c;
			}
		}
		
		t = levelSaveLevelName + "\n";
		
		if (Input.GetKeyDown(KeyCode.Return)) {
			editorController.LevelSave(levelSaveLevelName);
			SetMenu(TextMenu.Main);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Options);
		}
		
		t += "\nDirectory contents:\n";
		
		foreach (string file in files) {
			t += "\n" + file;
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuOptionsLoad : MenuAbstract {
	
	int levelLoadLevelNum = 0;
	
	override public void Begin() {
		levelLoadLevelNum = 0;
	}
	
	override public string Text() {
		string t;
		
		// Show all files in directory
		DirectoryInfo info = new DirectoryInfo(mainController.GetLevelFilePath());
		FileInfo[] fileInfo = info.GetFiles();
		List<string> files = new List<string>();
		foreach (FileInfo file in fileInfo) {
			if (file.Extension == mainController.GetLevelFileExtension())
				files.Add(Path.GetFileNameWithoutExtension(file.Name));
		}
		
		t = "";
		
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			levelLoadLevelNum++;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			levelLoadLevelNum--;
		}
		if (levelLoadLevelNum < 0)
			levelLoadLevelNum = 0;
		if (levelLoadLevelNum > (files.Count - 1))
			levelLoadLevelNum = (files.Count - 1);
		
		if (files.Count > 0) {
			t = files[levelLoadLevelNum];
		} else {
			// No level files in directory so show blank
			t = "";
		}
		t += "\n";
		
		if (Input.GetKeyDown(KeyCode.Return)) {
			// Check the array of levels is not empty
			if (files.Count > 0) {
				editorController.LevelLoad(files[levelLoadLevelNum]);
				SetMenu(TextMenu.Main);
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Options);
		}
		
		t += "\nDirectory contents:\n";
		
		foreach (string file in files) {
			t += "\n" + file;
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuError : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "Esc - Main menu\n" +
			"\n" +
			"Error! This menu does not exist.";
		
		return t;
	}
	
	override public void End() {
	}
}

/*
class MenuTemplate : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
	}
	
	override public void End() {
	}
}
*/