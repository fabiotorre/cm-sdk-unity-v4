using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Calculates WebView positioning based on layout style and device safe areas
    /// </summary>
    public static class CMPLayoutCalculator
    {
        /// <summary>
        /// Calculates WebView rectangle based on layout style
        /// </summary>
        /// <param name="style">Layout style preset</param>
        /// <param name="respectsSafeArea">Whether to apply safe area insets</param>
        /// <returns>Rectangle for WebView positioning</returns>
        public static Rect CalculateWebViewRect(CMPLayoutStyle style, bool respectsSafeArea)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Get safe area if needed
            Rect safeArea = respectsSafeArea ? Screen.safeArea : new Rect(0, 0, screenWidth, screenHeight);

            switch (style)
            {
                case CMPLayoutStyle.FullScreen:
                    return new Rect(
                        safeArea.x,
                        safeArea.y,
                        safeArea.width,
                        safeArea.height
                    );

                case CMPLayoutStyle.TopHalf:
                    float topHeight = safeArea.height * 0.5f;
                    return new Rect(
                        safeArea.x,
                        safeArea.y + safeArea.height - topHeight,
                        safeArea.width,
                        topHeight
                    );

                case CMPLayoutStyle.BottomHalf:
                    float bottomHeight = safeArea.height * 0.5f;
                    return new Rect(
                        safeArea.x,
                        safeArea.y,
                        safeArea.width,
                        bottomHeight
                    );

                default:
                    return safeArea;
            }
        }

        /// <summary>
        /// Converts Unity Rect to screen pixel coordinates
        /// </summary>
        /// <param name="rect">Unity Rect in screen space</param>
        /// <returns>Rectangle in pixel coordinates</returns>
        public static Rect ConvertToPixelCoordinates(Rect rect)
        {
            // Unity's screen space is bottom-left origin
            // WebView expects top-left origin, so we need to flip Y
            float flippedY = Screen.height - rect.y - rect.height;
            
            return new Rect(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(flippedY),
                Mathf.RoundToInt(rect.width),
                Mathf.RoundToInt(rect.height)
            );
        }

        /// <summary>
        /// Gets margin values for WebView positioning
        /// </summary>
        /// <param name="rect">Rectangle in screen space</param>
        /// <returns>Margins as (left, top, right, bottom)</returns>
        public static (int left, int top, int right, int bottom) GetMarginsForRect(Rect rect)
        {
            int left = Mathf.RoundToInt(rect.x);
            int top = Mathf.RoundToInt(Screen.height - rect.y - rect.height);
            int right = Mathf.RoundToInt(Screen.width - rect.x - rect.width);
            int bottom = Mathf.RoundToInt(rect.y);

            return (left, top, right, bottom);
        }
    }
}

