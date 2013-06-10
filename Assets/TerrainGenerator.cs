using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

}

public class TerrainPart {
	public BlueprintPart blueprintPart;
	Transform parent;
	GameObject[] objs;
	int objPos = 0;
	
	public TerrainPart(BlueprintPart blueprintPart) {
		this.blueprintPart = blueprintPart;
	}
	
	public void SetParent(Transform parent) {
		this.parent = parent;
	}
	
	public void Regenerate() {
		Vector3[] p = blueprintPart.CalculatePoints();
		
		// For (x) fenceposts, we need (x * 2 - 1) fences and fenceposts
		ObjsReset(p.Length * 2 - 1);
		
		ObjsAppend(CreateSphereAt(p[0]));
		for (int i = 1; i < p.Length; i++) {
			ObjsAppend(CreateBlockBetween(p[i-1], p[i]));
			ObjsAppend(CreateSphereAt(p[i]));
		}
	}
	
	void ObjsReset(int size) {
		ObjsDestroy();
		objs = new GameObject[size];
	}
	
	void ObjsAppend(GameObject obj) {
		objs[objPos] = obj;
		objPos++;
	}
	
	void ObjsDestroy() {
		if (objs != null) {
			for (int i = 0; i < objs.Length; i++) {
				GameObject.Destroy(objs[i]);
			}
		}
		objs = null;
		objPos = 0;
	}
	
	GameObject CreateSphereAt(Vector3 pos) {
		GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		s.transform.position = pos;
		
		if (parent != null)
			s.transform.parent = parent;
		
		return s;
	}
	
	GameObject CreateBlockBetween(Vector3 pos1, Vector3 pos2) {
		GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
		b.transform.position = (pos1 + pos2) / 2;
		
		// Rotate block
		float a = Mathf.Rad2Deg * Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x);
		b.transform.eulerAngles = new Vector3(0, 0, a);
		
		// Change its length
		float d = (pos1 - pos2).magnitude;
		b.transform.localScale = new Vector3(d, 1, 1);
		
		if (parent != null)
			b.transform.parent = parent;
		
		return b;
	}
}

public enum BlueprintPartType {StraightLine, CurveBezierCubic, CurveCircularArc}

public class BlueprintPart {
	
	BlueprintPartType type;
	StraightLine straightLine;
	CurveBezierCubic curveBezierCubic;
	CurveCircularArc curveCircularArc;
	// See also: TerrainPartObject.cs
	
	public BlueprintPart(BlueprintPartType type, Vector3 A, Vector3 B) {
		if (type == BlueprintPartType.StraightLine) {
			this.straightLine = new StraightLine(A, B);
			this.type = type;
		} else {
			Debug.LogError("No BlueprintPart of type " + type + " exists for these arguments");
		}
	}
	
	public BlueprintPart(BlueprintPartType type, Vector3 A, Vector3 B, Vector3 C) {
		if (type == BlueprintPartType.CurveCircularArc) {
			this.curveCircularArc = new CurveCircularArc(A, B, C);
			this.type = type;
		} else {
			Debug.LogError("No BlueprintPart of type " + type + " exists for these arguments");
		}
	}
	
	public BlueprintPart(BlueprintPartType type, Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
		if (type == BlueprintPartType.CurveBezierCubic) {
			this.curveBezierCubic = new CurveBezierCubic(A, B, C, D, 20);
			this.type = type;
		} else {
			Debug.LogError("No BlueprintPart of type " + type + " exists for these arguments");
		}
	}
	
	public BlueprintPartType GetType() {
		return type;
	}
	
	public void SetNodePosition(int node, Vector3 pos) {
		// Update the position of one of the nodes defining the shape
		switch (type) {
		case BlueprintPartType.StraightLine:
			straightLine.SetPoint(node, pos);
			break;
		case BlueprintPartType.CurveBezierCubic:
			curveBezierCubic.SetPoint(node, pos);
			break;
		case BlueprintPartType.CurveCircularArc:
			curveCircularArc.SetPoint(node, pos);
			break;
		default:
			Debug.Log("BlueprintPartType " + type + " does not exist.");
			break;
		}
	}
	
	/*
	// Returns the specific point in this part between its beginning and end
	// where `a` is a value between 0.0f and 1.0f.
	public Vector3 CalculatePoint(float a) {
		Vector3 p;
		switch (type) {
		case BlueprintPartType.StraightLine:
			p = straightLine.CalculateLinePoint(a);
			break;
		case BlueprintPartType.CurveBezierCubic:
			p = curveBezierCubic.CalculateCurvePoint(a);
			break;
		case BlueprintPartType.CurveCircularArc:
			p = curveCircularArc.CalculateCurvePoint(a);
			break;
		default:
			p = Vector3.zero;
			Debug.LogError("Invalid type: " + type);
			break;
		}
		return p;
	}
	*/
	
