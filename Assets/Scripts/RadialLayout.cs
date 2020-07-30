using UnityEngine;
using UnityEngine.UI;
/*
Radial Layout Group by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
public class RadialLayout : LayoutGroup {
    public float fDistanceX;
    public float fDistanceY;
    float Padding = 0;
    
    [Range(0f,360f)]
    public float MinAngle, MaxAngle, StartAngle;
    
    protected override void OnEnable() { base.OnEnable(); CalculateRadial(); }
    public override void SetLayoutHorizontal()
    {
    }
    public override void SetLayoutVertical()
    {
    }
   
    public override void CalculateLayoutInputHorizontal()
    { 
        base.CalculateLayoutInputHorizontal();
        CalculateRadial();
    }

    public override void CalculateLayoutInputVertical()
    { 
    }


    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateRadial();
    }
    #endif
    void CalculateRadial()
    {
        m_Tracker.Clear();

        
        
        float fAngle = StartAngle;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = (RectTransform)transform.GetChild(i);
            if (child != null || child.gameObject.activeInHierarchy) {
                //m_rectChildren.Add(child);
            }

        }
        if (rectChildren.Count < 2) 
            return;
        else if (rectChildren.Count == 2) {
            fDistanceY = 150;
            MinAngle = 180;
            Padding = 0;
        } else if (rectChildren.Count == 3) {
            fDistanceX = 250;
            fDistanceY = 200;
            MinAngle = 240;
            Padding = 50;    
        } else if (rectChildren.Count == 4) {
            fDistanceX = 350;
            fDistanceY = 150;
            MinAngle = 270;
            Padding = 0;
        }

        float fOffsetAngle = ((MaxAngle - MinAngle)) / (rectChildren.Count - 1);

        foreach (RectTransform child in rectChildren) {
        // for (int i = rectChildren.Count; i > 0; i--) {
            // RectTransform child = rectChildren[i-1];
            //Adding the elements to the tracker stops the user from modifiying their positions via the editor.
            m_Tracker.Add(this, child, 
            DrivenTransformProperties.Anchors |
            DrivenTransformProperties.AnchoredPosition |
            DrivenTransformProperties.Pivot);
            Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
            child.localPosition = new Vector3(vPos.x * fDistanceX, vPos.y * fDistanceY + Padding, 0);
            //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
            child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
            fAngle += fOffsetAngle;
    
        }

    }
}
 