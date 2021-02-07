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

using BriefingRoom4DCSWorld.DB;
using BriefingRoom4DCSWorld.Template;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public class TreeViewManager
    {
        private readonly MissionTemplate Template;
        private readonly TreeView Tree;

        private readonly ContextMenuStrip ContextMenu;

        public TreeViewManager(MissionTemplate template, TreeView templateTreeView)
        {
            Template = template;
            Tree = templateTreeView;
            ContextMenu = new ContextMenuStrip { BackColor = Tree.BackColor, Font = Tree.Font, ForeColor = Tree.ForeColor, ShowImageMargin = false, ShowItemToolTips = true };
            ContextMenu.ItemClicked += OnContextMenuItemClicked;

            SetupTreeView();
            RefreshAll();
            Tree.Sort();
        }

        private void SetupTreeView()
        {
            Tree.Nodes.Clear();

            AddNode("Context", "context");

            AddNode("", "objective");
            AddNode("", "objective", "objectiveCount");
            AddNode("", "objective", "objectiveDistance");

            AddNode("", "theater");
            AddNode("", "theater", "theaterCountries");

            Tree.Nodes.Add("theater", "");

            Tree.NodeMouseClick += OnNodeMouseClick;
        }

        public void RefreshAll()
        {
            GetNode("objective").Text = $"Objective: {Template.ObjectiveType}";
            GetNode("objective", "objectiveCount").Text = $"Count: {Template.ObjectiveCount}";
            GetNode("objective", "objectiveDistance").Text = $"Distance: {((Template.ObjectiveDistanceNM == 0) ? "Random" : $"{Template.ObjectiveDistanceNM} nm")}";

            GetNode("theater").Text = $"Theater: {Template.TheaterID}";
            //GetNode("theater", "theaterCountries").Text = $"Countries: {Template.TheaterRegionsCoalitions}";

            //TemplateTreeView.Nodes["objective"].Text = "Objective: " + Template.ObjectiveType;
            //TemplateTreeView.Nodes["objective"].Nodes["objectiveDistance"].Text = "Distance: " + ((Template.ObjectiveDistanceNM == 0) ? "Random" : $"{Template.ObjectiveDistanceNM} nm");
            //TemplateTreeView.Nodes["theater"].Text = "Theater: " + Template.TheaterID;
        }

        //private void SetNodeText(string text, params string[] nodePath)
        //{
        //    TreeNode node = GetNode(nodePath);
        //    if (node == null) return;

        //    node.Text = text;
        //}

        private void AddNode(string text, params string[] nodePath)
        {
            TreeNodeCollection collection = Tree.Nodes;

            for (int i = 0; i < nodePath.Length; i++)
            {
                if (!collection.ContainsKey(nodePath[i]))
                    collection.Add(nodePath[i], (i == nodePath.Length - 1) ? text : "");
                collection = collection[nodePath[i]].Nodes;
            }
        }

        private TreeNode GetNode(params string[] nodePath)
        {
            TreeNodeCollection collection = Tree.Nodes;

            for (int i = 0; i < nodePath.Length; i++)
            {
                if (!collection.ContainsKey(nodePath[i])) return null;
                if (i == nodePath.Length - 1) return collection[nodePath[i]];
                collection = collection[nodePath[i]].Nodes;
            }

            return null;
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;

            Tree.SelectedNode = e.Node;
            if (e.Button == MouseButtons.Right)
                ShowContextMenu(e.Node.GetPath(), e.Location);
        }

        private void OnContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if ((Tree.SelectedNode == null) || (e.ClickedItem == null))
                return;

            switch (Tree.SelectedNode.Name)
            {
                case "objective": Template.ObjectiveType = (string)e.ClickedItem.Tag; break;
                case "objectiveCount": Template.ObjectiveCount = (int)e.ClickedItem.Tag; break;
                case "objectiveDistance": Template.ObjectiveDistanceNM = (int)e.ClickedItem.Tag; break;
                default: return;
            }

            RefreshAll();
        }

        private void ShowContextMenu(string[] nodePath, Point location)
        {
            ContextMenu.Items.Clear();

            switch (nodePath.Last())
            {
                case "objective": AddDBEntriesToContextMenu<DBEntryObjective>(ContextMenu.Items); break;
                case "objectiveCount": AddIntegerToContextMenu(ContextMenu.Items, 1, TemplateTools.MAX_OBJECTIVES, 1); break;
                case "objectiveDistance": AddIntegerToContextMenu(ContextMenu.Items, TemplateTools.MIN_OBJECTIVE_DISTANCE, TemplateTools.MAX_OBJECTIVE_DISTANCE, 20, "%inm", true); break;
                case "theater": AddDBEntriesToContextMenu<DBEntryTheater>(ContextMenu.Items); break;
                case "theaterCountries": AddEnumToContextMenu<CountryCoalition>(ContextMenu.Items); break;
            }

            if (ContextMenu.Items.Count == 0) return; // No items, nothing to show

            ContextMenu.Show(Tree, location);
        }

        private void AddDBEntriesToContextMenu<T>(ToolStripItemCollection itemCollection, bool addRandomOption = false) where T : DBEntry
        {
            if (addRandomOption)
                itemCollection.Add("(Random)").Tag = "";

            foreach (T entry in Database.Instance.GetAllEntries<T>())
                itemCollection.Add(entry.ID).Tag = entry.ID;
        }

        private void AddEnumToContextMenu<T>(ToolStripItemCollection itemCollection) where T : struct
        {
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
                itemCollection.Add(e.ToString()).Tag = e;
        }

        private void AddIntegerToContextMenu(ToolStripItemCollection itemCollection, int min, int max, int increment = 1, string format = "%i", bool addRandomOption = false)
        {
            if (addRandomOption)
                itemCollection.Add("(Random)").Tag = 0;

            for (int i = min; i <= max; i += increment)
                itemCollection.Add(format.Replace("%i", i.ToString())).Tag = i;
        }
    }
}
