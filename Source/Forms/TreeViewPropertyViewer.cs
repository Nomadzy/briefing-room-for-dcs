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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public class TreeViewPropertyViewer<T>
    {
        private readonly T SelectedObject;
        private Type SelectedObjectType { get { return typeof(T); } }
        
        private readonly TreeView Tree;

        private readonly ContextMenuStrip ContextMenu;

        private string RandomString { get { return GUITools.GetEnumDisplayName(typeof(AmountN), AmountN.Random); } }

        public event EventHandler OnValueChanged = null;

        public TreeViewPropertyViewer(T selectedObject, TreeView templateTreeView)
        {
            SelectedObject = selectedObject;
            Tree = templateTreeView;

            // Make the context menu a little darker so it stands out from the treeview's background
            Color contextMenuBackColor = Color.FromArgb((int)(Tree.BackColor.R * .8), (int)(Tree.BackColor.G * .8), (int)(Tree.BackColor.B * .8));
            ContextMenu = new ContextMenuStrip { BackColor = contextMenuBackColor, Font = Tree.Font, ForeColor = Tree.ForeColor, ShowCheckMargin = true, ShowImageMargin = false, ShowItemToolTips = true };
            ContextMenu.ItemClicked += OnContextMenuItemClicked;

            PopulateTreeView();
            RefreshAll();
            Tree.Sort();
        }

        private void PopulateTreeView()
        {
            Tree.Nodes.Clear();

            if (SelectedObjectType.GetCustomAttribute<TreeViewExtraNodesAttribute>() != null)
                foreach (string extraTreeNode in SelectedObjectType.GetCustomAttribute<TreeViewExtraNodesAttribute>().ExtraNodes)
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
            foreach (PropertyInfo pi in SelectedObjectType.GetProperties())
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
                PropertyInfo pi = SelectedObjectType.GetProperty(tn.Name);
                if (pi == null) continue; // No property has the node's name, continue

                object value = pi.GetValue(SelectedObject);
                string valueString = value.ToString();

                if (pi.PropertyType.IsArray)
                {
                    tn.Nodes.Clear();
                    if (pi.PropertyType.GetElementType().IsEnum)
                        foreach (object o in ((Array)pi.GetValue(SelectedObject)))
                            tn.Nodes.Add(o.ToString(), GUITools.GetEnumDisplayName(pi.PropertyType.GetElementType(), o));
                    else if (pi.PropertyType.GetElementType() == typeof(string))
                    {
                        if (pi.GetCustomAttribute<DatabaseSourceAttribute>() != null)
                        {
                            DatabaseSourceAttribute dsa = pi.GetCustomAttribute<DatabaseSourceAttribute>();

                            foreach (object o in (Array)pi.GetValue(SelectedObject))
                            {
                                DBEntry entry = Database.Instance.GetEntry(dsa.DBEntryType, o.ToString());
                                tn.Nodes.Add(o.ToString(), (entry != null) ? entry.GUIDisplayName : o.ToString());
                            }
                        }
                        else
                            foreach (object o in (Array)pi.GetValue(SelectedObject))
                                tn.Nodes.Add(o.ToString(), o.ToString());
                    }
                    tn.Expand();

                    continue;
                }

                if (pi.PropertyType.IsEnum)
                    valueString = GUITools.GetEnumDisplayName(pi.PropertyType, value);
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

                // Node is the parent of player flight groups
                if (pi.GetCustomAttribute<PlayersFGParentNodeAttribute>() != null)
                {
                    tn.Nodes.Clear();

                    MissionTemplateFlightGroup[] playerFGs = (MissionTemplateFlightGroup[])SelectedObjectType.GetProperty("PlayerFlightGroups").GetValue(SelectedObject);

                    for (int i = 0; i < playerFGs.Length; i++)
                    {
                        tn.Nodes.Add(playerFGs[i].ToString());

                        // Only show the first group if single player
                        if (((MissionPlayersType)value) == MissionPlayersType.SinglePlayer) break;
                    }
                }

                tn.Text = $"{GetPropertyDisplayName(pi.Name)}: {valueString}";
            }
        }

        private string GetPropertyDisplayName(string internalName)
        {
            string displayName = Database.Instance.Strings.GetString(SelectedObjectType.Name, internalName);
            if (!string.IsNullOrEmpty(displayName)) return displayName;
            return internalName;
        }

        private string GetPropertyToolTip(string internalName)
        {
            return Database.Instance.Strings.GetString(SelectedObjectType.Name, $"{internalName}.ToolTip");
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
            //if (e.Node.Tag == null) return;

            ShowContextMenu(e.Node, e.Location);
            //if (e.Button == MouseButtons.Right)
            //    ShowContextMenu(e.Node.GetPath(), e.Location);
        }

        private void OnContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // No selected node or no clicked item, abort
            if ((Tree.SelectedNode == null) || (e.ClickedItem == null)) return;

            if (e.ClickedItem.Tag == null)
            {
                if (Tree.SelectedNode.Level == 0) return;
                PropertyInfo parentPi = SelectedObjectType.GetProperty(Tree.SelectedNode.Parent.Name);
                if (parentPi.PropertyType.IsArray)
                {

                }
                return;
            }

            PropertyInfo pi = SelectedObjectType.GetProperty(Tree.SelectedNode.Name);
            if (pi == null) return; // Property doesn't exist, abort

            if (pi.PropertyType.IsArray)
            {
                if (pi.PropertyType.GetElementType() == typeof(string))
                {
                    List<string> valuesList = ((string[])pi.GetValue(SelectedObject)).Distinct().ToList();
                    string newValue = (string)e.ClickedItem.Tag;
                    if (valuesList.Contains(newValue)) valuesList.Remove(newValue);
                    else valuesList.Add(newValue);
                    pi.SetValue(SelectedObject, valuesList.ToArray());
                }

                // TODO: arrays

                //Type arrayType = pi.PropertyType.GetElementType();

                //List<object> valueList = new List<object>();
                //valueList.AddRange((object[])pi.GetValue(SelectedObject));
                //if (valueList.Contains(e.ClickedItem.Tag))
                //    valueList.Remove(e.ClickedItem.Tag);
                //else
                //    valueList.Add(e.ClickedItem.Tag);
                //pi.SetValue(SelectedObject, valueList.ToArray());

                //Array valueArray = (Array)pi.GetValue(SelectedObject);
                //bool arrayContainsValue = false;
                //for (int i = 0; i < valueArray.Length; i++)
                //    if (valueArray.GetValue(i) == e.ClickedItem.Tag)
                //    {
                //        arrayContainsValue = true;
                //        break;
                //    }

                //if (arrayContainsValue)
                //{
                //}
                //{
                //    object o = a.GetValue(i);
                //}

                RefreshAll();
                return;
            }
            else
            {
                pi.SetValue(SelectedObject, e.ClickedItem.Tag);
                OnValueChanged?.Invoke(Tree, new EventArgs());
                RefreshAll();
                return;
            }
        }

        private void ShowContextMenu(TreeNode node, Point location)
        {
            ContextMenu.Items.Clear();

            PropertyInfo pi = SelectedObjectType.GetProperty(node.Name);
            if (pi == null)
            {
                if (node.Level == 0) return;

                // Property is a flight group
                if (SelectedObjectType.GetProperty(node.Parent.Name).GetCustomAttribute<PlayersFGParentNodeAttribute>() != null)
                    ShowContextMenuForPlayerFlightGroup(node, location);
                else if (SelectedObjectType.GetProperty(node.Parent.Name).PropertyType.IsArray)
                {
                    ContextMenu.Items.Add(Database.Instance.Strings.GetString("GUI", "Remove"));
                    ContextMenu.Show(Tree, location);
                    return;
                }

                return;
            }

            if (pi.PropertyType.IsEnum) // Property type is an enum
                AddEnumToContextMenu(ContextMenu.Items, pi.PropertyType, pi.GetValue(SelectedObject));
            else if (pi.PropertyType.IsArray && pi.PropertyType.GetElementType().IsEnum) // Property type is an array of enums
                AddEnumToContextMenu(ContextMenu.Items, pi.PropertyType.GetElementType(), null); // TODO: selected values
            else if (pi.GetCustomAttribute<DatabaseSourceAttribute>() != null) // Property is a database entry ID
                AddDBEntriesToContextMenu(ContextMenu.Items, pi.GetCustomAttribute<DatabaseSourceAttribute>());
            else if (pi.GetCustomAttribute<IntegerSourceAttribute>() != null) // Property is an integer
                AddIntegersToContextMenu(ContextMenu.Items, pi.GetCustomAttribute<IntegerSourceAttribute>());

            if (ContextMenu.Items.Count == 0) return; // No items, nothing to show
            ContextMenu.Show(Tree, location);
        }

        private void ShowContextMenuForPlayerFlightGroup(TreeNode node, Point location)
        {
            ToolStripMenuItem tsmi;

            ContextMenu.Items.Clear();
            tsmi = (ToolStripMenuItem)ContextMenu.Items.Add("Aircraft");
            AddDBEntriesToContextMenu(tsmi.DropDownItems, new DatabaseSourceAttribute(typeof(DBEntryUnit), false, DatabaseSourceAttributeSpecial.PlayerAircraft));
            tsmi = (ToolStripMenuItem)ContextMenu.Items.Add("Carrier");
            AddDBEntriesToContextMenu(tsmi.DropDownItems, new DatabaseSourceAttribute(typeof(DBEntryUnit), false, DatabaseSourceAttributeSpecial.Carriers));
            tsmi = (ToolStripMenuItem)ContextMenu.Items.Add("Count");
            AddIntegersToContextMenu(tsmi.DropDownItems, new IntegerSourceAttribute(1, Toolbox.MAXIMUM_FLIGHT_GROUP_SIZE));
            tsmi = (ToolStripMenuItem)ContextMenu.Items.Add("Task");
            AddEnumToContextMenu(tsmi.DropDownItems, typeof(MissionTemplateFlightGroupTask), null);
            
            ContextMenu.Show(Tree, location);
        }

        private void AddDBEntriesToContextMenu(ToolStripItemCollection itemCollection, DatabaseSourceAttribute dsa)
        {
            if (dsa.AllowRandom)
                itemCollection.Add(RandomString).Tag = "";

            ToolStripMenuItem tsmi;

            List<DBEntry> validEntries = new List<DBEntry>();
            foreach (DBEntry e in Database.Instance.GetAllEntries(dsa.DBEntryType))
            {
                // Only look for player aircraft
                if (dsa.DBEntryType == typeof(DBEntryUnit))
                {
                    switch (dsa.Special)
                    {
                        case DatabaseSourceAttributeSpecial.Carriers:
                            if (((DBEntryUnit)e).Families.Intersect(Toolbox.SHIP_CARRIER_FAMILIES).Count() == 0)
                                continue;
                            break;
                        case DatabaseSourceAttributeSpecial.PlayerAircraft:
                            if (!((DBEntryUnit)e).AircraftData.PlayerControllable) continue;
                            break;
                    }
                }
                validEntries.Add(e);
            }

            foreach (DBEntry entry in validEntries)
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

            foreach (DBEntry entry in validEntries)
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

        private void AddEnumToContextMenu(ToolStripItemCollection itemCollection, Type enumType, object value)
        {
            foreach (object e in Enum.GetValues(enumType))
            {
                ToolStripMenuItem item = (ToolStripMenuItem)itemCollection.Add(GUITools.GetEnumDisplayName(enumType, e));
                item.BackColor = ContextMenu.BackColor;
                item.ForeColor = ContextMenu.ForeColor;
                item.Checked = (e == value);
                item.Tag = e;
                item.ToolTipText = GUITools.GetEnumToolTip(enumType, e);
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