	// Returns all points necessary for generating this part
	public Vector3[] CalculatePoints() {
		Vector3[] p;
		switch (type) {
		case BlueprintPartType.StraightLine:
			p = new Vector3[2];
			p[0] = straightLine.GetPointA();
			p[1] = straightLine.GetPointB();
			break;
		case BlueprintPartType.CurveBezierCubic:
			p = curveBezierCubic.CalculateCurvePoints();
			break;
		case BlueprintPartType.CurveCircularArc:
			p = curveCircularArc.CalculateCurvePoints();
			break;
		default:
			p = new Vector3[1];
			Debug.LogError("Invalid type: " + type);
			break;
		}
		return p;
	}
}

public class StraightLine {
	private Vector3[] p = new Vector3[2];
	
	// # Constructor
	
	public StraightLine(Vector3 A, Vector3 B) {
		SetPointA(A);
		SetPointB(B);
	}
	
	// ## Setting
	
	public void SetPoint(int point, Vector3 p) {
		switch (point) {
		case 0:
			SetPointA(p);
			break;
		case 1:
			SetPointB(p);
			break;
		default:
			Debug.Log("There is not SetPoint for " + point);
			break;
		}
	}
	
	public void SetPointA(Vector3 A) {
		A.z = 0;
		this.p[0] = A;
	}
	
	public void SetPointB(Vector3 B) {
		B.z = 0;
		this.p[1] = B;
	}
	
	// ## Getting
	
	public Vector3 GetPointA() {
		return this.p[0];
	}
	
	public Vector3 GetPointB() {
		return this.p[1];
	}
	
	// ## Calculating
	
	public Vector3 CalculateLinePoint(float a) {
		return p[0] + (p[1] - p[0]) * a;
	}
}

/*
# BezierCubic
Contains data structure for storing the 4 points a cubic bezier requires,
and also how many segments the resulting curve should have. There is also a
function to adjust the number of segments according to how long you desire
each segment to be.

## Constructor
Requires the four points (A, B, C and D) for a cubic bezier. Also requires the
number of segments the resulting curve should have.

## Setting
After creation, each individual point (A, B, C and D) can be adjusted, using
the following functions:
* SetPoint()
* SetPointA()
* SetPointB()
* SetPointC()
* SetPointD()
The number of segments can be adjusted, according to how many you desire in
total, or how long you wish each segment to be (matched best as possible)
using the following functions:
* SetSegmentNum()
* SetSegmentLength()

## Getting
All the cubic curve data can be read using the following functions:
* GetPointA()
* GetPointB()
* GetPointC()
* GetPointD()
* GetSegmentNum()

## Calculating
Some more information can be calculated. These functions should be called
sparingly because the result has to be generated each time:
* CalculateCurvePoint()
* CalculateLength()
*/
public class CurveBezierCubic {
	private Vector3[] p = new Vector3[4];
	private int segments = 1;
	
	// ## Constructor
	
	public CurveBezierCubic(Vector3 A, Vector3 B, Vector3 C, Vector3 D, int segments) {
		SetPointA(A);
		SetPointB(B);
		SetPointC(C);
		SetPointD(D);
		SetSegmentNum(segments);
	}
	
	// ## Setting
	
	public void SetPoint(int point, Vector3 p) {
		switch (point) {
		case 0:
			SetPointA(p);
			break;
		case 1:
			SetPointB(p);
			break;
		case 2:
			SetPointC(p);
			break;
		case 3:
			SetPointD(p);
			break;
		default:
			Debug.Log("There is not SetPoint for " + point);
			break;
		}
	}
	
	public void SetPointA(Vector3 A) {
		A.z = 0;
		this.p[0] = A;
	}
	
	public void SetPointB(Vector3 B) {
		B.z = 0;
		this.p[1] = B;
	}
	
	public void SetPointC(Vector3 C) {
		C.z = 0;
		this.p[2] = C;
	}
	
	public void SetPointD(Vector3 D) {
		D.z = 0;
		this.p[3] = D;
	}
	
	public void SetSegmentNum(int segments) {
		if (segments >= 1) {
			this.segments = segments;
		} else {
			this.segments = 1;
			Debug.LogWarning("Attempted to assign an invalid number of segments to BezierCubic");
		}
	}
	
	public void SetSegmentLength(int segmentLength) {
		// Calculates how many segments are required based upon desired length
		int segments = (int) (CalculateLength() / segmentLength);
		if (segments < 1)
			segments = 1;
		SetSegmentNum(segments);
	}
	
	// ## Getting
	
	public Vector3 GetPointA() {
		return this.p[0];
	}
	
	public Vector3 GetPointB() {
		return this.p[1];
	}
	
	public Vector3 GetPointC() {
		return this.p[2];
	}
	
