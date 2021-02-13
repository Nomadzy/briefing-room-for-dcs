/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar
(https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World.
If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    /// <summary>
    /// A "toolbox" static class with some useful methods to help with the user interface.
    /// </summary>
    public static class GUITools
    {
        /// <summary>
        /// Path to the assembly namespace where embedded resources are stored.
        /// </summary>
        private const string EMBEDDED_RESOURCES_PATH = "BriefingRoom4DCSWorld.Resources.";

        /// <summary>
        /// Returns an icon from an embedded resource.
        /// </summary>
        /// <param name="resourcePath">Relative path to the icon from BriefingRoom4DCSWorld.Resources.</param>
        /// <returns>An icon or null if no resource was found.</returns>
        public static Icon GetIconFromResource(string resourcePath)
        {
            Icon icon = null;

            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"{EMBEDDED_RESOURCES_PATH}{resourcePath}"))
            {
                if (stream == null) return null;
                icon = new Icon(stream);
            }

            return icon;
        }

        /// <summary>
        /// Returns an image from an embedded resource.
        /// </summary>
        /// <param name="resourcePath">Relative path to the image from BriefingRoom4DCSWorld.Resources.</param>
        /// <returns>An image or null if no resource was found.</returns>
        public static Image GetImageFromResource(string resourcePath)
        {
            Image image = null;

            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"{EMBEDDED_RESOURCES_PATH}{resourcePath}"))
            {
                if (stream == null) return null;
                image = Image.FromStream(stream);
            }

            return image;
        }

        /// <summary>
        /// "Shortcut" method to set all parameters of an OpenFileDialog and display it.
        /// </summary>
        /// <param name="fileExtension">The desired file extension.</param>
        /// <param name="initialDirectory">The initial directory of the dialog.</param>
        /// <param name="fileTypeDescription">A description of the file type (e.g. "Windows PCM wave files")</param>
        /// <returns>The path to the file to load, or null if no file was selected.</returns>
        public static string ShowOpenFileDialog(string fileExtension, string initialDirectory, string fileTypeDescription = null)
        {
            string fileName = null;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = initialDirectory;
                if (string.IsNullOrEmpty(fileTypeDescription)) fileTypeDescription = $"{fileExtension.ToUpperInvariant()} files";
                ofd.Filter = $"{fileTypeDescription} (*.{fileExtension})|*.{fileExtension}";
                if (ofd.ShowDialog() == DialogResult.OK) fileName = ofd.FileName;
            }

            return fileName;
        }

        /// <summary>
        /// "Shortcut" method to set all parameters of a SaveFileDialog and display it.
        /// </summary>
        /// <param name="fileExtension">The desired file extension.</param>
        /// <param name="initialDirectory">The initial directory of the dialog.</param>
        /// <param name="defaultFileName">The defaule file name.</param>
        /// <param name="fileTypeDescription">A description of the file type (e.g. "Windows PCM wave files")</param>
        /// <returns>The path to the file to save to, or null if no file was selected.</returns>
        public static string ShowSaveFileDialog(string fileExtension, string initialDirectory, string defaultFileName = "", string fileTypeDescription = null)
        {
            string fileName = null;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = initialDirectory;
                sfd.FileName = defaultFileName;
                if (string.IsNullOrEmpty(fileTypeDescription)) fileTypeDescription = $"{fileExtension.ToUpperInvariant()} files";
                sfd.Filter = $"{fileTypeDescription} (*.{fileExtension})|*.{fileExtension}";
                if (sfd.ShowDialog() == DialogResult.OK) fileName = sfd.FileName;
            }

            return fileName;
        }

        ///// <summary>
        ///// Returns the full path of a treeview node in the form of a string array of node keys.
        ///// </summary>
        ///// <param name="node">A treeview node</param>
        ///// <returns>An array of string</returns>
        //public static string[] GetPath(this TreeNode node)
        //{
        //    List<string> path = new List<string>();
        //    do
        //    {
        //        path.Insert(0, node.Name ?? "");
        //        node = node.Parent;
        //    } while (node != null);

        //    return path.ToArray();
        //}

        public static List<TreeNode> GetAllNodes(this TreeView treeView)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (TreeNode child in treeView.Nodes)
                result.AddRange(child.GetAllNodes());
            return result;
        }

        public static List<TreeNode> GetAllNodes(this TreeNode treeNode)
        {
            List<TreeNode> result = new List<TreeNode> { treeNode };
            foreach (TreeNode child in treeNode.Nodes)
                result.AddRange(child.GetAllNodes());
            return result;
        }

        public static void SortToolStripItemCollection(ToolStripItemCollection itemCollection)
        {
            System.Collections.ArrayList oAList = new System.Collections.ArrayList(itemCollection);
            oAList.Sort(new ToolStripItemComparer());
            itemCollection.Clear();

            foreach (ToolStripItem oItem in oAList)
                itemCollection.Add(oItem);
        }

        public class ToolStripItemComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                ToolStripItem oItem1 = (ToolStripItem)x;
                ToolStripItem oItem2 = (ToolStripItem)y;
                return string.Compare(oItem1.Text, oItem2.Text, true);
            }
        }
    }
}