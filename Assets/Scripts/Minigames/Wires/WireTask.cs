using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WireTask : MonoBehaviour
{
	public List<Color> wireColors = new List<Color>();
	public List<Wire> leftWires = new List<Wire>();
	public List<Wire> rightWires = new List<Wire>();

	private List<Color> availableColors;
	private List<int> availableLeftWireIndex;
	private List<int> availableRightWireIndex;

	public Wire currentDraggedWire;
	public Wire currentHoveredWire;

	public bool isTaskCompleted = false;

    private void Start()
    {
		availableColors = new List<Color>(wireColors);
		availableLeftWireIndex = new List<int>();
		availableRightWireIndex = new List<int>();

		for (int i = 0; i < leftWires.Count; i++) { availableLeftWireIndex.Add(i); }
        for (int i = 0; i < rightWires.Count; i++) { availableRightWireIndex.Add(i); }

		while (availableColors.Count > 0 && availableLeftWireIndex.Count > 0 && availableRightWireIndex.Count > 0)
		{
			Color pickedColor = availableColors[Random.Range(0, availableColors.Count)];
			int pickedLeftWireIndex = Random.Range(0, availableLeftWireIndex.Count);
            int pickedRightWireIndex = Random.Range(0, availableRightWireIndex.Count);

			leftWires[availableLeftWireIndex[pickedLeftWireIndex]].SetColor(pickedColor);
            rightWires[availableRightWireIndex[pickedRightWireIndex]].SetColor(pickedColor);

			availableColors.Remove(pickedColor);
			availableLeftWireIndex.RemoveAt(pickedLeftWireIndex);
            availableRightWireIndex.RemoveAt(pickedRightWireIndex);
        }

        StartCoroutine(CheckTaskCompletion());
    }

	public IEnumerator CheckTaskCompletion()
	{
		while (!isTaskCompleted)
		{
            int successfulWires = 0;
            for (int i = 0; i < rightWires.Count; i++)
            {
                if (rightWires[i].isSuccess)
                {
                    successfulWires++;
                }
            }

            if (successfulWires >= rightWires.Count)
            {
                //Debug.Log("Task Complete");
                isTaskCompleted = true;
            }
            else
            {
                //Debug.Log("Task Incomplete");
            }

            yield return new WaitForSeconds(0.5f);
        }
	}
}