	public Vector3 GetPointD() {
		return this.p[3];
	}
	
	public int GetSegmentNum() {
		return segments;
	}
	
	// ## Calculating
	
	public Vector3[] CalculateCurvePoints() {
		Vector3[] P = new Vector3[segments + 1];
		
		float a = 1.0f;
		float d = (1 / (float) segments);
		
		for (int i = 0; i <= segments; i++) {
			P[i] = CalculateCurvePoint(a);
			a -= d;
		}
		
		return P;
	}
	
	public Vector3 CalculateCurvePoint(float a) {
		float b = 1.0f - a;
		float a2 = a*a;
		float b2 = b*b;
		return p[0]*a2*a + p[1]*3*a2*b + p[2]*3*a*b2 + p[3]*b2*b;
	}
	
	public float CalculateLength() {
		// Calculate length using 100 segments for a good length estimate
		return CalculateLength(100);
	}
	
	public float CalculateLength(int segmentsToMeasure) {
		float a = 1.0f;
		float d = (1 / (float) segmentsToMeasure);
		
		// Get all points
		Vector3[] P = new Vector3[segmentsToMeasure + 1];
		for (int i = 0; i < segmentsToMeasure; i++) {
			P[i] = CalculateCurvePoint(a);
			a -= d;
		}
		
		// Mesure their length
		float l = 0;
		float x, y;
		for (int i = 1; i < segmentsToMeasure; i++) {
			x = P[i - 1].x - P[i].x;
			y = P[i - 1].y - P[i].y;
			l += Mathf.Sqrt(x * x + y * y);
		}
		
		return l;
	}
}

public class CurveCircularArc {
	private Vector3[] p = new Vector3[3];

	// These values are set by CalculateDiameter() and CalculateMiddle()
	private float calculatedDiameter;
	private Vector3 calculatedMiddle;

	// These values are set by CalculateArcType()
	private bool calculatedArcExists;      // True = an arc exists (will be false if diameter is too large)
	private bool calculatedArcIsReflex;    // True = reflex angle, false = non-reflex angle
	private bool calculatedArcIsClockwise; // True = clockwise, false = anti-clockwise
	
	public CurveCircularArc(Vector3 A, Vector3 B, Vector3 C) {
		SetPointA(A);
		SetPointB(B);
		SetPointC(C);
	}
	
	// ## Setting
	
	public void SetPoint(int point, Vector3 p) {
		switch (point) {
		case 0:
			SetPointA(p);
			break;
		case 1:
			SetPointB(p);
			break;
		case 2:
			SetPointC(p);
			break;
		default:
			Debug.Log("There is not SetPoint for " + point);
			break;
		}
	}
	
	public void SetPointA(Vector3 A) {
		A.z = 0;
		this.p[0] = A;
	}
	
	public void SetPointB(Vector3 B) {
		B.z = 0;
		this.p[1] = B;
	}
	
	public void SetPointC(Vector3 C) {
		C.z = 0;
		this.p[2] = C;
	}
	
	// ## Calculating
	
	public void CalculateDiameter() {
		// Diameter = length of side / sine of opposite angle
		float l = (p[0] - p[1]).magnitude;
		
		float a0 = Mathf.Atan2(p[0].y - p[2].y, p[0].x - p[2].x);
		float a1 = Mathf.Atan2(p[1].y - p[2].y, p[1].x - p[2].x);
		
		float s = Mathf.Sin(a0 - a1);
		
		this.calculatedDiameter = l / s;
	}
	
	public void CalculateMiddle() {
		// Calculate center relative to A (p[0]):
		//Bd = B - A
		//Cd = C - A
		//Dd = 2(Bdx * Cdy - Bdy * Cdx)
		//Ux = (Cdy(Bdx2 + Bdy2) - Bdy(Cdx2 + Cdy2)) / Dd
		//Uy = (Bdx(Cdx2 + Cdy2) - Cdx(Bdx2 + Bdy2)) / Dd
		
		Vector3 Bd = p[1] - p[0];
		Vector3 Cd = p[2] - p[0];
		float Dd = 2 * (Bd.x * Cd.y - Bd.y * Cd.x);
		float Ux = (Cd.y * (Bd.x * Bd.x + Bd.y * Bd.y) - Bd.y * (Cd.x * Cd.x + Cd.y * Cd.y)) / Dd;
		float Uy = (Bd.x * (Cd.x * Cd.x + Cd.y * Cd.y) - Cd.x * (Bd.x * Bd.x + Bd.y * Bd.y)) / Dd;
		
		// Center was calculated relative to A (p[0]) so add it back
		this.calculatedMiddle = p[0] + new Vector3(Ux, Uy, 0);
	}
	
