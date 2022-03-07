using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using System;


namespace Unity.InteractiveTutorials
{
    [Serializable]
    class TutorialParagraphView
    {
        public TutorialParagraphView(TutorialParagraph paragraph, EditorWindow window, string orderedListDelimiter, string unorderedListBullet, int instructionIndex)
        {
            this.paragraph = paragraph;
            if (paragraph.type == ParagraphType.Instruction)
            {
                if (m_FadeGroupAnim == null) m_FadeGroupAnim = new AnimBool(false);
                m_FadeGroupAnim.valueChanged.AddListener(window.Repaint);
            }
            this.orderedListDelimiter = orderedListDelimiter;
            this.unorderedListBullet = unorderedListBullet;
            this.m_InstructionIndex = instructionIndex;
        }

        public void ResetState()
        {
            //m_ShouldShowText = false;
            //hasChangedOnActive = false;
            //hasChangedOnCompletion = false;
        }

        public void SetWindow(TutorialWindow window)
        {
            m_TutorialWindow = window;

            if (m_FadeGroupAnim == null)
                m_FadeGroupAnim = new AnimBool(false);
            m_FadeGroupAnim.valueChanged.AddListener(window.Repaint);
        }

        public TutorialParagraph paragraph;

        private AnimBool m_FadeGroupAnim = new AnimBool(false);

        // TODO proper clean-up for unused code
        //private bool m_ShouldShowText;
        //private bool hasChangedOnCompletion = false;
        //private bool hasChangedOnActive = false;

        private string orderedListDelimiter, unorderedListBullet;

        private int m_InstructionIndex;

        TutorialWindow m_TutorialWindow;

        public void RepaintSoon()
        {
            m_TutorialWindow.Repaint();
            m_TutorialWindow.UpdateVideoFrame(videoTextureCache);
            EditorApplication.update -= RepaintSoon;
        }

        Texture videoTextureCache;

        bool repainting = false;
        
        public void Draw(ref bool previousTaskState, bool pageCompleted)
        {
            switch (paragraph.type)
            {
                // TODO proper clean-up for unused code
                /*
                case ParagraphType.Icons:
                    using (var horizontal = new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        foreach (var icon in paragraph.icons)
                        {
                            GUIStyle style = icon.GetStyle();
                            if (style != null && style != GUIStyle.none)
                                GUILayout.Box(icon.GetTexture(), style);
                            else
                                GUILayout.Box(icon.GetTexture(), GUI.skin.box);
                            GUILayout.FlexibleSpace();
                        }
                    }
                    break;
                case ParagraphType.Instruction:
                    var completed = pageCompleted;
                    if (!pageCompleted)
                    {
                        completed = paragraph.completed;
                        if (!previousTaskState)
                            completed = false;
                    }
                    bool isActiveCriterion = !completed && previousTaskState;
                    
                    using (var verticalGroup = new EditorGUILayout.VerticalScope())
                    {
                        GUIStyle bgStyle;

                        if (isActiveCriterion)
                            bgStyle = AllTutorialStyles.activeElementBackground;
                        else if (completed)
                            bgStyle = AllTutorialStyles.completedElementBackground;
                        else
                            bgStyle = AllTutorialStyles.inActiveElementBackground;

                        //The scope of the colored/faded checkbox and summary
                        using (var backgroundElement = new EditorGUILayout.HorizontalScope(bgStyle))
                        {
                            AllTutorialStyles.instructionLabel.normal.textColor = previousTaskState ? Color.black : Color.gray;
                            GUILayout.Label(GUIContent.none, completed ? AllTutorialStyles.instructionLabelIconCompleted : AllTutorialStyles.instructionLabelIconNotCompleted);
                            GUILayout.Label(paragraph.summary, AllTutorialStyles.instructionLabel);

                            if (isActiveCriterion && !hasChangedOnActive)
                            {
                                hasChangedOnActive = true;
                                m_ShouldShowText = true;
                                AnalyticsHelper.ParagraphStarted(m_InstructionIndex);
                            }
                            else if (completed && !hasChangedOnCompletion)
                            {
                                //If we reached here the criterion has been completed recently, but has not been hidden as we want it to be after completion
                                hasChangedOnCompletion = true;
                                AnalyticsHelper.ParagraphEnded();
                                m_ShouldShowText = false;
                            }
                            if (Event.current.type == EventType.MouseDown && backgroundElement.rect.Contains(Event.current.mousePosition))
                            {
                                m_ShouldShowText = !m_ShouldShowText;
                                GUIUtility.ExitGUI();
                            }

                            m_FadeGroupAnim.target = m_ShouldShowText;
                            if (pageCompleted && !string.IsNullOrEmpty(paragraph.InstructionTitle))
                                m_FadeGroupAnim.value = true;
                        }

                        if (EditorGUILayout.BeginFadeGroup(m_FadeGroupAnim.faded))
                        {
                            var backgroundStyle = isActiveCriterion ? AllTutorialStyles.bgTheInBetweenText : AllTutorialStyles.theInBetweenTextNotActiveOrCompleted;
                            EditorGUILayout.BeginHorizontal(backgroundStyle, GUILayout.ExpandWidth(true));
                            GUILayout.Label(paragraph.InstructionTitle, AllTutorialStyles.theInBetweenText);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndFadeGroup();
                    }
                    previousTaskState = completed;
                    break;
                case ParagraphType.Narrative:
                    EditorGUILayout.BeginHorizontal(AllTutorialStyles.headerBGStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Label(paragraph.InstructionTitle, AllTutorialStyles.narrativeStyle);
                    EditorGUILayout.EndHorizontal();
                    break;
                case ParagraphType.SwitchTutorial:
                    if (GUILayout.Button(paragraph.m_TutorialButtonText, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(250) }))
                    {
                        TutorialManager.instance.StartTutorial(paragraph.m_Tutorial);
                    }
                    break;
                case ParagraphType.OrderedList:
                    EditorGUILayout.BeginVertical(AllTutorialStyles.listBGStyle, GUILayout.ExpandWidth(true));
                    string[] listItems = paragraph.InstructionTitle.Split('\n');
                    for (int i = 0, length = listItems.Length; i < length; ++i)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}{1}", i + 1, orderedListDelimiter), AllTutorialStyles.listPrefix);
                        GUILayout.Label(listItems[i], AllTutorialStyles.list);
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    break;
                case ParagraphType.UnorderedList:
                    EditorGUILayout.BeginVertical(AllTutorialStyles.listBGStyle, GUILayout.ExpandWidth(true));
                    foreach (var listItem in paragraph.InstructionTitle.Split('\n'))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(unorderedListBullet, AllTutorialStyles.listPrefix);
                        GUILayout.Label(listItem, AllTutorialStyles.list);
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    break;
                    */
                case ParagraphType.Image:
                    if (!repainting)
                    {
                        videoTextureCache = paragraph.image;
                        //repainting = true;
                        EditorApplication.update += RepaintSoon;
                        // TODO currently draws image all the time - let's draw it once for each page
                    }
                    break;
                case ParagraphType.Video:
                    if (paragraph.video != null && !repainting)
                    {
                        videoTextureCache = m_TutorialWindow.videoPlaybackManager.GetTextureForVideoClip(paragraph.video);
                        EditorApplication.update += RepaintSoon;
                    }
                    break;
            }
        }
    }
}