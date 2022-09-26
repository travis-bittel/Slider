using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : Box
{
    void Awake()
    {
        SetPaths();
/*        foreach (Path path in paths)
        {
            path.ChangePair();
        }*/

        currentShape = shapes[currentShapeIndex];
        paths[currentDirectionIndex].Activate(isDefaultCurrentPath());
        CreateShape();
    }

    private new void OnEnable()
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    private new void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CreateShape();
    }
    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        CreateShape();
    }

    public void ChangeShape()
    {
        currentShapeIndex = (currentShapeIndex + 1) % shapes.Count;
        currentShape = shapes[currentShapeIndex];
        CreateShape();
    }

}
