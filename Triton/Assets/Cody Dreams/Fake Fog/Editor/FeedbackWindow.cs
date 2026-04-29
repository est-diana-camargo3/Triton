using UnityEditor;
using UnityEngine;

namespace CodyDreams
{
    namespace Developer
    {
        // LPSNP stands for Low Poly Stylized Nature Pack
        // This script belongs to that pack
        [InitializeOnLoad]
        public class FeedbackWindowFF : EditorWindow
        {
            private const string AssetStoreFeedbackUrl = "https://assetstore.unity.com/packages/tools/particles-effects/fake-fog-296903"; // URL for the current pack
            private const string SupportURL = "https://assetstore.unity.com/packages/3d/props/weapons/pbr-medieval-weapons-301230";
            private const string WebsiteUrl = "https://sites.google.com/view/codydreams/home"; // Your website URL
            private const string ItchUrl = "https://cody-dreams.itch.io/"; // Itch.io games URL
            private const float WindowWidth = 600f;  // Fixed width
            private const float WindowHeight = 350f; // Increased height for additional content
            private static FeedbackWindowFF window;
            private static bool showOnStartup;

            // Static constructor for initializing static members
            static FeedbackWindowFF()
            { 
                    DelayedShowWindow();
            }
            private static void DelayedShowWindow()
            {
                // Remove the delegate after it's called to prevent multiple calls
                EditorApplication.delayCall -= DelayedShowWindow;

                // Double-check the preference just in case it was changed very rapidly during startup
                if (EditorPrefs.GetBool("ShowFeedbackWindowOnStartupFF", true))
                {
                    ShowWindow();
                }   
            }
            [MenuItem("Window/Cody Dremas/FeedBack windows/Fake Fog")]
            public static void ShowWindow()
            {
                if (window == null)
                {
                    window = GetWindow<FeedbackWindowFF>("Feedback");
                }
                else
                {
                    FocusWindowIfItsOpen<FeedbackWindowFF>();
                }

                // Set window size constraints
                window.minSize = new Vector2(WindowWidth, WindowHeight);
                window.maxSize = new Vector2(WindowWidth, WindowHeight);
            }

            private void OnGUI()
            {
                // Centering the content both vertically and horizontally
                EditorGUILayout.BeginVertical(GUILayout.Width(WindowWidth), GUILayout.Height(WindowHeight));
                GUILayout.FlexibleSpace(); // Push content to center vertically

                // Centering the label and button using GUIStyle
                GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleRight
                };

                GUIStyle normalLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                GUILayout.Label("We'd love your feedback!", centeredLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("This Fake Fog asset is perfect to use with our another assets such as Low poly stylized nature", normalLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("Please leave a review and support us.", normalLabelStyle, GUILayout.ExpandWidth(true));

                GUILayout.Space(10); // Space between labels and buttons

                // Feedback button
                if (GUILayout.Button("Give Feedback", GUILayout.ExpandWidth(true)))
                {
                    OpenFeedbackUrl(AssetStoreFeedbackUrl);
                }

                GUILayout.Space(5); // Space between buttons
                GUILayout.Label("Support us by Checking out PBR medieval weapons pack ", normalLabelStyle, GUILayout.ExpandWidth(true));

                // Fake Fog pack button
                if (GUILayout.Button("Check it out", GUILayout.ExpandWidth(true)))
                {
                    OpenFeedbackUrl(SupportURL);
                }

                GUILayout.Space(5); // Space between buttons

                // Website button
                if (GUILayout.Button("Visit Our Website", GUILayout.ExpandWidth(true)))
                {
                    OpenFeedbackUrl(WebsiteUrl);
                }

                GUILayout.Space(5); // Space between buttons

                // Itch.io games button with a note
                if (GUILayout.Button("Support Us by Donating on Itch.io and Playing Our Games", GUILayout.ExpandWidth(true)))
                {
                    OpenFeedbackUrl(ItchUrl);
                }

                GUILayout.Label("Note: These are older game jam projects, so quality may vary.", normalLabelStyle, GUILayout.ExpandWidth(true));

                GUILayout.FlexibleSpace(); // Push content to center vertically

                // Move toggle and label to the bottom-left corner
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace(); // Push toggle and label to the left
                bool newShowOnStartup = EditorGUILayout.Toggle("Don't show this again", showOnStartup);
                if (newShowOnStartup != showOnStartup)
                {
                    // Save the new state if it has changed
                    showOnStartup = newShowOnStartup;
                    EditorPrefs.SetBool("ShowFeedbackWindowOnStartupFF", showOnStartup);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            private void OpenFeedbackUrl(string url)
            {
                Application.OpenURL(url);
            }

            private void OnDestroy()
            {
                // Save the preference when the window is closed
                EditorPrefs.SetBool("ShowFeedbackWindowOnStartupFF", showOnStartup);
            }
        }
    }
}
