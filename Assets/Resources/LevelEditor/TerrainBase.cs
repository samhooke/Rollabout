using UnityEngine;
using System.Collections;

public abstract class TerrainBase : MonoBehaviour {

	protected bool requireNodes;
	public ObjectNode[] nodes;

	protected float segmentLength = 1f;
	private Color partColor = Color.white;

	// Init() is used instead of a constructor because parameters cannot be passed through AddComponent<>()
	// and this class is created in the CreateTerrain() function of class TerrainObjectMaker using AddComponent<>()
	// It is necessary that Init() is called before any other functions in this class
	public void Init(bool requireNodes) {
		// If set to true, nodes will be created when AssignBlueprint() is called
		// If set to false, nodes will not be created
		this.requireNodes = requireNodes;
	}

	public void SetSegmentLength(float segmentLength) {
		this.segmentLength = segmentLength;
	}

	public void SegmentLengthIncrease() {
		segmentLength += 0.5f;
		if (segmentLength > 10f)
			segmentLength = 10f;
	}

	public void SegmentLengthDecrease() {
		segmentLength -= 0.5f;
		if (segmentLength < 1f)
			segmentLength = 1f;
	}

	public void SetColor(Color c) {
		this.partColor = c;
	}

	protected void ApplyColor() {
		exSprite[] sprites = GetComponentsInChildren<exSprite>();
		foreach (exSprite sprite in sprites) {
			sprite.color = partColor;
		}
	}

	protected ObjectNode CreateNode(Vector3 pos, int numControls) {
		GameObject g = new GameObject();
		ObjectNode node = g.AddComponent<ObjectNode>();
		node.SetPosition(pos);
		node.CreateHandles(numControls);
		node.SetTerrain(this);

		// Make the node be a child of the terrain object
		g.transform.parent = this.transform;

		return node;
	}

	public abstract void AssignBlueprint(BlueprintPart blueprintPart);
	public abstract void Regenerate();
}
