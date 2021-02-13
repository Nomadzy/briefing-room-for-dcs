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

using BriefingRoom4DCSWorld.Attributes;
using BriefingRoom4DCSWorld.DB;
using BriefingRoom4DCSWorld.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public class TreeViewPropertyViewer<T>
    {
        private readonly T SelectedObject;
        private readonly TreeView Tree;

        private readonly ContextMenuStrip ContextMenu;
        private Type ObjectType { get { return typeof(T); } }

        public TreeViewPropertyViewer(T selectedObject, TreeView templateTreeView)
        {
            SelectedObject = selectedObject;
            Tree = templateTreeView;

            // Make the context menu a little darker so it stands out from the treeview's background
            Color contextMenuBackColor = Color.FromArgb((int)(Tree.BackColor.R * .8), (int)(Tree.BackColor.G * .8), (int)(Tree.BackColor.B * .8));
            ContextMenu = new ContextMenuStrip { BackColor = contextMenuBackColor, Font = Tree.Font, ForeColor = Tree.ForeColor, ShowImageMargin = false, ShowItemToolTips = true };
            ContextMenu.ItemClicked += OnContextMenuItemClicked;

            SetupTreeView();
            RefreshAll();
            Tree.Sort();
        }

        private void SetupTreeView()
        {
            Tree.Nodes.Clear();

            foreach (PropertyInfo pi in ObjectType.GetProperties())
            {
                CategoryAttribute ca = pi.GetCustomAttribute<CategoryAttribute>();
                if (ca == null) continue;

                if (!Tree.Nodes.ContainsKey(ca.Category)) // Add the category if it's not there already
                    Tree.Nodes.Add(ca.Category, ca.Category);

                // We set the display name now so the tree's alphabetical sorting is correct
                Tree.Nodes[ca.Category].Nodes.Add(pi.Name, GetPropertyDisplayName(pi));

                DescriptionAttribute desc = pi.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null)
                    Tree.Nodes[ca.Category].Nodes[pi.Name].ToolTipText = desc.Description;
            }

            Tree.Sort();
            Tree.NodeMouseClick += OnNodeMouseClick;
        }

        //private void AddTreeNode(string parentNode)
        //{

        //}

        public void RefreshAll()
        {
            List<TreeNode> nodes = Tree.GetAllNodes();
            foreach (TreeNode tn in nodes)
            {
                if (tn.Name == null) continue; // Node has no name, continue
                PropertyInfo pi = ObjectType.GetProperty(tn.Name);
                if (pi == null) continue; // No property has the node's name, continue

                string valueString = pi.GetValue(SelectedObject).ToString();

                if (pi.PropertyType.IsArray)
                {
                    tn.Nodes.Clear();
                    if (pi.PropertyType.GetElementType().IsEnum)
                    {
                        foreach (object o in ((Array)pi.GetValue(SelectedObject)))
                            tn.Nodes.Add(o.ToString());
                    }
                    //{
                    //    foreach (object o in ((Array)pi.GetValue(SelectedObject)))
                    //        valueString += $"{o}, ";
                    //}

                    continue;
                }
                
                if (pi.GetCustomAttribute<DatabaseSourceAttribute>() != null)
                {
                    DatabaseSourceAttribute dsa = pi.GetCustomAttribute<DatabaseSourceAttribute>();
                    if (dsa.AllowRandom && string.IsNullOrEmpty(valueString))
                        valueString = "(Random)";
                    else
                    {
                        DBEntry entry = Database.Instance.GetEntry(dsa.DBEntryType, valueString);
                        if (entry != null) valueString = entry.GUIDisplayName;
                    }
                }

                tn.Text = $"{GetPropertyDisplayName(pi)}: {valueString}";
            }

            //Type type = typeof(T);

            //foreach (PropertyInfo pi in type.GetProperties())
            //{
            //    CategoryAttribute ca = pi.GetCustomAttribute<CategoryAttribute>();
            //    if (ca == null) continue;

            //    if (!Tree.Nodes.ContainsKey(ca.Category)) // Add the category if it's not there already
            //        Tree.Nodes.Add(ca.Category, ca.Category);

            //    Tree.Nodes[ca.Category].Nodes[pi.Name].Text = $"{GetPropertyDisplayName(pi)}: {pi.GetValue(SelectedObject)}";
            //}
        }

        private string GetPropertyDisplayName(PropertyInfo pi)
        {
            DisplayNameAttribute dna = pi.GetCustomAttribute<DisplayNameAttribute>();
            return (dna != null) ? dna.DisplayName : pi.Name;
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;

            Tree.SelectedNode = e.Node;
            //if (e.Node.Nodes.Count > 0)
            //{
            //    if (e.Node.IsExpanded)
            //        e.Node.Collapse();
            //    else
            //        e.Node.Expand();
            //}
            //else
            if (e.Node.Level == 0) return;

                ShowContextMenu(e.Node.Name, e.Location);
            //if (e.Button == MouseButtons.Right)
            //    ShowContextMenu(e.Node.GetPath(), e.Location);
        }

        private void OnContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // No selected node or no clicked item, abort
            if ((Tree.SelectedNode == null) || (e.ClickedItem == null) || (e.ClickedItem.Tag == null))
                return;

            PropertyInfo pi = ObjectType.GetProperty(Tree.SelectedNode.Name);
            if (pi == null) return; // Property doesn't exist, abort

            if (pi.PropertyType.IsArray)
            {
                Type arrayType = pi.PropertyType.GetElementType();

                //List<object> valueList = new List<object>();
                //valueList.AddRange((object[])pi.GetValue(SelectedObject));
                //if (valueList.Contains(e.ClickedItem.Tag))
                //    valueList.Remove(e.ClickedItem.Tag);
                //else
                //    valueList.Add(e.ClickedItem.Tag);
                //pi.SetValue(SelectedObject, valueList.ToArray());

                RefreshAll();
                return;
            }
            else
            {
                pi.SetValue(SelectedObject, e.ClickedItem.Tag);
                RefreshAll();
                return;
            }
        }

        private void ShowContextMenu(string propertyName, Point location)
        {
            PropertyInfo pi = ObjectType.GetProperty(propertyName);
            if (pi == null) return; // Property doesn't exist, abort

            ContextMenu.Items.Clear();

            if (pi.PropertyType.IsEnum) // Property type is an enum
                AddEnumToContextMenu(pi.PropertyType, ContextMenu.Items);
            else if (pi.PropertyType.IsArray && pi.PropertyType.GetElementType().IsEnum) // Property type is an array of enums
                AddEnumToContextMenu(pi.PropertyType.GetElementType(), ContextMenu.Items);
            else if (pi.GetCustomAttribute<DatabaseSourceAttribute>() != null) // Property is a database entry ID
                AddDBEntriesToContextMenu(ContextMenu.Items, pi.GetCustomAttribute<DatabaseSourceAttribute>());
            else if (pi.GetCustomAttribute<IntegerSourceAttribute>() != null) // Property is an integer
                AddIntegersToContextMenu(ContextMenu.Items, pi.GetCustomAttribute<IntegerSourceAttribute>());

            if (ContextMenu.Items.Count == 0) return; // No items, nothing to show
            ContextMenu.Show(Tree, location);
        }

        private void AddDBEntriesToContextMenu(ToolStripItemCollection itemCollection, DatabaseSourceAttribute dsa)
        {
            if (dsa.AllowRandom)
                itemCollection.Add("(Random)").Tag = "";

            ToolStripMenuItem tsmi;

            foreach (DBEntry entry in Database.Instance.GetAllEntries(dsa.DBEntryType))
            {
                if (!string.IsNullOrEmpty(entry.GUICategory) &&
                    !itemCollection.ContainsKey(entry.GUICategory))
                {
                    tsmi = (ToolStripMenuItem)itemCollection.Add(entry.GUICategory);
                    tsmi.BackColor = ContextMenu.BackColor;
                    tsmi.ForeColor = ContextMenu.ForeColor;
                    tsmi.Name = entry.GUICategory;
                    tsmi.DropDownItemClicked += OnContextMenuItemClicked;
                }
            }
            GUITools.SortToolStripItemCollection(itemCollection);

            foreach (DBEntry entry in Database.Instance.GetAllEntries(dsa.DBEntryType))
            {
                if (string.IsNullOrEmpty(entry.GUICategory))
                    tsmi = (ToolStripMenuItem)itemCollection.Add(entry.GUIDisplayName);
                else
                    tsmi = (ToolStripMenuItem)((ToolStripMenuItem)itemCollection[entry.GUICategory]).DropDownItems.Add(entry.GUIDisplayName);

                tsmi.BackColor = ContextMenu.BackColor;
                tsmi.ForeColor = ContextMenu.ForeColor;
                tsmi.Name = entry.ID;
                tsmi.Tag = entry.ID;
                tsmi.ToolTipText = entry.GUIDescription;
            }
        }

        private void AddEnumToContextMenu(Type enumType, ToolStripItemCollection itemCollection)
        {
            foreach (object e in Enum.GetValues(enumType))
                itemCollection.Add(e.ToString()).Tag = e;
        }

        private void AddIntegersToContextMenu(ToolStripItemCollection itemCollection, IntegerSourceAttribute isa)
        {
            if (isa.RandomValue.HasValue)
                itemCollection.Add("(Random)").Tag = isa.RandomValue.Value;

            for (int i = isa.Min; i <= isa.Max; i += isa.Increment)
                itemCollection.Add(isa.Format.Replace("%i", i.ToString())).Tag = i;
        }
    }
}
