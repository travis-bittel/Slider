using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Path : MonoBehaviour
{
    public bool active = false;
    public Path pair;
    private Shape currentShape = null;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    public bool horizontal = false;


    [Header("Animation Blobs")]
    Direction direction;
    public GameObject blob;
    public int timeBetweenCreation = 50;
    public float blobspeed = 1f;
    public int travelDistance = 0;

    private int count = 0;

    void Update()
    {
        if (active)
        {
            if (count >= timeBetweenCreation)
            {
                CreateBlob();
                count = 0;
            }
            count++;
        }
    }

    public void CreateBlob()
    {
        GameObject go = Instantiate(blob);
        Blob new_blob = go.GetComponent<Blob>();
        new_blob.transform.parent = this.transform;

        BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
        {
            travelDistance = (int)collider.size.x + 1;
            if (pair != null)
            {
                travelDistance += (int)pair.GetComponent<BoxCollider2D>().size.x;
            }
        } else
        {
            travelDistance = (int)collider.size.y + 1;
            if (pair != null)
            {
                travelDistance += (int)pair.GetComponent<BoxCollider2D>().size.y;
            }
        }
        new_blob.UpdateBlobOnPath(defaultAnim, direction, blobspeed, travelDistance, pair);

        // update this to be the right end of the path
        if (direction == Direction.LEFT)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2), 0, 0);
        }
        else if (direction == Direction.RIGHT)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2), 0, 0);
        }
        else if (direction == Direction.UP)
        {
            new_blob.transform.localPosition = new Vector3(0, collider.offset.y - (collider.size.y / 2), 0);
        }
        else
        {
            new_blob.transform.localPosition = new Vector3(0, collider.offset.y + (collider.size.y / 2), 0);
        }
    }

    public void Activate(bool right, Shape shape)
    {
        //print("activating path: " + right + " for " + this.gameObject.name);
        active = true;
        if (right)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.green;   //right or down
            defaultAnim = true;
        } else
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.magenta; //up or left
            defaultAnim = false;
        }

        if (pair != null && !pair.isActive())
        {
            pair.Activate(right, shape);
        }
        currentShape = shape;
    }

    public void Deactivate()
    {
        currentShape = null;
        GetComponentInChildren<SpriteRenderer>().color = Color.white;       //unactivated
        active = false;

        if (pair != null && pair.isActive())
        {
            pair.Deactivate();
        }
    }

    public bool isActive()
    {
        return active;
    }

    public bool getAnimType()
    {
        return defaultAnim;
    }

    public void ChangePair()
    {
        pair = null;
        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);


        if (!horizontal)
        {
            one = new Vector2(0, 1);
            two = new Vector2(0, -1);
        }

        Physics2D.queriesStartInColliders = false;

        RaycastHit2D checkOne = Physics2D.Raycast(transform.position, one.normalized, 7, LayerMask.GetMask("JunglePaths"));
        RaycastHit2D checkTwo = Physics2D.Raycast(transform.position, two.normalized, 7, LayerMask.GetMask("JunglePaths"));

        // print("");
        //want to find the closest bin or box and stile
        if (checkOne.collider != null)
        {
            //check not on same stile
            pair = checkOne.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            }
            else
            {
                pair = null;
            }
        }
        if (checkTwo.collider != null && pair == null)
        {
            pair = checkTwo.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            }
            else
            {
                pair = null; 
            }
        }

        Physics2D.queriesStartInColliders = true;
    }

    private void OnDrawGizmos()
    {
        if (this.transform.localEulerAngles.z == -90 || this.transform.localEulerAngles.z == 90)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(0, 1, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(0, -1, 0) * 5);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(1, 0, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(-1, 0, 0) * 5);
        }
    }
}