	public void CalculateArcType() {
		// This function finds the correct pair of values for variables `flipAngle` and `flipDirection`
		// through brute force. This is done by generatating a high detailed curve for all 4 combinations
		// and finding which one is the correct arc.

		// The maximum arc diameter allowed. Any arc above this simply becomes a straight line
		const float maxDiameter = 200f;

		// The number of segments used for the arc when brute forcing the solution
		const int bruteForceCurveSegments = 100;

		// Ensure internal calculates are up to date
		CalculateDiameter();
		CalculateMiddle();

		bool flipAngle = false;     // True if angle is reflex, false if angle is not reflex (obtuse, right-angle, acute)
		bool flipDirection = false; // True if angle is clockwise, false if angle is anti-clockwise

		bool foundCorrectBools = false;
		if (Mathf.Abs(calculatedDiameter) < maxDiameter) {
			// First, calculate the boolean arguments for the curve through brute force
			int tempNum = bruteForceCurveSegments;
			Vector3[] temp = new Vector3[tempNum];
			foundCorrectBools = false;
			bool endsAtEndPoint;
			bool intersectsMidPoint;
			for (int j = 0; j < 4 && !foundCorrectBools; j++) {

				switch (j) {
					case 0:
					flipAngle = false;
					flipDirection = false;
					break;
					case 1:
					flipAngle = true;
					flipDirection = false;
					break;
					case 2:
					flipAngle = false;
					flipDirection = true;
					break;
					case 3:
					flipAngle = true;
					flipDirection = true;
					break;
				}

				intersectsMidPoint = false;

				for (int i = 0; i < tempNum; i++) {
					temp[i] = CalculateCurvePointRaw((float) i / (float) (tempNum - 1), flipAngle, flipDirection);

					if (!intersectsMidPoint)
						if (PointNearPoint(temp[i], p[1], 0.5f))
							intersectsMidPoint = true;
				}

				endsAtEndPoint = PointNearPoint(temp[tempNum - 1], p[2], 0.01f);

				if (endsAtEndPoint && intersectsMidPoint) {
					foundCorrectBools = true;
				}
			}
		} else {
			// Diameter was too big, so do not bother calculating
			//Debug.LogWarning("Diameter was too big");
			foundCorrectBools = false;
		}

		this.calculatedArcExists = foundCorrectBools;
		this.calculatedArcIsReflex = flipAngle;
		this.calculatedArcIsClockwise = flipDirection;
	}

	public Vector3[] CalculateCurvePoints() {
		CalculateArcType();

		Vector3[] points;

		if (this.calculatedArcExists) {
			int num = 20;
			points = new Vector3[num];
			
			//Debug.Log((flipAngle ? "Angle is reflex" : "Angle not reflex") + (flipDirection ? " and clockwise." : " and anti-clockwise."));

			for (int i = 0; i < num; i ++) {
				points[i] = CalculateCurvePointRaw((float) i / (float) (num-1), this.calculatedArcIsReflex, this.calculatedArcIsClockwise);
			}
		} else {
			// No fitting arc could be found, so just generate a straight line
			points = new Vector3[2];
			points[0] = p[0];
			points[0].z = 0;
			points[1] = p[2];
			points[1].z = 0;
		}
			
		return points;
	}
	
	Vector3 CalculateCurvePointRaw(float a, bool flipAngle, bool flipDirection) {
		Vector3 m = calculatedMiddle;

		float angleToA = Mathf.Atan2(p[0].y - m.y, p[0].x - m.x);
		//float angleToB = Mathf.Atan2(p[1].y - m.y, p[1].x - m.x);
		float angleToC = Mathf.Atan2(p[2].y - m.y, p[2].x - m.x);

		float angleStart = angleToA;
		float angleThrough = ShortAngleBetweenAngles(angleToA, angleToC);

		// Flip angle through (X)
		if (flipAngle)
			angleThrough = (360 * Mathf.Deg2Rad) - angleThrough;

		// Flip direction (Z)
		if (flipDirection)
			angleThrough = -angleThrough;

		float r = Mathf.Abs(calculatedDiameter) / 2;


		float d = angleStart + angleThrough * a;

		Vector3 P = Vector3.zero;
		P.x = m.x + (r * Mathf.Cos(d));
		P.y = m.y + (r * Mathf.Sin(d));
		
		return P;
	}

	float ShortAngleBetweenAngles(float a1, float a2) {
		float b = (Mathf.Abs(a1 - a2)) % (360 * Mathf.Deg2Rad);

		if (b > (180 * Mathf.Deg2Rad)) {
			b = (360 * Mathf.Deg2Rad) - b;
		}

		return b;
	}

	// Returns true if two points are very near
	bool PointNearPoint(Vector3 a, Vector3 b, float precision) {
		a.z = 0;
		b.z = 0;
		return ((a - b).magnitude < precision);
	}
}