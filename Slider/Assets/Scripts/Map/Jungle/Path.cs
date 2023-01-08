using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;


public class Path : MonoBehaviour
{
    private bool active = false;
    private bool creatingBlobs = true;
    public Path pair;
    private Shape currentShape = null;
    bool defaultAnim = true; //used to see if the sprite needs to be flipped
    public bool horizontal = false;


    [Header("Animation Blobs")]
    public GameObject blob;
    private Direction direction;
    private float timeBetweenCreation = 3.8f;
    private float travelDistance = 0;

    private float timeCount = 4f;

    void Update()
    {
        if (active && creatingBlobs)
        {
            if (timeCount >= timeBetweenCreation)
            {
                CreateBlob();
                timeCount = 0;
            }
            timeCount += Time.deltaTime;
        }
    }

    public void CreateBlob()
    {
        GameObject go = Instantiate(blob);
        Blob new_blob = go.GetComponent<Blob>();
        new_blob.transform.parent = this.transform;

        BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
        travelDistance = (int)this.transform.localScale.x; // + 0.5f
        if (pair != null)
        {
            travelDistance += (int)pair.transform.localScale.x;
        }

        // set blob to be the correct starting position
        if (direction == Direction.LEFT)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2), 0, 0);
        } else if (direction == Direction.DOWN)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - 0.1f, 0, 0);
        }
        else
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2), 0, 0);
        }

        new_blob.UpdateBlobOnPath(defaultAnim, direction, travelDistance, pair, currentShape);
    }

    public void Activate(bool right, Shape shape, bool creating = true)
    {
        creatingBlobs = creating;
        active = true;

        if (defaultAnim != right)
        {
            foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>())
            {
                Destroy(blob.gameObject);
            }
        }

        if (right)
        {
            defaultAnim = true;
        } else
        {
            defaultAnim = false;
        }

        if (horizontal)
        {
            if (defaultAnim)
            {
                direction = Direction.RIGHT;
            }
            else
            {
                direction = Direction.LEFT;
            }
        }
        else
        {
            if (defaultAnim)
            {
                direction = Direction.DOWN;
            }
            else
            {
                direction = Direction.UP;
            }
        }

        if (pair != null && !pair.isActive())
        {
            pair.Activate(right, shape, false);
        }

        currentShape = shape;

        //prepopulate some blobs if there are no blob
      /*  if (this.gameObject.transform.childCount == 0 && creatingBlobs)
        {
            BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
            float length = (int)this.transform.localScale.x;

            if (pair != null)
            {
                length += (int)pair.transform.localScale.x;
            }
            float distancebetween = 3f;
            int blobCount = (int)(length / distancebetween);

            for (int i = 0; i < blobCount; i++)
            {
                GameObject go = Instantiate(blob);
                Blob new_blob = go.GetComponent<Blob>();
                new_blob.transform.parent = this.transform;

                travelDistance = length;

                // set blob to be the correct starting position
                if (direction == Direction.LEFT)
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - (i * distancebetween), 0, 0);
                }
                else if (direction == Direction.DOWN)
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - 0.1f - (i * distancebetween), 0, 0);
                }
                else
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2) + (i * distancebetween), 0, 0);
                }

                travelDistance -= (i * distancebetween);
                print("distance " + travelDistance);

                new_blob.UpdateBlobOnPath(defaultAnim, direction, travelDistance, pair, currentShape);
                new_blob.setSpeed(0);
                new_blob.setAlpha(0);
            }
        }

        print(this.gameObject.transform.childCount);*/

        //fade in blobs
        foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>())
        {
            blob.fadeIn();
        }
    }

    public void Deactivate()
    {
        currentShape = null;
        active = false;

        if (pair != null && pair.isActive())
        {
            pair.Deactivate();
        }

        //fade out all the blobs
        foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>()) {
            blob.fadeOut();
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

        //want to find the closest bin or box and stile
        if (checkOne.collider != null)
        {
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

/*    private void OnDrawGizmos()
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
    }*/
}
