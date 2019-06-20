using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    // Use this for initialization
    //Taken from  http://gamedesigntheory.blogspot.ca/2010/09/controlling-aspect-ratio-in-unity.html
    /// <summary>
    /// Now the next step is to add a second camera to render the "black bar" region of the screen. While you can choose to render whatever the second camera is pointed at, in most cases you'll want to set the camera to render only a flat color such as black. To do this:
    ///Create the camera by choosing GameObject -> Create Other -> Camera from the editor's menu.
    ///Set the camera's depth value to -2 so it's rendered underneath the main camera(whose depth value defaults to -1).
    ///To set the black bar region to a solid color, set the camera's Clear Flags to "Solid Color", set the Culling Mask to "Nothing", and finally the Background to the desired color.
    ///That's all you need to do. Now your game will run with the desired aspect ratio regardless of the user's choice of resolution.
    /// </summary>
    void Start()
    {
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetaspect = 16.0f / 9.0f;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        // obtain camera component so we can modify its viewport
        Camera camera = GetComponent<Camera>();

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
