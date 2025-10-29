﻿using UnityEngine;
using PubScale.SdkOne.NativeAds;
using System.Collections;

public class CheckLogo : MonoBehaviour
{
    [SerializeField] NativeAdHolder nativeAdHolder;
    [SerializeField] CanvasGroup bigImgCanvasGrp;

    void Awake()
    {
        if (nativeAdHolder != null)
        {
            nativeAdHolder.Event_OnAdLoaded += OnNativeAdLoaded;
        }
        else
        {
            if (this.gameObject.GetComponentInParent<NativeAdHolder>() != null)
            {
                nativeAdHolder = this.gameObject.GetComponentInParent<NativeAdHolder>();

                if (nativeAdHolder != null)
                {
                    Debug.Log("Assigned NativeAdHolder at runtime");
                    nativeAdHolder.Event_OnAdLoaded += OnNativeAdLoaded;
                }
                else
                {
                    Debug.Log("UNABLE to assign NativeAdHolder at runtime");
                }
            }
            else
                Debug.LogWarning("PubScaleSDK: Please assign a nativeAdHolder in CheckLogo");
        }

        if (bigImgCanvasGrp != null)
            bigImgCanvasGrp.alpha = 0;

        if (nativeAdHolder != null && nativeAdHolder.adDisplay != null && nativeAdHolder.adDisplay.adIconImg != null)
        {
            if (nativeAdHolder.adDisplay.adIconImg.sprite != null)
                nativeAdHolder.adDisplay.adIconImg.sprite = null;
        }

    }

    private void OnNativeAdLoaded(AdEventInfo args)
    {
        if (nativeAdHolder != null)
        {
            if (bigImgCanvasGrp != null)
                bigImgCanvasGrp.alpha = 0;

            StartCoroutine(CheckAdLogoCreative());
        }
    }

    IEnumerator CheckAdLogoCreative()
    {
        if (nativeAdHolder != null)
        {
            yield return new WaitForSeconds(0.5f);

            if (nativeAdHolder != null && nativeAdHolder.adDisplay != null && nativeAdHolder.adDisplay.adIconImg != null)
            {
                if (nativeAdHolder.adDisplay.adIconImg.sprite == null)
                {
                    // Take alternative measures if Ad Logo is not present


                    // Example - Showing Big Image instead

                    if (bigImgCanvasGrp != null)
                    {

                        bigImgCanvasGrp.alpha = 1;

                        if (bigImgCanvasGrp.gameObject.activeSelf == false)
                            bigImgCanvasGrp.gameObject.SetActive(true);

                        /*

                        // DEBUG

                        if (bigImgCanvasGrp.GetComponent<Image>() != null)
                        {
                            if (bigImgCanvasGrp.GetComponent<Image>().sprite != null)
                                Debug.Log("bigImgCanvasGrp BgImg sprite NOT NULL");
                            else
                                Debug.Log("bigImgCanvasGrp BgImg IS NULL");
                        }

                        */

                    }
                }
                else
                {
                    if (bigImgCanvasGrp != null)
                    {
                        // Make sure big image is not visible is logo is present
                        bigImgCanvasGrp.alpha = 0;
                    }
                }
            }
        }
    }


}
