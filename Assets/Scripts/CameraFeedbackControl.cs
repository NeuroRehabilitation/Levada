using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFeedbackControl : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        var abstractFeedback = GameObject.FindGameObjectsWithTag("AbstractFeedback");

        switch (TestDetails.TestDesc.Feedback)
        {
            case TestDetails.FeedbackType.Mirror:
                foreach (var feedback in abstractFeedback)
                {
                    var meshRenderers = feedback.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in meshRenderers)
                        meshRenderer.enabled = false;
                }
                gameObject.GetComponent<Camera>().cullingMask |= 1 << 8;
                gameObject.GetComponent<Camera>().cullingMask |= 1 << 10;
                break;
            case TestDetails.FeedbackType.Abstract:
                foreach (var feedback in abstractFeedback)
                {
                    var meshRenderers = feedback.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in meshRenderers)
                        meshRenderer.enabled = true;
                }
                gameObject.GetComponent<Camera>().cullingMask &= ~(1 << 8);
                gameObject.GetComponent<Camera>().cullingMask &= ~(1 << 10);
                break;
            case TestDetails.FeedbackType.Game:
                break;
            case TestDetails.FeedbackType.Control:
                foreach (var feedback in abstractFeedback)
                {
                    var meshRenderers = feedback.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in meshRenderers)
                        meshRenderer.enabled = false;
                }
                gameObject.GetComponent<Camera>().cullingMask &= ~(1 << 8);
                gameObject.GetComponent<Camera>().cullingMask &= ~(1 << 10);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
