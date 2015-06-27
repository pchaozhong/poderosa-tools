﻿/*
 * Copyright 2011,2015 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 */
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.BuildComponent;
using System.Reflection;
using System;
using System.Diagnostics;

namespace MultilanguageDocumentationPlugIn
{
    /// <summary>
    /// Multilanguage Documentation PlugIn for Sandcastle Help File Builder
    /// </summary>
    [HelpFileBuilderPlugInExport(
        "Multilanguage Documentation Support",
        IsConfigurable = true,
        RunsInPartialBuild = true,
        Version = "1.1",
        Copyright = "Copyright (c) 2011,2015 Poderosa Project, All Rights Reserved",
        Description = "Multilanguage Documentation PlugIn for Sandcastle Help File Builder")]
    public sealed class MultilanguageDocumentationPlugIn : IPlugIn
    {
        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private string targetLanguage = null;

        private const string ENGLISH_TAG_NAME = "en";

        private Regex langTagPattern = new Regex(@"^[a-z][a-z](?:-[A-Z][A-Z])?$");
        private Regex notLangTagPattern = new Regex(@"^(?:ul|ol|dl|em|br|hr|tt)$");

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if (executionPoints == null)
                {
                    executionPoints =
                        new List<ExecutionPoint>() {
                            new ExecutionPoint(BuildStep.GenerateApiFilter, ExecutionBehaviors.Before)
                        };
                }
                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using (var dialog = new SettingsForm(currentConfig))
            {
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                    return dialog.Config;
            }
            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            this.builder = buildProcess;

            var configRoot = configuration.SelectSingleNode("configuration");
            if (configRoot != null)
            {
                var node = configRoot.SelectSingleNode("targetLanguage");
                if (node != null)
                {
                    targetLanguage = node.InnerXml.Trim();
                }
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            builder.ReportProgress("Multilanguage Documentation Support ...");

            if (targetLanguage == null || targetLanguage.Length == 0)
            {
                builder.ReportProgress("No target language was specified.");
                goto DONE;
            }

            builder.ReportProgress("Target language = {0}", targetLanguage);

            foreach (XmlCommentsFile commentsFile in builder.CommentsFiles)
            {
                // Collect elements which have language tags.
                List<XmlNode> docNodeList = new List<XmlNode>();
                foreach (XmlNode docNode in commentsFile.Comments.SelectNodes("doc/members/member//*[en|ja|" + targetLanguage + "]"))
                {
                    docNodeList.Add(docNode);
                }

                // Process each element.
                bool modified = false;
                foreach (XmlNode docNode in docNodeList)
                {
                    // Collect language tags.
                    List<XmlNode> langNodes = new List<XmlNode>();
                    bool hasEnglishDoc = false;
                    bool hasTargetDoc = false;
                    foreach (XmlNode langNode in docNode.ChildNodes)
                    {
                        if (IsLangTag(langNode))
                        {
                            langNodes.Add(langNode);

                            if (langNode.LocalName == ENGLISH_TAG_NAME)
                                hasEnglishDoc = true;

                            if (langNode.LocalName == targetLanguage)
                                hasTargetDoc = true;
                        }
                    }

                    // Determine which language to leave.
                    string languageToLeave;
                    if (hasTargetDoc)
                    {
                        languageToLeave = targetLanguage;
                    }
                    else if (hasEnglishDoc)
                    {
                        languageToLeave = ENGLISH_TAG_NAME;
                        builder.ReportProgress("Missing target language ... use {0} : {1}", languageToLeave, GetMemberName(docNode));
                    }
                    else if (langNodes.Count > 0)
                    {
                        languageToLeave = langNodes[0].LocalName;
                    }
                    else
                    {
                        languageToLeave = null;
                        builder.ReportProgress("No language tag : {0}", GetMemberName(docNode));
                    }

                    // Strip targeted language tag, and remove other language tags.
                    if (languageToLeave != null)
                    {
                        foreach (XmlNode langNode in langNodes)
                        {
                            if (langNode.LocalName == languageToLeave)
                            {
                                StripLangTag(docNode, langNode);
                                modified = true;
                            }
                            else
                            {
                                docNode.RemoveChild(langNode);
                                modified = true;
                            }
                        }
                    }
                }

                if (modified)
                {
                    commentsFile.Save();
                }
            }

            DONE:
            builder.ReportProgress("Multilanguage Documentation Support Done.");
        }

        private bool IsLangTag(XmlNode node)
        {
            return (node is XmlElement) && langTagPattern.IsMatch(node.LocalName) && !notLangTagPattern.IsMatch(node.LocalName);
        }

        private string GetMemberName(XmlNode node)
        {
            XmlNode memberNode = node.ParentNode;
            XmlAttribute memberNameAttr = memberNode.Attributes["name"];
            return (memberNameAttr != null) ? memberNameAttr.Value : "???";
        }

        private void StripLangTag(XmlNode docNode, XmlNode langNode)
        {
            List<XmlNode> innerNodeList = new List<XmlNode>();
            foreach (XmlNode innerNode in langNode.ChildNodes)
            {
                innerNodeList.Add(innerNode);
            }
            langNode.RemoveAll();

            foreach (XmlNode innerNode in innerNodeList)
            {
                docNode.InsertBefore(innerNode, langNode);
            }

            docNode.RemoveChild(langNode);
        }

        public void Dispose()
        {
        }

    }
}
