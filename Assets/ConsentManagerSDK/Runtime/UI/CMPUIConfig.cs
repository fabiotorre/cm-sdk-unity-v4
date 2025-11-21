using System;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Layout style presets for consent layer positioning
    /// </summary>
    public enum CMPLayoutStyle
    {
        /// <summary>
        /// Full screen coverage
        /// </summary>
        FullScreen = 0,

        /// <summary>
        /// Top half of screen (approximately 50%)
        /// </summary>
        TopHalf = 1,

        /// <summary>
        /// Bottom half of screen (approximately 50%)
        /// Recommended for games - shows game graphics at top
        /// </summary>
        BottomHalf = 2
    }

    /// <summary>
    /// Background styling type
    /// </summary>
    public enum CMPBackgroundType
    {
        /// <summary>
        /// Semi-transparent dimmed overlay
        /// </summary>
        Dimmed,

        /// <summary>
        /// Solid color background
        /// </summary>
        Solid,

        /// <summary>
        /// No background (transparent)
        /// </summary>
        None
    }

    /// <summary>
    /// UI configuration for consent layer appearance and behavior
    /// </summary>
    [Serializable]
    public class CMPUIConfig
    {
        /// <summary>
        /// Layout style preset
        /// </summary>
        public CMPLayoutStyle Layout { get; set; }

        /// <summary>
        /// Background styling type
        /// </summary>
        public CMPBackgroundType Background { get; set; }

        /// <summary>
        /// Background color (used with Dimmed or Solid background)
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Background alpha (0-1, used with Dimmed background)
        /// </summary>
        public float BackgroundAlpha { get; set; }

        /// <summary>
        /// Corner radius for rounded corners (0 for sharp corners)
        /// </summary>
        public float CornerRadius { get; set; }

        /// <summary>
        /// Whether to respect device safe area insets (iOS notch, Android system bars)
        /// </summary>
        public bool RespectsSafeArea { get; set; }

        /// <summary>
        /// Whether to automatically handle orientation changes
        /// </summary>
        public bool AllowsOrientationChanges { get; set; }

        /// <summary>
        /// Whether dark mode styling is enabled
        /// </summary>
        public bool DarkMode { get; set; }

        /// <summary>
        /// Creates default UI configuration (BottomHalf with dimmed background)
        /// </summary>
        public CMPUIConfig()
        {
            Layout = CMPLayoutStyle.BottomHalf;
            Background = CMPBackgroundType.Dimmed;
            BackgroundColor = Color.black;
            BackgroundAlpha = 0.5f;
            CornerRadius = 12f;
            RespectsSafeArea = true;
            AllowsOrientationChanges = true;
            DarkMode = false;
        }

        /// <summary>
        /// Creates UI configuration with specified layout style
        /// </summary>
        public CMPUIConfig(CMPLayoutStyle layout)
        {
            Layout = layout;
            Background = CMPBackgroundType.Dimmed;
            BackgroundColor = Color.black;
            BackgroundAlpha = 0.5f;
            CornerRadius = 12f;
            RespectsSafeArea = true;
            AllowsOrientationChanges = true;
            DarkMode = false;
        }

        /// <summary>
        /// Creates a full screen configuration
        /// </summary>
        public static CMPUIConfig FullScreen()
        {
            return new CMPUIConfig(CMPLayoutStyle.FullScreen);
        }

        /// <summary>
        /// Creates a top half configuration
        /// </summary>
        public static CMPUIConfig TopHalf()
        {
            return new CMPUIConfig(CMPLayoutStyle.TopHalf);
        }

        /// <summary>
        /// Creates a bottom half configuration (recommended for games)
        /// </summary>
        public static CMPUIConfig BottomHalf()
        {
            return new CMPUIConfig(CMPLayoutStyle.BottomHalf);
        }
    }
}

