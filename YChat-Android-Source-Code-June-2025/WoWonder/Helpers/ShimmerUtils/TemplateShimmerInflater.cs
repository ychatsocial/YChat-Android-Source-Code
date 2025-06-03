﻿using System;
using Android.App;
using Android.Views;
using Facebook.Shimmer;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.ShimmerUtils
{
    public class TemplateShimmerInflater
    {
        private ShimmerFrameLayout ShimmerLayout;
        private ShimmerTemplateLayout ShimmerTemplateLayout1, ShimmerTemplateLayout2, ShimmerTemplateLayout3, ShimmerTemplateLayout4, ShimmerTemplateLayout5, ShimmerTemplateLayout6, ShimmerTemplateLayout7, ShimmerTemplateLayout8;

        public void InflateLayout(Activity activity, View inflated, ShimmerTemplateStyle style)
        {
            try
            {
                ShimmerLayout = inflated.FindViewById<ShimmerFrameLayout>(Resource.Id.Shimmer_Layout);

                ShimmerTemplateLayout1 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout1);
                ShimmerTemplateLayout2 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout2);
                ShimmerTemplateLayout3 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout3);
                ShimmerTemplateLayout4 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout4);
                ShimmerTemplateLayout5 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout5);
                ShimmerTemplateLayout6 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout6);
                ShimmerTemplateLayout7 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout7);
                ShimmerTemplateLayout8 = inflated.FindViewById<ShimmerTemplateLayout>(Resource.Id.ShimmerRowLayout8);

                ShimmerTemplateLayout1?.SetTemplateStyle(style);
                ShimmerTemplateLayout2?.SetTemplateStyle(style);
                ShimmerTemplateLayout3?.SetTemplateStyle(style);
                ShimmerTemplateLayout4?.SetTemplateStyle(style);
                ShimmerTemplateLayout5?.SetTemplateStyle(style);
                ShimmerTemplateLayout6?.SetTemplateStyle(style);
                ShimmerTemplateLayout7?.SetTemplateStyle(style);
                ShimmerTemplateLayout8?.SetTemplateStyle(style);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Show()
        {
            try
            {
                if (ShimmerLayout != null)
                {
                    ShimmerLayout.Visibility = ViewStates.Visible;
                    ShimmerLayout.StartShimmer();
                    ShimmerLayout.ShowShimmer(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Hide()
        {
            try
            {
                if (ShimmerLayout != null && ShimmerLayout.IsShimmerVisible)
                {
                    ShimmerLayout.Visibility = ViewStates.Gone;
                    ShimmerLayout.StopShimmer();
                    ShimmerLayout.HideShimmer();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}