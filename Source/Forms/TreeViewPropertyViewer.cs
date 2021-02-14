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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public class TreeViewPropertyViewer<T>
    {
        private readonly T SelectedObject;
        private readonly TreeView Tree;

        private readonly ContextMenuStrip ContextMenu;
        private Type ObjectType { get { return typeof(T); } }

        private string RandomString { get { return GetEnumDisplayName(typeof(AmountN), AmountN.Random); } }

        public TreeViewPropertyViewer(T selectedObject, TreeView templateTreeView)
        {
            SelectedObject = selectedObject;
            Tree = templateTreeView;

            // Make the context menu a little darker so it stands out from the treeview's background
            Color contextMenuBackColor = Color.FromArgb((int)(Tree.BackColor.R * .8), (int)(Tree.BackColor.G * .8), (int)(Tree.BackColor.B * .8));
            ContextMenu = new ContextMenuStrip { BackColor = contextMenuBackColor, Font = Tree.Font, ForeColor = Tree.ForeColor, ShowImageMargin = false, ShowItemToolTips = true };
            ContextMenu.ItemClicked += OnContextMenuItemClicked;

            PopulateTreeView();
            RefreshAll();
            Tree.Sort();
        }

        private void PopulateTreeView()
        {
            Tree.Nodes.Clear();

            if (ObjectType.GetCustomAttribute<TreeViewExtraNodesAttribute>() != null)
                foreach (string extraTreeNode in ObjectType.GetCustomAttribute<TreeViewExtraNodesAttribute>().ExtraNodes)
                {
                    TreeNode node = Tree.Nodes.Add(extraTreeNode, GetPropertyDisplayName(extraTreeNode));
                    node.ToolTipText = GetPropertyToolTip(extraTreeNode);
                    AddTreeViewNodes(node.Nodes, extraTreeNode);
                }

            AddTreeViewNodes(Tree.Nodes, null);

            Tree.Sort();
            Tree.NodeMouseClick += OnNodeMouseClick;
        }

        private void AddTreeViewNodes(TreeNodeCollection nodes, string parentNodeName)
        {
            foreach (PropertyInfo pi in ObjectType.GetProperties())
            {
                TreeViewParentNodeAttribute ppa = pi.GetCustomAttribute<TreeViewParentNodeAttribute>();
                string parent = (ppa != null) ? ppa.PropertyName : null;

                if (parent != parentNodeName) continue;

                TreeNode node = nodes.Add(pi.Name, GetPropertyDisplayName(pi.Name));
                node.ToolTipText = GetPropertyToolTip(pi.Name);
                node.Tag = pi.Name;
                AddTreeViewNodes(node.Nodes, pi.Name);
            }
        }

        public void RefreshAll()
        {
            List<TreeNode> nodes = Tree.GetAllNodes();
            foreach (TreeNode tn in nodes)
            {
                if (tn.Name == null) continue; // Node has no name, continue
                PropertyInfo pi = ObjectType.GetProperty(tn.Name);
                if (pi == null) continue; // No property has the node's name, continue

                object value = pi.GetValue(SelectedObject);
                string valueString = value.ToString();

                if (pi.PropertyType.IsArray)
                {
                    tn.Nodes.Clear();
                    if (pi.PropertyType.GetElementType().IsEnum)
                    {
                        foreach (object o in ((Array)pi.GetValue(SelectedObject)))
                            tn.Nodes.Add(o.ToString());
                    }

                    continue;
                }

                if (pi.PropertyType.IsEnum)
                    valueString = GetEnumDisplayName(pi.PropertyType, value);
                else if (pi.GetCustomAttribute<DatabaseSourceAttribute>() != null)
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
                else if (pi.GetCustomAttribute<IntegerSourceAttribute>() != null)
                {
                    IntegerSourceAttribute isa = pi.GetCustomAttribute<IntegerSourceAttribute>();
                    if (isa.RandomValue.HasValue && (isa.RandomValue.Value == (int)value))
                        valueString = RandomString;
                    else
                        valueString = isa.Format.Replace("%i", valueString);
                }

                tn.Text = $"{GetPropertyDisplayName(pi.Name)}: {valueString}";
            }
        }

        private string GetEnumDisplayName(Type enumType, object value)
        {
            string displayName = Database.Instance.Strings.GetString("Enums", $"{enumType.Name}.{value}");
            if (!string.IsNullOrEmpty(displayName)) return displayName;
            return value.ToString();
        }

        private string GetEnumToolTip(Type enumType, object value)
        {
            return Database.Instance.Strings.GetString("Enums", $"{enumType.Name}.{value}.ToolTip");
        }

        private string GetPropertyDisplayName(string internalName)
        {
            string displayName = Database.Instance.Strings.GetString(ObjectType.Name, internalName);
            if (!string.IsNullOrEmpty(displayName)) return displayName;
            return internalName;
        }

        private string GetPropertyToolTip(string internalName)
        {
            return Database.Instance.Strings.GetString(ObjectType.Name, $"{internalName}.ToolTip");
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;
            if (e.Button != MouseButtons.Right) return;

            Tree.SelectedNode = e.Node;
            //if (e.Node.Nodes.Count > 0)
            //{
            //    if (e.Node.IsExpanded)
            //        e.Node.Collapse();
            //    else
            //        e.Node.Expand();
            //}
            //else
            if (e.Node.Tag == null) return;

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
                itemCollection.Add(RandomString).Tag = "";

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
            {
                ToolStripItem item = itemCollection.Add(GetEnumDisplayName(enumType, e));
                item.Tag = e;
                item.ToolTipText = GetEnumToolTip(enumType, e);
            }
        }

        private void AddIntegersToContextMenu(ToolStripItemCollection itemCollection, IntegerSourceAttribute isa)
        {
            if (isa.RandomValue.HasValue)
                itemCollection.Add(RandomString).Tag = isa.RandomValue.Value;

            for (int i = isa.Min; i <= isa.Max; i += isa.Increment)
                itemCollection.Add(isa.Format.Replace("%i", i.ToString())).Tag = i;
        }
    }
}
